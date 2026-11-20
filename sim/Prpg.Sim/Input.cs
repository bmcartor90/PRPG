namespace Prpg.Sim
{
    public enum HandKey { None = -1, Left = 0, Right = 1 }

    /// <summary>
    /// Everything the simulation sees of the controls for one tick. Sampled once per simulation step.
    /// Drawing and input code turn buttons into this; the simulation never reads devices.
    /// </summary>
    public struct Input
    {
        public int Dir;             // -1 left, 0 none, 1 right
        public bool Run;
        public bool JumpHeld;
        public bool JumpPressed;    // true only on the step the jump button went down
        public bool Up;
        public bool Down;
        public bool Grab;
        public HandKey Hand;        // a draw or sheathe key pressed this step

        public static Input None => new Input { Hand = HandKey.None };
    }
}
