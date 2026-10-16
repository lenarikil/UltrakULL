using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static UltrakULL.CommonFunctions;
using UltrakULL.json;

namespace UltrakULL
{
    public static class LevelStatWindow
    {
        public static void PatchStats(ref GameObject canvasObj)
        {
            GameObject levelStatsWindow = canvasObj.FindPath("Level Stats Controller", "Level Stats (1)");

            // Now Level name directly gets the name from the GetMissionName.cs
            //TextMeshProUGUI levelName = GetTextMeshProUGUI(GetGameObjectChild(levelStatsWindow, "Title"));
            //levelName.text = LevelNames.GetDiscordLevelName(GetCurrentSceneName());

            //Secret levels will only have a timer, or something else.
            TextMeshProUGUI timeName = levelStatsWindow.Child("Time Title").TMP();
            timeName.text = LanguageManager.CurrentLanguage.misc.levelstats_time; 

            TextMeshProUGUI killsName = levelStatsWindow.Child("Kills Title").TMP();
            killsName.text = LanguageManager.CurrentLanguage.misc.levelstats_kills;

            TextMeshProUGUI styleName = levelStatsWindow.Child("Style Title").TMP();
            styleName.text = LanguageManager.CurrentLanguage.misc.levelstats_style;

            TextMeshProUGUI secretsName = levelStatsWindow.Child("Secrets Title").TMP();
            secretsName.text = LanguageManager.CurrentLanguage.misc.levelstats_secrets;

            TextMeshProUGUI challengesName = levelStatsWindow.Child("Challenge Title").TMP();
            challengesName.text = LanguageManager.CurrentLanguage.misc.levelstats_challenge;

            TextMeshProUGUI assistsName = levelStatsWindow.Child("Assists Title").TMP();
            assistsName.text = LanguageManager.CurrentLanguage.misc.levelstats_majorAssists;

            if (GetCurrentSceneName() == "Level 4-S")
            {
                TextMeshProUGUI cratesName = levelStatsWindow.Child("Crates Counter").TMP();
                cratesName.text = LanguageManager.CurrentLanguage.misc.levelstats_boxes;
            }
            if (GetCurrentSceneName() == "Endless")
            {
                killsName.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_currentWave;
                styleName.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_enemiesRemaining;
            }
        }
    }
}
