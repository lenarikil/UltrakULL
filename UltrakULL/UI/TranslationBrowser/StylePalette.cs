using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// ULTRAKILL-style color palette shared by the Thorn and the fallback browser UI.
    /// Near-black background, dark panels with thin blue outlines, red accents for
    /// active/primary elements, and the unified capability status color scale.
    /// </summary>
    public static class StylePalette
    {
        // Base surfaces
        public static readonly Color Background = new Color32(6, 6, 10, 255);
        public static readonly Color Panel = new Color32(13, 13, 18, 255);
        public static readonly Color PanelAlt = new Color32(18, 18, 26, 255);
        public static readonly Color PanelHover = new Color32(26, 26, 38, 255);

        // Outlines
        public static readonly Color BorderBlue = new Color32(52, 94, 180, 255);
        public static readonly Color BorderDim = new Color32(34, 42, 58, 255);

        // Accents
        public static readonly Color AccentRed = new Color32(224, 45, 45, 255);
        public static readonly Color AccentRedDark = new Color32(122, 22, 22, 255);
        public static readonly Color AccentCyan = new Color32(52, 225, 235, 255);

        // Text
        public static readonly Color TextPrimary = new Color32(230, 232, 236, 255);
        public static readonly Color TextSecondary = new Color32(154, 163, 173, 255);
        public static readonly Color TextDim = new Color32(92, 102, 114, 255);

        // Translucent HUD-safe overlays
        public static readonly Color Scrim = new Color32(0, 0, 0, 180);

        /// <summary>
        /// Unified capability status color. Used by the list cards, the details panel
        /// and the legend so the meaning stays consistent across the whole UI.
        /// </summary>
        public static Color FeatureColor(FeatureStatus status)
        {
            switch (status)
            {
                case FeatureStatus.Full:
                    return new Color32(76, 175, 96, 255);   // green: fully implemented
                case FeatureStatus.Partial:
                    return new Color32(232, 193, 77, 255);  // yellow: partially implemented
                case FeatureStatus.None:
                    return new Color32(118, 122, 132, 255); // gray: not present
                case FeatureStatus.Broken:
                    return new Color32(224, 75, 75, 255);   // red: unavailable / known issue
                default:
                    return new Color32(96, 118, 145, 255);  // neutral: no information
            }
        }

        /// <summary>Short English status label used in tooltips (accessibility: not color-only).</summary>
        public static string FeatureLabel(FeatureStatus status)
        {
            switch (status)
            {
                case FeatureStatus.Full:
                    return "Full";
                case FeatureStatus.Partial:
                    return "Partial";
                case FeatureStatus.None:
                    return "Not included";
                case FeatureStatus.Broken:
                    return "Unavailable / known issue";
                default:
                    return "No data from the source";
            }
        }

        /// <summary>Feature display names shared by cards, details and legend.</summary>
        public static string FeatureDisplayName(TranslationFeatureType feature)
        {
            switch (feature)
            {
                case TranslationFeatureType.Dubbing:
                    return "Voice dubbing";
                case TranslationFeatureType.ExtraDubbing:
                    return "Extra dubbing";
                case TranslationFeatureType.Textures:
                    return "Textures";
                default:
                    return "Fonts";
            }
        }
    }

    /// <summary>Identifier of one of the four capability flags.</summary>
    public enum TranslationFeatureType
    {
        Dubbing,
        ExtraDubbing,
        Textures,
        Fonts
    }
}