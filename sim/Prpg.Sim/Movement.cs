using System;
using System.Collections.Generic;

namespace Prpg.Sim
{
    /// <summary>
    /// One character's movement numbers. Every tuning number the simulation uses lives here and comes from
    /// data/movement.json. Units are tiles and seconds. Field names match the JSON keys and the feel lab.
    /// </summary>
    public sealed class Movement
    {
        public double JumpHeight;
        public double ApexTime;
        public double FallMult;
        public double CutMult;
        public double Terminal;
        public double WalkSpeed;
        public double RunSpeed;
        public double AccelTime;
        public double StopTime;
        public double TurnTime;
        public double AirControl;
        public double CoyoteMs;
        public double BufferMs;
        public bool HardCut;
        public double WalkLimit;
        public double UpCost;
        public double DownBoost;
        public double SlideFriction;
        public double SlideSteer;
        public double SlideJump;
        public double FootWidth;
        public double BodyWidth;
        public double StaminaMax;
        public double JumpCost;
        public double RunDrain;
        public double WalkRegen;
        public double CrouchRegen;
        public double ExhaustTime;
        public double CharHeight;
        public double ArmReach;
        public double LungeLean;
        public double LungeCost;
        public double ProneTime;
        public double PullTime;
        public double PullCost;
        public double ClimbSpeed;
        public double Weight;
        public double ClimbEffort;
        public double HangCost;
        public double MultLadder;
        public double MultRope;
        public double MultWall;
        public double OneHand;
        public double DrawTime;
        public double ClimbSprint;
        public double ClimbSprintCost;
        public double DuckTime;
        public double DuckKeep;
        public double DuckCost;
        public double DuckHeight;

        // Fixed numbers: no slider in the lab, but still per-character tuning (the lab's BODY table).
        public double CatchOut;   // tiles outside a ledge end that still catch it
        public double CatchUnder;   // tiles under a ledge (inside its end) that still catch it
        public double CatchMinDrop;   // the ledge must be at least this far above his feet to be caught
        public double HangOut;   // where he hangs: this far outside the ledge end
        public double HangIn;   // where a pull-up puts him: this far in from the end
        public double HangEase;   // share of the remaining distance he closes each tick while settling into the hang
        public double ClimbReach;   // tiles either side of a climbable that still catch it, and of a platform at its top or bottom
        public double ClimbJumpSpeed;   // sprint-jumping off a climbable leaves at this share of run speed
        public double SlideDownSpeed;   // sliding down a ladder or rope is this many times climb speed
        public double DropGrabLock;   // seconds after letting go before he can grab again
        public double JumpGrabLock;   // seconds after jumping off a climbable before he can grab again
        public double WindedUntil;   // after exhaustion he cannot sprint until this share of the stamina bar is back
        public double SprintShare;   // he counts as sprinting above walk speed plus this share of the gap up to run speed
        public double DuckStopShare;   // a dodge slide ends early below this share of walk speed
        public double RunDrainAbove;   // running drains stamina only above walk speed plus this many tiles a second
        public double CrouchBelow;   // he can crouch only when moving slower than this, in tiles a second
        public double MinUphillShare;   // however steep the climb, top speed never drops below this share of normal
        public double PullSlideFrom;   // a pull-up lifts him straight up until this share of it is done, then also carries him in over the ledge

        /// <summary>The numbers the lab has sliders for, in file order.</summary>
        public static readonly string[] SliderNames = {
            "jumpHeight", "apexTime", "fallMult", "cutMult", "terminal", "walkSpeed", "runSpeed",
            "accelTime", "stopTime", "turnTime", "airControl", "coyoteMs", "bufferMs", "hardCut",
            "walkLimit", "upCost", "downBoost", "slideFriction", "slideSteer", "slideJump", "footWidth",
            "bodyWidth", "staminaMax", "jumpCost", "runDrain", "walkRegen", "crouchRegen", "exhaustTime",
            "charHeight", "armReach", "lungeLean", "lungeCost", "proneTime", "pullTime", "pullCost",
            "climbSpeed", "weight", "climbEffort", "hangCost", "multLadder", "multRope", "multWall",
            "oneHand", "drawTime", "climbSprint", "climbSprintCost", "duckTime", "duckKeep", "duckCost",
            "duckHeight"
        };
        /// <summary>Per-character numbers with no slider in the lab (its BODY table).</summary>
        public static readonly string[] FixedNames = {
            "catchOut", "catchUnder", "catchMinDrop", "hangOut", "hangIn", "hangEase", "climbReach",
            "climbJumpSpeed", "slideDownSpeed", "dropGrabLock", "jumpGrabLock", "windedUntil", "sprintShare", "duckStopShare",
            "runDrainAbove", "crouchBelow", "minUphillShare", "pullSlideFrom"
        };
        /// <summary>Every parameter name, in the order the file and the tuning panel list them.</summary>
        public static readonly string[] Names = Concat(SliderNames, FixedNames);
        static string[] Concat(string[] a, string[] b) { var r = new string[a.Length + b.Length]; a.CopyTo(r, 0); b.CopyTo(r, a.Length); return r; }
        public static bool IsSwitch(string name) => Array.IndexOf(new[] { "hardCut" }, name) >= 0;

