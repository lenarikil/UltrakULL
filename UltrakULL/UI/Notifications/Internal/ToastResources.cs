using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.Notifications
{
    public static class ToastResources
    {
        private static TMP_FontAsset _fallbackTmpFont;
        private static bool _fontLookupFailed;

        public static TMP_FontAsset GetTmpFont()
        {
            try
            {
                // Prefer the current global font: it changes with the selected language.
                if (Core.GlobalFontTMP != null) return Core.GlobalFontTMP;
                if (_fallbackTmpFont != null) return _fallbackTmpFont;
                // Try to find existing TMP fonts
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

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.3f, 0.92f);
            bg.raycastTarget = true;

            // The parent VerticalLayoutGroup drives this row's size via the group's preferred
            // height. A ContentSizeFitter stacked on top of a controlling parent layout can
            // fight it and collapse the element, so the group alone sizes this row.
            VerticalLayoutGroup vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 8, 8);
            vlg.spacing = 4f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            LayoutElement le = root.AddComponent<LayoutElement>();
            le.minWidth = 280f;
            le.preferredWidth = 320f;

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.font = GetTmpFont();
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(titleTmp);
            titleTmp.fontSize = 16f;
            titleTmp.color = Color.white;
            titleTmp.richText = true;
            titleTmp.text = "Title";
            titleTmp.enableWordWrapping = true;
            LayoutElement titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.minHeight = 18f;

            // Body
            GameObject bodyGo = new GameObject("Body", typeof(RectTransform));
            bodyGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            bodyTmp.font = GetTmpFont();
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(bodyTmp);
            bodyTmp.fontSize = 14f;
            bodyTmp.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            bodyTmp.richText = true;
            bodyTmp.text = "Body";
            bodyTmp.enableWordWrapping = true;
            LayoutElement bodyLe = bodyGo.AddComponent<LayoutElement>();
            bodyLe.minHeight = 16f;

            // Action button
            GameObject btnGo = new GameObject("ActionButton", typeof(RectTransform));
            btnGo.transform.SetParent(root.transform, false);
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(1f, 1f, 1f, 0.15f);
            Button btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            GameObject btnTextGo = new GameObject("ButtonText", typeof(RectTransform));
            btnTextGo.transform.SetParent(btnGo.transform, false);
            RectTransform btnTextRt = btnTextGo.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;
            TextMeshProUGUI btnTextTmp = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnTextTmp.font = GetTmpFont();
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(btnTextTmp);
            btnTextTmp.fontSize = 13f;
            btnTextTmp.color = Color.white;
            btnTextTmp.alignment = TextAlignmentOptions.Center;
            btnTextTmp.text = "Action";

            LayoutElement btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.minHeight = 22f;
            btnLe.preferredHeight = 24f;

            HorizontalLayoutGroup btnHlg = btnGo.AddComponent<HorizontalLayoutGroup>();
            btnHlg.padding = new RectOffset(4, 4, 2, 2);
            btnHlg.childAlignment = TextAnchor.MiddleCenter;
            btnHlg.childForceExpandWidth = true;
            btnHlg.childForceExpandHeight = false;

            ToastView view = root.AddComponent<ToastView>();
            view.titleText = titleTmp;
            view.bodyText = bodyTmp;
            view.actionButton = btn;
            view.actionButtonText = btnTextTmp;
            view.backgroundImage = bg;

            CanvasGroup cg = root.AddComponent<CanvasGroup>();
            view.canvasGroup = cg;
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            btnGo.SetActive(false);
            return root;
        }
    }
}