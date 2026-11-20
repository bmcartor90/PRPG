using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SimInput = Prpg.Sim.Input;
using HandKey = Prpg.Sim.HandKey;

namespace Prpg.Game
{
    /// <summary>
    /// Named actions with keyboard and controller bindings (design.md, "Controls"). Everything reads actions,
    /// never raw buttons, so bindings can be changed later.
    ///
    /// The screen runs at any rate; the simulation runs at exactly 60. Presses that happen between ticks are
    /// latched here and handed to the next tick, so a tap shorter than a tick still counts, as in the lab.
    /// </summary>
    public sealed class Controls
    {
        public readonly InputActionMap Map = new InputActionMap("Platforming");
        public readonly InputAction Move, Jump, Run, Grab, Interact, Imagine, LeftHand, RightHand, Menu, Pause;

        bool jumpQueued, upQueued, downQueued, upWas, downWas;
        readonly List<int> swapQueue = new List<int>();

        public Controls()
        {
            Move = Map.AddAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector(mode=2)")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Move.AddBinding("<Gamepad>/leftStick");
            Move.AddBinding("<Gamepad>/dpad");

            Jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            Run = Button("Run", "<Keyboard>/d", "<Gamepad>/buttonWest");
            Grab = Button("Grab", "<Keyboard>/f", "<Gamepad>/rightTrigger");
            Interact = Button("Interact", "<Keyboard>/a", "<Gamepad>/buttonEast");
            Imagine = Button("Imagine", "<Keyboard>/s", "<Gamepad>/leftTrigger");
            LeftHand = Button("LeftHand", "<Keyboard>/q", "<Gamepad>/leftShoulder");
            RightHand = Button("RightHand", "<Keyboard>/w", "<Gamepad>/rightShoulder");
            Menu = Button("Menu", "<Keyboard>/c", "<Gamepad>/buttonNorth");
            Pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            Map.Enable();
        }

        InputAction Button(string name, string key, string pad)
        {
            var a = Map.AddAction(name, InputActionType.Button);
            a.AddBinding(key);
            a.AddBinding(pad);
            return a;
        }

        Vector2 Stick => Move.ReadValue<Vector2>();
        // Thresholds from the lab's controller code: sideways past 0.35, up or down past 0.5.
        int Dir { get { float x = Stick.x; return x < -0.35f ? -1 : x > 0.35f ? 1 : 0; } }
        bool Up => Stick.y > 0.5f;
        bool Down => Stick.y < -0.5f;

        /// <summary>Call once per screen frame, before running ticks: remembers presses until a tick takes them.</summary>
        public void Poll()
        {
            if (Jump.WasPressedThisFrame()) jumpQueued = true;
            bool up = Up, down = Down;
            if (up && !upWas) upQueued = true;
            if (down && !downWas) downQueued = true;
            upWas = up; downWas = down;
            if (LeftHand.WasPressedThisFrame()) QueueSwap(0);
            if (RightHand.WasPressedThisFrame()) QueueSwap(1);
        }

        void QueueSwap(int hand) { if (!swapQueue.Contains(hand)) swapQueue.Add(hand); }

        /// <summary>The input for one simulation tick. A queued draw waits while a draw or sheathe is in progress: one hand at a time.</summary>
        public SimInput Sample(bool swapInProgress)
        {
            var hand = HandKey.None;
            if (!swapInProgress && swapQueue.Count > 0) { hand = (HandKey)swapQueue[0]; swapQueue.RemoveAt(0); }
            var inp = new SimInput
            {
                Dir = Dir,
                Run = Run.IsPressed(),
                JumpHeld = Jump.IsPressed(),
                JumpPressed = jumpQueued,
                Up = Up || upQueued,
                Down = Down || downQueued,
                Grab = Grab.IsPressed(),
                Hand = hand,
            };
            jumpQueued = upQueued = downQueued = false;
            return inp;
        }

        public void ClearQueues() { jumpQueued = upQueued = downQueued = false; swapQueue.Clear(); }
    }
}