        public double Get(string name)
        {
            switch (name)
            {
                case "jumpHeight": return JumpHeight;
                case "apexTime": return ApexTime;
                case "fallMult": return FallMult;
                case "cutMult": return CutMult;
                case "terminal": return Terminal;
                case "walkSpeed": return WalkSpeed;
                case "runSpeed": return RunSpeed;
                case "accelTime": return AccelTime;
                case "stopTime": return StopTime;
                case "turnTime": return TurnTime;
                case "airControl": return AirControl;
                case "coyoteMs": return CoyoteMs;
                case "bufferMs": return BufferMs;
                case "hardCut": return HardCut ? 1 : 0;
                case "walkLimit": return WalkLimit;
                case "upCost": return UpCost;
                case "downBoost": return DownBoost;
                case "slideFriction": return SlideFriction;
                case "slideSteer": return SlideSteer;
                case "slideJump": return SlideJump;
                case "footWidth": return FootWidth;
                case "bodyWidth": return BodyWidth;
                case "staminaMax": return StaminaMax;
                case "jumpCost": return JumpCost;
                case "runDrain": return RunDrain;
                case "walkRegen": return WalkRegen;
                case "crouchRegen": return CrouchRegen;
                case "exhaustTime": return ExhaustTime;
                case "charHeight": return CharHeight;
                case "armReach": return ArmReach;
                case "lungeLean": return LungeLean;
                case "lungeCost": return LungeCost;
                case "proneTime": return ProneTime;
                case "pullTime": return PullTime;
                case "pullCost": return PullCost;
                case "climbSpeed": return ClimbSpeed;
                case "weight": return Weight;
                case "climbEffort": return ClimbEffort;
                case "hangCost": return HangCost;
                case "multLadder": return MultLadder;
                case "multRope": return MultRope;
                case "multWall": return MultWall;
                case "oneHand": return OneHand;
                case "drawTime": return DrawTime;
                case "climbSprint": return ClimbSprint;
                case "climbSprintCost": return ClimbSprintCost;
                case "duckTime": return DuckTime;
                case "duckKeep": return DuckKeep;
                case "duckCost": return DuckCost;
                case "duckHeight": return DuckHeight;
                case "catchOut": return CatchOut;
                case "catchUnder": return CatchUnder;
                case "catchMinDrop": return CatchMinDrop;
                case "hangOut": return HangOut;
                case "hangIn": return HangIn;
                case "hangEase": return HangEase;
                case "climbReach": return ClimbReach;
                case "climbJumpSpeed": return ClimbJumpSpeed;
                case "slideDownSpeed": return SlideDownSpeed;
                case "dropGrabLock": return DropGrabLock;
                case "jumpGrabLock": return JumpGrabLock;
                case "windedUntil": return WindedUntil;
                case "sprintShare": return SprintShare;
                case "duckStopShare": return DuckStopShare;
                case "runDrainAbove": return RunDrainAbove;
                case "crouchBelow": return CrouchBelow;
                case "minUphillShare": return MinUphillShare;
                case "pullSlideFrom": return PullSlideFrom;
                default: throw new ArgumentException("Unknown movement parameter " + name);
            }
        }

