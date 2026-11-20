using System;
using System.Collections.Generic;
using Prpg.Sim;

namespace Prpg.Game.MapEditing
{
    /// <summary>A position in sixths of a tile. Every editor coordinate is one of these; nothing finer exists.</summary>
    public struct P6 : IEquatable<P6>
    {
        public int X, Y;
        public P6(int x, int y) { X = x; Y = y; }
        public double Tx => X / 6.0;
        public double Ty => Y / 6.0;
        public bool Equals(P6 o) => X == o.X && Y == o.Y;
        public override bool Equals(object o) => o is P6 p && Equals(p);
        public override int GetHashCode() => X * 73856093 ^ Y;
        public static P6 Snap(double tx, double ty) => new P6((int)Math.Round(tx * 6), (int)Math.Round(ty * 6));
        public override string ToString() => Tiles(X) + ", " + Tiles(Y);

        /// <summary>A length in sixths as tiles and sixths, e.g. "12 4/6".</summary>
        public static string Tiles(int sixths)
        {
            int whole = (int)Math.Floor(sixths / 6.0), rest = sixths - whole * 6;
            return rest == 0 ? whole.ToString() : whole + " " + rest + "/6";
        }
    }

    public sealed class ClimbDoc
    {
        public ClimbKind Kind;
        public int X, Y1, Y2;      // sixths
        public int Face;           // 0 free-standing, or the open side of the cliff face it runs down
        public ClimbDoc Copy() => (ClimbDoc)MemberwiseClone();
    }

    /// <summary>
    /// The editor's map: what Brad drew, in sixths. Ground is kept as the traces he clicked; the game's lines are
    /// worked out from them when the file is written. The file holds both, so the game reads plain lines and the
    /// editor gets its traces back.
    /// </summary>
    public sealed class MapDoc
    {
        public string Name = "untitled";
        public int Width = 72 * 6, Height = 20 * 6;
        public readonly List<List<P6>> Ground = new List<List<P6>>();      // open traces, left to right
        public readonly List<List<P6>> Rock = new List<List<P6>>();        // closed traces, clockwise
        public readonly List<P6[]> Scaffolds = new List<P6[]>();           // two points, left to right
        public readonly List<ClimbDoc> Climbs = new List<ClimbDoc>();
        public P6? Start;

        public static MapDoc NewDefault(string name)
        {
            var d = new MapDoc { Name = name };
            d.Ground.Add(new List<P6> { new P6(0, 12), new P6(d.Width, 12) });
            d.Start = new P6(18, 12);
            return d;
        }

        // ---- Shapes ----

        /// <summary>Puts an open trace left to right. False if it doubles back on itself, which ground cannot do.</summary>
        public static bool NormalizeGround(List<P6> t)
        {
            if (t.Count < 2) return false;
            int dir = 0;
            for (int i = 1; i < t.Count; i++)
            {
                int dx = t[i].X - t[i - 1].X;
                if (dx == 0) continue;
                int s = Math.Sign(dx);
                if (dir == 0) dir = s; else if (s != dir) return false;
            }
            if (dir == 0) return false;               // nothing but a vertical line: no surface
            if (dir < 0) t.Reverse();
            return true;
        }

        /// <summary>Puts a closed trace clockwise (y up), as the game expects floating rock.</summary>
        public static bool NormalizeRock(List<P6> t)
        {
            if (t.Count < 3) return false;
            long area2 = 0;
            for (int i = 0; i < t.Count; i++) { var a = t[i]; var b = t[(i + 1) % t.Count]; area2 += (long)a.X * b.Y - (long)b.X * a.Y; }
            if (area2 == 0) return false;
            if (area2 > 0) t.Reverse();               // counter-clockwise: turn it round
            return true;
        }

        // ---- File ----

        public JsonObject ToJson()
        {
            var o = new JsonObject();
            o.Set("name", Name);
            o.Set("unitsPerTile", 6.0);
            o.Set("width", (double)Width);
            o.Set("height", (double)Height);
            // What the game reads.
            var ground = new List<object>();
            foreach (var t in Ground)
                for (int i = 1; i < t.Count; i++)
                    if (t[i].X > t[i - 1].X) ground.Add(Nums(t[i - 1].X, t[i - 1].Y, t[i].X, t[i].Y));   // upright steps become cliffs
            o.Set("ground", ground);
            var rock = new List<object>();
            foreach (var t in Rock)
            {
                var pts = new List<object>();
                foreach (var p in t) pts.Add(Nums(p.X, p.Y));
                rock.Add(pts);
            }
            o.Set("floatingRock", rock);
            var scaff = new List<object>();
            foreach (var s in Scaffolds) scaff.Add(Nums(s[0].X, s[0].Y, s[1].X, s[1].Y));
            o.Set("scaffolds", scaff);
            var climbs = new List<object>();
            foreach (var c in Climbs)
            {
                var co = new JsonObject();
                co.Set("kind", c.Kind.ToString());
                co.Set("x", (double)c.X); co.Set("y1", (double)c.Y1); co.Set("y2", (double)c.Y2);
                if (c.Face != 0) co.Set("openSide", c.Face < 0 ? "left" : "right");
                climbs.Add(co);
            }
            o.Set("climbables", climbs);
            if (Start.HasValue) o.Set("start", Nums(Start.Value.X, Start.Value.Y));
            // What only the editor needs: the ground as drawn.
            var traces = new List<object>();
            foreach (var t in Ground)
            {
                var pts = new List<object>();
                foreach (var p in t) pts.Add(Nums(p.X, p.Y));
                traces.Add(pts);
            }
            o.Set("groundTraces", traces);
            return o;
        }

