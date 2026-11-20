using System;
using System.Collections.Generic;

namespace Prpg.Sim
{
    /// <summary>
    /// The movement simulation, ported from the feel lab (feel-lab/index.html, between SIM-START and SIM-END).
    /// Units are tiles and seconds; y is up. One call to Step advances one tick of 1/60 s.
    /// Arithmetic is written in the same order as the lab so results match it to the last bit where possible.
    /// </summary>
    public static class Mover
    {
        public const double DT = 1.0 / 60;

        // Tuning numbers come from Movement (per character) and WorldRules (per game). The only constants left are
        // geometry, and RADIUS, which only feeds the lab's stand-in circle drawing (Lift).
        const double RADIUS = 0.45;
        static readonly double SIN45 = Math.Sqrt(0.5); // JavaScript's Math.SQRT1_2

        public struct Derived { public double G, V0, GFall, Air, ReachWalk, ReachRun; }

        public static Derived Derive(Movement P)
        {
            double g = 2 * P.JumpHeight / (P.ApexTime * P.ApexTime);
            double v0 = 2 * P.JumpHeight / P.ApexTime;
            double gFall = g * P.FallMult;
            double tT = P.Terminal / gFall, dT = P.Terminal * P.Terminal / (2 * gFall);
            double fallTime = dT < P.JumpHeight ? tT + (P.JumpHeight - dT) / P.Terminal : Math.Sqrt(2 * P.JumpHeight / gFall);
            double air = P.ApexTime + fallTime;
            return new Derived { G = g, V0 = v0, GFall = gFall, Air = air, ReachWalk = P.WalkSpeed * air, ReachRun = P.RunSpeed * air };
        }

        public static bool IsSteep(Line L, Movement P) => L.Deg > P.WalkLimit + 1e-6;

        static double Toward(double v, double target, double stepSize)
            => v < target ? Math.Min(v + stepSize, target) : Math.Max(v - stepSize, target);

        // JavaScript's Math.sign for a double: keeps the comparison semantics the lab relies on.
        static double JsSign(double v) => v > 0 ? 1 : v < 0 ? -1 : v;

        // Accelerate, coast or skid toward the speed the input asks for.
        static double Drive(double v, int dir, double top, Movement P, double ctrl)
        {
            double accel = P.RunSpeed / P.AccelTime * ctrl;
            double brake = P.RunSpeed / P.StopTime * ctrl;
            double skid = P.RunSpeed / P.TurnTime * ctrl;
            if (dir == 0) return Toward(v, 0, brake * DT);
            double target = dir * top;
            if (v != 0 && JsSign(v) != dir) return Toward(v, target, skid * DT);
            if (Math.Abs(v) > top) return Toward(v, target, brake * DT);
            return Toward(v, target, accel * DT);
        }

        static void Respawn(PlayerState p, Map map, double x, int line)
        {
            var L = map.Lines[line];
            p.X = p.Px = x; p.Y = p.Py = L.YAt(x); p.Vx = 0; p.Vy = 0;
            p.OnGround = true; p.Line = line; p.Jumping = false; p.Cutting = false; p.Coyote = 0; p.Buffer = 0;
            p.Stamina = double.NaN; p.Exhaust = 0; p.Winded = false; p.Spent = false; p.Crouch = false; p.Mode = Mode.Normal;
            p.HangLine = -1; p.Climb = -1; p.GrabLock = 0; p.Lunge = 0; p.Prone = 0; p.SwapHand = -1; p.Duck = 0; p.DownWas = false;
        }

        // ---- Hands ----

        /// <summary>Hands that hold nothing and are not in the middle of a draw or sheathe.</summary>
        public static int FreeHands(PlayerState p)
        {
            int n = 0;
            for (int i = 0; i < 2; i++) if (!p.Held(i) && p.SwapHand != i) n++;
            return n;
        }

        static int HandsNeeded(Climbable C) => C.Kind == ClimbKind.Wall ? 2 : C.Kind == ClimbKind.Rope ? 1 : 0;

        // ---- Stamina, ledge grabs and climbing ----

        static void LetGo(PlayerState p, Movement P)
        {
            p.Mode = Mode.Normal; p.HangLine = -1; p.Climb = -1; p.OnGround = false; p.Line = -1;
            p.Vx = 0; p.Vy = 0; p.Coyote = 0; p.Jumping = false; p.Cutting = false; p.GrabLock = P.DropGrabLock;
        }

