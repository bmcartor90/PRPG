namespace Prpg.Sim
{
    public enum Mode { Normal, Hang, Pull, Climb }

    /// <summary>
    /// Everything about one character's movement, as plain values. Lines and climbables are referred to by
    /// their index in the map, so a copy shares nothing with the original.
    /// </summary>
    public sealed class PlayerState
    {
        public double X, Y, Px, Py;          // position now and at the start of this tick (for drawing between ticks)
        public double Vx, Vy;                // on the ground Vx is speed along the surface
        public bool OnGround;                // standing or sliding on a line (the lab's 'grounded'; includes steep slides)
        public int Line = -1;                // the line stood on, or -1
        public double Coyote, Buffer;
        public bool Jumping, Cutting;
        public double Lift, PLift;           // drawing aid from the lab: how far the circle sits up off a slope
        public double SafeX;
        public int SafeLine = -1;

        public double Stamina = double.NaN;  // NaN means "not set yet": the next step fills the bar
        public double Exhaust;
        public bool Winded, Spent, Crouch;
        public Mode Mode;

        public int HangLine = -1, HangSide;  // the ledge end held: line index and which end (-1 left, 1 right)
        public double HangProg, HangY0;
        public int Climb = -1;               // the climbable held, or -1

        public double GrabLock;
        public int Lunge;                    // 0, or the direction of a lunge in progress
        public double Prone;

        public bool HeldLeft, HeldRight;     // gear in each hand
        public int SwapHand = -1;            // hand with a draw or sheathe in progress (0 left, 1 right), or -1
        public double SwapT, SwapTotal;

        public bool Held(int hand) => hand == 0 ? HeldLeft : HeldRight;
        public void SetHeld(int hand, bool v) { if (hand == 0) HeldLeft = v; else HeldRight = v; }

        // ---- The three terms later systems depend on. ----

        /// <summary>Standing or sliding on a slope too steep to stand on.</summary>
        public bool Sliding(Map map, Movement p) => OnGround && Line >= 0 && Mover.IsSteep(map.Lines[Line], p);
        /// <summary>Gravity is not acting on him: standing, hanging, pulling up or climbing. Sliding is not anchored.</summary>
        public bool Anchored(Map map, Movement p) => Mode != Mode.Normal || (OnGround && !Sliding(map, p));
        /// <summary>Anchored and standing on walkable ground.</summary>
        public bool Grounded(Map map, Movement p) => Mode == Mode.Normal && OnGround && !Sliding(map, p);
        /// <summary>Anything that is not grounded.</summary>
        public bool InDynamicAction(Map map, Movement p) => !Grounded(map, p);

        public PlayerState Clone() => (PlayerState)MemberwiseClone();

        public static PlayerState Make(Map map, double x, int line)
        {
            var L = map.Lines[line];
            double y = L.YAt(x);
            return new PlayerState { X = x, Y = y, Px = x, Py = y, OnGround = true, Line = line, SafeX = x, SafeLine = line };
        }
    }
}
