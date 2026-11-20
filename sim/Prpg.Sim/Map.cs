using System;
using System.Collections.Generic;

namespace Prpg.Sim
{
    public enum ClimbKind { Ladder, Rope, Wall }

    /// <summary>A platform: a one-way line segment, flat or sloped, with x1 &lt; x2. Immutable.</summary>
    public sealed class Line
    {
        public readonly int Index;
        public readonly double X1, Y1, X2, Y2;
        public readonly double Th, Cos, Sin, Deg;
        public readonly int Sign;      // +1 rises to the right, -1 falls, 0 flat

        public Line(int index, double x1, double y1, double x2, double y2)
        {
            Index = index; X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
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

    /// <summary>A map: platforms and climbables. Order is kept exactly as in the file, because it settles ties.</summary>
    public sealed class Map
    {
        public string Name;
        public double Width;
        public readonly List<Line> Lines = new List<Line>();
        public readonly List<Climbable> Climbables = new List<Climbable>();

        /// <summary>Reads a map file. Fields it does not know are ignored, so the format can grow.</summary>
        public static Map Load(string json) => FromJson(Json.Parse(json) as JsonObject ?? throw new FormatException("A map must be a JSON object"));

        public static Map FromJson(JsonObject o)
        {
            var m = new Map { Name = o.String("name") ?? "", Width = o.Number("width") };
            var lines = o.Array("lines") ?? new List<object>();
            foreach (var item in lines)
            {
                var a = item as List<object> ?? throw new FormatException("Each line must be [x1, y1, x2, y2]");
                if (a.Count != 4) throw new FormatException("Each line must be [x1, y1, x2, y2]");
                double x1 = (double)a[0], y1 = (double)a[1], x2 = (double)a[2], y2 = (double)a[3];
                if (!(x2 > x1)) throw new FormatException("Line " + m.Lines.Count + " must have x1 < x2 (vertical lines are not platforms)");
                m.Lines.Add(new Line(m.Lines.Count, x1, y1, x2, y2));
            }
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
            return m;
        }
    }
}