        // The bar hit zero: down for a moment, and drop off anything held.
        static void Tire(PlayerState p, Movement P)
        {
            p.Stamina = 0; p.Exhaust = P.ExhaustTime; p.Winded = true;
            if (p.Mode != Mode.Normal) LetGo(p, P);
        }

        static bool EndIsOpen(List<Line> lines, Line M, int side, Movement P, WorldRules W)
        {
            double ex = side < 0 ? M.X1 : M.X2, ey = side < 0 ? M.Y1 : M.Y2;
            foreach (var L in lines)
            {
                if (L == M) continue;
                bool at1 = Math.Abs(L.X1 - ex) < W.Join && Math.Abs(L.Y1 - ey) < W.Join;
                bool at2 = Math.Abs(L.X2 - ex) < W.Join && Math.Abs(L.Y2 - ey) < W.Join;
                if (!at1 && !at2) continue;
                if (IsSteep(L, P) && (at1 ? L.Y2 : L.Y1) < ey - W.Join) continue;
                return false;
            }
            return true;
        }

        /// <summary>How tall he is right now. Nothing checks this yet; hazards and enemies will.</summary>
        public static double HeightNow(PlayerState p, Movement P) => p.Duck > 0 ? P.DuckHeight : P.CharHeight;

        static double ReachOf(Movement P) => P.CharHeight + P.ArmReach;

        struct Ledge { public Line Line; public int Side; public double Ex, Ey; }

        static readonly int[] Sides = { -1, 1 };

        static bool FindLedge(PlayerState p, Movement P, WorldRules W, List<Line> lines, double y, out Ledge found)
        {
            found = default;
            if (FreeHands(p) < 1 || p.GrabLock > 0 || p.Stamina <= 0) return false;
            double R = ReachOf(P);
            foreach (var M in lines)
            {
                if (IsSteep(M, P)) continue;
                foreach (int side in Sides)
                {
                    double ex = side < 0 ? M.X1 : M.X2, ey = side < 0 ? M.Y1 : M.Y2;
                    double dy = ey - y;
                    if (dy < P.CatchMinDrop || dy > R) continue;
                    double outside = (p.X - ex) * side;
                    bool ok = outside >= -P.CatchUnder && outside <= P.CatchOut;
                    if (!ok && p.Lunge != 0 && side == -p.Lunge)
                    {
                        double fwd = (ex - p.X) * p.Lunge, dist = Hypot(fwd, dy);
                        ok = fwd >= 0 && dist <= R && fwd / dist <= P.LungeLean;
                    }
                    if (ok && EndIsOpen(lines, M, side, P, W))
                    {
                        found = new Ledge { Line = M, Side = side, Ex = ex, Ey = ey };
                        return true;
                    }
                }
            }
            return false;
        }

        // Math.hypot as V8 computes it for two finite arguments: scale by the larger magnitude, then sqrt.
        static double Hypot(double a, double b)
        {
            a = Math.Abs(a); b = Math.Abs(b);
            double max = Math.Max(a, b);
            if (max == 0) return 0;
            double sa = a / max, sb = b / max;
            // V8 sums the squares with Kahan compensation; for two terms that reduces to this order.
            double sum = 0, comp = 0;
            foreach (double s in new[] { sa, sb })
            {
                double summand = s * s - comp;
                double prelim = sum + summand;
                comp = (prelim - sum) - summand;
                sum = prelim;
            }
            return Math.Sqrt(sum) * max;
        }

        static void StartHang(PlayerState p, Ledge g, double y)
        {
            p.Mode = Mode.Hang; p.HangLine = g.Line.Index; p.HangSide = g.Side; p.HangProg = 0; p.HangY0 = y; p.Lunge = 0;
            p.Y = y; p.Vx = 0; p.Vy = 0; p.OnGround = false; p.Line = -1;
            p.Jumping = false; p.Cutting = false; p.Coyote = 0;
        }

