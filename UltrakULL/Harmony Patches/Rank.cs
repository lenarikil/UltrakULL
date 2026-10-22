using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltrakULL.json;
using UnityEngine.UI;
using TMPro;

namespace UltrakULL.Harmony_Patches
{
	[HarmonyPatch(typeof(FinalRank), "SetRank")]
	public static class FinalRank_SetRankPatch
	{
		static bool enablepatchSetRank = true;
		[HarmonyPrefix]
		public static void Patch(FinalRank __instance, ref string rank)
		{
			// Built-in English keeps the game's original rank letters.
			if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

			try
			{
				if (!enablepatchSetRank) { return; }
				Logging.Info("CALLED SET RANK :D");
				Logging.Info(rank[16].ToString());

				Rank ranks = LanguageManager.CurrentLanguage.ranks;
				string replacement = "_";

				switch (rank[15].ToString())
				{
					case "P":
						if (ranks.rank_letter_p != null)
						{
							// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang P!");
							replacement = ranks.rank_letter_p;
							// return __result;
						}
						break;
					case "C":
						if (ranks.rank_letter_c != null)
						{
							// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang C!");
							replacement = ranks.rank_letter_c;
							// return __result;
						}
						break;
					case "B":
						if (ranks.rank_letter_b != null)
						{
							// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang B!");
							replacement = ranks.rank_letter_b;
							// return __result;
						}
						break;
					case "A":
						if (ranks.rank_letter_a != null)
						{
							// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang A!");
							replacement = ranks.rank_letter_a;
							// return __result;
						}
						break;
					case "S":
						if (ranks.rank_letter_s != null)
						{
							// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang S!");
							replacement = ranks.rank_letter_s;
							// return __result;
						}
						break;
					case "D":
						if (ranks.rank_letter_d != null)
						{
							// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang D!");
							replacement = ranks.rank_letter_d;
							// return __result;
						}
						break;
				}

				char[] chars = rank.ToCharArray();
				chars[15] = replacement[0];

				rank = new string(chars);
			}
            catch (Exception e)
            {
                Logging.Warn("Failed to Patch SetRank :(");
                Logging.Warn(e.Message);
                enablepatchSetRank = false;
                return;
            }
        }

		[HarmonyPatch(typeof(StatsManager), "GetRanks")]
		public static class StatsManager_GetRanksPatch
        {
            [HarmonyPostfix]
			public static void Patch(FinalRank __instance, ref string __result, int[] ranksToCheck, float value, bool reverse, bool addToRankScore = false)
			{
                // Built-in English keeps the game's original rank letters.
                if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

                try
                {
                    string rank = __result;
					Rank ranks = LanguageManager.CurrentLanguage.ranks;
					if(ranks == null)
					{
						Logging.Warn("Ranks is null in JSON lang file! Is ranks section missing?");
						return;
					}
					string replacement = "_";
					switch (rank[15].ToString())
					{
						case "P":
							if (ranks.rank_letter_p != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang P!");
								replacement = ranks.rank_letter_p;
								// return __result;
							}
							break;
						case "C":
							if (ranks.rank_letter_c != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang C!");
								replacement = ranks.rank_letter_c;
								// return __result;
							}
							break;
						case "B":
							if (ranks.rank_letter_b != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang B!");
								replacement = ranks.rank_letter_b;
								// return __result;
							}
							break;
						case "A":
							if (ranks.rank_letter_a != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang A!");
								replacement = ranks.rank_letter_a;
								// return __result;
							}
							break;
						case "S":
							if (ranks.rank_letter_s != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang S!");
								replacement = ranks.rank_letter_s;
								// return __result;
							}
							break;
						case "D":
							if (ranks.rank_letter_d != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang D!");
								replacement = ranks.rank_letter_d;
								// return __result;
							}
							break;
					}

					char[] chars = rank.ToCharArray();
					chars[15] = replacement[0];

					__result = new string(chars);
				}
                catch (Exception e)
                {
                    Logging.Warn("Failed to Patch GetRanks :(");
                    Logging.Warn(e.ToString());
                    return;
                }
            }
		}

		[HarmonyPatch(typeof(LevelSelectPanel), nameof(LevelSelectPanel.CheckScore))]
		public static class LevelSelectPanel_CheckScorePatch
		{
            [HarmonyPostfix]
			public static void Postfix(LevelSelectPanel __instance)
			{
                // Built-in English keeps the game's original rank letters.
                if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

                try
				{
                    TextMeshProUGUI componentInChildren = __instance.transform.Find("Stats").Find("Rank").GetComponentInChildren<TextMeshProUGUI>();

					Rank ranks = LanguageManager.CurrentLanguage.ranks;
					string replacement = "_";
					string rank = componentInChildren.text;

					componentInChildren.verticalAlignment = VerticalAlignmentOptions.Bottom;
					//componentInChildren.autoSizeTextContainer = true;

					// at least 16 in length, otherwise its 99.999% certain to be nothing.
					if (rank.Length < 16)
						return;

					switch (rank[15].ToString())
					{
						case "P":
							if (ranks.rank_letter_p != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang P!");
								replacement = ranks.rank_letter_p;
								// return __result;
							}
							break;
						case "C":
							if (ranks.rank_letter_c != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang C!");
								replacement = ranks.rank_letter_c;
								// return __result;
							}
							break;
						case "B":
							if (ranks.rank_letter_b != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang B!");
								replacement = ranks.rank_letter_b;
								// return __result;
							}
							break;
						case "A":
							if (ranks.rank_letter_a != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang A!");
								replacement = ranks.rank_letter_a;
								// return __result;
							}
							break;
						case "S":
							if (ranks.rank_letter_s != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang S!");
								replacement = ranks.rank_letter_s;
								// return __result;
							}
							break;
						case "D":
							if (ranks.rank_letter_d != null)
							{
								// Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang D!");
								replacement = ranks.rank_letter_d;
								// return __result;
							}
							break;
						default:
							// this means that we havent played
							return;
					}

					char[] chars = rank.ToCharArray();
					chars[15] = replacement[0];

					componentInChildren.text = new string(chars);
				}
				catch (Exception e) 
				{ 
					Logging.Warn("Failed to Patch Level Menu :(");
                    if (LanguageManager.CurrentLanguage.ranks == null)
                    { Logging.Warn("Category \"Ranks\" is missing from the language file!"); return; }
                    Logging.Warn(e.ToString());
                    return;
				}
			}
		}
	}
}
