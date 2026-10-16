using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UltrakULL.CommonFunctions;
using UltrakULL.json;

namespace UltrakULL
{
    public class Intermission
    {
        private void Act1Int(GameObject intermissionObject)
        {
            //ACT 1
            //Two text elements to patch: Foreground and background shadow.
            Text toBeContinued = intermissionObject.FindPath("Panel (1)", "Text", "Text (1)").Text();
            toBeContinued.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_tobecontinued;

            Text tobeContinuedShadow = intermissionObject.FindPath("Panel (1)", "Text").Text();
            tobeContinuedShadow.text = toBeContinued.text;

            GameObject act1EndObject = intermissionObject.FindPath("Act End Message", "Sound 1");

            Text act1EndText = act1EndObject.Child("Text").Text();
            act1EndText.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_endof + "\n\n\n\n" + LanguageManager.CurrentLanguage.intermission.act1_intermission_insertAct2;
            
            Text act1EndMenu = act1EndObject.FindPath("Menu", "Text").Text();
            act1EndMenu.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_returnToMenu;

            Text act1EndInsert = act1EndObject.FindPath("Insert", "Text").Text();
            act1EndInsert.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_insert;
        }

        private void Act2Int(GameObject intermissionObject)
        {
            Text toBeContinued = intermissionObject.FindPath("Panel (1)", "Text", "Text (1)").Text();
            toBeContinued.text = LanguageManager.CurrentLanguage.intermission.act2_intermission_tobecontinued;

            Text tobeContinuedShadow = intermissionObject.FindPath("Panel (1)", "Text").Text();
            tobeContinuedShadow.text = toBeContinued.text;
            
            GameObject act2EndObject = intermissionObject.FindPath("Act End Message", "Sound 1");

            Text act2EndText = act2EndObject.Child("Text").Text();
            act2EndText.text = LanguageManager.CurrentLanguage.intermission.act2_intermission_endof + "\n\n\n\n" + LanguageManager.CurrentLanguage.intermission.act2_intermission_insertAct3;
            
            Text act2EndMenu = act2EndObject.FindPath("Menu", "Text").Text();
            act2EndMenu.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_returnToMenu;

            Text act2EndInsert = act2EndObject.FindPath("Insert", "Text").Text();
            act2EndInsert.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_insert;
        }

        private void EarlyAccessEnd(GameObject intermissionObject)
        {
            GameObject earlyAccessEnd = intermissionObject.Child("Skippables");
            if (earlyAccessEnd != null)
            {
                TextMeshProUGUI earlyAccessEndText = earlyAccessEnd.Child("Text").TMP();

                earlyAccessEndText.text =
                    "<size=48><b>" + LanguageManager.CurrentLanguage.misc.earlyAccessEnd1 + "</b></size>" + "\n\n"
                    + LanguageManager.CurrentLanguage.misc.earlyAccessEnd2 + "\n\n"
                    + LanguageManager.CurrentLanguage.misc.earlyAccessEnd3;

                TextMeshProUGUI earlyAccessQuitToMenu = earlyAccessEnd.FindPath("Quit Mission", "Text").TMP();
                earlyAccessQuitToMenu.text = LanguageManager.CurrentLanguage.intermission.act1_intermission_returnToMenu;
            }
        }

        public Intermission(ref GameObject canvasObj)
        {
            GameObject intermissionObject = canvasObj.FindPath("PowerUpVignette", "Panel");

            switch (GetCurrentSceneName())
            {
                case "Intermission1": { Act1Int(intermissionObject);  break; }
                case "Intermission2": { Act2Int(intermissionObject);  break; }
                case "EarlyAccessEnd": { EarlyAccessEnd(canvasObj.FindPath("UnderwaterOverlay", "Panel")); break; }
            }
        }
    }
}
