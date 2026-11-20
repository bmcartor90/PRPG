using System;
using System.Collections.Generic;
using System.Linq;
using Prpg.Sim;
using Xunit;

namespace Prpg.Sim.Tests
{
    public class StateTests
    {
        public static IEnumerable<object[]> Names() => TraceTests.Names();

        // Every field of the state, bit for bit, so "identical" means identical.
        static string Snapshot(SimState s)
        {
            var p = s.Player;
            string B(double v) => BitConverter.DoubleToInt64Bits(v).ToString("X16");
            return string.Join("|", new[]
            {
                s.Frame.ToString(), s.RunSeed.ToString(), s.BattleSeed.ToString(), s.RunRng.State.ToString(), s.BattleRng.State.ToString(),
                B(p.X), B(p.Y), B(p.Px), B(p.Py), B(p.Vx), B(p.Vy), p.OnGround.ToString(), p.Line.ToString(), B(p.Coyote), B(p.Buffer),
                p.Jumping.ToString(), p.Cutting.ToString(), B(p.Lift), B(p.PLift), B(p.SafeX), p.SafeLine.ToString(),
                B(p.Stamina), B(p.Exhaust), p.Winded.ToString(), p.Spent.ToString(), p.Crouch.ToString(), p.Mode.ToString(),
                p.HangLine.ToString(), p.HangSide.ToString(), B(p.HangProg), B(p.HangY0), p.Climb.ToString(),
                B(p.GrabLock), p.Lunge.ToString(), B(p.Prone), p.HeldLeft.ToString(), p.HeldRight.ToString(),
                p.SwapHand.ToString(), B(p.SwapT), B(p.SwapTotal), B(p.Duck), p.DownWas.ToString(), p.Low.ToString(),
            });
        }

        static List<string> Run(Simulation sim, IList<Input> inputs)
        {
            var o = new List<string>();
            foreach (var i in inputs) { sim.Step(i); o.Add(Snapshot(sim.State)); }
            return o;
        }

        // Trace inputs, extended past their end with a little extra movement so 300-tick windows always fit.
        static List<Input> LongInputs(Trace t)
        {
            var list = new List<Input>(t.Inputs);
            var extra = new[] { new Input { Dir = 1, Run = true }, new Input { Dir = -1 }, new Input { JumpHeld = true, JumpPressed = true }, new Input { Grab = true, Up = true } };
            for (int i = 0; list.Count < t.Inputs.Count + 320; i++)
                for (int k = 0; k < 40; k++) { var inp = extra[i % extra.Length]; if (k > 0) inp.JumpPressed = false; list.Add(inp); }
            return list;
        }

        [Theory]
        [MemberData(nameof(Names))]
        public void Repeatable(string name)
        {
            var t = Trace.Load(name);
            var map = Repo.Map(); var m = Repo.Lucca();
            var a = Run(t.Start(map, m), t.Inputs);
            var b = Run(t.Start(map, m), t.Inputs);
            Assert.Equal(a, b);
        }

        [Theory]
        [MemberData(nameof(Names))]
        public void SaveAndRestore(string name)
        {
            var t = Trace.Load(name);
            var map = Repo.Map(); var m = Repo.Lucca();
            var inputs = LongInputs(t);
            // Several arbitrary save points through the trace, including mid-hang, mid-climb and mid-draw where they occur.
            for (int at = 0; at < t.Inputs.Count; at += 37)
            {
                var sim = t.Start(map, m);
                for (int i = 0; i < at; i++) sim.Step(inputs[i]);
                var saved = sim.Save();
                string savedSnap = Snapshot(saved);
                var window = inputs.Skip(at).Take(300).ToList();
                var first = Run(sim, window);
                sim.Restore(saved);
                var second = Run(sim, window);
                Assert.Equal(first, second);
                Assert.Equal(savedSnap, Snapshot(saved));      // running on never touches the saved copy
            }
        }

