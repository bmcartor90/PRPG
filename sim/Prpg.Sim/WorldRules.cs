using System;
using System.Collections.Generic;

namespace Prpg.Sim
{
    /// <summary>
    /// Numbers that belong to the game rather than to a character (the lab's WORLD table).
    /// Loaded from data/world.json.
    /// </summary>
    public sealed class WorldRules
    {
        public double Snap;   // how far apart two line ends can be and still count as joined
        public double Join;   // how close two line ends must be to count as the same point
        public double StepOff;   // how close to a platform's height the top of a climbable must be to step onto it
        public double EdgeMargin;   // he cannot go closer than this to either end of the map
        public double RespawnBelow;   // fall below this and he is put back on the last safe ground (a lab convenience)
        public double SafeInset;   // the safe spot is kept this far in from a platform's ends

        public static readonly string[] Names = { "snap", "join", "stepOff", "edgeMargin", "respawnBelow", "safeInset" };

        public double Get(string name)
        {
            switch (name)
            {
                case "snap": return Snap;
                case "join": return Join;
                case "stepOff": return StepOff;
                case "edgeMargin": return EdgeMargin;
                case "respawnBelow": return RespawnBelow;
                case "safeInset": return SafeInset;
                default: throw new ArgumentException("Unknown world number " + name);
            }
        }

        public bool Set(string name, double v)
        {
            switch (name)
            {
                case "snap": Snap = v; return true;
                case "join": Join = v; return true;
                case "stepOff": StepOff = v; return true;
                case "edgeMargin": EdgeMargin = v; return true;
                case "respawnBelow": RespawnBelow = v; return true;
                case "safeInset": SafeInset = v; return true;
                default: return false;
            }
        }

        public WorldRules Clone() => (WorldRules)MemberwiseClone();

        /// <summary>Reads data/world.json. Every number must be present; unknown keys are ignored.</summary>
        public static WorldRules Load(string json) => FromJson(Json.Parse(json) as JsonObject ?? throw new FormatException("world.json must be a JSON object"));

        public static WorldRules FromJson(JsonObject o)
        {
            var w = new WorldRules();
            var missing = new List<string>();
            foreach (var n in Names)
            {
                if (!o.Has(n)) { missing.Add(n); continue; }
                w.Set(n, o.Number(n));
            }
            if (missing.Count > 0) throw new FormatException("world.json is missing: " + string.Join(", ", missing));
            return w;
        }
    }
}
