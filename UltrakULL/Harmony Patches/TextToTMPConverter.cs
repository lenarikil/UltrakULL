using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    /// <summary>
    /// Converts legacy uGUI Text components into TextMeshProUGUI siblings at runtime.
    ///
    /// The sibling mirrors the source component (rect, text, style, effects) and is
    /// kept in sync when the game writes to the source. The source itself stays in
    /// the hierarchy - it still drives layout and animations - but is hidden by
    /// setting its canvas renderer alpha to zero and no longer eats raycasts.
    ///
    /// Shadows are preserved, never fabricated: uGUI Shadow/Outline components on
    /// the source are copied to the sibling, and the font swap carries the original
    /// material's underlay parameters over (see TMPFontMaterials).
    /// </summary>
    public static class TextToTMPConverter
    {
        private static readonly Dictionary<int, TextMeshProUGUI> textToTMP = new Dictionary<int, TextMeshProUGUI>();
        private static readonly Dictionary<int, Text> tmpToSourceText = new Dictionary<int, Text>();

        // UnityExplorer / UniverseLib builds its own UI with legacy Text; the mod
        // must not convert or touch it.
        private static readonly string[] universeLibPatterns =
        {
            "UniverseLibCanvas",
            "unityexplorer",
            "com.sinai",
            "UniverseLib",
            "ExplorerCanvas",
            "InspectorCanvas",
            "MouseInspectDropdown",
            "Dropdown List",
            "Viewport",
            "Inspector",
            "PanelHolder"
        };

        /// <summary>
        /// Returns the legacy Text a TMP sibling was converted from, if any.
        /// </summary>
        public static Text GetSourceText(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return null;

            int tmpId = tmp.GetInstanceID();
            Text sourceText;
            if (tmpToSourceText.TryGetValue(tmpId, out sourceText) && sourceText != null)
                return sourceText;

            tmpToSourceText.Remove(tmpId);
            return null;
        }

        /// <summary>
        /// Returns the TMP sibling created for a legacy Text, if any.
        /// </summary>
        public static TextMeshProUGUI GetConvertedTMP(Text source)
        {
            return GetTMPForSource(source);
        }

        /// <summary>
        /// True if the TMP sibling is the converted hp label of a HealthBar.
        /// The sibling sits next to the original and is not the hpText reference
        /// itself, so it has to be recognized through the conversion map.
        /// </summary>
        public static bool IsConvertedFromHealthBar(TextMeshProUGUI tmp)
        {
            Text sourceText = GetSourceText(tmp);
            if (sourceText == null)
                return false;

            HealthBar healthBar = sourceText.transform.GetComponentInParent<HealthBar>();
            return healthBar != null && healthBar.hpText == sourceText;
        }

        /// <summary>
        /// Reapplies the active language's TMP font to a converted legacy text.
        /// The legacy source keeps the original font classification, which is used
        /// to pick the right replacement font again.
        /// </summary>
        public static bool RefreshConvertedFont(TextMeshProUGUI tmp)
        {
            Text source = GetSourceText(tmp);
            if (source == null)
                return false;

            TextMeshProFontSwap.SwapFont(tmp, source.font != null ? source.font.name : null);
            return true;
        }

        [HarmonyPatch(typeof(Text), "OnEnable")]
        public static class TextOnEnablePatch
        {
            [HarmonyPostfix]
            public static void Postfix(Text __instance)
            {
                if (__instance == null)
                    return;

                ConvertTextToTMP(__instance);
            }
        }

        [HarmonyPatch(typeof(Text), "set_text")]
        public static class TextSetTextPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Text __instance)
            {
                if (__instance == null)
                    return;

                SyncText(__instance);
            }
        }

        [HarmonyPatch(typeof(Text), "OnDisable")]
        public static class TextOnDisablePatch
        {
            [HarmonyPostfix]
            public static void Postfix(Text __instance)
            {
                if (__instance == null)
                    return;

                SetTMPActiveState(__instance, false);
            }
        }

        private static void ConvertTextToTMP(Text source)
        {
            if (source == null)
                return;

            if (IsUniverseLibCanvas(source))
                return;

            // The intermission vignette's shadow is a black backing Text behind
            // the main text. The backing is removed instead of converted; the
            // main text's TMP carries the shadow in its material instead (see
            // TextMeshProFontSwap.ApplyTextShadow).
            if (IsIntermissionShadowSource(source))
            {
                source.canvasRenderer.SetAlpha(0f);
                source.raycastTarget = false;
                return;
            }

            TextMeshProUGUI tmp = GetTMPForSource(source);
            if (tmp != null)
            {
                RefreshExistingConvertedText(source, tmp);
                return;
            }

            tmp = CreateTMPSibling(source);
            if (tmp == null)
                return;
            textToTMP[source.GetInstanceID()] = tmp;
            tmpToSourceText[tmp.GetInstanceID()] = source;

            if (IsFishingResultText(source))
                ApplyFishingTMP(source, tmp);
            else
                CopyTextProperties(source, tmp);

            SyncEffects(source, tmp);

            if (Core.TMPFontReady)
                TextMeshProFontSwap.SwapFont(tmp, source.font != null ? source.font.name : null);

            // The source keeps its place in the hierarchy (layout, animations),
            // but must no longer render or block raycasts.
            source.canvasRenderer.SetAlpha(0f);
            source.raycastTarget = false;

            SetTMPActiveState(source, source.isActiveAndEnabled);
        }

        private static void RefreshExistingConvertedText(Text source, TextMeshProUGUI converted)
        {
            CopyRectTransform(source.rectTransform, converted.rectTransform);
            CopyTextProperties(source, converted);
            SyncEffects(source, converted);
            source.canvasRenderer.SetAlpha(0f);
            source.raycastTarget = false;

            if (converted.gameObject.activeSelf != source.isActiveAndEnabled)
                converted.gameObject.SetActive(source.isActiveAndEnabled);
        }

        private static void SyncText(Text source)
        {
            TextMeshProUGUI tmp = GetTMPForSource(source);
            if (tmp == null)
                return;

            if (IsFishingResultText(source))
            {
                ApplyFishingTMP(source, tmp);
                return;
            }

            CopyRectTransform(source.rectTransform, tmp.rectTransform);
            CopyTextProperties(source, tmp);
        }

        private static void SetTMPActiveState(Text source, bool active)
        {
            TextMeshProUGUI tmp = GetTMPForSource(source);
            if (tmp == null)
                return;

            if (tmp.gameObject.activeSelf != active)
                tmp.gameObject.SetActive(active);
        }

        private static TextMeshProUGUI GetTMPForSource(Text source)
        {
            if (source == null)
                return null;

            int id = source.GetInstanceID();
            TextMeshProUGUI tmp;
            if (textToTMP.TryGetValue(id, out tmp) && tmp != null)
                return tmp;

            textToTMP.Remove(id);
            return null;
        }

        private static bool IsFishingResultText(Text source)
        {
            return source != null &&
                   (source.name == "Fish Caught Label" ||
                    source.name == "Fish Size Text");
        }

        /// <summary>
        /// True for the game's artificial intermission shadow: the black backing
        /// Text behind the vignette's main text ("PowerUpVignette/Panel/Panel
        /// (1)/Text" with a direct child "Text (1)"). The game draws the shadow
        /// as a second Text at the same rect; the converter removes it and the
        /// main text's TMP renders the shadow through its material instead.
        /// </summary>
        public static bool IsIntermissionShadowSource(Text source)
        {
            if (source == null || source.name != "Text" || source.transform.parent == null)
                return false;

            bool insideVignette = false;
            for (Transform ancestor = source.transform.parent; ancestor != null; ancestor = ancestor.parent)
            {
                if (ancestor.name == "PowerUpVignette")
                {
                    insideVignette = true;
                    break;
                }
            }
            if (!insideVignette)
                return false;

            for (int i = 0; i < source.transform.childCount; i++)
            {
                Transform child = source.transform.GetChild(i);
                if (child.name == "Text (1)" && child.GetComponent<Text>() != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// True for the visible text of the intermission vignette pair (the
        /// child of the black backing Text).
        /// </summary>
        public static bool IsIntermissionShadowChild(Text source)
        {
            if (source == null || source.name != "Text (1)" || source.transform.parent == null)
                return false;

            Text parentText = source.transform.parent.GetComponent<Text>();
            return parentText != null && IsIntermissionShadowSource(parentText);
        }

        /// <summary>
        /// True for a converted text whose source is the intermission vignette's
        /// main text; its TMP renders the game's shadow through its material.
        /// </summary>
        public static bool IsIntermissionMainText(TextMeshProUGUI tmp)
        {
            return IsIntermissionShadowChild(GetSourceText(tmp));
        }

        // The fishing HUD repositions its labels while showing them; the converted
        // sibling needs an immediate mesh update to follow along.
        private static void ApplyFishingTMP(Text source, TextMeshProUGUI tmp)
        {
            if (source == null || tmp == null)
                return;

            tmp.text = source.text;
            tmp.color = source.color;
            tmp.richText = source.supportRichText;
            tmp.alignment = ConvertAlignment(source.alignment);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableAutoSizing = false;
            tmp.fontSize = source.fontSize;
            tmp.fontSizeMax = source.fontSize;
            tmp.fontSizeMin = source.fontSize;
            tmp.rectTransform.localScale = Vector3.one;

            tmp.ForceMeshUpdate(true);
        }

        private static TextMeshProUGUI CreateTMPSibling(Text source)
        {
            if (source == null)
                return null;

            Transform parent = source.transform.parent;
            GameObject tmpObject = new GameObject($"TMP_for_{source.gameObject.name}_{source.GetInstanceID()}", typeof(RectTransform));
            tmpObject.transform.SetParent(parent, worldPositionStays: false);
            tmpObject.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            TextMeshProUGUI tmp = tmpObject.AddComponent<TextMeshProUGUI>();
            LayoutElement layoutElement = tmpObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            CopyRectTransform(source.rectTransform, tmp.rectTransform);

            return tmp;
        }

        private static void CopyRectTransform(RectTransform source, RectTransform destination)
        {
            if (source == null || destination == null)
                return;

            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.anchoredPosition = source.anchoredPosition;
            destination.sizeDelta = source.sizeDelta;
            destination.localScale = source.localScale;
            destination.localEulerAngles = source.localEulerAngles;
            destination.offsetMin = source.offsetMin;
            destination.offsetMax = source.offsetMax;
        }

        private static void CopyTextProperties(Text source, TextMeshProUGUI target)
        {
            if (source == null || target == null)
                return;

            target.text = source.text;
            target.fontSize = source.fontSize;
            target.color = source.color;
            target.richText = source.supportRichText;
            target.raycastTarget = source.raycastTarget;
            target.maskable = source.maskable;
            target.lineSpacing = source.lineSpacing;
            target.alignment = ConvertAlignment(source.alignment);
            target.enableWordWrapping = (source.horizontalOverflow == HorizontalWrapMode.Wrap);
            target.overflowMode = (source.verticalOverflow == VerticalWrapMode.Overflow) ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            target.enableAutoSizing = source.resizeTextForBestFit;
            if (source.resizeTextForBestFit)
            {
                target.fontSizeMin = source.resizeTextMinSize;
                target.fontSizeMax = source.resizeTextMaxSize;
            }
            target.fontStyle = ConvertFontStyle(source.fontStyle);
        }

        /// <summary>
        /// Carries the source's uGUI Shadow and Outline components over to the
        /// sibling, with their original colors and distances. This is how the game
        /// itself draws shadows on legacy texts (e.g. the navmesh warning uses
        /// several Shadow components stacked into an outline).
        /// </summary>
        private static void SyncEffects(Text source, TextMeshProUGUI target)
        {
            if (source == null || target == null)
                return;

            Shadow[] sourceShadows = source.GetComponents<Shadow>();
            Shadow[] targetShadows = target.GetComponents<Shadow>();

            if (sourceShadows.Length > 0 && sourceShadows.Length == targetShadows.Length)
            {
                // Steady state: update the existing copies in place.
                for (int i = 0; i < sourceShadows.Length; i++)
                {
                    if (targetShadows[i].GetType() != sourceShadows[i].GetType())
                    {
                        UnityEngine.Object.Destroy(targetShadows[i]);
                        targetShadows[i] = CreateShadowCopy(target, sourceShadows[i]);
                    }
                    CopyShadowSettings(sourceShadows[i], targetShadows[i]);
                }
                return;
            }

            for (int i = 0; i < targetShadows.Length; i++)
            {
                if (targetShadows[i] != null)
                    UnityEngine.Object.Destroy(targetShadows[i]);
            }

            for (int i = 0; i < sourceShadows.Length; i++)
            {
                CopyShadowSettings(sourceShadows[i], CreateShadowCopy(target, sourceShadows[i]));
            }
        }

        private static Shadow CreateShadowCopy(TextMeshProUGUI target, Shadow source)
        {
            return source is Outline
                ? (Shadow)target.gameObject.AddComponent<Outline>()
                : target.gameObject.AddComponent<Shadow>();
        }

        private static void CopyShadowSettings(Shadow source, Shadow target)
        {
            if (source == null || target == null)
                return;

            target.effectColor = source.effectColor;
            target.effectDistance = source.effectDistance;
            target.useGraphicAlpha = source.useGraphicAlpha;
        }

        private static bool IsUniverseLibCanvas(Text source)
        {
            if (source == null)
                return false;

            Transform current = source.transform;
            while (current != null)
            {
                string name = current.name;
                for (int i = 0; i < universeLibPatterns.Length; i++)
                    if (name.IndexOf(universeLibPatterns[i], StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;

                current = current.parent;
            }

            return false;
        }

        private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter:
                    return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight:
                    return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft:
                    return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter:
                    return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight:
                    return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft:
                    return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter:
                    return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight:
                    return TextAlignmentOptions.BottomRight;
                default:
                    return TextAlignmentOptions.TopLeft;
            }
        }

        private static FontStyles ConvertFontStyle(FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case FontStyle.Bold:
                    return FontStyles.Bold;
                case FontStyle.Italic:
                    return FontStyles.Italic;
                case FontStyle.BoldAndItalic:
                    return FontStyles.Bold | FontStyles.Italic;
                default:
                    return FontStyles.Normal;
            }
        }
    }
}
