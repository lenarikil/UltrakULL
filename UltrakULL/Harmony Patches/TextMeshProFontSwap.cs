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
using BepInEx;

namespace UltrakULL.Harmony_Patches
{
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
			private static List<IntPtr> objectsFixed = new List<IntPtr>();

			public static void ClearCache()
			{
				objectsFixed.Clear();
			}

            [HarmonyPostfix]
			public static void SwapFont(ref TextMeshProUGUI __instance, IntPtr ___m_CachedPtr)
			{
				if ((objectsFixed.Count <= 0 || !objectsFixed.Contains(___m_CachedPtr)) && Core.TMPFontReady && (!CommonFunctions.isUsingEnglish() || !(CommonFunctions.GetCurrentSceneName() != "Main Menu")))
				{
                    SwapTMPFont(ref __instance);
					objectsFixed.Add(___m_CachedPtr);
				}
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
			if (m_sharedMaterialField != null)
			{
				Material mat = m_sharedMaterialField.GetValue(text) as Material;
				if (mat != null)
					return mat;
			}
			return text.fontSharedMaterial;
		}

		private static void CopyUnderlayProps(Material from, Material to)
		{
			if (from == null || to == null)
				return;
			string[] props = { "_UnderlayColor", "_UnderlayOffset", "_UnderlaySoftness", "_UnderlayDilate" };
			foreach (var prop in props)
			{
				if (from.HasProperty(prop) && to.HasProperty(prop))
				{
					if (prop == "_UnderlayColor" || prop == "_UnderlayOffset")
						to.SetVector(prop, from.GetVector(prop));
					else
						to.SetFloat(prop, from.GetFloat(prop));
				}
			}
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

public static void ApplyOverlayZTest(TMP_Text text, bool onTop, Material overlayMat, Material normalMat)
		{
			if (text == null)
				return;
			Material refMat = onTop ? overlayMat : normalMat;
			if (refMat == null)
				return;
			Material currentMat = GetCurrentMaterialSafe(text);
			Material newMat = new Material(refMat);
			newMat.renderQueue = 5000;
			newMat.SetFloat("_ZTest", onTop ? 8f : 4f);
			if (currentMat != null && currentMat.HasProperty("_MainTex"))
			{
				newMat.SetTexture("_MainTex", currentMat.GetTexture("_MainTex"));
			}
			CopyUnderlayProps(currentMat, newMat);
			
			text.fontMaterial = newMat;
			
			ForceRendererUpdate(text, newMat);
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

foreach (HealthBar hb in healthBars)
				{
					if (hb == null || hb.hpText == null)
						continue;
					ApplyOverlayZTest(hb.hpText, onTop, ___overlayTextMaterial, ___normalTextMaterial);
				}

				foreach (Speedometer spd in speedometers)
				{
					if (spd == null || spd.textMesh == null)
						continue;
					ApplyOverlayZTest(spd.textMesh, onTop, ___overlayTextMaterial, ___normalTextMaterial);
				}

				if (CommonFunctions.isUsingEnglish())
				{
					return true;
				}

				if (___textElements != null && ___textElements.Length != 0)
				{
					TMP_Text[] array = ___textElements;
					foreach (TMP_Text val in array)
					{
						if (val == null)
							continue;
						ApplyOverlayZTest(val, onTop, ___overlayTextMaterial, ___normalTextMaterial);
					}
				}
}
			catch (Exception e)
			{
				Logging.Warn("Failed to apply Always On Top font swap");
				Logging.Warn(e.ToString());
			}
			return false;
		}

