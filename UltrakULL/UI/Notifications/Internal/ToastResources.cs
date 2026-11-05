using System;
using TMPro;
using UltrakULL.UI.TranslationBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.Notifications
{
    /// <summary>
    /// Builds the toast prefab programmatically in the mod's UI style
    /// (UiFactory + StylePalette): rounded native panel with a type-colored
    /// border, a left accent strip with a type icon, title/body text and an
    /// optional action button, plus a reverse progress strip at the bottom that
    /// visualizes the remaining display time.
    /// </summary>
    public static class ToastResources
    {
        private static TMP_FontAsset _fallbackTmpFont;
        private static bool _fontLookupFailed;
        private static bool _triedFallback;

        public static TMP_FontAsset GetTmpFont()
        {
            try
            {
                // Prefer the current global font: it changes with the selected language.
                if (Core.GlobalFontTMP != null) return Core.GlobalFontTMP;
                if (_fallbackTmpFont != null) return _fallbackTmpFont;
                // Try to find existing TMP fonts. Only scan once: fonts that exist at
                // startup won't disappear, and repeating the scan on every toast is
                // wasteful.
                if (!_triedFallback)
                {
                    _triedFallback = true;
                    var all = UnityEngine.Object.FindObjectsOfType<TMP_FontAsset>();
                    foreach (var f in all)
                    {
                        if (f != null) { _fallbackTmpFont = f; return f; }
                    }
                    // Last resort: TMP's built-in default font. A null font makes TMP report zero
                    // preferred height, which collapses the toast to zero size (fully invisible).
                    TMP_FontAsset def = TMP_Settings.defaultFontAsset;
                    if (def != null) { _fallbackTmpFont = def; return def; }
                }
            }
            catch (Exception e)
            {
                // Log once: this lookup runs on every toast, so a repeat would flood the log.
                if (!_fontLookupFailed)
                {
                    _fontLookupFailed = true;
                    Logging.Warn("Failed to resolve a TMP font for notifications: " + e.Message);
                }
            }
            return null;
        }

        public static GameObject CreateToastPrefab()
        {
            GameObject root = new GameObject("UltrakULL_Toast", typeof(RectTransform));
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.pivot = new Vector2(1f, 0f);

            // The root itself carries the layout group: a LayoutGroup is an
            // ILayoutElement, so the root reports the content's preferred height
            // to the container's VerticalLayoutGroup (childControlHeight = true).
            // With the group on the stretched Panel child instead, the root's
            // LayoutElement below only declared a width, every toast got a
            // zero-height slot and the contents spilled over the toasts below.
            VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(0, 0, 0, 0);
            rootLayout.spacing = 0f;
            rootLayout.childAlignment = TextAnchor.UpperLeft;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            LayoutElement le = root.AddComponent<LayoutElement>();
            le.minWidth = 280f;
            le.preferredWidth = 320f;

            // Panel background: a pure visual layer (the game's own rounded art
            // when Addressables are ready, flat rectangles otherwise). It ignores
            // the root layout so its stretch anchors survive, exactly like the
            // border strips in UiFactory.CreatePanel.
            GameObject panel = UiFactory.CreatePanel(root.transform, "Panel", Vector2.zero,
                ToastBackground, StylePalette.BorderBlue, 2f);
            UiFactory.StretchToParent(panel);
            panel.AddComponent<LayoutElement>().ignoreLayout = true;

            Image background = panel.GetComponent<Image>();
            background.raycastTarget = true;

            // Content row: accent strip + icon + text column. It is a child of
            // the root (after the Panel, so it renders above the background) and
            // drives the root's preferred size through the root layout group.
            GameObject contentRow = new GameObject("ContentRow", typeof(RectTransform));
            contentRow.transform.SetParent(root.transform, false);
            HorizontalLayoutGroup contentLayout = contentRow.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 8f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            // forceExpand stays off (as in the browser rows): with it on, the icon
            // would share the extra width with the text column and squeeze the text.
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;
            // (vertical accent strip removed per request)

            // Type icon (kind picked by ToastView per type).
            Image icon = UiFactory.CreateImage(contentRow.transform, "Icon", new Vector2(24f, 24f), StylePalette.BorderBlue, PrimitiveSprites.Kind.Circle);
            icon.raycastTarget = false;
            UiFactory.AddLayoutElement(icon.gameObject, 24f, 24f, 24f, 24f, 0f, 0f);

            // Text column.
            GameObject textColumn = new GameObject("TextColumn", typeof(RectTransform));
            textColumn.transform.SetParent(contentRow.transform, false);
            VerticalLayoutGroup textLayout = textColumn.AddComponent<VerticalLayoutGroup>();
            textLayout.padding = new RectOffset(0, 0, 0, 0);
            textLayout.spacing = 4f;
            textLayout.childAlignment = TextAnchor.UpperLeft;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;
            // Width-only layout values: the height entries must stay unset so the
            // parent row reads the preferred height from the column's own layout
            // group (same rule as TranslationDetailsView.CreateSectionPanel). A set
            // preferredHeight of 0 takes priority over the group and collapses the
            // column to zero height, spilling the text outside the toast.
            UiFactory.AddLayoutElement(textColumn, 0f, -1f, 10f, -1f, 1f, -1f);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(textColumn.transform, false);
            TextMeshProUGUI titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.font = GetTmpFont();
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(titleTmp);
            titleTmp.fontSize = 16f;
            titleTmp.color = StylePalette.TextPrimary;
            // Titles are mod-controlled constants, but disable rich text anyway so
            // nothing user-provided can ever inject markup.
            titleTmp.richText = false;
            titleTmp.text = "Title";
            titleTmp.enableWordWrapping = true;
            LayoutElement titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.minHeight = 18f;

            // Body: user-visible strings (including translation names) end up
            // here, so rich text stays OFF.
            GameObject bodyGo = new GameObject("Body", typeof(RectTransform));
            bodyGo.transform.SetParent(textColumn.transform, false);
            TextMeshProUGUI bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            bodyTmp.font = GetTmpFont();
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(bodyTmp);
            bodyTmp.fontSize = 14f;
            bodyTmp.color = StylePalette.TextSecondary;
            bodyTmp.richText = false;
            bodyTmp.text = "Body";
            bodyTmp.enableWordWrapping = true;
            LayoutElement bodyLe = bodyGo.AddComponent<LayoutElement>();
            bodyLe.minHeight = 16f;

            // Action button (mod-style; hidden until a toast provides an action).
            GameObject btnGo = UiFactory.CreateButton(textColumn.transform, "ActionButton", "Action", null,
                new Vector2(0f, 24f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(btnGo, 0f, 24f, 0f, 24f, 1f, 0f);
            Button btn = btnGo.GetComponent<Button>();
            TextMeshProUGUI btnTextTmp = btnGo.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTextTmp != null)
            {
                btnTextTmp.fontSize = 13f;
                btnTextTmp.richText = false;
            }

            // Reverse progress strip along the bottom edge: the fill shrinks from
            // full to empty as the toast's remaining time runs out.
            GameObject progressGo = new GameObject("Progress", typeof(RectTransform));
            progressGo.transform.SetParent(panel.transform, false);
            RectTransform progressRect = progressGo.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0f, 0f);
            progressRect.anchorMax = new Vector2(1f, 0f);
            progressRect.pivot = new Vector2(0.5f, 0f);
            progressRect.anchoredPosition = Vector2.zero;
            progressRect.sizeDelta = new Vector2(0f, 4f);
            progressGo.AddComponent<LayoutElement>().ignoreLayout = true;

            Image progressBackground = progressGo.AddComponent<Image>();
            progressBackground.sprite = UiFactory.WhiteSprite;
            progressBackground.color = new Color(0f, 0f, 0f, 0.35f);
            progressBackground.raycastTarget = false;

            GameObject progressFillGo = new GameObject("Fill", typeof(RectTransform));
            progressFillGo.transform.SetParent(progressGo.transform, false);
            RectTransform progressFillRect = progressFillGo.GetComponent<RectTransform>();
            progressFillRect.anchorMin = new Vector2(0f, 0f);
            progressFillRect.anchorMax = new Vector2(1f, 1f);
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;
            Image progressFill = progressFillGo.AddComponent<Image>();
            progressFill.sprite = UiFactory.WhiteSprite;
            progressFill.color = StylePalette.BorderBlue;
            progressFill.raycastTarget = false;

            ToastView view = root.AddComponent<ToastView>();
            view.titleText = titleTmp;
            view.bodyText = bodyTmp;
            view.actionButton = btn;
            view.actionButtonText = btnTextTmp;
            view.backgroundImage = background;
            view.iconImage = icon;
            view.progressFill = progressFill;
            view.panelRoot = panel;

            CanvasGroup cg = root.AddComponent<CanvasGroup>();
            view.canvasGroup = cg;
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            btnGo.SetActive(false);
            return root;
        }

        /// <summary>Toast background: near-black panel with a slight translucency.</summary>
        public static Color ToastBackground
        {
            get { return new Color(StylePalette.Panel.r, StylePalette.Panel.g, StylePalette.Panel.b, 0.95f); }
        }

        /// <summary>Accent color of a toast type (border, strip, icon, progress).</summary>
        public static Color TypeColor(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return new Color32(76, 175, 96, 255);   // green
                case ToastType.Warning: return new Color32(232, 161, 77, 255);  // orange
                case ToastType.Error: return StylePalette.AccentRed;
                case ToastType.Info:
                default: return StylePalette.BorderBlue;
            }
        }

        /// <summary>Icon glyph of a toast type (from the runtime primitive set).</summary>
        public static PrimitiveSprites.Kind TypeIcon(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success: return PrimitiveSprites.Kind.Check;
                case ToastType.Warning: return PrimitiveSprites.Kind.Diamond;
                case ToastType.Error: return PrimitiveSprites.Kind.Close;
                case ToastType.Info:
                default: return PrimitiveSprites.Kind.Circle;
            }
        }
    }
}
