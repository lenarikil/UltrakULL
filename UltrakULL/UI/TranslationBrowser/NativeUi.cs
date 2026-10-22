using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Loads the game's own rounded control sprites through Addressables so the
    /// fallback browser can render buttons, panels and dropdowns with the exact
    /// ULTRAKILL SettingsMenu art instead of flat rectangles. The sprite keys are
    /// the Addressables addresses baked into the shipped game catalogs; they are
    /// the same ones the game's SettingsMenu prefabs use. Loading is lazy and
    /// best-effort: if Addressables is unavailable (or a key is missing) the
    /// callers fall back to their previous flat rendering.
    /// </summary>
    public static class NativeUi
    {
        private const string BorderLargeKey = "Assets/Textures/UI/Controls/Round_BorderLarge.png";
        private const string BorderSmallKey = "Assets/Textures/UI/Controls/Round_BorderSmall.png";
        private const string FillLargeKey = "Assets/Textures/UI/Controls/Round_FillLarge.png";
        private const string FillSmallKey = "Assets/Textures/UI/Controls/Round_FillSmall.png";
        private const string CheckKey = "Assets/Textures/UI/Controls/Check.png";
        private const string DropdownPanelKey = "Assets/Textures/UI/Controls/Round_DropdownPanel.png";

        /// <summary>Slice multiplier the game uses for these controls (1 Unity unit / 5.4 px).</summary>
        public const float PixelsPerUnitMultiplier = 5.4f;

        public static Sprite BorderLarge { get; private set; }
        public static Sprite BorderSmall { get; private set; }
        public static Sprite FillLarge { get; private set; }
        public static Sprite FillSmall { get; private set; }
        public static Sprite Check { get; private set; }
        public static Sprite DropdownPanel { get; private set; }

        private static bool _initialized;

        /// <summary>True once the native border/fill sprites are available.</summary>
        public static bool IsReady
        {
            get { return _initialized && FillLarge != null && BorderLarge != null; }
        }

        public static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            // Only latch on success: Addressables may not be ready during the very
            // first options-menu build, and we want later calls to retry instead of
            // permanently falling back to the flat rendering.
            Sprite borderLarge = Load(BorderLargeKey);
            Sprite borderSmall = Load(BorderSmallKey);
            Sprite fillLarge = Load(FillLargeKey);
            Sprite fillSmall = Load(FillSmallKey);
            Sprite check = Load(CheckKey);
            Sprite dropdown = Load(DropdownPanelKey);

            if (borderLarge == null || fillLarge == null)
            {
                return;
            }

            BorderLarge = borderLarge;
            BorderSmall = borderSmall;
            FillLarge = fillLarge;
            FillSmall = fillSmall;
            Check = check;
            DropdownPanel = dropdown;
            _initialized = true;
        }

        private static Sprite Load(string key)
        {
            try
            {
                return Addressables.LoadAssetAsync<Sprite>((object)key).WaitForCompletion();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[UltrakULL] NativeUi failed to load native sprite '" + key + "': " + ex.Message);
                return null;
            }
        }

        /// <summary>Applies a native 9-slice sprite to an <see cref="Image"/>.</summary>
        public static void ApplySliced(Image image, Sprite sprite, Color color)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = PixelsPerUnitMultiplier;
            image.color = color;
        }

        /// <summary>Applies the native fill background (rounded solid) to an <see cref="Image"/>.</summary>
        public static void ApplyFill(Image image, Color color)
        {
            ApplySliced(image, FillLarge, color);
        }

        /// <summary>Applies the native border outline to an <see cref="Image"/>.</summary>
        public static void ApplyBorder(Image image, Color color)
        {
            ApplySliced(image, BorderLarge, color);
        }
    }
}