        static void HoldStep(PlayerState p, Movement P, WorldRules W, Input inp, Map map)
        {
            var lines = map.Lines;
            double effort = P.ClimbEffort * P.Weight;
            if (p.Mode == Mode.Hang || p.Mode == Mode.Pull)
            {
                var L = lines[p.HangLine];
                int side = p.HangSide;
                double ex = side < 0 ? L.X1 : L.X2, ey = side < 0 ? L.Y1 : L.Y2;
                int f = FreeHands(p);
                if (!inp.Grab || inp.Down || f < 1) { LetGo(p, P); return; }
                if (p.Mode == Mode.Hang)
                {
                    p.X += (ex + side * P.HangOut - p.X) * P.HangEase;
                    p.Stamina -= P.HangCost * P.Weight * DT;
                    if (inp.JumpPressed || inp.Up) p.Mode = Mode.Pull;
                }
                if (p.Mode == Mode.Pull)
                {
                    p.HangProg += DT / P.PullTime * (f == 1 ? P.OneHand : 1);
                    p.Stamina -= P.PullCost * P.Weight * DT;
                    double t = Math.Min(1, p.HangProg);
                    p.Y = p.HangY0 + (ey - p.HangY0) * t;
                    p.X = ex + side * P.HangOut - side * (P.HangOut + P.HangIn) * Math.Max(0, (t - P.PullSlideFrom) / (1 - P.PullSlideFrom));
                    if (t >= 1)
                    {
                        p.Mode = Mode.Normal; p.HangLine = -1; p.OnGround = true; p.Line = L.Index;
                        p.X = ex - side * P.HangIn; p.Y = L.YAt(p.X); p.Vx = 0; p.Vy = 0;
                        return;
                    }
                }
                if (p.Stamina <= 0) Tire(p, P);
                return;
            }

            // On a climbable.
            var C = map.Climbables[p.Climb];
            int mv = (inp.Up ? 1 : 0) - (inp.Down ? 1 : 0);
            int fh = FreeHands(p);
            bool ladder = C.Kind == ClimbKind.Ladder;
            if (fh < HandsNeeded(C)) { LetGo(p, P); return; }
            if (inp.JumpPressed)
            {
                // Jump always jumps, at full height and full cost: away if a direction is held, straight up if not.
                // Down plus jump just lets go.
                var d = Derive(P);
                LetGo(p, P);
                if (!inp.Down)
                {
                    p.Vx = inp.Dir * (inp.Run ? P.RunSpeed * P.ClimbJumpSpeed : P.WalkSpeed); p.Vy = d.V0; p.Jumping = true; p.GrabLock = P.JumpGrabLock;
                    p.Stamina -= P.JumpCost;
                    if (p.Stamina <= 0) { p.Stamina = 0; p.Spent = true; }
                }
                return;
            }
            if (!ladder && !inp.Grab) { LetGo(p, P); return; }
            double speed = P.ClimbSpeed;
            if (ladder) speed *= fh == 2 ? 1 : fh == 1 ? (1 + P.OneHand) / 2 : P.OneHand;
            else if (fh == 1) speed *= P.OneHand;
            double mult = P.Mult(C.Kind);
            double cost = (mv != 0 ? effort : P.HangCost * P.Weight) * mult;
            bool canSprint = inp.Run && !p.Winded && (ladder ? fh >= 1 : fh == 2);
            if (canSprint && mv > 0) { speed *= P.ClimbSprint; cost *= P.ClimbSprintCost; }
            if (inp.Run && mv < 0 && C.Kind != ClimbKind.Wall) { speed = P.ClimbSpeed * P.SlideDownSpeed; cost = P.HangCost * P.Weight * mult; }
            p.Y = Math.Min(C.Y2, Math.Max(C.Y1, p.Y + mv * speed * DT));
            if (ladder && mv == 0 && fh == 2)
                p.Stamina = Math.Min(P.StaminaMax, p.Stamina + P.WalkRegen * DT);
            else p.Stamina -= cost * DT;
            if ((mv > 0 && p.Y >= C.Y2) || (mv < 0 && p.Y <= C.Y1))
            {
                // At an end: step onto a platform if one is there.
                foreach (var L in lines)
                {
                    if (IsSteep(L, P) || C.X < L.X1 - P.ClimbReach || C.X > L.X2 + P.ClimbReach) continue;
                    double nx = L.ClampX(C.X);
                    if (Math.Abs(L.YAt(nx) - p.Y) < W.StepOff)
                    {
                        p.Mode = Mode.Normal; p.Climb = -1; p.OnGround = true; p.Line = L.Index; p.X = nx; p.Y = L.YAt(nx); p.Vx = 0; p.Vy = 0;
                        return;
                    }
                }
            }
            if (p.Stamina <= 0) Tire(p, P);
        }

