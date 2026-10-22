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
            public Action OnConfirm;
        }

        private static readonly Dictionary<Canvas, DialogState> Dialogs = new Dictionary<Canvas, DialogState>();

        public static void Show(Canvas canvas, string title, string message, Action onConfirm)
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

            // Center panel
            GameObject panel = UiFactory.CreatePanel(root.transform, "Panel", new Vector2(520f, 240f), StylePalette.Panel, StylePalette.AccentRed, 2f);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(520f, 240f);

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(20, 20, 18, 18);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = UiFactory.CreateLabel(panel.transform, "Title", "", 20f, StylePalette.TextPrimary, TextAlignmentOptions.MidlineLeft, true);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 30f, 0f, 30f, 1f, 0f);

            TextMeshProUGUI message = UiFactory.CreateLabel(panel.transform, "Message", "", 14f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            UiFactory.AddLayoutElement(message.gameObject, 0f, 90f, 0f, 90f, 1f, 0f);

            GameObject buttonRow = new GameObject("ButtonRow", typeof(RectTransform));
            buttonRow.transform.SetParent(panel.transform, false);
            HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            UiFactory.AddLayoutElement(buttonRow, 0f, 48f, 0f, 48f, 1f, 0f);

            GameObject noButton = UiFactory.CreateButton(buttonRow.transform, "No",
                "Cancel",
                delegate { root.SetActive(false); },
                new Vector2(170f, 44f),
                StylePalette.PanelAlt,
                StylePalette.BorderBlue,
                StylePalette.PanelHover);
            UiFactory.AddLayoutElement(noButton, 170f, 44f, 170f, 44f, 0f, 0f);

            GameObject yesButton = UiFactory.CreateButton(buttonRow.transform, "Yes",
                "Download",
                delegate
                {
                    Action confirm = state.OnConfirm;
                    root.SetActive(false);
                    if (confirm != null)
                    {
                        confirm();
                    }
                },
                new Vector2(170f, 44f),
                StylePalette.AccentRedDark,
                StylePalette.AccentRed,
                new Color(0.45f, 0.12f, 0.12f, 1f));
            UiFactory.AddLayoutElement(yesButton, 170f, 44f, 170f, 44f, 0f, 0f);

            state = new DialogState { Root = root, Title = title, Message = message, OnConfirm = null };
            Dialogs[canvas] = state;
            return state;
        }
    }
}