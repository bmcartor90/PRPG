using System;
using System.Collections.Generic;
using Prpg.Sim;
using UnityEngine;

namespace Prpg.Game
{
    /// <summary>
    /// Draws one character: the body frame and any held items, pinned to the fists.
    /// Port of the lab's render code after pickFrame. Coordinates inside the character are atlas pixels relative to
    /// the anchor point (x right, y down), exactly as the lab's canvas works; this class turns them into Unity space.
    /// </summary>
    public sealed class CharacterView
    {
        readonly Transform root;
        readonly SpriteRenderer body;
        readonly List<SpriteRenderer> items = new List<SpriteRenderer>();
        CharacterAtlas atlas;
        ItemAtlas itemAtlas;
        string swordName, shieldName;

        public CharacterView(Transform parent)
        {
            root = new GameObject("Character").transform;
            root.SetParent(parent, false);
            body = NewRenderer("Body", 0);
        }

        SpriteRenderer NewRenderer(string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sortingOrder = 100 + order;
            return r;
        }

        public void SetCharacter(CharacterAtlas a, ItemAtlas it, string sword, string shield)
        {
            atlas = a; itemAtlas = it; swordName = sword; shieldName = shield;
        }

        struct ItemDraw { public string Name; public float X, Y, Angle, Squash; }

