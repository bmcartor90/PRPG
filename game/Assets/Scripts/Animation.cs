using System;
using Prpg.Sim;

namespace Prpg.Game
{
    /// <summary>
    /// Drawing-only bookkeeping, updated once per simulation tick, as the lab's tick() does after step():
    /// which way he faces, how far he has travelled on foot, and landing, slide and lunge timers.
    /// Never feeds back into the simulation.
    /// </summary>
    public sealed class AnimState
    {
        public int Face = 1;
        public double Dist;
        public int Land, Slide, LungeT;
        bool wasOnGround = true;

        public void AfterTick(PlayerState p, Movement P, Map map, int inputDir)
        {
            bool slidingNow = p.OnGround && Mover.IsSteep(map.Lines[p.Line], P);
            LungeT = p.Lunge != 0 ? LungeT + 1 : 0;
            if (p.Mode == Mode.Hang || p.Mode == Mode.Pull) { Face = -p.HangSide; Slide = 0; }
            else if (p.Mode == Mode.Climb && map.Climbables[p.Climb].Face != 0) { Face = -map.Climbables[p.Climb].Face; Slide = 0; }   // on a cliff face he faces the rock
            else if (p.Lunge != 0) { Face = p.Lunge; Slide = 0; }
            else if (slidingNow) { if (Math.Abs(p.Vx) > 0.3) Face = Math.Sign(p.Vx); Slide++; }
            else { Slide = 0; if (inputDir != 0) Face = inputDir; }
            if (p.OnGround) Dist += Math.Abs(p.Vx) * Mover.DT;
            if (!wasOnGround && p.OnGround) Land = 7; else if (Land > 0) Land--;
            wasOnGround = p.OnGround;
        }

        public void Reset() { Dist = 0; Land = Slide = LungeT = 0; wasOnGround = true; }
    }

    /// <summary>The lab's pickFrame: which frame to show for the current state. Port of its choices, in its order.</summary>
    public static class FramePicker
    {
        const double WalkCycle = 2.4, RunCycle = 4.2;   // tiles travelled per full loop of the walk and run frames

        /// <summary>Progress of a draw or sheathe as how far out the item is: 0 stowed, 1 in hand.</summary>
        public static double SwapProgress(PlayerState p)
        {
            double done = 1 - p.SwapT / p.SwapTotal;
            return p.Held(p.SwapHand) ? 1 - done : done;
        }

        /// <param name="flip">0, or +1 / -1 when the frame must be drawn that way regardless of facing (climbing seen from behind).</param>
        public static Frame Pick(PlayerState p, Movement P, Map map, CharacterAtlas A, AnimState anim, long frameCount, out int flip)
        {
            flip = 0;
            Frame[] F(string name) => A.Get(name);
            Frame At(string name, int i) { var a = A.Get(name); return a[Math.Min(i, a.Length - 1)]; }

            if (p.Prone > 0)
            {
                double left = p.Prone / P.ProneTime;
                return left > 0.45 ? At("prone", 0) : left > 0.18 ? At("getup", 0) : At("getup", 1);
            }
            if (p.Lunge != 0) return At("lunge", anim.LungeT < 5 ? 0 : 1);
            if (p.Mode == Mode.Hang) return Mover.FreeHands(p) == 1 ? At("hang1", 0) : At("hang", (int)(frameCount / 45 % 2));
            if (p.Mode == Mode.Pull)
            {
                double t = p.HangProg;
                return At("pull", t < 0.3 ? 0 : t < 0.55 ? 1 : t < 0.85 ? 2 : 3);
            }
            if (p.Mode == Mode.Climb)
            {
                var C = map.Climbables[p.Climb];
                int f = Mover.FreeHands(p);
                bool moving = Math.Abs(p.Y - p.Py) > 1e-4;
                int i = (((int)Math.Floor(p.Y / 0.4) % 4) + 4) % 4;
                if (C.Kind == ClimbKind.Rope || C.Face != 0)
                {
                    // Side view, pinned by his lower hand. Every climbable on a cliff face uses these until it has its own frames.
                    if (p.Py - p.Y > P.ClimbSpeed * Mover.DT * 1.5) return At("ropeSlide", 0);
                    if (!moving) return f == 1 ? At("rope1", 0) : At("ropeHang", 0);
                    if (f == 1) return i % 2 != 0 ? At("rope1", 0) : At("ropeHang", 0);
                    return At("rope", i);
                }
                if (C.Kind == ClimbKind.Ladder)
                {
                    if (!moving) return f == 2 ? At("ladderRest", 0) : At("ladder", 1);
                    flip = i == 2 ? -1 : 1;
                    return At("ladder", i % 2 != 0 ? 1 : 0);
                }
                if (!moving) return At("wallHang", 0);
                flip = i >= 2 ? -1 : 1;
                return At("wall", i % 2);
            }
            if (p.Exhaust > 0 && p.OnGround) return At("getup", 0);
            if (p.Duck > 0) { double u = 1 - p.Duck / P.DuckTime; return At("slide", u < 0.2 ? 0 : u < 0.8 ? 1 : 2); }
            if (p.Low && Math.Abs(p.Vx) > 0.3) return At("getup", 0);   // crawling: on hands and knees, until he has crawl frames
            if (p.Crouch || p.Low) return At("crouch", 0);
            if (!p.OnGround)
            {
                double r = p.Vy / Mover.Derive(P).V0;      // +1 at takeoff, 0 at the apex, negative while falling
                if (r > 0.55) return At("jump", 1);
                if (r > 0.15) return At("jump", 2);
                if (r > -0.15) return At("jump", 3);
                if (r > -0.6) return At("jump", 4);
                return At("jump", 5);
            }
            if (Mover.IsSteep(map.Lines[p.Line], P)) return At("slide", Math.Min(2, anim.Slide / 5));
            double sp = Math.Abs(p.Vx);
            bool running = sp > P.WalkSpeed + 0.25 * (P.RunSpeed - P.WalkSpeed);
            // Drawing or sheathing is animated only for the arm nearer the viewer: his right (sword) facing right,
            // his left (shield) facing left. Sheathing is the draw played backwards.
            if (p.SwapHand >= 0 && p.SwapHand == (anim.Face > 0 ? 1 : 0))
            {
                bool sword = p.SwapHand == 1;
                double u = SwapProgress(p);
                var set = sp < 0.4 ? (sword ? F("drawSword") : F("nearShield"))
                        : running ? (sword ? F("runDrawSword") : F("runDrawShield"))
                        : (sword ? F("walkDrawSword") : F("walkDrawShield"));
                if (set != null) return set[Math.Min(set.Length - 1, (int)Math.Floor(u * set.Length))];   // no moving-draw frames: keep walking
            }
            if (anim.Land > 0 && sp < 2) return At("jump", 6);     // brief landing crouch, only when landing nearly still
            if (sp < 0.4) return At("walk", 0);
            if (running) return At("run", (int)Math.Floor(anim.Dist / RunCycle * 6) % 6);
            return At("walk", 1 + (int)Math.Floor(anim.Dist / WalkCycle * 4) % 4);
        }
    }
}
