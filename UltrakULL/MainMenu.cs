using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UltrakULL.json;
using System.Data;
using System.Threading.Tasks;

namespace UltrakULL
{
	public static class MainMenu
	{

		//Patches all text strings in the title menu.
		private static void PatchMainMenu(GameObject mainMenu)
		{
			try
			{
				GameObject titleObject = mainMenu.FindPath("Main Menu (1)", "LeftSide");

				//Early access tag
				TextMeshProUGUI earlyAccessText = titleObject.FindPath("Text (3)", "Text").TMP();
				earlyAccessText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_earlyAccess;
				TextMeshProUGUI earlyAccessBackground = titleObject.Child("Text (3)").TMP();
				earlyAccessBackground.text = "<mark=#000000>" + LanguageManager.CurrentLanguage.frontend.mainmenu_earlyAccess;

				//V1 Initialization strings
				TextMeshProUGUI v1InitText = titleObject.FindPath("Text (2)", "Text (1)").TMP();
				v1InitText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_v1Init;
				TextMeshProUGUI v1InitBackground = titleObject.Child("Text (2)").TMP(); // Yep. Background is a TMP too.
				v1InitBackground.text = "<mark=#000000>" + LanguageManager.CurrentLanguage.frontend.mainmenu_v1Init;

				//Init Socials
				TextMeshProUGUI initSocialsText = titleObject.FindPath("Panel", "Text (2)", "Text").TMP();
				initSocialsText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_initSocials;
				TextMeshProUGUI initSocialsBackground = titleObject.FindPath("Panel", "Text (2)").TMP(); // Yep. Background is a TMP too.
				initSocialsBackground.text = "<mark=#000000>" + LanguageManager.CurrentLanguage.frontend.mainmenu_initSocials;

				GameObject holidayObject = titleObject.Child("Holiday Greetings");
				//Halloween
				TextMeshProUGUI halloweenText = holidayObject.Child("Text (Halloween)").TMP();
				halloweenText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_halloween;

				//Easter
				TextMeshProUGUI easterText = holidayObject.Child("Text (Easter)").TMP();
				easterText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_easter;

				//Christmas
				TextMeshProUGUI christmasText = holidayObject.Child("Text (Christmas)").TMP();
				christmasText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_christmas;

				//Play button
				TextMeshProUGUI playButtonText = titleObject.FindPath("Continue", "Text").TMP();
				playButtonText.text = LanguageManager.CurrentLanguage.frontend.mainmenu_play;

				//Options button
				TextMeshProUGUI optionsButtontext = titleObject.FindPath("Options", "Text").TMP();
				optionsButtontext.text = LanguageManager.CurrentLanguage.frontend.mainmenu_options;

				//Credits button
				TextMeshProUGUI creditsButtontext = titleObject.FindPath("Credits", "Text").TMP();
				creditsButtontext.text = LanguageManager.CurrentLanguage.frontend.mainmenu_credits;

				//Quit button
				TextMeshProUGUI quitButtontext = titleObject.FindPath("Quit", "Text").TMP();
				quitButtontext.text = LanguageManager.CurrentLanguage.frontend.mainmenu_quit;
			}
			catch (Exception e)
			{
				Logging.Error("An error occured while patching main menu. Check the console for details.");
				Logging.Error(e.ToString());
			}
		}

		private static void PatchPopUps(GameObject mainMenu)
		{
			try
			{
				GameObject aboutEncoreObject = mainMenu.FindPath("EncorePopUp (1)", "Image");

				//About Encore Title
				TextMeshProUGUI aboutEncoreTitleText = aboutEncoreObject.Child("Text (TMP) (1)").TMP();
				if (LanguageManager.CurrentLanguage.frontend.aboutEncoreTitle != "")
				{
					aboutEncoreTitleText.text = LanguageManager.CurrentLanguage.frontend.aboutEncoreTitle;
				}
				else 
				{
					Logging.Warn("No aboutEncoreTitle text found in the language file. Using default: " + aboutEncoreTitleText.text);
				}
				//About Encore Main text
				TextMeshProUGUI aboutEncoreMainText = aboutEncoreObject.Child("Text (TMP)").TMP();
				if (LanguageManager.CurrentLanguage.frontend.aboutEncoreMain != "")
				{
					aboutEncoreMainText.text = LanguageManager.CurrentLanguage.frontend.aboutEncoreMain;
				}
				else 
				{ 
					Logging.Warn("No aboutEncoreMain text found in the language file. Using default: " + aboutEncoreMainText.text); 
				}

				//About Encore Button //Umm, id dont know, need to translate "Ok" button, but... OK
				GameObject aboutEncoreButtonTextObject = aboutEncoreObject.FindPath("General (1)", "Text");
				TextMeshProUGUI aboutEncoreButtonText = aboutEncoreButtonTextObject.TMP();
				if (LanguageManager.CurrentLanguage.frontend.aboutEncoreButton != "")
				{
					aboutEncoreButtonText.text = LanguageManager.CurrentLanguage.frontend.aboutEncoreButton;
				}
				else
				{
					Logging.Warn("No aboutEncoreButton text found in the language file. Using default: " + aboutEncoreButtonText.text);
				}

				//Encore available PopUp
				GameObject encoreAvailableObject = mainMenu.FindPath("EncorePopUp", "Image");

				//Encore available Main text
				TextMeshProUGUI encoreAvailableMainText = encoreAvailableObject.Child("Text (TMP)").TMP();
				if (LanguageManager.CurrentLanguage.frontend.encoreAvailableMainText != "")
				{
					encoreAvailableMainText.text = LanguageManager.CurrentLanguage.frontend.encoreAvailableMainText;
				}
				else
				{
					Logging.Warn("No encoreAvailableMainText text found in the language file. Using default: " + encoreAvailableMainText.text);
				}

				//Encore available Button //Umm, id dont know, need to translate "Ok" button, but... OK
				GameObject encoreAvailableButtonTextObject = encoreAvailableObject.FindPath("General (1)", "Text");
				TextMeshProUGUI encoreAvailableButtonText = encoreAvailableButtonTextObject.TMP();
				if (LanguageManager.CurrentLanguage.frontend.encoreAvailableButton != "")
				{
					encoreAvailableButtonText.text = LanguageManager.CurrentLanguage.frontend.encoreAvailableButton;
				}
				else
				{
					Logging.Warn("No encoreAvailableButton text found in the language file. Using default: " + encoreAvailableButtonText.text);
				}

			}
			catch (Exception e)
			{
				Logging.Error("An error occured while patching PopUp's. Check the console for details.");
				Logging.Error(e.ToString());
			}
		}


