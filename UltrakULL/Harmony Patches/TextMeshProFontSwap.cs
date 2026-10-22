using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UltrakULL.json;
using UltrakULL;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    /// <summary>
    /// Swaps the fonts of TMP texts to the active language's fonts and keeps the
    /// game's "Always On Top" HUD option working with them.
    ///
    /// Font choice mirrors the original game: HUD and boss bar labels use the main
    /// font, museum texts use the museum font, terminal screens use the terminal
    /// font, and languages with their own scripts (CJK, Japanese, Arabic, Hebrew)
    /// use their dedicated font assets.
    ///
    /// Materials are handled by <see cref="TMPFontMaterials"/>: the game's own HUD
    /// materials are used as presets with the mod font's atlas, and shadow (underlay)
    /// parameters are preserved from each text's original material instead of being
    /// fabricated.
    /// </summary>
    public static class TextMeshProFontSwap
    {
        // TMP components that already received the active language's font.
        // Assigned fonts and materials persist on the component, so the swap only
        // has to run once per component per scene load.
        private static readonly HashSet<IntPtr> swappedTexts = new HashSet<IntPtr>();

        // The game's own font of every swapped text, remembered from the first swap.
        // A later swap - a language switch, a reopened menu - has to classify the
        // text by the font the game put on it: the font that is on the text by then
        // is one of the mod's fonts, and those are created from files whose name
        // says nothing about the game's font classification (terminal, museum, ...).
        private static readonly Dictionary<TextMeshProUGUI, TMP_FontAsset> originalFonts = new Dictionary<TextMeshProUGUI, TMP_FontAsset>();

        // The game's own HUD text list, captured so texts that enable after the
        // Always On Top toggle can still be recognized as HUD texts.
        private static TMP_Text[] hudTextElements;

        // Texts the mod provides shadows for (the shadow policy texts). The
        // Always On Top toggle has to restyle them even when they are not part
        // of the game's own HUD text list.
        private static readonly HashSet<TextMeshProUGUI> managedShadowTexts = new HashSet<TextMeshProUGUI>();

        private static void RegisterManagedShadowText(TextMeshProUGUI text)
        {
            if (text != null)
                managedShadowTexts.Add(text);
        }

        private static void PruneDeadManagedShadowTexts()
        {
            if (managedShadowTexts.Count == 0)
                return;

            List<TextMeshProUGUI> dead = null;
            foreach (TextMeshProUGUI text in managedShadowTexts)
            {
                if (text == null)
                {
                    if (dead == null)
                        dead = new List<TextMeshProUGUI>();
                    dead.Add(text);
                }
            }

            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                managedShadowTexts.Remove(dead[i]);
        }

        // TMP components that belong to the mod's own UI. They manage their fonts
        // themselves and must never receive the translation's custom font, whose
        // character set does not need to cover the mod UI's texts.
        private static readonly Dictionary<IntPtr, TextMeshProUGUI> modUiTexts = new Dictionary<IntPtr, TextMeshProUGUI>();

        /// <summary>
        /// Marks a TMP component as part of the mod's own UI. Call right after
        /// creating and configuring the component.
        ///
        /// The material is normalized as well. Assigning the bundled font only
        /// points the renderer at the font's own material when the previous
        /// material's atlas differs, and that material ships with an opaque
        /// underlay that smudges small UI text. That state is reached whenever
        /// the label held another font before (the OnEnable swap that runs
        /// before this mark, a template cloned from a swapped game button) or
        /// had no material at all (a fresh component). A fresh clean preset -
        /// the same normalization the language switch applies - makes the
        /// creation path deterministic in every language.
        /// </summary>
        public static void MarkAsModUI(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            modUiTexts[GetTextPtr(text)] = text;

            TMPFontMaterials.RefreshMaterial(text, text.font);
        }

        // m_CachedPtr lives on UnityEngine.Component, so the field is resolved by
        // walking the hierarchy (Type.GetField does not look at base types).
        private static System.Reflection.FieldInfo cachedPtrField;

        private static IntPtr GetTextPtr(TextMeshProUGUI text)
        {
            if (cachedPtrField == null)
            {
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                System.Type type = typeof(TextMeshProUGUI);
                while (type != null && type != typeof(object))
                {
                    cachedPtrField = type.GetField("m_CachedPtr", flags);
                    if (cachedPtrField != null)
                        break;
                    type = type.BaseType;
                }
            }

            return cachedPtrField != null ? (IntPtr)cachedPtrField.GetValue(text) : IntPtr.Zero;
        }

        private static bool IsModUi(TextMeshProUGUI text)
        {
            if (text == null)
                return false;

            TextMeshProUGUI registered;
            return modUiTexts.TryGetValue(GetTextPtr(text), out registered);
        }

        public static void ClearFontSwapCache()
        {
            swappedTexts.Clear();
            hudTextElements = null;
            TMPFontMaterials.ClearCache();
            PruneDeadModUiTexts();
            PruneDeadManagedShadowTexts();
            TerminalFontScale = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.metadata != null
                ? LanguageManager.CurrentLanguage.metadata.tmFontSize
                : 100;
            PruneDeadOriginalFontSizes();
            PruneDeadOriginalFonts();
        }

        /// <summary>
        /// Drops the remembered fonts of the texts that are gone. The fonts of the
        /// texts that are still alive are kept: they are the fonts of the game and
        /// do not belong to any language.
        /// </summary>
        private static void PruneDeadOriginalFonts()
        {
            if (originalFonts.Count == 0)
                return;

            List<TextMeshProUGUI> dead = null;
            foreach (KeyValuePair<TextMeshProUGUI, TMP_FontAsset> pair in originalFonts)
            {
                if (pair.Key == null)
                {
                    if (dead == null)
                        dead = new List<TextMeshProUGUI>();
                    dead.Add(pair.Key);
                }
            }

            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                originalFonts.Remove(dead[i]);
        }

        // Entries of destroyed texts are dropped; Unity nulls out destroyed
        // components, so a stale IntPtr never blocks a newly created component.
        private static void PruneDeadModUiTexts()
        {
            if (modUiTexts.Count == 0)
                return;

            List<IntPtr> dead = null;
            foreach (KeyValuePair<IntPtr, TextMeshProUGUI> pair in modUiTexts)
            {
                if (pair.Value == null)
                {
                    if (dead == null)
                        dead = new List<IntPtr>();
                    dead.Add(pair.Key);
                }
            }

            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                modUiTexts.Remove(dead[i]);
        }

        // The font swap only applies when a translation is active. The main menu is
        // exempt because the mod's own UI (language menu, notifications) lives there.
        private static bool CanSwapFonts()
        {
            return !CommonFunctions.isUsingEnglish() || CommonFunctions.GetCurrentSceneName() == "Main Menu";
        }

        /// <summary>
        /// Applies the active language's font to a single TMP text.
        /// </summary>
        /// <param name="text">Text to swap the font of.</param>
        /// <param name="originalFontName">
        /// Font name of the legacy Text this TMP was converted from, when known.
        /// Used to keep museum texts on the museum font.
        /// </param>
        public static void SwapFont(TextMeshProUGUI text, string originalFontName = null)
        {
            if (text == null || !Core.TMPFontReady || Core.GlobalFontTMP == null)
                return;

            // The mod's own UI manages its fonts itself, and shadow twins are
            // synced by their controller.
            if (IsModUi(text) || TMPShadowTwin.IsShadowTwin(text))
                return;

            // Slot machine style texts use a bitmap font; replacing it breaks them.
            string currentText = text.text;
            if (currentText != null && currentText.Contains("■") && currentText.Contains("|"))
                return;

            // The font of the game is remembered before it is replaced, so the next
            // swap still classifies the text by the game's own font.
            if (originalFontName == null)
                RememberGameFont(text);

            TMP_FontAsset font = ResolveFont(text, originalFontName);
            if (font == null)
                return;

            ApplyTextShadow(text, font);

            if (font == Core.CustomTerminalFontTMP)
                ApplyTerminalFontScale(text);
        }

        /// <summary>
        /// Resolves the shadow styles from a text's uGUI Shadow/Outline
        /// components. TMP does not render those components, so their exact
        /// colors and canvas-space distances are drawn by shadow twins instead -
        /// this reproduces native looks like the navmesh warning's stacked
        /// shadows. Returns null when the text has no such components.
        /// </summary>
        private static List<TMPShadowTwin.ShadowStyle> ResolveComponentStyles(TextMeshProUGUI text)
        {
            Shadow[] shadows = text != null ? text.GetComponents<Shadow>() : null;
            if (shadows == null || shadows.Length == 0)
                return null;

            List<TMPShadowTwin.ShadowStyle> styles = null;
            for (int i = 0; i < shadows.Length; i++)
            {
                Shadow shadow = shadows[i];
                if (shadow == null)
                    continue;

                if (styles == null)
                    styles = new List<TMPShadowTwin.ShadowStyle>();

                Outline outline = shadow as Outline;
                if (outline != null)
                {
                    // uGUI Outline draws four copies at the corners of the distance.
                    Vector2 d = outline.effectDistance;
                    styles.Add(MakeComponentStyle(outline, new Vector2(d.x, d.y)));
                    styles.Add(MakeComponentStyle(outline, new Vector2(-d.x, d.y)));
                    styles.Add(MakeComponentStyle(outline, new Vector2(d.x, -d.y)));
                    styles.Add(MakeComponentStyle(outline, new Vector2(-d.x, -d.y)));
                }
                else
                {
                    styles.Add(MakeComponentStyle(shadow, shadow.effectDistance));
                }
            }

            return styles;
        }

        private static TMPShadowTwin.ShadowStyle MakeComponentStyle(Shadow shadow, Vector2 offset)
        {
            return new TMPShadowTwin.ShadowStyle
            {
                Color = shadow.effectColor,
                CanvasOffset = offset,
                UseSourceAlpha = shadow.useGraphicAlpha
            };
        }

        /// <summary>
        /// The base material for overlay ("Always On Top") presets. The mod
        /// bundle's overlay material is used because its shader copy contains the
        /// compiled UNDERLAY variant, which the game's own overlay shader lacks -
        /// an underlay configured on a preset cloned from the game's material
        /// would silently not render.
        /// </summary>
        private static Material OverlayBaseMaterial(Material gameOverlayMaterial)
        {
            return Core.GlobalFontTMPOverlayMat != null ? Core.GlobalFontTMPOverlayMat : gameOverlayMaterial;
        }

        /// <summary>
        /// Applies the font together with the text's shadow.
        ///
        /// Component shadows (uGUI Shadow/Outline) are drawn by twins - TMP does
        /// not render those components. Underlay shadows (the mod's managed
        /// shadows and preserved material underlays) use the shader underlay in
        /// both modes; the overlay presets are cloned from the mod bundle's
        /// overlay material, whose shader renders the underlay.
        /// </summary>
        // The intermission vignette shadow style the mod has always used: black,
        // semi-transparent, offset down-right in canvas units. The game's own
        // shadow there is a black backing Text (removed by the converter); the
        // main text's TMP renders this shadow through its material instead.
        private static readonly Vector2 IntermissionShadowCanvasOffset = new Vector2(2f, -2.75f);
        private const float IntermissionShadowAlpha = 0.45f;

        private static void ApplyTextShadow(TextMeshProUGUI text, TMP_FontAsset font)
        {
            if (text == null)
                return;

            if (font == null)
                font = text.font;
            if (font == null || font.material == null)
                return;

            // The intermission vignette is a screen-space canvas, not a HUD
            // element: it never follows the "Always On Top" state.
            bool intermission = TextToTMPConverter.IsIntermissionMainText(text);
            bool overlay = !intermission && HudControllerPatch.isOverlaid;
            List<TMPShadowTwin.ShadowStyle> componentStyles = ResolveComponentStyles(text);

            TMPFontMaterials.UnderlayParams style;
            bool hasStyle;
            if (intermission)
            {
                style = TMPFontMaterials.CanvasShadowStyle(text, font, IntermissionShadowCanvasOffset, IntermissionShadowAlpha);
                hasStyle = true;
            }
            else if (RequiresManagedShadow(text.transform))
            {
                hasStyle = CopyGameShadowStyle(out style);
            }
            else
            {
                hasStyle = TMPFontMaterials.TryGetPreservedUnderlay(text, out style);
            }

            if ((componentStyles != null || hasStyle) && !intermission)
                RegisterManagedShadowText(text);

            bool followsOverlayState = !intermission &&
                (componentStyles != null || hasStyle || IsHudText(text) || managedShadowTexts.Contains(text));

            HudController hud = HudController.Instance;
            Material overlayBase = overlay && followsOverlayState && hud != null && hud.overlayTextMaterial != null
                ? OverlayBaseMaterial(hud.overlayTextMaterial)
                : null;

            if (componentStyles != null)
            {
                DisableComponentShadows(text);
                TMPFontMaterials.ApplyFont(text, font, overlayBase, null);
                TMPShadowTwin.Set(text, componentStyles);
                return;
            }

            if (hasStyle)
                DisableComponentShadows(text);

            TMPFontMaterials.ApplyFont(text, font, overlayBase, hasStyle ? style : (TMPFontMaterials.UnderlayParams?)null);
            TMPShadowTwin.Set(text, null);
        }

        private static bool CopyGameShadowStyle(out TMPFontMaterials.UnderlayParams style)
        {
            style = TMPFontMaterials.GameShadowStyle();
            return true;
        }

        private static TMP_FontAsset ResolveFont(TMP_Text text, string originalFontName)
        {
            TMP_FontAsset mainFont = Core.CustomMainFontTMP ?? Core.GlobalFontTMP;

            // HUD and boss bar labels always use the main font.
            if (IsHudText(text))
                return mainFont;

            TMP_FontAsset museumFont = Core.CustomMuseumFontTMP ?? Core.MuseumFontTMP ?? mainFont;
            TMP_FontAsset terminalFont = Core.CustomTerminalFontTMP ?? Core.GlobalFontTMP;
            TMP_FontAsset secretTerminalFont = Core.CustomSecretTerminalFontTMP ?? Core.GlobalFontTMP;

            // Converted legacy texts keep their font classification: the museum font
            // is identified by the font the game used on the Text component.
            if (!string.IsNullOrEmpty(originalFontName) &&
                (originalFontName.IndexOf("Garaldus", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 originalFontName.IndexOf("Garamond", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return museumFont;
            }

            // The font the game itself used decides the replacement: the terminal
            // screens are recognized by their Tahoma font, the secret terminal by
            // its pixel font. The font that is on the text right now is not
            // reliable for this - after the first swap it is one of the mod's
            // fonts - so the remembered font of the game is used, and for a
            // converted legacy text the font of its source.
            string classifyFontName = !string.IsNullOrEmpty(originalFontName)
                ? originalFontName
                : GetGameFontName(text);
            if (!string.IsNullOrEmpty(classifyFontName))
            {
                string classifyLower = classifyFontName.ToLower();
                if (classifyLower.Contains("tahoma"))
                    return terminalFont;
                // The secret terminal is recognized by its pixel font, not by
                // object names: names like "Secret Mission" in the level select
                // must not get the secret terminal font.
                if (classifyLower.Contains("bittypix"))
                    return secretTerminalFont;
            }

            // Terminal screens are also identified by their object names, which
            // works no matter which font the game put on the text. Objects with
            // "secret" in the name are excluded, like the original implementation.
            if (UsesTerminalObject(text) && terminalFont != Core.GlobalFontTMP)
            {
                return terminalFont;
            }

            switch (GetLanguageCode())
            {
                case "zh":
                    return Core.CJKFontTMP;
                case "ja":
                    return Core.JaFontTMP;
                case "ar":
                case "fa":
                case "ur":
                    FlipAlignmentForRtl(text);
                    EnsureArabicFallback();
                    return UsesMuseumFont(text) ? museumFont : mainFont;
                case "jr":
                case "he":
                case "yi":
                case "la":
                case "ro":
                    return Core.HebrewFontTMP;
                default:
                    return UsesMuseumFont(text) ? museumFont : mainFont;
            }
        }

        private static string GetLanguageCode()
        {
            string langName = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.metadata != null
                ? LanguageManager.CurrentLanguage.metadata.langName
                : null;
            if (string.IsNullOrEmpty(langName))
                return string.Empty;
            langName = langName.ToLower();
            return langName.Length >= 2 ? langName.Substring(0, 2) : langName;
        }

        // The museum scene labels its exhibits with the museum font; those labels
        // keep it after the swap.
        private static bool UsesMuseumFont(TMP_Text text)
        {
            // The font of the game decides this, not the font that is on the text
            // after the swap.
            return CommonFunctions.GetCurrentSceneName() == "CreditsMuseum2" &&
                GetGameFontName(text) == "GFS Garaldus";
        }

        private static bool UsesTerminalObject(TMP_Text text)
        {
            // The texts of a terminal sit deep inside its canvas ("shopTerminal" >
            // "Canvas" > "Background" > ...), so the whole chain of parents is
            // checked, not only the direct parent.
            for (Transform current = text.transform; current != null; current = current.parent)
            {
                if (ContainsName(current.name, "secret"))
                {
                    // The secret terminal is recognized by its pixel font instead.
                    return false;
                }

                if (ContainsName(current.name, "terminal"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Remembers the font the game itself put on the text. Called before the
        /// first swap replaces it; later swaps keep the remembered font.
        /// </summary>
        private static void RememberGameFont(TextMeshProUGUI text)
        {
            if (text == null || originalFonts.ContainsKey(text) || text.font == null)
                return;

            originalFonts[text] = text.font;
        }

        /// <summary>
        /// The name of the font the game itself used on the text: the remembered one
        /// when the mod already swapped it, the current one otherwise.
        /// </summary>
        private static string GetGameFontName(TMP_Text text)
        {
            TextMeshProUGUI ugui = text as TextMeshProUGUI;
            if (ugui != null)
            {
                TMP_FontAsset remembered;
                if (originalFonts.TryGetValue(ugui, out remembered) && remembered != null)
                    return remembered.name;
            }

            return text != null && text.font != null ? text.font.name : null;
        }

        /// <summary>
        /// True for the texts the mod provides HUD/menu shadows for, in the game's
        /// own shadow style (see TMPFontMaterials.GameShadowStyle). The list mirrors
        /// the original mod's shadow policy. uGUI Shadow/Outline components are
        /// included on purpose: like the original mod, they are converted to the
        /// game-style underlay (their native look is far subtler than expected).
        /// </summary>
        private static bool RequiresManagedShadow(Transform target)
        {
            if (target == null)
                return false;

            // Converted legacy texts are named "TMP_for_..." after their creation,
            // so the policy is evaluated against the original Text as well.
            TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                Text sourceText = TextToTMPConverter.GetSourceText(tmp);
                if (sourceText != null && !ReferenceEquals(sourceText.transform, target) &&
                    RequiresManagedShadow(sourceText.transform))
                {
                    return true;
                }
            }

            string name = target.name;

            // Level select labels.
            if (ContainsName(name, "NameText") || ContainsName(name, "LayerText"))
                return true;

            // The cheats info panel.
            for (Transform current = target; current != null; current = current.parent)
            {
                if (ContainsName(current.name, "Cheats Info"))
                    return true;
            }

            // Scanning progress text.
            if (name == "Text (1)" &&
                target.parent != null && target.parent.name == "Panel" &&
                target.parent.parent != null && target.parent.parent.name == "ReadingScanned")
            {
                return true;
            }

            // Navmesh warning.
            if (name == "Text (1)" &&
                target.parent != null && target.parent.name == "Navmesh Warning" &&
                target.parent.parent != null && target.parent.parent.name == "Canvas")
            {
                return true;
            }

            if (name.Equals("CutsceneSkipText", StringComparison.OrdinalIgnoreCase) &&
                target.parent != null && target.parent.name.Equals("Canvas", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Alt HUD health title.
            if (ContainsName(name, "Title") &&
                target.parent != null && ContainsName(target.parent.name, "Health"))
            {
                for (Transform current = target.parent; current != null; current = current.parent)
                {
                    if (ContainsName(current.name, "AltHud"))
                        return true;
                }
            }

            // HUD values: health bars (value, symbol, plus) and the speedometer.
            if (target.GetComponentInParent<HealthBar>() != null ||
                target.GetComponentInParent<Speedometer>() != null)
            {
                return true;
            }

            // Texts carrying uGUI shadow components (e.g. the navmesh warning's
            // triple shadow) are converted to the underlay, like the original mod.
            return target.GetComponent<Shadow>() != null;
        }

        /// <summary>
        /// Disables a text's uGUI Shadow/Outline components. Used when the
        /// game-style underlay is applied, so the shadow is not drawn twice.
        /// </summary>
        private static void DisableComponentShadows(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            Shadow[] shadows = text.GetComponents<Shadow>();
            for (int i = 0; i < shadows.Length; i++)
            {
                if (shadows[i] != null && shadows[i].enabled)
                    shadows[i].enabled = false;
            }
        }

        private static bool ContainsName(string value, string expected)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Arabic (and Persian/Urdu) texts are mirrored: the horizontal alignment
        // has to be flipped so right-aligned labels stay on the correct side.
        private static void FlipAlignmentForRtl(TMP_Text text)
        {
            switch ((int)text.alignment)
            {
                case 257:
                    text.alignment = (TextAlignmentOptions)260;
                    break;
                case 513:
                    text.alignment = (TextAlignmentOptions)516;
                    break;
                case 1025:
                    text.alignment = (TextAlignmentOptions)1028;
                    break;
                case 2049:
                    text.alignment = (TextAlignmentOptions)2052;
                    break;
            }
        }

        private static void EnsureArabicFallback()
        {
            if (Core.GlobalFontTMP != null &&
                Core.ArabicFontTMP != null &&
                !Core.GlobalFontTMP.fallbackFontAssetTable.Contains(Core.ArabicFontTMP))
            {
                Core.GlobalFontTMP.fallbackFontAssetTable.Add(Core.ArabicFontTMP);
            }
        }

        /// <summary>
        /// True for texts that belong to the HUD and therefore follow the
        /// "Always On Top" option. Mirrors the sets the game itself covers
        /// (textElements) plus the ones the game handles per component
        /// (health bars, boss bars, speedometer, style meter).
        /// </summary>
        private static bool IsHudText(TMP_Text text)
        {
            if (text == null)
                return false;

            if (hudTextElements != null && Array.IndexOf(hudTextElements, text) >= 0)
                return true;

            Transform transform = text.transform;
            if (transform.GetComponentInParent<HealthBar>() != null ||
                transform.GetComponentInParent<BossHealthBar>() != null ||
                transform.GetComponentInParent<Speedometer>() != null ||
                transform.GetComponentInParent<StyleHUD>() != null)
            {
                return true;
            }

            // A converted legacy hp label is a sibling of its source, recognized
            // through the conversion map instead of the hierarchy.
            TextMeshProUGUI ugui = text as TextMeshProUGUI;
            return ugui != null && TextToTMPConverter.IsConvertedFromHealthBar(ugui);
        }

        [HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
        public static class TextMeshProFontSwapper
        {
            [HarmonyPostfix]
            public static void SwapFont(TextMeshProUGUI __instance, IntPtr ___m_CachedPtr)
            {
                if (__instance == null)
                    return;

                // Shadow twins are synced by their controller.
                if (TMPShadowTwin.IsShadowTwin(__instance))
                    return;

                if (!swappedTexts.Contains(___m_CachedPtr) && Core.TMPFontReady && CanSwapFonts())
                {
                    TextMeshProFontSwap.SwapFont(__instance);
                    swappedTexts.Add(___m_CachedPtr);
                }
                else if (Core.TMPFontReady && !CanSwapFonts())
                {
                    // English: no font swap, but the mod's managed shadows apply
                    // in every language.
                    ApplyTextShadow(__instance, null);
                }

                // HUD texts that enable after the Always On Top toggle missed the
                // bulk pass, so they get their overlay material here. Runs in any
                // language: texts still on the game's own fonts already render
                // correctly and are left alone by the material logic.
                if (Core.TMPFontReady && HudControllerPatch.isOverlaid)
                    HudControllerPatch.TryApplyOverlayMaterial(__instance);
            }
        }

        [HarmonyPatch(typeof(HudController))]
        public static class HudControllerPatch
        {
            private static bool? isOverlaidCached;

            public static bool isOverlaid
            {
                get
                {
                    if (!isOverlaidCached.HasValue)
                    {
                        try
                        {
                            isOverlaidCached = MonoSingleton<PrefsManager>.Instance.GetBool("hudAlwaysOnTop", false);
                        }
                        catch
                        {
                            isOverlaidCached = false;
                        }
                    }
                    return isOverlaidCached.Value;
                }
                set { isOverlaidCached = value; }
            }

            /// <summary>
            /// Replaces HudController.SetAlwaysOnTop with the same native logic the
            /// game uses (assigning a shared HUD material to every HUD text), but
            /// with materials that always carry the atlas of the font each text
            /// actually uses. The original assignment hands every text the game's
            /// own VCR atlas, which breaks rendering for mod fonts - including the
            /// mod's fonts that stay on texts after switching the language to
            /// English. Runs for every language; with the game's own fonts the
            /// applied materials are identical to the game's, so unmodded English
            /// is unaffected.
            /// </summary>
            [HarmonyPatch("SetAlwaysOnTop")]
            [HarmonyPrefix]
            public static bool SetAlwaysOnTop_Prefix(
                HudController __instance,
                TMP_Text[] ___textElements,
                bool onTop,
                Material ___overlayTextMaterial,
                Material ___normalTextMaterial)
            {
                isOverlaid = onTop;
                hudTextElements = ___textElements;

                try
                {
                    ApplyToAllHudTexts(__instance, onTop, ___overlayTextMaterial, ___normalTextMaterial, ___textElements);
                }
                catch (Exception e)
                {
                    Logging.Warn("Failed to apply Always On Top font materials.");
                    Logging.Warn(e.ToString());
                }
                return false;
            }

            /// <summary>
            /// Assigns the HUD material for the current Always On Top state to one
            /// text, preserving or providing its shadow. Component shadows are
            /// drawn by twins; underlay shadows use the shader underlay in both
            /// modes (overlay presets are cloned from the mod bundle's overlay
            /// material, whose shader renders the underlay).
            /// </summary>
            public static void ApplyHudMaterial(TMP_Text text, Material gameBaseMaterial, bool isOverlay)
            {
                if (text == null || text.font == null || gameBaseMaterial == null)
                    return;

                TextMeshProUGUI ugui = text as TextMeshProUGUI;
                if (ugui == null || TMPShadowTwin.IsShadowTwin(ugui))
                    return;

                Material baseMaterial = isOverlay ? OverlayBaseMaterial(gameBaseMaterial) : gameBaseMaterial;

                List<TMPShadowTwin.ShadowStyle> componentStyles = ResolveComponentStyles(ugui);
                if (componentStyles != null)
                {
                    RegisterManagedShadowText(ugui);
                    DisableComponentShadows(ugui);
                    TMPFontMaterials.ApplyFont(ugui, text.font, baseMaterial, null);
                    TMPShadowTwin.Set(ugui, componentStyles);
                    return;
                }

                TMPFontMaterials.UnderlayParams style;
                bool hasStyle = RequiresManagedShadow(ugui.transform)
                    ? CopyGameShadowStyle(out style)
                    : TMPFontMaterials.TryGetPreservedUnderlay(ugui, out style);

                if (hasStyle)
                {
                    RegisterManagedShadowText(ugui);
                    DisableComponentShadows(ugui);
                }

                TMPFontMaterials.ApplyFont(ugui, text.font, baseMaterial, hasStyle ? style : (TMPFontMaterials.UnderlayParams?)null);
                TMPShadowTwin.Set(ugui, null);
            }

            private static void ApplyHudMaterial(TMP_Text text, bool onTop, Material overlayMat, Material normalMat)
            {
                ApplyHudMaterial(text, onTop ? overlayMat : normalMat, onTop);
            }

            /// <summary>
            /// Applies the HUD materials to every HUD text in the scene: the game's
            /// own textElements plus the texts of health bars, boss bars, the
            /// speedometer and the style meter, which the game handles per component.
            /// </summary>
            public static void ApplyToAllHudTexts(
                HudController hud,
                bool onTop,
                Material overlayMat,
                Material normalMat,
                TMP_Text[] textElements = null)
            {
                Material baseMaterial = onTop ? overlayMat : normalMat;
                if (baseMaterial == null)
                    return;

                HashSet<TMP_Text> processed = new HashSet<TMP_Text>();

                if (textElements != null)
                {
                    foreach (TMP_Text text in textElements)
                    {
                        if (text != null && processed.Add(text))
                            ApplyHudMaterial(text, baseMaterial, onTop);
                    }
                }

                foreach (HealthBar healthBar in UnityEngine.Object.FindObjectsOfType<HealthBar>())
                {
                    if (healthBar == null)
                        continue;
                    CollectHealthBarTexts(healthBar, processed, baseMaterial, onTop);
                }

                foreach (BossHealthBar bossBar in UnityEngine.Object.FindObjectsOfType<BossHealthBar>())
                {
                    if (bossBar == null)
                        continue;
                    foreach (TMP_Text text in bossBar.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (text != null && processed.Add(text))
                            ApplyHudMaterial(text, baseMaterial, onTop);
                    }
                }

                foreach (Speedometer speedometer in UnityEngine.Object.FindObjectsOfType<Speedometer>())
                {
                    if (speedometer == null || speedometer.textMesh == null)
                        continue;
                    if (processed.Add(speedometer.textMesh))
                        ApplyHudMaterial(speedometer.textMesh, baseMaterial, onTop);
                }

                foreach (StyleHUD styleHud in UnityEngine.Object.FindObjectsOfType<StyleHUD>())
                {
                    if (styleHud == null)
                        continue;
                    foreach (TMP_Text text in styleHud.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (text != null && processed.Add(text))
                            ApplyHudMaterial(text, baseMaterial, onTop);
                    }
                }

                // Managed-shadow texts outside the game's own HUD sets (Cheats
                // Info, the navmesh warning, ...) follow the Always On Top state
                // as well.
                foreach (TextMeshProUGUI text in managedShadowTexts)
                {
                    if (text != null && processed.Add(text))
                        ApplyHudMaterial(text, baseMaterial, onTop);
                }
            }

            /// <summary>
            /// Overlay material for a single text that enabled while Always On Top
            /// is active. Cheap membership check first; non-HUD texts exit here.
            /// </summary>
            public static void TryApplyOverlayMaterial(TMP_Text text)
            {
                if (!isOverlaid)
                    return;

                TextMeshProUGUI ugui = text as TextMeshProUGUI;
                if (!IsHudText(text) && (ugui == null || !managedShadowTexts.Contains(ugui)))
                    return;

                HudController hud = HudController.Instance;
                if (hud == null || hud.overlayTextMaterial == null)
                    return;

                ApplyHudMaterial(text, hud.overlayTextMaterial, true);
            }
        }

        /// <summary>
        /// Applies the HUD material to the hp label and every TMP text of a health
        /// bar. The hp label may be a converted legacy Text; its TMP sibling is
        /// found through the conversion map.
        /// </summary>
        private static void CollectHealthBarTexts(HealthBar healthBar, HashSet<TMP_Text> processed, Material baseMaterial, bool isOverlay)
        {
            TextMeshProUGUI hpText = healthBar.hpText as TextMeshProUGUI;
            if (hpText == null && healthBar.hpText != null)
            {
                Text legacyText = healthBar.hpText.gameObject.GetComponent<Text>();
                if (legacyText != null)
                    hpText = TextToTMPConverter.GetConvertedTMP(legacyText);
            }

            if (hpText != null && processed.Add(hpText))
                HudControllerPatch.ApplyHudMaterial(hpText, baseMaterial, isOverlay);

            foreach (TMP_Text text in healthBar.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text != null && processed.Add(text))
                    HudControllerPatch.ApplyHudMaterial(text, baseMaterial, isOverlay);
            }
        }

        [HarmonyPatch(typeof(HealthBar), "Start")]
        public static class HealthBarStartPatch
        {
            [HarmonyPostfix]
            public static void Start_Postfix(HealthBar __instance)
            {
                if (__instance == null)
                    return;

                HudController hud = HudController.Instance;
                if (hud == null)
                    return;

                Material baseMaterial = HudControllerPatch.isOverlaid
                    ? hud.overlayTextMaterial
                    : hud.normalTextMaterial;
                if (baseMaterial == null)
                    return;

                HashSet<TMP_Text> processed = new HashSet<TMP_Text>();
                CollectHealthBarTexts(__instance, processed, baseMaterial, HudControllerPatch.isOverlaid);
            }
        }

        [HarmonyPatch(typeof(Speedometer), "OnEnable")]
        public static class SpeedometerOnEnablePatch
        {
            [HarmonyPostfix]
            public static void OnEnable_Postfix(Speedometer __instance)
            {
                HudController hud = HudController.Instance;
                if (hud == null || __instance == null || __instance.textMesh == null)
                    return;

                Material baseMaterial = HudControllerPatch.isOverlaid
                    ? hud.overlayTextMaterial
                    : hud.normalTextMaterial;
                if (baseMaterial != null)
                    HudControllerPatch.ApplyHudMaterial(__instance.textMesh, baseMaterial, HudControllerPatch.isOverlaid);

                if (HudControllerPatch.isOverlaid && !__instance.gameObject.activeSelf)
                    __instance.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(Speedometer), "OnPrefChanged")]
        public static class SpeedometerPrefPatch
        {
            [HarmonyPostfix]
            public static void OnPrefChanged_Postfix(Speedometer __instance, string id, object value)
            {
                if (id != "speedometer" || !HudControllerPatch.isOverlaid)
                    return;

                HudController hud = HudController.Instance;
                if (hud == null || __instance == null || __instance.textMesh == null)
                    return;

                HudControllerPatch.ApplyHudMaterial(__instance.textMesh, hud.overlayTextMaterial, true);
            }
        }

        [HarmonyPatch(typeof(HudController), "Start")]
        public static class HudControllerStartPatch
        {
            [HarmonyPostfix]
            public static void Start_Postfix(HudController __instance)
            {
                ReapplyHudMaterials(__instance);
            }
        }

        [HarmonyPatch(typeof(HudController), "OnPrefChanged")]
        public static class HudControllerPrefPatch
        {
            [HarmonyPostfix]
            public static void OnPrefChanged_Postfix(HudController __instance, string key, object value)
            {
                // The HUD type switch swaps the whole HUD hierarchy; the newly
                // activated texts need the HUD material again.
                if (key == "hudType")
                    ReapplyHudMaterials(__instance);
            }
        }

        private static void ReapplyHudMaterials(HudController hud)
        {
            if (hud == null)
                return;

            HudControllerPatch.ApplyToAllHudTexts(
                hud,
                HudControllerPatch.isOverlaid,
                hud.overlayTextMaterial,
                hud.normalTextMaterial,
                hud.textElements);
        }

        [HarmonyPatch(typeof(SubtitleController))]
        public static class SubtitleFontSwapper
        {
            [HarmonyPatch("DisplaySubtitle", new Type[]
            {
                typeof(string),
                typeof(AudioSource),
                typeof(bool)
            })]
            [HarmonyPrefix]
            public static bool SubtitlePostfix(SubtitleController __instance, string caption, AudioSource audioSource, bool ignoreSetting, Subtitle ___subtitleLine, Transform ___container, Subtitle ___previousSubtitle)
            {
                if (!__instance.SubtitlesEnabled && !ignoreSetting)
                {
                    return false;
                }
                Subtitle val = UnityEngine.Object.Instantiate<Subtitle>(___subtitleLine, ___container, true);
                ((Component)val).GetComponentInChildren<TMP_Text>().text = caption;

                // The key of the line is looked up by its text, so the subtitle can be
                // written again in another language while it is on screen.
                UltrakULL.LiveSubtitles.OnDisplayed(val, caption);

                TextMeshProUGUI subtitleText = ((Component)val).GetComponentInChildren<TextMeshProUGUI>();
                if (Core.TMPFontReady && CanSwapFonts())
                {
                    // The subtitle keeps the font of the game in built-in English;
                    // every other language gets the font of the translation.
                    SwapFont(subtitleText);
                }
                if (audioSource != null)
                {
                    val.distanceCheckObject = audioSource;
                }
                ((Component)val).gameObject.SetActive(true);
                if (___previousSubtitle == null)
                {
                    val.ContinueChain();
                }
                else
                {
                    ___previousSubtitle.nextInChain = val;
                }
                ___previousSubtitle = val;
                return false;
            }
        }

        // Terminal fonts can be sized independently of the rest of the UI through
        // the language's tmFontSize setting.
        private sealed class OriginalFontSize
        {
            public float Size;
            public float Min;
            public float Max;
            public bool AutoSized;
        }

        private static readonly Dictionary<TextMeshProUGUI, OriginalFontSize> OriginalFontSizes = new Dictionary<TextMeshProUGUI, OriginalFontSize>();

        public static int TerminalFontScale = (LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.metadata != null)
            ? LanguageManager.CurrentLanguage.metadata.tmFontSize
            : 100;

        private static void ApplyTerminalFontScale(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return;

            float scale = TerminalFontScale / 100f;

            OriginalFontSize original;
            if (!OriginalFontSizes.TryGetValue(tmp, out original))
            {
                original = new OriginalFontSize
                {
                    Size = tmp.fontSize,
                    Min = tmp.fontSizeMin,
                    Max = tmp.fontSizeMax,
                    AutoSized = tmp.enableAutoSizing
                };
                OriginalFontSizes[tmp] = original;
            }

            // The size is always computed from the size of the game, so applying the
            // scale again - a language switch, a menu that is reopened - never
            // compounds it.
            tmp.fontSize = original.Size * scale;

            if (original.AutoSized)
            {
                tmp.fontSizeMin = original.Min * scale;
                tmp.fontSizeMax = original.Max * scale;
            }
        }

        /// <summary>
        /// Drops the original font sizes of the components that are gone. The ones
        /// that are still alive keep their original size: a language switch has to
        /// apply the new scale to the size of the game, not to the size that is
        /// already scaled by the previous language.
        /// </summary>
        private static void PruneDeadOriginalFontSizes()
        {
            if (OriginalFontSizes.Count == 0)
                return;

            List<TextMeshProUGUI> dead = null;
            foreach (KeyValuePair<TextMeshProUGUI, OriginalFontSize> pair in OriginalFontSizes)
            {
                if (pair.Key == null)
                {
                    if (dead == null)
                        dead = new List<TextMeshProUGUI>();
                    dead.Add(pair.Key);
                }
            }

            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                OriginalFontSizes.Remove(dead[i]);
        }

        /// <summary>
        /// Applies the newly selected language's font assets to all TMP components
        /// that already exist in the active game scene. Normally the font swap runs
        /// from OnEnable, which leaves an open UI on the previous language's custom
        /// font until it is closed and opened again.
        /// </summary>
        public static void ReapplyActiveSceneFonts()
        {
            if (!Core.TMPFontReady)
                return;

            Scene activeScene = SceneManager.GetActiveScene();
            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true);
            int applied = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshProUGUI text = texts[i];
                if (text == null || text.gameObject.scene != activeScene)
                    continue;

                try
                {
                    // Converted legacy texts always need the mod's font (their TMP
                    // siblings start on TMP's default font). Native texts keep the
                    // game's own fonts in English, so they are only swapped when a
                    // translation is active. The mod's own UI and shadow twins are
                    // skipped entirely.
                    if (IsModUi(text) || TMPShadowTwin.IsShadowTwin(text))
                        continue;

                    if (!TextToTMPConverter.RefreshConvertedFont(text) && CanSwapFonts())
                        SwapFont(text);
                    applied++;
                }
                catch (Exception e)
                {
                    Logging.Warn("[TextMeshProFontSwap] Failed to refresh a live TMP font: " + e.Message);
                }
            }

            if (applied > 0)
                Logging.Message("[TextMeshProFontSwap] Refreshed " + applied + " TMP font(s) for the active language.");

            // The mod's own UI is skipped by the loop above by design: its texts
            // keep the bundled font. But their material state is a snapshot from
            // the moment they were created, and a language switch can leave that
            // snapshot behind (a cleared preset, a material of the previous
            // language). Their material is refreshed here the same way.
            ReapplyModUiFonts();

            // The font swap above resets materials to the fonts' own, so the HUD
            // texts need their Always On Top materials back.
            ReapplyHudMaterials(HudController.Instance);
        }

        /// <summary>
        /// Applies a fresh clean material to every text of the mod's own UI.
        ///
        /// The mod UI texts manage their fonts themselves and are skipped by the
        /// swap, so nothing refreshed them at a language switch: a text whose
        /// material still pointed at what the switch left behind rendered noise
        /// until its object was updated. The bundled font of the mod UI never
        /// changes, so a fresh preset of it is always valid.
        /// </summary>
        private static void ReapplyModUiFonts()
        {
            if (modUiTexts.Count == 0)
                return;

            // The refresh assigns materials, which must not run while the table is
            // being iterated (MarkAsModUI is called by UI code that can run
            // in between).
            List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>(modUiTexts.Values);
            int refreshed = 0;
            for (int i = 0; i < texts.Count; i++)
            {
                TextMeshProUGUI text = texts[i];
                if (text == null)
                    continue;

                TMP_FontAsset font = text.font != null ? text.font : Core.GlobalFontTMP;
                if (font == null || font.material == null)
                    continue;

                try
                {
                    TMPFontMaterials.RefreshMaterial(text, font);

                    // The mesh was built for the old material state; force a
                    // rebuild so the padding and the shader ratios are recomputed.
                    text.SetVerticesDirty();
                    refreshed++;
                }
                catch (Exception e)
                {
                    Logging.Warn("[TextMeshProFontSwap] Failed to refresh a mod UI font: " + e.Message);
                }
            }

            if (refreshed > 0)
                Logging.Message("[TextMeshProFontSwap] Refreshed " + refreshed + " mod UI text(s) for the active language.");
        }
    }
}
