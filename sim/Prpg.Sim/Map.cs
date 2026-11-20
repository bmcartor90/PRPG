using System;
using System.Collections.Generic;

namespace Prpg.Sim
{
    public enum ClimbKind { Ladder, Rope, Wall }

    /// <summary>
    /// A surface: a line segment, flat or sloped, with x1 &lt; x2. Immutable.
    /// Solid lines are the top of ground filled down to the bottom of the map; the rest are scaffolds you can jump up through.
    /// </summary>
    public sealed class Line
    {
        public readonly int Index;
        public readonly bool Solid;
        public readonly bool Slab;     // the top of floating rock (solid, but it brings its own faces)
        public readonly double X1, Y1, X2, Y2;
        public readonly double Th, Cos, Sin, Deg;
        public readonly int Sign;      // +1 rises to the right, -1 falls, 0 flat

        public Line(int index, bool solid, bool slab, double x1, double y1, double x2, double y2)
        {
            Index = index; Solid = solid; Slab = slab; X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
            Th = Math.Atan2(Math.Abs(y2 - y1), x2 - x1);
            Cos = Math.Cos(Th);
            Sin = Math.Sin(Th);
            Sign = Math.Sign(y2 - y1);
            Deg = Th * 180 / Math.PI;
        }

        public double YAt(double x) => Y1 + (Y2 - Y1) * (x - X1) / (X2 - X1);
        public double ClampX(double x) => Math.Min(Math.Max(x, X1), X2);
    }

    /// <summary>A ladder, rope or wall: a vertical climbable at x from y1 up to y2. Immutable.</summary>
    public sealed class Climbable
    {
        public readonly int Index;
        public readonly ClimbKind Kind;
        public readonly double X, Y1, Y2;
        public Climbable(int index, ClimbKind kind, double x, double y1, double y2)
        {
            Index = index; Kind = kind; X = x; Y1 = y1; Y2 = y2;
        }
    }

    /// <summary>An underside of floating rock, x1 &lt; x2. Rising into it stops his rise; it also blocks walking in where it is too low.</summary>
    public sealed class Underside
    {
        public readonly double X1, Y1, X2, Y2;
        public Underside(double x1, double y1, double x2, double y2) { X1 = x1; Y1 = y1; X2 = x2; Y2 = y2; }
        public double YAt(double x) => Y1 + (Y2 - Y1) * (x - X1) / (X2 - X1);
    }

    /// <summary>
    /// A cliff face: where solid ground ends or steps down. Blocks sideways movement for anyone whose feet are below Top.
    /// Derived from the ends of ground lines and from the upright edges of floating rock, never authored.
    /// Faces of floating rock have a real bottom: he passes under them when his head is below it.
    /// </summary>
    public sealed class Cliff
    {
        public readonly double X, Top, Bottom;   // Bottom is negative infinity when the face runs out of the map
        public readonly int Face;                // which side is open air: -1 left, 1 right
        public Cliff(double x, double top, double bottom, int face) { X = x; Top = top; Bottom = bottom; Face = face; }
    }

    /// <summary>
    /// A map: surfaces and climbables, and the cliff faces derived from the ground.
    /// Lines are numbered ground first, then the tops of floating rock, then scaffolds, each in file order.
    /// Order matters: it settles ties, and it is the lab's order.
    /// </summary>
    public sealed class Map
    {
        public string Name;
        public double Width;
        public readonly List<Line> Lines = new List<Line>();
        public readonly List<Climbable> Climbables = new List<Climbable>();
        public readonly List<Cliff> Cliffs = new List<Cliff>();
        public readonly List<Underside> Undersides = new List<Underside>();
        /// <summary>Each floating rock's outline in tiles, as listed in the file (clockwise). For drawing.</summary>
        public readonly List<double[][]> FloatingRock = new List<double[][]>();
        public double UnitsPerTile = 1;

        /// <summary>Reads a map file. Fields it does not know are ignored, so the format can grow.</summary>
        public static Map Load(string json, WorldRules world) => FromJson(Json.Parse(json) as JsonObject ?? throw new FormatException("A map must be a JSON object"), world);

