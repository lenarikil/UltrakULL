using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Right-hand reference panel: explains the icons and color states used across
    /// the browser. This is static documentation, not data of any particular
    /// translation. The content lives in a scroll view so the list of entries can
    /// never overflow the panel or overlap neighbouring sections.
    /// </summary>
    public static class LegendPanel
    {
        public static GameObject Create(Transform parent, string name, float width)
        {
            GameObject panel = UiFactory.CreatePanel(parent, name, new Vector2(width, 0f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UiFactory.ScrollViewLayout scroll = UiFactory.CreateScrollView(panel.transform, "LegendScroll", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            UiFactory.StretchToParent(scroll.Root);

            Transform content = scroll.Content;
            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;

            TextMeshProUGUI title = UiFactory.CreateLabel(content, "Title", "Reference", 16f, StylePalette.AccentCyan, TextAlignmentOptions.Left, false);
            UiFactory.SingleLine(title);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 24f, 0f, 24f, 1f, 0f);

            AddSpacer(content, 4f);
            AddItem(content, "LogoItem", PrimitiveSprites.Kind.Diamond, "Team logo with a language flag in its corner.");
            AddSpacer(content, 2f);
            AddItem(content, "SpeakerItem", PrimitiveSprites.Kind.Speaker, "Main voice dubbing.");
            AddItem(content, "StarItem", PrimitiveSprites.Kind.Star, "Extra (non-essential) dubbing.");
            AddItem(content, "ImageItem", PrimitiveSprites.Kind.Image, "Replaced textures and graphics.");
            FontGlyphItem(content, "FontsItem", "Custom or adapted fonts.");
            AddSpacer(content, 2f);

            TextMeshProUGUI colorsTitle = UiFactory.CreateLabel(content, "ColorsTitle", "Capability colors", 14f, StylePalette.TextSecondary, TextAlignmentOptions.Left, false);
            UiFactory.SingleLine(colorsTitle);
            UiFactory.AddLayoutElement(colorsTitle.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);

            AddColorItem(content, "FullItem", FeatureStatus.Full, "fully implemented.");
            AddColorItem(content, "PartialItem", FeatureStatus.Partial, "partially implemented.");
            AddColorItem(content, "NoneItem", FeatureStatus.None, "not included.");
            AddColorItem(content, "BrokenItem", FeatureStatus.Broken, "unavailable / known issue.");
            AddColorItem(content, "UnknownItem", FeatureStatus.Unknown, "no information from the source.");
            AddSpacer(content, 2f);

            AddItem(content, "VersionItem", PrimitiveSprites.Kind.Diamond, "Version if provided by the authors.");
            AddItem(content, "LikesItem", PrimitiveSprites.Kind.Heart, "Likes reported by the source (read-only).");
            AddItem(content, "ReadmeItem", PrimitiveSprites.Kind.Image, "README with notes, thanks, instructions and warnings.");
            AddItem(content, "DownloadItem", PrimitiveSprites.Kind.Download, "Download button: the only trigger that starts a download.");

            AddSpacer(content, 6f);

            // Note about the data source, pinned low.
            TextMeshProUGUI note = UiFactory.CreateLabel(content, "Note",
                "Data is loaded from the bot message and shown as a preview. Files are never downloaded until you press the download button.",
                11f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, true);
            UiFactory.AddLayoutElement(note.gameObject, 0f, -1f, 0f, -1f, 1f, 0f);

            UiFactory.ForceLayoutHeights(content);

            RectTransform sbRect = UiFactory.CreateScrollbar(panel.transform, "LegendScrollbar", scroll.ScrollRect).GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-2f, 0f);
            sbRect.sizeDelta = new Vector2(10f, 0f);

            return panel;
        }

        private static void AddSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(parent, false);
            UiFactory.AddLayoutElement(spacer, 0f, height, 0f, height, 0f, 0f);
        }

        /// <summary>
        /// One legend row: a fixed icon on the left and a wrapping, full-width text
        /// label on the right. The row has no fixed height, so a long description grows
        /// the row instead of overflowing into the next entry.
        /// </summary>
        private static void AddItem(Transform parent, string name, PrimitiveSprites.Kind kind, string text)
        {
            GameObject row = CreateRow(parent, name);

            Image icon = UiFactory.CreateImage(row.transform, "Icon", new Vector2(16f, 16f), StylePalette.TextSecondary, kind);
            UiFactory.AddLayoutElement(icon.gameObject, 16f, 16f, 16f, 16f, 0f, 0f);

            TextMeshProUGUI label = UiFactory.CreateLabel(row.transform, "Text", text, 12f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            UiFactory.AddLayoutElement(label.gameObject, 0f, -1f, 0f, -1f, 1f, 0f);
        }

        private static void FontGlyphItem(Transform parent, string name, string text)
        {
            GameObject row = CreateRow(parent, name);

            TextMeshProUGUI glyph = UiFactory.CreateLabel(row.transform, "Glyph", "Aa", 14f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, false);
            UiFactory.AddLayoutElement(glyph.gameObject, 20f, 20f, 20f, 20f, 0f, 0f);

            TextMeshProUGUI label = UiFactory.CreateLabel(row.transform, "Text", text, 12f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            UiFactory.AddLayoutElement(label.gameObject, 0f, -1f, 0f, -1f, 1f, 0f);
        }

        private static void AddColorItem(Transform parent, string name, FeatureStatus status, string text)
        {
            GameObject row = CreateRow(parent, name);

            Image swatch = UiFactory.CreateImage(row.transform, "Swatch", new Vector2(12f, 12f), StylePalette.FeatureColor(status), PrimitiveSprites.Kind.Circle);
            UiFactory.AddLayoutElement(swatch.gameObject, 12f, 12f, 12f, 12f, 0f, 0f);

            TextMeshProUGUI label = UiFactory.CreateLabel(row.transform, "Text",
                StylePalette.FeatureLabel(status) + ": " + text,
                11f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            UiFactory.AddLayoutElement(label.gameObject, 0f, -1f, 0f, -1f, 1f, 0f);
        }

        private static GameObject CreateRow(Transform parent, string name)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return row;
        }
    }
}
