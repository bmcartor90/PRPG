using System;
using System.Collections.Generic;
using System.IO;
using Prpg.Sim;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prpg.Game.MapEditing
{
    public enum Tool { Select, Ground, Scaffold, Climb, Start, Ruler, Reach }

    /// <summary>
    /// The map editor, inside the game. Draws one map at a time, saves it to data/maps/ after every change,
    /// and hands it to the game to play at the cursor or from the start point.
    /// Everything is in sixths of a tile.
    /// </summary>
    public sealed class MapEditor
    {
        // ---- What the game provides ----
        readonly Camera cam;
        readonly MapView view;
        readonly MeshBuilder overlay;
        readonly WorldRules world;
        public Func<Movement> CurrentMovement;
        public Action<Map, double, double> PlayAt;      // play this map with him placed at (x, y) in tiles

        // ---- Document ----
        public MapDoc Doc;
        Map map;
        string path;
        bool dirty, backedUp;
        float lastChange;
        readonly List<string> undo = new List<string>(), redo = new List<string>();
        List<string> warnings = new List<string>();

        // ---- Tools ----
        public Tool Tool = Tool.Select;
        ClimbKind climbKind = ClimbKind.Ladder;
        readonly List<P6> trace = new List<P6>();       // a ground trace being clicked out
        P6? scaffoldStart, rulerA, rulerB;
        P6? climbStart; int climbFace; double climbFaceTop, climbFaceBottom; int climbFaceX;
        Reach.Result reach;

        // Selection: what kind, which one, and which point of it (-1 for the whole thing).
        enum Sel { None, Ground, Rock, Scaffold, Climb, Start }
        Sel sel; int selIndex, selVertex = -1;
        bool dragging; P6 dragFrom; string dragBefore;
        bool panning; Vector2 panFromScreen; Vector3 panFromCam;

        // ---- View ----
        public double CamX, CamY;                          // centre of the view, tiles
        public float Zoom = 13.5f;                         // tiles from the bottom of the window to the top
        string status = "";
        float statusUntil;
        string newName = "";
        public bool PanelHasMouse;

        static readonly Color GridTile = new Color(1, 1, 1, 0.07f), GridSixth = new Color(1, 1, 1, 0.03f);
        static readonly Color Border = new Color(1, 1, 1, 0.35f), Hi = new Color32(0xff, 0xc8, 0x3c, 255);
        static readonly Color Draft = new Color32(0x5c, 0xc8, 0xff, 255), RulerCol = new Color32(0xff, 0x6e, 0xc8, 255);

        public MapEditor(Camera cam, MapView view, Material material, WorldRules world, Transform parent)
        {
            this.cam = cam; this.view = view; this.world = world;
            overlay = new MeshBuilder("Editor overlay", parent, material, 50);
            overlay.Z = -1;
            overlay.Object.SetActive(false);
        }

        public static string MapsFolder => Paths.Data("maps");

        /// <summary>Editor maps: everything in data/maps except the lab's test map, which is not in sixths.</summary>
        public static List<string> MapNames()
        {
            var names = new List<string>();
            if (Directory.Exists(MapsFolder))
                foreach (var f in Directory.GetFiles(MapsFolder, "*.json"))
                {
                    var n = Path.GetFileNameWithoutExtension(f);
                    if (n != "test") names.Add(n);
                }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        public void Open(string name)
        {
            SaveNow();
            path = Path.Combine(MapsFolder, name + ".json");
            if (File.Exists(path))
            {
                try { Doc = MapDoc.FromText(File.ReadAllText(path), name); }
                catch (Exception e) { Say("Could not open " + name + ": " + e.Message); return; }
            }
            else
            {
                Directory.CreateDirectory(MapsFolder);
                Doc = MapDoc.NewDefault(name);
                dirty = true;
            }
            Doc.Name = name;
            backedUp = false;
            undo.Clear(); redo.Clear();
            ClearTransient();
            sel = Sel.None;
            Rebuild();
            FitAll();
            PlayerPrefs.SetString("editorMap", name);
            Say("Editing " + name);
        }

        // ---- Changes, undo and saving ----

        void Before() { undo.Add(Doc.ToText()); redo.Clear(); }
        void Changed() { dirty = true; lastChange = Time.unscaledTime; Rebuild(); }

        void Undo()
        {
            if (undo.Count == 0) { Say("Nothing to undo."); return; }
            redo.Add(Doc.ToText());
            Doc = MapDoc.FromText(undo[undo.Count - 1], Doc.Name); undo.RemoveAt(undo.Count - 1);
            sel = Sel.None; Changed();
        }

        void Redo()
        {
            if (redo.Count == 0) { Say("Nothing to redo."); return; }
            undo.Add(Doc.ToText());
            Doc = MapDoc.FromText(redo[redo.Count - 1], Doc.Name); redo.RemoveAt(redo.Count - 1);
            sel = Sel.None; Changed();
        }

        public void SaveNow()
        {
            if (Doc == null || !dirty || path == null) return;
            try
            {
                if (!backedUp && File.Exists(path)) { File.Copy(path, path + ".bak", true); backedUp = true; }
                // Write to a temporary file first, so a crash mid-write never leaves a broken map.
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, Doc.ToText());
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
                dirty = false;
            }
            catch (Exception e) { Say("Could not save: " + e.Message); }
        }

        void Rebuild()
        {
            map = Doc.ToMap(world);
            view.Build(map, CurrentMovement());
            warnings = Doc.Warnings(map, CurrentMovement(), world);
            reach = null;
        }

        public Map CurrentMap => map;

        void ClearTransient()
        {
            trace.Clear(); scaffoldStart = null; rulerA = rulerB = null; climbStart = null; reach = null; dragging = false;
        }

        void Say(string s) { status = s; statusUntil = Time.unscaledTime + 4; }

        // ---- View ----

        public void FitAll()
        {
            CamX = Doc.Width / 12.0; CamY = Doc.Height / 12.0;
            float aspect = cam.aspect;
            Zoom = (float)Math.Max(Doc.Height / 6.0 * 1.1, Doc.Width / 6.0 * 1.1 / aspect);
        }

        public void ApplyCamera()
        {
            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographicSize = Zoom / 2;
            cam.transform.position = new Vector3((float)CamX, (float)CamY, -10);
        }

        Vector2 MouseWorld()
        {
            var m = Mouse.current.position.ReadValue();
            var w = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, 10));
            return new Vector2(w.x, w.y);
        }

        float PixelWorld => Zoom / Math.Max(1, cam.pixelHeight);

        P6 Snapped(Vector2 w)
        {
            var p = P6.Snap(w.x, w.y);
            return new P6(Math.Max(0, Math.Min(Doc.Width, p.X)), Math.Max(0, Math.Min(Doc.Height, p.Y)));
        }

        // Shift held: snap the slope from the previous point to flat, upright, or one of the clean grades 1 in 2, 1 in 1, 2 in 1.
        static P6 GradeSnap(P6 from, P6 to)
        {
            int dx = to.X - from.X, dy = to.Y - from.Y;
            if (dx == 0 && dy == 0) return to;
            double ang = Math.Atan2(dy, dx);
            P6 best = to; double bestErr = double.MaxValue;
            void Try(int x, int y)
            {
                if (x == 0 && y == 0) return;
                double e = Math.Abs(Math.Atan2(y, x) - ang);
                if (e > Math.PI) e = 2 * Math.PI - e;
                if (e < bestErr) { bestErr = e; best = new P6(from.X + x, from.Y + y); }
            }
            Try(dx, 0); Try(0, dy);
            foreach (double k in new[] { 0.5, 1, 2 })
                foreach (int s in new[] { 1, -1 })
                {
                    if (k >= 1) Try(dx, (int)Math.Round(dx * k) * s);
                    else { int ex = (int)Math.Round(dx / 2.0) * 2; Try(ex, ex / 2 * s); }
                }
            return best;
        }

        // ---- Input, once per screen frame while editing ----

        public void Update()
        {
            var kb = Keyboard.current; var mouse = Mouse.current;
            if (kb == null || mouse == null || Doc == null) return;
            bool typing = GUIUtility.keyboardControl != 0;
            bool ctrl = kb.ctrlKey.isPressed, shift = kb.shiftKey.isPressed;

            if (dirty && Time.unscaledTime - lastChange > 0.5f) SaveNow();
            if (!typing) Keys(kb, ctrl, shift);

            // Pan: right or middle drag, or arrow keys. Zoom: wheel, about the cursor.
            var mpos = mouse.position.ReadValue();
            if (!PanelHasMouse && (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame))
            { panning = true; panFromScreen = mpos; panFromCam = new Vector3((float)CamX, (float)CamY, 0); }
            if (panning)
            {
                if (!mouse.rightButton.isPressed && !mouse.middleButton.isPressed) panning = false;
                else { var d = (mpos - panFromScreen) * PixelWorld; CamX = panFromCam.x - d.x; CamY = panFromCam.y - d.y; }
            }
            if (!typing)
            {
                float pan = Zoom * 0.8f * Time.unscaledDeltaTime;
                if (kb.leftArrowKey.isPressed) CamX -= pan;
                if (kb.rightArrowKey.isPressed) CamX += pan;
                if (kb.upArrowKey.isPressed) CamY += pan;
                if (kb.downArrowKey.isPressed) CamY -= pan;
            }
            float wheel = mouse.scroll.ReadValue().y;
            if (!PanelHasMouse && wheel != 0)
            {
                var before = MouseWorld();
                Zoom = Mathf.Clamp(Zoom * (wheel > 0 ? 1 / 1.15f : 1.15f), 3f, 400f);
                ApplyCamera();
                var after = MouseWorld();
                CamX += before.x - after.x; CamY += before.y - after.y;
            }
            ApplyCamera();

            if (PanelHasMouse && !dragging) return;
            var w = MouseWorld();
            var p = Snapped(w);
            if (mouse.leftButton.wasPressedThisFrame) Press(w, p, shift);
            if (dragging && mouse.leftButton.isPressed) Drag(p);
            if (mouse.leftButton.wasReleasedThisFrame) Release(p, shift);
        }

        void Keys(Keyboard kb, bool ctrl, bool shift)
        {
            bool Hit(Key k) => kb[k].wasPressedThisFrame;
            if (ctrl)
            {
                if (Hit(Key.Z)) { if (shift) Redo(); else Undo(); }
                if (Hit(Key.Y)) Redo();
                if (Hit(Key.S)) { dirty = true; SaveNow(); Say("Saved " + path); }
                if (Hit(Key.D)) Duplicate();
                return;
            }
            if (Hit(Key.V)) SetTool(Tool.Select);
            if (Hit(Key.G)) SetTool(Tool.Ground);
            if (Hit(Key.S)) SetTool(Tool.Scaffold);
            if (Hit(Key.C)) SetTool(Tool.Climb);
            if (Hit(Key.P)) SetTool(Tool.Start);
            if (Hit(Key.R)) SetTool(Tool.Ruler);
            if (Hit(Key.O)) SetTool(Tool.Reach);
            if (Tool == Tool.Climb)
            {
                if (Hit(Key.Digit1)) climbKind = ClimbKind.Ladder;
                if (Hit(Key.Digit2)) climbKind = ClimbKind.Rope;
                if (Hit(Key.Digit3)) climbKind = ClimbKind.RoughRock;
                if (Hit(Key.Digit4)) climbKind = ClimbKind.SmoothRock;
            }
            if (Hit(Key.Home)) FitAll();
            if (Hit(Key.Enter) || Hit(Key.NumpadEnter)) FinishTrace(false);
            if (Hit(Key.Backspace) && trace.Count > 0) trace.RemoveAt(trace.Count - 1);
            if (Hit(Key.Escape)) { ClearTransient(); sel = Sel.None; }
            if (Hit(Key.Delete)) DeleteSelected();
            if (Hit(Key.F2)) { var w = MouseWorld(); SaveNow(); PlayAt(map, w.x, w.y); }
            if (Hit(Key.F3))
            {
                if (Doc.Start.HasValue) { SaveNow(); PlayAt(map, Doc.Start.Value.Tx, Doc.Start.Value.Ty); }
                else Say("Place a start point first (P).");
            }
        }

        void SetTool(Tool t)
        {
            if (Tool == Tool.Ground && trace.Count > 0 && t != Tool.Ground) Say("Trace dropped.");
            ClearTransient();
            Tool = t;
        }

        // ---- Mouse ----

        void Press(Vector2 w, P6 p, bool shift)
        {
            switch (Tool)
            {
                case Tool.Select:
                    if (HitTest(w, out var s, out int i, out int v))
                    {
                        sel = s; selIndex = i; selVertex = v;
                        dragging = true; dragFrom = p; dragBefore = Doc.ToText();
                    }
                    else { sel = Sel.None; panning = true; panFromScreen = Mouse.current.position.ReadValue(); panFromCam = new Vector3((float)CamX, (float)CamY, 0); }
                    break;

                case Tool.Ground:
                    if (shift && trace.Count > 0) p = GradeSnap(trace[trace.Count - 1], p);
                    if (trace.Count >= 3 && Near(w, trace[0])) { FinishTrace(true); break; }
                    if (trace.Count > 0 && trace[trace.Count - 1].Equals(p)) { FinishTrace(false); break; }   // clicking the last point again ends the trace
                    trace.Add(p);
                    break;

                case Tool.Scaffold:
                    if (!scaffoldStart.HasValue) { scaffoldStart = p; break; }
                    if (shift) p = GradeSnap(scaffoldStart.Value, p);
                    if (p.X == scaffoldStart.Value.X) { Say("A scaffold cannot be upright."); break; }
                    Before();
                    var a = scaffoldStart.Value; var b = p;
                    if (b.X < a.X) { var t = a; a = b; b = t; }
                    Doc.Scaffolds.Add(new[] { a, b });
                    scaffoldStart = null;
                    Changed();
                    break;

                case Tool.Climb:
                    climbStart = p; climbFace = 0;
                    // Pressed on a cliff face: the climbable runs down that face.
                    foreach (var c in map.Cliffs)
                    {
                        double bottom = double.IsNegativeInfinity(c.Bottom) ? 0 : c.Bottom;
                        if (Math.Abs(w.x - c.X) < 8 * PixelWorld + 0.1 && w.y <= c.Top + 0.1 && w.y >= bottom - 0.1)
                        {
                            climbFace = c.Face; climbFaceX = (int)Math.Round(c.X * 6); climbFaceTop = c.Top; climbFaceBottom = bottom;
                            break;
                        }
                    }
                    break;

                case Tool.Start:
                    Before(); Doc.Start = p; Changed();
                    break;

                case Tool.Ruler:
                    if (!rulerA.HasValue || rulerB.HasValue) { rulerA = p; rulerB = null; }
                    else rulerB = shift ? GradeSnap(rulerA.Value, p) : p;
                    break;

                case Tool.Reach:
                    reach = Reach.From(map, CurrentMovement(), world, p.Tx, p.Ty + 0.05);
                    if (reach == null) Say("Click on or just above something he can stand on.");
                    break;
            }
        }

        void Drag(P6 p)
        {
            if (!dragging || sel == Sel.None) return;
            int dx = p.X - dragFrom.X, dy = p.Y - dragFrom.Y;
            var orig = MapDoc.FromText(dragBefore, Doc.Name);
            Doc = orig;                         // start from how it was when the drag began, then move by the whole offset
            switch (sel)
            {
                case Sel.Ground: MovePoints(Doc.Ground[selIndex], dx, dy); break;
                case Sel.Rock: MovePoints(Doc.Rock[selIndex], dx, dy); break;
                case Sel.Scaffold:
                {
                    var s = Doc.Scaffolds[selIndex];
                    if (selVertex < 0 || selVertex == 0) s[0] = new P6(s[0].X + dx, s[0].Y + dy);
                    if (selVertex < 0 || selVertex == 1) s[1] = new P6(s[1].X + dx, s[1].Y + dy);
                    break;
                }
                case Sel.Climb:
                {
                    var c = Doc.Climbs[selIndex];
                    if (selVertex < 0) { if (c.Face == 0) c.X += dx; c.Y1 += dy; c.Y2 += dy; }
                    else if (selVertex == 0) c.Y1 += dy; else c.Y2 += dy;
                    break;
                }
                case Sel.Start: Doc.Start = new P6(Doc.Start.Value.X + dx, Doc.Start.Value.Y + dy); break;
            }
            map = Doc.ToMap(world);
            view.Build(map, CurrentMovement());
        }

        void MovePoints(List<P6> t, int dx, int dy)
        {
            if (selVertex >= 0) t[selVertex] = new P6(t[selVertex].X + dx, t[selVertex].Y + dy);
            else for (int i = 0; i < t.Count; i++) t[i] = new P6(t[i].X + dx, t[i].Y + dy);
        }

        void Release(P6 p, bool shift)
        {
            if (Tool == Tool.Climb && climbStart.HasValue)
            {
                var a = climbStart.Value; climbStart = null;
                int y1 = Math.Min(a.Y, p.Y), y2 = Math.Max(a.Y, p.Y);
                var c = new ClimbDoc { Kind = climbKind, X = a.X, Y1 = y1, Y2 = y2, Face = climbFace };
                if (climbFace != 0)
                {
                    c.X = climbFaceX;
                    c.Y1 = Math.Max(c.Y1, (int)Math.Round(climbFaceBottom * 6));
                    c.Y2 = Math.Min(c.Y2, (int)Math.Round(climbFaceTop * 6));
                }
                if (c.Y2 - c.Y1 < 3) { Say("Drag from the bottom to the top of the climbable."); return; }
                Before(); Doc.Climbs.Add(c); Changed();
                return;
            }
            if (!dragging) return;
            dragging = false;
            if (p.Equals(dragFrom)) { Doc = MapDoc.FromText(dragBefore, Doc.Name); Rebuild(); return; }
            // A move that makes ground double back, or flattens a rock, is undone.
            bool ok = true;
            if (sel == Sel.Ground) ok = MapDoc.NormalizeGround(Doc.Ground[selIndex]);
            if (sel == Sel.Rock) ok = MapDoc.NormalizeRock(Doc.Rock[selIndex]);
            if (sel == Sel.Scaffold)
            {
                var s = Doc.Scaffolds[selIndex];
                if (s[0].X == s[1].X) ok = false;
                else if (s[1].X < s[0].X) { var t = s[0]; s[0] = s[1]; s[1] = t; }
            }
            if (sel == Sel.Climb) { var c = Doc.Climbs[selIndex]; if (c.Y2 - c.Y1 < 3) ok = false; }
            if (!ok) { Doc = MapDoc.FromText(dragBefore, Doc.Name); Rebuild(); Say("That move would break the shape, so it was undone."); return; }
            undo.Add(dragBefore); redo.Clear();
            Changed();
        }

        void FinishTrace(bool closed)
        {
            if (trace.Count == 0) return;
            var t = new List<P6>(trace);
            if (closed)
            {
                if (!MapDoc.NormalizeRock(t)) { Say("A floating rock needs at least three corners that are not in a line."); return; }
                Before(); Doc.Rock.Add(t);
            }
            else
            {
                if (!MapDoc.NormalizeGround(t)) { Say("Ground runs one way, left to right, and needs at least one sideways step. Upright steps become cliffs."); return; }
                Before(); Doc.Ground.Add(t);
            }
            trace.Clear();
            Changed();
        }

        void DeleteSelected()
        {
            if (sel == Sel.None) return;
            Before();
            switch (sel)
            {
                case Sel.Ground:
                    if (selVertex >= 0 && Doc.Ground[selIndex].Count > 2) Doc.Ground[selIndex].RemoveAt(selVertex);
                    else Doc.Ground.RemoveAt(selIndex);
                    break;
                case Sel.Rock:
                    if (selVertex >= 0 && Doc.Rock[selIndex].Count > 3) Doc.Rock[selIndex].RemoveAt(selVertex);
                    else Doc.Rock.RemoveAt(selIndex);
                    break;
                case Sel.Scaffold: Doc.Scaffolds.RemoveAt(selIndex); break;
                case Sel.Climb: Doc.Climbs.RemoveAt(selIndex); break;
                case Sel.Start: Doc.Start = null; break;
            }
            sel = Sel.None;
            Changed();
        }

        void Duplicate()
        {
            if (sel == Sel.None || sel == Sel.Start) return;
            Before();
            const int off = 6;           // one tile to the right
            switch (sel)
            {
                case Sel.Ground: { var t = new List<P6>(); foreach (var q in Doc.Ground[selIndex]) t.Add(new P6(q.X + off, q.Y)); Doc.Ground.Add(t); selIndex = Doc.Ground.Count - 1; break; }
                case Sel.Rock: { var t = new List<P6>(); foreach (var q in Doc.Rock[selIndex]) t.Add(new P6(q.X + off, q.Y)); Doc.Rock.Add(t); selIndex = Doc.Rock.Count - 1; break; }
                case Sel.Scaffold: { var s = Doc.Scaffolds[selIndex]; Doc.Scaffolds.Add(new[] { new P6(s[0].X + off, s[0].Y), new P6(s[1].X + off, s[1].Y) }); selIndex = Doc.Scaffolds.Count - 1; break; }
                case Sel.Climb: { var c = Doc.Climbs[selIndex].Copy(); if (c.Face == 0) c.X += off; Doc.Climbs.Add(c); selIndex = Doc.Climbs.Count - 1; break; }
            }
            selVertex = -1;
            Changed();
        }

        // ---- Picking things under the cursor ----

        bool Near(Vector2 w, P6 p) => Math.Abs(w.x - p.Tx) < 8 * PixelWorld && Math.Abs(w.y - p.Ty) < 8 * PixelWorld;

        static double SegDist(Vector2 w, double ax, double ay, double bx, double by)
        {
            double dx = bx - ax, dy = by - ay, l2 = dx * dx + dy * dy;
            double t = l2 == 0 ? 0 : Math.Max(0, Math.Min(1, ((w.x - ax) * dx + (w.y - ay) * dy) / l2));
            double px = ax + t * dx - w.x, py = ay + t * dy - w.y;
            return Math.Sqrt(px * px + py * py);
        }

        bool HitTest(Vector2 w, out Sel s, out int index, out int vertex)
        {
            double tol = 8 * PixelWorld;
            s = Sel.None; index = -1; vertex = -1;
            if (Doc.Start.HasValue && Near(w, Doc.Start.Value)) { s = Sel.Start; index = 0; return true; }
            // Points first, so a corner can be grabbed where lines meet.
            for (int i = 0; i < Doc.Climbs.Count; i++)
            {
                var c = Doc.Climbs[i];
                if (Near(w, new P6(c.X, c.Y1))) { s = Sel.Climb; index = i; vertex = 0; return true; }
                if (Near(w, new P6(c.X, c.Y2))) { s = Sel.Climb; index = i; vertex = 1; return true; }
            }
            for (int i = 0; i < Doc.Scaffolds.Count; i++)
                for (int k = 0; k < 2; k++)
                    if (Near(w, Doc.Scaffolds[i][k])) { s = Sel.Scaffold; index = i; vertex = k; return true; }
            for (int i = 0; i < Doc.Ground.Count; i++)
                for (int k = 0; k < Doc.Ground[i].Count; k++)
                    if (Near(w, Doc.Ground[i][k])) { s = Sel.Ground; index = i; vertex = k; return true; }
            for (int i = 0; i < Doc.Rock.Count; i++)
                for (int k = 0; k < Doc.Rock[i].Count; k++)
                    if (Near(w, Doc.Rock[i][k])) { s = Sel.Rock; index = i; vertex = k; return true; }
            // Then whole things.
            for (int i = 0; i < Doc.Climbs.Count; i++)
            {
                var c = Doc.Climbs[i];
                if (SegDist(w, c.X / 6.0, c.Y1 / 6.0, c.X / 6.0, c.Y2 / 6.0) < tol + 0.3) { s = Sel.Climb; index = i; return true; }
            }
            for (int i = 0; i < Doc.Scaffolds.Count; i++)
            {
                var a = Doc.Scaffolds[i];
                if (SegDist(w, a[0].Tx, a[0].Ty, a[1].Tx, a[1].Ty) < tol) { s = Sel.Scaffold; index = i; return true; }
            }
            for (int i = 0; i < Doc.Ground.Count; i++)
            {
                var t = Doc.Ground[i];
                for (int k = 1; k < t.Count; k++)
                    if (SegDist(w, t[k - 1].Tx, t[k - 1].Ty, t[k].Tx, t[k].Ty) < tol) { s = Sel.Ground; index = i; return true; }
            }
            for (int i = 0; i < Doc.Rock.Count; i++)
            {
                var t = Doc.Rock[i];
                for (int k = 0; k < t.Count; k++)
                {
                    var a = t[k]; var b = t[(k + 1) % t.Count];
                    if (SegDist(w, a.Tx, a.Ty, b.Tx, b.Ty) < tol) { s = Sel.Rock; index = i; return true; }
                }
            }
            return false;
        }

        // ---- Drawing the editor's own marks, every frame ----

        public void SetVisible(bool on)
        {
            overlay.Object.SetActive(on);
        }

        public void DrawOverlay()
        {
            overlay.Clear();
            float px = PixelWorld, lw = 2 * px;
            float W = Doc.Width / 6f, H = Doc.Height / 6f;

            // Grid: tiles when there are few enough on screen, sixths when zoomed right in.
            float halfW = Zoom * cam.aspect / 2, halfH = Zoom / 2;
            float x0 = Mathf.Max(0, (float)CamX - halfW), x1 = Mathf.Min(W, (float)CamX + halfW);
            float y0 = Mathf.Max(0, (float)CamY - halfH), y1 = Mathf.Min(H, (float)CamY + halfH);
            if (Zoom < 60)
            {
                for (float x = Mathf.Ceil(x0); x <= x1; x++) overlay.Seg(x, y0, x, y1, px, GridTile);
                for (float y = Mathf.Ceil(y0); y <= y1; y++) overlay.Seg(x0, y, x1, y, px, GridTile);
            }
            if (Zoom < 12)
            {
                for (float x = Mathf.Ceil(x0 * 6) / 6; x <= x1; x += 1 / 6f) overlay.Seg(x, y0, x, y1, px, GridSixth);
                for (float y = Mathf.Ceil(y0 * 6) / 6; y <= y1; y += 1 / 6f) overlay.Seg(x0, y, x1, y, px, GridSixth);
            }
            overlay.Box(0, 0, W, H, lw, Border);

            // Corners of what is drawn, so they can be grabbed.
            foreach (var t in Doc.Ground) foreach (var q in t) overlay.Dot(q.Tx2(), q.Ty2(), 5 * px, Color.white);
            foreach (var t in Doc.Rock) foreach (var q in t) overlay.Dot(q.Tx2(), q.Ty2(), 5 * px, Color.white);
            foreach (var s in Doc.Scaffolds) { overlay.Dot(s[0].Tx2(), s[0].Ty2(), 5 * px, Color.white); overlay.Dot(s[1].Tx2(), s[1].Ty2(), 5 * px, Color.white); }
            foreach (var c in Doc.Climbs) { overlay.Dot(c.X / 6f, c.Y1 / 6f, 5 * px, Color.white); overlay.Dot(c.X / 6f, c.Y2 / 6f, 5 * px, Color.white); }

            // Start point: a little flag.
            if (Doc.Start.HasValue)
            {
                var s = Doc.Start.Value;
                overlay.Seg(s.Tx2(), s.Ty2(), s.Tx2(), s.Ty2() + 1.2f, lw, Color.green);
                overlay.Quad(new Vector2(s.Tx2(), s.Ty2() + 1.2f), new Vector2(s.Tx2() + 0.6f, s.Ty2() + 1f), new Vector2(s.Tx2(), s.Ty2() + 0.8f), new Vector2(s.Tx2(), s.Ty2() + 1.2f), Color.green);
            }

            DrawSelection(px, lw);

            // Work in progress.
            var w = MouseWorld();
            var p = Snapped(w);
            bool shift = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
            if (Tool == Tool.Ground && trace.Count > 0)
            {
                for (int i = 1; i < trace.Count; i++) overlay.Seg(trace[i - 1].Tx2(), trace[i - 1].Ty2(), trace[i].Tx2(), trace[i].Ty2(), lw * 1.5f, Draft);
                var last = trace[trace.Count - 1];
                var next = shift ? GradeSnap(last, p) : p;
                overlay.Seg(last.Tx2(), last.Ty2(), next.Tx2(), next.Ty2(), lw, Draft);
                foreach (var q in trace) overlay.Dot(q.Tx2(), q.Ty2(), 6 * px, Draft);
                if (trace.Count >= 3) overlay.Box(trace[0].Tx2() - 6 * px, trace[0].Ty2() - 6 * px, trace[0].Tx2() + 6 * px, trace[0].Ty2() + 6 * px, lw, Draft);
            }
            if (Tool == Tool.Scaffold && scaffoldStart.HasValue)
            {
                var a = scaffoldStart.Value; var b = shift ? GradeSnap(a, p) : p;
                overlay.Seg(a.Tx2(), a.Ty2(), b.Tx2(), b.Ty2(), lw * 1.5f, Draft);
            }
            if (Tool == Tool.Climb && climbStart.HasValue)
            {
                float cx = climbFace != 0 ? climbFaceX / 6f : climbStart.Value.Tx2();
                overlay.Seg(cx, climbStart.Value.Ty2(), cx, p.Ty2(), lw * 2, Draft);
            }
            if (rulerA.HasValue)
            {
                var a = rulerA.Value; var b = rulerB ?? (shift ? GradeSnap(a, p) : p);
                overlay.Seg(a.Tx2(), a.Ty2(), b.Tx2(), b.Ty2(), lw, RulerCol);
                overlay.Seg(a.Tx2(), a.Ty2(), b.Tx2(), a.Ty2(), px, RulerCol);
                overlay.Seg(b.Tx2(), a.Ty2(), b.Tx2(), b.Ty2(), px, RulerCol);
            }
            if (reach != null) DrawReach(px, lw);

            // Cursor: the snapped point.
            if (!PanelHasMouse) overlay.Box(p.Tx2() - 4 * px, p.Ty2() - 4 * px, p.Tx2() + 4 * px, p.Ty2() + 4 * px, px * 1.5f, Hi);
            overlay.Apply();
        }

        void DrawSelection(float px, float lw)
        {
            if (sel == Sel.None) return;
            float t = lw * 2;
            switch (sel)
            {
                case Sel.Ground:
                case Sel.Rock:
                {
                    var tr = sel == Sel.Ground ? Doc.Ground[selIndex] : Doc.Rock[selIndex];
                    int n = sel == Sel.Rock ? tr.Count : tr.Count - 1;
                    for (int k = 0; k < n; k++) { var a = tr[k]; var b = tr[(k + 1) % tr.Count]; overlay.Seg(a.Tx2(), a.Ty2(), b.Tx2(), b.Ty2(), t, Hi); }
                    if (selVertex >= 0) overlay.Dot(tr[selVertex].Tx2(), tr[selVertex].Ty2(), 10 * px, Hi);
                    break;
                }
                case Sel.Scaffold:
                {
                    var s = Doc.Scaffolds[selIndex];
                    overlay.Seg(s[0].Tx2(), s[0].Ty2(), s[1].Tx2(), s[1].Ty2(), t, Hi);
                    if (selVertex >= 0) overlay.Dot(s[selVertex].Tx2(), s[selVertex].Ty2(), 10 * px, Hi);
                    break;
                }
                case Sel.Climb:
                {
                    var c = Doc.Climbs[selIndex];
                    overlay.Seg(c.X / 6f, c.Y1 / 6f, c.X / 6f, c.Y2 / 6f, t * 1.5f, Hi);
                    if (selVertex >= 0) overlay.Dot(c.X / 6f, (selVertex == 0 ? c.Y1 : c.Y2) / 6f, 10 * px, Hi);
                    break;
                }
                case Sel.Start:
                    overlay.Dot(Doc.Start.Value.Tx2(), Doc.Start.Value.Ty2(), 10 * px, Hi);
                    break;
            }
        }

        void DrawReach(float px, float lw)
        {
            var colors = new[] { new Color(0.4f, 1f, 0.4f, 0.9f), new Color(1f, 0.6f, 0.2f, 0.9f), new Color(0.4f, 1f, 0.4f, 0.9f), new Color(1f, 0.6f, 0.2f, 0.9f), new Color(0.6f, 0.8f, 1f, 0.9f) };
            for (int i = 0; i < reach.Paths.Count; i++)
            {
                var path = reach.Paths[i];
                var col = colors[i % colors.Length];
                for (int k = 1; k < path.Xs.Count; k++)
                    overlay.Seg((float)path.Xs[k - 1], (float)path.Ys[k - 1], (float)path.Xs[k], (float)path.Ys[k], lw, col);
                int e = path.Xs.Count - 1;
                if (path.Caught) overlay.Dot((float)path.Xs[e], (float)path.Ys[e], 8 * px, Color.white);
            }
            // Highest ledge he can catch from a standing jump here, and how far ahead a lunge can reach.
            float cy = (float)reach.CatchHeight, x = (float)reach.X, R = (float)reach.LungeReach;
            overlay.Seg(x - 1.5f, cy, x + 1.5f, cy, lw, new Color(0.6f, 0.8f, 1f, 0.9f));
            overlay.Dot(x, (float)reach.Y, 8 * px, Color.white);
        }

        // ---- The side panel ----

        public void OnGUI(Movement P, string character)
        {
            float u = Mathf.Max(14, Screen.height / 60f);
            var panel = new Rect(8, 8, u * 17, Screen.height - 16);
            var m = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            PanelHasMouse = panel.Contains(new Vector2(m.x, Screen.height - m.y));
            GUI.Box(panel, GUIContent.none);
            var st = new GUIStyle(GUI.skin.label) { fontSize = (int)(u * 0.75f), wordWrap = true };
            var bt = new GUIStyle(GUI.skin.button) { fontSize = (int)(u * 0.75f) };
            GUILayout.BeginArea(new Rect(panel.x + 8, panel.y + 8, panel.width - 16, panel.height - 16));
            GUILayout.Label("MAP EDITOR: " + Doc.Name + (dirty ? "  (saving...)" : "  (saved)"), st);

            GUILayout.Label("Tools", st);
            ToolButton(Tool.Select, "V  Select / move", bt);
            ToolButton(Tool.Ground, "G  Ground", bt);
            ToolButton(Tool.Scaffold, "S  Scaffold", bt);
            ToolButton(Tool.Climb, "C  Climbable: " + KindName(climbKind), bt);
            ToolButton(Tool.Start, "P  Start point", bt);
            ToolButton(Tool.Ruler, "R  Ruler", bt);
            ToolButton(Tool.Reach, "O  Reach", bt);
            GUILayout.Label(Help(), st);

            if (Tool == Tool.Ruler && rulerA.HasValue && rulerB.HasValue)
            {
                int dx = rulerB.Value.X - rulerA.Value.X, dy = rulerB.Value.Y - rulerA.Value.Y;
                GUILayout.Label("Across " + P6.Tiles(Math.Abs(dx)) + ", up " + P6.Tiles(dy) + "  (" + Math.Sqrt(dx * dx + dy * dy) / 6.0 + " tiles)", st);
            }
            if (Tool == Tool.Reach && reach != null)
                GUILayout.Label(character + ": green standing jumps, orange running jumps, blue straight up and the highest ledge he can catch from here. White dots: ledges caught.", st);

            var w = MouseWorld(); var p = Snapped(w);
            GUILayout.Label("Cursor  " + p + "  (tiles and sixths)", st);

            GUILayout.Space(u * 0.5f);
            GUILayout.Label("Size in tiles: " + P6.Tiles(Doc.Width) + " wide, " + P6.Tiles(Doc.Height) + " tall", st);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Wider", bt)) Resize(Doc.Width + 12 * 6, Doc.Height);
            if (GUILayout.Button("Narrower", bt)) Resize(Doc.Width - 12 * 6, Doc.Height);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Taller", bt)) Resize(Doc.Width, Doc.Height + 6 * 6);
            if (GUILayout.Button("Shorter", bt)) Resize(Doc.Width, Doc.Height - 6 * 6);
            GUILayout.EndHorizontal();

            GUILayout.Space(u * 0.5f);
            GUILayout.Label("Maps", st);
            foreach (var n in MapNames())
                if (GUILayout.Button(n == Doc.Name ? "> " + n : n, bt)) Open(n);
            GUILayout.BeginHorizontal();
            newName = GUILayout.TextField(newName, GUILayout.Width(u * 9));
            if (GUILayout.Button("New map", bt) && newName.Trim().Length > 0)
            {
                string n = string.Join("_", newName.Trim().Split(Path.GetInvalidFileNameChars()));
                newName = ""; GUIUtility.keyboardControl = 0;
                Open(n);
            }
            GUILayout.EndHorizontal();

            if (warnings.Count > 0)
            {
                GUILayout.Space(u * 0.5f);
                var ws = new GUIStyle(st); ws.normal.textColor = new Color32(0xff, 0xc8, 0x3c, 255);
                GUILayout.Label("Warnings", ws);
                foreach (var x in warnings) GUILayout.Label("• " + x, ws);
            }
            if (Time.unscaledTime < statusUntil) GUILayout.Label(status, st);
            GUILayout.EndArea();

            // Click in empty space to stop typing in the name box.
            if (Event.current.type == EventType.MouseDown && !panel.Contains(Event.current.mousePosition)) GUIUtility.keyboardControl = 0;
        }

        void ToolButton(Tool t, string label, GUIStyle bt)
        {
            if (GUILayout.Toggle(Tool == t, label, bt) && Tool != t) SetTool(t);
        }

        static string KindName(ClimbKind k) => k == ClimbKind.RoughRock ? "rough rock" : k == ClimbKind.SmoothRock ? "smooth rock" : k.ToString().ToLowerInvariant();

        string Help()
        {
            switch (Tool)
            {
                case Tool.Select: return "Click a corner or a line to pick it, drag to move it. Delete removes it (a corner, if one is picked). Ctrl+D copies it one tile right.";
                case Tool.Ground: return "Click points along the surface. Enter, or clicking the last point again, ends it: open ground, filled to the bottom. Click the first point again to close it into floating rock. Backspace takes back a point. Shift snaps to flat, upright, 1:2, 1:1 or 2:1.";
                case Tool.Scaffold: return "Click both ends. Shift snaps the slope.";
                case Tool.Climb: return "1 ladder, 2 rope, 3 rough rock, 4 smooth rock. Drag from bottom to top. Start the drag on a cliff face to put it on the face.";
                case Tool.Start: return "Click where he starts.";
                case Tool.Ruler: return "Click two points. Shift snaps the slope.";
                default: return "Click a spot he can stand on to see where he can get to from there.";
            }
        }

        void Resize(int w, int h)
        {
            if (w < 12 || h < 12) return;
            Before(); Doc.Width = w; Doc.Height = h; Changed();
        }

        public static string KeysLine => "F2 play from the cursor  F3 play from the start  Home show all  wheel zoom  right-drag pan  Ctrl+Z undo  Ctrl+Y redo  Tab character  F4 leave editor";
    }

    static class P6Draw
    {
        public static float Tx2(this P6 p) => p.X / 6f;
        public static float Ty2(this P6 p) => p.Y / 6f;
    }
}
