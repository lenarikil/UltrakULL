using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
	public static class Act3Strings
	{
		public static string Level71(string message, string message2)
		{
			if ((message + message2).Contains("A door opens."))
			{
				return LanguageManager.CurrentLanguage.act3.act3_violenceFirst_doorOpens;
			}
			return "Unimplemented string";
		}

		public static string Level72(string message, string message2, string input)
		{
			string text = message + message2;
			if (text.Contains("Swap arms with"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_violenceSecond_guttermanTutorial1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.act3.act3_violenceSecond_guttermanTutorial2;
			}
			if (text.Contains("You should probably"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_violenceSecond_guttermanTutorialNoKB;
			}
			if (text.Contains("BIGGER BOOM"))
			{
				return "<color=red>" + LanguageManager.CurrentLanguage.act3.act3_violenceSecond_biggerBoom + "</color>";
			}
			if (message.Contains("versions"))
			{
				return LanguageManager.CurrentLanguage.misc.hud_alternateVersion;
			}
			if (text.Contains("ALTERNATE SHOTGUN"))
			{
				return "<color=orange>" + LanguageManager.CurrentLanguage.act3.act3_violenceSecond_alternateShotgun;
			}
			return "Unimplemented string";
		}

		public static string Level73(string message, string message2)
		{
			if ((message + message2).Contains("F E E D"))
			{
				return "<color=red>" + LanguageManager.CurrentLanguage.act3.act3_violenceThird_feedIt + "</color>";
			}
			return "Unimplemented string";
		}

		public static string Level74(string message, string message2)
		{
			if ((message + message2).Contains("MAGENTA"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_violenceFourth_magentaAttack;
			}
			return "Unimplemented string";
		}

		public static string Level7Secret(string message, string message2)
		{
			return "Unimplemented string";
		}

		public static string Level81(string message, string message2)
		{
		    string text = message + message2;
		    if (text.Contains("The cycle of life"))
		    {
		        return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_cycleOfLife;
		    }
		    if (text.Contains("It is happening again"))
		    {
		        return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_happeningAgain;
		    }
		    return "Unimplemented string";
		}

		public static string Level82(string message, string message2)
		{
			string text = message + message2;
			if (text.Contains("The cycle of life"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_cycleOfLife;
			}
            if (text.Contains("YOU'RE NOT SUPPOSED TO BE HERE."))
            {
                return LanguageManager.CurrentLanguage.act3.act3_secretNotReady;
            }
            if (text.Contains("It is happening again"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_happeningAgain;
			}
			return "Unimplemented string";
		}

		public static string Level83(string message, string message2)
		{
		    string text = message + message2;
		    if (text.Contains("The cycle of life"))
		    {
		        return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_cycleOfLife;
		    }
		    if (text.Contains("It is happening again"))
		    {
		        return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_happeningAgain;
		    }
		    return "Unimplemented string";
		}

		public static string Level84(string format = "", string message2 = "", string[] inputs = null)
		{
			string text = format + message2;
			if (text.Contains("WARNING:") && text.Contains("free fall"))
			{
				string text2 = (inputs != null && inputs.Length != 0) ? GetLocalizedInput(inputs[0]) : "";
				string text3 = (inputs != null && inputs.Length > 1) ? GetLocalizedInput(inputs[1]) : "";
				return LanguageManager.CurrentLanguage.act3.act3_fraudFourth_fallWarning_part1 + "\n" + LanguageManager.CurrentLanguage.act3.act3_fraudFourth_fallWarning_part2 + " <color=orange>" + text2 + "</color> " + LanguageManager.CurrentLanguage.act3.act3_fraudFourth_fallWarning_part3 + " <color=orange>" + text3 + "</color>.";
			}
			if (text.Contains("The cycle of life"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_cycleOfLife;
			}
			if (text.Contains("It is happening again"))
			{
				return LanguageManager.CurrentLanguage.act3.act3_fraudSecond_happeningAgain;
			}
			return "Unimplemented string";
		}

		public static string Level8Secret(string message, string message2)
		{
			return "Unimplemented string";
		}

		public static string Level91(string message, string message2)
		{
			return "Unimplemented string";
		}

		public static string Level92(string message, string message2)
		{
			return "Unimplemented string";
		}

		public static string GetMessage(string message, string message2, string input)
		{
			string currentSceneName = CommonFunctions.GetCurrentSceneName();
			_ = message + message2;
			switch (currentSceneName)
			{
			case "Level 7-1":
				return Level71(message, message2);
			case "Level 7-2":
				return Level72(message, message2, input);
			case "Level 7-3":
				return Level73(message, message2);
			case "Level 7-4":
				return Level74(message, message2);
			case "Level 7-S":
				return Level7Secret(message, message2);
			case "Level 8-1":
				return Level81(message, message2);
			case "Level 8-2":
				return Level82(message, message2);
			case "Level 8-3":
				return Level83(message, message2);
			case "Level 8-4":
				return Level84(message, message2);
			case "Level 8-S":
				return Level8Secret(message, message2);
			case "Level 9-1":
				return Level91(message, message2);
			case "Level 9-2":
				return Level92(message, message2);
			default:
				return "Unimplemented string";
			}
		}

		public static string GetLevelChallenge(string currentLevel)
		{
			switch (currentLevel)
			{
			case "Level 7-1":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_violenceFirst;
			case "Level 7-2":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_violenceSecond;
			case "Level 7-3":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_violenceThird;
			case "Level 7-4":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_violenceFourth;
			case "Level 8-1":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_fraudFirst;
			case "Level 8-2":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_fraudSecond;
			case "Level 8-3":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_fraudThird;
			case "Level 8-4":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_fraudFourth;
			case "Level 9-1":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_treacheryFirst;
			case "Level 9-2":
				return LanguageManager.CurrentLanguage.levelChallenges.challenges_treacherySecond;
			default:
				return "Unknown challenge description";
			}
		}

		public static string GetLevelName()
		{
			switch (CommonFunctions.GetCurrentSceneName())
			{
			case "Level 7-1":
				return "7-1: " + LanguageManager.CurrentLanguage.levelNames.levelName_violenceFirst;
			case "Level 7-2":
				return "7-2: " + LanguageManager.CurrentLanguage.levelNames.levelName_violenceSecond;
			case "Level 7-3":
				return "7-3: " + LanguageManager.CurrentLanguage.levelNames.levelName_violenceThird;
			case "Level 7-4":
				return "7-4: " + LanguageManager.CurrentLanguage.levelNames.levelName_violenceFourth;
			case "Level 8-1":
				return "8-1: " + LanguageManager.CurrentLanguage.levelNames.levelName_fraudFirst;
			case "Level 8-2":
				return "8-2: " + LanguageManager.CurrentLanguage.levelNames.levelName_fraudSecond;
			case "Level 8-3":
				return "8-3: " + LanguageManager.CurrentLanguage.levelNames.levelName_fraudThird;
			case "Level 8-4":
				return "8-4: " + LanguageManager.CurrentLanguage.levelNames.levelName_fraudFourth;
			case "Level 9-1":
				return "9-1: " + LanguageManager.CurrentLanguage.levelNames.levelName_treacheryFirst;
			case "Level 9-2":
				return "9-2: " + LanguageManager.CurrentLanguage.levelNames.levelName_treacherySecond;
			default:
				return "Unknown level name";
			}
		}
	}
}
