using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Minimalist monochrome icons generated at runtime as white-on-transparent
    /// textures. They are tinted via <see cref="UnityEngine.UI.Image.color"/>, which
    /// both keeps the ULTRAKILL aesthetic and gives the capability state colors.
    /// The generated set avoids relying on font glyph coverage (CJK/Arabic etc.).
    ///
    /// The Discord mark is the one exception: the official white logo PNG
    /// embedded into the assembly (see tools/discord-logo.md) is preferred, and
    /// the procedural Clyde approximation is only a fallback for a missing
    /// resource.
    /// </summary>
    public static class PrimitiveSprites
    {
        private const string DiscordLogoResource = "UltrakULL.assets.discord_logo_white.png";
        /// <summary>Name of the embedded warning icon resource (item F4).</summary>
        /// <remarks>
        /// Asset: white caution/warning mark, shown when a remote pack logo or
        /// gallery image fails to load.
        /// Source: CityPNG — https://www.citypng.com/photo/19198/white-caution-mark-warning-sign-icon-hd-png
        /// The file lives in UltrakULL/assets/warning_icon.png and is embedded
        /// into the mod DLL at build time so every user gets it, not just the
        /// original downloader.
        /// </remarks>
        private const string WarningIconResource = "UltrakULL.assets.warning_icon.png";
        public enum Kind
        {
            Speaker,      // main voice dubbing
            Star,         // extra dubbing
            Image,        // replaced textures
            Heart,        // like count
            ArrowRight,   // selectable card indicator
            Check,        // confirmation / done
            Close,        // close/dismiss
            Download,     // download arrow
            Refresh,      // reload the list
            Circle,       // generic dot / loader / authors
            Ring,         // hollow ring / donut loader
            Diamond,      // small accent
            TriangleUp,   // expand / sort direction
            ChevronDown,  // collapse / dropdown
            Discord,      // link to the original Discord post
            Warning,      // warning / error indicator (image load failure)
        }

        private const int Size = 32;

        private static readonly Dictionary<Kind, Sprite> Cache = new Dictionary<Kind, Sprite>();

        public static Sprite Get(Kind kind)
        {
            Sprite cached;
            if (Cache.TryGetValue(kind, out cached))
            {
                return cached;
            }

            // The Discord mark comes from the embedded official asset; every
            // other icon (and Discord when the asset is missing) is generated.
            // The Warning icon comes from the embedded CityPNG asset; falls back
            // to the procedural drawing if the resource is missing.
            Sprite sprite = null;
            if (kind == Kind.Discord)
            {
                sprite = CreateDiscordFromEmbeddedAsset();
            }
            else if (kind == Kind.Warning)
            {
                sprite = LoadWarningFromEmbeddedAsset();
            }
            if (sprite == null)
            {
                sprite = Create(kind);
            }
            Cache[kind] = sprite;
            return sprite;
        }

        /// <summary>
        /// Loads the embedded white Discord logo PNG. Returns null (and logs a
        /// warning) when the resource is missing or unreadable, so the caller
        /// falls back to the procedural icon.
        /// </summary>
        private static Sprite CreateDiscordFromEmbeddedAsset()
        {
            return LoadEmbeddedSprite(DiscordLogoResource, "UllBrowser_Discord");
        }

        /// <summary>
        /// Loads the embedded CityPNG warning icon PNG. Returns null (and logs a
        /// warning) when the resource is missing or unreadable, so the caller
        /// falls back to the procedural Warning drawing.
        /// </summary>
        private static Sprite LoadWarningFromEmbeddedAsset()
        {
            return LoadEmbeddedSprite(WarningIconResource, "UllBrowser_Warning_Custom");
        }

        /// <summary>
        /// Loads a PNG from the assembly's embedded resources and turns it into
        /// a sprite. Used for the Discord logo and the warning icon so every
        /// user gets the asset regardless of their local Downloads folder.
        /// </summary>
        private static Sprite LoadEmbeddedSprite(string resourceName, string spriteName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Logging.Warn("[PrimitiveSprites] Embedded resource '" + resourceName + "' was not found.");
                        return null;
                    }

                    using (MemoryStream memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                        texture.name = spriteName;
                        if (!ImageConversion.LoadImage(texture, memory.ToArray(), false))
                        {
                            UnityEngine.Object.Destroy(texture);
                            Logging.Warn("[PrimitiveSprites] Could not decode embedded resource '" + resourceName + "'.");
                            return null;
                        }
                        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[PrimitiveSprites] Could not load embedded resource '" + resourceName + "': " + e.Message);
                return null;
            }
        }

        private static Sprite Create(Kind kind)
        {
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.name = "UllBrowser_" + kind;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] pixels = new Color32[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }

            System.Action<int, int> paint = delegate (int x, int y)
            {
                if (x >= 0 && x < Size && y >= 0 && y < Size)
                {
                    pixels[y * Size + x] = new Color32(255, 255, 255, 255);
                }
            };

            // Punches pixels back to transparency; used to carve shapes (e.g. the
            // Discord eyes) out of an already painted silhouette.
            System.Action<int, int> erase = delegate (int x, int y)
            {
                if (x >= 0 && x < Size && y >= 0 && y < Size)
                {
                    pixels[y * Size + x] = new Color32(0, 0, 0, 0);
                }
            };

            switch (kind)
            {
                case Kind.Speaker: PaintSpeaker(paint); break;
                case Kind.Star: PaintPolygon(paint, StarPolygon(15.5f, 16f, 14f, 6f)); break;
                case Kind.Image: PaintImage(paint); break;
                case Kind.Heart: PaintHeart(paint); break;
                case Kind.ArrowRight: PaintPolygon(paint, new Vector2[] { new Vector2(7, 5), new Vector2(28, 16), new Vector2(7, 27) }); break;
                case Kind.Check: PaintSegment(paint, new Vector2(7, 17), new Vector2(15, 25), 4f); PaintSegment(paint, new Vector2(15, 25), new Vector2(26, 10), 4f); break;
                case Kind.Close: PaintSegment(paint, new Vector2(9, 9), new Vector2(23, 23), 4f); PaintSegment(paint, new Vector2(23, 9), new Vector2(9, 23), 4f); break;
                case Kind.Download: FillRect(paint, 14, 3, 17, 16); PaintPolygon(paint, new Vector2[] { new Vector2(5, 14), new Vector2(27, 14), new Vector2(15.5f, 25) }); FillRect(paint, 6, 28, 25, 30); break;
                case Kind.Refresh: PaintRefresh(paint); break;
                case Kind.Circle: PaintPolygon(paint, CirclePolygon(15.5f, 15.5f, 12f, 24)); break;
                case Kind.Ring: PaintRing(paint); break;
                case Kind.Diamond: PaintPolygon(paint, new Vector2[] { new Vector2(15.5f, 3), new Vector2(29, 15.5f), new Vector2(15.5f, 28), new Vector2(2, 15.5f) }); break;
                case Kind.TriangleUp: PaintPolygon(paint, new Vector2[] { new Vector2(15.5f, 3), new Vector2(28, 27), new Vector2(3, 27) }); break;
                case Kind.ChevronDown: PaintPolygon(paint, new Vector2[] { new Vector2(5, 12), new Vector2(15.5f, 25), new Vector2(26, 12), new Vector2(22, 8), new Vector2(15.5f, 17), new Vector2(9, 8) }); break;
                case Kind.Discord: PaintDiscord(paint, erase); break;
                case Kind.Warning: PaintWarning(paint); break;
            }

            // The icon painters treat y as growing downward (screen-style), but Unity
            // texture rows start at the bottom, so flip the buffer vertically before
            // uploading. Without this every icon renders upside down.
            Color32[] flipped = new Color32[pixels.Length];
            for (int y = 0; y < Size; y++)
            {
                int src = y * Size;
                int dst = (Size - 1 - y) * Size;
                for (int x = 0; x < Size; x++)
                {
                    flipped[dst + x] = pixels[src + x];
                }
            }

            texture.SetPixels32(flipped);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---------------------------------------------------------------------
        // Individual icon painters
        // ---------------------------------------------------------------------

        private static void PaintSpeaker(System.Action<int, int> paint)
        {
            // Small body box on the left
            FillRect(paint, 5, 12, 11, 20);
            // Cone expanding to the right
            PaintPolygon(paint, new Vector2[]
            {
                new Vector2(11, 12),
                new Vector2(18, 6),
                new Vector2(18, 26),
                new Vector2(11, 20)
            });
            // Two sound waves emanating from the cone
            DrawArc(paint, 19f, 16f, 5f, 1.7f, -52f, 52f);
            DrawArc(paint, 19f, 16f, 10f, 1.7f, -52f, 52f);
        }

        private static void PaintImage(System.Action<int, int> paint)
        {
            // Outer frame (2px thick)
            for (int x = 5; x <= 26; x++)
            {
                for (int y = 5; y <= 26; y++)
                {
                    bool edge = x <= 7 || x >= 24 || y <= 7 || y >= 24;
                    if (edge)
                    {
                        paint(x, y);
                    }
                }
            }
            // Mountain
            PaintPolygon(paint, new Vector2[] { new Vector2(9, 22), new Vector2(15, 12), new Vector2(21, 22) });
            // Sun
            PaintPolygon(paint, CirclePolygon(22f, 10.5f, 2.8f, 8));
        }

        private static void PaintHeart(System.Action<int, int> paint)
        {
            // Two lobes at the top
            FillCircle(paint, 10.5f, 12f, 6.2f);
            FillCircle(paint, 21.5f, 12f, 6.2f);
            // Lower V that meets the lobes into a heart
            PaintPolygon(paint, new Vector2[]
            {
                new Vector2(4.5f, 14f),
                new Vector2(16f, 29f),
                new Vector2(27.5f, 14f),
                new Vector2(16f, 11f)
            });
        }

        private static void PaintRefresh(System.Action<int, int> paint)
        {
            // Classic circular-arrows refresh (item 21): two opposing ~150-degree
            // arcs, each ending in a tangential arrowhead, both suggesting the
            // same clockwise rotation. Angles live in the painters' y-down space,
            // where growing angles run clockwise on screen.
            float cx = 15.5f;
            float cy = 15.5f;
            float radius = 9f;

            // Right-side arc (top-right to bottom-right) and left-side arc
            // (bottom-left to top-left); the two gaps sit at the bottom and top.
            DrawArc(paint, cx, cy, radius, 1.6f, -75f, 75f);
            DrawArc(paint, cx, cy, radius, 1.6f, 105f, 255f);

            // Arrowheads at the leading (clockwise) ends of both arcs.
            PaintArcArrowhead(paint, cx, cy, radius, 75f);
            PaintArcArrowhead(paint, cx, cy, radius, 255f);
        }

        /// <summary>
        /// Triangular arrowhead at the circle position <paramref name="angleDeg"/>,
        /// pointing along the clockwise tangent (the direction of a growing angle).
        /// </summary>
        private static void PaintArcArrowhead(System.Action<int, int> paint, float cx, float cy, float radius, float angleDeg)
        {
            float radians = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            // Position on the circle, the clockwise tangent and its perpendicular.
            Vector2 position = new Vector2(cx + radius * cos, cy + radius * sin);
            Vector2 tangent = new Vector2(-sin, cos);
            Vector2 perpendicular = new Vector2(cos, sin);

            Vector2 tip = position + tangent * 5.5f;
            Vector2 baseA = position + perpendicular * 3.2f;
            Vector2 baseB = position - perpendicular * 3.2f;

            PaintPolygon(paint, new Vector2[] { tip, baseA, baseB });
        }

        private static void PaintRing(System.Action<int, int> paint)
        {
            // Hollow ring (donut): outer radius ~12, inner radius ~8, stroke ~4px.
            float cx = 15.5f;
            float cy = 15.5f;
            float outer = 12f;
            float inner = 8f;
            for (int x = 4; x < 27; x++)
            {
                for (int y = 4; y < 27; y++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < inner || dist > outer)
                    {
                        continue;
                    }
                    paint(x, y);
                }
            }
        }

        private static void PaintDiscord(System.Action<int, int> paint, System.Action<int, int> erase)
        {
            // Discord "Clyde" logo: a stylised game-controller / face mark.
            // Rounded head, wider at the top (controller grips), slightly tapered
            // toward the bottom, with two eye cut-outs.
            float cx = 15.5f;
            float cy = 15.5f;

            // Head outline: a rounded rectangle / controller shape.
            // Top band is wider (controller grips), bottom tapers in.
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;

                    // The controller shape: wider at top, rounded bottom.
                    float topWidth = 13.5f;
                    float bottomWidth = 10.5f;
                    float halfW = Mathf.Lerp(bottomWidth, topWidth, (y + 0.5f) / 32f);
                    float halfH = 13f;

                    if (Mathf.Abs(dx) > halfW || Mathf.Abs(dy) > halfH)
                    {
                        continue;
                    }

                    // Rounded corners
                    float corner = 4.5f;
                    float cxCorner = cx + Mathf.Sign(dx) * (halfW - corner);
                    float cyCorner = cy - Mathf.Sign(dy) * (halfH - corner);
                    float cdx = dx - Mathf.Sign(dx) * (halfW - corner);
                    float cdy = dy + Mathf.Sign(dy) * (halfH - corner);
                    if (Mathf.Abs(dx) > halfW - corner && Mathf.Abs(dy) > halfH - corner)
                    {
                        float dist = Mathf.Sqrt(cdx * cdx + cdy * cdy);
                        if (dist > corner)
                        {
                            continue;
                        }
                    }

                    paint(x, y);
                }
            }

            // Eyes: two vertical ovals carved out of the upper face.
            FillEllipse(erase, 11.5f, 13f, 2.2f, 4f);
            FillEllipse(erase, 19.5f, 13f, 2.2f, 4f);
        }

        // ---------------------------------------------------------------------
        // Primitives
        // ---------------------------------------------------------------------

        private static void FillRect(System.Action<int, int> paint, int xMin, int yMin, int xMax, int yMax)
        {
            xMin = Mathf.Max(0, xMin);
            yMin = Mathf.Max(0, yMin);
            xMax = Mathf.Min(Size - 1, xMax);
            yMax = Mathf.Min(Size - 1, yMax);
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    paint(x, y);
                }
            }
        }

        private static void FillCircle(System.Action<int, int> paint, float cx, float cy, float radius)
        {
            PaintPolygon(paint, CirclePolygon(cx, cy, radius, 24));
        }

        private static void FillEllipse(System.Action<int, int> paint, float cx, float cy, float rx, float ry)
        {
            // A y-scaled circle, sampled like CirclePolygon.
            Vector2[] points = new Vector2[16];
            for (int i = 0; i < 16; i++)
            {
                float angle = i * (2f * Mathf.PI / 16f);
                points[i] = new Vector2(cx + rx * Mathf.Cos(angle), cy + ry * Mathf.Sin(angle));
            }
            PaintPolygon(paint, points);
        }

        private static void PaintWarning(System.Action<int, int> paint)
        {
            // Warning icon: thick outline triangle with exclamation mark inside.
            float cx = 15.5f;
            float cy = 15.5f;
            float h = 12f;   // half-height of triangle
            float w = 12f;   // half-width of base

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;

                    // Bottom edge: horizontal line at y = cy + h
                    bool bottomEdge = Mathf.Abs(dy - h) < 1.5f && Mathf.Abs(dx) <= w;

                    // Left edge: from (cx-w, cy+h) to (cx, cy-h)
                    // Line equation: dy = h - (2h/w) * |dx| for dx < 0
                    float leftEdgeY = h - (2f * h / w) * Mathf.Abs(dx);
                    bool leftEdge = dx < -1f && dy > 0f && dy < h && Mathf.Abs(dy - leftEdgeY) < 1.5f;

                    // Right edge: from (cx+w, cy+h) to (cx, cy-h)
                    float rightEdgeY = h - (2f * h / w) * Mathf.Abs(dx);
                    bool rightEdge = dx > 1f && dy > 0f && dy < h && Mathf.Abs(dy - rightEdgeY) < 1.5f;

                    if (bottomEdge || leftEdge || rightEdge)
                    {
                        paint(x, y);
                        continue;
                    }

                    // Exclamation mark stem (vertical bar).
                    bool stem = Mathf.Abs(dx) < 1.5f && dy > 2f && dy < 10f;

                    // Exclamation mark dot.
                    float dotDist = Mathf.Sqrt(dx * dx + (dy - 0.5f) * (dy - 0.5f));
                    bool dot = dotDist < 2.5f;

                    if (stem || dot)
                    {
                        paint(x, y);
                    }
                }
            }
        }

        private static void DrawArc(System.Action<int, int> paint, float cx, float cy, float radius, float thickness, float angleStart, float angleEnd)        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < radius - thickness || dist > radius + thickness)
                    {
                        continue;
                    }
                    float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (ang < angleStart || ang > angleEnd)
                    {
                        continue;
                    }
                    paint(x, y);
                }
            }
        }

        private static void PaintSegment(System.Action<int, int> paint, Vector2 a, Vector2 b, float thickness)
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (DistanceToSegment(new Vector2(x + 0.5f, y + 0.5f), a, b) <= thickness)
                    {
                        paint(x, y);
                    }
                }
            }
        }

        private static void PaintPolygon(System.Action<int, int> paint, Vector2[] polygon)
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), polygon))
                    {
                        paint(x, y);
                    }
                }
            }
        }

        private static Vector2[] StarPolygon(float cx, float cy, float outer, float inner)
        {
            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = -Mathf.PI / 2f + i * Mathf.PI / 5f;
                float radius = (i % 2 == 0) ? outer : inner;
                points[i] = new Vector2(cx + radius * Mathf.Cos(angle), cy + radius * Mathf.Sin(angle));
            }
            return points;
        }

        private static Vector2[] CirclePolygon(float cx, float cy, float radius, int segments)
        {
            var points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * (2f * Mathf.PI / segments);
                points[i] = new Vector2(cx + radius * Mathf.Cos(angle), cy + radius * Mathf.Sin(angle));
            }
            return points;
        }

        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int count = polygon.Length;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];
                if ((a.y > point.y) != (b.y > point.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSqr = ab.x * ab.x + ab.y * ab.y;
            if (lengthSqr <= 0.0001f)
            {
                return Vector2.Distance(point, a);
            }
            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSqr);
            return Vector2.Distance(point, a + ab * t);
        }
    }
}