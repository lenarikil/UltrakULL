using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UltrakULL.audio;
using static UltrakULL.CommonFunctions;
using UltrakULL.json;

namespace UltrakULL
{
    public static class Act2
    {
        private static void PatchHellmap(ref GameObject canvasObj)
        {
            GameObject hellMapObject = canvasObj.FindPath("Hellmap", "Hellmap Act 2");

            TextMeshProUGUI hellmapGreed = hellMapObject.Child("Text").TMP();
            hellmapGreed.text = LanguageManager.CurrentLanguage.misc.hellmap_greed;

            TextMeshProUGUI hellmapWrath = hellMapObject.Child("Text (1)").TMP();
            hellmapWrath.text = LanguageManager.CurrentLanguage.misc.hellmap_wrath;

            TextMeshProUGUI hellmapHeresy = hellMapObject.Child("Text (2)").TMP();
            hellmapHeresy.text = LanguageManager.CurrentLanguage.misc.hellmap_heresy;
        }
    
        public static void PatchAct2(ref GameObject canvasObj)
        {
            string currentLevel = GetCurrentSceneName();
            string levelName = Act2Strings.GetLevelName();
            string levelChallenge = Act2Strings.GetLevelChallenge(currentLevel);
            
            PatchResultsScreen(levelName, levelChallenge);
            PatchHellmap(ref canvasObj);
        }
    }
}