		public static void ReapplyOverlayToAll()
		{
			var hud = HudController.Instance;
			if (hud == null) return;

			foreach (var hb in UnityEngine.Object.FindObjectsOfType<HealthBar>())
			{
				if (hb?.hpText != null) ApplyOverlayZTest(hb.hpText, true, hud.overlayTextMaterial, hud.normalTextMaterial);
				// HP Symbol ("+" sign)
				var hpSymbol = hb.transform.GetComponentsInChildren<TextMeshProUGUI>(true)
					.FirstOrDefault(t => t != null && (t.name.IndexOf("HP Symbol", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Plus", StringComparison.OrdinalIgnoreCase) >= 0));
				if (hpSymbol != null) ApplyOverlayZTest(hpSymbol, true, hud.overlayTextMaterial, hud.normalTextMaterial);
			}

			foreach (var spd in UnityEngine.Object.FindObjectsOfType<Speedometer>())
				if (spd?.textMesh != null) ApplyOverlayZTest(spd.textMesh, true, hud.overlayTextMaterial, hud.normalTextMaterial);

			if (!CommonFunctions.isUsingEnglish() && hud.textElements != null)
				foreach (var t in hud.textElements) if (t != null) ApplyOverlayZTest(t, true, hud.overlayTextMaterial, hud.normalTextMaterial);
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

        public static void SwapTMPFont(ref TextMeshProUGUI __instance, bool onTop = false, bool editOverlayStatus = false, bool isConvertedFromText = false, string originalFontName = null)
        {
            // Защита от null
            if (__instance == null)
                return;
            
            // Если шрифты ещё не загружены, выходим
            if (!Core.TMPFontReady || Core.GlobalFontTMP == null)
                return;

            if (__instance.text != null && __instance.text.Contains("■") && __instance.text.Contains("|"))
                return;

            if (IsBossBarText((TMP_Text)__instance))
            {
                TMP_FontAsset bossFont = Core.CustomMainFontTMP ?? Core.GlobalFontTMP;
                if (bossFont != null && ((TMP_Text)__instance).font != bossFont)
                {
                    ((TMP_Text)__instance).font = bossFont;
                }
                return;
            }

            // Log the original font before replacement
            TMPFontLogger.LogFont(__instance.font);

			string text = null;
			if (((TMP_Text)__instance).transform.parent != null && ((TMP_Text)__instance).transform.parent.parent != null)
			{
				text = ((Component)((TMP_Text)__instance).transform.parent.parent).gameObject.name + "/" + ((Component)((TMP_Text)__instance).transform.parent).gameObject.name + "/" + ((Component)((TMP_Text)__instance).transform).gameObject.name;
			}
			string text2 = LanguageManager.CurrentLanguage.metadata.langName.ToLower().Substring(0, 2);
			bool isUnderlaid = ((Component)__instance).gameObject.name.Contains("NameText") || ((Component)__instance).gameObject.name.Contains("LayerText") || ((Component)((TMP_Text)__instance).transform.parent).gameObject.name.Contains("Cheats Info") || (text?.Equals("ReadingScanned/Panel/Text (1)") ?? false);
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

			if (currentMaterial != null && currentMaterial.HasProperty("_UnderlayColor"))
			{
				underlayColor = currentMaterial.GetVector("_UnderlayColor");
				preserveExistingUnderlay = underlayColor.w > 0.001f;
			}
			if (currentMaterial != null && currentMaterial.HasProperty("_UnderlayOffset"))
			{
				underlayOffset = currentMaterial.GetVector("_UnderlayOffset");
				preserveExistingUnderlay = preserveExistingUnderlay ||
					Mathf.Abs(underlayOffset.x) > 0.001f ||
					Mathf.Abs(underlayOffset.y) > 0.001f;
			}
			if (currentMaterial != null && currentMaterial.HasProperty("_UnderlaySoftness"))
			{
				underlaySoftness = currentMaterial.GetFloat("_UnderlaySoftness");
			}
			if (currentMaterial != null && currentMaterial.HasProperty("_UnderlayDilate"))
			{

			}

			if (preserveExistingUnderlay && currentMaterial != null && !currentMaterial.IsKeywordEnabled("UNDERLAY_ON"))
			{
				preserveExistingUnderlay = false;
				//Logging.Message($"[SWAP]   preserveExisting canceled: source keyword=False");
			}
			//Logging.Message($"[SWAP]   after fontMaterial read: underlayColor=({underlayColor.x:F2},{underlayColor.y:F2},{underlayColor.z:F2},{underlayColor.w:F2}), preserve={preserveExistingUnderlay}");

			// Fallback: check fontSharedMaterial if fontMaterial has no underlay
			if (!preserveExistingUnderlay)
			{
				Material sharedMat = sharedMatFallback;
				if (sharedMat != null && sharedMat.HasProperty("_UnderlayColor"))
				{
					Vector4 sharedColor = sharedMat.GetVector("_UnderlayColor");
					//Logging.Message($"[SWAP]   sharedMat._UnderlayColor=({sharedColor.x:F2},{sharedColor.y:F2},{sharedColor.z:F2},{sharedColor.w:F2})");
					if (sharedColor.w > 0.001f)
					{
						underlayColor = sharedColor;
						preserveExistingUnderlay = true;
						if (sharedMat.HasProperty("_UnderlayOffset"))
							underlayOffset = sharedMat.GetVector("_UnderlayOffset");
						if (sharedMat.HasProperty("_UnderlaySoftness"))
							underlaySoftness = sharedMat.GetFloat("_UnderlaySoftness");
						if (sharedMat.HasProperty("_UnderlayDilate"))
							underlayDilate = sharedMat.GetFloat("_UnderlayDilate");
						//Logging.Message($"[SWAP]   FOUND underlay in sharedMat! preserve=true, color=({underlayColor.x:F2},{underlayColor.y:F2},{underlayColor.z:F2},{underlayColor.w:F2})");
					}
				}
			}

			// Force underlay for Text->TMP converted texts in intermission scenes
			if (isConvertedFromText)
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
				overlayMat = Core.GlobalFontTMPOverlayMat; // Keep same overlay material for now
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
			Material museumOverlayMat = Core.GlobalFontTMPOverlayMat;
			Material museumNormalMat = museumFont?.material;
			if (Core.CustomMuseumFontTMP != null && Core.CustomMuseumFontTMPOverlayMat != null)
			{
				museumOverlayMat = Core.CustomMuseumFontTMPOverlayMat;
			}

			Material terminalOverlayMat = Core.GlobalFontTMPOverlayMat;
			Material terminalNormalMat = terminalFont?.material;
			if (Core.CustomTerminalFontTMP != null && Core.CustomTerminalFontTMPOverlayMat != null)
			{
				terminalOverlayMat = Core.CustomTerminalFontTMPOverlayMat;
			}

			Material secretTerminalOverlayMat = Core.GlobalFontTMPOverlayMat;
			Material secretTerminalNormalMat = secretTerminalFont?.material;
			if (Core.CustomSecretTerminalFontTMP != null && Core.CustomSecretTerminalFontTMPOverlayMat != null)
			{
				secretTerminalOverlayMat = Core.CustomSecretTerminalFontTMPOverlayMat;
			}

			// Special handling for Text converted to TMP
			if (isConvertedFromText && !string.IsNullOrEmpty(originalFontName))
			{
				Logging.Message($"Text converted to TMP: originalFontName='{originalFontName}', scene='{CommonFunctions.GetCurrentSceneName()}'");
				
				// If original font is museum font, use museum font (custom if available)
				// Museum font can be "GFS Garaldus", "EBGaramond", or any font containing "Garaldus" or "Garamond"
				if (originalFontName == "GFS Garaldus" || originalFontName.Contains("Garaldus") || originalFontName.Contains("EBGaramond") || originalFontName.Contains("Garamond"))
				{
					Logging.Message($"Detected museum font: originalFontName='{originalFontName}', museumFont={(museumFont != null ? museumFont.name : "NULL")}, custom={Core.CustomMuseumFontTMP != null}");
					if (museumFont != null)
					{
						Logging.Message($"Applying museum font: {museumFont.name} with materials (overlay={museumOverlayMat?.name}, normal={museumNormalMat?.name})");
						TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, museumFont, museumOverlayMat, museumNormalMat);
					}
					else
					{
						// Fallback to main font if museum font not available
						Logging.Message($"Museum font is null, falling back to main font: {mainFont?.name}");
						TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
					}
					return;
				}
				// For other fonts (VCR OSD mono), use main font
				else
				{
					Logging.Message($"Original font is not museum font, using main font: {mainFont?.name}");
					TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
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
						Logging.Message($"Detected Tahoma font: '{currentFontName}', applying terminal font: {terminalFont?.name}");
                        ApplyFont(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, terminalFont, terminalOverlayMat, terminalNormalMat);
						return;
					}
					else if (fontNameLower.Contains("bittypix monospace ") && fontNameLower.Contains("bittypix"))
					{
						// Apply secret terminal font
						Logging.Message($"Detected Bittypix Monospace font: '{currentFontName}', applying secret terminal font: {secretTerminalFont?.name}");
						TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, secretTerminalFont, secretTerminalOverlayMat, secretTerminalNormalMat);
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
				TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, Core.CJKFontTMP, Core.CJKFontTMPOverlayMat, ((TMP_Asset)Core.CJKFontTMP).material);
				break;
			case "ja":
				TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, Core.JaFontTMP, Core.jaFontTMPOverlayMat, ((TMP_Asset)Core.JaFontTMP).material);
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
					TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, museumFont, museumOverlayMat, museumNormalMat);
				}
				else
				{
					TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
				}
				break;
			}
			case "jr":
			case "he":
			case "yi":
			case "la":
			case "ro":
				TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, Core.HebrewFontTMP, Core.GlobalFontTMPOverlayMat, ((TMP_Asset)Core.GlobalFontTMP).material);
				break;
			default:
				if (CommonFunctions.GetCurrentSceneName() == "CreditsMuseum2" && ((TMP_Text)__instance).font.name == "GFS Garaldus")
				{
					TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, museumFont, museumOverlayMat, museumNormalMat);
				}
				else
				{
					TMPFontUtils.ApplyUnderlayAndZTest(__instance, underlayColor, underlayOffset, underlaySoftness, underlayDilate, preserveExistingUnderlay, isUnderlaid, isOverlay, editOverlayStatus, mainFont, overlayMat, normalMat);
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
				if (__instance == null || !HudControllerPatch.isOverlaid)
					return;
				var hud = HudController.Instance;
				if (hud == null)
					return;
				
				if (__instance.hpText != null)
					HudControllerPatch.ApplyOverlayZTest(__instance.hpText, true, hud.overlayTextMaterial, hud.normalTextMaterial);
				
				// Find and patch HP Symbol ("+" sign) - child TMP_Text in hierarchy
				var hpSymbol = __instance.transform.GetComponentsInChildren<TextMeshProUGUI>(true)
					.FirstOrDefault(t => t != null && (t.name.IndexOf("HP Symbol", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Plus", StringComparison.OrdinalIgnoreCase) >= 0));
				if (hpSymbol != null)
					HudControllerPatch.ApplyOverlayZTest(hpSymbol, true, hud.overlayTextMaterial, hud.normalTextMaterial);
			}
		}

		[HarmonyPatch(typeof(Speedometer), "OnEnable")]
		public static class SpeedometerOnEnableOverlayPatch
		{
			[HarmonyPostfix]
			public static void OnEnable_Postfix(Speedometer __instance)
			{
				if (__instance?.textMesh != null && HudControllerPatch.isOverlaid)
				{
					var hud = HudController.Instance;
					if (hud != null)
						HudControllerPatch.ApplyOverlayZTest(__instance.textMesh, true, hud.overlayTextMaterial, hud.normalTextMaterial);
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
						HudControllerPatch.ApplyOverlayZTest(__instance.textMesh, true, hud.overlayTextMaterial, hud.normalTextMaterial);
				}
			}
		}

		[HarmonyPatch(typeof(HudController), "OnPrefChanged")]
		public static class HudControllerPrefOverlayPatch
		{
			[HarmonyPostfix]
			public static void OnPrefChanged_Postfix(HudController __instance, string key, object value)
			{
				if (key == "hudType" && HudControllerPatch.isOverlaid)
				{
					HudControllerPatch.ReapplyOverlayToAll();
				}
			}
		}
	}
}