        /// <summary>Places everything for this screen frame. alpha blends between the last two simulation states.</summary>
        public void Draw(PlayerState p, Movement P, Map map, AnimState anim, long frameCount, double alpha)
        {
            var f = FramePicker.Pick(p, P, map, atlas, anim, frameCount, out int frameFlip);
            float s = (float)(P.CharHeight / atlas.StandingHeightPx);      // tiles per atlas pixel
            double ix = p.Px + (p.X - p.Px) * alpha, iy = p.Py + (p.Y - p.Py) * alpha;

            // Normally pinned by the point under his feet. On a ledge, pinned by his hands so they stay on the edge;
            // the last pull-up frame (no hand point) is pinned by his feet on top.
            double wx = ix, wy = iy;
            float ax = f.AnchorX, ay = f.AnchorY;
            if (p.Mode == Mode.Hang || p.Mode == Mode.Pull)
            {
                var L = map.Lines[p.HangLine];
                int sd = p.HangSide;
                double ex = sd < 0 ? L.X1 : L.X2, ey = sd < 0 ? L.Y1 : L.Y2;
                if (f.HasHand) { wx = ex + sd * 0.12; wy = ey; ax = f.HandX; ay = f.HandY; }
                else { wx = ex - sd * 0.45; wy = ey; }
            }
            else if (p.Mode == Mode.Climb && f.HasHand)
            {
                wy = iy + 0.76 * P.CharHeight; ax = f.HandX; ay = f.HandY;   // lower hand grips about three-quarters of his height up
            }

            int face = frameFlip != 0 ? frameFlip : anim.Face;
            root.localPosition = new Vector3((float)wx, (float)wy, 0);
            root.localScale = new Vector3(face, 1, 1);

            body.sprite = f.Sprite;
            body.transform.localPosition = new Vector3(-ax * s, ay * s, 0);
            body.transform.localScale = new Vector3(s, s, 1);

            // ---- Items (the lab's before / after lists) ----
            float SH = atlas.StandingHeightPx;
            float mx = f.W / 2 - ax, my = f.H / 2 - ay;
            bool holding = p.Mode != Mode.Normal, fromBehind = frameFlip != 0;
            bool swapping(int hand) => p.SwapHand == hand;
            bool showing(int hand) => swapping(hand) ? (f.IsDraw || FramePicker.SwapProgress(p) > 0.05) : p.Held(hand);
            double glide() { double u = FramePicker.SwapProgress(p); return u * u * (3 - 2 * u); }
            var before = new List<ItemDraw>();
            var after = new List<ItemDraw>();
            void Add(List<ItemDraw> list, string name, double x, double y, double ang, double squash)
                => list.Add(new ItemDraw { Name = name, X = (float)x, Y = (float)y, Angle = (float)ang, Squash = (float)squash });

            if (f.HasFists)
            {
                // His right hand (sword) is the back fist facing right and the front fist facing left; the shield is on the other.
                bool right = face > 0;
                bool fast = !p.OnGround || Math.Abs(p.Vx) > P.WalkSpeed + 0.25 * (P.RunSpeed - P.WalkSpeed);
                // One-handed hang and rope frames grip with a fixed hand; a held item goes to the free fist.
                bool oneHand = (f.Anim == "hang1" || f.Anim == "rope1") && f.Index == 0 && f.HasHand;
                bool freeFront = false;
                if (oneHand)
                {
                    double dFront = f.FrontX.HasValue ? Hypot(f.FrontX.Value - f.HandX, f.FrontY.Value - f.HandY) : double.NaN;
                    double dBack = f.BackX.HasValue ? Hypot(f.BackX.Value - f.HandX, f.BackY.Value - f.HandY) : double.NaN;
                    freeFront = dFront > dBack;
                }
                (double x, double y, bool hidden) fist(bool front)
                {
                    if (oneHand) front = freeFront;
                    float? fx = front ? f.FrontX : f.BackX, fy = front ? f.FrontY : f.BackY;
                    if (!fx.HasValue) return ((front ? 0.62 : 0.38) * f.W - ax, 0.52 * f.H - ay, true);
                    return (fx.Value - ax, fy.Value - ay, fromBehind);   // seen from behind, items are between him and the wall
                }
                if (showing(1))
                {
                    if (swapping(1) && f.IsDraw)
                    {
                        if (f.ItemPose != null) Add(f.ItemPose[3] != 0 ? before : after, swordName, f.ItemPose[0] - ax, f.ItemPose[1] - ay, f.ItemPose[2], 1);
                    }
                    else
                    {
                        bool front = !right;
                        var h = fist(front);
                        double ang = holding ? 78 : fast ? (front ? 12 : 152) : 38;
                        if (swapping(1))
                        {
                            // No arm animation for this draw: the sword slides from his hip to his fist.
                            double e = glide(), x0 = 0.03 * SH, y0 = -0.52 * SH;
                            Add(e < 0.6 || h.hidden ? before : after, swordName, x0 + (h.x - x0) * e, y0 + (h.y - y0) * e, 70 + (ang - 70) * e, 1);
                        }
                        else Add(h.hidden ? before : after, swordName, h.x, h.y, ang, 1);
                    }
                }
                if (showing(0))
                {
                    if (swapping(0) && f.IsDraw && f.ItemPose != null)
                        Add(f.ItemPose[3] != 0 ? before : after, shieldName, f.ItemPose[0] - ax, f.ItemPose[1] - ay, f.ItemPose[2], 0.55);
                    else
                    {
                        var h = fist(right);
                        if (swapping(0))
                        {
                            // Likewise the shield slides round from his back.
                            double e = glide(), x0 = -0.10 * SH, y0 = -0.62 * SH;
                            Add(e < 0.6 || h.hidden ? before : after, shieldName, x0 + (h.x + 4 - x0) * e, y0 + (h.y - 14 - y0) * e, 0, 0.55);
                        }
                        else Add(h.hidden ? before : after, shieldName, h.x + 4, h.y - 14, 0, 0.55);
                    }
                }
            }
            else
            {
                // A frame without fist positions: items sit at a fixed spot on the body.
                if (showing(0) && !fromBehind) Add(before, shieldName, mx - 0.13 * SH, my - 0.04 * SH, 0, 0.55);
                if (showing(1)) Add(after, swordName, mx + (fromBehind ? 0.17 : 0.05) * SH, my + 0.03 * SH, holding ? 78 : 38, 1);
            }

            int n = 0;
            for (int k = 0; k < before.Count; k++) Place(n++, before[k], s, -before.Count + k);
            for (int k = 0; k < after.Count; k++) Place(n++, after[k], s, 1 + k);
            for (int k = n; k < items.Count; k++) items[k].enabled = false;
        }

        void Place(int slot, ItemDraw d, float s, int order)
        {
            while (items.Count <= slot) items.Add(NewRenderer("Item", 0));
            var r = items[slot];
            if (!itemAtlas.Sprites.TryGetValue(d.Name, out var sprite)) { r.enabled = false; return; }
            r.enabled = true;
            r.sprite = sprite;
            r.sortingOrder = 100 + order;
            var t = r.transform;
            t.localPosition = new Vector3(d.X * s, -d.Y * s, 0);
            t.localRotation = Quaternion.Euler(0, 0, -d.Angle);   // the lab's canvas angle is clockwise with y down
            t.localScale = new Vector3(d.Squash, 1, 1);
        }

        static double Hypot(double a, double b) => Math.Sqrt(a * a + b * b);
    }
}
