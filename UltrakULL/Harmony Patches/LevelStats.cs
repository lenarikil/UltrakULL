using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

using static UltrakULL.CommonFunctions;
using UltrakULL.json;

namespace UltrakULL.Harmony_Patches
{

    //@Override
    //Overrides the Start method from LevelStats class to localize level names
    //MAY BE OBSOLETE AND CAN REMOVE.
    [HarmonyPatch(typeof(LevelStats), "Start")]
    public static class LocalizeLevelStatNames
    {
        private static StatsManager sman = MonoSingleton<StatsManager>.Instance;

        /// <summary>
        /// The name of the level as the game writes it. It is kept, so the built-in
        /// English can be put back on a language switch.
        /// </summary>
        private static string gameLevelName;

        [HarmonyPostfix]
        public static void LevelStatsStart_Postfix(LevelStats __instance)//, StatsManager ___sman)
        {
            if (__instance != null && __instance.levelName != null)
            {
                // The game wrote the name of the level a moment ago.
                gameLevelName = __instance.levelName.text;
            }

            ApplyLevelName(__instance);
        }

        /// <summary>
        /// Applies the active language to the name of the level in the stats screen.
        ///
        /// The game writes the name of the level only once, when the level starts, so
        /// a language switch did not reach it and it kept the previous language.
        /// </summary>
        public static void Reapply()
        {
            LevelStats[] stats = UnityEngine.Object.FindObjectsOfType<LevelStats>(true);
            for (int i = 0; i < stats.Length; i++)
            {
                ApplyLevelName(stats[i]);
            }
        }

        private static void ApplyLevelName(LevelStats __instance)
        {
            if (__instance == null || __instance.levelName == null)
            {
                return;
            }

            if(isUsingEnglish())
            {
                // The built-in English is the game's own text.
                if (!string.IsNullOrEmpty(gameLevelName))
                {
                    __instance.levelName.text = gameLevelName;
                }
                return;
            }
            if (__instance.secretLevel)
            {
                __instance.levelName.text = LanguageManager.CurrentLanguage.frontend.chapter_secretMission;
            }
            if (SceneHelper.IsPlayingCustom)
            {
                MapInfo instance = MapInfo.Instance;
                __instance.levelName.text = ((instance != null) ? instance.levelName : "???");
            }
            RankData rankData = null;
            if (sman.levelNumber != 0 && !Debug.isDebugBuild)
            {
                rankData = GameProgressSaver.GetRank(true);
                __instance.levelName.text = LevelNames.GetDiscordLevelName(GetCurrentSceneName());
            }
            if (sman.levelNumber != 0 && (Debug.isDebugBuild || (rankData != null && rankData.levelNumber == sman.levelNumber)))
            {
                StockMapInfo instance2 = StockMapInfo.Instance;
                if (instance2 != null)
                {
                    __instance.levelName.text = LevelNames.GetDiscordLevelName(GetCurrentSceneName());
                }
            }
            __instance.levelName.text = LevelNames.GetDiscordLevelName(GetCurrentSceneName());
        }
    }

    //@Override
    //Overrides the CheckStats method from the LevelStats class to localize the stats screen
    [HarmonyPatch(typeof(LevelStats), "CheckStats")]
    public static class LocalizeStatsScreen
    {
        private static StatsManager sman = MonoSingleton<StatsManager>.Instance;
        
        [HarmonyPostfix]
        public static void CheckStats_Postfix(LevelStats __instance)
        {
            if(isUsingEnglish())
            {
                return;
            }
            if (__instance.challenge)
            {
                if (MonoSingleton<ChallengeManager>.Instance.challengeDone && !MonoSingleton<ChallengeManager>.Instance.challengeFailed)
                {
                    __instance.challenge.text = "<color=#FFAF00>" + LanguageManager.CurrentLanguage.misc.state_yes + "</color>";
                }
                else
                {
                    __instance.challenge.text = LanguageManager.CurrentLanguage.misc.state_no;
                }
            }
            if (__instance.majorAssists)
            {
                if (sman.majorUsed)
                {
                    __instance.majorAssists.text = "<color=#4C99E6>" + LanguageManager.CurrentLanguage.misc.state_yes + "</color>";
                    return;
                }
                __instance.majorAssists.text = LanguageManager.CurrentLanguage.misc.state_no;
            }
        }
    }
}