		/*public static void ChangeTitle(GameObject mainMenu) // This feature is currently disabled because there is a more global texture replacement feature (＞﹏＜)
		{
			try
			{
				Logging.Warn("Attempting to change the main menu's title image");
				GameObject trueMainMenu = GetGameObjectChild(GetGameObjectChild(mainMenu, "Main Menu (1)"), "LeftSide");
				GameObject titleObject = GetGameObjectChild(trueMainMenu, "Title");
				GameObject titleObjectArabic = null;
				string currentLangName = LanguageManager.CurrentLanguage.metadata.langName;
				bool usingArabicLogo = false;

				if (currentLangName.Substring(currentLangName.Length - 2).ToUpper() == "AR" && Core.ArabicUltrakillLogo != null)
				{
					if (titleObjectArabic == null)
					{
						GameObject.Instantiate(titleObject, titleObject.transform.position, Quaternion.identity, trueMainMenu.transform);
					}
					titleObjectArabic = GetGameObjectChild(trueMainMenu, "Title(Clone)");
					titleObjectArabic.GetComponent<Image>().sprite = Core.ArabicUltrakillLogo;
					usingArabicLogo = true;
				}
				else
				{
					usingArabicLogo = false;
				}
				if (titleObjectArabic != null)
				{
					if (usingArabicLogo)
					{
						trueMainMenu.GetComponent<ObjectActivateInSequence>().objectsToActivate[0] = titleObjectArabic;
						titleObject.SetActive(false);
						titleObjectArabic.SetActive(true);
					}
					else
					{
						trueMainMenu.GetComponent<ObjectActivateInSequence>().objectsToActivate[0] = titleObject;
						titleObject.SetActive(true);
						titleObjectArabic.SetActive(false);
					}
				}
			}
			catch (Exception e)
			{
				Logging.Error("An error occured while switching the title. Check the console for details.");
				Logging.Error(e.ToString());
			}
		}*/

		//Patches all text strings in the difficulty selection menu.
		private static void PatchDifficultyMenu(GameObject frontEnd)
		{
			try
			{
				GameObject difficultyObject = frontEnd.FindPath("Difficulty Select (1)", "Interactables");

				//Difficulty header text (note: this can't fit much without reducing the default font size.)
				TextMeshProUGUI difficultyText = difficultyObject.Child("Title").TMP();
				difficultyText.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_title + "--";

				//Easy header text
				GameObject easyObject = difficultyObject.Child("Easy");
				TextMeshProUGUI easyText = easyObject.TMP();
				easyText.text =LanguageManager.CurrentLanguage.frontend.difficulty_easy;

				//Normal header text
				TextMeshProUGUI normalText = difficultyObject.Child("Normal").TMP();
				normalText.text = LanguageManager.CurrentLanguage.frontend.difficulty_normal;

				//Hard header text
				TextMeshProUGUI hardText = difficultyObject.Child("Hard").TMP();
				hardText.text = LanguageManager.CurrentLanguage.frontend.difficulty_hard;

				//Harmless header
				GameObject harmlessTextObject = difficultyObject.Child("Casual Easy");
				TextMeshProUGUI harmlessText = harmlessTextObject.Child("Name").TMP();
				harmlessText.text = LanguageManager.CurrentLanguage.frontend.difficulty_harmless;

				//Lenient header
				GameObject lenientTextObject = difficultyObject.Child("Casual Hard");
				TextMeshProUGUI lenientText = lenientTextObject.Child("Name").TMP();
				lenientText.text = LanguageManager.CurrentLanguage.frontend.difficulty_lenient;

				//Standard header
				GameObject standardTextObject = difficultyObject.Child("Standard");
				TextMeshProUGUI standardText = standardTextObject.Child("Name").TMP();
				standardText.text = LanguageManager.CurrentLanguage.frontend.difficulty_standard + " <color=orange>*</color>";

				//Violent header
				GameObject violentTextObject = difficultyObject.Child("Violent");
				TextMeshProUGUI violentText = violentTextObject.Child("Name").TMP();
				violentText.text = LanguageManager.CurrentLanguage.frontend.difficulty_violent;

				//Brutal header
				GameObject brutalTextObject = difficultyObject.Child("Brutal");
				TextMeshProUGUI brutalText = brutalTextObject.Child("Name").TMP();
				brutalText.text = LanguageManager.CurrentLanguage.frontend.difficulty_brutal;

						//UKMD header
						GameObject umdTextObject = difficultyObject.Child("V1 Must Die");
						TextMeshProUGUI umdText = umdTextObject.Child("Name").TMP();
						umdText.text = LanguageManager.CurrentLanguage.frontend.difficulty_umd;


				TextMeshProUGUI underConstructionText = umdTextObject.Child("Under Construction").TMP();
				underConstructionText.text = LanguageManager.CurrentLanguage.frontend.difficulty_underConstruction;


				//No need for UMD header yet as it's not in-game

				if (LanguageManager.IsRightToLeft)
				{
					RtlFixDifficultyButton(brutalTextObject, brutalText);
					RtlFixDifficultyButton(violentTextObject, violentText);
					RtlFixDifficultyButton(standardTextObject, standardText);
					RtlFixDifficultyButton(lenientTextObject, lenientText);
					RtlFixDifficultyButton(harmlessTextObject, harmlessText);
				}

				//Tooltip
				GameObject assistTip = difficultyObject.Child("Assist Tip");
				TextMeshProUGUI assistTipText = assistTip.TMP();
				assistTipText.text = LanguageManager.CurrentLanguage.frontend.difficulty_tweakReminder;
			}
			catch (Exception e)
			{
				Logging.Error("Failed to patch difficulty menu.");
				Logging.Error(e.ToString());
			}
		}

