using UltrakULL.json;
using UnityEngine;

namespace UltrakULL
{
	public static class StringsParent
	{
		private static string fullMessage;

		/// <summary>
		/// Проверяет строку перевода: если null или пустая, логирует и возвращает оригинал.
		/// </summary>
		private static string EnsureTranslation(string translated, string original)
		{
			if (string.IsNullOrEmpty(translated))
			{
				Logging.Warn($"[StringsParent] Translation missing or empty, falling back to original: '{original}'");
				return original;
			}
			return translated;
		}

		/// <summary>
		/// Проверяет две части перевода и объединяет их разделителем.
		/// Если хотя бы одна часть null/пустая, возвращает оригинал.
		/// </summary>
		private static string EnsureTranslation(string part1, string part2, string separator, string original)
		{
			if (string.IsNullOrEmpty(part1) || string.IsNullOrEmpty(part2))
			{
				Logging.Warn($"[StringsParent] Translation part missing or empty, falling back to original: '{original}'");
				return original;
			}
			return part1 + separator + part2;
		}

		/// <summary>
		/// Проверяет три части перевода и объединяет их разделителями.
		/// Если хотя бы одна часть null/пустая, возвращает оригинал.
		/// </summary>
		private static string EnsureTranslation(string part1, string part2, string part3, string separator1, string separator2, string original)
		{
			if (string.IsNullOrEmpty(part1) || string.IsNullOrEmpty(part2) || string.IsNullOrEmpty(part3))
			{
				Logging.Warn($"[StringsParent] Translation part missing or empty, falling back to original: '{original}'");
				return original;
			}
			return part1 + separator1 + part2 + separator2 + part3;
		}

