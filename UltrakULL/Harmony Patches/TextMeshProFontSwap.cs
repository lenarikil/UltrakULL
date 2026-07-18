using System;
using System.Collections.Generic;
using System.IO;
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
			public static bool isOverlaid = MonoSingleton<PrefsManager>.Instance.GetBool("hudAlwaysOnTop", false);

			[HarmonyPatch("SetAlwaysOnTop")]
			[HarmonyPrefix]
			public static bool SetAlwaysOnTop_Prefix(ref TMP_Text[] ___textElements, bool onTop, Material ___overlayTextMaterial, Material ___normalTextMaterial)
			{
				if (CommonFunctions.isUsingEnglish())
				{
					return true;
				}
				isOverlaid = onTop;
				if (___textElements.Length != 0)
				{
					TMP_Text[] array = ___textElements;
					foreach (TMP_Text val in array)
					{
						if (IsBossBarText(val))
						{
							TMP_FontAsset bossFont = Core.CustomMainFontTMP ?? Core.GlobalFontTMP;
							if (bossFont != null)
								val.font = bossFont;
							val.fontSharedMaterial = (isOverlaid ? ___overlayTextMaterial : ___normalTextMaterial);
							continue;
						}
						TextMeshProUGUI __instance = ((Component)val).GetComponent<TextMeshProUGUI>();
						SwapTMPFont(ref __instance, isOverlaid, editOverlayStatus: true);
					}
				}
				return false;
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
    }
}