		//Same as above.
		private static void PatchDifficultyDescriptors(GameObject frontEnd)
		{
			try
			{
				GameObject difficultyObject = frontEnd.FindPath("Difficulty Select (1)", "Interactables");

				//Harmless title
				GameObject harmlessObject = difficultyObject.Child("Harmless Info");
				TextMeshProUGUI harmlessTitle = harmlessObject.Child("Title (1)").TMP();
				harmlessTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_harmless + "--";

				//Harmless descriptor
				TextMeshProUGUI harmlessDescriptor = harmlessObject.Child("Text").TMP();
				harmlessDescriptor.text =
					LanguageManager.CurrentLanguage.frontend.difficulty_harmlessDescription1
					+ "\n\n"
					+ LanguageManager.CurrentLanguage.frontend.difficulty_harmlessDescription2
					+ "\n\n"
					+ "<color=green>" + LanguageManager.CurrentLanguage.frontend.difficulty_harmlessDescription3 + "</color>";

				//Lenient title
				GameObject lenientObject = difficultyObject.Child("Lenient Info");
				TextMeshProUGUI lenientTitle = lenientObject.Child("Title (1)").TMP();
				lenientTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_lenient + "--";

				//Lenient descriptor
				TextMeshProUGUI lenientDescriptor = lenientObject.Child("Text").TMP();
				lenientDescriptor.text =
					LanguageManager.CurrentLanguage.frontend.difficulty_lenientDescription1
					+ "\n\n"
					+ LanguageManager.CurrentLanguage.frontend.difficulty_lenientDescription2
					+ "\n\n"
					+ "<color=yellow>" + LanguageManager.CurrentLanguage.frontend.difficulty_lenientDescription3 + "</color>";

				//Standard title
				GameObject standardObject = difficultyObject.Child("Standard Info");
				TextMeshProUGUI standardTitle = standardObject.Child("Title (1)").TMP();
				standardTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_standard + "--";

				//Standard descriptor
				TextMeshProUGUI standardDescriptor = standardObject.Child("Text").TMP();
				standardDescriptor.text =
					LanguageManager.CurrentLanguage.frontend.difficulty_standardDescription1
					+ "\n\n"
					+ LanguageManager.CurrentLanguage.frontend.difficulty_standardDescription2
					+ "\n\n"
					+ "<color=orange>" + LanguageManager.CurrentLanguage.frontend.difficulty_standardDescription3 + "</color>";

				//Violent title
				GameObject violentObject = difficultyObject.Child("Violent Info");
				TextMeshProUGUI violentTitle = violentObject.Child("Title (1)").TMP();
				violentTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_violent + "--";

				//Violent descriptor
				TextMeshProUGUI violentDescriptor = violentObject.Child("Text").TMP();
				violentDescriptor.text =
					LanguageManager.CurrentLanguage.frontend.difficulty_violentDescription1
					+ "\n\n"
					+ LanguageManager.CurrentLanguage.frontend.difficulty_violentDescription2
					+ "\n\n"
					+ "<color=red>" + LanguageManager.CurrentLanguage.frontend.difficulty_violentDescription3 + "</color>";

				//Brutal title
				GameObject brutalObject = difficultyObject.Child("Brutal Info");
				TextMeshProUGUI brutalTitle = brutalObject.Child("Title (1)").TMP();
				brutalTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_brutal + "--";

				//Brutal descriptor
				TextMeshProUGUI brutalDescriptor = brutalObject.Child("Text").TMP();
				brutalDescriptor.text =
					"<color=white>" + LanguageManager.CurrentLanguage.frontend.difficulty_brutalDescription1
					+ "\n\n"
					+ LanguageManager.CurrentLanguage.frontend.difficulty_brutalDescription2 + "</color>"
					+ "\n\n"
					+ "<b>" + LanguageManager.CurrentLanguage.frontend.difficulty_brutalDescription3 + "<b>";
				// RTL
				if (LanguageManager.IsRightToLeft)
				{
					harmlessDescriptor.alignment = TextAlignmentOptions.TopRight;
					harmlessTitle.alignment = TextAlignmentOptions.MidlineRight;
					lenientDescriptor.alignment = TextAlignmentOptions.TopRight;
					lenientTitle.alignment = TextAlignmentOptions.MidlineRight;
					standardDescriptor.alignment = TextAlignmentOptions.TopRight;
					standardTitle.alignment = TextAlignmentOptions.MidlineRight;
					violentDescriptor.alignment = TextAlignmentOptions.TopRight;
					violentTitle.alignment = TextAlignmentOptions.MidlineRight;
				}

				//UMD stuff isn't in-game yet so the below is commmented out until the devs add them.

				/*UMD title - not in-game yet
				GameObject umdObject = GetGameObjectChild(difficultyObject, "UMD Info");
				TextMeshProUGUI umdTitle = GetTextMeshProUGUI(umdObject.transform.Find("Title (1)").gameObject);
				umdTitle.text = LanguageManager.CurrentLanguage.frontend.difficulty_umd;

				//UMD descriptor - not in-game yet
				TextMeshProUGUI brutalDescriptor = GetTextMeshProUGUI(umdObject.transform.Find("Text").gameObject);
				umdDescriptor.text = 
					LanguageManager.CurrentLanguage.frontend.difficulty_umdDescription1
					+ "\n\n"
					+ LanguageManager.CurrentLanguage.frontend.difficulty_umdDescription2
					+ "\n\n"
					+ "<color=red>" + LanguageManager.CurrentLanguage.frontend.difficulty_umdDescription3 + "</color>";
				*/

			}
			catch (Exception e)
			{
				Logging.Error("Failed to patch difficulty text.");
				Logging.Error(e.ToString());
			}

		}

