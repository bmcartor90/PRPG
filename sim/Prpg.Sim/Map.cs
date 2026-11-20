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
        public readonly double X1, Y1, X2, Y2;
        public readonly double Th, Cos, Sin, Deg;
        public readonly int Sign;      // +1 rises to the right, -1 falls, 0 flat

        public Line(int index, bool solid, double x1, double y1, double x2, double y2)
        {
            Index = index; Solid = solid; X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
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

    /// <summary>
    /// A cliff face: where solid ground ends or steps down. Blocks sideways movement for anyone whose feet are below Top.
    /// Derived from the ground, never authored.
    /// </summary>
    public sealed class Cliff
    {
        public readonly double X, Top, Bottom;   // Bottom is negative infinity when the face runs out of the map
        public readonly int Face;                // which side is open air: -1 left, 1 right
        public Cliff(double x, double top, double bottom, int face) { X = x; Top = top; Bottom = bottom; Face = face; }
    }

    /// <summary>
    /// A map: surfaces and climbables, and the cliff faces derived from the ground.
    /// Lines are numbered ground first, then scaffolds, each in file order. Order matters: it settles ties.
    /// </summary>
    public sealed class Map
    {
        public string Name;
        public double Width;
        public readonly List<Line> Lines = new List<Line>();
        public readonly List<Climbable> Climbables = new List<Climbable>();
        public readonly List<Cliff> Cliffs = new List<Cliff>();

        /// <summary>Reads a map file. Fields it does not know are ignored, so the format can grow.</summary>
        public static Map Load(string json) => FromJson(Json.Parse(json) as JsonObject ?? throw new FormatException("A map must be a JSON object"));

        public static Map FromJson(JsonObject o)
        {
            var m = new Map { Name = o.String("name") ?? "", Width = o.Number("width") };
            AddLines(m, o.Array("ground"), true);
            AddLines(m, o.Array("scaffolds"), false);
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
                m.Climbables.Add(new Climbable(m.Climbables.Count, kind, c.Number("x"), c.Number("y1"), c.Number("y2")));
            }
            m.BuildCliffs();
            return m;
        }

        static void AddLines(Map m, List<object> lines, bool solid)
        {
            if (lines == null) return;
            foreach (var item in lines)
            {
                var a = item as List<object> ?? throw new FormatException("Each line must be [x1, y1, x2, y2]");
                if (a.Count != 4) throw new FormatException("Each line must be [x1, y1, x2, y2]");
                double x1 = (double)a[0], y1 = (double)a[1], x2 = (double)a[2], y2 = (double)a[3];
                if (!(x2 > x1)) throw new FormatException("Line " + m.Lines.Count + " must have x1 < x2 (vertical lines are not surfaces)");
                m.Lines.Add(new Line(m.Lines.Count, solid, x1, y1, x2, y2));
            }
        }

        static readonly int[] Sides = { -1, 1 };

        // The lab's buildCliffs: at each end of solid ground, look just beyond it. If other ground carries on at the same
        // height or higher there is no cliff; otherwise a face runs from this end down to the highest lower ground (or out of the map).
        void BuildCliffs()
        {
            Cliffs.Clear();
            foreach (var M in Lines)
            {
                if (!M.Solid) continue;
                foreach (int side in Sides)
                {
                    double ex = side < 0 ? M.X1 : M.X2, ey = side < 0 ? M.Y1 : M.Y2, bx = ex + side * 0.01;
                    double below = double.NegativeInfinity;
                    bool covered = false;
                    foreach (var N in Lines)
                    {
                        if (N == M || !N.Solid || bx < N.X1 || bx > N.X2) continue;
                        double ny = N.YAt(bx);
                        if (ny >= ey - 0.05) { covered = true; break; }
                        if (ny > below) below = ny;
                    }
                    if (!covered) Cliffs.Add(new Cliff(ex, ey, below, side));
                }
            }
        }
    }
}