        static void Tail(PlayerState p, Movement P, WorldRules W, Map map)
        {
            double liftTarget = p.OnGround ? RADIUS / map.Lines[p.Line].Cos - RADIUS : 0;
            p.Lift += (liftTarget - p.Lift) * 0.35;
            if (p.Winded && p.Stamina >= P.StaminaMax * P.WindedUntil) p.Winded = false;
            if (p.OnGround && !IsSteep(map.Lines[p.Line], P))
            {
                var L = map.Lines[p.Line];
                p.SafeX = (L.X2 - L.X1 > 2 * W.SafeInset) ? Math.Min(Math.Max(p.X, L.X1 + W.SafeInset), L.X2 - W.SafeInset) : (L.X1 + L.X2) / 2;
                p.SafeLine = L.Index;
            }
            if (p.Y < W.RespawnBelow) Respawn(p, map, p.SafeX, p.SafeLine);
        }

        /// <summary>Advances one tick of 1/60 s.</summary>
        public static void Step(PlayerState p, Movement P, WorldRules W, Input inp, Map map)
        {
            var lines = map.Lines;
            var roofs = map.Undersides;
            double worldW = map.Width;
            var d = Derive(P);
            p.Px = p.X; p.Py = p.Y; p.PLift = p.Lift;
            bool downPressed = inp.Down && !p.DownWas;     // a fresh press of down, not a hold
            p.DownWas = inp.Down;
            if (p.Mode != Mode.Normal || !p.OnGround) p.Duck = 0;
            if (double.IsNaN(p.Stamina) || p.Stamina > P.StaminaMax) p.Stamina = P.StaminaMax;
            if (p.GrabLock > 0) p.GrabLock -= DT;
            // Draw or sheathe: takes a moment, and that hand is useless until it finishes.
            if (p.SwapHand >= 0)
            {
                p.SwapT -= DT;
                if (p.SwapT <= 0) { p.SetHeld(p.SwapHand, !p.Held(p.SwapHand)); p.SwapHand = -1; }
            }
            else if (inp.Hand == HandKey.Left || inp.Hand == HandKey.Right)
            {
                p.SwapHand = (int)inp.Hand; p.SwapT = P.DrawTime; p.SwapTotal = P.DrawTime;
            }
            if (p.Exhaust > 0)
            {
                // Down for a moment: no input counts, and anything held is dropped.
                p.Exhaust -= DT;
                inp = Input.None;
                if (p.Mode != Mode.Normal) LetGo(p, P);
            }
            if (p.Prone > 0)
            {
                // Face down after a missed lunge: nothing pressed counts until up.
                p.Prone -= DT;
                inp = Input.None;
            }
            if (p.Mode != Mode.Normal) { HoldStep(p, P, W, inp, map); Tail(p, P, W, map); return; }

            // Getting onto a climbable.
            if (p.GrabLock <= 0 && p.Exhaust <= 0 && p.Stamina > 0 && (inp.Grab || inp.Up || (inp.Down && p.OnGround)))
            {
                foreach (var C in map.Climbables)
                {
                    if (Math.Abs(p.X - C.X) > P.ClimbReach) continue;
                    bool ladder = C.Kind == ClimbKind.Ladder;
                    if (FreeHands(p) < HandsNeeded(C)) continue;
                    bool within = p.Y >= C.Y1 - W.StepOff && p.Y < C.Y2 - W.Join;
                    bool goOn = within && (inp.Grab || (ladder && inp.Up));
                    bool goDown = inp.Down && p.OnGround && Math.Abs(p.Y - C.Y2) < W.StepOff && (ladder || inp.Grab);
                    if (goOn || goDown)
                    {
                        p.Mode = Mode.Climb; p.Climb = C.Index; p.X = C.X; p.Vx = 0; p.Vy = 0; p.OnGround = false; p.Line = -1;
                        p.Jumping = false; p.Cutting = false; p.Coyote = 0; p.Buffer = 0;
                        Tail(p, P, W, map); return;
                    }
                }
            }
            // Lunge: in the air, or sliding on a slope too steep to stand on, hold grab and a direction and press jump.
            if ((!p.OnGround || IsSteep(lines[p.Line], P)) && p.Lunge == 0 && inp.JumpPressed && inp.Grab && inp.Dir != 0 && p.Coyote <= 0
                && FreeHands(p) >= 1 && p.Stamina > P.LungeCost)
            {
                p.Lunge = inp.Dir; p.Stamina -= P.LungeCost;
                inp.JumpPressed = false;       // that press was the lunge, not a jump
            }
            // On the ground with a ledge end within reach: grab catches it without jumping.
            if (p.OnGround && inp.Grab && p.Exhaust <= 0 && p.Prone <= 0)
            {
                if (FindLedge(p, P, W, lines, p.Y, out var g)) { StartHang(p, g, p.Y); Tail(p, P, W, map); return; }
            }
            if (p.OnGround && p.Lunge != 0) { p.Lunge = 0; p.Prone = P.ProneTime; }

            bool run = inp.Run && !p.Winded;
            double baseTop = run ? P.RunSpeed : P.WalkSpeed;
            double ax = p.X, ay = p.Y;

            if (p.OnGround)
            {
                var L = lines[p.Line];
                double duck0 = p.Duck;
                if (IsSteep(L, P))
                {
                    double pull = d.GFall * L.Sin * (1 - P.SlideFriction);
                    double steer = inp.Dir * P.SlideSteer * P.RunSpeed / P.AccelTime;
                    p.Vx += (-L.Sign * pull + steer) * DT;
                    p.Vx = Math.Max(-P.Terminal, Math.Min(P.Terminal, p.Vx));
                    p.Duck = 0;
                }
                else if (p.Duck > 0)
                {
                    // Dodge slide: low, no steering, bleeding off speed. Ends on its own, when too slow, or with a jump.
                    p.Duck -= DT;
                    p.Vx *= Math.Pow(P.DuckKeep, DT / P.DuckTime);
                    if (p.Duck <= 0 || Math.Abs(p.Vx) < P.DuckStopShare * P.WalkSpeed) p.Duck = 0;
                }
                else if (downPressed && Math.Abs(p.Vx) > P.WalkSpeed + P.SprintShare * (P.RunSpeed - P.WalkSpeed) && p.Stamina > P.DuckCost && p.Exhaust <= 0 && P.DuckTime > 0)
                {
                    // Press down while sprinting: drop into a dodge slide.
                    p.Duck = P.DuckTime; p.Stamina -= P.DuckCost;
                }
                else
                {
                    double f = L.Sin / SIN45;
                    double heading = inp.Dir != 0 ? inp.Dir : JsSign(p.Vx);
                    double top = baseTop;
                    if (L.Sign != 0 && heading != 0)
                    {
                        if (heading * L.Sign > 0) top *= Math.Max(P.MinUphillShare, 1 - P.UpCost / 100 * f);
                        else top *= 1 + P.DownBoost / 100 * f;
                    }
                    p.Vx = Drive(p.Vx, inp.Dir, top, P, 1);
                }
                p.X += p.Vx * L.Cos * DT;
                if (p.X < W.EdgeMargin) { p.X = W.EdgeMargin; p.Vx = 0; }
                if (p.X > worldW - W.EdgeMargin) { p.X = worldW - W.EdgeMargin; p.Vx = 0; }
                HitCliffs(p, P, W, map, p.Px, p.Py);

                if (p.X < L.X1 || p.X > L.X2)
                {
                    double hw = IsSteep(L, P) ? 0 : P.FootWidth / 2;
                    double yEdge = L.YAt(p.X);
                    double xe = L.ClampX(p.X);
                    double yEnd = L.YAt(xe);
                    Line best = null; double bestD = W.Snap;
                    foreach (var M in lines)
                    {
                        if (M == L || p.X < M.X1 || p.X > M.X2) continue;
                        double dy = Math.Abs(M.YAt(M.ClampX(xe)) - yEnd);
                        if (dy <= bestD) { best = M; bestD = dy; }
                    }
                    if (best != null) { p.Line = best.Index; p.Y = best.YAt(p.X); }
                    else if (p.X >= L.X1 - hw && p.X <= L.X2 + hw)
                    {
                        p.Y = yEnd;
                    }
                    else
                    {
                        Line reach = null; double reachD = W.Snap, reachY = 0;
                        foreach (var M in lines)
                        {
                            if (M == L || IsSteep(M, P) || p.X < M.X1 - hw || p.X > M.X2 + hw) continue;
                            double my = M.YAt(M.ClampX(p.X)), dy = Math.Abs(my - yEnd);
                            if (dy <= reachD) { reach = M; reachD = dy; reachY = my; }
                        }
                        if (reach != null) { p.Line = reach.Index; p.Y = reachY; }
                        else
                        {
                            double sp = p.Vx;
                            p.Vx = sp * L.Cos; p.Vy = sp * L.Sin * L.Sign;
                            p.Y = hw > 0 ? yEnd : yEdge; p.OnGround = false; p.Line = -1; p.Duck = 0;
                            p.Coyote = IsSteep(L, P) ? 0 : P.CoyoteMs / 1000;
                        }
                    }
                }
                else
                {
                    p.Y = L.YAt(p.X);
                }
                // Rock overhead: if this step would put his head into an underside, he does not take it.
                // He always counts at full standing height, even in a dodge slide.
                if (roofs.Count > 0 && RoofOver(P, W, roofs, p.X, p.Y) < p.Y + P.CharHeight && RoofOver(P, W, roofs, p.Px, p.Py) >= p.Py + P.CharHeight)
                {
                    p.X = p.Px; p.Y = p.Py; p.Vx = 0; p.Vy = 0; p.OnGround = true; p.Line = L.Index; p.Coyote = 0; p.Duck = duck0 > 0 ? 0 : p.Duck;
                }
                ax = p.X; ay = p.Y;
            }
            else
            {
                if (p.Lunge == 0) p.Vx = Drive(p.Vx, inp.Dir, baseTop, P, P.AirControl);
                p.X += p.Vx * DT;
                if (p.X < W.EdgeMargin) { p.X = W.EdgeMargin; p.Vx = 0; }
                if (p.X > worldW - W.EdgeMargin) { p.X = worldW - W.EdgeMargin; p.Vx = 0; }
                HitCliffs(p, P, W, map, p.Px, p.Py);
                if (roofs.Count > 0 && RoofOver(P, W, roofs, p.X, p.Py) < p.Py + P.CharHeight && RoofOver(P, W, roofs, p.Px, p.Py) >= p.Py + P.CharHeight)
                {
                    p.X = p.Px; p.Vx = 0;                     // drifting sideways into an overhang: stopped, as at a face
                }
            }

            // Jump: a press counts now, or shortly before landing (buffer); the ground counts shortly after leaving it (coyote).
            if (inp.JumpPressed) p.Buffer = P.BufferMs / 1000;
            bool wants = inp.JumpPressed || p.Buffer > 0;
            bool sliding = p.OnGround && IsSteep(lines[p.Line], P);
            bool can = p.OnGround ? (!sliding || P.SlideJump > 0) : p.Coyote > 0;
            bool justJumped = false;
            if (wants && can)
            {
                justJumped = true;
                p.Stamina -= P.JumpCost;
                if (p.Stamina <= 0) { p.Stamina = 0; p.Spent = true; }
                if (p.OnGround)
                {
                    var L = lines[p.Line];
                    double sp = p.Vx;
                    if (sliding && P.SlideJump == 1)
                    {
                        p.Vy = d.V0 * L.Cos;
                        p.Vx = -L.Sign * Math.Max(Math.Abs(sp * L.Cos), P.WalkSpeed);
                    }
                    else
                    {
                        p.Vy = d.V0; p.Vx = sp * L.Cos;
                    }
                }
                else
                {
                    p.Vy = d.V0;
                }
                p.OnGround = false; p.Line = -1; p.Coyote = 0; p.Buffer = 0; p.Jumping = true; p.Cutting = false; p.Duck = 0;
            }
            else
            {
                if (!p.OnGround) p.Coyote -= DT;
                p.Buffer -= DT;
            }

            // Vertical: three gravities. Rising with jump held, rising after letting go, and falling.
            if (!p.OnGround)
            {
                bool held = justJumped || (p.Jumping && inp.JumpHeld);
                if (p.Jumping && !held)
                {
                    p.Jumping = false; p.Cutting = true;
                    if (P.HardCut && p.Vy > 0) p.Vy = 0;
                }
                double g;
                if (p.Vy > 0) g = p.Cutting ? d.G * P.CutMult : d.G;
                else g = d.GFall;
                double v1 = Math.Max(p.Vy - g * DT, -P.Terminal);
                double y1 = ay + 0.5 * (p.Vy + v1) * DT;
                // Hitting his head: rising into an underside stops the rise dead. Sideways speed is untouched.
                if (roofs.Count > 0 && y1 > ay)
                {
                    double c = RoofOver(P, W, roofs, p.X, ay);
                    if (y1 + P.CharHeight > c) { y1 = Math.Max(ay, c - P.CharHeight); v1 = 0; p.Jumping = false; p.Cutting = true; }
                }

                // One-way lines: pass up through them, land on them from above, with any part of the footing.
                Line land = null; double landY = double.NegativeInfinity;
                double hwAir = P.FootWidth / 2;
                foreach (var M in lines)
                {
                    double hw = IsSteep(M, P) ? 0 : hwAir;
                    if (p.X < M.X1 - hw || p.X > M.X2 + hw) continue;
                    double my = M.YAt(M.ClampX(p.X));
                    if (ay >= M.YAt(M.ClampX(ax)) - 1e-9 && y1 <= my && my > landY) { land = M; landY = my; }
                }
                Ledge grab = default;
                bool grabbed = land == null && inp.Grab && FindLedge(p, P, W, lines, y1, out grab);
                if (grabbed)
                {
                    StartHang(p, grab, y1);
                }
                else if (land != null)
                {
                    double hv = p.Vx;
                    p.Y = landY; p.OnGround = true; p.Line = land.Index; p.Jumping = false; p.Cutting = false;
                    p.Vx = IsSteep(land, P) ? hv * land.Cos + v1 * land.Sin * land.Sign * (1 - P.SlideFriction) : hv;
                    p.Vy = 0;
                    if (p.Lunge != 0) { p.Lunge = 0; p.Prone = P.ProneTime; }
                    if (p.Spent) { p.Spent = false; Tire(p, P); }
                }
                else
                {
                    p.Y = y1; p.Vy = v1;
                }
            }

            // Stamina on the ground: running drains it slowly, walking or standing recovers it, crouching recovers it fast.
            p.Crouch = false;
            if (p.OnGround && p.Mode == Mode.Normal && p.Exhaust <= 0 && p.Prone <= 0)
            {
                bool steep = IsSteep(lines[p.Line], P);
                bool running = run && inp.Dir != 0 && Math.Abs(p.Vx) > P.WalkSpeed + P.RunDrainAbove && !steep;
                p.Crouch = inp.Down && inp.Dir == 0 && Math.Abs(p.Vx) < P.CrouchBelow && !steep;
                if (p.Duck > 0) { p.Crouch = false; }       // sliding: neither draining nor recovering
                else if (running) { p.Stamina -= P.RunDrain * DT; if (p.Stamina <= 0) Tire(p, P); }
                else p.Stamina = Math.Min(P.StaminaMax, p.Stamina + (p.Crouch ? P.CrouchRegen : P.WalkRegen) * DT);
            }
            Tail(p, P, W, map);
        }

