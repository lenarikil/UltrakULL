using System;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using static UltrakULL.CommonFunctions;


namespace UltrakULL
{
    public static class Encore
    {
        public static void PatchEncore(ref GameObject canvasObj)
        {
            string currentLevel = GetCurrentSceneName();
            string levelName = EncoreStrings.GetLevelName();
            string levelChallenge = EncoreStrings.GetLevelChallenge(currentLevel); //We leave the part with the challenges patch if the game developers decide to add them.

            PatchResultsScreen(levelName, levelChallenge);
            Logging.Info("Current level: " + currentLevel);

            if (currentLevel.Contains("0-E"))
            {
                try
                {
                    GameObject heatResistanceWindow = canvasObj.FindPath("HurtScreen", "Heat Resistance");

                    TextMeshProUGUI heatResistanceWarn = heatResistanceWindow.Child("Warning").TMP();
                    heatResistanceWarn.enableWordWrapping = false;
                    heatResistanceWarn.text = LanguageManager.CurrentLanguage.encore.encorePrelude_heatResistanceWarn;

                    TextMeshProUGUI heatResistanceText = heatResistanceWindow.Child("Flavor Text").TMP();
                    heatResistanceText.text = LanguageManager.CurrentLanguage.encore.encorePrelude_heatResistanceText;

                    TextMeshProUGUI heatResistanceTitle = heatResistanceWindow.FindPath("Meter", "Label").TMP();
                    heatResistanceTitle.text = LanguageManager.CurrentLanguage.encore.encorePrelude_heatResistanceTitle;

                    GameObject heatResistanceFixedWindow = canvasObj.FindPath("HurtScreen", "Heat Fixed");

                    TextMeshProUGUI heatResistanceFixedWarn = heatResistanceFixedWindow.Child("Warning").TMP();
                    heatResistanceFixedWarn.enableWordWrapping = false;
                    heatResistanceFixedWarn.text = LanguageManager.CurrentLanguage.encore.encorePrelude_heatResistanceRepaired;

                    TextMeshProUGUI heatResistanceFixedText = heatResistanceFixedWindow.Child("Flavor Text").TMP();
                    heatResistanceFixedText.text = LanguageManager.CurrentLanguage.encore.encorePrelude_heatResistanceRepairedText;

                    TextMeshProUGUI heatResistanceFixedTitle = heatResistanceFixedWindow.FindPath("Meter", "Label").TMP();
                    heatResistanceFixedTitle.text = LanguageManager.CurrentLanguage.encore.encorePrelude_heatResistanceTitle;
                }
                catch (Exception e)
                {
                    Logging.Error("Error patching 0-E: " + e.Message);

                }
            }
            if (currentLevel.Contains("1-E"))
            {
                try
                {
                    GameObject warningCanvas = GetInactiveRootObject("11 - Skull Room").FindPath("11 Nonstuff", "Room", "Cube (24)", "Canvas");
                    TextMeshProUGUI warningText = warningCanvas.Child("Text (TMP)").TMP();
                    warningText.text = LanguageManager.CurrentLanguage.encore.encoreLimbo_warningText;
                }
                catch (Exception e)
                {
                    Logging.Error("Error patching 1-E: " + e.Message);

                }
            }
        }
    }
}

