using System.Text.RegularExpressions;
using TMPro;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL
{
	public static class Act3
	{
		private static void PatchHellmap(ref GameObject canvasObj)
		{
			GameObject gameObjectChild = canvasObj.FindPath("Hellmap", "Hellmap Act 3");
			gameObjectChild.Child("Text").TMP().text = LanguageManager.CurrentLanguage.misc.hellmap_violence;
			gameObjectChild.Child("Text (1)").TMP().text = LanguageManager.CurrentLanguage.misc.hellmap_fraud;
			gameObjectChild.Child("Text (2)").TMP().text = LanguageManager.CurrentLanguage.misc.hellmap_treachery;
		}

		public static void PatchAct3(ref GameObject canvasObj)
		{
			string currentSceneName = CommonFunctions.GetCurrentSceneName();
			string levelName = Act3Strings.GetLevelName();
			string levelChallenge = Act3Strings.GetLevelChallenge(currentSceneName);
			CommonFunctions.PatchResultsScreen(levelName, levelChallenge);
			PatchHellmap(ref canvasObj);
			if (currentSceneName.Contains("7-2"))
			{
				GameObject gameObjectChild = CommonFunctions.GetInactiveRootObject("Other Interiors").FindPath("9 - Tram Station", "9 Stuff", "9A", "InteractiveScreenWithStand", "InteractiveScreen", "Canvas", "Background");
				TextMeshProUGUI textMeshProUGUI = gameObjectChild.FindPath("A", "Opened", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI2 = gameObjectChild.FindPath("A", "Closed", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI3 = gameObjectChild.FindPath("B", "Opened", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI4 = gameObjectChild.FindPath("B", "Closed", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI5 = gameObjectChild.FindPath("C", "Opened", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI6 = gameObjectChild.FindPath("C", "Closed", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI7 = gameObjectChild.FindPath("D", "Opened", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI8 = gameObjectChild.FindPath("D", "Closed", "Text (TMP)").TMP();
				textMeshProUGUI.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				textMeshProUGUI2.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				textMeshProUGUI3.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				textMeshProUGUI4.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				textMeshProUGUI5.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				textMeshProUGUI6.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				textMeshProUGUI7.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				textMeshProUGUI8.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				GameObject gameObjectChild2 = CommonFunctions.GetInactiveRootObject("Outdoors").FindPath("10 - Ambush Station", "10 Nonstuff", "InteractiveScreenWithStand", "InteractiveScreen", "Canvas", "Background");
				TextMeshProUGUI textMeshProUGUI9 = gameObjectChild2.Child("Text (TMP) (1)").TMP();
				TextMeshProUGUI textMeshProUGUI10 = gameObjectChild2.FindPath("Button (Open)", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI11 = gameObjectChild2.FindPath("Button (Closed)", "Text (TMP)").TMP();
				textMeshProUGUI9.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_cartGateControlTitle;
				textMeshProUGUI10.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_cartGateControlOpen;
				textMeshProUGUI11.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_cartGateControlClosed;
				GameObject gameObjectChild3 = CommonFunctions.GetInactiveRootObject("Outdoors").FindPath("11 - Bomb Station", "11 Nonstuff", "Bomb Mechanisms", "InteractiveScreenWithStand", "InteractiveScreen", "Canvas");
				TextMeshProUGUI textMeshProUGUI12 = gameObjectChild3.Child("Text (TMP)").TMP();
				TextMeshProUGUI[] componentsInChildren = gameObjectChild3.Child("UsableButtons").GetComponentsInChildren<TextMeshProUGUI>(true);
				TextMeshProUGUI textMeshProUGUI13 = gameObjectChild3.FindPath("UsableButtons", "Error").TMP();
				TextMeshProUGUI textMeshProUGUI14 = gameObjectChild3.Child("Done").TMP();
				textMeshProUGUI12.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlTitle;
				TextMeshProUGUI[] array = componentsInChildren;
				foreach (TextMeshProUGUI val in array)
				{
					if (val.text.Contains("LOWER"))
					{
						val.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlLower;
					}
				}
				textMeshProUGUI13.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlError1 + "<size=10>\n" + LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlError2;
				textMeshProUGUI14.text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlHell;
			}
			else if (currentSceneName.Contains("7-3"))
			{
				GameObject gameObjectChild4 = CommonFunctions.GetInactiveRootObject("Outdoors Areas").FindPath("8 - Upper Garden Battlefield", "8 Stuff", "Destructible Tunnel", "InteractiveScreenWithStand", "InteractiveScreen", "Canvas", "Background");
				TextMeshProUGUI textMeshProUGUI15 = gameObjectChild4.FindPath("PreActivation", "Text (TMP) (1)").TMP();
				TextMeshProUGUI textMeshProUGUI16 = gameObjectChild4.FindPath("PreActivation", "InteractiveScreenButton", "Text (TMP)").TMP();
				TextMeshProUGUI textMeshProUGUI17 = gameObjectChild4.FindPath("PostActivation", "Text (TMP) (1)").TMP();
				textMeshProUGUI15.text = LanguageManager.CurrentLanguage.act3.act3_violenceThird_becomeMarked;
				textMeshProUGUI16.text = LanguageManager.CurrentLanguage.act3.act3_violenceThird_becomeMarkedButton;
				textMeshProUGUI17.text = "<color=red>" + LanguageManager.CurrentLanguage.act3.act3_violenceThird_starOfTheShow + "</color>";
			}
			else if (currentSceneName.Contains("7-4"))
			{
				canvasObj.FindPath("Warning", "Text (TMP)").TMP().text = LanguageManager.CurrentLanguage.act3.act3_violenceFourth_floodingWarning;
				canvasObj.FindPath("Countdown", "Text (TMP)").TMP().text = LanguageManager.CurrentLanguage.act3.act3_violenceFourth_countdownTitle;
			}
			else if (currentSceneName.Contains("8-2"))
			{
				GameObject hub = CommonFunctions.GetInactiveRootObject("4 - Hub");
				if (hub == null)
				{
					hub = CommonFunctions.GetInactiveRootObject("Hub");
				}
				if (hub != null)
				{
					TextMeshProUGUI[] allTexts = hub.GetComponentsInChildren<TextMeshProUGUI>(true);
					string outOfOrderTranslation = LanguageManager.CurrentLanguage.act3.act3_fraudSecond_outOfOrder;
					string errorResetTranslation = LanguageManager.CurrentLanguage.act3.act3_fraudSecond_errorResetPower;
					bool outOfOrderMissing = string.IsNullOrEmpty(outOfOrderTranslation);
					bool errorResetMissing = string.IsNullOrEmpty(errorResetTranslation);
					if (outOfOrderMissing)
					{
						Logging.Warn("[Act3] Translation for 'act3_fraudSecond_outOfOrder' is missing or empty. Will keep original text.");
					}
					if (errorResetMissing)
					{
						Logging.Warn("[Act3] Translation for 'act3_fraudSecond_errorResetPower' is missing or empty. Will keep original text.");
					}
					foreach (TextMeshProUGUI text in allTexts)
					{
						string original = text.text;
						// Remove HTML tags
						string noTags = Regex.Replace(original, "<.*?>", "");
						// Replace newlines and multiple spaces with single space
						string normalized = Regex.Replace(noTags, @"\s+", " ").Trim().ToUpperInvariant();
						// Diagnostic log for texts that look similar
						if (normalized.Contains("ERROR") || normalized.Contains("RESET") || normalized.Contains("POWER") || normalized.Contains("OUT") || normalized.Contains("ORDER"))
						{
							Logging.Info($"[Act3] Text candidate: original='{original}', normalized='{normalized}'");
						}
						if (normalized == "OUT OF ORDER" && !outOfOrderMissing)
						{
							Logging.Info($"[Act3] Replacing OUT OF ORDER with translation");
							text.text = outOfOrderTranslation;
						}
						else if (normalized == "ERROR RESET POWER TO OPEN" && !errorResetMissing)
						{
							Logging.Info($"[Act3] Replacing ERROR RESET POWER TO OPEN with translation");
							text.text = errorResetTranslation;
						}
					}
				}
				else
				{
					Logging.Warn("[Act3] Hub not found");
				}
			}
			else if (currentSceneName.Contains("8-3"))
			{
				string[] screenPaths = new string[]
				{
					"Pre-Space/Rooms/10B - Night Street/10B Nonstuff/Office/ElevatorSet (1)/ElevatorStop/InteractiveScreen/Canvas/Background/Text (TMP)",
					"Pre-Space/Rooms/10B - Night Street/10B Nonstuff/Office/ElevatorSet (1)/ElevatorStop (1)/InteractiveScreen/Canvas/Background/Text (TMP)"
				};
				string outOfOrderTranslation = LanguageManager.CurrentLanguage.act3.act3_fraudSecond_outOfOrder;
				bool translationMissing = string.IsNullOrEmpty(outOfOrderTranslation);
				if (translationMissing)
				{
					Logging.Warn("[Act3] Translation for 'act3_fraudSecond_outOfOrder' is missing or empty. Will keep original text.");
				}
				foreach (string path in screenPaths)
				{
					GameObject screenObj = CommonFunctions.GetObject(path);
					if (screenObj != null)
					{
						TextMeshProUGUI textComp = screenObj.TMP();
						if (textComp != null)
						{
							if (!translationMissing)
							{
								textComp.text = outOfOrderTranslation;
								Logging.Info($"[Act3] Replaced OUT OF ORDER text on 8-3 screen at path: {path}");
							}
						}
						else
						{
							Logging.Warn($"[Act3] Text (TMP) component not found on screen object at path: {path}");
						}
					}
					else
					{
						Logging.Warn($"[Act3] Screen object not found at path: {path}");
					}
				}
			}
			else if (currentSceneName.Contains("8-4"))
			{
				TextMeshProUGUI textMeshProUGUI18 = canvasObj.FindPath("HeightMarkerParent", "HeightMarker", "Title").TMP();
				string act3_fraudFourth_heightMarkerTitle = LanguageManager.CurrentLanguage.act3.act3_fraudFourth_heightMarkerTitle;
				textMeshProUGUI18.text = CommonFunctions.MakeVertical(act3_fraudFourth_heightMarkerTitle);
				textMeshProUGUI18.ForceMeshUpdate(false, false);

				// Patch "N O P E" text
				GameObject introRoot = CommonFunctions.GetInactiveRootObject("The Intro");
				GameObject nopeBackground = introRoot.FindPath("3 - Upper Intro", "ElevatorSet", "Elevator", "InteractiveScreen", "Canvas", "Background", "1 (Nope)");
				if (nopeBackground != null)
				{
					TextMeshProUGUI nopeText = nopeBackground.Child("Text (TMP)").TMP();
					if (nopeText != null)
					{
						string nopeTranslation = LanguageManager.CurrentLanguage.act3.act3_fraudFourth_nope;
						if (!string.IsNullOrEmpty(nopeTranslation))
						{
							nopeText.text = nopeTranslation;
						}
						else
						{
							Logging.Warn("[Act3] Translation for 'act3_fraudFourth_nope' is missing or empty. Will keep original text.");
						}
					}
				}
			}
		}
	}
}
