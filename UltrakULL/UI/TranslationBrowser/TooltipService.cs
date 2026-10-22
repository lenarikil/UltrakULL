using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Lightweight hover tooltips for the translation browser. A single tooltip
    /// instance is created lazily per canvas and follows the mouse pointer.
    /// Tooltips are used so capability states stay understandable without color
    /// alone (accessibility requirement of the interface spec).
    /// </summary>
    public static class TooltipService
    {
        private sealed class TooltipBinding : MonoBehaviour
        {
            public string Text;
        }

        private sealed class TooltipInstance
        {
            public GameObject Root;
            public TextMeshProUGUI Label;
            public CanvasGroup Group;
        }

        private static readonly Dictionary<Canvas, TooltipInstance> Instances = new Dictionary<Canvas, TooltipInstance>();

        /// <summary>Shows a tooltip while the pointer hovers the target game object.</summary>
        public static void Attach(GameObject target, string text)
        {
            if (target != null)
            {
                Attach(target.transform, text);
            }
        }

        /// <summary>Shows a tooltip while the pointer hovers the target component.</summary>
        public static void Attach(Component target, string text)
        {
            if (target == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            Canvas canvas = target.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            TooltipBinding binding = target.GetComponent<TooltipBinding>();
            if (binding == null)
            {
                binding = target.gameObject.AddComponent<TooltipBinding>();
                AddTriggerEntries(target.gameObject, canvas, binding);
            }
            binding.Text = text;
        }

        private static void AddTriggerEntries(GameObject target, Canvas canvas, TooltipBinding binding)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = target.gameObject.AddComponent<EventTrigger>();
                trigger.triggers = new List<EventTrigger.Entry>();
            }

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(delegate { Show(canvas, binding); });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(delegate { Hide(canvas); });
            trigger.triggers.Add(exit);
        }

        private static void Show(Canvas canvas, TooltipBinding binding)
        {
            TooltipInstance instance = GetOrCreate(canvas);
            if (instance == null)
            {
                return;
            }

            instance.Label.text = binding.Text;
            instance.Root.transform.SetAsLastSibling();
            instance.Root.SetActive(true);
            instance.Group.alpha = 1f;

            Vector3 pos = Input.mousePosition + new Vector3(14f, -14f, 0f);
            instance.Root.transform.position = pos;
        }

        private static void Hide(Canvas canvas)
        {
            TooltipInstance instance;
            if (canvas == null || !Instances.TryGetValue(canvas, out instance) || instance == null || instance.Root == null)
            {
                return;
            }
            instance.Root.SetActive(false);
        }

        private static TooltipInstance GetOrCreate(Canvas canvas)
        {
            TooltipInstance instance;
            if (Instances.TryGetValue(canvas, out instance) && instance != null && instance.Root != null)
            {
                return instance;
            }

            if (canvas == null)
            {
                return null;
            }

            GameObject root = new GameObject("UllBrowser_Tooltip", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            root.SetActive(false);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            // Background
            Image background = root.AddComponent<Image>();
            background.color = StylePalette.PanelAlt;
            background.raycastTarget = false;

            // Thin blue border
            CreateBorder(root.transform, StylePalette.BorderBlue, 1f);

            // Text
            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(root.transform, false);
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = "";
            label.fontSize = 14f;
            label.color = StylePalette.TextPrimary;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            if (Core.GlobalFontTMP != null)
            {
                label.font = Core.GlobalFontTMP;
            }
            Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(label);

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(8f, 6f);
            labelRect.offsetMax = new Vector2(-8f, -6f);

            // Fixed comfortable size; text wraps inside it.
            rect.sizeDelta = new Vector2(240f, 44f);

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            instance = new TooltipInstance { Root = root, Label = label, Group = group };
            Instances[canvas] = instance;
            return instance;
        }

        private static void CreateBorder(Transform parent, Color color, float thickness)
        {
            CreateBorderStrip(parent, "BorderTop", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -thickness), new Vector2(0f, thickness), color);
            CreateBorderStrip(parent, "BorderBottom", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, thickness), new Vector2(0f, thickness), color);
            CreateBorderStrip(parent, "BorderLeft", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(thickness, 0f), new Vector2(thickness, 0f), color);
            CreateBorderStrip(parent, "BorderRight", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-thickness, 0f), new Vector2(thickness, 0f), color);
        }

        private static void CreateBorderStrip(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
        {
            GameObject strip = new GameObject(name, typeof(RectTransform));
            strip.transform.SetParent(parent, false);
            RectTransform rect = strip.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            Image image = strip.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }
    }
}