        [Fact]
        public void RestoringDoesNotShareStateWithTheSave()
        {
            var t = Trace.Load("draw-and-sheathe");
            var sim = t.Start(Repo.Map(), Repo.Lucca());
            for (int i = 0; i < 20; i++) sim.Step(t.Inputs[i]);
            var saved = sim.Save();
            string before = Snapshot(saved);
            sim.Restore(saved);
            for (int i = 20; i < t.Inputs.Count; i++) sim.Step(t.Inputs[i]);
            Assert.Equal(before, Snapshot(saved));
        }

        [Fact]
        public void FrameCounterCountsEveryTickAndNeverResets()
        {
            var t = Trace.Load("running-gap-jumps");      // includes a fall into a gap and a respawn
            var sim = t.Start(Repo.Map(), Repo.Lucca());
            for (int i = 0; i < t.Inputs.Count; i++) { sim.Step(t.Inputs[i]); Assert.Equal(i + 1, sim.State.Frame); }
        }

        [Fact]
        public void FrameRateDoesNotChangeTheResult()
        {
            var t = Trace.Load("ledge-grab-and-pull-up");
            var map = Repo.Map(); var m = Repo.Lucca();
            var reference = Run(t.Start(map, m), t.Inputs);

            // Screens at 30, 60, 144 and 240 Hz, and a jittery one, all driven for the same total time.
            var patterns = new Func<int, double>[]
            {
                f => 1.0 / 30, f => 1.0 / 60, f => 1.0 / 144, f => 1.0 / 240,
                f => new[] { 0.004, 0.021, 0.011, 0.0167, 0.0333, 0.002 }[f % 6],
            };
            foreach (var frameTime in patterns)
            {
                var sim = t.Start(map, m);
                var stepper = new FixedStepper();
                var got = new List<string>();
                int tick = 0;
                for (int f = 0; tick < t.Inputs.Count; f++)
                {
                    stepper.Advance(frameTime(f), () => t.Inputs[Math.Min(tick, t.Inputs.Count - 1)], inp =>
                    {
                        if (tick >= t.Inputs.Count) return;
                        sim.Step(inp); got.Add(Snapshot(sim.State)); tick++;
                    });
                    Assert.InRange(stepper.Alpha, 0.0, 1.0);
                }
                Assert.Equal(reference, got);
            }
        }

        [Fact]
        public void RandomNumbersAreFixedForever()
        {
            // If this test fails, the generator changed, and every shared seed in the world just broke. Do not "fix" the numbers.
            var expected = new Dictionary<ulong, uint[]>
            {
                [0] = new uint[] { 3894649422u, 2055130073u, 2315086854u, 2925816488u, 3443325253u },
                [1] = new uint[] { 1412771199u, 1791099446u, 124312908u, 1968572995u, 1080415314u },
                [12345] = new uint[] { 1411482639u, 3165192603u, 3360792183u, 2433038347u, 628889468u },
            };
            foreach (var kv in expected)
            {
                var r = Rng.FromSeed(kv.Key);
                foreach (var v in kv.Value) Assert.Equal(v, r.NextUInt());
            }
        }

        [Fact]
        public void RandomStateIsPartOfSaveAndRestore()
        {
            var sim = Simulation.Start(Repo.Map(), Repo.Lucca(), Repo.World(), 20, 0, 99, 100);
            var saved = sim.Save();
            var a = new[] { sim.State.RunRng.NextUInt(), sim.State.BattleRng.NextUInt() };
            sim.Restore(saved);
            var b = new[] { sim.State.RunRng.NextUInt(), sim.State.BattleRng.NextUInt() };
            Assert.Equal(a, b);
        }

        [Fact]
        public void SeedsRoundTripAsText()
        {
            var sim = Simulation.Start(Repo.Map(), Repo.Lucca(), Repo.World(), 20, 0, 0x75BCD15UL, ulong.MaxValue);
            Assert.True(SimState.TryParseSeed(sim.State.RunSeedText, out var run));
            Assert.True(SimState.TryParseSeed(sim.State.BattleSeedText, out var battle));
            Assert.Equal(0x75BCD15UL, run);
            Assert.Equal(ulong.MaxValue, battle);
        }

        [Fact]
        public void BelowIsInRange()
        {
            var r = Rng.FromSeed(7);
            for (int i = 0; i < 10000; i++) Assert.InRange(r.Below(6), 0, 5);
        }
    }
}