        static List<object> Nums(params int[] v)
        {
            var l = new List<object>();
            foreach (int n in v) l.Add((double)n);
            return l;
        }

        public string ToText() => Json.Write(ToJson(), 1) + "\n";

        public static MapDoc FromText(string text, string fallbackName)
        {
            var o = Json.Parse(text) as JsonObject ?? throw new FormatException("A map must be a JSON object");
            double u = o.Has("unitsPerTile") ? o.Number("unitsPerTile") : 1;
            if (u != 6) throw new FormatException("The editor only opens maps measured in sixths (unitsPerTile 6).");
            var d = new MapDoc { Name = o.String("name") ?? fallbackName };
            d.Width = (int)o.Number("width");
            d.Height = o.Has("height") ? (int)o.Number("height") : 20 * 6;
            if (o.Array("groundTraces") is List<object> traces)
                foreach (List<object> t in traces) d.Ground.Add(Points(t));
            else if (o.Array("ground") is List<object> lines)
                foreach (List<object> a in lines) d.Ground.Add(new List<P6> { new P6(I(a[0]), I(a[1])), new P6(I(a[2]), I(a[3])) });
            if (o.Array("floatingRock") is List<object> rock)
                foreach (List<object> t in rock) d.Rock.Add(Points(t));
            if (o.Array("scaffolds") is List<object> sc)
                foreach (List<object> a in sc) d.Scaffolds.Add(new[] { new P6(I(a[0]), I(a[1])), new P6(I(a[2]), I(a[3])) });
            if (o.Array("climbables") is List<object> cl)
                foreach (JsonObject c in cl)
                {
                    var kind = c.String("kind") == "Wall" ? ClimbKind.SmoothRock : (ClimbKind)Enum.Parse(typeof(ClimbKind), c.String("kind"));
                    string side = c.String("openSide");
                    d.Climbs.Add(new ClimbDoc { Kind = kind, X = I(c.Get("x")), Y1 = I(c.Get("y1")), Y2 = I(c.Get("y2")), Face = side == "left" ? -1 : side == "right" ? 1 : 0 });
                }
            if (o.Array("start") is List<object> st) d.Start = new P6(I(st[0]), I(st[1]));
            return d;
        }

        static int I(object v) => (int)Math.Round((double)v);
        static List<P6> Points(List<object> t)
        {
            var l = new List<P6>();
            foreach (List<object> p in t) l.Add(new P6(I(p[0]), I(p[1])));
            return l;
        }

        /// <summary>The map as the game sees it: the same loader the game uses on the saved file.</summary>
        public Map ToMap(WorldRules world) => Map.FromJson(ToJson(), world);

        public MapDoc Clone() => FromText(ToText(), Name);

        // ---- Warnings: shown, never blocking ----

        public List<string> Warnings(Map map, Movement P, WorldRules W)
        {
            var w = new List<string>();
            if (!Start.HasValue) w.Add("No start point.");
            for (int i = 0; i < Ground.Count; i++)
                for (int j = i + 1; j < Ground.Count; j++)
                {
                    int a0 = Ground[i][0].X, a1 = Ground[i][Ground[i].Count - 1].X, b0 = Ground[j][0].X, b1 = Ground[j][Ground[j].Count - 1].X;
                    if (Math.Max(a0, b0) < Math.Min(a1, b1)) w.Add("Ground traces " + (i + 1) + " and " + (j + 1) + " overlap.");
                }
            for (int i = 0; i < Climbs.Count; i++)
            {
                var c = Climbs[i];
                if (c.Face != 0) continue;
                double x = c.X / 6.0, top = c.Y2 / 6.0;
                bool step = false;
                foreach (var L in map.Lines)
                {
                    if (Mover.IsSteep(L, P) || x < L.X1 - P.ClimbReach || x > L.X2 + P.ClimbReach) continue;
                    if (Math.Abs(L.YAt(L.ClampX(x)) - top) < W.StepOff) { step = true; break; }
                }
                if (!step) w.Add(Name0(c) + " " + (i + 1) + ": nothing to step onto at its top.");
                for (int y = c.Y1 + 3; y < c.Y2 - 2; y += 3)
                    if (InRock(map, x, y / 6.0)) { w.Add(Name0(c) + " " + (i + 1) + " runs through rock."); break; }
            }
            if (Start.HasValue && Start.Value.X < 0 || Start.HasValue && Start.Value.X > Width) w.Add("The start point is outside the map.");
            return w;
        }

        static string Name0(ClimbDoc c) => c.Kind == ClimbKind.RoughRock ? "Rough rock" : c.Kind == ClimbKind.SmoothRock ? "Smooth rock" : c.Kind.ToString();

        /// <summary>Is this point inside rock: under solid ground, or inside floating rock?</summary>
        public static bool InRock(Map map, double x, double y)
        {
            foreach (var L in map.Lines)
                if (L.Solid && !L.Slab && x > L.X1 && x < L.X2 && y < L.YAt(x) - 0.05) return true;
            foreach (var poly in map.FloatingRock)
            {
                bool inside = false;
                for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                {
                    double xi = poly[i][0], yi = poly[i][1], xj = poly[j][0], yj = poly[j][1];
                    if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) inside = !inside;
                }
                if (inside) return true;
            }
            return false;
        }
    }
}
