using System;
using TMPro;
using UltrakULL.audio;
using UnityEngine;
using UnityEngine.UI;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    public static class Act1
    {
        private static void PatchHellmap(ref GameObject canvasObj)
        {
            try
            {
                GameObject hellMapObject = canvasObj.FindPath("Hellmap", "Hellmap Act 1");

                TextMeshProUGUI hellmapLimbo = hellMapObject.Child("Text").TMP();
                hellmapLimbo.text = LanguageManager.CurrentLanguage.misc.hellmap_limbo;

                TextMeshProUGUI hellmapLust = hellMapObject.Child("Text (1)").TMP();
                hellmapLust.text = LanguageManager.CurrentLanguage.misc.hellmap_lust;

                TextMeshProUGUI hellmapGluttony = hellMapObject.Child("Text (2)").TMP();
                hellmapGluttony.text = LanguageManager.CurrentLanguage.misc.hellmap_gluttony;
            }
            catch(Exception e)
            {
                Logging.Warn("Failed to patch Act 1 hellmap.");
                Logging.Warn(e.ToString());
            }
        }
       
        public static void PatchAct1(ref GameObject canvasObj)
        {
            string currentLevel = GetCurrentSceneName();
            string levelName = Act1Strings.GetLevelName();
            string levelChallenge = Act1Strings.GetLevelChallenge(currentLevel);

            PatchHellmap(ref canvasObj);
            PatchResultsScreen(levelName, levelChallenge);

            //"Crane control" and "Test Elevators" panels in 2-1
            if (currentLevel.Contains("2-1"))
            {
                GameObject outdoorsArenas = GetInactiveRootObject("3-4 - Outdoors Arenas");
                GameObject stuff = outdoorsArenas.Child("3-4 Stuff");

                //"Crane control"
                stuff
                    .FindPath("Crane (Moveable)", "Cube (19)", "Cube", "UsableScreen New", "InteractiveScreen", "Canvas", "Background", "Text (TMP) (1)")
                    .SetTMPText(LanguageManager.CurrentLanguage.act1.act1_lustFirst_crane);

                //"Test Elevators" ( 3 - 4 - Outdoors Arenas / 3 - 4 Stuff(Clone) / UsableScreen New / InteractiveScreen / Canvas / Background / InteractiveScreen Button / Text (TMP) )
                stuff
                    .FindPath("UsableScreen New", "InteractiveScreen", "Canvas", "Background", "InteractiveScreen Button", "Text (TMP)")
                    .SetTMPText(LanguageManager.CurrentLanguage.act1.act1_lustFirst_elevator);
            }
        }
    }
}
