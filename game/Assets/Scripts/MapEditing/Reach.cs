using System;
using System.Collections.Generic;
using Prpg.Sim;

namespace Prpg.Game.MapEditing
{
    /// <summary>
    /// The reach overlay: from a spot, where can he get to? Worked out by running the real simulation, so it can
    /// never disagree with the game. Each path is one scripted attempt, recorded until he lands, catches a ledge or 3 seconds pass.
    /// </summary>
    public static class Reach
    {
        public sealed class Path
        {
            public string Name;
            public readonly List<double> Xs = new List<double>(), Ys = new List<double>();
            public bool Caught;           // ended hanging from a ledge
        }

        public sealed class Result
        {
            public double X, Y;                       // where he stood
            public readonly List<Path> Paths = new List<Path>();
            public double CatchHeight;                // highest a ledge can be and still be caught from a standing jump
            public double LungeReach;                 // how far ahead of him a lunge can catch, at the top of a running jump
        }

        public static Result From(Map map, Movement P, WorldRules W, double x, double y)
        {
            var r = new Result();
            int line = map.SurfaceBelow(x, y, P);
            if (line < 0) return null;
            r.X = x; r.Y = map.Lines[line].YAt(x);
            foreach (int dir in new[] { 1, -1 })
            {
                r.Paths.Add(Run(map, P, W, x, line, dir, false, dir > 0 ? "standing jump right" : "standing jump left"));
                r.Paths.Add(Run(map, P, W, x, line, dir, true, dir > 0 ? "running jump right" : "running jump left"));
            }
            var up = Run(map, P, W, x, line, 0, false, "straight up");
            r.Paths.Add(up);
            double top = double.NegativeInfinity;
            foreach (var yy in up.Ys) top = Math.Max(top, yy);
            r.CatchHeight = top + P.CharHeight + P.ArmReach;
            r.LungeReach = P.CharHeight + P.ArmReach;
            return r;
        }

        static Path Run(Map map, Movement P, WorldRules W, double x, int line, int dir, bool running, string name)
        {
            var sim = Simulation.Start(map, P, W, x, line, 1, 1);
            var p = sim.State.Player;
            if (running) p.Vx = dir * P.RunSpeed;              // as if after a run-up
            var path = new Path { Name = name };
            path.Xs.Add(p.X); path.Ys.Add(p.Y);
            bool left = false;
            for (int t = 0; t < 180; t++)
            {
                var inp = new Input { Dir = dir, Run = running, JumpHeld = true, JumpPressed = t == 0, Grab = true, Hand = HandKey.None };
                sim.Step(inp);
                path.Xs.Add(p.X); path.Ys.Add(p.Y);
                if (!p.OnGround) left = true;
                if (p.Mode == Mode.Hang) { path.Caught = true; break; }
                if (left && p.OnGround) break;
                if (p.Mode == Mode.Climb) break;
            }
            return path;
        }
    }
}