		public static string GetMessage(string message, string message2, string input)
		{
			string currentSceneName = CommonFunctions.GetCurrentSceneName();
			if (input != null && input.Length > 0)
			{
				input = CommonFunctions.GetLocalizedInput(input);
			}
			if (message.Contains("WARNING") || message.Contains("fall") || message.Contains("free"))
			{
				Logging.Warn("[StringsParent] Level: " + currentSceneName + " | message: '" + message + "' | message2: '" + message2 + "' | input: '" + input + "'");
			}
			if (message.Contains("versions"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_alternateVersion, message);
			}
			if (message.Contains("ALTERNATE NAILGUN"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.act2.act2_greedFourth_alternateNailgun, message);
			}
			if (currentSceneName.Contains("Tutorial"))
			{
				GameObject canvasObj = CommonFunctions.GetInactiveRootObject("Canvas");
				new TutorialStrings(ref canvasObj);
				fullMessage = TutorialStrings.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if (currentSceneName.Contains("0-") && !currentSceneName.Contains("0-E"))
			{
				fullMessage = PreludeStrings.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if ((currentSceneName.Contains("1-") && !currentSceneName.Contains("1-E")) || (currentSceneName.Contains("2-") && !currentSceneName.Contains("2-E")) || (currentSceneName.Contains("3-") && !currentSceneName.Contains("3-E")))
			{
				fullMessage = Act1Strings.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if ((currentSceneName.Contains("4-") && !currentSceneName.Contains("4-E")) || (currentSceneName.Contains("5-") && !currentSceneName.Contains("5-E")) || (currentSceneName.Contains("6-") && !currentSceneName.Contains("6-E")))
			{
				fullMessage = Act2Strings.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if ((currentSceneName.Contains("7-") && !currentSceneName.Contains("7-E")) || (currentSceneName.Contains("8-") && !currentSceneName.Contains("8-E")) || (currentSceneName.Contains("9-") && !currentSceneName.Contains("9-E")))
			{
				fullMessage = Act3Strings.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if (currentSceneName.Contains("CreditsMuseum2"))
			{
				fullMessage = DevMuseum.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if (currentSceneName.Contains("-E"))
			{
				fullMessage = EncoreStrings.GetMessage(message, message2, input);
				if (!fullMessage.Contains("Unimplemented string"))
				{
					return fullMessage;
				}
			}
			if (message.Contains("V-Rank"))
			{
				return message;
			}
			if (message.Contains("PUNCH"))
			{
				string part1 = LanguageManager.CurrentLanguage.misc.hud_noArm1;
				string part2 = LanguageManager.CurrentLanguage.misc.hud_noArm2;
				if (string.IsNullOrEmpty(part1) || string.IsNullOrEmpty(part2))
				{
					Logging.Warn($"[StringsParent] Translation missing or empty for PUNCH, falling back to original: '{message}'");
					return message;
				}
				return "<color=red>" + part1 + "</color>\n" + part2;
			}
			if (message.Contains("MAJOR"))
			{
				string translated = LanguageManager.CurrentLanguage.misc.hud_majorAssists;
				if (string.IsNullOrEmpty(translated))
				{
					Logging.Warn($"[StringsParent] Translation missing or empty for MAJOR, falling back to original: '{message}'");
					return message;
				}
				return "<color=#4C99E6>" + translated + "</color>";
			}
			if (message.Contains("200"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_overhealOrb1, LanguageManager.CurrentLanguage.misc.hud_overhealOrb2, "\n", message);
			}
			if (message.Contains("ERROR"))
			{
				string translated = LanguageManager.CurrentLanguage.misc.hud_itemGrabError;
				if (string.IsNullOrEmpty(translated))
				{
					Logging.Warn($"[StringsParent] Translation missing or empty for ERROR, falling back to original: '{message}'");
					return message;
				}
				return "<color=red>" + translated + "</color>";
			}
			if (message.Contains("TAB"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_levelStats1, LanguageManager.CurrentLanguage.misc.hud_levelStats2, "\n", message);
			}
			if (message.Contains("Whoops"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_outOfBounds, message);
			}
			if (message.Contains("CLASH"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_clashMode, message);
			}
			if (message.Contains("DRONE HAUNTING"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_droneHaunting, message);
			}
			if (message.Contains("EQUIPPED"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.misc.hud_weaponVariation, message);
			}
			if (message.Contains("Altered"))
			{
				string translated = LanguageManager.CurrentLanguage.misc.enemyAlter_alteredDestroyed;
				if (string.IsNullOrEmpty(translated))
				{
					Logging.Warn($"[StringsParent] Translation missing or empty for Altered, falling back to original: '{message}'");
					return message;
				}
				return "<color=red>" + translated + "</color>";
			}
			if (message.Contains("INSUFFICIENT LIGHT"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_first_insufficientlight, message);
			}
			if (message.Contains("=>"))
			{
				return message;
			}
            if (message.Contains("You have found a <color=orange>SECRET MISSION</color>."))
            {
                return EnsureTranslation(
                    LanguageManager.CurrentLanguage.misc.secretMissionFound,
                    message
                );
            }
            Logging.Warn("Unimplemented string in \"" + CommonFunctions.GetCurrentSceneName() + "\": " + message);
			return message;
		}

		public static string GetLevelTip(string tipDescriptionText)
		{
			string currentSceneName = CommonFunctions.GetCurrentSceneName();
			if (currentSceneName.Contains("0-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_preludeSecond1, LanguageManager.CurrentLanguage.levelTips.leveltips_preludeSecond2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("0-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_preludeThird1, LanguageManager.CurrentLanguage.levelTips.leveltips_preludeThird2, LanguageManager.CurrentLanguage.levelTips.leveltips_preludeThird3, "\n\n", "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("0-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_preludeFourth1, LanguageManager.CurrentLanguage.levelTips.leveltips_preludeFourth2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("0-5"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_preludeFifth, tipDescriptionText);
			}
			if (currentSceneName.Contains("1-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_limboFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("1-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_limboSecond, tipDescriptionText);
			}
			if (currentSceneName.Contains("1-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_limboThird1, LanguageManager.CurrentLanguage.levelTips.leveltips_limboThird2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("1-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_limboFourth, tipDescriptionText);
			}
			if (currentSceneName.Contains("2-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_lustFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("2-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_lustSecond1, LanguageManager.CurrentLanguage.levelTips.leveltips_lustSecond2, LanguageManager.CurrentLanguage.levelTips.leveltips_lustSecond3, " ", "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("2-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_lustThird, tipDescriptionText);
			}
			if (currentSceneName.Contains("2-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_lustFourth1, LanguageManager.CurrentLanguage.levelTips.leveltips_lustFourth2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("3-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_gluttonyFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("3-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_gluttonySecond1, LanguageManager.CurrentLanguage.levelTips.leveltips_gluttonySecond2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("4-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_greedFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("4-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_greedSecond, tipDescriptionText);
			}
			if (currentSceneName.Contains("4-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_greedThird, tipDescriptionText);
			}
			if (currentSceneName.Contains("4-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_greedFourth, tipDescriptionText);
			}
			if (currentSceneName.Contains("5-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_wrathFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("5-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_wrathSecond, tipDescriptionText);
			}
            if (currentSceneName.Contains("5-3"))
            {
                string trimmedTip = tipDescriptionText.Trim();
                string brokenTip = LanguageManager.CurrentLanguage.levelTips.leveltips_wrathThirdBroken;

                if (trimmedTip == "Ow." || trimmedTip == brokenTip)
                {
                    return EnsureTranslation(brokenTip, tipDescriptionText);
                }

                return EnsureTranslation(
                    LanguageManager.CurrentLanguage.levelTips.leveltips_wrathThird,
                    tipDescriptionText
                );
            }
            if (currentSceneName.Contains("5-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_wrathFourth1, LanguageManager.CurrentLanguage.levelTips.leveltips_wrathFourth2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("6-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_heresyFirst1, LanguageManager.CurrentLanguage.levelTips.leveltips_heresyFirst2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("6-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_heresySecond1, LanguageManager.CurrentLanguage.levelTips.leveltips_heresySecond2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("7-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_violenceFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("7-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_violenceSecond, tipDescriptionText);
			}
			if (currentSceneName.Contains("7-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_violenceThird, tipDescriptionText);
			}
			if (currentSceneName.Contains("7-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_violenceFourth, tipDescriptionText);
			}
			if (currentSceneName.Contains("7-S"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_violenceSecret, tipDescriptionText);
			}
			if (currentSceneName.Contains("8-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_fraudFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("8-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_fraudSecond, tipDescriptionText);
			}
			if (currentSceneName.Contains("8-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_fraudThird, tipDescriptionText);
			}
			if (currentSceneName.Contains("8-4"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_fraudFourth, tipDescriptionText);
			}
			if (currentSceneName.Contains("9-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_treacheryFirst, tipDescriptionText);
			}
			if (currentSceneName.Contains("9-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_treacherySecond, tipDescriptionText);
			}
			if (currentSceneName.Contains("uk_construct"))
			{
				string part1 = LanguageManager.CurrentLanguage.levelTips.leveltips_sandbox1;
				string part2 = LanguageManager.CurrentLanguage.levelTips.leveltips_sandbox2;
				string part3 = LanguageManager.CurrentLanguage.levelTips.leveltips_sandbox3;
				if (string.IsNullOrEmpty(part1) || string.IsNullOrEmpty(part2))
				{
					Logging.Warn($"[StringsParent] Translation missing or empty for uk_construct, falling back to original: '{tipDescriptionText}'");
					return tipDescriptionText;
				}
				return part1 + "\n\n<color=#FF4343>↑ ↑ ↓ ↓ ← → ← → " + part2 + "</color>\n\n" + part3;
			}
			if (currentSceneName.Contains("0-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encorePrelude1, LanguageManager.CurrentLanguage.levelTips.leveltips_encorePrelude2, "\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("1-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreLimbo, tipDescriptionText);
			}
			if (currentSceneName.Contains("2-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreLust, tipDescriptionText);
			}
			if (currentSceneName.Contains("3-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreGluttony, tipDescriptionText);
			}
			if (currentSceneName.Contains("4-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreGreed, tipDescriptionText);
			}
			if (currentSceneName.Contains("5-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreWrath, tipDescriptionText);
			}
			if (currentSceneName.Contains("6-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreHeresy, tipDescriptionText);
			}
			if (currentSceneName.Contains("7-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreViolence, tipDescriptionText);
			}
			if (currentSceneName.Contains("8-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreFraud, tipDescriptionText);
			}
			if (currentSceneName.Contains("9-E"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_encoreTreachery, tipDescriptionText);
			}
			if (currentSceneName.Contains("P-1"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_primeFirst1, LanguageManager.CurrentLanguage.levelTips.leveltips_primeFirst2, "\n\n\n", tipDescriptionText);
			}
			if (currentSceneName.Contains("P-2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_primeSecond, tipDescriptionText);
			}
			if (currentSceneName.Contains("P-3"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_primeThird, tipDescriptionText);
			}
			if (currentSceneName.Contains("Endless"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_cybergrind, tipDescriptionText);
			}
			if (currentSceneName.Contains("CreditsMuseum2"))
			{
				return EnsureTranslation(LanguageManager.CurrentLanguage.levelTips.leveltips_devMuseum, tipDescriptionText);
			}
			Logging.Warn("The source of the Level tip is not specified in the mod. We returned the value as it was. Level name: " + currentSceneName + ". tipDescriptionText= \"" + tipDescriptionText + "\"");
			return tipDescriptionText;
		}
	}
}
