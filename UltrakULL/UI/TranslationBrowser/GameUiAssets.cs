using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Extracts native ULTRAKILL settings UI assets at runtime so the browser's
    /// buttons/borders match the game's own settings menu. The game builds its
    /// settings rows from prefab clones ("Action Button(Clone)", "Toggle(Clone)"),
    /// so we cache a hidden template plus the extracted 9-slice sprite and colors.
    /// This is more reliable than loading sprites from asset bundles by path
    /// (internal bundle addresses are not known), and it reuses the exact art.
    /// </summary>
    public static class GameUiAssets
    {
        public static bool IsReady { get; private set; }

        /// <summary>Hidden clone of the game's settings action button.</summary>
        public static GameObject ButtonTemplate;

        /// <summary>9-slice sprite used by the native settings buttons.</summary>
        public static Sprite ButtonSprite;

        /// <summary>Image.Type of the native button (usually Sliced).</summary>
        public static Image.Type ButtonImageType = Image.Type.Sliced;

        /// <summary>Native button size (e.g. ~400x40).</summary>
        public static Vector2 ButtonSize = new Vector2(400f, 40f);

        /// <summary>ColorBlock of the native button.</summary>
        public static ColorBlock ButtonColors;

        /// <summary>Font and size of the native button label.</summary>
        public static TMP_FontAsset ButtonFont;

        public static float ButtonFontSize = 18f;

        /// <summary>Hidden clone of the game's settings toggle (for the Settings tab).</summary>
        public static GameObject ToggleTemplate;

        /// <summary>
        /// Scans the options menu hierarchy for native UI assets. Safe to call
        /// repeatedly; does nothing after the first successful pass. Missing assets
        /// degrade gracefully: buttons fall back to UiFactory styling.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (IsReady)
            {
                return;
            }

            try
            {
                Transform optionsParent = GetOptionsMenuTransform();
                if (optionsParent == null)
                {
                    return;
                }

                // ---- Action button ----
                Transform actionButton = FindActionButton(optionsParent);
                if (actionButton != null)
                {
                    ButtonTemplate = Object.Instantiate(actionButton.gameObject);
                    ButtonTemplate.name = "UllBrowser_ActionButtonTemplate";
                    ButtonTemplate.SetActive(false);

                    RectTransform rect = ButtonTemplate.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        ButtonSize = rect.sizeDelta;
                    }

                    Image image = ButtonTemplate.GetComponent<Image>();
                    if (image != null)
                    {
                        ButtonSprite = image.sprite;
                        ButtonImageType = image.type;
                    }

                    Button button = ButtonTemplate.GetComponent<Button>();
                    if (button != null)
                    {
                        ButtonColors = button.colors;
                    }

                    TextMeshProUGUI label = ButtonTemplate.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        // The mod's own UI always uses the bundled font. The font of
                        // the game's label changes with the active language: a
                        // captured reference would point at the custom font of a
                        // language that is replaced on the next switch, and the
                        // buttons cloned from the template would render from a font
                        // that no language controls anymore.
                        ButtonFont = Core.GlobalFontTMP ?? label.font;
                        ButtonFontSize = label.fontSize;
                    }

                    Logging.Info("[GameUiAssets] Extracted native action button (size " + ButtonSize.x + "x" + ButtonSize.y + ").");
                }
                else
                {
                    Logging.Warn("[GameUiAssets] Action Button(Clone) not found; buttons will use UiFactory styling.");
                }

                // ---- Toggle ----
                Transform toggle = FindToggle(optionsParent);
                if (toggle != null)
                {
                    ToggleTemplate = Object.Instantiate(toggle.gameObject);
                    ToggleTemplate.name = "UllBrowser_ToggleTemplate";
                    ToggleTemplate.SetActive(false);
                    Logging.Info("[GameUiAssets] Extracted native toggle template.");
                }

                IsReady = true;
            }
            catch (System.Exception e)
            {
                Logging.Warn("[GameUiAssets] Failed to extract native UI assets: " + e.Message);
            }
        }

        private static Transform GetOptionsMenuTransform()
        {
            // The options menu lives under the main Canvas; it can be inactive
            // (the game keeps it in the scene while on the pause/main menu).
            GameObject canvas = UltrakULL.CommonFunctions.GetInactiveRootObject("Canvas");
            if (canvas == null)
            {
                return null;
            }

            Transform options = canvas.transform.Find("OptionsMenu");
            return options;
        }

        private static Transform FindActionButton(Transform optionsParent)
        {
            // The game instantiates action buttons as "Action Button(Clone)" rows
            // anywhere in the settings pages. Search recursively across the whole
            // options menu (including inactive objects) so nesting changes are safe.
            Transform[] all = optionsParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t.name.IndexOf("Action Button", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return t;
                }
            }
            return null;
        }

        private static Transform FindToggle(Transform optionsParent)
        {
            // Settings toggles are instantiated as "Toggle(Clone)" inside rows.
            Transform[] all = optionsParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t.name.IndexOf("Toggle(Clone)", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return t;
                }
            }
            return null;
        }
    }
}