        public bool Set(string name, double v)
        {
            switch (name)
            {
                case "jumpHeight": JumpHeight = v; return true;
                case "apexTime": ApexTime = v; return true;
                case "fallMult": FallMult = v; return true;
                case "cutMult": CutMult = v; return true;
                case "terminal": Terminal = v; return true;
                case "walkSpeed": WalkSpeed = v; return true;
                case "runSpeed": RunSpeed = v; return true;
                case "accelTime": AccelTime = v; return true;
                case "stopTime": StopTime = v; return true;
                case "turnTime": TurnTime = v; return true;
                case "airControl": AirControl = v; return true;
                case "coyoteMs": CoyoteMs = v; return true;
                case "bufferMs": BufferMs = v; return true;
                case "hardCut": HardCut = v != 0; return true;
                case "walkLimit": WalkLimit = v; return true;
                case "upCost": UpCost = v; return true;
                case "downBoost": DownBoost = v; return true;
                case "slideFriction": SlideFriction = v; return true;
                case "slideSteer": SlideSteer = v; return true;
                case "slideJump": SlideJump = v; return true;
                case "footWidth": FootWidth = v; return true;
                case "bodyWidth": BodyWidth = v; return true;
                case "staminaMax": StaminaMax = v; return true;
                case "jumpCost": JumpCost = v; return true;
                case "runDrain": RunDrain = v; return true;
                case "walkRegen": WalkRegen = v; return true;
                case "crouchRegen": CrouchRegen = v; return true;
                case "exhaustTime": ExhaustTime = v; return true;
                case "charHeight": CharHeight = v; return true;
                case "armReach": ArmReach = v; return true;
                case "lungeLean": LungeLean = v; return true;
                case "lungeCost": LungeCost = v; return true;
                case "proneTime": ProneTime = v; return true;
                case "pullTime": PullTime = v; return true;
                case "pullCost": PullCost = v; return true;
                case "climbSpeed": ClimbSpeed = v; return true;
                case "weight": Weight = v; return true;
                case "climbEffort": ClimbEffort = v; return true;
                case "hangCost": HangCost = v; return true;
                case "multLadder": MultLadder = v; return true;
                case "multRope": MultRope = v; return true;
                case "multWall": MultWall = v; return true;
                case "oneHand": OneHand = v; return true;
                case "drawTime": DrawTime = v; return true;
                case "climbSprint": ClimbSprint = v; return true;
                case "climbSprintCost": ClimbSprintCost = v; return true;
                case "duckTime": DuckTime = v; return true;
                case "duckKeep": DuckKeep = v; return true;
                case "duckCost": DuckCost = v; return true;
                case "duckHeight": DuckHeight = v; return true;
                case "catchOut": CatchOut = v; return true;
                case "catchUnder": CatchUnder = v; return true;
                case "catchMinDrop": CatchMinDrop = v; return true;
                case "hangOut": HangOut = v; return true;
                case "hangIn": HangIn = v; return true;
                case "hangEase": HangEase = v; return true;
                case "climbReach": ClimbReach = v; return true;
                case "climbJumpSpeed": ClimbJumpSpeed = v; return true;
                case "slideDownSpeed": SlideDownSpeed = v; return true;
                case "dropGrabLock": DropGrabLock = v; return true;
                case "jumpGrabLock": JumpGrabLock = v; return true;
                case "windedUntil": WindedUntil = v; return true;
                case "sprintShare": SprintShare = v; return true;
                case "duckStopShare": DuckStopShare = v; return true;
                case "runDrainAbove": RunDrainAbove = v; return true;
                case "crouchBelow": CrouchBelow = v; return true;
                case "minUphillShare": MinUphillShare = v; return true;
                case "pullSlideFrom": PullSlideFrom = v; return true;
                default: return false;
            }
        }

        public double Mult(ClimbKind kind)
        {
            switch (kind)
            {
                case ClimbKind.Ladder: return MultLadder;
                case ClimbKind.Rope: return MultRope;
                default: return MultWall;
            }
        }

        public Movement Clone() => (Movement)MemberwiseClone();

        /// <summary>Reads one character's numbers. Every known parameter must be present; unknown keys are ignored.</summary>
        public static Movement FromJson(JsonObject o)
        {
            var m = new Movement();
            var missing = new List<string>();
            foreach (var n in Names)
            {
                if (!o.Has(n)) { missing.Add(n); continue; }
                m.Set(n, o.Number(n));
            }
            if (missing.Count > 0) throw new FormatException("Movement is missing: " + string.Join(", ", missing));
            return m;
        }

        public JsonObject ToJson()
        {
            var o = new JsonObject();
            foreach (var n in Names) o.Set(n, IsSwitch(n) ? (object)(Get(n) != 0) : Get(n));
            return o;
        }
    }

    /// <summary>The movement file: one entry per character.</summary>
    public static class MovementFile
    {
        public static Movement Load(string json, string character)
        {
            var root = Json.Parse(json) as JsonObject;
            var chars = root?.Object("characters") ?? throw new FormatException("movement.json needs a \"characters\" object");
            var c = chars.Object(character) ?? throw new FormatException("movement.json has no character \"" + character + "\"");
            return Movement.FromJson(c);
        }

        /// <summary>Replaces one character's numbers in the file text, keeping the other characters as they were.</summary>
        public static string Save(string existingJson, string character, Movement m)
        {
            var root = (string.IsNullOrWhiteSpace(existingJson) ? null : Json.Parse(existingJson) as JsonObject) ?? new JsonObject();
            var chars = root.Object("characters");
            if (chars == null) { chars = new JsonObject(); root.Set("characters", chars); }
            chars.Set(character, m.ToJson());
            return Json.Write(root, 2) + "\n";
        }
    }
}
