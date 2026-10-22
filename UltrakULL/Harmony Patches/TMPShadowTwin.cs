using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    /// <summary>
    /// Draws a text's uGUI Shadow/Outline components with TextMeshProUGUI twins
    /// rendered behind it.
    ///
    /// TMP generates its mesh outside the uGUI geometry pipeline, so uGUI
    /// Shadow/Outline components have no effect on TMP texts. Each twin renders
    /// the same text with the same font and material as its source, in a solid
    /// shadow color, offset by the shadow distance - reproducing the components'
    /// exact native look (colors, canvas-space distances, alpha semantics),
    /// including stacked shadows like the navmesh warning's.
    ///
    /// Regular text shadows (the shader underlay) do not use twins; they are
    /// configured on the text's material (see TMPFontMaterials).
    /// </summary>
    public sealed class TMPShadowTwin : MonoBehaviour
    {
        private const string TwinNamePrefix = "TMP_ShadowTwin_";

        /// <summary>A single shadow to draw behind a text.</summary>
        public struct ShadowStyle
        {
            public Color Color;
            /// <summary>Offset in canvas units.</summary>
            public Vector2 CanvasOffset;
            /// <summary>uGUI semantics: multiply the alpha with the source's alpha.</summary>
            public bool UseSourceAlpha;
        }

        private sealed class Twin
        {
            public TextMeshProUGUI Text;
            public RectTransform Rect;
            public ShadowStyle Style;
        }

        private TextMeshProUGUI source;
        private readonly List<Twin> twins = new List<Twin>();

        /// <summary>
        /// True for the shadow twin objects created by this class, so the font
        /// swap and the HUD material logic leave them alone.
        /// </summary>
        public static bool IsShadowTwin(TextMeshProUGUI text)
        {
            return text != null && text.gameObject != null &&
                text.gameObject.name.StartsWith(TwinNamePrefix);
        }

        /// <summary>
        /// Enables the given shadow styles on a text, or removes its shadows
        /// when styles is null/empty. Twin objects are created and destroyed as
        /// needed.
        /// </summary>
        public static void Set(TextMeshProUGUI text, List<ShadowStyle> styles)
        {
            if (text == null)
                return;

            TMPShadowTwin controller = text.GetComponent<TMPShadowTwin>();
            if (styles == null || styles.Count == 0)
            {
                if (controller != null)
                    Destroy(controller);
                return;
            }

            if (controller == null)
            {
                controller = text.gameObject.AddComponent<TMPShadowTwin>();
                controller.source = text;
            }
            controller.ApplyStyles(styles);
        }

        private void ApplyStyles(List<ShadowStyle> styles)
        {
            for (int i = twins.Count - 1; i >= styles.Count; i--)
            {
                if (twins[i].Text != null)
                    Destroy(twins[i].Text.gameObject);
                twins.RemoveAt(i);
            }

            for (int i = 0; i < styles.Count; i++)
            {
                if (i < twins.Count)
                {
                    twins[i].Style = styles[i];
                    continue;
                }

                Twin twin = CreateTwin(styles[i]);
                if (twin != null)
                    twins.Add(twin);
            }

            SyncAll();
        }

        private Twin CreateTwin(ShadowStyle style)
        {
            if (source == null || source.rectTransform == null || source.rectTransform.parent == null)
                return null;

            RectTransform sourceRect = source.rectTransform;
            GameObject twinObject = new GameObject(TwinNamePrefix + source.gameObject.name, typeof(RectTransform));
            twinObject.transform.SetParent(sourceRect.parent, false);
            // Insert right before the source so the twin renders behind it.
            twinObject.transform.SetSiblingIndex(sourceRect.GetSiblingIndex());

            RectTransform twinRect = twinObject.GetComponent<RectTransform>();
            twinRect.anchorMin = sourceRect.anchorMin;
            twinRect.anchorMax = sourceRect.anchorMax;
            twinRect.pivot = sourceRect.pivot;
            twinRect.sizeDelta = sourceRect.sizeDelta;
            twinRect.localScale = sourceRect.localScale;

            TextMeshProUGUI twin = twinObject.AddComponent<TextMeshProUGUI>();
            // Never participate in the parent's layout; the twin mirrors the
            // source's rect directly.
            twinObject.AddComponent<LayoutElement>().ignoreLayout = true;

            twin.text = source.text;
            twin.font = source.font;
            twin.fontSize = source.fontSize;
            twin.fontStyle = source.fontStyle;
            twin.alignment = source.alignment;
            twin.raycastTarget = false;
            twin.maskable = source.maskable;
            twin.richText = source.richText;
            twin.enableWordWrapping = source.enableWordWrapping;
            twin.overflowMode = source.overflowMode;
            twin.enableAutoSizing = source.enableAutoSizing;
            twin.fontSizeMin = source.fontSizeMin;
            twin.fontSizeMax = source.fontSizeMax;
            twin.fontSharedMaterial = source.fontSharedMaterial;

            return new Twin { Text = twin, Rect = twinRect, Style = style };
        }

        private void OnEnable()
        {
            SyncAll();
        }

        private void OnDisable()
        {
            for (int i = 0; i < twins.Count; i++)
            {
                if (twins[i].Text != null)
                    twins[i].Text.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < twins.Count; i++)
            {
                if (twins[i].Text != null)
                    Destroy(twins[i].Text.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (source == null || twins.Count == 0)
            {
                Destroy(this);
                return;
            }

            SyncAll();
        }

        /// <summary>
        /// Keeps every twin in sync with everything of the source that can
        /// change at runtime (HUD values rewrite the text every frame).
        /// </summary>
        private void SyncAll()
        {
            if (source == null)
                return;

            bool sourceVisible = source.isActiveAndEnabled &&
                source.gameObject.activeInHierarchy &&
                !string.IsNullOrEmpty(source.text) &&
                source.color.a > 0.001f;

            RectTransform sourceRect = source.rectTransform;
            for (int i = 0; i < twins.Count; i++)
            {
                Twin twin = twins[i];
                if (twin.Text == null)
                    continue;

                if (twin.Text.text != source.text)
                    twin.Text.text = source.text;
                if (twin.Text.font != source.font)
                    twin.Text.font = source.font;
                if (!Mathf.Approximately(twin.Text.fontSize, source.fontSize))
                    twin.Text.fontSize = source.fontSize;
                if (twin.Text.fontStyle != source.fontStyle)
                    twin.Text.fontStyle = source.fontStyle;
                if (twin.Text.alignment != source.alignment)
                    twin.Text.alignment = source.alignment;
                if (twin.Text.fontSharedMaterial != source.fontSharedMaterial)
                    twin.Text.fontSharedMaterial = source.fontSharedMaterial;

                Vector2 position = sourceRect.anchoredPosition + twin.Style.CanvasOffset;
                if (twin.Rect.anchoredPosition != position)
                    twin.Rect.anchoredPosition = position;
                if (twin.Rect.sizeDelta != sourceRect.sizeDelta)
                    twin.Rect.sizeDelta = sourceRect.sizeDelta;
                // The twin must stay before the source in draw order; only fix
                // it when the hierarchy was reordered externally.
                if (twin.Rect.GetSiblingIndex() > sourceRect.GetSiblingIndex())
                    twin.Rect.SetSiblingIndex(sourceRect.GetSiblingIndex());

                Color color = twin.Style.Color;
                if (twin.Style.UseSourceAlpha)
                    color.a *= source.color.a;
                twin.Text.color = color;

                if (twin.Text.gameObject.activeSelf != sourceVisible)
                    twin.Text.gameObject.SetActive(sourceVisible);
            }
        }
    }
}
