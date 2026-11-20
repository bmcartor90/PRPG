using System.Collections.Generic;
using UnityEngine;

namespace Prpg.Game
{
    /// <summary>Builds one flat mesh of coloured shapes: thick line segments, quads and filled polygons. Drawn with the sprite material.</summary>
    public sealed class MeshBuilder
    {
        readonly Mesh mesh = new Mesh();
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<Color> c = new List<Color>();
        readonly List<int> tri = new List<int>();
        public readonly GameObject Object;
        public float Z;

        public MeshBuilder(string name, Transform parent, Material material, int sortingOrder)
        {
            Object = new GameObject(name);
            Object.transform.SetParent(parent, false);
            Object.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = Object.AddComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.sortingOrder = sortingOrder;
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();
        }

        public void Clear() { v.Clear(); c.Clear(); tri.Clear(); }

        public void Apply()
        {
            mesh.Clear();
            mesh.SetVertices(v);
            mesh.SetColors(c);
            mesh.SetTriangles(tri, 0);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000);   // never culled
        }

        public void Seg(float x1, float y1, float x2, float y2, float width, Color col)
        {
            var a = new Vector2(x1, y1); var b = new Vector2(x2, y2);
            var d = b - a;
            if (d.sqrMagnitude < 1e-12f) return;
            var n = new Vector2(-d.y, d.x).normalized * (width / 2);
            var along = d.normalized * (width / 2);          // square caps, so joined lines meet cleanly
            a -= along; b += along;
            Quad(a - n, b - n, b + n, a + n, col);
        }

        public void Rect(float x1, float y1, float x2, float y2, Color col)
            => Quad(new Vector2(x1, y1), new Vector2(x2, y1), new Vector2(x2, y2), new Vector2(x1, y2), col);

        /// <summary>A square marker of the given size centred on a point.</summary>
        public void Dot(float x, float y, float size, Color col) => Rect(x - size / 2, y - size / 2, x + size / 2, y + size / 2, col);

        /// <summary>The outline of a box, as four segments.</summary>
        public void Box(float x1, float y1, float x2, float y2, float width, Color col)
        {
            Seg(x1, y1, x2, y1, width, col); Seg(x2, y1, x2, y2, width, col);
            Seg(x2, y2, x1, y2, width, col); Seg(x1, y2, x1, y1, width, col);
        }

        public void Quad(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, Color col)
        {
            int i = v.Count;
            v.Add(new Vector3(p0.x, p0.y, Z)); v.Add(new Vector3(p1.x, p1.y, Z)); v.Add(new Vector3(p2.x, p2.y, Z)); v.Add(new Vector3(p3.x, p3.y, Z));
            for (int k = 0; k < 4; k++) c.Add(col);
            tri.Add(i); tri.Add(i + 1); tri.Add(i + 2);
            tri.Add(i); tri.Add(i + 2); tri.Add(i + 3);
            tri.Add(i); tri.Add(i + 2); tri.Add(i + 1);      // both windings, so orientation never hides a shape
            tri.Add(i); tri.Add(i + 3); tri.Add(i + 2);
        }

        /// <summary>Fills a simple polygon (concave is fine) by ear clipping.</summary>
        public void Polygon(List<Vector2> pts, Color col)
        {
            if (pts.Count < 3) return;
            int baseIndex = v.Count;
            foreach (var p in pts) { v.Add(new Vector3(p.x, p.y, Z)); c.Add(col); }
            var idx = new List<int>();
            for (int i = 0; i < pts.Count; i++) idx.Add(i);
            float area = 0;
            for (int i = 0; i < pts.Count; i++) { var a = pts[i]; var b = pts[(i + 1) % pts.Count]; area += a.x * b.y - b.x * a.y; }
            float sign = area >= 0 ? 1 : -1;
            int guard = 0;
            while (idx.Count > 3 && guard++ < 100000)
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
                    Tri(baseIndex + i0, baseIndex + i1, baseIndex + i2);
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
            if (idx.Count == 3) Tri(baseIndex + idx[0], baseIndex + idx[1], baseIndex + idx[2]);
        }

        void Tri(int a, int b, int d) { tri.Add(a); tri.Add(b); tri.Add(d); tri.Add(a); tri.Add(d); tri.Add(b); }

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
