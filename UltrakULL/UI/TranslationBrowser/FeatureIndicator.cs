using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>Container of a single capability indicator.</summary>
    public sealed class FeatureIndicatorView
    {
        public GameObject Root;
        public Image Icon;               // null when the feature is shown as text ("Aa")
        public TextMeshProUGUI Label;    // "Aa" glyph for the Fonts feature
        public TranslationFeatureType Type;
        public FeatureStatus Status;
    }

    /// <summary>
    /// Compact capability badge used on cards, in the details panel and in the legend.
    /// The color encodes the state (green/yellow/gray/red) and a hover tooltip always
    /// states the feature name + state so the meaning is never color-only.
    /// </summary>
    public static class FeatureIndicator
    {
        /// <summary>
        /// Creates one indicator of <paramref name="size"/>x<paramref name="size"/>.
        /// The Fonts feature is rendered as the "Aa" glyph, the rest as icon sprites.
        /// </summary>
        public static FeatureIndicatorView Create(
            Transform parent,
            string name,
            TranslationFeatureType type,
            FeatureStatus status,
            float size,
            bool showTooltip = true)
        {
            Color color = StylePalette.FeatureColor(status);

            FeatureIndicatorView view = new FeatureIndicatorView { Type = type, Status = status };

            if (type == TranslationFeatureType.Fonts)
            {
                TextMeshProUGUI label = UiFactory.CreateLabel(parent, name, "Aa", size * 0.62f, color, TextAlignmentOptions.Center, false);
                label.rectTransform.sizeDelta = new Vector2(size, size);
                view.Root = label.gameObject;
                view.Label = label;
                if (showTooltip)
                {
                    TooltipService.Attach(label, BuildTooltip(type, status));
                }
            }
            else
            {
                PrimitiveSprites.Kind kind;
                switch (type)
                {
                    case TranslationFeatureType.Dubbing:
                        kind = PrimitiveSprites.Kind.Speaker;
                        break;
                    case TranslationFeatureType.ExtraDubbing:
                        kind = PrimitiveSprites.Kind.Star;
                        break;
                    default:
                        kind = PrimitiveSprites.Kind.Image;
                        break;
                }

                Image icon = UiFactory.CreateImage(parent, name, new Vector2(size, size), color, kind);
                icon.raycastTarget = showTooltip;
                view.Root = icon.gameObject;
                view.Icon = icon;
                if (showTooltip)
                {
                    TooltipService.Attach(icon, BuildTooltip(type, status));
                }
            }

            return view;
        }

        /// <summary>Applies a new status color and tooltip to an existing indicator.</summary>
        public static void SetStatus(FeatureIndicatorView view, FeatureStatus status)
        {
            if (view == null)
            {
                return;
            }

            view.Status = status;
            Color color = StylePalette.FeatureColor(status);
            if (view.Icon != null)
            {
                view.Icon.color = color;
            }
            if (view.Label != null)
            {
                view.Label.color = color;
            }
        }

        private static string BuildTooltip(TranslationFeatureType type, FeatureStatus status)
        {
            return StylePalette.FeatureDisplayName(type) + ": " + StylePalette.FeatureLabel(status);
        }
    }
}