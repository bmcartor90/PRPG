using System;
using System.Collections.Generic;
using System.Linq;
using Prpg.Sim;
using Xunit;

namespace Prpg.Sim.Tests
{
    public class TraceTests
    {
        public static IEnumerable<object[]> Names() => Repo.TraceNames().Select(n => new object[] { n });

        static string ModeName(Mode m) => m switch { Mode.Normal => "normal", Mode.Hang => "hang", Mode.Pull => "pull", _ => "climb" };

        // The trace values are rounded to 6 decimals, so a perfect port can differ from them by up to 5e-7.
        const double Tol = 1e-6;

        [Theory]
        [MemberData(nameof(Names))]
        public void MatchesTheLabEveryTick(string name)
        {
            var t = Trace.Load(name);
            var map = Repo.Map();
            var sim = t.Start(map, Repo.Lucca());
            Assert.Equal(t.States.Count, t.Inputs.Count);
            Assert.Equal(t.StartY, sim.State.Player.Y);
            for (int i = 0; i < t.Inputs.Count; i++)
            {
                sim.Step(t.Inputs[i]);
                var p = sim.State.Player;
                var e = t.States[i];
                string where = $"{name} tick {i}";
                Near((double)e[0], p.X, where, "x");
                Near((double)e[1], p.Y, where, "y");
                Near((double)e[2], p.Vx, where, "vx");
                Near((double)e[3], p.Vy, where, "vy");
                // After a respawn the lab's stamina is null until the next tick, and the recorder rounds null to 0.
                // Ours is NaN (unset) at the same moment.
                Near((double)e[4], double.IsNaN(p.Stamina) ? 0 : p.Stamina, where, "stamina");
                Assert.True((string)e[5] == ModeName(p.Mode), $"{where}: mode {ModeName(p.Mode)}, lab {e[5]}");
                Assert.True((double)e[6] == (p.OnGround ? 1 : 0), $"{where}: grounded {p.OnGround}, lab {e[6]}");
                Assert.True((double)e[7] == (p.HeldLeft ? 1 : 0), $"{where}: left hand {p.HeldLeft}, lab {e[7]}");
                Assert.True((double)e[8] == (p.HeldRight ? 1 : 0), $"{where}: right hand {p.HeldRight}, lab {e[8]}");
                Assert.True((double)e[9] == (p.DodgeSliding ? 1 : 0), $"{where}: dodge slide {p.DodgeSliding}, lab {e[9]}");
            }
        }

        static void Near(double expected, double actual, string where, string what)
        {
            if (!(Math.Abs(expected - actual) <= Tol))
                throw new Xunit.Sdk.XunitException($"{where}: {what} = {actual:R}, lab {expected:R} (off by {Math.Abs(expected - actual):E2})");
        }

        [Fact]
        public void ThereAreTwentyEightTraces() => Assert.Equal(28, Repo.TraceNames().Count());

        [Fact]
        public void CliffsMatchTheLab()
        {
            var w = (JsonObject)Json.Parse(Repo.Read("feel-lab", "traces", "_world.json"));
            var lab = w.Array("cliffs").Cast<JsonObject>().Select(c =>
                $"{c.Number("x"):R} {c.Number("top"):R} {(c.Get("bottom") == null ? "out" : c.Number("bottom").ToString("R"))} {c.String("openSide")}").ToList();
            var ours = Repo.Map().Cliffs.Select(c =>
                $"{c.X:R} {c.Top:R} {(double.IsNegativeInfinity(c.Bottom) ? "out" : c.Bottom.ToString("R"))} {(c.Face < 0 ? "left" : "right")}").ToList();
            Assert.Equal(lab, ours);
        }

        [Fact]
        public void UndersidesMatchTheLab()
        {
            var w = (JsonObject)Json.Parse(Repo.Read("feel-lab", "traces", "_world.json"));
            var lab = w.Array("undersides").Cast<List<object>>().Select(a => string.Join(" ", a.Select(v => ((double)v).ToString("R")))).ToList();
            var ours = Repo.Map().Undersides.Select(u => $"{u.X1:R} {u.Y1:R} {u.X2:R} {u.Y2:R}").ToList();
            Assert.Equal(lab, ours);
        }

        [Fact]
        public void NinaIsLuccaButShorterAndRamorandIsLucca()
        {
            var text = Repo.Read("data", "movement.json");
            var lucca = MovementFile.Load(text, "Lucca");
            var nina = MovementFile.Load(text, "Nina");
            var ramorand = MovementFile.Load(text, "Ramorand");
            foreach (var n in Movement.Names) Assert.True(lucca.Get(n) == ramorand.Get(n), n);
            foreach (var n in Movement.Names)
                if (n == "charHeight") Assert.Equal(1.7, nina.Get(n));
                else Assert.True(lucca.Get(n) == nina.Get(n), n);
        }

        [Fact]
        public void DataFilesMatchTheLabWorld()
        {
            var w = (JsonObject)Json.Parse(Repo.Read("feel-lab", "traces", "_world.json"));
            var labParams = w.Object("params");
            var lucca = Repo.Lucca();
            Assert.Equal(Movement.SliderNames, labParams.Keys.ToArray());
            foreach (var n in labParams.Keys) Assert.True(labParams.Number(n) == lucca.Get(n), "movement.json differs from the lab on " + n);

            var fixedChar = w.Object("fixed").Object("character");
            foreach (var n in fixedChar.Keys) Assert.True(fixedChar.Number(n) == lucca.Get(n), "movement.json differs from the lab's BODY table on " + n);
            Assert.Equal(Movement.FixedNames, fixedChar.Keys.ToArray());
            var fixedWorld = w.Object("fixed").Object("world");
            var world = Repo.World();
            Assert.Equal(WorldRules.Names, fixedWorld.Keys.ToArray());
            foreach (var n in fixedWorld.Keys) Assert.True(fixedWorld.Number(n) == world.Get(n), "world.json differs from the lab's WORLD table on " + n);

            var labMap = Map.FromJson(w.Object("world"), Repo.World());
            var map = Repo.Map();
            Assert.Equal(labMap.Width, map.Width);
            Assert.Equal(labMap.Lines.Count, map.Lines.Count);
            for (int i = 0; i < map.Lines.Count; i++)
            {
                var a = labMap.Lines[i]; var b = map.Lines[i];
                Assert.True(a.Solid == b.Solid && a.X1 == b.X1 && a.Y1 == b.Y1 && a.X2 == b.X2 && a.Y2 == b.Y2, "line " + i + " differs");
            }
            Assert.Equal(labMap.Climbables.Count, map.Climbables.Count);
            for (int i = 0; i < map.Climbables.Count; i++)
            {
                var a = labMap.Climbables[i]; var b = map.Climbables[i];
                Assert.True(a.Kind == b.Kind && a.X == b.X && a.Y1 == b.Y1 && a.Y2 == b.Y2, "climbable " + i + " differs");
            }
        }
    }
}
