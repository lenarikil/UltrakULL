using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltrakULL.json;

namespace UltrakULL.Harmony_Patches
{

	[HarmonyPatch(typeof(RankHelper), nameof(RankHelper.GetRankLetter))]
	public static class RankHelperPatch
	{
		[HarmonyPostfix]
		public static void Patch(ref string __result, int rank)
		{
			// Built-in English keeps the game's original rank letters.
			if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

			// string __result;
#if DEBUG
			Logging.Info("[DEBUG] RankHelper::GetRankLetter -> CALLED");
#endif
			Rank ranks = LanguageManager.CurrentLanguage.ranks;
			if (rank < 0)
			{
				// return "";
				return;
			}

			switch (rank)
			{
				case 12:
					if (ranks.rank_letter_p != null)
					{
						Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang P!");
						__result = ranks.rank_letter_p;
						// return __result;
					}
					break;
				case 1:
					if (ranks.rank_letter_c != null)
					{
						Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang C!");
						__result = ranks.rank_letter_c;
						// return __result;
					}
					break;
				case 2:
					if (ranks.rank_letter_b != null)
					{
						Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang B!");
						__result = ranks.rank_letter_b;
						// return __result;
					}
					break;
				case 3:
					if (ranks.rank_letter_a != null)
					{
						Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang A!");
						__result = ranks.rank_letter_a;
						// return __result;
					}
					break;
				case 4:
				case 5:
				case 6:
					if (ranks.rank_letter_s != null)
					{
						Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang S!");
						__result = ranks.rank_letter_s;
						// return __result;
					}
					break;
				default:
					if (ranks.rank_letter_d != null)
					{
						Logging.Info("[DEBUG] RankHelper::GetRankLetter -> Result is lang D!");
						__result = ranks.rank_letter_d;
						// return __result;
					}
					break;
			}
			// return "";
		}
	}
}
