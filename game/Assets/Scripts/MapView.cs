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
        const float Bottom = -40;        // rock is drawn down to here

        readonly MeshFilter filter;
        readonly Mesh mesh = new Mesh();
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<Color> c = new List<Color>();
        readonly List<int> tri = new List<int>();

        public MapView(Transform parent, Material material)
        {
            var go = new GameObject("Map");
            go.transform.SetParent(parent, false);
            filter = go.AddComponent<MeshFilter>();
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.sortingOrder = 0;
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            filter.sharedMesh = mesh;
        }

        public void Build(Map map, Movement P)
        {
            v.Clear(); c.Clear(); tri.Clear();

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

            mesh.Clear();
            mesh.SetVertices(v);
            mesh.SetColors(c);
            mesh.SetTriangles(tri, 0);
            mesh.RecalculateBounds();
        }

        void Seg(float x1, float y1, float x2, float y2, float width, Color col)
        {
            var a = new Vector2(x1, y1); var b = new Vector2(x2, y2);
            var d = b - a;
            if (d.sqrMagnitude < 1e-12f) return;
            var n = new Vector2(-d.y, d.x).normalized * (width / 2);
            var along = d.normalized * (width / 2);          // square caps, so joined lines meet cleanly
            a -= along; b += along;
            Quad(a - n, b - n, b + n, a + n, col);
        }

        void Quad(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color col)
        {
            int i = v.Count;
            v.Add(p0); v.Add(p1); v.Add(p2); v.Add(p3);
            for (int k = 0; k < 4; k++) c.Add(col);
            tri.Add(i); tri.Add(i + 1); tri.Add(i + 2);
            tri.Add(i); tri.Add(i + 2); tri.Add(i + 3);
            // Both windings, so orientation never hides a shape.
            tri.Add(i); tri.Add(i + 2); tri.Add(i + 1);
            tri.Add(i); tri.Add(i + 3); tri.Add(i + 2);
        }

        // Ear clipping for a simple polygon (floating rock outlines may be concave).
        void Polygon(List<Vector2> pts, Color col)
        {
            int baseIndex = v.Count;
            foreach (var p in pts) { v.Add(p); c.Add(col); }
            var idx = new List<int>();
            for (int i = 0; i < pts.Count; i++) idx.Add(i);
            float area = 0;
            for (int i = 0; i < pts.Count; i++) { var a = pts[i]; var b = pts[(i + 1) % pts.Count]; area += a.x * b.y - b.x * a.y; }
            float sign = area >= 0 ? 1 : -1;     // +1 counter-clockwise
            int guard = 0;
            while (idx.Count > 3 && guard++ < 10000)
            {
                bool clipped = false;
                for (int i = 0; i < idx.Count; i++)
                {
                    int i0 = idx[(i + idx.Count - 1) % idx.Count], i1 = idx[i], i2 = idx[(i + 1) % idx.Count];
                    Vector2 a = pts[i0], b = pts[i1], d = pts[i2];
                    float cross = (b.x - a.x) * (d.y - a.y) - (b.y - a.y) * (d.x - a.x);
                    if (cross * sign <= 0) continue;
                    bool inside = false;
                    foreach (int j in idx)
                    {
                        if (j == i0 || j == i1 || j == i2) continue;
                        if (InTriangle(pts[j], a, b, d)) { inside = true; break; }
                    }
                    if (inside) continue;
                    AddTri(baseIndex + i0, baseIndex + i1, baseIndex + i2);
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
            if (idx.Count == 3) AddTri(baseIndex + idx[0], baseIndex + idx[1], baseIndex + idx[2]);
        }

        void AddTri(int a, int b, int d) { tri.Add(a); tri.Add(b); tri.Add(d); tri.Add(a); tri.Add(d); tri.Add(b); }

        static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
            float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
            float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }
    }
}
