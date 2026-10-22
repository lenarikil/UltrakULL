using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    /// <summary>
    /// TMP material handling for swapped fonts.
    ///
    /// Every material assigned to a swapped text must carry the new font's atlas.
    /// Presets are built from base materials (the game's HUD text materials or
    /// the font asset's own material) with the active font's atlas and atlas
    /// metrics transferred onto them - the same construction the game itself
    /// uses, where VCR_OSD_MONO_1's material IS the HUD normal text material.
    ///
    /// Presets are assigned through TMP's fontMaterial instance mechanism (never
    /// by writing to the canvas renderer alone): assigning fontMaterial makes
    /// TMP clone the preset per text, recompute the mesh padding and shader
    /// scale ratios, recalculate the stencil and flag a full rebuild. Bypassing
    /// that mechanism produced stale meshes (translucent text) that only fixed
    /// themselves when an inspector touched the component.
    ///
    /// Shadows (the shader underlay) follow the game's own style
    /// (VCR_OSD_MONO Outline: offset (1,-1), black, alpha 0.502):
    /// - a text whose original material had the underlay enabled keeps its exact
    ///   parameters (e.g. the stamina labels, the shop);
    /// - the mod's managed-shadow HUD texts get the game shadow style;
    /// - in "Always On Top" mode the shadow is drawn with a uGUI Shadow
    ///   component instead, because the game build does not include the
    ///   Distance Field Overlay shader's UNDERLAY variant.
    /// </summary>
    public static class TMPFontMaterials
    {
        private const string UnderlayKeyword = "UNDERLAY_ON";

        private static readonly int IdMainTex = Shader.PropertyToID("_MainTex");
        private static readonly int IdGradientScale = Shader.PropertyToID("_GradientScale");
        private static readonly int IdTextureWidth = Shader.PropertyToID("_TextureWidth");
        private static readonly int IdTextureHeight = Shader.PropertyToID("_TextureHeight");
        private static readonly int IdWeightNormal = Shader.PropertyToID("_WeightNormal");
        private static readonly int IdWeightBold = Shader.PropertyToID("_WeightBold");
        private static readonly int IdUnderlayColor = Shader.PropertyToID("_UnderlayColor");
        private static readonly int IdUnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");
        private static readonly int IdUnderlayOffsetY = Shader.PropertyToID("_UnderlayOffsetY");
        private static readonly int IdUnderlaySoftness = Shader.PropertyToID("_UnderlaySoftness");
        private static readonly int IdUnderlayDilate = Shader.PropertyToID("_UnderlayDilate");
        private static readonly int IdFaceDilate = Shader.PropertyToID("_FaceDilate");

        /// <summary>
        /// Exact underlay (shadow) parameters of a material.
        /// </summary>
        public struct UnderlayParams
        {
            public Color Color;
            public float OffsetX;
            public float OffsetY;
            public float Softness;
            public float Dilate;

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Mathf.RoundToInt(Color.r * 255f);
                    hash = hash * 31 + Mathf.RoundToInt(Color.g * 255f);
                    hash = hash * 31 + Mathf.RoundToInt(Color.b * 255f);
                    hash = hash * 31 + Mathf.RoundToInt(Color.a * 255f);
                    hash = hash * 31 + Mathf.RoundToInt(OffsetX * 100f);
                    hash = hash * 31 + Mathf.RoundToInt(OffsetY * 100f);
                    hash = hash * 31 + Mathf.RoundToInt(Softness * 100f);
                    hash = hash * 31 + Mathf.RoundToInt(Dilate * 100f);
                    return hash;
                }
            }
        }

        /// <summary>
        /// The game's shadow style, taken from the game's own underlay material
        /// (VCR_OSD_MONO Outline): offset (1,-1), black, alpha 0.502, no softness,
        /// no dilation. Used for the HUD texts the mod provides shadows for.
        /// </summary>
        public static UnderlayParams GameShadowStyle()
        {
            return new UnderlayParams
            {
                Color = new Color(0f, 0f, 0f, 0.502f),
                OffsetX = 1f,
                OffsetY = -1f,
                Softness = 0f,
                Dilate = 0f
            };
        }

        /// <summary>
        /// A shadow style for the shader underlay, given the desired offset in
        /// canvas units. The underlay offset is in atlas texel units scaled by
        /// _ScaleRatioC * _GradientScale, where one texel is fontSize / pointSize
        /// canvas units; offsets up to 1 map linearly (beyond that TMP's ratio
        /// normalization saturates the distance), so the requested offset is
        /// converted within that linear range.
        /// </summary>
        public static UnderlayParams CanvasShadowStyle(TMP_Text text, TMP_FontAsset font, Vector2 canvasOffset, float alpha)
        {
            Vector2 underlayOffset = CanvasOffsetToUnderlay(text, font, canvasOffset);
            return new UnderlayParams
            {
                Color = new Color(0f, 0f, 0f, alpha),
                OffsetX = underlayOffset.x,
                OffsetY = underlayOffset.y,
                Softness = 0f,
                Dilate = 0f
            };
        }

        private static Vector2 CanvasOffsetToUnderlay(TMP_Text text, TMP_FontAsset font, Vector2 canvasOffset)
        {
            Material material = font != null ? font.material : null;
            if (text == null || material == null || !material.HasProperty(IdGradientScale))
                return canvasOffset;

            float gradientScale = material.GetFloat(IdGradientScale);
            float pointSize = font.faceInfo.pointSize;
            if (gradientScale <= 1f || pointSize <= 0f)
                return canvasOffset;

            // _ScaleRatioC as computed by ShaderUtilities.UpdateShaderRatios:
            // (GS - 1 - (weight + faceDilate) * (GS - 1)) / (GS * extent). For
            // offsets up to 1 the extent term is 1, so one underlay unit moves
            // the shadow by (GS - 1 - faceTerm) * fontSize / pointSize canvas
            // units.
            float weight = Mathf.Max(
                material.HasProperty(IdWeightNormal) ? material.GetFloat(IdWeightNormal) : 0f,
                material.HasProperty(IdWeightBold) ? material.GetFloat(IdWeightBold) : 0f) / 4f;
            float faceDilate = material.HasProperty(IdFaceDilate) ? material.GetFloat(IdFaceDilate) : 0f;
            float faceTerm = (weight + faceDilate) * (gradientScale - 1f);

            float fontScale = font.faceInfo.scale > 0f ? font.faceInfo.scale : 1f;
            float canvasPerUnit = (gradientScale - 1f - faceTerm) * (text.fontSize / pointSize) * fontScale;
            if (canvasPerUnit <= 0.001f)
                return canvasOffset;

            Vector2 underlay = new Vector2(canvasOffset.x / canvasPerUnit, canvasOffset.y / canvasPerUnit);
            // Beyond 1 the shader normalizes the offset (TMP recomputes
            // _ScaleRatioC), saturating the shadow distance; clamp into the
            // linear range and keep the direction.
            float maxAbs = Mathf.Max(Mathf.Abs(underlay.x), Mathf.Abs(underlay.y));
            if (maxAbs > 1f)
                underlay /= maxAbs;
            return underlay;
        }

        private struct PresetKey
        {
            public int FontId;
            public int BaseId;
            public int UnderlayHash;

            public bool Equals(PresetKey other)
            {
                return FontId == other.FontId && BaseId == other.BaseId && UnderlayHash == other.UnderlayHash;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is PresetKey))
                    return false;
                return Equals((PresetKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + FontId;
                    hash = hash * 31 + BaseId;
                    hash = hash * 31 + UnderlayHash;
                    return hash;
                }
            }
        }

        private static readonly Dictionary<PresetKey, Material> presets = new Dictionary<PresetKey, Material>();

        // The material each text had before the mod first touched it. The game's
        // own materials carry per-text looks (e.g. underlay shadows) that must be
        // re-read on every re-apply instead of being read from an already swapped
        // material, where they would be lost.
        private static readonly Dictionary<TextMeshProUGUI, Material> originalMaterials = new Dictionary<TextMeshProUGUI, Material>();

        /// <summary>
        /// Drops cached presets. They are cheap to rebuild and the fonts they were
        /// built for may belong to a language that is no longer active. Entries of
        /// destroyed texts are removed from the original material registry; live
        /// texts keep theirs, since their game materials never change.
        /// </summary>
        public static void ClearCache()
        {
            presets.Clear();
            PruneDestroyed(originalMaterials);
        }

        private static void PruneDestroyed(Dictionary<TextMeshProUGUI, Material> map)
        {
            if (map.Count == 0)
                return;

            List<TextMeshProUGUI> dead = null;
            foreach (KeyValuePair<TextMeshProUGUI, Material> pair in map)
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
                map.Remove(dead[i]);
        }

        /// <summary>
        /// Swaps the font of a text and makes sure the assigned material matches it.
        /// </summary>
        /// <param name="text">Text to apply the font to.</param>
        /// <param name="font">Replacement font asset.</param>
        /// <param name="baseMaterial">
        /// Optional material the text should render with (an overlay material in
        /// "Always On Top" mode). Its shader and look are kept; only the atlas of
        /// the active font is transferred. When the text already renders with a
        /// matching material, nothing is changed - this keeps English (unmodded)
        /// rendering fully native.
        /// </param>
        /// <param name="forcedUnderlay">
        /// Shadow parameters to force on the resulting material (the game's shadow
        /// style for the mod's managed-shadow HUD texts), or null.
        /// </param>
        public static void ApplyFont(TextMeshProUGUI text, TMP_FontAsset font, Material baseMaterial, UnderlayParams? forcedUnderlay)
        {
            if (text == null || font == null || font.material == null)
                return;

            RecordOriginalMaterial(text);

            // Shadow precedence: forced (managed HUD shadows) over the text's
            // original material underlay (the game's own shadow style). The base
            // material's own underlay is never inherited - bases are used for
            // their shader, and their leftover underlay parameters (the mod
            // bundle materials ship offset (0.5,-0.5)) are an implementation
            // detail, not a look to preserve.
            UnderlayParams? underlay = forcedUnderlay;
            if (!underlay.HasValue)
            {
                Material original;
                UnderlayParams originalUnderlay;
                if (originalMaterials.TryGetValue(text, out original) &&
                    TryReadUnderlay(original, out originalUnderlay))
                {
                    underlay = originalUnderlay;
                }
            }

            bool fontChanged = text.font != font;
            if (fontChanged)
                text.font = font;

            // Native path. Changing the font does NOT point the renderer at the
            // new font's material: TMP keeps the material of the previous font,
            // so the rebuilt mesh samples the old atlas and the text turns into
            // noise. After a font change the material is therefore always
            // (re)assigned. When the font did not change, the material is the
            // text's own and is kept when it matches the font - a game material
            // whose look is the game's own, or a preset that is already there.
            // The assigned material is a clean preset - the font's material with
            // the underlay disabled - because the bundled font ships with the
            // underlay enabled and a text without a requested shadow must not
            // inherit it (the subtitles and the menu buttons showed one).
            if (baseMaterial == null && !underlay.HasValue)
            {
                if (!fontChanged)
                {
                    Material current = text.fontSharedMaterial;
                    if (current != null && AtlasMatches(current, font))
                        return;
                }

                Material cleanPreset = GetOrCreatePreset(font, font.material, null);
                RefreshAtlasProperties(font, cleanPreset);
                AssignMaterial(text, font, cleanPreset);
                return;
            }

            if (baseMaterial != null && !underlay.HasValue)
            {
                // The text already renders with an equivalent material (same
                // shader by name, matching atlas) - keep the game's own material
                // instance, e.g. English HUD texts in Always On Top mode.
                Material current = text.fontSharedMaterial;
                if (current != null && AtlasMatches(current, font) &&
                    current.shader != null && baseMaterial.shader != null &&
                    current.shader.name == baseMaterial.shader.name)
                {
                    return;
                }
            }

            Material template = baseMaterial;
            if (template == null)
                template = underlay.HasValue ? GetUnderlayTemplate(font) : font.material;

            Material preset = GetOrCreatePreset(font, template, underlay);
            RefreshAtlasProperties(font, preset);
            AssignMaterial(text, font, preset);
        }

        /// <summary>
        /// The material underlay presets are cloned from. Runtime-created font
        /// assets ship a material on the "TextMeshPro/Mobile/Distance Field"
        /// shader, and this game build contains no material (and therefore no
        /// compiled variant) combining that shader with the UNDERLAY keyword -
        /// an underlay configured on it is silently not rendered. The bundled mod
        /// font's own material is a full Distance Field material (the shader of
        /// the game's outlined materials), so underlay presets for custom fonts
        /// are built from it instead.
        /// </summary>
        private static Material GetUnderlayTemplate(TMP_FontAsset font)
        {
            Material fontMaterial = font.material;
            if (IsDistanceField(fontMaterial))
                return fontMaterial;

            Material bundled = Core.GlobalFontTMP != null ? Core.GlobalFontTMP.material : null;
            if (IsDistanceField(bundled))
                return bundled;

            return fontMaterial;
        }

        private static bool IsDistanceField(Material material)
        {
            return material != null && material.shader != null &&
                material.shader.name == "TextMeshPro/Distance Field";
        }

        /// <summary>
        /// The underlay parameters of a text's original (pre-swap) material, if
        /// it had the underlay enabled. Used to route such texts to shadow twins
        /// in overlay mode, where the shader underlay is not rendered.
        /// </summary>
        public static bool TryGetPreservedUnderlay(TextMeshProUGUI text, out UnderlayParams underlay)
        {
            underlay = default(UnderlayParams);
            Material original;
            if (text == null || !originalMaterials.TryGetValue(text, out original))
                return false;

            return TryReadUnderlay(original, out underlay);
        }

        private static void RecordOriginalMaterial(TextMeshProUGUI text)
        {
            if (originalMaterials.ContainsKey(text))
                return;

            Material current = text.fontSharedMaterial;
            if (current != null)
                originalMaterials[text] = current;
        }

        /// <summary>
        /// Assigns a material to a text through TMP's fontMaterial instance
        /// mechanism. TMP clones the preset per text, recomputes the mesh padding
        /// and shader ratios and flags a full rebuild; the canvas renderer is
        /// synced immediately so no frame is rendered with a stale material.
        /// Re-applying the same preset is a no-op.
        /// </summary>
        private static void AssignMaterial(TextMeshProUGUI text, TMP_FontAsset font, Material material)
        {
            if (text == null || material == null)
                return;

            Material current = text.fontSharedMaterial;
            if (current != null &&
                current.name == material.name + " (Instance)" &&
                AtlasMatches(current, font))
            {
                // Already an instance of exactly this preset.
                return;
            }

            text.fontMaterial = material;

            CanvasRenderer canvasRenderer = text.canvasRenderer;
            if (canvasRenderer != null)
            {
                canvasRenderer.materialCount = 1;
                canvasRenderer.SetMaterial(material, material.GetTexture(IdMainTex));
            }
        }

        private static bool AtlasMatches(Material material, TMP_FontAsset font)
        {
            if (material == null)
                return false;

            Texture atlas = material.GetTexture(IdMainTex);
            Texture fontAtlas = font.atlasTexture;
            return atlas != null && fontAtlas != null && atlas.GetInstanceID() == fontAtlas.GetInstanceID();
        }

        private static bool TryReadUnderlay(Material material, out UnderlayParams underlay)
        {
            underlay = default(UnderlayParams);
            if (material == null ||
                !material.IsKeywordEnabled(UnderlayKeyword) ||
                !material.HasProperty(IdUnderlayColor))
            {
                return false;
            }

            underlay.Color = material.GetColor(IdUnderlayColor);
            underlay.OffsetX = material.HasProperty(IdUnderlayOffsetX) ? material.GetFloat(IdUnderlayOffsetX) : 0f;
            underlay.OffsetY = material.HasProperty(IdUnderlayOffsetY) ? material.GetFloat(IdUnderlayOffsetY) : 0f;
            underlay.Softness = material.HasProperty(IdUnderlaySoftness) ? material.GetFloat(IdUnderlaySoftness) : 0f;
            underlay.Dilate = material.HasProperty(IdUnderlayDilate) ? material.GetFloat(IdUnderlayDilate) : 0f;
            return true;
        }

        private static Material GetOrCreatePreset(TMP_FontAsset font, Material baseMaterial, UnderlayParams? underlay)
        {
            PresetKey key = new PresetKey
            {
                FontId = font.GetInstanceID(),
                BaseId = baseMaterial.GetInstanceID(),
                UnderlayHash = underlay.HasValue ? underlay.Value.GetHashCode() : 0
            };

            Material preset;
            if (presets.TryGetValue(key, out preset) && preset != null)
                return preset;

            preset = new Material(baseMaterial)
            {
                name = font.name + "__" + baseMaterial.name + (underlay.HasValue ? "_shadow" : "")
            };
            CopyAtlasProperties(font, preset);

            if (underlay.HasValue)
            {
                ApplyUnderlay(preset, underlay.Value);
            }
            else
            {
                // Strip the base material's own underlay: bases are used for
                // their shader (e.g. the mod bundle's overlay material with the
                // UNDERLAY variant), not for their look. Without an explicit
                // style the text renders without a shadow, matching the normal
                // mode.
                preset.DisableKeyword(UnderlayKeyword);
            }

            presets[key] = preset;
            return preset;
        }

        /// <summary>
        /// Configures the shader underlay of a material with the given shadow
        /// parameters. Scale ratios are not touched: TMP recomputes them from
        /// the material on every mesh rebuild (ShaderUtilities.UpdateShaderRatios).
        /// </summary>
        public static void ApplyUnderlay(Material material, UnderlayParams underlay)
        {
            if (material == null || !material.HasProperty(IdUnderlayColor))
                return;

            material.SetColor(IdUnderlayColor, underlay.Color);
            if (material.HasProperty(IdUnderlayOffsetX))
                material.SetFloat(IdUnderlayOffsetX, underlay.OffsetX);
            if (material.HasProperty(IdUnderlayOffsetY))
                material.SetFloat(IdUnderlayOffsetY, underlay.OffsetY);
            if (material.HasProperty(IdUnderlaySoftness))
                material.SetFloat(IdUnderlaySoftness, underlay.Softness);
            if (material.HasProperty(IdUnderlayDilate))
                material.SetFloat(IdUnderlayDilate, underlay.Dilate);
            material.EnableKeyword(UnderlayKeyword);
        }

        /// <summary>
        /// Points a preset at the given font's atlas and copies the atlas metrics
        /// that determine SDF sampling (gradient scale, texture size and weights).
        /// Scale ratios and sharpness stay at the base material's values: TMP
        /// recomputes the ratios from these very properties on rebuild.
        /// </summary>
        private static void CopyAtlasProperties(TMP_FontAsset font, Material destination)
        {
            Material source = font.material;
            destination.SetTexture(IdMainTex, font.atlasTexture);
            CopyFloat(source, destination, IdGradientScale);
            CopyFloat(source, destination, IdTextureWidth);
            CopyFloat(source, destination, IdTextureHeight);
            CopyFloat(source, destination, IdWeightNormal);
            CopyFloat(source, destination, IdWeightBold);
        }

        /// <summary>
        /// Re-syncs a preset with the font's current atlas. Custom fonts are
        /// populated dynamically; their atlas changes as glyphs are added, so a
        /// cached preset must be pointed at the current atlas every time it is
        /// used, not only when it is created.
        /// </summary>
        private static void RefreshAtlasProperties(TMP_FontAsset font, Material material)
        {
            if (material == null)
                return;

            CopyAtlasProperties(font, material);
        }

        private static void CopyFloat(Material source, Material destination, int propertyId)
        {
            if (source != null && source.HasProperty(propertyId) && destination.HasProperty(propertyId))
                destination.SetFloat(propertyId, source.GetFloat(propertyId));
        }

        /// <summary>
        /// Re-applies the clean material of a font to a text, bypassing the
        /// shortcuts of <see cref="ApplyFont"/>.
        ///
        /// The mod's own UI texts are skipped by the font swap, so nothing refreshes
        /// their material state when the language changes: the font setter
        /// snapshots a material when the text is created, and whatever that
        /// snapshot points at can be left behind by the switch (a preset that was
        /// cleared, an instance of a material of the previous language). A fresh
        /// clean preset - the font's material with the underlay disabled - replaces
        /// it through the same instance mechanism the swapped texts use.
        /// </summary>
        public static void RefreshMaterial(TextMeshProUGUI text, TMP_FontAsset font)
        {
            if (text == null || font == null || font.material == null)
                return;

            Material cleanPreset = GetOrCreatePreset(font, font.material, null);
            RefreshAtlasProperties(font, cleanPreset);
            AssignMaterial(text, font, cleanPreset);
        }
    }
}
