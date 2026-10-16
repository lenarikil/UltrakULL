using HarmonyLib;
using UnityEngine.UI;
using UltrakULL.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using TMPro;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    public class LevelLeaderboards
    {
        [HarmonyPatch(typeof(LevelSelectLeaderboard),"OnEnable")]
        public class LevelLeaderboardPatch
        {
            [HarmonyPostfix]
            public static void LevelLeaderboardPatch_Postfix(ref TMP_Text ___anyPercentLabel, ref TMP_Text ___pRankLabel, ref GameObject ___noItemsPanel)
            {
                if(isUsingEnglish())
                {
                    return;
                }
                LeaderboardProperties.Difficulties[0] = LanguageManager.CurrentLanguage.frontend.difficulty_harmless;
                LeaderboardProperties.Difficulties[1] =  LanguageManager.CurrentLanguage.frontend.difficulty_lenient;
                LeaderboardProperties.Difficulties[2] =  LanguageManager.CurrentLanguage.frontend.difficulty_standard;
                LeaderboardProperties.Difficulties[3] =  LanguageManager.CurrentLanguage.frontend.difficulty_violent;
                LeaderboardProperties.Difficulties[4] =  LanguageManager.CurrentLanguage.frontend.difficulty_brutal;
                //not yet
                //LeaderboardProperties.Difficulties[5] = LanguageManager.CurrentLanguage.frontend.difficulty_umd;

                ___anyPercentLabel.text = LanguageManager.CurrentLanguage.frontend.leaderboard_anyPercent;
                ___pRankLabel.text = LanguageManager.CurrentLanguage.frontend.leaderboard_pPercent;

                Text noItems = ___noItemsPanel.Child("Text").Text();
                noItems.text = LanguageManager.CurrentLanguage.frontend.leaderboard_noEntries;

            }
        }
        
        [HarmonyPatch(typeof(LevelEndLeaderboard),"Update")]
        public class LevelLeaderboardEndPatch
        {
            // Update runs every frame while the results panel is shown; cache the TMP
            // references and only write when the target text actually changes.
            private static GameObject cachedPanel;
            private static TextMeshProUGUI cachedConnecting;
            private static TextMeshProUGUI cachedReminder;

            [HarmonyPostfix]
            public static void LevelLeaderboardEndPatch_Postfix(ref bool ___displayPRank, ref TMP_Text ___leaderboardType, ref GameObject ___loadingPanel)
            {
                if(isUsingEnglish())
                {
                    return;
                }

                string typeText = (___displayPRank ? LanguageManager.CurrentLanguage.frontend.leaderboard_pPercent : LanguageManager.CurrentLanguage.frontend.leaderboard_anyPercent);
                if (___leaderboardType != null && ___leaderboardType.text != typeText)
                {
                    ___leaderboardType.text = typeText;
                }

                if (!ReferenceEquals(cachedPanel, ___loadingPanel))
                {
                    cachedPanel = ___loadingPanel;
                    cachedConnecting = null;
                    cachedReminder = null;
                }

                if (cachedConnecting == null && ___loadingPanel != null)
                {
                    cachedConnecting = ___loadingPanel.TMP();
                }

                if (cachedReminder == null && ___loadingPanel != null)
                {
                    cachedReminder = ___loadingPanel.transform.parent.gameObject.Child("SettingsReminder").TMP();
                }

                string connectingText = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_connectingToSteam;
                if (cachedConnecting != null && cachedConnecting.text != connectingText)
                {
                    cachedConnecting.text = connectingText;
                }

                string reminderText = LanguageManager.CurrentLanguage.frontend.leaderboard_reminder;
                if (cachedReminder != null && cachedReminder.text != reminderText)
                {
                    cachedReminder.text = reminderText;
                }
            }
        }
        
    }
}