        // The lowest underside over his body at (x, y), or +infinity if there is open air above. Only rock above his feet counts.
        static double RoofOver(Movement P, WorldRules W, List<Underside> roofs, double x, double y)
        {
            double low = double.PositiveInfinity;
            double hb = P.BodyWidth / 2;
            foreach (var U in roofs)
            {
                double lo = Math.Max(U.X1, x - hb), hi = Math.Min(U.X2, x + hb);
                if (lo >= hi - 1e-9) continue;
                double u = Math.Min(U.YAt(lo), U.YAt(hi));
                if (u > y + W.CliffTop && u < low) low = u;
            }
            return low;
        }

        // Stop him at a cliff face. x0 and y0 are where he was before this tick's sideways move.
        static void HitCliffs(PlayerState p, Movement P, WorldRules W, Map map, double x0, double y0)
        {
            double hb = P.BodyWidth / 2;
            foreach (var c in map.Cliffs)
            {
                if (y0 >= c.Top - W.CliffTop) continue;          // feet at or above the top: over it, not against it
                if (y0 + P.CharHeight <= c.Bottom + W.CliffTop) continue;   // head below the bottom of the face: he passes under it
                if (c.Face < 0) { if (x0 <= c.X && p.X + hb > c.X) { p.X = c.X - hb; if (p.Vx > 0) p.Vx = 0; } }
                else { if (x0 >= c.X && p.X - hb < c.X) { p.X = c.X + hb; if (p.Vx < 0) p.Vx = 0; } }
            }
        }
    }
}
