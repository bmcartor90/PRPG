using System;
using System.Collections.Generic;
using System.IO;
using Prpg.Sim;
using UnityEngine;

namespace Prpg.Game
{
    /// <summary>One frame of a character atlas, in atlas pixels (x right, y down), as the atlas JSON gives it.</summary>
    public sealed class Frame
    {
        public string Anim; public int Index;
        public float X, Y, W, H, AnchorX, AnchorY;
        public bool HasHand; public float HandX, HandY;
        public bool HasFists;
        public float? BackX, BackY, FrontX, FrontY;     // null when that fist is hidden behind the body
        public bool IsDraw;                             // has an itemPose entry (draw frames)
        public float[] ItemPose;                        // [x, y, angle, behind], or null while the item is not visible yet
        public Sprite Sprite;
    }

    /// <summary>A character's atlas: the image and every animation's frames. Loaded from files at run time.</summary>
    public sealed class CharacterAtlas
    {
        public float StandingHeightPx;
        public readonly Dictionary<string, Frame[]> Anims = new Dictionary<string, Frame[]>();
        public Texture2D Texture;

        public Frame[] Get(string name) => Anims.TryGetValue(name, out var f) ? f : null;

        /// <summary>Sprites are made at one atlas pixel per unit; the view scales them by the character's height.</summary>
        public static CharacterAtlas Load(string jsonPath)
        {
            var o = (JsonObject)Json.Parse(File.ReadAllText(jsonPath));
            var a = new CharacterAtlas { StandingHeightPx = (float)o.Number("standingHeightPx") };
            a.Texture = Images.Load(Path.Combine(Path.GetDirectoryName(jsonPath), o.String("image")));
            var anims = o.Object("animations");
            foreach (var name in anims.Keys)
            {
                var list = anims.Array(name);
                var frames = new Frame[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    var f = (JsonObject)list[i];
                    var fr = new Frame
                    {
                        Anim = name, Index = i,
                        X = (float)f.Number("x"), Y = (float)f.Number("y"), W = (float)f.Number("w"), H = (float)f.Number("h"),
                        AnchorX = (float)f.Number("anchorX"), AnchorY = (float)f.Number("anchorY"),
                    };
                    if (f.Has("handX")) { fr.HasHand = true; fr.HandX = (float)f.Number("handX"); fr.HandY = (float)f.Number("handY"); }
                    if (f.Has("backFist") || f.Has("frontFist"))
                    {
                        fr.HasFists = true;
                        if (f.Get("backFist") is List<object> b) { fr.BackX = (float)(double)b[0]; fr.BackY = (float)(double)b[1]; }
                        if (f.Get("frontFist") is List<object> fr2) { fr.FrontX = (float)(double)fr2[0]; fr.FrontY = (float)(double)fr2[1]; }
                    }
                    if (f.Has("itemPose"))
                    {
                        fr.IsDraw = true;
                        if (f.Get("itemPose") is List<object> p) fr.ItemPose = new[] { (float)(double)p[0], (float)(double)p[1], (float)(double)p[2], (float)(double)p[3] };
                    }
                    // Pivot at the frame's top-left; the view places it so the anchor lands where it should.
                    var rect = new Rect(fr.X, a.Texture.height - fr.Y - fr.H, fr.W, fr.H);
                    fr.Sprite = Sprite.Create(a.Texture, rect, new Vector2(0, 1), 1, 0, SpriteMeshType.FullRect);
                    frames[i] = fr;
                }
                a.Anims[name] = frames;
            }
            return a;
        }
    }

    /// <summary>Items (sword, shield...) as separate pictures pinned to the fists. All drawn at one size in the world.</summary>
    public sealed class ItemAtlas
    {
        public float PixelsPerTile;
        public readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static ItemAtlas Load(string jsonPath)
        {
            var o = (JsonObject)Json.Parse(File.ReadAllText(jsonPath));
            var a = new ItemAtlas { PixelsPerTile = (float)o.Number("scaledForPixelsPerTile") };
            var tex = Images.Load(Path.Combine(Path.GetDirectoryName(jsonPath), o.String("image")));
            var items = o.Object("items");
            foreach (var name in items.Keys)
            {
                var it = items.Object(name);
                float x = (float)it.Number("x"), y = (float)it.Number("y"), w = (float)it.Number("w"), h = (float)it.Number("h");
                float gx = (float)it.Number("gripX"), gy = (float)it.Number("gripY");
                var rect = new Rect(x, tex.height - y - h, w, h);
                a.Sprites[name] = Sprite.Create(tex, rect, new Vector2(gx / w, 1 - gy / h), a.PixelsPerTile, 0, SpriteMeshType.FullRect);
            }
            return a;
        }
    }

    static class Images
    {
        public static Texture2D Load(string path)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            if (!tex.LoadImage(File.ReadAllBytes(path))) throw new InvalidDataException("Could not read image " + path);
            tex.filterMode = FilterMode.Trilinear;     // smooth scaling, never snapped to whole pixels
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.anisoLevel = 4;
            return tex;
        }
    }
}
