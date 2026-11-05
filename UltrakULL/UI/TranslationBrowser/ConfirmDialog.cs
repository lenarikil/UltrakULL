using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Modal confirmation dialog used before any download starts. One instance is
    /// created per canvas (lazily) and reused. The dialog blocks input behind it
    /// with a scrim; nothing is downloaded until the user confirms.
    /// </summary>
    public static class ConfirmDialog
    {
        private sealed class DialogState
        {
            public GameObject Root;
            public TextMeshProUGUI Title;
            public TextMeshProUGUI Message;
            public TextMeshProUGUI ConfirmLabel;
            public TextMeshProUGUI CancelLabel;
            public GameObject ConfirmButton;
            public GameObject CancelButton;
            public Action OnConfirm;
        }

        private static readonly Dictionary<Canvas, DialogState> Dialogs = new Dictionary<Canvas, DialogState>();

        public static void Show(Canvas canvas, string title, string message, Action onConfirm)
        {
            Show(canvas, title, message, onConfirm, null, null);
        }

        /// <summary>
        /// Same as <see cref="Show(Canvas, string, string, Action)"/> with
        /// custom button labels (e.g. the tour's "Start"/"Not now"); null
        /// falls back to the shared Confirm/Cancel labels.
        /// </summary>
        public static void Show(Canvas canvas, string title, string message, Action onConfirm, string confirmLabel, string cancelLabel)
        {
            if (canvas == null)
            {
                return;
            }

            DialogState dialog = GetOrCreate(canvas);
            if (dialog == null)
            {
                return;
            }

            dialog.Title.text = title ?? "Confirm";
            dialog.Message.text = message ?? "";
            dialog.OnConfirm = onConfirm;

            // Custom button labels (e.g. the tour's "Start"/"Not now"); null
            // falls back to the shared Confirm/Cancel labels.
            if (dialog.CancelLabel != null)
            {
                dialog.CancelLabel.text = string.IsNullOrEmpty(cancelLabel) ? BrowserText.ActionCancel : cancelLabel;
            }
            if (dialog.ConfirmLabel != null)
            {
                dialog.ConfirmLabel.text = string.IsNullOrEmpty(confirmLabel) ? BrowserText.ActionConfirm : confirmLabel;
            }

            dialog.Root.transform.SetAsLastSibling();
            dialog.Root.SetActive(true);
        }

        private static DialogState GetOrCreate(Canvas canvas)
        {
            DialogState state;
            if (Dialogs.TryGetValue(canvas, out state) && state != null && state.Root != null)
            {
                return state;
            }

            GameObject root = new GameObject("UllBrowser_ConfirmDialog", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            root.SetActive(false);

            // Scrim that blocks interaction with the UI below
            Image scrim = root.AddComponent<Image>();
            scrim.sprite = UiFactory.WhiteSprite;
            scrim.color = StylePalette.Scrim;
            scrim.raycastTarget = true;
            RectTransform scrimRect = scrim.GetComponent<RectTransform>();
            scrimRect.anchorMin = Vector2.zero;
            scrimRect.anchorMax = Vector2.one;
            scrimRect.offsetMin = Vector2.zero;
            scrimRect.offsetMax = Vector2.zero;

            // Center panel. The panel's height follows the actual content
            // (title + message + compact button row) via a ContentSizeFitter,
            // so the buttons always sit right under the text with a small
            // margin and never cross the frame.
            const float panelWidth = 520f;
            const float stackSpacing = 8f;
            const float titleHeight = 28f;
            const float buttonWidth = 120f;
            const float buttonHeight = 28f;

            GameObject panel = UiFactory.CreatePanel(root.transform, "Panel", new Vector2(panelWidth, 200f), StylePalette.Panel, StylePalette.AccentRed, 2f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(panelWidth, 200f);

            // The same open animation the game's own panels play (item 23);
            // the component animates on enable, so every Show replays it.
            panel.AddComponent<HudOpenEffect>();

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = stackSpacing;
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Panel height = content height, so the buttons hug the message.
            ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI title = UiFactory.CreateLabel(panel.transform, "Title", "", 20f, StylePalette.TextPrimary, TextAlignmentOptions.MidlineLeft, true);
            UiFactory.AddLayoutElement(title.gameObject, -1f, titleHeight, -1f, titleHeight, 1f, 0f);

            // No fixed height: the message sizes to its text.
            TextMeshProUGUI message = UiFactory.CreateLabel(panel.transform, "Message", "", 14f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            UiFactory.AddLayoutElement(message.gameObject, -1f, 24f, -1f, -1f, 1f, 0f);

            // Compact button row: small buttons, centered, tight spacing.
            GameObject buttonRow = new GameObject("ButtonRow", typeof(RectTransform));
            buttonRow.transform.SetParent(panel.transform, false);
            HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(buttonRow, -1f, buttonHeight, -1f, buttonHeight, 1f, 0f);

            GameObject noButton = UiFactory.CreateButton(buttonRow.transform, "No",
                BrowserText.ActionCancel,
                delegate { root.SetActive(false); },
                new Vector2(buttonWidth, buttonHeight),
                StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);

            GameObject yesButton = UiFactory.CreateButton(buttonRow.transform, "Yes",
                BrowserText.ActionConfirm,
                delegate
                {
                    Action confirm = state.OnConfirm;
                    root.SetActive(false);
                    if (confirm != null)
                    {
                        confirm();
                    }
                },
                new Vector2(buttonWidth, buttonHeight),
                StylePalette.PanelAlt, StylePalette.AccentRed, StylePalette.PanelHover);

            state = new DialogState
            {
                Root = root,
                Title = title,
                Message = message,
                ConfirmLabel = yesButton != null ? yesButton.GetComponentInChildren<TextMeshProUGUI>() : null,
                CancelLabel = noButton != null ? noButton.GetComponentInChildren<TextMeshProUGUI>() : null,
                ConfirmButton = yesButton,
                CancelButton = noButton,
                OnConfirm = null
            };

            Dialogs[canvas] = state;
            return state;
        }
    }
}