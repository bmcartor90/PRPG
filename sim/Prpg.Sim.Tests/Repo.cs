using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prpg.Sim;

namespace Prpg.Sim.Tests
{
    static class Repo
    {
        public static readonly string Root = FindRoot();

        static string FindRoot()
        {
            var d = new DirectoryInfo(AppContext.BaseDirectory);
            while (d != null && !Directory.Exists(Path.Combine(d.FullName, "feel-lab", "traces"))) d = d.Parent;
            if (d == null) throw new DirectoryNotFoundException("Could not find the repository root above " + AppContext.BaseDirectory);
            return d.FullName;
        }

        public static string Read(params string[] parts) => File.ReadAllText(Path.Combine(new[] { Root }.Concat(parts).ToArray()));

        public static Map Map() => Sim.Map.Load(Read("data", "maps", "test.json"), World());
        public static Movement Lucca() => MovementFile.Load(Read("data", "movement.json"), "Lucca");
        public static WorldRules World() => WorldRules.Load(Read("data", "world.json"));

        public static IEnumerable<string> TraceNames() =>
            Directory.GetFiles(Path.Combine(Root, "feel-lab", "traces"), "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(n => !n.StartsWith("_"))
                .OrderBy(n => n, StringComparer.Ordinal);
    }

    /// <summary>A recorded run of the feel lab: where he starts, the inputs each tick, and the state after each tick.</summary>
    sealed class Trace
    {
        public string Name;
        public double StartX;
        public List<Input> Inputs = new List<Input>();
        public List<object[]> States = new List<object[]>();

        public static Trace Load(string name)
        {
            var o = (JsonObject)Json.Parse(Repo.Read("feel-lab", "traces", name + ".json"));
            var t = new Trace { Name = name, StartX = o.Number("startX"), StartY = o.Number("startY") };
            // Rebuild per-tick input exactly as make-traces.js does.
            bool prevJump = false;
            foreach (JsonObject seg in o.Array("inputs"))
            {
                int ticks = (int)seg.Number("ticks");
                bool jump = seg.Get("jump") is bool j && j;
                for (int i = 0; i < ticks; i++)
                {
                    var hand = HandKey.None;
                    if (i == 0 && seg.Has("hand")) hand = (HandKey)(int)seg.Number("hand");
                    t.Inputs.Add(new Input
                    {
                        Dir = seg.Has("dir") ? (int)seg.Number("dir") : 0,
                        Run = seg.Get("run") is bool r && r,
                        JumpHeld = jump,
                        JumpPressed = jump && !prevJump,
                        Up = seg.Get("up") is bool u && u,
                        Down = seg.Get("down") is bool dn && dn,
                        Grab = seg.Get("grab") is bool g && g,
                        Hand = hand,
                    });
                    prevJump = jump;
                }
            }
            foreach (List<object> row in o.Array("states")) t.States.Add(row.ToArray());
            return t;
        }

        public double StartY;
        public Simulation Start(Map map, Movement m) => Simulation.Start(map, m, Repo.World(), StartX, Simulation.GroundLineAt(map, StartX), 12345, 67890);
    }
}
