using System;

namespace Prpg.Sim
{
    /// <summary>
    /// Our own random number generator (PCG32, XSH-RR variant). Its output for a given seed is fixed forever:
    /// do not change this code. Tests pin the first outputs for known seeds.
    /// </summary>
    public struct Rng
    {
        const ulong Multiplier = 6364136223846793005UL;
        const ulong Increment = 1442695040888963407UL;
        public ulong State;

        public static Rng FromSeed(ulong seed)
        {
            var r = new Rng { State = 0 };
            r.NextUInt();
            r.State += seed;
            r.NextUInt();
            return r;
        }

        public uint NextUInt()
        {
            ulong old = State;
            State = unchecked(old * Multiplier + Increment);
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        /// <summary>A whole number from 0 up to but not including n, without bias.</summary>
        public int Below(int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            uint bound = (uint)n;
            uint threshold = unchecked((uint)(-(int)bound)) % bound;
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold) return (int)(r % bound);
            }
        }

        /// <summary>A double in [0, 1) with 32 bits of randomness.</summary>
        public double NextDouble() => NextUInt() / 4294967296.0;
    }

    /// <summary>
    /// The whole simulation state as one value. Clone it, run forward, put the clone back,
    /// and the game is exactly where it was. This is the foundation for Imagine.
    /// </summary>
    public sealed class SimState
    {
        public long Frame;                 // simulation ticks since the run began. Never resets.
        public ulong RunSeed, BattleSeed;
        public Rng RunRng, BattleRng;
        public PlayerState Player;

        public SimState Clone()
        {
            var s = (SimState)MemberwiseClone();
            s.Player = Player.Clone();
            return s;
        }

        /// <summary>Seeds as text a player could copy, e.g. "RUN-00000000075BCD15".</summary>
        public string RunSeedText => "RUN-" + RunSeed.ToString("X16");
        public string BattleSeedText => "BTL-" + BattleSeed.ToString("X16");

        public static bool TryParseSeed(string text, out ulong seed)
        {
            seed = 0;
            if (text == null) return false;
            text = text.Trim();
            int dash = text.IndexOf('-');
            if (dash >= 0) text = text.Substring(dash + 1);
            return ulong.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out seed);
        }
    }

    /// <summary>
    /// Owns the state, the map and the movement numbers, and advances one fixed tick at a time.
    /// Nothing here reads time, devices or Unity.
    /// </summary>
    public sealed class Simulation
    {
        public readonly Map Map;
        public Movement Movement;          // the tuning panel may replace numbers between ticks
        public WorldRules World;
        public SimState State;

        public Simulation(Map map, Movement movement, WorldRules world, SimState state)
        {
            Map = map; Movement = movement; World = world; State = state;
        }

        public static Simulation Start(Map map, Movement movement, WorldRules world, double x, int line, ulong runSeed, ulong battleSeed)
        {
            var s = new SimState
            {
                Frame = 0,
                RunSeed = runSeed,
                BattleSeed = battleSeed,
                RunRng = Rng.FromSeed(runSeed),
                BattleRng = Rng.FromSeed(battleSeed),
                Player = PlayerState.Make(map, x, line),
            };
            return new Simulation(map, movement, world, s);
        }

        /// <summary>The highest ground (not floating rock) under x (the first one, on a tie), as the lab's trace recorder picks it.</summary>
        public static int GroundLineAt(Map map, double x)
        {
            Line best = null;
            foreach (var L in map.Lines)
                if (L.Solid && !L.Slab && x >= L.X1 && x <= L.X2 && (best == null || L.YAt(x) > best.YAt(x))) best = L;
            if (best == null) throw new ArgumentException("No solid ground under x = " + x);
            return best.Index;
        }

        public void Step(Input input)
        {
            Mover.Step(State.Player, Movement, World, input, Map);
            State.Frame++;
        }

        /// <summary>
        /// Puts him at (x, y), standing if a surface is right under his feet, otherwise in the air to fall from there.
        /// For the editor's "play from here". His safe spot is the surface below, if any.
        /// </summary>
        public void PlaceAt(double x, double y)
        {
            var p = State.Player;
            if (Map.Lines.Count == 0) { p.X = p.Px = x; p.Y = p.Py = y; p.OnGround = false; p.Line = -1; return; }
            int below = Map.SurfaceBelow(x, y, Movement);
            if (below >= 0 && y - Map.Lines[below].YAt(x) < 0.25)
            {
                Mover.Respawn(p, Map, x, below);
                p.SafeX = x; p.SafeLine = below;
                return;
            }
            int safe = below >= 0 ? below : 0;
            Mover.Respawn(p, Map, Map.Lines[safe].ClampX(x), safe);
            p.SafeX = p.X; p.SafeLine = safe;
            p.X = p.Px = x; p.Y = p.Py = y; p.OnGround = false; p.Line = -1;
        }

        /// <summary>Lab-style warp: stand him on the highest ground at x and make that his safe spot. A test convenience.</summary>
        public void Warp(double x)
        {
            int line = GroundLineAt(Map, x);
            var p = State.Player;
            Mover.Respawn(p, Map, x, line);
            p.SafeX = x; p.SafeLine = line;
        }

        public SimState Save() => State.Clone();
        public void Restore(SimState saved) => State = saved.Clone();
    }

    /// <summary>
    /// Turns uneven screen frames into exact 1/60 s ticks. The screen calls Advance with however much time passed;
    /// this runs zero or more ticks, sampling input once per tick, and reports how far between the last two
    /// ticks the screen is, for blending. Time is counted in whole microseconds so rounding never adds a tick.
    /// </summary>
    public sealed class FixedStepper
    {
        const long TickMicros = 1000000;   // in units of 1/60 microsecond, so one tick is exactly 1,000,000
        long acc;
        public int MaxTicksPerAdvance = 8; // after a long stall, drop time rather than spiral

        /// <summary>Runs the ticks that fit in elapsedSeconds. Returns how many ran.</summary>
        public int Advance(double elapsedSeconds, Func<Input> sampleInput, Action<Input> tick)
        {
            if (elapsedSeconds > 0) acc += (long)Math.Round(elapsedSeconds * 60.0 * 1000000.0);
            int n = 0;
            while (acc >= TickMicros && n < MaxTicksPerAdvance)
            {
                tick(sampleInput());
                acc -= TickMicros;
                n++;
            }
            if (n == MaxTicksPerAdvance && acc > TickMicros) acc = 0;
            return n;
        }

        /// <summary>0 to 1: how far the screen is from the previous tick toward the next.</summary>
        public double Alpha => Math.Min(1.0, (double)acc / TickMicros);
    }
}
