using TMPro;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    class PrimeSanctum
    {
        private void PatchSecretText(PrimeSanctumStrings strings)
        {
            string currentLevel = GetCurrentSceneName();

            Text secretText = null;

            if (currentLevel.Contains("P-1"))
            {
                GameObject bossRoom = GetInactiveRootObject("3 - Fuckatorium");

                secretText = GetTextfromGameObject(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(bossRoom, "3 Stuff"),"End"),"FinalRoom Prime"),"Testament Shop"),"Canvas"),"Border"),"TipBox"),"Panel"),"Scroll View"),"Viewport"),"Content"),"Text (1)"));
            }
            else if (currentLevel.Contains("P-2"))
            {
                GameObject bossRoom = GetGameObjectChild(GetInactiveRootObject("Main Section"),"9 - Boss Arena");
                
                secretText = GetTextfromGameObject(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(bossRoom, "Boss Stuff"),"Outro"),"FinalRoom Prime Variant"),"Testament Shop"),"Canvas"),"Border"),"TipBox"),"Panel"),"Scroll View"),"Viewport"),"Content"),"Text (1)"));
            }

            if (secretText != null)
            {
                secretText.fontSize = 18;
                secretText.text = strings.GetSecretText();
            }

            
        }

        public PrimeSanctum()
        {
            string currentLevel = GetCurrentSceneName();

            if (currentLevel.Contains("P-1"))
            {
                PrimeSanctumStrings primeSanctumChallengeStrings = new PrimeSanctumStrings();
                string levelname = primeSanctumChallengeStrings.GetLevelName();
                PatchResultsScreen(levelname, "");
                
                PatchSecretText(primeSanctumChallengeStrings);
            }
            else if (currentLevel.Contains("P-2"))
            {
                PrimeSanctumStrings primeSanctumChallengeStrings = new PrimeSanctumStrings();
                string levelname = primeSanctumChallengeStrings.GetLevelName();
                
                //First lock buttons
                GameObject firstLockObject = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Main Section"),"Inside"),"8 - Elevator"),"8 Stuff"), "InteractiveScreenWithStand"), "InteractiveScreen"),"Canvas"), "Background");
                
                TextMeshProUGUI firstLockLocked = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(firstLockObject,"A"), "Text (TMP)"));
                TextMeshProUGUI firstLockUnlocked = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(firstLockObject, "A (Pressed)"), "Text (TMP)"));
                TextMeshProUGUI secondLockLocked = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(firstLockObject,"B"), "Text (TMP)"));
                TextMeshProUGUI secondLockUnlocked = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(firstLockObject, "B (Pressed)"),"Text (TMP)"));
                TextMeshProUGUI thirdLockLocked = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(firstLockObject,"C"), "Text (TMP)"));
                TextMeshProUGUI thirdLockUnlocked = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(firstLockObject, "C (Pressed)"), "Text (TMP)"));

                firstLockLocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockFirstLocked;
                firstLockUnlocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockUnlocked;
                secondLockLocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockSecondLocked;
                secondLockUnlocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockUnlocked;
                thirdLockLocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockThirdLocked;
                thirdLockUnlocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockUnlocked;
                
                //Second lock buttons
                GameObject secondLockObject = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Main Section"), "Inside"), "8 - Elevator"), "8 Stuff"), "InteractiveScreenWithStand (1)"), "InteractiveScreen"), "Canvas"), "Background");

                TextMeshProUGUI secondLockOpen =
                    GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(secondLockObject, "InteractiveScreenButton"), "Text (TMP)"));
                TextMeshProUGUI secondLockAreYouSure = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(secondLockObject, "AreYouSure"), "Text"));

                TextMeshProUGUI secondLockWarning = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(secondLockObject, "WarningText"), "WarningText"));
                GameObject secondLockAsIf = GetGameObjectChild(secondLockObject, "AsIfText");

                secondLockOpen.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockOpen;
                //secondLockOpen.fontSize = 20;
                secondLockAreYouSure.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockAreYouSure;
                //secondLockAreYouSure.fontSize = 20;
                secondLockWarning.text =
                    LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockYes1 + "\n\n"
                    + LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockYes2 + "\n\n"
                    + LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockYes3 + "\n\n"
                    + LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockYes4 + "\n\n<i>"
                    + LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockYes5 + "</i>";

                //secondLockWarning.fontSize = 18;

                for (int i = 0; i < 6; i++)
                {
                    try
                    {
                        TextMeshProUGUI haText = GetTextMeshProUGUI(GetGameObjectChild(secondLockAsIf.transform.GetChild(i).gameObject, "Text (TMP)"));
                        haText.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockNo1;
                    }
                    catch { }
                }

                TextMeshProUGUI secondLockAsIfText = GetTextMeshProUGUI(GetGameObjectChild(secondLockAsIf.transform.GetChild(7).gameObject, "Text (TMP)"));
                secondLockAsIfText.text =
                    "<color=red>" + LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockNo2 + "\n</color>";
                
                
                
                PatchResultsScreen(levelname, "");
                
                PatchSecretText(primeSanctumChallengeStrings);
            }
        }
    }
}