        public static Map FromJson(JsonObject o, WorldRules world)
        {
            // Coordinates are whole numbers of a unit; the file says how many make a tile (6 for editor maps).
            double u = o.Has("unitsPerTile") ? o.Number("unitsPerTile") : 1;
            if (!(u > 0)) throw new FormatException("unitsPerTile must be positive");
            var m = new Map { Name = o.String("name") ?? "", UnitsPerTile = u, Width = o.Number("width") / u };
            AddLines(m, o.Array("ground"), true, u);
            var faces = new List<Cliff>();
            foreach (var item in o.Array("floatingRock") ?? new List<object>()) m.AddSlab(item as List<object> ?? throw new FormatException("Each floating rock must be a list of points"), u, faces);
            AddLines(m, o.Array("scaffolds"), false, u);
            var climbs = o.Array("climbables") ?? new List<object>();
            foreach (var item in climbs)
            {
                var c = item as JsonObject ?? throw new FormatException("Each climbable must be an object");
                ClimbKind kind;
                switch (c.String("kind"))
                {
                    case "Ladder": kind = ClimbKind.Ladder; break;
                    case "Rope": kind = ClimbKind.Rope; break;
                    case "Wall": kind = ClimbKind.Wall; break;
                    default: throw new FormatException("Unknown climbable kind \"" + c.String("kind") + "\"");
                }
                m.Climbables.Add(new Climbable(m.Climbables.Count, kind, c.Number("x") / u, c.Number("y1") / u, c.Number("y2") / u));
            }
            m.BuildCliffs(world);
            m.Cliffs.AddRange(faces);
            return m;
        }

        static void AddLines(Map m, List<object> lines, bool solid, double u)
        {
            if (lines == null) return;
            foreach (var item in lines)
            {
                var a = item as List<object> ?? throw new FormatException("Each line must be [x1, y1, x2, y2]");
                if (a.Count != 4) throw new FormatException("Each line must be [x1, y1, x2, y2]");
                double x1 = (double)a[0] / u, y1 = (double)a[1] / u, x2 = (double)a[2] / u, y2 = (double)a[3] / u;
                if (!(x2 > x1)) throw new FormatException("Line " + m.Lines.Count + " must have x1 < x2 (vertical lines are not surfaces)");
                m.Lines.Add(new Line(m.Lines.Count, solid, false, x1, y1, x2, y2));
            }
        }

        // The lab's makeSlab: walk the clockwise outline. Left-to-right edges are surfaces, right-to-left edges are undersides,
        // upright edges are faces with a bottom.
        void AddSlab(List<object> points, double u, List<Cliff> faces)
        {
            int n = points.Count;
            if (n < 3) throw new FormatException("A floating rock needs at least three points");
            var px = new double[n]; var py = new double[n];
            for (int i = 0; i < n; i++)
            {
                var pt = points[i] as List<object>;
                if (pt == null || pt.Count != 2) throw new FormatException("Each floating rock point must be [x, y]");
                px[i] = (double)pt[0] / u; py[i] = (double)pt[1] / u;
            }
            var outline = new double[n][];
            for (int i = 0; i < n; i++) outline[i] = new[] { px[i], py[i] };
            FloatingRock.Add(outline);
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                double ax = px[i], ay = py[i], bx = px[j], by = py[j];
                if (bx > ax) Lines.Add(new Line(Lines.Count, true, true, ax, ay, bx, by));
                else if (bx < ax) Undersides.Add(new Underside(bx, by, ax, ay));
                else if (by != ay) faces.Add(new Cliff(ax, Math.Max(ay, by), Math.Min(ay, by), by < ay ? 1 : -1));
            }
        }

        static readonly int[] Sides = { -1, 1 };

        // The lab's buildCliffs: at each end of solid ground, look just beyond it. If other ground carries on at the same
        // height or higher there is no cliff; otherwise a face runs from this end down to the highest lower ground (or out of the map).
        void BuildCliffs(WorldRules W)
        {
            Cliffs.Clear();
            foreach (var M in Lines)
            {
                if (!M.Solid || M.Slab) continue;          // floating rock brings its own faces
                foreach (int side in Sides)
                {
                    double ex = side < 0 ? M.X1 : M.X2, ey = side < 0 ? M.Y1 : M.Y2, bx = ex + side * W.CliffProbe;
                    double below = double.NegativeInfinity;
                    bool covered = false;
                    foreach (var N in Lines)
                    {
                        if (N == M || !N.Solid || N.Slab || bx < N.X1 || bx > N.X2) continue;
                        double ny = N.YAt(bx);
                        if (ny >= ey - W.CliffLevel) { covered = true; break; }
                        if (ny > below) below = ny;
                    }
                    if (!covered) Cliffs.Add(new Cliff(ex, ey, below, side));
                }
            }
        }
    }
}
