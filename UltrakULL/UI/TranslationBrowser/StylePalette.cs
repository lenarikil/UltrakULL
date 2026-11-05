using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// ULTRAKILL-style color palette shared by every translation browser view:
    /// near-black translucent fills, white outlines, red accents for
    /// active/primary elements, and the unified capability status color scale.
    /// All view colors live here - views must not hardcode their own.
    /// </summary>
    public static class StylePalette
    {
        // Base surfaces: pure black with the game's standard HUD fill alpha.
        public static readonly Color Background = new Color32(6, 6, 10, 255);
        public static readonly Color Panel = new Color(0f, 0f, 0f, 0.851f);
        public static readonly Color PanelAlt = new Color(0f, 0f, 0f, 0.88f);
        // Hover/selected fill: a touch of gray so the state stays visible on
        // top of the black panels.
        public static readonly Color PanelHover = new Color(0.13f, 0.13f, 0.13f, 0.88f);

        // Outlines: white (BorderDim is the same white at reduced alpha).
        public static readonly Color BorderBlue = new Color32(255, 255, 255, 255);
        public static readonly Color BorderDim = new Color32(255, 255, 255, 160);

        // Accents
        public static readonly Color AccentRed = new Color32(224, 45, 45, 255);
        public static readonly Color AccentRedDark = new Color32(122, 22, 22, 255);
        public static readonly Color AccentCyan = new Color32(52, 225, 235, 255);
        // Same green as the "Installed" status: marks the pack currently in use.
        public static readonly Color AccentGreen = new Color32(76, 175, 96, 255);

        // Active tab fill (dark red) and its hover variant.
        public static readonly Color TabActiveBg = new Color(0.45f, 0.08f, 0.08f, 1f);

        // Primary action button (Download/Update/Reinstall/Yes).
        public static readonly Color ActionButtonBg = new Color(
            AccentRedDark.r, AccentRedDark.g, AccentRedDark.b, 0.85f);
        public static readonly Color ActionButtonHover = new Color(0.45f, 0.12f, 0.12f, 1f);

        // Download progress bar fill.
        public static readonly Color ProgressFill = new Color(
            AccentRedDark.r, AccentRedDark.g, AccentRedDark.b, 0.8f);

        // Gallery navigation buttons and the failed-image box.
        public static readonly Color GalleryNavBg = new Color(0f, 0f, 0f, 0.55f);
        public static readonly Color GalleryNavHover = new Color(0.1f, 0.15f, 0.25f, 0.75f);
        public static readonly Color GalleryFailure = new Color(0.2f, 0.2f, 0.25f, 0.9f);

        // Install-status badge colors (shared by the card badges and the legend).
        public static readonly Color StatusUpdate = new Color32(232, 161, 77, 255);
        public static readonly Color StatusManual = new Color32(232, 193, 77, 255);

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

        /// <summary>
        /// Badge color of an install status, shared by the card badges and the
        /// status tooltips. Formerly part of the legend panel, which was
        /// replaced by the product tour.
        /// </summary>
        public static Color StatusColor(InstallStatus status)
        {
            switch (status)
            {
                case InstallStatus.Installed:
                    return AccentGreen;
                case InstallStatus.UpdateAvailable:
                    return StatusUpdate;
                case InstallStatus.LanguageInstalledManual:
                    return StatusManual;
                case InstallStatus.NotInstalled:
                    return BorderBlue;
                default:
                    return TextDim;
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
