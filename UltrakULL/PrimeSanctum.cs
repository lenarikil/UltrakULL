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

                secretText = bossRoom.FindPath("3 Stuff", "End", "FinalRoom Prime", "Testament Shop", "Canvas", "Border", "TipBox", "Panel", "Scroll View", "Viewport", "Content", "Text (1)").Text();
            }
            else if (currentLevel.Contains("P-2"))
            {
                GameObject bossRoom = GetInactiveRootObject("Main Section").Child("9 - Boss Arena");
                
                secretText = bossRoom.FindPath("Boss Stuff", "Outro", "FinalRoom Prime Variant", "Testament Shop", "Canvas", "Border", "TipBox", "Panel", "Scroll View", "Viewport", "Content", "Text (1)").Text();
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
                GameObject firstLockObject = GetInactiveRootObject("Main Section").FindPath("Inside", "8 - Elevator", "8 Stuff", "InteractiveScreenWithStand", "InteractiveScreen", "Canvas", "Background");
                
                TextMeshProUGUI firstLockLocked = firstLockObject.FindPath("A", "Text (TMP)").TMP();
                TextMeshProUGUI firstLockUnlocked = firstLockObject.FindPath("A (Pressed)", "Text (TMP)").TMP();
                TextMeshProUGUI secondLockLocked = firstLockObject.FindPath("B", "Text (TMP)").TMP();
                TextMeshProUGUI secondLockUnlocked = firstLockObject.FindPath("B (Pressed)", "Text (TMP)").TMP();
                TextMeshProUGUI thirdLockLocked = firstLockObject.FindPath("C", "Text (TMP)").TMP();
                TextMeshProUGUI thirdLockUnlocked = firstLockObject.FindPath("C (Pressed)", "Text (TMP)").TMP();

                firstLockLocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockFirstLocked;
                firstLockUnlocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockUnlocked;
                secondLockLocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockSecondLocked;
                secondLockUnlocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockUnlocked;
                thirdLockLocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockThirdLocked;
                thirdLockUnlocked.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockUnlocked;
                
                //Second lock buttons
                GameObject secondLockObject = GetInactiveRootObject("Main Section").FindPath("Inside", "8 - Elevator", "8 Stuff", "InteractiveScreenWithStand (1)", "InteractiveScreen", "Canvas", "Background");

                TextMeshProUGUI secondLockOpen =
                    secondLockObject.FindPath("InteractiveScreenButton", "Text (TMP)").TMP();
                TextMeshProUGUI secondLockAreYouSure = secondLockObject.FindPath("AreYouSure", "Text").TMP();

                TextMeshProUGUI secondLockWarning = secondLockObject.FindPath("WarningText", "WarningText").TMP();
                GameObject secondLockAsIf = secondLockObject.Child("AsIfText");

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
                        TextMeshProUGUI haText = secondLockAsIf.ChildAt(i).Child("Text (TMP)").TMP();
                        haText.text = LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockNo1;
                    }
                    catch { }
                }

                TextMeshProUGUI secondLockAsIfText = secondLockAsIf.ChildAt(7).Child("Text (TMP)").TMP();
                secondLockAsIfText.text =
                    "<color=red>" + LanguageManager.CurrentLanguage.primeSanctum.primeSanctum_second_lockNo2 + "\n</color>";
                
                
                
                PatchResultsScreen(levelname, "");
                
                PatchSecretText(primeSanctumChallengeStrings);
            }
        }
    }
}
