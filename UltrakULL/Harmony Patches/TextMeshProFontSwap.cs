using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UltrakULL.json;
using UltrakULL;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;

namespace UltrakULL.Harmony_Patches
{
	internal sealed class TMPOverlayMaterialTracker : MonoBehaviour
	{
		private Material generatedMaterial;

		public void SetGeneratedMaterial(Material material)
		{
			if (generatedMaterial != null && generatedMaterial != material)
				Destroy(generatedMaterial);
			generatedMaterial = material;
		}

		private void OnDestroy()
		{
			if (generatedMaterial != null)
				Destroy(generatedMaterial);
		}
	}

	public class TextMeshProFontSwap
	{
		private static bool IsBossBarText(TMP_Text tmpText)
		{
			if (tmpText == null)
				return false;

			Transform current = tmpText.transform;
			while (current != null)
			{
				if (current.GetComponent<HealthBar>() != null)
					return true;

				string n = current.name;
				if (n.IndexOf("HP Text", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					if (current.GetComponentInParent<HealthBar>() != null)
						return true;
				}

				if (n.IndexOf("Boss Health", StringComparison.OrdinalIgnoreCase) >= 0)
					return true;

				if (n.IndexOf("Health After Image", StringComparison.OrdinalIgnoreCase) >= 0)
					return true;

				current = current.parent;
			}

			return false;
		}

		private static bool IsHealthText(TMP_Text tmpText)
		{
			if (tmpText == null)
				return false;

			// A converted legacy hpText is a sibling of the original Text component.
			// It is therefore not equal to HealthBar.hpText and must be recognised
			// before the hierarchy check below.
			var convertedTmp = tmpText as TextMeshProUGUI;
			if (convertedTmp != null && TextToTMPConverter.IsConvertedFromHealthBar(convertedTmp))
				return true;

			Transform current = tmpText.transform;
			while (current != null)
			{
				var healthBar = current.GetComponent<HealthBar>();
				if (healthBar != null)
				{
					// Check if this text is the hpText or HP Symbol of this HealthBar
					var hpTextUGUI = healthBar.hpText as TextMeshProUGUI;
					if (hpTextUGUI != null && hpTextUGUI == tmpText)
						return true;

					// Check for HP Symbol child
					var hpSymbol = current.GetComponentsInChildren<TextMeshProUGUI>(true)
						.FirstOrDefault(t => t != null && (t.name.IndexOf("HP Symbol", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Plus", StringComparison.OrdinalIgnoreCase) >= 0));
					if (hpSymbol != null && hpSymbol == tmpText)
						return true;

					return false;
				}

				current = current.parent;
			}

			// Check for Classic HUD health texts: AltHud/Filler/Health/Title or AltHud (2)/Filler/Health (1)/Title
			Transform parent = tmpText.transform.parent;
			while (parent != null)
			{
				string parentName = parent.name;
				if ((parentName.IndexOf("AltHud", StringComparison.OrdinalIgnoreCase) >= 0) &&
					parent.parent != null)
				{
					// Look for Health/Title or Health (1)/Title in the hierarchy
					var healthTitle = parent.GetComponentsInChildren<TextMeshProUGUI>(true)
						.FirstOrDefault(t => t != null && t == tmpText &&
							(t.transform.parent != null &&
							 (t.transform.parent.name.IndexOf("Health", StringComparison.OrdinalIgnoreCase) >= 0)));
					if (healthTitle != null)
						return true;
				}
				parent = parent.parent;
			}

			return false;
		}

		private static bool IsSpeedometerText(TMP_Text tmpText)
		{
			if (tmpText == null)
				return false;

			Transform current = tmpText.transform;
			while (current != null)
			{
				if (current.GetComponent<Speedometer>() != null)
					return true;
				current = current.parent;
			}

			return false;
		}

		private static bool IsHpSymbolText(TMP_Text tmpText)
		{
			if (tmpText == null)
				return false;

			string name = tmpText.gameObject.name;
			return name.IndexOf("HP Symbol", StringComparison.OrdinalIgnoreCase) >= 0 ||
				name.IndexOf("Plus", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static class TMPFontLogger
		{
			private static HashSet<string> loggedFonts = new HashSet<string>();
			private static readonly object lockObject = new object();
			private static string logFilePath = null;

			private static string GetLogFilePath()
			{
				if (logFilePath == null)
				{
					logFilePath = Path.Combine(Paths.ConfigPath, "ultrakull", "fonts_tmp.txt");
				}
				return logFilePath;
			}

			public static void LogFont(TMP_FontAsset font)
			{
				if (font == null)
					return;

				string fontName = font.name;
				if (string.IsNullOrEmpty(fontName))
					return;

				lock (lockObject)
				{
					if (loggedFonts.Contains(fontName))
						return;

					loggedFonts.Add(fontName);
					string path = GetLogFilePath();
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(path));
						File.AppendAllText(path, $"{fontName}\n");
						UltrakULL.Logging.Debug($"Logged TMP font: {fontName}");
					}
					catch (Exception e)
					{
						UltrakULL.Logging.Error($"Failed to log TMP font {fontName}: {e.Message}");
					}
				}
			}
		}

		[HarmonyPatch(typeof(TextMeshProUGUI), "OnEnable")]
		public static class TextMeshProFontSwapper
		{
			private static readonly HashSet<IntPtr> objectsFixed = new HashSet<IntPtr>();

			public static void ClearCache()
			{
				objectsFixed.Clear();
			}

            [HarmonyPostfix]
			public static void SwapFont(ref TextMeshProUGUI __instance, IntPtr ___m_CachedPtr)
			{
                if (__instance == null || TMPShadowTwin.IsShadowGraphic(__instance))
                    return;

                if (!objectsFixed.Contains(___m_CachedPtr) && Core.TMPFontReady && (!CommonFunctions.isUsingEnglish() || !(CommonFunctions.GetCurrentSceneName() != "Main Menu")))
                {
                    SwapTMPFont(ref __instance);
					objectsFixed.Add(___m_CachedPtr);
				}

				if (Core.TMPFontReady)
				{
					bool requiresForcedShadow = TMPShadowPolicy.RequiresForcedShadow(__instance.transform);
					if (requiresForcedShadow)
					{
						HudControllerPatch.TrackForcedShadowText(__instance);
						var hud = HudController.Instance;
						if (hud != null)
							HudControllerPatch.ApplyOverlayZTest(
								__instance,
								HudControllerPatch.isOverlaid,
								hud.overlayTextMaterial,
								hud.normalTextMaterial,
								forceManagedShadow: true);
					}
					else if (HudControllerPatch.isOverlaid && HudControllerPatch.IsStyleHudText(__instance.transform))
					{
						var hud = HudController.Instance;
						if (hud != null)
							HudControllerPatch.ApplyOverlayZTest(
								__instance,
								true,
								hud.overlayTextMaterial,
								hud.normalTextMaterial);
					}
				}
			}
		}

		[HarmonyPatch(typeof(TextMeshProUGUI), "OnDisable")]
		public static class TextMeshProFontSwapDisablePatch
		{
			[HarmonyPostfix]
			public static void UntrackForcedShadowText(TextMeshProUGUI __instance)
			{
				HudControllerPatch.UntrackForcedShadowText(__instance);
			}
		}

[HarmonyPatch(typeof(HudController))]
		public static class HudControllerPatch
		{
			private static readonly FieldInfo m_sharedMaterialField;

			static HudControllerPatch()
			{
				Type type = typeof(TMP_Text);
				while (type != null && type != typeof(object))
				{
					m_sharedMaterialField = type.GetField("m_sharedMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
					if (m_sharedMaterialField != null) break;
					type = type.BaseType;
				}
			}

			private static bool? _isOverlaid = null;
			private static readonly HashSet<TextMeshProUGUI> forcedShadowTexts = new HashSet<TextMeshProUGUI>();

			public static void TrackForcedShadowText(TextMeshProUGUI text)
			{
				if (text != null && !TMPShadowTwin.IsShadowGraphic(text))
					forcedShadowTexts.Add(text);
			}

			public static void UntrackForcedShadowText(TextMeshProUGUI text)
			{
				if (text != null)
					forcedShadowTexts.Remove(text);
			}

			public static bool isOverlaid
			{
				get
				{
					if (!_isOverlaid.HasValue)
					{
						try
						{
							_isOverlaid = MonoSingleton<PrefsManager>.Instance.GetBool("hudAlwaysOnTop", false);
						}
						catch
						{
							_isOverlaid = false;
						}
					}
					return _isOverlaid.Value;
				}
				set { _isOverlaid = value; }
			}

private static Material GetCurrentMaterialSafe(TMP_Text text)
		{
			if (text == null)
				return null;
			try
			{
				// Use fontMaterial property getter which returns the properly configured instance material
				// with all keywords (including UNDERLAY_ON) properly set
				Material mat = text.fontMaterial;
				if (mat != null)
					return mat;
			}
			catch { }
			return text.fontSharedMaterial;
		}

		private static bool IsAdditionalForcedShadowText(TextMeshProUGUI text, TMP_Text[] hudTextElements)
		{
			if (text == null ||
			    TMPShadowTwin.IsShadowGraphic(text) ||
			    !TMPShadowPolicy.RequiresForcedShadow(text.transform) ||
			    text.GetComponentInParent<HealthBar>() != null ||
			    text.GetComponentInParent<Speedometer>() != null)
				return false;

			return hudTextElements == null || Array.IndexOf(hudTextElements, text) < 0;
		}

		private static void ForceRendererUpdate(TMP_Text text, Material mat)
		{
			if (text == null || mat == null)
				return;
			var canvasRenderer = text.canvasRenderer;
			if (canvasRenderer != null)
			{
				canvasRenderer.materialCount = 1;
				canvasRenderer.SetMaterial(mat, 0);
				return;
			}
			
			var meshRenderer = text.GetComponent<MeshRenderer>();
			if (meshRenderer != null)
			{
				Material[] mats = meshRenderer.sharedMaterials;
				for (int i = 0; i < mats.Length; i++)
					mats[i] = mat;
				meshRenderer.sharedMaterials = mats;
				return;
			}
			
			text.SetMaterialDirty();
		}

		public static void ApplyOverlayZTest(
			TMP_Text text,
			bool onTop,
			Material overlayMat,
			Material normalMat,
			bool forceManagedShadow = false)
		{
			if (text == null)
				return;
			if (TMPShadowTwin.IsShadowGraphic(text))
				return;

			bool isHealthText = IsHealthText(text);
			bool isSpeedometerText = IsSpeedometerText(text);
			bool isHpSymbol = IsHpSymbolText(text);
			bool forceTextShadow = TMPShadowPolicy.RequiresForcedShadow(text.transform);
			if (text is TextMeshProUGUI forcedText && forceTextShadow)
				TrackForcedShadowText(forcedText);

			if (!isHealthText)
			{
				// Also check classic HUD health hierarchy directly
				Transform parent = text.transform.parent;
				while (parent != null)
				{
					string parentName = parent.name;
					if ((parentName.IndexOf("AltHud", StringComparison.OrdinalIgnoreCase) >= 0) &&
						parent.parent != null)
					{
						var healthTitle = parent.GetComponentsInChildren<TextMeshProUGUI>(true)
							.FirstOrDefault(t => t != null && t == text &&
								(t.transform.parent != null &&
								 (t.transform.parent.name.IndexOf("Health", StringComparison.OrdinalIgnoreCase) >= 0)));
						if (healthTitle != null)
						{
							isHealthText = true;
							break;
						}
					}
					parent = parent.parent;
				}
			}

			bool needsManagedShadow = forceManagedShadow ||
				forceTextShadow ||
				((isHealthText || isSpeedometerText) && !isHpSymbol);
			Vector2 shadowDistance = TMPShadowPolicy.IsAltHud(text)
				? new Vector2(1.25f, -1.25f)
				: new Vector2(1.5f, -1.5f);
			shadowDistance = TMPFontUtils.GetGraphicShadowDistance(
				text as TextMeshProUGUI,
				shadowDistance);
			Vector2 overlayShadowDistance = shadowDistance;
			if (onTop && (isHealthText || isSpeedometerText) && !isHpSymbol)
				overlayShadowDistance = new Vector2(3f, -3f);
			Color shadowColor = TMPFontUtils.GetGraphicShadowColor(
				text as TextMeshProUGUI,
				new Color(0f, 0f, 0f, 0.75f));
			if (onTop && (isHealthText || isSpeedometerText) && !isHpSymbol)
				shadowColor.a = shadowColor.a > 0.001f ? Mathf.Min(shadowColor.a, 0.55f) : 0.55f;
Material refMat = onTop ? overlayMat : normalMat;
			if (refMat == null)
				return;
			Material currentMat = GetCurrentMaterialSafe(text);
			if (currentMat == null)
				return;

			// The game's HUD overlay material has the shader variant and render
			// state expected by Always On Top. Use the matching mod font preset as
			// the base where possible, then transfer the active font atlas.
			Material baseMat = onTop
				? (GetOverlayMaterialForFont(text) ?? refMat)
				: ((text.font != null ? text.font.material : null) ?? normalMat ?? refMat);
			if (onTop && needsManagedShadow && text.font != null)
			{
				Material fontMaterial = text.font.material;
				if (fontMaterial != null &&
				    TMPFontUtils.HasUnderlayOffset(fontMaterial) &&
				    fontMaterial.HasProperty("_UnderlayColor") &&
				    fontMaterial.HasProperty("_ZTest"))
				{
					baseMat = fontMaterial;
				}
			}
			Material newMat = new Material(baseMat);
			newMat.renderQueue = refMat.renderQueue;
			if (newMat.HasProperty("_ZTest"))
				newMat.SetFloat("_ZTest", onTop ? 8f : 4f);
			else
			{
				UnityEngine.Object.Destroy(newMat);
				newMat = new Material(refMat);
				newMat.renderQueue = refMat.renderQueue;
				if (newMat.HasProperty("_ZTest"))
					newMat.SetFloat("_ZTest", onTop ? 8f : 4f);
			}

			TMPFontUtils.CopyFontRenderingProperties(currentMat, newMat);

			Material underlaySource = currentMat.IsKeywordEnabled("UNDERLAY_ON")
				? currentMat
				: text.fontSharedMaterial;
			bool sourceHasUnderlay = underlaySource != null && underlaySource.IsKeywordEnabled("UNDERLAY_ON");
			bool keepUnderlay = sourceHasUnderlay || needsManagedShadow;
			bool underlayConfigured = false;
			bool needsOverlayShadowTwin = onTop && (needsManagedShadow || sourceHasUnderlay);

			if (keepUnderlay && !onTop)
			{
				bool hasUnderlayProperties = TMPFontUtils.HasUnderlayOffset(newMat) && newMat.HasProperty("_UnderlayColor");
				if (hasUnderlayProperties)
				{
					if (needsManagedShadow)
						underlayConfigured = TMPFontUtils.ConfigureUnderlayFromText(
							newMat,
							text as TextMeshProUGUI,
							new Vector4(0f, 0f, 0f, 0.75f),
							new Vector4(1.5f, -1.5f, 0f, 0f),
							0f,
							0f);
					else if (sourceHasUnderlay)
						underlayConfigured = TMPFontUtils.CopyUnderlayProperties(underlaySource, newMat);
					else
						underlayConfigured = TMPFontUtils.ConfigureUnderlay(
							newMat,
							new Vector4(0f, 0f, 0f, 0.75f),
							new Vector4(1.5f, -1.5f, 0f, 0f),
							0f,
							0f);
				}
			}
			else
			{
				newMat.DisableKeyword("UNDERLAY_ON");
			}

			if (needsOverlayShadowTwin)
			{
				newMat.DisableKeyword("UNDERLAY_ON");
				TMPFontUtils.SetGraphicShadowEnabled(text as TextMeshProUGUI, false);
				TMPOverlayShadowFallback.Set(text as TextMeshProUGUI, false, Vector2.zero);
			}
			else if (needsManagedShadow && !underlayConfigured)
			{
				newMat.DisableKeyword("UNDERLAY_ON");
				TMPOverlayShadowFallback.Set(text as TextMeshProUGUI, true, shadowDistance);
			}
			else
			{
				TMPFontUtils.SetGraphicShadowEnabled(text as TextMeshProUGUI, false);
				TMPOverlayShadowFallback.Set(text as TextMeshProUGUI, false, Vector2.zero);
			}
			text.fontMaterial = newMat;
			TMPOverlayMaterialTracker materialTracker = text.GetComponent<TMPOverlayMaterialTracker>();
			if (materialTracker == null)
				materialTracker = text.gameObject.AddComponent<TMPOverlayMaterialTracker>();
			materialTracker.SetGeneratedMaterial(newMat);
			ForceRendererUpdate(text, newMat);
			TMPShadowTwin.SetOverlayShadow(
				text as TextMeshProUGUI,
				needsOverlayShadowTwin,
				overlayShadowDistance,
				shadowColor);
		}

		private static Material GetOverlayMaterialForFont(TMP_Text text)
		{
			if (text == null || text.font == null)
				return null;

			if (text.font == Core.CustomMainFontTMP)
				return Core.CustomMainFontTMPOverlayMat;
			if (text.font == Core.CustomMuseumFontTMP)
				return Core.CustomMuseumFontTMPOverlayMat;
			if (text.font == Core.CustomTerminalFontTMP)
				return Core.CustomTerminalFontTMPOverlayMat;
			if (text.font == Core.CustomSecretTerminalFontTMP)
				return Core.CustomSecretTerminalFontTMPOverlayMat;
			if (text.font == Core.CJKFontTMP)
				return Core.CJKFontTMPOverlayMat;
			if (text.font == Core.JaFontTMP)
				return Core.jaFontTMPOverlayMat;

			return Core.GlobalFontTMPOverlayMat;
		}

		public static void ApplyOverlayToAllHealthBarTexts(
			HealthBar hb,
			bool onTop,
			Material overlayMat,
			Material normalMat,
			HashSet<TMP_Text> processedTexts = null)
		{
			if (hb == null)
				return;

			HashSet<TMP_Text> alreadyApplied = processedTexts ?? new HashSet<TMP_Text>();

			if (hb.hpText != null)
			{
				var hpTextUGUI = hb.hpText as TextMeshProUGUI;
				if (hpTextUGUI != null)
				{
					if (alreadyApplied.Add(hpTextUGUI))
						ApplyOverlayZTest(hpTextUGUI, onTop, overlayMat, normalMat, forceManagedShadow: true);
				}
				else
				{
					var hpTextGameObject = hb.hpText?.gameObject;
					if (hpTextGameObject != null)
					{
						var legacyText = hpTextGameObject.GetComponent<Text>();
						if (legacyText != null)
						{
							var convertedTMP = TextToTMPConverter.GetConvertedTMP(legacyText);
							if (convertedTMP != null)
							{
								if (alreadyApplied.Add(convertedTMP))
									ApplyOverlayZTest(convertedTMP, onTop, overlayMat, normalMat, forceManagedShadow: true);
							}
						}
					}
				}
			}

			var hpSymbol = hb.transform.GetComponentsInChildren<TextMeshProUGUI>(true)
				.FirstOrDefault(t => t != null && (t.name.IndexOf("HP Symbol", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Plus", StringComparison.OrdinalIgnoreCase) >= 0));
			if (hpSymbol != null)
			{
				if (alreadyApplied.Add(hpSymbol))
					ApplyOverlayZTest(hpSymbol, onTop, overlayMat, normalMat, forceManagedShadow: true);
			}

			// Any other TMP under the bar (classic HUD "HEALTH" title, etc.)
			foreach (var t in hb.GetComponentsInChildren<TextMeshProUGUI>(true))
			{
				if (t != null && alreadyApplied.Add(t))
					ApplyOverlayZTest(t, onTop, overlayMat, normalMat);
			}
		}

		public static bool IsStyleHudText(Transform target)
		{
			return target != null && target.GetComponentInParent<StyleHUD>() != null;
		}

		// Style meter texts (multiplier, style info, bonus list) live under the
		// StyleHUD hierarchy. They are not part of HudController.textElements, so
		// they need the same Always On Top overlay treatment as health/speedometer.
		public static void ApplyOverlayToStyleHudTexts(
			bool onTop,
			Material overlayMat,
			Material normalMat,
			HashSet<TMP_Text> processedTexts = null)
		{
			foreach (StyleHUD styleHud in UnityEngine.Object.FindObjectsOfType<StyleHUD>())
			{
				if (styleHud == null)
					continue;
				foreach (TMP_Text text in styleHud.GetComponentsInChildren<TMP_Text>(true))
				{
					if (text == null || TMPShadowTwin.IsShadowGraphic(text))
						continue;
					if (processedTexts != null && !processedTexts.Add(text))
						continue;
					ApplyOverlayZTest(text, onTop, overlayMat, normalMat, forceManagedShadow: false);
				}
			}
		}

		[HarmonyPatch("SetAlwaysOnTop")]
		[HarmonyPrefix]
		public static bool SetAlwaysOnTop_Prefix(ref TMP_Text[] ___textElements, bool onTop, Material ___overlayTextMaterial, Material ___normalTextMaterial)
		{
			try
			{
				isOverlaid = onTop;

				HealthBar[] healthBars = UnityEngine.Object.FindObjectsOfType<HealthBar>();
				Speedometer[] speedometers = UnityEngine.Object.FindObjectsOfType<Speedometer>();

				HashSet<TMP_Text> processedTexts = new HashSet<TMP_Text>();

				foreach (HealthBar hb in healthBars)
				{
					if (hb == null)
						continue;
					ApplyOverlayToAllHealthBarTexts(hb, onTop, ___overlayTextMaterial, ___normalTextMaterial, processedTexts);
				}

				foreach (Speedometer spd in speedometers)
				{
					if (spd == null || spd.textMesh == null)
						continue;
					if (processedTexts.Add(spd.textMesh))
						ApplyOverlayZTest(
							spd.textMesh,
							onTop,
							___overlayTextMaterial,
							___normalTextMaterial,
							forceManagedShadow: true
						);
				}

				ApplyOverlayToStyleHudTexts(onTop, ___overlayTextMaterial, ___normalTextMaterial, processedTexts);

				if (___textElements != null && ___textElements.Length != 0)
				{
					TMP_Text[] array = ___textElements;
					foreach (TMP_Text val in array)
					{
						if (val == null)
							continue;
						if (processedTexts.Add(val))
							ApplyOverlayZTest(val, onTop, ___overlayTextMaterial, ___normalTextMaterial);
					}
				}

				foreach (TextMeshProUGUI text in forcedShadowTexts)
				{
					if (IsAdditionalForcedShadowText(text, ___textElements) && processedTexts.Add(text))
						ApplyOverlayZTest(text, onTop, ___overlayTextMaterial, ___normalTextMaterial, forceManagedShadow: true);
				}
			}
			catch (Exception e)
			{
				Logging.Warn("Failed to apply Always On Top font swap");
				Logging.Warn(e.ToString());
			}
			return false;
		}

		public static void ReapplyOverlayToAll(bool onTop)
		{
			var hud = HudController.Instance;
			if (hud == null) return;

			HashSet<TMP_Text> processedTexts = new HashSet<TMP_Text>();

			foreach (var hb in UnityEngine.Object.FindObjectsOfType<HealthBar>())
			{
				if (hb != null)
					ApplyOverlayToAllHealthBarTexts(hb, onTop, hud.overlayTextMaterial, hud.normalTextMaterial, processedTexts);
			}

			foreach (var spd in UnityEngine.Object.FindObjectsOfType<Speedometer>())
				if (spd?.textMesh != null)
					if (processedTexts.Add(spd.textMesh))
						ApplyOverlayZTest(
							spd.textMesh,
							onTop,
							hud.overlayTextMaterial,
							hud.normalTextMaterial,
							forceManagedShadow: true
						);

			ApplyOverlayToStyleHudTexts(onTop, hud.overlayTextMaterial, hud.normalTextMaterial, processedTexts);

			if (!CommonFunctions.isUsingEnglish() && hud.textElements != null)
				foreach (var t in hud.textElements)
					if (t != null && processedTexts.Add(t))
						ApplyOverlayZTest(t, onTop, hud.overlayTextMaterial, hud.normalTextMaterial);

			foreach (TextMeshProUGUI text in forcedShadowTexts)
			{
				bool wasAppliedAsHudElement = !CommonFunctions.isUsingEnglish() &&
					hud.textElements != null &&
					Array.IndexOf(hud.textElements, text) >= 0;
				if (!wasAppliedAsHudElement &&
				    IsAdditionalForcedShadowText(text, null) &&
				    processedTexts.Add(text))
					ApplyOverlayZTest(text, onTop, hud.overlayTextMaterial, hud.normalTextMaterial, forceManagedShadow: true);
			}
		}
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
				TextMeshProUGUI __instance2 = ((Component)val).GetComponentInChildren<TextMeshProUGUI>();
				if (Core.TMPFontReady)
				{
					SwapTMPFont(ref __instance2);
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

        public static void SwapTMPFont(ref TextMeshProUGUI __instance, bool onTop = false, bool editOverlayStatus = false, bool isConvertedFromText = false, string originalFontName = null, bool forceUnderlay = false, bool skipIntermissionUnderlay = false)
        {
            // Защита от null
            if (__instance == null)
                return;
            
            // Если шрифты ещё не загружены, выходим
            if (!Core.TMPFontReady || Core.GlobalFontTMP == null)
                return;

            if (__instance.text != null && __instance.text.Contains("■") && __instance.text.Contains("|"))
                return;

if (IsBossBarText((TMP_Text)__instance) || IsHealthText((TMP_Text)__instance))
			{
				TMP_FontAsset bossFont = Core.CustomMainFontTMP ?? Core.GlobalFontTMP;
				if (bossFont != null && ((TMP_Text)__instance).font != bossFont)
				{
					((TMP_Text)__instance).font = bossFont;
				}
				// Apply underlay/shadow for health bar text (HP text, HP Symbol, boss bars)
				TMPFontUtils.ApplyUnderlayAndZTest(__instance,
					new Vector4(0f, 0f, 0f, 0.75f),
					new Vector4(1.5f, -1.5f, 0f, 0f),
					0f, 0f,
					false, true, onTop, false, bossFont, null, null);
				return;
			}

            // Log the original font before replacement
            TMPFontLogger.LogFont(__instance.font);

			string text2 = LanguageManager.CurrentLanguage.metadata.langName.ToLower().Substring(0, 2);
			bool isUnderlaid = forceUnderlay || TMPShadowPolicy.RequiresForcedShadow(__instance.transform);
			bool isOverlay = onTop;
			Material currentMaterial = ((TMP_Text)__instance).fontMaterial;
			Material sharedMatFallback = ((TMP_Text)__instance).fontSharedMaterial;
			//Logging.Message($"[SWAP] SwapTMPFont: {((Component)__instance).gameObject.name}, isConverted={isConvertedFromText}, origFont={originalFontName ?? "NULL"}");
			//Logging.Message($"[SWAP]   fontMaterial={currentMaterial?.name ?? "NULL"}, fontSharedMaterial={sharedMatFallback?.name ?? "NULL"}");
			//if (currentMaterial != null) Logging.Message($"[SWAP]   mat.hasKeyword(UNDERLAY_ON)={currentMaterial.IsKeywordEnabled("UNDERLAY_ON")}");

			Vector4 underlayColor = new Vector4(0f, 0f, 0f, 0f);
			Vector4 underlayOffset = Vector4.zero;
			float underlaySoftness = 0f;
			float underlayDilate = 0f;
			bool preserveExistingUnderlay = false;
			Material underlaySource = currentMaterial;
			if ((underlaySource == null || !underlaySource.IsKeywordEnabled("UNDERLAY_ON")) &&
			    sharedMatFallback != null &&
			    sharedMatFallback.IsKeywordEnabled("UNDERLAY_ON"))
			{
				underlaySource = sharedMatFallback;
			}
			if (underlaySource != null && underlaySource.IsKeywordEnabled("UNDERLAY_ON"))
			{
				if (underlaySource.HasProperty("_UnderlayColor"))
					underlayColor = underlaySource.GetVector("_UnderlayColor");
				underlayOffset = TMPFontUtils.GetUnderlayOffset(underlaySource, Vector4.zero);
				if (underlaySource.HasProperty("_UnderlaySoftness"))
					underlaySoftness = underlaySource.GetFloat("_UnderlaySoftness");
				if (underlaySource.HasProperty("_UnderlayDilate"))
					underlayDilate = underlaySource.GetFloat("_UnderlayDilate");
				preserveExistingUnderlay = true;
			}
			if (isUnderlaid && (!preserveExistingUnderlay || underlayColor.w <= 0.001f))
			{
				underlayColor = new Vector4(0f, 0f, 0f, 0.75f);
				underlayOffset = new Vector4(1.5f, -1.5f, 0f, 0f);
				underlaySoftness = 0f;
				underlayDilate = 0f;
			}
			//Logging.Message($"[SWAP]   after fontMaterial read: underlayColor=({underlayColor.x:F2},{underlayColor.y:F2},{underlayColor.z:F2},{underlayColor.w:F2}), preserve={preserveExistingUnderlay}");

			// Force underlay for Text->TMP converted texts in intermission scenes
			if (isConvertedFromText && !skipIntermissionUnderlay)
			{
				string currentScene = CommonFunctions.GetCurrentSceneName();
				//Logging.Message($"[SWAP]   isConvertedFromText, scene={currentScene}");
				if (currentScene == "Intermission1" || currentScene == "Intermission2")
				{
					underlayColor = new Vector4(0f, 0f, 0f, 0.75f);
					underlayOffset = new Vector4(1.5f, -1.5f, 0f, 0f);
					isUnderlaid = true;
					//Logging.Message($"[SWAP]   INTERMISSION: force shadow, isUnderlaid=true");
				}
			}

			// Determine which font to use
			TMP_FontAsset mainFont = Core.GlobalFontTMP;
			TMP_FontAsset museumFont = Core.MuseumFontTMP ?? mainFont; // Fallback to main font if null
			TMP_FontAsset terminalFont = Core.GlobalFontTMP; // Default to main font
			TMP_FontAsset secretTerminalFont = Core.GlobalFontTMP;
			Material overlayMat = Core.GlobalFontTMPOverlayMat;
			Material normalMat = Core.GlobalFontTMP?.material;

			// Check for custom fonts
			if (Core.CustomMainFontTMP != null)
			{
				mainFont = Core.CustomMainFontTMP;
				overlayMat = Core.CustomMainFontTMPOverlayMat ?? Core.GlobalFontTMPOverlayMat;
				normalMat = mainFont?.material;
			}
			if (Core.CustomMuseumFontTMP != null)
			{
				museumFont = Core.CustomMuseumFontTMP;
			}
			if (Core.CustomTerminalFontTMP != null)
			{
				terminalFont = Core.CustomTerminalFontTMP;
			}
			if (Core.CustomSecretTerminalFontTMP != null)
			{
				secretTerminalFont = Core.CustomSecretTerminalFontTMP;
			}

			// Determine materials for each font type
			Material museumOverlayMat = Core.CustomMuseumFontTMPOverlayMat ?? Core.GlobalFontTMPOverlayMat;
			Material museumNormalMat = museumFont?.material;

			Material terminalOverlayMat = Core.CustomTerminalFontTMPOverlayMat ?? Core.GlobalFontTMPOverlayMat;
			Material terminalNormalMat = terminalFont?.material;

			Material secretTerminalOverlayMat = Core.CustomSecretTerminalFontTMPOverlayMat ?? Core.GlobalFontTMPOverlayMat;
			Material secretTerminalNormalMat = secretTerminalFont?.material;

			// Special handling for Text converted to TMP
			if (isConvertedFromText && !string.IsNullOrEmpty(originalFontName))
			{
				// If original font is museum font, use museum font (custom if available)
				// Museum font can be "GFS Garaldus", "EBGaramond", or any font containing "Garaldus" or "Garamond"
				if (originalFontName == "GFS Garaldus" || originalFontName.Contains("Garaldus") || originalFontName.Contains("EBGaramond") || originalFontName.Contains("Garamond"))
				{
					if (museumFont != null)
					{
						ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, museumFont, museumOverlayMat, museumNormalMat);
					}
					else
					{
						// Fallback to main font if museum font not available
						ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
					}
					return;
				}
				// For other fonts (VCR OSD mono), use main font
				else
				{
					ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
					return;
				}
			}

			// Check original TMP font for tahoma or fs-tahoma-8px SDF (only for non-converted texts)
			if (!isConvertedFromText)
			{
				string currentFontName = __instance.font?.name;
				if (!string.IsNullOrEmpty(currentFontName))
				{
					string fontNameLower = currentFontName.ToLower();
					if (fontNameLower.Contains("tahoma") || fontNameLower.Contains("fs-tahoma-8px sdf"))
					{
						// Apply terminal font
                        ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, terminalFont, terminalOverlayMat, terminalNormalMat);
						return;
					}
					else if (fontNameLower.Contains("bittypix monospace ") && fontNameLower.Contains("bittypix"))
					{
						// Apply secret terminal font
						ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, secretTerminalFont, secretTerminalOverlayMat, secretTerminalNormalMat);
						return;
					}
				}
			}

			// Check if this is a terminal or secret terminal text
			bool isTerminal = ((Component)__instance).gameObject.name.ToLower().Contains("terminal") ||
							  ((Component)((TMP_Text)__instance).transform.parent).gameObject.name.ToLower().Contains("terminal");
			bool isSecretTerminal = ((Component)__instance).gameObject.name.ToLower().Contains("secret") ||
									((Component)((TMP_Text)__instance).transform.parent).gameObject.name.ToLower().Contains("secret");

			// If terminal and custom terminal font exists, use it
			if (isTerminal && !isSecretTerminal && terminalFont != Core.GlobalFontTMP)
			{
                ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, terminalFont, terminalOverlayMat, terminalNormalMat);
				return;
			}

			// Original language-based logic
			switch (text2)
			{
			case "zh":
				ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, Core.CJKFontTMP, Core.CJKFontTMPOverlayMat, ((TMP_Asset)Core.CJKFontTMP).material);
				break;
			case "ja":
				ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, Core.JaFontTMP, Core.jaFontTMPOverlayMat, ((TMP_Asset)Core.JaFontTMP).material);
				break;
			case "ar":
			case "fa":
			case "ur":
			{
				TextAlignmentOptions alignment = ((TMP_Text)__instance).alignment;
				if ((int)alignment <= 513)
				{
					if ((int)alignment != 257)
					{
						if ((int)alignment == 513)
						{
							((TMP_Text)__instance).alignment = (TextAlignmentOptions)516;
						}
					}
					else
					{
						((TMP_Text)__instance).alignment = (TextAlignmentOptions)260;
					}
				}
				else if ((int)alignment != 1025)
				{
					if ((int)alignment == 2049)
					{
						((TMP_Text)__instance).alignment = (TextAlignmentOptions)2052;
					}
				}
				else
				{
					((TMP_Text)__instance).alignment = (TextAlignmentOptions)1028;
				}
				Core.GlobalFontTMP.fallbackFontAssetTable.Add(Core.ArabicFontTMP);
				if (CommonFunctions.GetCurrentSceneName() == "CreditsMuseum2" && ((TMP_Text)__instance).font.name == "GFS Garaldus")
				{
					ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, museumFont, museumOverlayMat, museumNormalMat);
				}
				else
				{
					ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
				}
				break;
			}
			case "jr":
			case "he":
			case "yi":
			case "la":
			case "ro":
				ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, Core.HebrewFontTMP, Core.GlobalFontTMPOverlayMat, ((TMP_Asset)Core.GlobalFontTMP).material);
				break;
			default:
				if (CommonFunctions.GetCurrentSceneName() == "CreditsMuseum2" && ((TMP_Text)__instance).font.name == "GFS Garaldus")
				{
					ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, museumFont, museumOverlayMat, museumNormalMat);
				}
				else
				{
					ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
				}
				break;
			}
		}
        //test font size scaling for terminal font
        private static readonly Dictionary<TextMeshProUGUI, float> OriginalFontSizes = new Dictionary<TextMeshProUGUI, float>();

        public static int TerminalFontScale = LanguageManager.CurrentLanguage.metadata.tmFontSize;

        private static void ApplyTerminalFontScale(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return;

            float scale = TerminalFontScale / 100f;

            if (!OriginalFontSizes.TryGetValue(tmp, out float originalSize))
            {
                originalSize = tmp.fontSize;
                OriginalFontSizes[tmp] = originalSize;
            }

            tmp.fontSize = originalSize * scale;

            if (tmp.enableAutoSizing)
            {
                tmp.fontSizeMin *= scale;
                tmp.fontSizeMax *= scale;
            }
        }

        private static void ApplyFont(
            TextMeshProUGUI tmp,
            Vector4 underlayColor,
            Vector4 underlayOffset,
            float underlaySoftness,
            float underlayDilate,
            bool preserveExistingUnderlay,
            bool isUnderlaid,
            bool isOverlay,
            bool editOverlayStatus,
            TMP_FontAsset font,
            Material overlayMat,
            Material normalMat)
        {
            TMPFontUtils.ApplyUnderlayAndZTest(
                tmp,
                underlayColor,
                underlayOffset,
                underlaySoftness,
                underlayDilate,
                preserveExistingUnderlay,
                isUnderlaid,
                isOverlay,
                editOverlayStatus,
                font,
                overlayMat,
                normalMat
            );

            if (font == Core.CustomTerminalFontTMP)
            {
                ApplyTerminalFontScale(tmp);
            }
        }
public static void ClearFontSwapCache()
		{
			TerminalFontScale = LanguageManager.CurrentLanguage.metadata.tmFontSize;
			OriginalFontSizes.Clear();
			//TextMeshProFontSwapper.ClearCache();
			//TMPFontUtils.ClearMaterialCache();
		}

		[HarmonyPatch(typeof(HealthBar), "Start")]
		public static class HealthBarOverlayPatch
		{
			[HarmonyPostfix]
			public static void Start_Postfix(HealthBar __instance)
			{
				if (__instance == null)
					return;
				
				// Apply shadow/underlay to health text (hpText) if it's already TMP
				if (__instance.hpText != null)
				{
					var hpTextUGUI = __instance.hpText as TextMeshProUGUI;
					if (hpTextUGUI != null)
					{
						TMPFontUtils.ApplyUnderlayAndZTest(
							hpTextUGUI,
							new Vector4(0f, 0f, 0f, 0.75f),
							new Vector4(1.5f, -1.5f, 0f, 0f),
							0f, 0f,
							false, true, false, false,
							Core.GlobalFontTMP,
							Core.GlobalFontTMPOverlayMat,
							Core.GlobalFontTMP?.material
						);
					}
					else
					{
						// hpText is UnityEngine.Text (legacy) - find the converted TMP sibling and apply shadow
						var hpTextGameObject = __instance.hpText?.gameObject;
						if (hpTextGameObject != null)
						{
							var legacyText = hpTextGameObject.GetComponent<Text>();
							if (legacyText != null)
							{
								var convertedTMP = TextToTMPConverter.GetConvertedTMP(legacyText);
								if (convertedTMP != null)
								{
									TMPFontUtils.ApplyUnderlayAndZTest(
										convertedTMP,
										new Vector4(0f, 0f, 0f, 0.75f),
										new Vector4(1.5f, -1.5f, 0f, 0f),
										0f, 0f,
										false, true, false, false,
										Core.GlobalFontTMP,
										Core.GlobalFontTMPOverlayMat,
										Core.GlobalFontTMP?.material
									);
								}
							}
						}
					}
				}
				
				// Find and patch HP Symbol ("+" sign) - child TMP_Text in hierarchy
				var hpSymbol = __instance.transform.GetComponentsInChildren<TextMeshProUGUI>(true)
					.FirstOrDefault(t => t != null && (t.name.IndexOf("HP Symbol", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Plus", StringComparison.OrdinalIgnoreCase) >= 0));
				if (hpSymbol != null)
				{
					TMPFontUtils.ApplyUnderlayAndZTest(
						hpSymbol,
						new Vector4(0f, 0f, 0f, 0.75f),
						new Vector4(1.5f, -1.5f, 0f, 0f),
						0f, 0f,
						false, true, false, false,
						Core.GlobalFontTMP,
						Core.GlobalFontTMPOverlayMat,
						Core.GlobalFontTMP?.material
					);
				}

				var hud = HudController.Instance;
				if (hud == null)
					return;

				HudControllerPatch.ApplyOverlayToAllHealthBarTexts(
					__instance,
					HudControllerPatch.isOverlaid,
					hud.overlayTextMaterial,
					hud.normalTextMaterial
				);
			}
		}

		[HarmonyPatch(typeof(Speedometer), "OnEnable")]
		public static class SpeedometerOnEnableOverlayPatch
		{
			[HarmonyPostfix]
			public static void OnEnable_Postfix(Speedometer __instance)
			{
				if (__instance?.textMesh != null)
				{
					var hud = HudController.Instance;
					if (hud != null)
						HudControllerPatch.ApplyOverlayZTest(
							__instance.textMesh,
							HudControllerPatch.isOverlaid,
							hud.overlayTextMaterial,
							hud.normalTextMaterial,
							forceManagedShadow: true
						);
				}
				if (HudControllerPatch.isOverlaid && !__instance.gameObject.activeSelf)
					__instance.gameObject.SetActive(true);
			}
		}

		[HarmonyPatch(typeof(Speedometer), "OnPrefChanged")]
		public static class SpeedometerPrefOverlayPatch
		{
			[HarmonyPostfix]
			public static void OnPrefChanged_Postfix(Speedometer __instance, string id, object value)
			{
				if (id == "speedometer" && HudControllerPatch.isOverlaid && __instance?.textMesh != null)
				{
					var hud = HudController.Instance;
					if (hud != null)
						HudControllerPatch.ApplyOverlayZTest(
							__instance.textMesh,
							true,
							hud.overlayTextMaterial,
							hud.normalTextMaterial,
							forceManagedShadow: true
						);
				}
			}
		}

		[HarmonyPatch(typeof(HudController), "Start")]
		public static class HudControllerStartOverlayPatch
		{
			[HarmonyPostfix]
			public static void Start_Postfix()
			{
				HudControllerPatch.ReapplyOverlayToAll(HudControllerPatch.isOverlaid);
			}
		}

		[HarmonyPatch(typeof(HudController), "OnPrefChanged")]
		public static class HudControllerPrefOverlayPatch
		{
			[HarmonyPostfix]
			public static void OnPrefChanged_Postfix(HudController __instance, string key, object value)
			{
				if (key == "hudType")
					HudControllerPatch.ReapplyOverlayToAll(HudControllerPatch.isOverlaid);
			}
		}
	}
}