		public static void RtlFixActButton(GameObject obj, TextMeshProUGUI txt)
		{
			RectTransform rect = txt.rectTransform;
			if (rect != null)
			{
				rect.anchorMax = new Vector2(1.0f, 0.5f);
				rect.anchorMin = new Vector2(1.0f, 0.5f);
				rect.anchoredPosition = new Vector3(-388f, 0f, 0f);
			}

			GameObject act1RankIcon = obj.transform.Find("RankPanel").gameObject;
			if (act1RankIcon != null)
			{
				Image rankImage = act1RankIcon.GetComponent<Image>();
				if (rankImage != null)
				{
					RectTransform rankRect = rankImage.rectTransform;
					rankRect.anchorMin = new Vector2(0.00f, 0.50f);
					rankRect.anchorMax = new Vector2(0.00f, 0.50f);
					rankRect.anchoredPosition = new Vector3(43f, 0f, 0f);
				}
			}
		}

		public static void RtlFixDifficultyButton(GameObject obj, TextMeshProUGUI txt)
		{
			RectTransform rect = txt.rectTransform;
			if (rect != null)
			{
				rect.anchorMax = new Vector2(1.0f, 0.5f);
				rect.anchorMin = new Vector2(1.0f, 0.5f);
				rect.anchoredPosition = new Vector3(-388f, 0f, 0f);
			}

			GameObject act1RankIcon = obj.transform.Find("RankPanel").gameObject;
			if (act1RankIcon != null)
			{
				Image rankImage = act1RankIcon.GetComponent<Image>();
				if (rankImage != null)
				{
					RectTransform rankRect = rankImage.rectTransform;
					if (rankRect != null)
					{
						rankRect.anchorMin = new Vector2(0.00f, 0.50f);
						rankRect.anchorMax = new Vector2(0.00f, 0.50f);
						rankRect.anchoredPosition = new Vector3(43f, 0f, 0f);
					}
				}
			}

			GameObject progressObject = obj.transform.Find("Progress").gameObject;
			if (progressObject != null)
			{
				TextMeshProUGUI progress = progressObject.GetComponent<TextMeshProUGUI>();
				if (progress != null)
				{
					progress.alignment = TextAlignmentOptions.MidlineLeft;
					RectTransform rectTrans = progress.rectTransform;
					if (rectTrans != null)
					{
						rectTrans.anchorMin = new Vector2(0.0f, 0.5f);
						rectTrans.anchorMax = new Vector2(0.0f, 0.5f);
						GameObject rankPanel = obj.transform.Find("RankPanel")?.gameObject;
						RectTransform parentRect = obj.GetComponent<RectTransform>();
						float minDistance = -70f; // Минимальный отступ между Rank и Progress
						float progressWidth = rectTrans.rect.width;
						float newX = 0f;

						if (rankPanel != null)
						{
							RectTransform rankRect = rankPanel.GetComponent<RectTransform>();
							if (rankRect != null)
							{
								// Вычисляем правый край RankPanel с учетом его позиции и ширины
								float rankFullRight = rankRect.anchoredPosition.x + (rankRect.rect.width * 1.0f);
								// Ставим Progress справа от RankPanel (учитываем, что anchor слева)
								newX = rankFullRight + minDistance;
							}
						}

						// Проверка на выход за пределы родителя
						if (parentRect != null)
						{
							float parentRight = parentRect.rect.width;
							float maxX = parentRight - progressWidth - minDistance;
							if (newX > maxX)
							{
								newX = maxX;
							}
						}

						rectTrans.anchoredPosition = new Vector2(newX, 0.0f);
					}
				}
				else
				{
				}
			}

		}

