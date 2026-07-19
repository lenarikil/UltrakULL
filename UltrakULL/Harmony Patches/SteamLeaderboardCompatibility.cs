using HarmonyLib;
using System;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch]
    public static class SteamLeaderboardCompatibility
    {
        private static readonly string[] _originalDifficulties = {
            "Harmless", "Lenient", "Standard", "Violent", "Brutal", ""
        };

        private static void RestoreEnglishDifficulties()
        {
            for (int i = 0; i < _originalDifficulties.Length && i < LeaderboardProperties.Difficulties.Length; i++)
            {
                LeaderboardProperties.Difficulties[i] = _originalDifficulties[i];
            }
        }

        private static void RestoreTranslatedDifficulties()
        {
            LeaderboardProperties.Difficulties[0] = LanguageManager.CurrentLanguage.frontend.difficulty_harmless;
            LeaderboardProperties.Difficulties[1] = LanguageManager.CurrentLanguage.frontend.difficulty_lenient;
            LeaderboardProperties.Difficulties[2] = LanguageManager.CurrentLanguage.frontend.difficulty_standard;
            LeaderboardProperties.Difficulties[3] = LanguageManager.CurrentLanguage.frontend.difficulty_violent;
            LeaderboardProperties.Difficulties[4] = LanguageManager.CurrentLanguage.frontend.difficulty_brutal;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LeaderboardController), "SubmitCyberGrindScore")]
        public static void OnSubmitCyberGrindPrefix()
        {
            if (isUsingEnglish()) return;
            RestoreEnglishDifficulties();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LeaderboardController), "SubmitCyberGrindScore")]
        public static void OnSubmitCyberGrindPostfix()
        {
            if (isUsingEnglish()) return;
            RestoreTranslatedDifficulties();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LeaderboardController), "GetCyberGrindScores")]
        public static void OnGetCyberGrindScoresPrefix()
        {
            if (isUsingEnglish()) return;
            RestoreEnglishDifficulties();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LeaderboardController), "GetCyberGrindScores")]
        public static void OnGetCyberGrindScoresPostfix()
        {
            if (isUsingEnglish()) return;
            RestoreTranslatedDifficulties();
        }
    }
}
