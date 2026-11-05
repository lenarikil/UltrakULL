using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UltrakULL.json;
using UltrakULL.UI.Notifications;
using UltrakULL.UI.TranslationBrowser;
using static UltrakULL.CommonFunctions;
using System.Linq;
using UnityEngine.EventSystems;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(OptionsMenuToManager), "Start")]
    public static class InjectLanguageButton
    {
        public static TextMeshProUGUI languageButtonText;
        public static TextMeshProUGUI languagePageTitleText;

        private static List<GameObject> languageButtons = new List<GameObject>();

        private static GameObject langLocalPage;
        private static GameObject referenceButtonTemplate;

        // Exposed so the navigation patches below can react to page/highlight changes
        // made by the game itself or by other mods (e.g. PluginConfigurator).
        internal static GameObject LangPage;
        internal static Image LangNavButtonImage;
        private static ButtonHighlightParent langNavHighlightParent;
        private static Transform navigationRailTransform;

        // The game's ButtonHighlightParent stores the active-state visuals privately and,
        // when 'pressedVersion' is set, swaps sprites instead of using Image.fillCenter.
        private static Sprite langNavNormalSprite;
        private static Sprite langNavPressedSprite;

        private static readonly FieldInfo ButtonsField =
            typeof(ButtonHighlightParent).GetField("buttons", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo ButtonTextsField =
            typeof(ButtonHighlightParent).GetField("buttonTexts", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo ButtonSpritesField =
            typeof(ButtonHighlightParent).GetField("buttonSprites", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo PressedVersionField =
            typeof(ButtonHighlightParent).GetField("pressedVersion", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void EnsureReferenceButtonTemplate()
        {
            if (referenceButtonTemplate != null)
                return;

            var optionsParent = GetInactiveRootObject("Canvas").Child("OptionsMenu").transform;
            var navigationRail = optionsParent.Find("Navigation Rail").gameObject;
            // Clone a real navigation item (not the narrow "Back" button) so the language
            // button matches the other page buttons, including its highlight behaviour.
            var buttonPrefab = navigationRail.Child("General");

            referenceButtonTemplate = GameObject.Instantiate(buttonPrefab);
            referenceButtonTemplate.name = "ReferenceButtonTemplate";
            referenceButtonTemplate.SetActive(false);
        }
        /// <summary>
        /// Refactored utility method for creating consistent TMP buttons based on a reference button.
        /// </summary>
        public static class ButtonUtils
        {
            private static readonly ColorBlock defaultColorBlock = new ColorBlock
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(0.5094f, 0.5094f, 0.5094f, 1f),
                pressedColor = new Color(1f, 0f, 0f, 1f),
                selectedColor = new Color(0.5094f, 0.5094f, 0.5094f, 1f),
                disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            public static GameObject CreateTMPButton(
                Transform parent,
                string name,
                string labelText,
                Action onClick,
                Color? buttonColor = null,
                Vector2? size = null,
                bool richText = true,
                bool changeSize = true,
                bool addHighlightSupport = false)
            {
                EnsureReferenceButtonTemplate();
                GameObject buttonObj = GameObject.Instantiate(referenceButtonTemplate, parent);
                if (buttonObj.GetComponent<HudOpenEffect>() == null)
                {
                    buttonObj.AddComponent<HudOpenEffect>();
                }
                buttonObj.name = name;

                // Reset position/rotation/scale
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.localPosition = Vector3.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;

                if (buttonColor.HasValue)
                {
                    Image img = buttonObj.GetComponent<Image>();
                    if (img != null)
                        img.color = buttonColor.Value;
                }

                Button button = buttonObj.GetComponent<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                if (onClick != null)
                    button.onClick.AddListener(() => onClick());

                button.interactable = true;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = defaultColorBlock;
                button.navigation = new Navigation { mode = Navigation.Mode.None };

                TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = labelText;
                    text.richText = richText;
                    text.alignment = TextAlignmentOptions.Center;
                    text.font = Core.GlobalFontTMP;
                    Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(text);

                    if (changeSize)
                    {
                        text.enableAutoSizing = true;
                        text.fontSizeMin = 10f;
                        text.fontSizeMax = 36f;
                    }
                }

                if (changeSize && size.HasValue)
                {
                    rect.sizeDelta = size.Value;
                }

                buttonObj.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                if (addHighlightSupport)
                {
                    var buttonImage = buttonObj.GetComponent<Image>();
                    var highlightParent = buttonObj.GetComponentInParent<ButtonHighlightParent>();
                    if (highlightParent != null && buttonImage != null)
                    {
                        // Add click reaction to this button
                        button.onClick.AddListener(() => highlightParent.ChangeButton(buttonImage));
                    }
                }

                return buttonObj;
            }
        }


        public static void updateLanguageButtonText()
        {
            if (languageButtonText != null)
            {
                languageButtonText.text = LanguageManager.CurrentLanguage.options.language_languages;
            }
            if (languagePageTitleText != null)
            {
                languagePageTitleText.text = "--" + LanguageManager.CurrentLanguage.options.language_languages + "--";
            }
        }

        /// <summary>
        /// Hides the Language page (if open) and clears the navigation highlight on its
        /// button. Safe to call repeatedly and when the page was never created.
        /// </summary>
        internal static void HideLanguagePage()
        {
            if (langLocalPage != null && langLocalPage.activeSelf)
            {
                langLocalPage.SetActive(false);
            }
            SetLanguageNavHighlight(false);
        }

        internal static bool IsLanguageNavButton(Image target)
        {
            return LangNavButtonImage != null && target == LangNavButtonImage;
        }

        /// <summary>
        /// Shrinks the Language page so its left edge starts to the right of the game's
        /// navigation rail. The page is cloned from the General page rect, which spans the
        /// full options width; without this the browser would sit on top of the game's
        /// settings-page buttons and block them. Called every time the page is opened,
        /// after the menu has been laid out.
        /// </summary>
        private static void FitLanguagePageToRail()
        {
            if (langLocalPage == null || navigationRailTransform == null)
            {
                return;
            }

            RectTransform pageRect = langLocalPage.GetComponent<RectTransform>();
            RectTransform railRect = navigationRailTransform.GetComponent<RectTransform>();
            RectTransform parentRect = pageRect != null ? pageRect.parent as RectTransform : null;
            if (pageRect == null || railRect == null || parentRect == null)
            {
                return;
            }

            // Make sure the serialized rects reflect the current layout before measuring.
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(railRect);

            Vector3[] railCorners = new Vector3[4];
            railRect.GetWorldCorners(railCorners);
            // corners[2] is the top-right corner; project it into the page's parent space.
            Vector3 railRightLocal = parentRect.InverseTransformPoint(railCorners[2]);

            // Distance from the parent's left edge to the rail's right edge (+ a small gap).
            float leftInset = railRightLocal.x - parentRect.rect.xMin + 8f;
            if (leftInset < 0f)
            {
                leftInset = 0f;
            }

            pageRect.anchorMin = new Vector2(0f, 0f);
            pageRect.anchorMax = new Vector2(1f, 1f);
            pageRect.pivot = new Vector2(0.5f, 0.5f);
            pageRect.offsetMin = new Vector2(leftInset, 0f);
            pageRect.offsetMax = new Vector2(0f, 0f);

            Logging.Message("[LanguagePage] parent width=" + parentRect.rect.width
                + " rail right=" + railRightLocal.x + " inset=" + leftInset);
        }

        /// <summary>
        /// Mirrors the game's ButtonHighlightParent visuals (filled background + black text
        /// for the active page, normal otherwise) for the language navigation button, which
        /// is created after the game has already captured its own button list.
        /// </summary>
        internal static void SetLanguageNavHighlight(bool active)
        {
            if (LangNavButtonImage != null)
            {
                // Mirror the game's own highlight: it either swaps to the "pressed" sprite
                // (when the navigation rail defines one) or fills the sliced sprite centre.
                // The button's Image always fills its sliced sprite centre while the page is
                // active, so keep fillCenter in sync as well.
                if (langNavPressedSprite != null)
                {
                    LangNavButtonImage.sprite = active ? langNavPressedSprite : langNavNormalSprite;
                }
                LangNavButtonImage.fillCenter = active;
            }
            if (languageButtonText != null)
            {
                languageButtonText.color = active ? Color.black : Color.white;
            }
        }

        /// <summary>
        /// Adds the Language button to the game's ButtonHighlightParent list. That list is
        /// captured in its Start and only contains the buttons that existed at that moment;
        /// our button is created later (or earlier) than Start, so without this the game
        /// would never clear the previously-active page button nor highlight ours. When the
        /// list is not built yet, Start will pick the button up on its own.
        /// </summary>
        private static void RegisterNavButtonWithHighlighter(ButtonHighlightParent highlighter, Image image)
        {
            if (highlighter == null || image == null)
            {
                return;
            }

            if (ButtonsField == null)
            {
                return;
            }

            langNavPressedSprite = PressedVersionField != null ? PressedVersionField.GetValue(highlighter) as Sprite : null;

            Image[] buttons = ButtonsField.GetValue(highlighter) as Image[];
            TMP_Text[] texts = ButtonTextsField != null ? ButtonTextsField.GetValue(highlighter) as TMP_Text[] : null;
            Sprite[] sprites = ButtonSpritesField != null ? ButtonSpritesField.GetValue(highlighter) as Sprite[] : null;

            // The button was cloned from the General page button and may have inherited its
            // highlighted (pressed) sprite. Find a sibling rail button that is currently in
            // the normal state and borrow its sprite so our button (and its captured
            // "original" sprite) never sticks in the pressed look.
            Sprite normalSprite = null;
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    Image candidate = buttons[i];
                    if (candidate == null || candidate == image)
                    {
                        continue;
                    }
                    if (candidate.transform.parent != image.transform.parent)
                    {
                        continue;
                    }
                    if (langNavPressedSprite != null && candidate.sprite == langNavPressedSprite)
                    {
                        continue;
                    }
                    normalSprite = sprites != null && i < sprites.Length && sprites[i] != null
                        ? sprites[i]
                        : candidate.sprite;
                    break;
                }
            }
            langNavNormalSprite = normalSprite != null ? normalSprite : image.sprite;
            image.sprite = langNavNormalSprite;

            if (buttons == null)
            {
                Logging.Message("[LanguageNav] Highlighter list not built yet; Start will include the button.");
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == image)
                {
                    return;
                }
            }

            int length = buttons.Length;
            Image[] newButtons = new Image[length + 1];
            TMP_Text[] newTexts = new TMP_Text[length + 1];
            Sprite[] newSprites = new Sprite[length + 1];
            for (int i = 0; i < length; i++)
            {
                newButtons[i] = buttons[i];
                newTexts[i] = texts != null ? texts[i] : null;
                newSprites[i] = sprites != null ? sprites[i] : null;
            }
            newButtons[length] = image;
            newTexts[length] = image.GetComponentInChildren<TMP_Text>();
            newSprites[length] = image.sprite;

            ButtonsField.SetValue(highlighter, newButtons);
            if (ButtonTextsField != null)
            {
                ButtonTextsField.SetValue(highlighter, newTexts);
            }
            if (ButtonSpritesField != null)
            {
                ButtonSpritesField.SetValue(highlighter, newSprites);
            }

            Logging.Message("[LanguageNav] Registered language nav button with the highlighter (total=" + (length + 1) + ").");
        }

        /// <summary>
        /// Replicates ButtonHighlightParent.ChangeButton over the list the game captured in
        /// its Start, then applies the same visual to our own button (which may not be part
        /// of that list). Doing the loop ourselves guarantees that every other navigation
        /// button loses its active look even if our button was created after the game
        /// captured its buttons, and avoids a NullReferenceException when the list has not
        /// been built yet (nothing is highlighted at that point anyway).
        /// </summary>
        internal static void ApplyNavHighlight(Image target)
        {
            ButtonHighlightParent highlighter = langNavHighlightParent != null
                ? langNavHighlightParent
                : (navigationRailTransform != null
                    ? navigationRailTransform.GetComponentInParent<ButtonHighlightParent>()
                    : null);

            Image[] buttons = null;
            TMP_Text[] texts = null;
            Sprite[] sprites = null;
            Sprite pressed = null;
            if (highlighter != null && ButtonsField != null)
            {
                pressed = PressedVersionField != null ? PressedVersionField.GetValue(highlighter) as Sprite : null;
                buttons = ButtonsField.GetValue(highlighter) as Image[];
                texts = ButtonTextsField != null ? ButtonTextsField.GetValue(highlighter) as TMP_Text[] : null;
                sprites = ButtonSpritesField != null ? ButtonSpritesField.GetValue(highlighter) as Sprite[] : null;
            }

            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    Image button = buttons[i];
                    if (button == null)
                    {
                        continue;
                    }

                    bool isTarget = button == target;
                    if (pressed != null)
                    {
                        button.sprite = isTarget
                            ? pressed
                            : (sprites != null && i < sprites.Length && sprites[i] != null ? sprites[i] : button.sprite);
                    }
                    else
                    {
                        button.fillCenter = isTarget;
                    }

                    TMP_Text text = texts != null && i < texts.Length ? texts[i] : button.GetComponentInChildren<TMP_Text>();
                    if (text != null)
                    {
                        text.color = isTarget ? Color.black : Color.white;
                    }
                }
            }

            // Our button is not necessarily part of the captured list; make sure it ends up
            // in the active state regardless. Its Image always fills in while the page is
            // open, even when the rail highlights the other buttons through sprite swaps.
            if (target != null)
            {
                if (pressed != null)
                {
                    target.sprite = pressed;
                }
                target.fillCenter = true;
                TMP_Text ourText = target.GetComponentInChildren<TMP_Text>();
                if (ourText != null)
                {
                    ourText.color = Color.black;
                }
            }

            Logging.Message("[LanguageNav] ApplyNavHighlight captured=" + (buttons != null ? buttons.Length : -1)
                + " pressed=" + (pressed != null) + " targetRegistered="
                + (buttons != null && Array.IndexOf(buttons, target) >= 0));
        }

        public static bool Prefix(OptionsMenuToManager __instance)
        {
            languageButtons.Clear();

            Logging.Message("Adding language option to options menu...");

            Transform optionsParent = __instance.optionsMenu.transform;
            Transform navigationRail = optionsParent.Find("Navigation Rail");
            navigationRailTransform = navigationRail;
            Transform pagesParent = optionsParent.Find("Pages");
            Transform generalPage = pagesParent.Find("General");
            RectTransform generalPageRect = generalPage.GetComponent<RectTransform>();

            // Native button/toggle templates from the game for a consistent look.
            GameUiAssets.EnsureInitialized();

            // The browser is embedded directly into this page so the game's left
            // navigation rail stays visible and clickable (no full-screen overlay).
            // The page rect matches the General page, so the browser sits exactly in
            // the options content area.
            langLocalPage = new GameObject("Language Page", typeof(RectTransform), typeof(CanvasRenderer));
            langLocalPage.transform.SetParent(pagesParent, false);
            // The same open animation the game's own panels and buttons play
            // (item 23); the component animates on enable, so re-opening the
            // page replays it.
            langLocalPage.AddComponent<HudOpenEffect>();
            langLocalPage.SetActive(false);
            RectTransform pageRect = langLocalPage.GetComponent<RectTransform>();
            pageRect.anchorMin = generalPageRect.anchorMin;
            pageRect.anchorMax = generalPageRect.anchorMax;
            pageRect.pivot = generalPageRect.pivot;
            pageRect.anchoredPosition = generalPageRect.anchoredPosition;
            pageRect.sizeDelta = generalPageRect.sizeDelta;
            LangPage = langLocalPage;

            BrowserHost.Embed(langLocalPage.transform, delegate
            {
                HideLanguagePage();
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            });

            Logging.Message("Creating language menu button...");
            GameObject languageButton = ButtonUtils.CreateTMPButton(navigationRail, "Language", LanguageManager.CurrentLanguage.options.language_title, () => ShowLanguagePage(), changeSize: false);
            languageButtonText = languageButton.Child("Text").TMP();
            RectTransform sourceRect = navigationRail.gameObject.Child("General").GetComponent<RectTransform>();
            RectTransform targetRect = languageButton.GetComponent<RectTransform>();
            targetRect.sizeDelta = sourceRect.sizeDelta;
            languageButton.transform.SetSiblingIndex(7);

            LangNavButtonImage = languageButton.GetComponent<Image>();
            langNavHighlightParent = languageButton.GetComponentInParent<ButtonHighlightParent>();
            if (langNavHighlightParent == null && navigationRailTransform != null)
            {
                langNavHighlightParent = navigationRailTransform.GetComponentInParent<ButtonHighlightParent>();
            }
            Logging.Message("[LanguageNav] highlighter=" + (langNavHighlightParent != null));
            RegisterNavButtonWithHighlighter(langNavHighlightParent, LangNavButtonImage);
            // The template clone may inherit the highlighted state of the source button.
            SetLanguageNavHighlight(false);

            Logging.Message("Setting up navigation buttons to hide language page...");
            foreach (Transform child in navigationRail)
            {
                if (child.name != "Language" && child.name != "Saves")
                {
                    Button navButton = child.GetComponent<Button>();
                    if (navButton != null)
                    {
                        navButton.onClick.AddListener(() => HideLanguagePage());
                    }
                }
            }

            void ShowLanguagePage()
            {
                if (langLocalPage != null && langLocalPage.activeSelf)
                {
                    // Already open: only refresh the highlight, never rebuild the page.
                    ApplyLanguageNavHighlight();
                    return;
                }

                Logging.Message("Opening translation browser...");
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
                foreach (Transform page in pagesParent)
                {
                    if (page != null && page.gameObject != langLocalPage)
                    {
                        page.gameObject.SetActive(false);
                    }
                }
                if (langLocalPage != null)
                {
                    FitLanguagePageToRail();
                    langLocalPage.SetActive(true);
                }
                ApplyLanguageNavHighlight();

                // Offer the product tour only when the mod page is actually opened,
                // never on construction/game start (item 13.5).
                var browser = UltrakULL.UI.TranslationBrowser.BrowserHost.CurrentView;
                if (browser != null)
                {
                    browser.ShowWelcomeOnce();
                }
            }

            void ApplyLanguageNavHighlight()
            {
                // Clear every other navigation button and highlight ours. We replicate the
                // game's ChangeButton loop ourselves so it works even though our button is
                // created after the game captured its button list.
                ApplyNavHighlight(LangNavButtonImage);
            }

            Logging.Info("Language nav button added; the browser is embedded in the Language page.");
            return true;
        }
    }

    /// <summary>
    /// A change of the navigation highlight (a game page button, or another mod's button such
    /// as PluginConfigurator's) means the user left the Language page, so hide it. Our own
    /// button is exempt, so opening/re-opening the page does not close it.
    /// </summary>
    [HarmonyPatch(typeof(global::ButtonHighlightParent), "ChangeButton")]
    public static class LanguageNavHighlightPatch
    {
        private static void Postfix(Image target)
        {
            if (!InjectLanguageButton.IsLanguageNavButton(target))
            {
                InjectLanguageButton.HideLanguagePage();
            }
        }
    }

    /// <summary>
    /// The game switches settings pages through SettingsMenu.SetActivePage. When it targets a
    /// different page, hide ours as well, otherwise the Language page would stay visible on
    /// top of the page the game just opened.
    /// </summary>
    [HarmonyPatch(typeof(global::SettingsMenu.Components.SettingsMenu), "SetActivePage")]
    public static class LanguagePageDeactivatePatch
    {
        private static void Postfix(GameObject targetPage)
        {
            if (targetPage != InjectLanguageButton.LangPage)
            {
                InjectLanguageButton.HideLanguagePage();
            }
        }
    }
}