		private static void PatchChapterSelect(GameObject frontEnd)
		{
			GameObject chapterObject = frontEnd.FindPath("Chapter Select", "Chapters");
			TextMeshProUGUI chapterText = frontEnd.FindPath("Chapter Select", "Title (1)").TMP();
			chapterText.text = "--" + LanguageManager.CurrentLanguage.frontend.chapter_title + "--";

			//Start patching the Primary and Secondary chapters type titles
			GameObject primaryObject = chapterObject.FindPath("Primary", "Title");
			TextMeshProUGUI primaryText = primaryObject.TMP();
			primaryText.text = LanguageManager.CurrentLanguage.frontend.chapter_type_primary;

			GameObject secondaryObject = chapterObject.FindPath("Secondary", "Title");
			TextMeshProUGUI secondaryText = secondaryObject.TMP();
			secondaryText.text = LanguageManager.CurrentLanguage.frontend.chapter_type_secondary;
			// End patching the Primary and Secondary chapters type titles

			GameObject preludeObject = chapterObject.Child("Prelude");
			TextMeshProUGUI preludeText = preludeObject.Child("Name").TMP();
			preludeText.text = LanguageManager.CurrentLanguage.frontend.chapter_prelude;

			GameObject act1Object = chapterObject.Child("Act I");
			TextMeshProUGUI act1Text = act1Object.Child("Name").TMP();
			act1Text.text = LanguageManager.CurrentLanguage.frontend.chapter_act1;

			GameObject act2Object = chapterObject.Child("Act II");
			TextMeshProUGUI act2Text = act2Object.Child("Name").TMP();
			act2Text.text = LanguageManager.CurrentLanguage.frontend.chapter_act2;

			GameObject act3Object = chapterObject.Child("Act III");
			TextMeshProUGUI act3Text = act3Object.Child("Name").TMP();
			act3Text.text = LanguageManager.CurrentLanguage.frontend.chapter_act3;

			GameObject encoreObject = chapterObject.Child("Encore");
			TextMeshProUGUI encoreText = encoreObject.Child("Name").TMP();
			encoreText.text = LanguageManager.CurrentLanguage.frontend.chapter_encore;


			GameObject primeObject = chapterObject.Child("Prime");
			TextMeshProUGUI primeText = primeObject.Child("Name").TMP();
			primeText.text = LanguageManager.CurrentLanguage.frontend.chapter_prime;

			GameObject cgObject = chapterObject.Child("The Cyber Grind");
			TextMeshProUGUI cgText = cgObject.Child("Name").TMP();
			cgText.text = LanguageManager.CurrentLanguage.frontend.chapter_cyberGrind;

			GameObject sandboxObject = chapterObject.Child("Sandbox");
			TextMeshProUGUI sandboxText = sandboxObject.Child("Name").TMP();
			sandboxText.text = LanguageManager.CurrentLanguage.frontend.chapter_sandbox;

			if (LanguageManager.IsRightToLeft)
			{
				RtlFixActButton(preludeObject, preludeText);
				RtlFixActButton(act1Object, act1Text);
				RtlFixActButton(act2Object, act2Text);
				RtlFixActButton(act3Object, act3Text);
				RtlFixActButton(encoreObject, encoreText);
				RtlFixActButton(sandboxObject, sandboxText);
				RtlFixActButton(cgObject, cgText);
				RtlFixActButton(primeObject, primeText);
			}
		}

		private static void PatchLevelSelectPrelude(GameObject frontEnd)
		{
			GameObject lsPreludeObject = frontEnd.Child("Level Select (Prelude)");
			
			GameObject preludeHeader = lsPreludeObject.FindPath("Overture", "Header");

			//Prelude title
			TextMeshProUGUI preludeTitleText = preludeHeader.Child("Text").TMP();
			preludeTitleText.text = LanguageManager.CurrentLanguage.frontend.layer_prelude;
			preludeTitleText.fontSize = 46;

			//Prelude secret mission title
			TextMeshProUGUI secretText = preludeHeader.FindPath("Secret Mission", "Text").TMP();
			secretText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
			

			GameObject preludeObject = lsPreludeObject.FindPath("Overture", "Level Row");
			
			//0-1 challenge
			GameObject firstObject = preludeObject.Child("0-1 Panel");
			TextMeshProUGUI firstChallenge = firstObject.FindPath("Panel", "Text").TMP();
			firstChallenge.text = PreludeStrings.GetLevelChallenge("Level 0-1");

			//0-2 challenge
			GameObject secondObject = preludeObject.Child("0-2 Panel");
			TextMeshProUGUI secondChallenge = secondObject.FindPath("Panel (2)", "Text").TMP();
			secondChallenge.text = PreludeStrings.GetLevelChallenge("Level 0-2");

			//0-3 challenge
			GameObject thirdObject = preludeObject.Child("0-3 Panel");
			TextMeshProUGUI thirdChallenge = thirdObject.FindPath("Panel (4)", "Text").TMP();
			thirdChallenge.text = PreludeStrings.GetLevelChallenge("Level 0-3");

			//0-4 challenge
			GameObject fourthObject = preludeObject.Child("0-4 Panel");
			TextMeshProUGUI fourthChallenge = fourthObject.FindPath("Panel (6)", "Text").TMP();
			fourthChallenge.text = PreludeStrings.GetLevelChallenge("Level 0-4");

			//0-5 challenge
			GameObject fifthObject = preludeObject.Child("0-5 Panel");

			TextMeshProUGUI fifthChallenge = fifthObject.FindPath("Panel (6)", "Text").TMP();
			fifthChallenge.text = PreludeStrings.GetLevelChallenge("Level 0-5");
			
			//Full intro panel why this is not using the TMPro
			GameObject fullIntroObject = lsPreludeObject.FindPath("FullIntroPopup", "Panel");

			Text fullIntroText = fullIntroObject.Child("Text").Text();
			fullIntroText.text = LanguageManager.CurrentLanguage.frontend.level_fullIntroPrompt;

			UnityEngine.UI.Text fullIntroYesText = fullIntroObject.FindPath("Button (1)", "Text").Text();
			fullIntroYesText.text = LanguageManager.CurrentLanguage.frontend.level_fullIntroPromptYes;

			Text fullIntroNoText = fullIntroObject.FindPath("Button", "Text").Text();
			fullIntroNoText.text = LanguageManager.CurrentLanguage.frontend.level_fullIntroPromptNo;

			Text fullIntroCancelText = fullIntroObject.FindPath("Button (2)", "Text").Text();
			fullIntroCancelText.text = LanguageManager.CurrentLanguage.frontend.level_fullIntroPromptCancel;
		}

