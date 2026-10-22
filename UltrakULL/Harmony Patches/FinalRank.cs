using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UltrakULL.json;
using UnityEngine.UI;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the SetInfo method from the FinalRank class. This is needed to swap text in the extra into box on the results screen.
    [HarmonyPatch(typeof(FinalRank), "SetInfo")]
    public static class LocalizeFinalRankInfo
    {
        private sealed class RankInfo
        {
            public int Restarts;
            public bool Damage;
            public bool MajorUsed;
            public bool CheatsUsed;
        }

        private static readonly Dictionary<FinalRank, RankInfo> displayedRanks = new Dictionary<FinalRank, RankInfo>();

        [HarmonyPrefix]
        public static bool SetInfo_MyPatch(int restarts, bool damage, bool majorUsed, bool cheatsUsed, FinalRank __instance, bool ___noRestarts, bool ___majorAssists, bool ___noDamage)
        {
            displayedRanks[__instance] = new RankInfo
            {
                Restarts = restarts,
                Damage = damage,
                MajorUsed = majorUsed,
                CheatsUsed = cheatsUsed
            };

            if(isUsingEnglish())
            {
                return true;
            }
            
            __instance.extraInfo.text = "";
            int num = 1;
            if (!damage)
            {
                num++;
            }
            if (majorUsed)
            {
                num++;
            }
            if (cheatsUsed)
            {
                num++;
            }
            if (cheatsUsed)
            {
                TMP_Text text = __instance.extraInfo;
                text.text += "- <color=#44FF45>" + LanguageManager.CurrentLanguage.misc.endstats_cheatsUsed + "</color>\n";
            }
            if (majorUsed)
            {
                TMP_Text text2 = __instance.extraInfo;
                text2.text += "- <color=#4C99E6>" + LanguageManager.CurrentLanguage.misc.endstats_assistsUsed + "</color>\n";
                ___majorAssists = true;
            }
            if (restarts == 0)
            {
                if (num >= 3)
                {
                    TMP_Text text3 = __instance.extraInfo;
                    text3.text += "+ " + LanguageManager.CurrentLanguage.misc.endstats_noRestarts + "\n";
                }
                else
                {
                    TMP_Text text4 = __instance.extraInfo;
                    if (LanguageManager.CurrentLanguage.metadata.langHinduNumbers)
                    {
                        text4.text += "+ " + LanguageManager.CurrentLanguage.misc.endstats_noRestarts + "\n  (+500<color=orange>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>)\n";
                    }
                    else
                    {
						text4.text += "+ " + LanguageManager.CurrentLanguage.misc.endstats_noRestarts + "\n  (+500<color=orange>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>)\n";
					}
                }
                ___noRestarts = true;
            }
            else
            {
                TMP_Text text5 = __instance.extraInfo;
                if (LanguageManager.CurrentLanguage.metadata.langHinduNumbers)
                {
                    text5.text = string.Concat(new object[]
                    {
                    text5.text,
                    "- <color=red>",
                    ArabicFixerTool.FixLine(restarts.ToString()),
                    "</color> " + LanguageManager.CurrentLanguage.misc.endstats_restarts +"\n"
                    });
                }
                else
                {
                    text5.text = "- <color=red>" + restarts + "</color> " + LanguageManager.CurrentLanguage.misc.endstats_restarts + "\n";
                }
            }
            if (!damage)
            {
                if (num >= 3)
                {
                    TMP_Text text6 = __instance.extraInfo;
                    text6.text += "+ <color=orange>" + LanguageManager.CurrentLanguage.misc.endstats_noDamage + "</color>\n";
                }
                else
                {
                    TMP_Text text7 = __instance.extraInfo;
                    text7.text += "+ <color=orange>" + LanguageManager.CurrentLanguage.misc.endstats_noDamage + "\n  (</color>+5,000<color=orange>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + ")</color>\n";
                }
                ___noDamage = true;
            }
            return false;
        }

        /// <summary>
        /// Rewrites the extra-information line of an already visible final-rank
        /// panel. SetInfo only mutates its own text and flags, so this does not replay
        /// the rank animation or reload the results screen.
        /// </summary>
        public static void Reapply()
        {
            List<FinalRank> dead = new List<FinalRank>();
            // SetInfo passes through this Harmony prefix and refreshes the cached
            // entry. Iterate a snapshot so that cache update cannot invalidate the
            // enumerator.
            foreach (KeyValuePair<FinalRank, RankInfo> pair in new List<KeyValuePair<FinalRank, RankInfo>>(displayedRanks))
            {
                FinalRank rank = pair.Key;
                if (rank == null)
                {
                    dead.Add(rank);
                    continue;
                }

                if (!rank.gameObject.activeInHierarchy || pair.Value == null)
                    continue;

                try
                {
                    rank.SetInfo(pair.Value.Restarts, pair.Value.Damage, pair.Value.MajorUsed, pair.Value.CheatsUsed);
                }
                catch (System.Exception e)
                {
                    Logging.Warn("[FinalRank] Failed to refresh the visible rank information: " + e.Message);
                }
            }

            for (int i = 0; i < dead.Count; i++)
                displayedRanks.Remove(dead[i]);
        }
    }

    [HarmonyPatch(typeof(FinalRank), "PointsShow")]
    public static class FinalRank_PointsShow_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && (string)code.operand == "<color=orange>P</color>")
                {
                    // Keep the vanilla "P" when the language has no money label (built-in English).
                    string money = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.shop != null
                        ? LanguageManager.CurrentLanguage.shop.shop_moneyCount : null;
                    if (!string.IsNullOrEmpty(money))
                        code.operand = "<color=orange>" + money + "</color>";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(FinalCyberRank), "PointsShow")]
    public static class FinalCyberRank_PointsShow_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && (string)code.operand == "<color=orange>P</color>")
                {
                    // Keep the vanilla "P" when the language has no money label (built-in English).
                    string money = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.shop != null
                        ? LanguageManager.CurrentLanguage.shop.shop_moneyCount : null;
                    if (!string.IsNullOrEmpty(money))
                        code.operand = "<color=orange>" + money + "</color>";
                }

                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(FinalCyberRank), "Update")]
    public static class FinalCyberRank_PointsUpdate_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && (string)code.operand == "<color=orange>P</color>")
                {
                    // Keep the vanilla "P" when the language has no money label (built-in English).
                    string money = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.shop != null
                        ? LanguageManager.CurrentLanguage.shop.shop_moneyCount : null;
                    if (!string.IsNullOrEmpty(money))
                        code.operand = "<color=orange>" + money + "</color>";
                }

                yield return code;
            }
        }
    }
}
