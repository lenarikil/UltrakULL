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
			GameObject gameObjectChild = CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(canvasObj, "Hellmap"), "Hellmap Act 3");
			((TMP_Text)CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(gameObjectChild, "Text"))).text = LanguageManager.CurrentLanguage.misc.hellmap_violence;
			((TMP_Text)CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(gameObjectChild, "Text (1)"))).text = LanguageManager.CurrentLanguage.misc.hellmap_fraud;
			((TMP_Text)CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(gameObjectChild, "Text (2)"))).text = LanguageManager.CurrentLanguage.misc.hellmap_treachery;
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
				GameObject gameObjectChild = CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetInactiveRootObject("Other Interiors"), "9 - Tram Station"), "9 Stuff"), "9A"), "InteractiveScreenWithStand"), "InteractiveScreen"), "Canvas"), "Background");
				TextMeshProUGUI textMeshProUGUI = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "A"), "Opened"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI2 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "A"), "Closed"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI3 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "B"), "Opened"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI4 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "B"), "Closed"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI5 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "C"), "Opened"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI6 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "C"), "Closed"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI7 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "D"), "Opened"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI8 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild, "D"), "Closed"), "Text (TMP)"));
				((TMP_Text)textMeshProUGUI).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				((TMP_Text)textMeshProUGUI2).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				((TMP_Text)textMeshProUGUI3).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				((TMP_Text)textMeshProUGUI4).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				((TMP_Text)textMeshProUGUI5).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				((TMP_Text)textMeshProUGUI6).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				((TMP_Text)textMeshProUGUI7).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlOpen;
				((TMP_Text)textMeshProUGUI8).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_gateControlClosed;
				GameObject gameObjectChild2 = CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetInactiveRootObject("Outdoors"), "10 - Ambush Station"), "10 Nonstuff"), "InteractiveScreenWithStand"), "InteractiveScreen"), "Canvas"), "Background");
				TextMeshProUGUI textMeshProUGUI9 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(gameObjectChild2, "Text (TMP) (1)"));
				TextMeshProUGUI textMeshProUGUI10 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild2, "Button (Open)"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI11 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild2, "Button (Closed)"), "Text (TMP)"));
				((TMP_Text)textMeshProUGUI9).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_cartGateControlTitle;
				((TMP_Text)textMeshProUGUI10).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_cartGateControlOpen;
				((TMP_Text)textMeshProUGUI11).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_cartGateControlClosed;
				GameObject gameObjectChild3 = CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetInactiveRootObject("Outdoors"), "11 - Bomb Station"), "11 Nonstuff"), "Bomb Mechanisms"), "InteractiveScreenWithStand"), "InteractiveScreen"), "Canvas");
				TextMeshProUGUI textMeshProUGUI12 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(gameObjectChild3, "Text (TMP)"));
				TextMeshProUGUI[] componentsInChildren = CommonFunctions.GetGameObjectChild(gameObjectChild3, "UsableButtons").GetComponentsInChildren<TextMeshProUGUI>(true);
				TextMeshProUGUI textMeshProUGUI13 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild3, "UsableButtons"), "Error"));
				TextMeshProUGUI textMeshProUGUI14 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(gameObjectChild3, "Done"));
				((TMP_Text)textMeshProUGUI12).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlTitle;
				TextMeshProUGUI[] array = componentsInChildren;
				foreach (TextMeshProUGUI val in array)
				{
					if (((TMP_Text)val).text.Contains("LOWER"))
					{
						((TMP_Text)val).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlLower;
					}
				}
				((TMP_Text)textMeshProUGUI13).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlError1 + "<size=12>\n" + LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlError2;
				((TMP_Text)textMeshProUGUI14).text = LanguageManager.CurrentLanguage.act3.act3_violenceSecond_payloadControlHell;
			}
			else if (currentSceneName.Contains("7-3"))
			{
				GameObject gameObjectChild4 = CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetInactiveRootObject("Outdoors Areas"), "8 - Upper Garden Battlefield"), "8 Stuff"), "Destructible Tunnel"), "InteractiveScreenWithStand"), "InteractiveScreen"), "Canvas"), "Background");
				TextMeshProUGUI textMeshProUGUI15 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild4, "PreActivation"), "Text (TMP) (1)"));
				TextMeshProUGUI textMeshProUGUI16 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild4, "PreActivation"), "InteractiveScreenButton"), "Text (TMP)"));
				TextMeshProUGUI textMeshProUGUI17 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(gameObjectChild4, "PostActivation"), "Text (TMP) (1)"));
				((TMP_Text)textMeshProUGUI15).text = LanguageManager.CurrentLanguage.act3.act3_violenceThird_becomeMarked;
				((TMP_Text)textMeshProUGUI16).text = LanguageManager.CurrentLanguage.act3.act3_violenceThird_becomeMarkedButton;
				((TMP_Text)textMeshProUGUI17).text = "<color=red>" + LanguageManager.CurrentLanguage.act3.act3_violenceThird_starOfTheShow + "</color>";
			}
			else if (currentSceneName.Contains("7-4"))
			{
				((TMP_Text)CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(canvasObj, "Warning"), "Text (TMP)"))).text = LanguageManager.CurrentLanguage.act3.act3_violenceFourth_floodingWarning;
				((TMP_Text)CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(canvasObj, "Countdown"), "Text (TMP)"))).text = LanguageManager.CurrentLanguage.act3.act3_violenceFourth_countdownTitle;
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
						TextMeshProUGUI textComp = CommonFunctions.GetTextMeshProUGUI(screenObj);
						if (textComp != null)
						{
							if (!translationMissing)
							{
								((TMP_Text)textComp).text = outOfOrderTranslation;
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
				TextMeshProUGUI textMeshProUGUI18 = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(CommonFunctions.GetGameObjectChild(canvasObj, "HeightMarkerParent"), "HeightMarker"), "Title"));
				string act3_fraudFourth_heightMarkerTitle = LanguageManager.CurrentLanguage.act3.act3_fraudFourth_heightMarkerTitle;
				((TMP_Text)textMeshProUGUI18).text = CommonFunctions.MakeVertical(act3_fraudFourth_heightMarkerTitle);
				((TMP_Text)textMeshProUGUI18).ForceMeshUpdate(false, false);

				// Patch "N O P E" text
				GameObject introRoot = CommonFunctions.GetInactiveRootObject("The Intro");
				if (introRoot != null)
				{
					GameObject upperIntro = CommonFunctions.GetGameObjectChild(introRoot, "3 - Upper Intro");
					if (upperIntro != null)
					{
						GameObject elevatorSet = CommonFunctions.GetGameObjectChild(upperIntro, "ElevatorSet");
						if (elevatorSet != null)
						{
							GameObject elevator = CommonFunctions.GetGameObjectChild(elevatorSet, "Elevator");
							if (elevator != null)
							{
								GameObject interactiveScreen = CommonFunctions.GetGameObjectChild(elevator, "InteractiveScreen");
								if (interactiveScreen != null)
								{
									GameObject canvas = CommonFunctions.GetGameObjectChild(interactiveScreen, "Canvas");
									if (canvas != null)
									{
										GameObject background = CommonFunctions.GetGameObjectChild(canvas, "Background");
										if (background != null)
										{
											GameObject nopeBackground = CommonFunctions.GetGameObjectChild(background, "1 (Nope)");
											if (nopeBackground != null)
											{
												TextMeshProUGUI nopeText = CommonFunctions.GetTextMeshProUGUI(CommonFunctions.GetGameObjectChild(nopeBackground, "Text (TMP)"));
												if (nopeText != null)
												{
													string nopeTranslation = LanguageManager.CurrentLanguage.act3.act3_fraudFourth_nope;
													if (!string.IsNullOrEmpty(nopeTranslation))
													{
														((TMP_Text)nopeText).text = nopeTranslation;
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
						}
					}
				}
			}
		}
	}
}