		//Patches all text strings in the Act 1 menu.
		private static void PatchLevelSelectAct1(GameObject frontEnd)
		{
			GameObject act1Object = frontEnd.FindPath("Level Select (Act I)", "Scroll Rect", "Contents");

			GameObject limboObject = act1Object.Child("Layer 1 Limbo");
			GameObject lustObject = act1Object.Child("Layer 2 Lust");
			GameObject gluttonyObject = act1Object.Child("Layer 3 Gluttony");

			//Layer 1 - Limbo
			GameObject limboHeader = limboObject.Child("Header");

			TextMeshProUGUI limboTitle = limboHeader.Child("Text").TMP();
			limboTitle.text = LanguageManager.CurrentLanguage.frontend.layer_limbo;

			TextMeshProUGUI limboSecretMissionText = limboHeader.FindPath("Secret Mission", "Text").TMP();
			limboSecretMissionText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
	
			//Main levels
			GameObject limboContent = limboObject.Child("Level Row");

			TextMeshProUGUI limboFirstChallenge = limboContent.FindPath("1-1 Panel", "Panel", "Text").TMP();
			limboFirstChallenge.text = Act1Strings.GetLevelChallenge("Level 1-1");

			TextMeshProUGUI limboSecondChallenge = limboContent.FindPath("1-2 Panel", "Panel (2)", "Text").TMP();
			limboSecondChallenge.text = Act1Strings.GetLevelChallenge("Level 1-2");

			TextMeshProUGUI limboThirdChallenge = limboContent.FindPath("1-3 Panel", "Panel (4)", "Text").TMP();
			limboThirdChallenge.text = Act1Strings.GetLevelChallenge("Level 1-3");

			TextMeshProUGUI limboClimaxChallenge = limboContent.FindPath("1-4 Panel", "Panel (6)", "Text").TMP();
			limboClimaxChallenge.text = Act1Strings.GetLevelChallenge("Level 1-4");

			//Layer 2 - Lust
			GameObject lustHeader = lustObject.Child("Header");

			TextMeshProUGUI lustTitle = lustHeader.Child("Text").TMP();
			lustTitle.text = LanguageManager.CurrentLanguage.frontend.layer_lust;

			TextMeshProUGUI lustSecretMissionText = lustHeader.FindPath("Secret Mission", "Text").TMP();
			lustSecretMissionText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
			
			GameObject lustContent = lustObject.Child("Level Row");

			//Main levels
			TextMeshProUGUI lustFirstChallenge = lustContent.FindPath("2-1 Panel", "Panel", "Text").TMP();
			lustFirstChallenge.text = Act1Strings.GetLevelChallenge("Level 2-1");

			TextMeshProUGUI lustSecondChallenge = lustContent.FindPath("2-2 Panel", "Panel (2)", "Text").TMP();
			lustSecondChallenge.text = Act1Strings.GetLevelChallenge("Level 2-2");

			TextMeshProUGUI lustThirdChallenge = lustContent.FindPath("2-3 Panel", "Panel (4)", "Text").TMP();
			lustThirdChallenge.text = Act1Strings.GetLevelChallenge("Level 2-3");

			TextMeshProUGUI lustClimaxChallenge = lustContent.FindPath("2-4 Panel", "Panel (6)", "Text").TMP();
			lustClimaxChallenge.text = Act1Strings.GetLevelChallenge("Level 2-4");

			//Layer 3 - Gluttony
			GameObject gluttonyHeader = gluttonyObject.Child("Header");

			TextMeshProUGUI gluttonyTitle = gluttonyHeader.Child("Text").TMP();
			gluttonyTitle.text = LanguageManager.CurrentLanguage.frontend.layer_gluttony;
			
			//Main levels
			GameObject gluttonyContent = gluttonyObject.Child("Level Row");

			TextMeshProUGUI gluttonyFirstChallenge = gluttonyContent.FindPath("3-1 Panel", "Panel", "Text").TMP();
			gluttonyFirstChallenge.text = Act1Strings.GetLevelChallenge("Level 3-1");

			TextMeshProUGUI gluttonySecondChallenge = gluttonyContent.FindPath("3-2 Panel", "Panel (2)", "Text").TMP();
			gluttonySecondChallenge.text = Act1Strings.GetLevelChallenge("Level 3-2");

		}

