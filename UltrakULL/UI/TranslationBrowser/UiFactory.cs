using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Programmatic UGUI builder shared by the Thorn and fallback browsers. All
    /// elements are created from scratch (no game prefabs) and follow the
    /// ULTRAKILL palette from <see cref="StylePalette"/>. Child element names are
    /// stable so higher-level views can reach them via Transform.Find.
    /// </summary>
    public static class UiFactory
    {
        private static Sprite _whiteSprite;

        /// <summary>
        /// Shared ColorTint for buttons/rows. The visible color lives on the target
        /// Image, so normalColor is white (unchanged) and hover/press only brighten
        /// or darken it.
        /// </summary>
        public static readonly ColorBlock ButtonTint = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = new Color(1.4f, 1.4f, 1.4f, 1f),
            pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f),
            selectedColor = new Color(1.4f, 1.4f, 1.4f, 1f),
            disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };

        /// <summary>1x1 white sprite used as the base for all Image colors.</summary>
        public static Sprite WhiteSprite
        {
            get
            {
                if (_whiteSprite == null)
                {
                    Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.name = "UllBrowser_White";
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
                }
                return _whiteSprite;
            }
        }

        /// <summary>Loads a sprite from a local image file (png/jpg) in the pack folder.</summary>
        public static Sprite LoadLocalSprite(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return null;
            }
            try
            {
                byte[] data = System.IO.File.ReadAllBytes(filePath);
                if (data == null || data.Length == 0)
                {
                    return null;
                }
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(data))
                {
                    return null;
                }
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            catch
            {
                return null;
            }
        }

        // ---------------------------------------------------------------------
        // Panels & borders
        // ---------------------------------------------------------------------

        /// <summary>
        /// Creates a solid panel with an optional thin border (4 strips, so the
        /// border thickness is exactly 1-2px regardless of scaling).
        /// </summary>
        public static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color background, Color? borderColor = null, float borderThickness = 2f)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            Image image = panel.AddComponent<Image>();
            image.sprite = WhiteSprite;
            image.color = background;
            image.raycastTarget = false;

            if (borderColor.HasValue)
            {
                // Prefer the game's own rounded 9-slice art when Addressables exposed
                // it; otherwise fall back to the flat 4-strip border.
                bool small = IsSmallControl(size);
                if (NativeUi.IsReady)
                {
                    NativeUi.ApplySliced(image, small ? NativeUi.FillSmall : NativeUi.FillLarge, background);
                    AddNativeBorder(panel.transform, borderColor.Value, small);
                }
                else
                {
                    AddBorder(panel.transform, borderColor.Value, borderThickness);
                }
            }
            else if (NativeUi.IsReady && background.a > 0f)
            {
                NativeUi.ApplySliced(image, IsSmallControl(size) ? NativeUi.FillSmall : NativeUi.FillLarge, background);
            }

            return panel;
        }

        /// <summary>
        /// True when the control is small enough that the game would use its small
        /// rounded control art (Round_FillSmall / Round_BorderSmall) instead of the
        /// large variants. A zero-sized (stretched) control is treated as large.
        /// </summary>
        private static bool IsSmallControl(Vector2 size)
        {
            float min = float.MaxValue;
            if (size.x > 0f)
            {
                min = Mathf.Min(min, size.x);
            }
            if (size.y > 0f)
            {
                min = Mathf.Min(min, size.y);
            }
            return min < 56f;
        }

        /// <summary>
        /// Adds a single native rounded outline child (9-slice) that stretches over the
        /// whole panel. Used instead of the four flat border strips when the native
        /// sprites are available.
        /// </summary>
        private static void AddNativeBorder(Transform parent, Color color, bool small)
        {
            GameObject border = new GameObject("NativeBorder", typeof(RectTransform));
            border.transform.SetParent(parent, false);
            RectTransform rect = border.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            Image image = border.AddComponent<Image>();
            NativeUi.ApplySliced(image, small ? NativeUi.BorderSmall : NativeUi.BorderLarge, color);
            image.raycastTarget = false;
            // Decorative overlay: keep the border out of any layout group on the panel,
            // otherwise the group overrides its stretch anchors and the border collapses
            // into an empty rectangle instead of framing the content.
            AddIgnoreLayout(border);
        }

        public static void AddBorder(Transform parent, Color color, float thickness)
        {
            CreateBorderStrip(parent, "BorderTop", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -thickness / 2f), new Vector2(0f, thickness), color);
            CreateBorderStrip(parent, "BorderBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, thickness / 2f), new Vector2(0f, thickness), color);
            CreateBorderStrip(parent, "BorderLeft", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(thickness / 2f, 0f), new Vector2(thickness, 0f), color);
            CreateBorderStrip(parent, "BorderRight", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-thickness / 2f, 0f), new Vector2(thickness, 0f), color);
        }

        private static void CreateBorderStrip(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            GameObject strip = new GameObject(name, typeof(RectTransform));
            strip.transform.SetParent(parent, false);
            RectTransform rect = strip.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            Image image = strip.AddComponent<Image>();
            image.sprite = WhiteSprite;
            image.color = color;
            image.raycastTarget = false;
            AddIgnoreLayout(strip);
        }

        /// <summary>
        /// Marks a decorative child (border strip / native outline) so layout groups on
        /// the parent skip it. Without this a Vertical/HorizontalLayoutGroup would try
        /// to lay the border out as a normal child, overriding its stretch anchors.
        /// </summary>
        private static void AddIgnoreLayout(GameObject target)
        {
            LayoutElement element = target.AddComponent<LayoutElement>();
            element.ignoreLayout = true;
        }

        /// <summary>Recolors the four border strips of a panel created by <see cref="CreatePanel"/>.</summary>
        public static void SetBorderColor(GameObject panelRoot, Color color)
        {
            if (panelRoot == null)
            {
                return;
            }

            SetBorderColor(panelRoot.transform, "BorderTop", color);
            SetBorderColor(panelRoot.transform, "BorderBottom", color);
            SetBorderColor(panelRoot.transform, "BorderLeft", color);
            SetBorderColor(panelRoot.transform, "BorderRight", color);

            // Native rounded outline (when the 9-slice path was used instead of strips).
            SetBorderColor(panelRoot.transform, "NativeBorder", color);
        }

        private static void SetBorderColor(Transform parent, string childName, Color color)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        // ---------------------------------------------------------------------
        // Text
        // ---------------------------------------------------------------------

        public static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            string text,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft,
            bool wrap = true,
            bool autoSize = false)
        {
            GameObject labelObj = new GameObject(name, typeof(RectTransform));
            labelObj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text ?? "";
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = wrap;
            tmp.raycastTarget = false;
            if (autoSize)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 8f;
                tmp.fontSizeMax = fontSize;
            }
            if (Core.GlobalFontTMP != null)
            {
                tmp.font = Core.GlobalFontTMP;
            }
            // The mod UI manages its own fonts and must not receive the active
            // translation's custom font.
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(tmp);
            return tmp;
        }

        /// <summary>
        /// Turns a label into a single line that clips overflowing text with an
        /// ellipsis. Used for names/titles that must never wrap into a neighbouring
        /// column (a wrapped line inside a fixed-height row would overlap it).
        /// </summary>
        public static void SingleLine(TextMeshProUGUI label)
        {
            if (label == null)
            {
                return;
            }
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }

        /// <summary>Stretches an existing RectTransform to the full parent area with margins.</summary>
        public static void StretchToParent(GameObject target, float margin = 0f)
        {
            if (target != null)
            {
                StretchToParent(target.GetComponent<RectTransform>(), margin);
            }
        }

        /// <summary>Stretches an existing RectTransform to the full parent area with margins.</summary>
        public static void StretchToParent(Component target, float margin = 0f)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(margin, margin);
            rect.offsetMax = new Vector2(-margin, -margin);
        }

        // ---------------------------------------------------------------------
        // Images & icons
        // ---------------------------------------------------------------------

        public static Image CreateImage(Transform parent, string name, Vector2 size, Color color, PrimitiveSprites.Kind? icon = null)
        {
            GameObject imageObj = new GameObject(name, typeof(RectTransform));
            imageObj.transform.SetParent(parent, false);
            RectTransform rect = imageObj.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            // Icons must never inherit a stray rotation from their parent hierarchy.
            rect.localRotation = Quaternion.identity;

            Image image = imageObj.AddComponent<Image>();
            image.sprite = icon.HasValue ? PrimitiveSprites.Get(icon.Value) : WhiteSprite;
            image.color = color;
            // Primitive icons are square; preserve their aspect so a non-square rect
            // does not stretch them. Non-icon images (logos, flags) set this themselves.
            image.preserveAspect = icon.HasValue;
            image.raycastTarget = false;
            return image;
        }

        // ---------------------------------------------------------------------
        // Buttons
        // ---------------------------------------------------------------------

        /// <summary>
        /// Creates a button with a solid background, thin border, centered label and
        /// hover/press color feedback. The label is reachable at child "Label".
        /// When <paramref name="nativeSprite"/> is provided the button uses the game's
        /// 9-slice border sprite and color block instead of the ULTRAKILL palette.
        /// </summary>
        public static GameObject CreateButton(
            Transform parent,
            string name,
            string labelText,
            Action onClick,
            Vector2 size,
            Color backgroundColor,
            Color borderColor,
            Color hoverColor = default(Color),
            Sprite nativeSprite = null)
        {
            GameObject buttonObj = CreatePanel(parent, name, size, backgroundColor, borderColor, 2f);
            RectTransform rect = buttonObj.GetComponent<RectTransform>();

            TextMeshProUGUI label = CreateLabel(buttonObj.transform, "Label", labelText, 15f, StylePalette.TextPrimary, TextAlignmentOptions.Center, false);
            StretchToParent(label);

            Button button = buttonObj.AddComponent<Button>();
            Image targetGraphic = buttonObj.GetComponent<Image>();
            // The panel image is created with raycastTarget=false; a Button needs its
            // targetGraphic to receive raycasts to be clickable.
            targetGraphic.raycastTarget = true;
            button.targetGraphic = targetGraphic;
            button.transition = Selectable.Transition.ColorTint;
            // The visible color lives on the Image (set by CreatePanel); the tint block
            // only brightens/darkens it, so hover/press feedback works on top of both
            // the flat and native rounded backgrounds.
            button.colors = ButtonTint;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            return buttonObj;
        }

        /// <summary>
        /// Instantiates the game's own settings action button (exact native borders,
        /// colors and font) with our label and click handler. Used for lists and tabs
        /// in the fallback browser. Falls back to <see cref="CreateButton"/> when the
        /// native template is unavailable.
        /// </summary>
        public static GameObject CreateNativeButton(
            Transform parent,
            string name,
            string labelText,
            Action onClick,
            Vector2? size = null)
        {
            if (GameUiAssets.ButtonTemplate == null)
            {
                Vector2 fallbackSize = size.HasValue ? size.Value : new Vector2(400f, 40f);
                return CreateButton(parent, name, labelText, onClick, fallbackSize,
                    StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            }

            GameObject buttonObj = UnityEngine.Object.Instantiate(GameUiAssets.ButtonTemplate, parent);
            buttonObj.name = name;
            buttonObj.SetActive(true);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            if (size.HasValue)
            {
                rect.sizeDelta = size.Value;
            }
            else
            {
                rect.sizeDelta = GameUiAssets.ButtonSize;
            }

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = labelText ?? "";
                if (GameUiAssets.ButtonFont != null)
                {
                    label.font = GameUiAssets.ButtonFont;
                }

                // The label is part of the mod's own UI: it must not receive the
                // active translation's custom font through the font swap, and its
                // material is refreshed together with the other mod UI texts when
                // the language changes.
                Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(label);
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
            }
            button.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            return buttonObj;
        }

        /// <summary>
        /// Instantiates the game's own consent Yes/No button (exact native
        /// borders, colors and font) for the browser's confirmation dialogs.
        /// Falls back to <see cref="CreateButton"/> when the template could not
        /// be extracted.
        /// </summary>
        public static GameObject CreateConsentButton(
            Transform parent,
            string name,
            string labelText,
            Action onClick,
            Vector2? size = null)
        {
            Vector2 buttonSize = size.HasValue ? size.Value : GameUiAssets.ConsentButtonSize;

            if (GameUiAssets.ConsentButtonTemplate == null)
            {
                return CreateButton(parent, name, labelText, onClick, buttonSize,
                    StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            }

            GameObject buttonObj = UnityEngine.Object.Instantiate(GameUiAssets.ConsentButtonTemplate, parent);
            buttonObj.name = name;
            buttonObj.SetActive(true);

            // The clone may still carry layout components from the game's own
            // consent row (a LayoutElement with stale min/preferred sizes or a
            // size fitter). The dialog's HorizontalLayoutGroup runs with
            // childControlWidth=false, where such a LayoutElement wins over the
            // clamped sizeDelta and pushes the buttons across the panel border,
            // so strip it from the fresh clone before any layout pass runs.
            LayoutElement staleLayout = buttonObj.GetComponent<LayoutElement>();
            if (staleLayout != null)
            {
                UnityEngine.Object.DestroyImmediate(staleLayout);
            }
            ContentSizeFitter staleFitter = buttonObj.GetComponent<ContentSizeFitter>();
            if (staleFitter != null)
            {
                UnityEngine.Object.DestroyImmediate(staleFitter);
            }

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.sizeDelta = buttonSize;

            // The clone may also carry the game's own layout group or open
            // animation on its root; both would fight the explicit size.
            LayoutGroup rootGroup = buttonObj.GetComponent<LayoutGroup>();
            if (rootGroup != null)
            {
                UnityEngine.Object.DestroyImmediate(rootGroup);
            }
            HudOpenEffect rootEffect = buttonObj.GetComponent<HudOpenEffect>();
            if (rootEffect != null)
            {
                UnityEngine.Object.DestroyImmediate(rootEffect);
            }

            // The clone's children (background image, label) keep the rects
            // they had inside the game's consent panel - fixed sizes authored
            // for a much larger button. Left alone they render far past the
            // resized root, which makes the buttons look huge and cross the
            // dialog border. Force every descendant to stretch-fill its parent
            // and strip the template's internal layout components, so the
            // root's sizeDelta fully controls the visual size.
            NormalizeTemplateChildren(rect);

            TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = labelText ?? "";
                if (GameUiAssets.ButtonFont != null)
                {
                    label.font = GameUiAssets.ButtonFont;
                }
                Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(label);
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
            }
            button.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            return buttonObj;
        }

        /// <summary>
        /// Forces every descendant of a cloned template button to stretch-fill
        /// its parent and strips the template's internal layout components and
        /// open animation, so the root's sizeDelta fully controls the visual
        /// size and nothing can render past the root rect.
        /// </summary>
        private static void NormalizeTemplateChildren(RectTransform root)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                RectTransform childRect = root.GetChild(i) as RectTransform;
                if (childRect == null)
                {
                    continue;
                }

                LayoutElement childLayout = childRect.GetComponent<LayoutElement>();
                if (childLayout != null)
                {
                    UnityEngine.Object.DestroyImmediate(childLayout);
                }
                ContentSizeFitter childFitter = childRect.GetComponent<ContentSizeFitter>();
                if (childFitter != null)
                {
                    UnityEngine.Object.DestroyImmediate(childFitter);
                }
                LayoutGroup childGroup = childRect.GetComponent<LayoutGroup>();
                if (childGroup != null)
                {
                    UnityEngine.Object.DestroyImmediate(childGroup);
                }
                HudOpenEffect childEffect = childRect.GetComponent<HudOpenEffect>();
                if (childEffect != null)
                {
                    UnityEngine.Object.DestroyImmediate(childEffect);
                }

                childRect.anchorMin = Vector2.zero;
                childRect.anchorMax = Vector2.one;
                childRect.pivot = new Vector2(0.5f, 0.5f);
                childRect.offsetMin = Vector2.zero;
                childRect.offsetMax = Vector2.zero;
                childRect.localScale = Vector3.one;
                childRect.localRotation = Quaternion.identity;

                NormalizeTemplateChildren(childRect);
            }
        }

        // ---------------------------------------------------------------------
        // Scroll view
        // ---------------------------------------------------------------------

        public sealed class ScrollViewLayout
        {
            public RectTransform Root;
            public RectTransform Viewport;
            public RectTransform Content;
            public ScrollRect ScrollRect;
        }

        /// <summary>
        /// Vertical-only scroll view with a mask. Content is laid out vertically and
        /// grows upward with a ContentSizeFitter; callers add children to the Content.
        /// </summary>
        public static ScrollViewLayout CreateScrollView(Transform parent, string name, Vector2 size, Color background)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = size;
            rootRect.localScale = Vector3.one;

            Image rootImage = root.AddComponent<Image>();
            rootImage.sprite = WhiteSprite;
            rootImage.color = background;
            // Raycast target true: ScrollRect needs a graphic to receive drag input
            // over the empty list area.
            rootImage.raycastTarget = true;

            // RectMask2D clips by the rectangular viewport and, unlike Mask, does NOT depend
            // on the mask graphic's alpha. A Mask over a transparent (alpha 0) background
            // clips away every child, which made the list/details render empty.
            RectMask2D mask = root.AddComponent<RectMask2D>();
            mask.padding = Vector4.zero;

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 24f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(4, 4, 4, 4);

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            return new ScrollViewLayout
            {
                Root = rootRect,
                Viewport = viewportRect,
                Content = contentRect,
                ScrollRect = scrollRect
            };
        }

        /// <summary>
        /// Vertical scrollbar for a scroll view. Prefers the game's own
        /// scrollbar template (exact native handle art, colors and behavior,
        /// item 12); falls back to the flat HUD-blue bar when it could not be
        /// extracted. The caller positions the returned object.
        /// </summary>
        public static Scrollbar CreateScrollbar(Transform parent, string name, ScrollRect scrollRect)
        {
            Scrollbar scrollbar;
            Image handleImage;

            if (GameUiAssets.ScrollbarTemplate != null)
            {
                // Clone the native scrollbar: it keeps the game's own sliding
                // area, handle sprite and colors, and Instantiate remaps the
                // handleRect/targetGraphic references to the clone.
                GameObject scrollbarObj = UnityEngine.Object.Instantiate(GameUiAssets.ScrollbarTemplate.gameObject, parent);
                scrollbarObj.name = name;
                scrollbarObj.SetActive(true);

                scrollbar = scrollbarObj.GetComponent<Scrollbar>();
                // Defensive: the template's event is already empty, and the
                // clone must never drive anything but its own scroll rect.
                scrollbar.onValueChanged = new Scrollbar.ScrollEvent();
                scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };

                handleImage = scrollbar.targetGraphic as Image;
                if (handleImage == null && scrollbar.handleRect != null)
                {
                    handleImage = scrollbar.handleRect.GetComponent<Image>();
                }

                if (scrollRect != null)
                {
                    scrollRect.verticalScrollbar = scrollbar;
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                }

                return scrollbar;
            }

            GameObject fallbackObj = new GameObject(name, typeof(RectTransform));
            fallbackObj.transform.SetParent(parent, false);

            scrollbar = fallbackObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.transition = Selectable.Transition.None;
            scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };

            GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(fallbackObj.transform, false);
            RectTransform slidingRect = slidingArea.GetComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.pivot = new Vector2(0.5f, 0.5f);
            slidingRect.offsetMin = new Vector2(2f, 2f);
            slidingRect.offsetMax = new Vector2(-2f, -2f);

            GameObject handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(slidingArea.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(1f, 0.2f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(0f, 40f);

            handleImage = handle.AddComponent<Image>();
            // Prefer the game's rounded fill art for the handle so the scrollbar reads
            // as a native control; fall back to a flat bar otherwise.
            if (NativeUi.IsReady)
            {
                NativeUi.ApplySliced(handleImage, NativeUi.FillSmall, StylePalette.BorderBlue);
            }
            else
            {
                handleImage.sprite = WhiteSprite;
                handleImage.color = StylePalette.BorderBlue;
            }
            handleImage.raycastTarget = true;

            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;

            if (scrollRect != null)
            {
                scrollRect.verticalScrollbar = scrollbar;
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            }

            return scrollbar;
        }

        // ---------------------------------------------------------------------
        // Logo + flag
        // ---------------------------------------------------------------------

    public sealed class LogoSlot
    {
        public GameObject Root;
        public Image Logo;
        public Image Flag;
        public TextMeshProUGUI FlagBadge;
    }

        /// <summary>
        /// Square logo slot with an optional flag image (a known flag sprite, or
        /// a flag URL, or a language-code badge when neither is available)
        /// pinned to its bottom-right corner.
        ///
        /// Structure (render order, bottom to top): fill, LogoMask (RectMask2D,
        /// inset by the border thickness) holding the cover-cropped logo, the
        /// fallback glyph, the sweep spinner, the flag overlay and finally the
        /// border - so the image can never cover the border (items 3, 13, 14).
        /// While a remote logo is pending only the spinner is shown; the
        /// fallback diamond appears only after a failed load (item 11).
        /// </summary>
        public static LogoSlot CreateLogoSlot(
            Transform parent,
            string name,
            float size,
            string logoUrl,
            string flagUrl,
            string languageCode,
            Sprite flagSprite = null,
            Action<bool> onRemoteLoadResult = null)
        {
            GameObject slot = CreatePanel(parent, name, new Vector2(size, size), StylePalette.PanelAlt, StylePalette.BorderDim, 1f);

            // Safety net: even when a parent layout group stretches its
            // children, the slot stays a square (items 8 and 20).
            AspectRatioFitter aspect = slot.AddComponent<AspectRatioFitter>();
            aspect.aspectRatio = 1f;

            // The image area is inset by the border thickness so the clipped
            // logo can never render over the border ring.
            const float inset = 3f;
            float innerSize = Mathf.Max(1f, size - inset * 2f);

            GameObject logoCanvas = new GameObject("LogoMask", typeof(RectTransform), typeof(RectMask2D));
            logoCanvas.transform.SetParent(slot.transform, false);
            RectTransform logoCanvasRect = logoCanvas.GetComponent<RectTransform>();
            logoCanvasRect.anchorMin = Vector2.zero;
            logoCanvasRect.anchorMax = Vector2.one;
            logoCanvasRect.pivot = new Vector2(0.5f, 0.5f);
            logoCanvasRect.offsetMin = new Vector2(inset, inset);
            logoCanvasRect.offsetMax = new Vector2(-inset, -inset);

            // The logo image: sized to COVER the mask square (cropped by the
            // RectMask2D) instead of fitting inside it (item 4).
            Image logo = CreateImage(logoCanvas.transform, "Logo", Vector2.zero, Color.white);
            logo.preserveAspect = false;
            logo.raycastTarget = false;
            logo.gameObject.SetActive(false);

            // Fallback glyph, shown only after a failed load (item 11).
            Image fallback = CreateImage(slot.transform, "Fallback", new Vector2(size * 0.45f, size * 0.45f), StylePalette.TextDim, PrimitiveSprites.Kind.Diamond);
            fallback.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            fallback.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            fallback.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            fallback.rectTransform.anchoredPosition = Vector2.zero;
            fallback.rectTransform.sizeDelta = new Vector2(size * 0.45f, size * 0.45f);

            // Sweep spinner, shown alone while a remote logo is pending (item 10).
            Image spinner = LoadingSpinner.Create(slot.transform, "Spinner", new Vector2(size * 0.5f, size * 0.5f), StylePalette.TextDim);
            spinner.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            spinner.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            spinner.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            spinner.rectTransform.anchoredPosition = Vector2.zero;
            spinner.rectTransform.sizeDelta = new Vector2(size * 0.5f, size * 0.5f);
            spinner.raycastTarget = false;
            spinner.gameObject.SetActive(false);

            LogoSlot result = new LogoSlot { Root = slot, Logo = logo, Flag = null, FlagBadge = null };

            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                // Local file paths (installed packs) load directly; remote URLs go
                // through ImageCache.
                bool isLocal = !logoUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !logoUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                if (isLocal)
                {
                    Sprite localSprite = LoadLocalSprite(logoUrl);
                    if (localSprite != null)
                    {
                        ApplyLogo(logo, fallback, spinner, localSprite, innerSize);
                    }
                    else
                    {
                        fallback.gameObject.SetActive(true);
                    }
                }
                else
                {
                    string url = logoUrl;
                    spinner.gameObject.SetActive(true);
                    ImageCache.GetOrLoad(url, delegate (Sprite sprite)
                    {
                        if (logo == null)
                        {
                            return;
                        }
                        ApplyLogo(logo, fallback, spinner, sprite, innerSize);
                        if (onRemoteLoadResult != null)
                        {
                            onRemoteLoadResult(sprite != null);
                        }
                        if (sprite == null)
                        {
                            // Remote download failed: show warning icon instead of the plain diamond.
                            if (fallback != null)
                            {
                                fallback.sprite = PrimitiveSprites.Get(PrimitiveSprites.Kind.Warning);
                                fallback.color = StylePalette.AccentRed;
                            }
                        }
                    });
                }
            }
            else
            {
                fallback.gameObject.SetActive(true);
            }

            if (flagSprite != null)
            {
                result.Flag = CreateFlagOverlay(slot.transform, size, null, flagSprite);
            }
            else if (!string.IsNullOrWhiteSpace(flagUrl))
            {
                Image flag = CreateFlagOverlay(slot.transform, size, flagUrl, null);
                result.Flag = flag;

                bool flagLocal = !flagUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !flagUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                if (flagLocal)
                {
                    Sprite localSprite = LoadLocalSprite(flagUrl);
                    if (localSprite != null)
                    {
                        flag.sprite = localSprite;
                        flag.color = Color.white;
                        flag.gameObject.SetActive(true);
                    }
                }
                else
                {
                    string flagUrlCopy = flagUrl;
                    ImageCache.GetOrLoad(flagUrlCopy, delegate (Sprite sprite)
                    {
                        if (flag == null)
                        {
                            return;
                        }
                        if (sprite != null)
                        {
                            flag.sprite = sprite;
                            flag.color = Color.white;
                            flag.gameObject.SetActive(true);
                        }
                    });
                }
            }
            else if (!string.IsNullOrWhiteSpace(languageCode))
            {
                result.FlagBadge = CreateFlagBadge(slot.transform, languageCode, size);
            }

            // The border renders above everything (including the logo) so the
            // frame is always fully visible (items 3 and 14).
            MoveBorderToTop(slot.transform);

            return result;
        }

        /// <summary>
        /// Applies a loaded logo sprite: sizes it to cover the square mask area
        /// (wide images match the height, tall images the width, centered) and
        /// hides the spinner and the fallback glyph.
        /// </summary>
        private static void ApplyLogo(Image logo, Image fallback, Image spinner, Sprite sprite, float innerSize)
        {
            if (sprite == null)
            {
                // Failed load: hide the spinner, show the fallback glyph only now.
                if (spinner != null)
                {
                    spinner.gameObject.SetActive(false);
                }
                if (fallback != null)
                {
                    fallback.gameObject.SetActive(true);
                }
                return;
            }

            if (spinner != null)
            {
                spinner.gameObject.SetActive(false);
            }
            if (fallback != null)
            {
                fallback.gameObject.SetActive(false);
            }

            RectTransform rect = logo.rectTransform;
            float aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;
            float width;
            float height;
            if (aspect >= 1f)
            {
                height = innerSize;
                width = innerSize * aspect;
            }
            else
            {
                width = innerSize;
                height = innerSize / aspect;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);

            logo.sprite = sprite;
            logo.color = Color.white;
            logo.gameObject.SetActive(true);
        }

        /// <summary>
        /// The flag overlay pinned to the slot's bottom-right corner, above the
        /// logo mask but inside the border ring.
        /// </summary>
        private static Image CreateFlagOverlay(Transform parent, float slotSize, string url, Sprite sprite)
        {
            float flagSize = slotSize * 0.34f;
            Image flag = CreateImage(parent, "Flag", new Vector2(flagSize, flagSize * 0.72f), Color.white);
            flag.rectTransform.anchorMin = new Vector2(1f, 0f);
            flag.rectTransform.anchorMax = new Vector2(1f, 0f);
            flag.rectTransform.pivot = new Vector2(1f, 0f);
            flag.rectTransform.anchoredPosition = new Vector2(-4f, -4f);
            flag.rectTransform.sizeDelta = new Vector2(flagSize, flagSize * 0.72f);
            flag.preserveAspect = true;
            if (sprite != null)
            {
                flag.sprite = sprite;
                flag.gameObject.SetActive(true);
            }
            else
            {
                flag.gameObject.SetActive(false);
            }
            return flag;
        }

        /// <summary>
        /// Moves a panel's border (the native rounded outline, or the four flat
        /// strips) above every other child so panel content can never render
        /// over the border.
        /// </summary>
        public static void MoveBorderToTop(Transform panelRoot)
        {
            if (panelRoot == null)
            {
                return;
            }

            string[] borderNames = { "NativeBorder", "BorderTop", "BorderBottom", "BorderLeft", "BorderRight" };
            foreach (string borderName in borderNames)
            {
                Transform border = panelRoot.Find(borderName);
                if (border != null)
                {
                    border.SetAsLastSibling();
                }
            }
        }

        /// <summary>Small bordered badge with the uppercased language code (pseudo flag).</summary>
        public static TextMeshProUGUI CreateFlagBadge(Transform parent, string languageCode, float slotSize)
        {
            float badgeSize = slotSize * 0.42f;
            string code = languageCode.Length >= 2 ? languageCode.Substring(0, 2).ToUpperInvariant() : languageCode.ToUpperInvariant();

            GameObject badge = CreatePanel(parent, "FlagBadge", new Vector2(badgeSize, badgeSize * 0.7f), StylePalette.AccentRedDark, StylePalette.AccentRed, 1f);
            RectTransform badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(1f, 0f);
            badgeRect.anchoredPosition = new Vector2(-4f, -4f);

            TextMeshProUGUI tmp = CreateLabel(badge.transform, "Code", code, 12f, Color.white, TextAlignmentOptions.Center, false);
            StretchToParent(tmp);
            return tmp;
        }

        // ---------------------------------------------------------------------
        // Search input
        // ---------------------------------------------------------------------

        /// <summary>
        /// Programmatic TMP input field for the search bar. Works with the legacy
        /// Input Manager (InputLegacyModule is already referenced by the mod).
        /// </summary>
        public static TMP_InputField CreateInputField(Transform parent, string name, Vector2 size, string placeholder, string initialText)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = size;
            rootRect.localScale = Vector3.one;

            Image background = root.AddComponent<Image>();
            background.sprite = WhiteSprite;
            background.color = StylePalette.Panel;
            background.raycastTarget = true;

            // Use the game's own rounded input art (fill + outline) when available;
            // otherwise fall back to the flat 4-strip border.
            if (NativeUi.IsReady)
            {
                NativeUi.ApplySliced(background, NativeUi.FillSmall, StylePalette.Panel);
                AddNativeBorder(root.transform, StylePalette.BorderBlue, true);
            }
            else
            {
                AddBorder(root.transform, StylePalette.BorderBlue, 1f);
            }

            GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(root.transform, false);
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0f, 0f);
            textAreaRect.anchorMax = new Vector2(1f, 1f);
            textAreaRect.pivot = new Vector2(0.5f, 0.5f);
            textAreaRect.offsetMin = new Vector2(8f, 4f);
            textAreaRect.offsetMax = new Vector2(-8f, -4f);

            TextMeshProUGUI placeholderTmp = CreateLabel(textArea.transform, "Placeholder", placeholder, 15f, StylePalette.TextDim, TextAlignmentOptions.MidlineLeft, true);
            StretchToParent(placeholderTmp);

            TextMeshProUGUI textTmp = CreateLabel(textArea.transform, "Text", initialText ?? "", 15f, StylePalette.TextPrimary, TextAlignmentOptions.MidlineLeft, true);
            StretchToParent(textTmp);
            textTmp.raycastTarget = true;

            TMP_InputField input = root.AddComponent<TMP_InputField>();
            input.textViewport = textAreaRect;
            input.textComponent = textTmp;
            input.placeholder = placeholderTmp;
            input.text = initialText ?? "";
            input.characterLimit = 60;
            input.caretBlinkRate = 0.8f;
            input.selectionColor = new Color(0.3f, 0.5f, 1f, 0.5f);
            input.caretColor = StylePalette.AccentCyan;
            input.customCaretColor = true;

            return input;
        }

        /// <summary>
        /// Forces every layout group under <paramref name="root"/> to control both the
        /// width and the height of its children. Children in this UI declare their sizes
        /// through LayoutElement min/preferred/flexible values and text columns rely on
        /// flexible widths to fill the remaining space; a group that does not control
        /// widths leaves every text column at its default RectTransform width (a narrow
        /// strip), and a group that does not control heights collapses the rows.
        /// </summary>
        public static void ForceLayoutHeights(Transform root)
        {
            if (root == null)
            {
                return;
            }

            HorizontalOrVerticalLayoutGroup[] groups = root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true);
            foreach (HorizontalOrVerticalLayoutGroup group in groups)
            {
                if (group != null)
                {
                    group.childControlHeight = true;
                    group.childControlWidth = true;
                }
            }
        }

        /// <summary>Adds a LayoutElement so layout groups honor fixed sizes.</summary>
        public static LayoutElement AddLayoutElement(
            GameObject target,
            float minWidth = -1f,
            float minHeight = -1f,
            float preferredWidth = -1f,
            float preferredHeight = -1f,
            float flexibleWidth = -1f,
            float flexibleHeight = -1f)
        {
            LayoutElement element = target.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = target.AddComponent<LayoutElement>();
            }
            if (minWidth >= 0f) element.minWidth = minWidth;
            if (minHeight >= 0f) element.minHeight = minHeight;
            if (preferredWidth >= 0f) element.preferredWidth = preferredWidth;
            if (preferredHeight >= 0f) element.preferredHeight = preferredHeight;
            if (flexibleWidth >= 0f) element.flexibleWidth = flexibleWidth;
            if (flexibleHeight >= 0f) element.flexibleHeight = flexibleHeight;
            return element;
        }

        // ---------------------------------------------------------------------
        // Layout helpers
        // ---------------------------------------------------------------------

        public static void SetRect(Component target, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }
    }
}