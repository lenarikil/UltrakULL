using System.Collections.Generic;
using UnityEngine;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Minimalist monochrome icons generated at runtime as white-on-transparent
    /// textures. They are tinted via <see cref="UnityEngine.UI.Image.color"/>, which
    /// both keeps the ULTRAKILL aesthetic and gives the capability state colors.
    /// The generated set avoids relying on font glyph coverage (CJK/Arabic etc.).
    /// </summary>
    public static class PrimitiveSprites
    {
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
            Diamond,      // small accent
            TriangleUp,   // expand / sort direction
            ChevronDown   // collapse / dropdown
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

            Sprite sprite = Create(kind);
            Cache[kind] = sprite;
            return sprite;
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
                case Kind.Diamond: PaintPolygon(paint, new Vector2[] { new Vector2(15.5f, 3), new Vector2(29, 15.5f), new Vector2(15.5f, 28), new Vector2(2, 15.5f) }); break;
                case Kind.TriangleUp: PaintPolygon(paint, new Vector2[] { new Vector2(15.5f, 3), new Vector2(28, 27), new Vector2(3, 27) }); break;
                case Kind.ChevronDown: PaintPolygon(paint, new Vector2[] { new Vector2(5, 12), new Vector2(15.5f, 25), new Vector2(26, 12), new Vector2(22, 8), new Vector2(15.5f, 17), new Vector2(9, 8) }); break;
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
            // Open ring (gap in the lower-right quadrant)
            float cx = 15.5f;
            float cy = 15.5f;
            for (int x = 2; x < 30; x++)
            {
                for (int y = 2; y < 30; y++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < 6.2f || dist > 9.8f)
                    {
                        continue;
                    }
                    float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (ang < -140f && ang > -70f)
                    {
                        continue; // the gap
                    }
                    paint(x, y);
                }
            }
            // Directional arrow at the end of the arc
            PaintPolygon(paint, new Vector2[]
            {
                new Vector2(23, 8.5f),
                new Vector2(28, 14),
                new Vector2(20.5f, 14)
            });
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

        private static void DrawArc(System.Action<int, int> paint, float cx, float cy, float radius, float thickness, float angleStart, float angleEnd)
        {
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