		private static void PatchLevelSelectAct2(GameObject frontEnd)
		{
			GameObject act2Object = frontEnd.FindPath("Level Select (Act II)", "Scroll Rect", "Contents");

			GameObject greedObject = act2Object.Child("Layer 4 Greed");
			GameObject wrathObject = act2Object.Child("Layer 5 Wrath");
			GameObject heresyObject = act2Object.Child("Layer 6 Heresy");

			//Layer 4 - Greed
			GameObject greedHeader = greedObject.Child("Header");

			TextMeshProUGUI greedTitle = greedHeader.Child("Text").TMP();
			greedTitle.text = LanguageManager.CurrentLanguage.frontend.layer_greed;

			TextMeshProUGUI greedSecretMissionText = greedHeader.FindPath("Secret Mission", "Text").TMP();
			greedSecretMissionText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
			
			//Main levels
			GameObject greedContent = greedObject.Child("Level Row");


			TextMeshProUGUI greedFirstChallenge = greedContent.FindPath("4-1 Panel", "Panel", "Text").TMP();
			greedFirstChallenge.text = Act2Strings.GetLevelChallenge("Level 4-1");

			TextMeshProUGUI greedSecondChallenge = greedContent.FindPath("4-2 Panel", "Panel (2)", "Text").TMP();
			greedSecondChallenge.text = Act2Strings.GetLevelChallenge("Level 4-2");

			TextMeshProUGUI greedThirdChallenge = greedContent.FindPath("4-3 Panel", "Panel (4)", "Text").TMP();
			greedThirdChallenge.text = Act2Strings.GetLevelChallenge("Level 4-3");

			TextMeshProUGUI greedClimaxChallenge = greedContent.FindPath("4-4 Panel", "Panel (6)", "Text").TMP();
			greedClimaxChallenge.text = Act2Strings.GetLevelChallenge("Level 4-4");

			
			//Layer 5 - Wrath
			GameObject wrathHeader =  wrathObject.Child("Header");

			TextMeshProUGUI wrathTitle = wrathHeader.Child("Text").TMP();
			wrathTitle.text = LanguageManager.CurrentLanguage.frontend.layer_wrath;

			TextMeshProUGUI wrathSecretMissionText = wrathHeader.FindPath("Secret Mission", "Text").TMP();
			wrathSecretMissionText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
			
			//Main levels
			GameObject wrathContent = wrathObject.Child("Level Row");

			TextMeshProUGUI wrathFirstChallenge = wrathContent.FindPath("5-1 Panel", "Panel", "Text").TMP();
			wrathFirstChallenge.text = Act2Strings.GetLevelChallenge("Level 5-1");

			TextMeshProUGUI wrathSecondChallenge = wrathContent.FindPath("5-2 Panel", "Panel (2)", "Text").TMP();
			wrathSecondChallenge.text = Act2Strings.GetLevelChallenge("Level 5-2");

			TextMeshProUGUI wrathThirdChallenge = wrathContent.FindPath("5-3 Panel", "Panel (4)", "Text").TMP();
			wrathThirdChallenge.text = Act2Strings.GetLevelChallenge("Level 5-3");

			TextMeshProUGUI wrathFourthChallenge = wrathContent.FindPath("5-4 Panel", "Panel (6)", "Text").TMP();
			wrathFourthChallenge.text = Act2Strings.GetLevelChallenge("Level 5-4");


			//Layer 6 - Heresy
			GameObject heresyHeader = heresyObject.Child("Header");

			TextMeshProUGUI heresyTitle = heresyHeader.Child("Text").TMP();
			heresyTitle.text = LanguageManager.CurrentLanguage.frontend.layer_heresy;
			
			//Main levels
			GameObject heresyContent = heresyObject.Child("Level Row");


			TextMeshProUGUI heresyFirstChallenge = heresyContent.FindPath("1-1 Panel", "Panel", "Text").TMP();
			heresyFirstChallenge.text = Act2Strings.GetLevelChallenge("Level 6-1");

			TextMeshProUGUI heresySecondChallenge = heresyContent.FindPath("1-2 Panel", "Panel (2)", "Text").TMP();
			heresySecondChallenge.text = Act2Strings.GetLevelChallenge("Level 6-2");
		}
		
		private static void PatchLevelSelectAct3(GameObject frontEnd)
		{
			GameObject act3Object = frontEnd.FindPath("Level Select (Act III)", "Scroll Rect", "Contents");

			GameObject violenceObject = act3Object.Child("Layer 7 Violence");
			GameObject fraudObject = act3Object.Child("Layer 8 Fraud");
			GameObject treacheryObject = act3Object.Child("Layer 9 Treachery");

			//Layer 7 - Violence
			GameObject violenceHeader = violenceObject.Child("Header");

			TextMeshProUGUI violenceTitle = violenceHeader.Child("Text").TMP();
			violenceTitle.text = LanguageManager.CurrentLanguage.frontend.layer_violence;

			TextMeshProUGUI violenceSecretMissionText = violenceHeader.FindPath("Secret Mission", "Text").TMP();
			violenceSecretMissionText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
			
			//Main levels
			GameObject violenceContent = violenceObject.Child("Level Row");


			TextMeshProUGUI violenceFirstChallenge = violenceContent.FindPath("7-1 Panel", "Panel", "Text").TMP();
			violenceFirstChallenge.text = Act3Strings.GetLevelChallenge("Level 7-1");

			TextMeshProUGUI violenceSecondChallenge = violenceContent.FindPath("7-2 Panel", "Panel (2)", "Text").TMP();
			violenceSecondChallenge.text = Act3Strings.GetLevelChallenge("Level 7-2");

			TextMeshProUGUI violenceThirdChallenge = violenceContent.FindPath("7-3 Panel", "Panel (4)", "Text").TMP();
			violenceThirdChallenge.text = Act3Strings.GetLevelChallenge("Level 7-3");

			TextMeshProUGUI violenceClimaxChallenge = violenceContent.FindPath("7-4 Panel", "Panel (6)", "Text").TMP();
			violenceClimaxChallenge.text = Act3Strings.GetLevelChallenge("Level 7-4");

			
			//Layer 8 - Fraud
			GameObject fraudHeader = fraudObject.Child("Header");

			TextMeshProUGUI fraudTitle = fraudHeader.Child("Text").TMP();
			fraudTitle.text = LanguageManager.CurrentLanguage.frontend.layer_fraud;

			TextMeshProUGUI fraudSecretMissionText = fraudHeader.FindPath("Secret Mission", "Text").TMP();
			fraudSecretMissionText.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
			
			//Main levels
			GameObject fraudContent = fraudObject.Child("Level Row");


			TextMeshProUGUI fraudFirstChallenge = fraudContent.FindPath("8-1 Panel", "Panel", "Text").TMP();
			fraudFirstChallenge.text = Act3Strings.GetLevelChallenge("Level 8-1");

			TextMeshProUGUI fraudSecondChallenge = fraudContent.FindPath("8-2 Panel", "Panel (2)", "Text").TMP();
			fraudSecondChallenge.text = Act3Strings.GetLevelChallenge("Level 8-2");

			TextMeshProUGUI fraudThirdChallenge = fraudContent.FindPath("8-3 Panel", "Panel (4)", "Text").TMP();
			fraudThirdChallenge.text = Act3Strings.GetLevelChallenge("Level 8-3");

			TextMeshProUGUI fraudClimaxChallenge = fraudContent.FindPath("8-4 Panel", "Panel (6)", "Text").TMP();
			fraudClimaxChallenge.text = Act3Strings.GetLevelChallenge("Level 8-4");


			//Layer 9 - Treachery
			GameObject treacheryHeader = treacheryObject.Child("Header");

			TextMeshProUGUI treacheryTitle = treacheryHeader.Child("Text").TMP();
			treacheryTitle.text = LanguageManager.CurrentLanguage.frontend.layer_treachery;
			
			//Main levels
			GameObject treacheryContent = treacheryObject.Child("Level Row");


			TextMeshProUGUI treacheryFirstChallenge = treacheryContent.FindPath("9-1 Panel", "Panel", "Text").TMP();
			treacheryFirstChallenge.text = Act3Strings.GetLevelChallenge("Level 9-1");

			TextMeshProUGUI treacherySecondChallenge = treacheryContent.FindPath("9-2 Panel", "Panel (2)", "Text").TMP();
			treacherySecondChallenge.text = Act3Strings.GetLevelChallenge("Level 9-2");
		}

