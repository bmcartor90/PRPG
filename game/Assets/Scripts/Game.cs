using System;
using System.Collections.Generic;
using Prpg.Game.MapEditing;
using Prpg.Sim;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prpg.Game
{
    /// <summary>
    /// The game: one character on a map. Owns the simulation, runs it at exactly 60 ticks a second, and draws between
    /// the last two ticks. Also hosts the map editor (F4). Created at start-up from code, so the scene can stay empty.
    /// </summary>
    public sealed class Game : MonoBehaviour
    {
        // Lab view: 24 by 13.5 tiles on every display, with the floor this far up from the bottom edge.
        const float ViewW = 24f, ViewH = 13.5f;
        static readonly float FloorPad = Math.Min(1.5f, ViewH * 0.18f);
        // Section starts on the test map, as the lab's number keys.
        static readonly double[] Sections = { 3, 36, 115, 163, 229, 294, 346, 381 };

        enum Mode { Play, Edit, EditPlay }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Game>() != null) return;
            new GameObject("PRPG").AddComponent<Game>();
        }

        Mode mode = Mode.Play;
        Simulation sim;
        Map testMap;
        WorldRules world;
        string movementText;
        Controls controls;
        readonly FixedStepper stepper = new FixedStepper();
        readonly AnimState anim = new AnimState();
        Camera cam, backdrop;
        MapView mapView;
        CharacterView view;
        MapEditor editor;
        ItemAtlas items;
        JsonObject characterArt;
        readonly List<string> characterOrder = new List<string>();
        readonly Dictionary<string, CharacterAtlas> atlases = new Dictionary<string, CharacterAtlas>();
        string character;
        int section;
        double placeX, placeY;          // where "retry" puts him when playing an editor map
        double camX, camY, camPx, camPy;
        string error;
        bool showHelp = true;

        Map CurrentMap => sim.Map;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
            try { Load(); }
            catch (Exception e) { error = e.ToString(); Debug.LogException(e); }
        }

        void Load()
        {
            world = WorldRules.Load(Paths.ReadData("world.json"));
            testMap = Map.Load(Paths.ReadData("maps", "test.json"), world);
            movementText = Paths.ReadData("movement.json");

            var chars = (JsonObject)Json.Parse(Paths.ReadData("characters.json"));
            characterArt = chars.Object("characters");
            foreach (var o in chars.Array("order")) characterOrder.Add((string)o);
            items = ItemAtlas.Load(Paths.Art(chars.String("items").Split('/')));

            character = PlayerPrefs.GetString("character", characterOrder[0]);
            if (!characterOrder.Contains(character)) character = characterOrder[0];
            var movement = MovementFile.Load(movementText, character);

            double x = Sections[0];
            sim = Simulation.Start(testMap, movement, world, x, Simulation.GroundLineAt(testMap, x), NewSeed(), NewSeed());
            controls = new Controls();

            // Cameras: one clears the whole window to the lab's surround colour, the other draws the view.
            backdrop = new GameObject("Backdrop").AddComponent<Camera>();
            backdrop.clearFlags = CameraClearFlags.SolidColor;
            backdrop.backgroundColor = new Color32(0x1b, 0x1b, 0x1b, 255);
            backdrop.cullingMask = 0;
            backdrop.depth = -10;
            cam = new GameObject("Camera").AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = ViewH / 2;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.depth = 0;
            cam.nearClipPlane = -100; cam.farClipPlane = 100;

            var material = DefaultSpriteMaterial();
            mapView = new MapView(transform, material);
            view = new CharacterView(transform);
            editor = new MapEditor(cam, mapView, material, world, transform)
            {
                CurrentMovement = () => sim.Movement,
                PlayAt = PlayFromEditor,
            };
            ApplyCharacter();
            Warp(0);
        }

        static Material DefaultSpriteMaterial()
        {
            // The built-in sprite material: always in a build, draws vertex colours.
            var probe = new GameObject("probe").AddComponent<SpriteRenderer>();
            var m = probe.sharedMaterial;
            Destroy(probe.gameObject);
            return m;
        }

        static ulong NewSeed()
        {
            // Seeds come from outside the simulation; the clock is fine here. Inside the simulation nothing reads time.
            var g = Guid.NewGuid().ToByteArray();
            return BitConverter.ToUInt64(g, 0);
        }

        void ApplyCharacter()
        {
            var art = characterArt.Object(character);
            if (!atlases.TryGetValue(character, out var atlas))
            {
                atlas = CharacterAtlas.Load(Paths.Art(art.String("atlas").Split('/')));
                atlases[character] = atlas;
            }
            sim.Movement = MovementFile.Load(movementText, character);
            view.SetCharacter(atlas, items, art.String("sword"), art.String("shield"));
            mapView.Build(CurrentMap, sim.Movement);
            PlayerPrefs.SetString("character", character);
        }

        // ---- Modes ----

        /// <summary>A fresh simulation on another map, keeping the seeds and the character's numbers.</summary>
        void UseMap(Map m)
        {
            var old = sim.State;
            sim = new Simulation(m, sim.Movement, world, new SimState
            {
                RunSeed = old.RunSeed, BattleSeed = old.BattleSeed,
                RunRng = Rng.FromSeed(old.RunSeed), BattleRng = Rng.FromSeed(old.BattleSeed),
                Player = new PlayerState(),
            });
            mapView.Build(m, sim.Movement);
        }

        void Warp(int i)
        {
            section = i;
            sim.Warp(Sections[i]);
            AfterPlace();
        }

        void AfterPlace()
        {
            anim.Reset();
            controls.ClearQueues();
            var p = sim.State.Player;
            CameraTarget(p, out camX, out camY);
            camPx = camX; camPy = camY;
        }

        void OpenEditor()
        {
            mode = Mode.Edit;
            view.SetVisible(false);
            editor.SetVisible(true);
            if (editor.Doc == null)
            {
                var names = MapEditor.MapNames();
                string last = PlayerPrefs.GetString("editorMap", names.Count > 0 ? names[0] : "first");
                editor.Open(last);
            }
            else mapView.Build(editor.CurrentMap, sim.Movement);
        }

        void LeaveEditor()
        {
            editor.SaveNow();
            editor.SetVisible(false);
            view.SetVisible(true);
            mode = Mode.Play;
            UseMap(testMap);
            Warp(section);
        }

        void PlayFromEditor(Map m, double x, double y)
        {
            mode = Mode.EditPlay;
            editor.SetVisible(false);
            view.SetVisible(true);
            UseMap(m);
            placeX = x; placeY = y;
            sim.PlaceAt(x, y);
            AfterPlace();
        }

        void BackToEditor()
        {
            mode = Mode.Edit;
            view.SetVisible(false);
            editor.SetVisible(true);
            mapView.Build(editor.CurrentMap, sim.Movement);
        }

        // ---- Frame loop ----

        void Update()
        {
            if (error != null || sim == null) return;
            try
            {
                var kb = Keyboard.current;
                bool typing = GUIUtility.keyboardControl != 0;
                if (kb != null && !typing)
                {
                    if (kb[Key.Tab].wasPressedThisFrame)
                    {
                        character = characterOrder[(characterOrder.IndexOf(character) + 1) % characterOrder.Count];
                        ApplyCharacter();
                        if (mode == Mode.Edit) mapView.Build(editor.CurrentMap, sim.Movement);
                    }
                    if (kb[Key.F4].wasPressedThisFrame) { if (mode == Mode.Play) OpenEditor(); else LeaveEditor(); return; }
                }

                if (mode == Mode.Edit) { editor.Update(); return; }

                if (kb != null && !typing) PlayKeys(kb);
                if (mode == Mode.Edit) return;
                controls.Poll();
                stepper.Advance(Time.unscaledDeltaTime,
                    () => controls.Sample(sim.State.Player.SwapHand >= 0),
                    inp =>
                    {
                        sim.Step(inp);
                        anim.AfterTick(sim.State.Player, sim.Movement, CurrentMap, inp.Dir);
                        CameraTick();
                    });
            }
            catch (Exception e) { error = e.ToString(); Debug.LogException(e); }
        }

        void PlayKeys(Keyboard kb)
        {
            if (mode == Mode.Play)
            {
                Key[] digits = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8 };
                for (int i = 0; i < digits.Length && i < Sections.Length; i++) if (kb[digits[i]].wasPressedThisFrame) Warp(i);
                if (kb[Key.T].wasPressedThisFrame) Warp(section);
            }
            else
            {
                if (kb[Key.T].wasPressedThisFrame) { sim.PlaceAt(placeX, placeY); AfterPlace(); }
                if (kb[Key.F2].wasPressedThisFrame || kb[Key.F3].wasPressedThisFrame || controls.Pause.WasPressedThisFrame()) { BackToEditor(); return; }
            }
            if (kb[Key.F1].wasPressedThisFrame) showHelp = !showHelp;
            if (kb[Key.F5].wasPressedThisFrame) GUIUtility.systemCopyBuffer = sim.State.RunSeedText + " " + sim.State.BattleSeedText;
        }

        // The lab's camera: leads the character a little, and keeps his head on screen when he goes up.
        void CameraTarget(PlayerState p, out double tx, out double ty)
        {
            var m = CurrentMap;
            tx = Math.Min(Math.Max(p.X - ViewW * 0.42, 0), Math.Max(0, m.Width - ViewW));
            ty = Math.Max(0, p.Y - Math.Max(0.5, ViewH - FloorPad - sim.Movement.CharHeight - 1.0));
            if (!double.IsInfinity(m.Height)) ty = Math.Max(0, Math.Min(ty, m.Height - ViewH + FloorPad));
        }

        void CameraTick()
        {
            camPx = camX; camPy = camY;
            CameraTarget(sim.State.Player, out double tx, out double ty);
            camX += (tx - camX) * 0.2;
            camY += (ty - camY) * 0.18;
        }

        void LateUpdate()
        {
            if (error != null || sim == null) return;
            try
            {
                if (mode == Mode.Edit) { editor.ApplyCamera(); editor.DrawOverlay(); return; }
                double a = stepper.Alpha;
                FitView();
                cam.orthographicSize = ViewH / 2;
                double cx = camPx + (camX - camPx) * a, cy = camPy + (camY - camPy) * a;
                cam.transform.position = new Vector3((float)(cx + ViewW / 2), (float)(cy - FloorPad + ViewH / 2), -10);
                view.Draw(sim.State.Player, sim.Movement, CurrentMap, anim, sim.State.Frame, a);
            }
            catch (Exception e) { error = e.ToString(); Debug.LogException(e); }
        }

        // 24 by 13.5 tiles on every display: the largest 16:9 area that fits, smoothly scaled.
        void FitView()
        {
            float target = ViewW / ViewH, screen = (float)Screen.width / Math.Max(1, Screen.height);
            if (screen > target) { float w = target / screen; cam.rect = new Rect((1 - w) / 2, 0, w, 1); }
            else { float h = screen / target; cam.rect = new Rect(0, (1 - h) / 2, 1, h); }
        }

        void OnApplicationQuit() { editor?.SaveNow(); }

        void OnGUI()
        {
            if (error != null)
            {
                GUI.Label(new Rect(20, 20, Screen.width - 40, Screen.height - 40), "PRPG hit an error. Send Claude this text (it is also in Player.log):\n\n" + error);
                return;
            }
            if (sim == null) return;
            if (mode == Mode.Edit)
            {
                editor.OnGUI(sim.Movement, character);
                var ks = new GUIStyle(GUI.skin.label) { fontSize = Math.Max(11, Screen.height / 80), alignment = TextAnchor.LowerRight };
                ks.normal.textColor = new Color32(0xc0, 0xc0, 0xc0, 255);
                GUI.Label(new Rect(0, 0, Screen.width - 10, Screen.height - 6), MapEditor.KeysLine + "\n" + character, ks);
                return;
            }
            var r = cam.pixelRect;
            float unit = r.height / ViewH;                         // screen pixels per tile
            float x0 = r.x + 0.5f * unit, y0 = (Screen.height - r.yMax) + 0.45f * unit;
            var p = sim.State.Player;
            var P = sim.Movement;
            double st = double.IsNaN(p.Stamina) ? P.StaminaMax : p.Stamina;
            float frac = (float)Math.Max(0, Math.Min(1, st / P.StaminaMax));
            bool down = p.Exhaust > 0;
            float bw = 5 * unit, bh = 0.3f * unit;
            Fill(new Rect(x0 - 2, y0 - 2, bw + 4, bh + 4), Color.white);
            Fill(new Rect(x0, y0, bw, bh), Color.black);
            Fill(new Rect(x0, y0, bw * frac, bh), down ? new Color32(0x66, 0x66, 0x66, 255) : p.Winded ? new Color32(0x9a, 0x9a, 0x9a, 255) : Color.white);

            var style = new GUIStyle(GUI.skin.label) { fontSize = Math.Max(11, (int)(0.34f * unit)) };
            style.normal.textColor = new Color32(0xe8, 0xe8, 0xe8, 255);
            string Hand(int i) => p.SwapHand == i ? (p.Held(i) ? "sheathing" : "drawing") : (p.Held(i) ? (i == 0 ? "shield" : "sword") : "empty");
            string text = "STAMINA " + Math.Round(st) + (down ? "  exhausted" : p.Winded ? "  winded" : "")
                        + "\nLEFT " + Hand(0) + "   RIGHT " + Hand(1)
                        + "\n" + character;
            GUI.Label(new Rect(x0, y0 + bh + 0.1f * unit, 12 * unit, 3 * unit), text, style);

            if (showHelp)
            {
                string keys = mode == Mode.Play
                    ? "1-8 sections  T retry  Tab character  F4 map editor  F5 copy seeds  F1 hide"
                    : "Playing " + (editor.Doc != null ? editor.Doc.Name : "") + ":  T retry  F2 or Esc back to the editor  Tab character  F1 hide";
                string help = "Arrows move  Space jump  D sprint  F grab  Q/W left/right hand\n" + keys + "\n"
                            + sim.State.RunSeedText + "  " + sim.State.BattleSeedText;
                var hs = new GUIStyle(style) { alignment = TextAnchor.UpperRight };
                hs.normal.textColor = new Color32(0x8f, 0x8f, 0x8f, 255);
                GUI.Label(new Rect(r.x, y0, r.width - 0.5f * unit, 3 * unit), help, hs);
            }
        }

        static Texture2D white;
        static void Fill(Rect rect, Color col)
        {
            if (white == null) { white = new Texture2D(1, 1); white.SetPixel(0, 0, Color.white); white.Apply(); }
            var old = GUI.color; GUI.color = col; GUI.DrawTexture(rect, white); GUI.color = old;
        }
    }
}
