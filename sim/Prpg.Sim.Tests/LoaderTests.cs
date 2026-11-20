using System;
using Prpg.Sim;
using Xunit;

namespace Prpg.Sim.Tests
{
    public class LoaderTests
    {
        [Fact]
        public void MapIgnoresFieldsItDoesNotKnow()
        {
            var m = Map.Load(@"{""width"": 10, ""areas"": [{""id"": 1}], ""monsters"": null, ""ground"": [[0, 0, 10, 0]], ""climbables"": []}");
            Assert.Single(m.Lines);
            Assert.Equal(10, m.Width);
        }

        [Fact]
        public void MapAcceptsFractionalHeights()
        {
            var m = Map.Load(@"{""width"": 10, ""scaffolds"": [[0, 1.66, 4, 1.66]], ""climbables"": [{""kind"": ""Rope"", ""x"": 2.5, ""y1"": 0, ""y2"": 1.66, ""colour"": ""red""}]}");
            Assert.Equal(1.66, m.Lines[0].Y1);
            Assert.Equal(ClimbKind.Rope, m.Climbables[0].Kind);
        }

        [Fact]
        public void MovementMustHaveEveryNumber()
        {
            Assert.Throws<FormatException>(() => MovementFile.Load(@"{""characters"": {""Lucca"": {""jumpHeight"": 4}}}", "Lucca"));
        }

        [Fact]
        public void MovementSavesAndLoadsWithoutLoss()
        {
            var text = Repo.Read("data", "movement.json");
            var m = MovementFile.Load(text, "Lucca");
            m.JumpHeight = 4.25; m.HardCut = true; m.WalkSpeed = 1.0 / 3;
            var saved = MovementFile.Save(text, "Lucca", m);
            var back = MovementFile.Load(saved, "Lucca");
            foreach (var n in Movement.Names) Assert.True(m.Get(n) == back.Get(n), n);
            Assert.Contains("\"about\"", saved);       // the rest of the file is kept
        }

        [Fact]
        public void SavingUnchangedNumbersKeepsTheFileAsItWas()
        {
            var text = Repo.Read("data", "movement.json");
            Assert.Equal(text.Replace("\r\n", "\n"), MovementFile.Save(text, "Lucca", MovementFile.Load(text, "Lucca")));
        }
    }
}