		private static void PatchLevelSelectEncore(GameObject frontEnd)
		{
			GameObject lsEncoreObject = frontEnd.FindPath("Level Select (Encore)", "Scroll Rect", "Contents");

			GameObject encoreHeader = lsEncoreObject.FindPath("Encores", "Header");

			//Encore title
			TextMeshProUGUI preludeTitleText = encoreHeader.Child("Text").TMP();
			preludeTitleText.text = LanguageManager.CurrentLanguage.frontend.chapter_encore;
			preludeTitleText.fontSize = 46;
			
		}

		private static void PatchLevelSelectPrime(GameObject frontEnd)
		{
			GameObject primeObject = frontEnd.FindPath("Level Select (Prime)", "Prime Sanctums", "Header");
			TextMeshProUGUI primeTitle = primeObject.Child("Text").TMP();
			primeTitle.text = LanguageManager.CurrentLanguage.frontend.layer_prime;
		}
		private static void PatchTextArroundV1(GameObject mainMenu)
		{
			GameObject textV1 = mainMenu.FindPath("Main Menu (1)", "BackgroundSwapper", "Text (TMP)", "V1Text");
			TextMeshProUGUI wingModule = textV1.Child("Text (TMP)").TMP();
			TextMeshProUGUI armModuleFactory = textV1.Child("Text (TMP) (1)").TMP();
			TextMeshProUGUI armModuleFeedbacker = textV1.Child("Text (TMP) (2)").TMP();
			TextMeshProUGUI visualCortexModule = textV1.Child("Text (TMP) (3)").TMP();
			TextMeshProUGUI legModule = textV1.Child("Text (TMP) (4)").TMP();
			if (!string.IsNullOrEmpty(LanguageManager.CurrentLanguage.frontend.wingModule))
				wingModule.text = LanguageManager.CurrentLanguage.frontend.wingModule;
			if (!string.IsNullOrEmpty(LanguageManager.CurrentLanguage.frontend.armModuleFactory))
				armModuleFactory.text = LanguageManager.CurrentLanguage.frontend.armModuleFactory;
			if (!string.IsNullOrEmpty(LanguageManager.CurrentLanguage.frontend.armModuleFeedbacker))
				armModuleFeedbacker.text = LanguageManager.CurrentLanguage.frontend.armModuleFeedbacker;
			if (!string.IsNullOrEmpty(LanguageManager.CurrentLanguage.frontend.visualCortexModule))
				visualCortexModule.text = LanguageManager.CurrentLanguage.frontend.visualCortexModule;
			if (!string.IsNullOrEmpty(LanguageManager.CurrentLanguage.frontend.legModule))
				legModule.text = LanguageManager.CurrentLanguage.frontend.legModule;
		}

		public static void Patch(GameObject frontEnd)
		{
			try
			{
				PatchMainMenu(frontEnd);
				PatchTextArroundV1(frontEnd);
				PatchPopUps(frontEnd);
				//ChangeTitle(frontEnd);
				PatchDifficultyMenu(frontEnd);
				PatchDifficultyDescriptors(frontEnd);

				PatchChapterSelect(frontEnd);
				PatchLevelSelectPrelude(frontEnd);
				PatchLevelSelectAct1(frontEnd);
				PatchLevelSelectAct2(frontEnd);
				PatchLevelSelectAct3(frontEnd);
				PatchLevelSelectEncore(frontEnd);
				PatchLevelSelectPrime(frontEnd);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

		}

	}
}
