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

        /// <summary>Hidden clone of the game's consent dialog Yes/No button.</summary>
        public static GameObject ConsentButtonTemplate;

        /// <summary>Native size of the consent Yes/No button (e.g. ~170x44).</summary>
        public static Vector2 ConsentButtonSize = new Vector2(170f, 44f);

        /// <summary>
        /// Hidden clone of the game's own vertical scrollbar (the settings
        /// pages). Its event is cleared so clones can be wired to the
        /// browser's scroll rects.
        /// </summary>
        public static Scrollbar ScrollbarTemplate;

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

                // ---- Consent dialog button ----
                // The Saves page's consent blockers ("Reload/Wipe Consent
                // Blocker" -> Consent -> Panel -> Yes/No) carry the game's own
                // confirmation buttons; one of them becomes the template for the
                // browser's confirm dialogs (item 25).
                Transform consentButton = FindConsentButton(optionsParent);
                if (consentButton != null)
                {
                    // Measure while the button is still parented inside the
                    // game's consent panel: the detached template clone has no
                    // parent, and MeasureEffectiveSize falls back to the raw
                    // sizeDelta there, which for stretched anchors holds only
                    // the offsets instead of the real button size.
                    RectTransform originalRect = consentButton.GetComponent<RectTransform>();
                    if (originalRect != null)
                    {
                        ConsentButtonSize = MeasureEffectiveSize(originalRect);
                    }

                    ConsentButtonTemplate = Object.Instantiate(consentButton.gameObject);
                    ConsentButtonTemplate.name = "UllBrowser_ConsentButtonTemplate";
                    ConsentButtonTemplate.SetActive(false);

                    Logging.Info("[GameUiAssets] Extracted native consent button (size "
                        + ConsentButtonSize.x + "x" + ConsentButtonSize.y + ").");
                }
                else
                {
                    Logging.Warn("[GameUiAssets] Consent Yes/No button not found; confirm dialogs will use UiFactory styling.");
                }

                // ---- Native scrollbar ----
                // The settings pages scroll through Unity ScrollRects whose
                // scrollbar carries the game's own visuals. One of them becomes
                // the template for the browser's lists and preview (item 12).
                Scrollbar nativeScrollbar = FindNativeScrollbar(optionsParent);
                if (nativeScrollbar != null)
                {
                    ScrollbarTemplate = Object.Instantiate(nativeScrollbar);
                    ScrollbarTemplate.name = "UllBrowser_ScrollbarTemplate";
                    ScrollbarTemplate.gameObject.SetActive(false);

                    // The game's scrollbar drives its own scroll rect through its
                    // event; the template must not keep that listener, the clones
                    // are wired to the browser's scroll rects instead.
                    ScrollbarTemplate.onValueChanged = new Scrollbar.ScrollEvent();

                    Logging.Info("[GameUiAssets] Extracted native scrollbar template.");
                }
                else
                {
                    Logging.Warn("[GameUiAssets] Native scrollbar not found; the browser keeps the flat scrollbar styling.");
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

        private static Transform FindConsentButton(Transform optionsParent)
        {
            // The consent panels (Saves page) contain "Yes"/"No" buttons with a
            // Text child. Search the whole options menu recursively (including
            // inactive objects) so nesting changes stay safe.
            Transform[] all = optionsParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t.name == "Yes" && t.GetComponent<Button>() != null)
                {
                    return t;
                }
            }
            return null;
        }

        private static Scrollbar FindNativeScrollbar(Transform optionsParent)
        {
            // Any vertical scrollbar of the settings pages carries the game's
            // own art. Search the whole options menu recursively (including
            // inactive objects); the first vertical one wins.
            Scrollbar[] all = optionsParent.GetComponentsInChildren<Scrollbar>(true);
            foreach (Scrollbar bar in all)
            {
                if (bar == null)
                {
                    continue;
                }
                if (bar.direction == Scrollbar.Direction.BottomToTop ||
                    bar.direction == Scrollbar.Direction.TopToBottom)
                {
                    return bar;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the rect's effective size, valid for both fixed and stretched
        /// anchors. For stretched anchors <see cref="RectTransform.sizeDelta"/> alone
        /// holds only the offsets from the anchor-driven size, which would
        /// mis-size the cloned button.
        /// </summary>
        private static Vector2 MeasureEffectiveSize(RectTransform rect)
        {
            if (rect == null)
            {
                return Vector2.zero;
            }

            RectTransform parentRect = rect.parent as RectTransform;
            bool stretchedX = rect.anchorMin.x != rect.anchorMax.x;
            bool stretchedY = rect.anchorMin.y != rect.anchorMax.y;
            if ((stretchedX || stretchedY) && parentRect != null)
            {
                Vector2 parentSize = parentRect.rect.size;
                return new Vector2(
                    rect.sizeDelta.x + (stretchedX ? (rect.anchorMax.x - rect.anchorMin.x) * parentSize.x : 0f),
                    rect.sizeDelta.y + (stretchedY ? (rect.anchorMax.y - rect.anchorMin.y) * parentSize.y : 0f));
            }
            return rect.sizeDelta;
        }
    }
}