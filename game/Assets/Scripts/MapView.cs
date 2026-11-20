using System;
using System.Collections.Generic;
using Prpg.Sim;
using UnityEngine;

namespace Prpg.Game
{
    /// <summary>
    /// Draws the map the way the lab does: white surface lines, dashed where too steep to stand on, dark rock filled
    /// beneath ground and inside floating rock, grey cliff faces and undersides, scaffolds with thin posts, and climbables.
    /// One mesh with vertex colours; rebuilt when the map or the walkable limit changes.
    /// </summary>
    public sealed class MapView
    {
        static readonly Color Rock = new Color32(0x1c, 0x1c, 0x1c, 255);
        static readonly Color Face = new Color32(0x8a, 0x8a, 0x8a, 255);
        static readonly Color Post = new Color32(0x5a, 0x5a, 0x5a, 255);
        static readonly Color Climb = new Color32(0x9a, 0x9a, 0x9a, 255);
        static readonly Color Gold = new Color32(0xc9, 0xa2, 0x4a, 255);
        const float Lw = 0.09f;          // surface line width in tiles (the lab's s*0.09)
        const float Bottom = -40;        // rock is drawn down to here (below the bottom of any map)

        readonly MeshBuilder mb;

        public MapView(Transform parent, Material material) { mb = new MeshBuilder("Map", parent, material, 0); }

        public void SetVisible(bool on) => mb.Object.SetActive(on);

        void Seg(float x1, float y1, float x2, float y2, float width, Color col) => mb.Seg(x1, y1, x2, y2, width, col);
        void Quad(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color col) => mb.Quad(p0, p1, p2, p3, col);
        void Polygon(List<Vector2> pts, Color col) => mb.Polygon(pts, col);

        public void Build(Map map, Movement P)
        {
            mb.Clear();

            // Climbables, behind everything else.
            foreach (var C in map.Climbables)
            {
                float x = (float)C.X, ya = (float)C.Y1, yb = (float)C.Y2, w = 0.05f;
                if (C.Face != 0) { Seg(x + C.Face * 0.04f, ya, x + C.Face * 0.04f, yb, 0.08f, Gold); continue; }   // marked on the rock face
                if (C.Kind == ClimbKind.Ladder)
                {
                    Seg(x - 0.3f, ya, x - 0.3f, yb, w, Climb); Seg(x + 0.3f, ya, x + 0.3f, yb, w, Climb);
                    for (double y = C.Y1 + 0.4; y < C.Y2; y += 0.5) Seg(x - 0.3f, (float)y, x + 0.3f, (float)y, w, Climb);
                }
                else if (C.Kind == ClimbKind.Rope)
                {
                    Seg(x, ya, x, yb, w, Climb);
                    for (double y = C.Y1 + 0.5; y < C.Y2; y += 1) Seg(x - 0.1f, (float)y, x + 0.1f, (float)y, w, Climb);
                }
                else
                {
                    Seg(x - 0.35f, ya, x - 0.35f, yb, w, Climb); Seg(x + 0.35f, ya, x + 0.35f, yb, w, Climb);
                    Seg(x - 0.35f, yb, x + 0.35f, yb, w, Climb); Seg(x - 0.35f, ya, x + 0.35f, ya, w, Climb);
                    int k = 0;
                    for (double y = C.Y1 + 0.5; y < C.Y2; y += 0.5, k++)
                    {
                        Seg(x - 0.35f, (float)y, x + 0.35f, (float)y, w, Climb);
                        float off = k % 2 != 0 ? 0.12f : -0.12f;
                        Seg(x + off, (float)y, x + off, (float)(y - 0.5), w, Climb);
                    }
                }
            }

            // Rock: under every ground line down past the bottom, and inside each floating rock.
            foreach (var L in map.Lines)
                if (L.Solid && !L.Slab)
                    Quad(new Vector2((float)L.X1, (float)L.Y1), new Vector2((float)L.X2, (float)L.Y2),
                         new Vector2((float)L.X2, Bottom), new Vector2((float)L.X1, Bottom), Rock);
            foreach (var outline in map.FloatingRock)
            {
                var pts = new List<Vector2>();
                foreach (var q in outline) pts.Add(new Vector2((float)q[0], (float)q[1]));
                Polygon(pts, Rock);
            }

            // Cliff faces and undersides.
            foreach (var W in map.Cliffs)
                Seg((float)W.X, (float)W.Top, (float)W.X, double.IsNegativeInfinity(W.Bottom) ? Bottom : (float)W.Bottom, Lw * 0.6f, Face);
            foreach (var U in map.Undersides) Seg((float)U.X1, (float)U.Y1, (float)U.X2, (float)U.Y2, Lw * 0.6f, Face);

            // Scaffolds: thin posts underneath, so they read as something built.
            foreach (var L in map.Lines)
            {
                if (L.Solid) continue;
                foreach (double q in new[] { 0.12, 0.88 })
                {
                    double x = L.X1 + (L.X2 - L.X1) * q, y = L.YAt(x);
                    Seg((float)x, (float)y, (float)x, (float)(y - 0.9), Lw * 0.35f, Post);
                }
                Seg((float)(L.X1 + (L.X2 - L.X1) * 0.12), (float)(L.Y1 - 0.9), (float)(L.X1 + (L.X2 - L.X1) * 0.88), (float)(L.Y2 - 0.45), Lw * 0.35f, Post);
            }

            // Surfaces: solid white where he can stand, dashed where too steep.
            foreach (var L in map.Lines)
            {
                if (!Mover.IsSteep(L, P)) { Seg((float)L.X1, (float)L.Y1, (float)L.X2, (float)L.Y2, Lw, Color.white); continue; }
                double len = Math.Sqrt((L.X2 - L.X1) * (L.X2 - L.X1) + (L.Y2 - L.Y1) * (L.Y2 - L.Y1));
                double dash = Lw * 3, gap = Lw * 2.5;
                for (double d = 0; d < len; d += dash + gap)
                {
                    double e = Math.Min(len, d + dash), t0 = d / len, t1 = e / len;
                    Seg((float)(L.X1 + (L.X2 - L.X1) * t0), (float)(L.Y1 + (L.Y2 - L.Y1) * t0),
                        (float)(L.X1 + (L.X2 - L.X1) * t1), (float)(L.Y1 + (L.Y2 - L.Y1) * t1), Lw, Color.white);
                }
            }

            mb.Apply();
        }
    }
}
