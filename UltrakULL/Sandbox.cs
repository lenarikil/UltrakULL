using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static UltrakULL.CommonFunctions;
using UltrakULL.json;

namespace UltrakULL
{
    class Sandbox
    {
        private void PatchSandboxDupeMenu(ref GameObject canvasObj)
        {
            GameObject dupeMenu = canvasObj.FindPath("Cheat Menu", "Sandbox Saves");

            TextMeshProUGUI dupeMenuTitle = dupeMenu.Child("Title").TMP();
            dupeMenuTitle.text = LanguageManager.CurrentLanguage.cheats.cheats_dupesTitle;

            TextMeshProUGUI dupeMenuOpenFolder = dupeMenu.FindPath("Directory Button Wrapper", "Directory Button", "Text").TMP();
            dupeMenuOpenFolder.text = LanguageManager.CurrentLanguage.cheats.cheats_dupesOpenFolder;

            TextMeshProUGUI dupeMenuPlaceholder = dupeMenu.FindPath("Button", "InputField", "Placeholder").TMP();
            dupeMenuPlaceholder.text = LanguageManager.CurrentLanguage.cheats.cheats_dupesSaveNamePrompt;

            TextMeshProUGUI dupeMenuSave = dupeMenu.FindPath("New Save Wrapper", "Save Button", "Text").TMP();
            dupeMenuSave.text = LanguageManager.CurrentLanguage.cheats.cheats_dupesNewSave;

            GameObject saveOverwriteConfirmPanel = canvasObj.FindPath("SandboxDialogs", "Save Overwrite Confirmation", "Panel");
            TextMeshProUGUI saveOverwriteConfirmText = saveOverwriteConfirmPanel.Child("Text").TMP();
            saveOverwriteConfirmText.text = LanguageManager.CurrentLanguage.cheats.cheats_dupesOverWriteWarn;

            TextMeshProUGUI saveOverwritedisableWindow = saveOverwriteConfirmPanel.Child("Text (1)").TMP();
            saveOverwritedisableWindow.text = LanguageManager.CurrentLanguage.pauseMenu.pause_disableWindow;

            TextMeshProUGUI saveOverwriteCancel = saveOverwriteConfirmPanel.FindPath("Cancel", "Text").TMP();
            saveOverwriteCancel.text = LanguageManager.CurrentLanguage.cheats.cheats_disclaimerNo;

            TextMeshProUGUI saveOverwriteSave = saveOverwriteConfirmPanel.FindPath("Confirm", "Text").TMP();
            saveOverwriteSave.text = "<color=#F9A100>" + LanguageManager.CurrentLanguage.cheats.cheats_dupesSave + "</color>";

        }

        private void PatchMisc(ref GameObject canvasObj)
        {
            //text mesh pro out of date
            Text navmeshWarning = canvasObj.FindPath("Navmesh Warning", "Text (1)").Text();
            navmeshWarning.text = LanguageManager.CurrentLanguage.sandbox.sandbox_navmeshWarn;
			//I don't know how to add a second line that doesn't exist.
			//Text navmeshWarningShadow = GetTextfromGameObject(GetGameObjectChild(GetGameObjectChild(canvasObj, "Navmesh Warning"), "Text (1)"));
            //navmeshWarningShadow.text = LanguageManager.CurrentLanguage.sandbox.sandbox_navmeshWarnshadow;
            //shop
            GameObject sandboxShop = GameObject.Find("Sandbox Shop");

            GameObject sandboxShopCanvas = sandboxShop.FindPath("Canvas", "Background", "Main Panel");

            TextMeshProUGUI sandboxShopStatsTitle = sandboxShopCanvas.FindPath("Stats", "Title").TMP();
            sandboxShopStatsTitle.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_stats;
            //for wondering, actual stat texts are patched in StatsPatch.cs

            
            //Main menu
            GameObject sandboxShopMenu = sandboxShopCanvas.FindPath("Main Menu", "Buttons");

            TextMeshProUGUI sandboxShopTimeOfDayButton = sandboxShopMenu.FindPath("TimeOfDayButton", "Text").TMP();
            sandboxShopTimeOfDayButton.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_timeOfDay;
            
            TextMeshProUGUI sandboxShopWorldOptionsButton = sandboxShopMenu.FindPath("WorldOptionsButton", "Text").TMP();
            sandboxShopWorldOptionsButton.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_worldOptions;
            
            TextMeshProUGUI sandboxShopIconsButton = sandboxShopMenu.FindPath("IconsButton", "Text").TMP();
            sandboxShopIconsButton.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_icons;
            
            //Time of day
            GameObject sandboxShopTimeOfDay = sandboxShopCanvas.Child("Time of Day");

            TextMeshProUGUI sandboxShopTimeOfDayTitle =
                sandboxShopTimeOfDay.Child("Title").TMP();
            sandboxShopTimeOfDayTitle.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_timeOfDay;

            TextMeshProUGUI sandboxShopTimeOfDayLoading =
                sandboxShopTimeOfDay.FindPath("Panel", "Loading", "Title").TMP();
            sandboxShopTimeOfDayLoading.text = LanguageManager.CurrentLanguage.misc.loading + "...";

            TextMeshProUGUI sandboxShopTimeOfDayClose = sandboxShopTimeOfDay.FindPath("Panel", "Close Button", "Text").TMP();
            sandboxShopTimeOfDayClose.text = LanguageManager.CurrentLanguage.options.save_close;
            
            //World options
            GameObject sandboxShopWorldOptions = sandboxShopCanvas.Child("World Options");
            TextMeshProUGUI sandboxShopWorldOptionsTitle = sandboxShopWorldOptions.Child("Title").TMP();
            sandboxShopWorldOptionsTitle.text =
                LanguageManager.CurrentLanguage.sandbox.sandbox_shop_worldOptionsTitle;
            TextMeshProUGUI sandboxWorldOptionsClose = sandboxShopWorldOptions.FindPath("Panel", "Close Button", "Text").TMP();
            sandboxWorldOptionsClose.text = LanguageManager.CurrentLanguage.options.save_close;
            
            GameObject sandboxShopWorldOptionsMapBorder = sandboxShopWorldOptions.FindPath("Panel", "Options", "Map Border");
            TextMeshProUGUI sandboxShopWorldOptionsMapBorderTitle = sandboxShopWorldOptionsMapBorder.Child("Title Text").TMP();
            sandboxShopWorldOptionsMapBorderTitle.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_mapBorder;

            TextMeshProUGUI sandboxShopWorldOptionsMapBorderStatus = sandboxShopWorldOptionsMapBorder.Child("Status Text").TMP();
            TextMeshProUGUI sandboxShopWorldOptionsMapBorderButton = sandboxShopWorldOptionsMapBorder.FindPath("Button", "Text").TMP();
            switch(sandboxShopWorldOptionsMapBorderStatus.text.ToUpper())
            {
                case "ENABLED":
                {
                    sandboxShopWorldOptionsMapBorderStatus.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_worldOptionsEnabled;
                    break;
                }
                case "DISABLED":
                {
                    sandboxShopWorldOptionsMapBorderStatus.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_worldOptionsDisabled;
                    break;
                }
            }
            switch (sandboxShopWorldOptionsMapBorderButton.text.ToUpper())
            {
                case "ENABLE":
                {
                    sandboxShopWorldOptionsMapBorderButton.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_worldOptionsEnable;
                    break;
                }
                case "DISABLE":
                {
                    sandboxShopWorldOptionsMapBorderButton.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_worldOptionsDisable;
                    break;
                }
            }

            //Icons
            GameObject sandboxShopIcons = sandboxShopCanvas.FindPath("Icons", "Icons Window", "Panel");
            TextMeshProUGUI sandboxShopIconsTitle = sandboxShopIcons.transform.parent.gameObject.Child("Title").TMP();
            sandboxShopIconsTitle.text =
                LanguageManager.CurrentLanguage.sandbox.sandbox_shop_iconsTitle;
            
            TextMeshProUGUI sandboxShopIconsDefault = sandboxShopIcons.FindPath("Default", "Text").TMP();
            sandboxShopIconsDefault.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_default;
            
            TextMeshProUGUI sandboxShopIconsPitr = sandboxShopIcons.FindPath("PITR", "Text").TMP();
            sandboxShopIconsPitr.text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_pitr;
            
            TextMeshProUGUI sandboxIconsClose = sandboxShopCanvas.FindPath("Icons", "Back Button", "Text").TMP();
            sandboxIconsClose.text = LanguageManager.CurrentLanguage.shop.shop_back;
        }
        public static void PatchAlterMenu()
        {
            // Sandbox enemy modifier menu

            GameObject panel = GetInactiveRootObject("Canvas").FindPath("Alter Menu Wrapper", "Sandbox Alter Menu", "Spawning Menu");

            GameObject enemyAlterMenu = panel.FindPath("Scroll View", "Viewport", "Content");

            TextMeshProUGUI enemyAlterMenuTitle = enemyAlterMenu.FindPath("Header", "Title").TMP();
            enemyAlterMenuTitle.text = LanguageManager.CurrentLanguage.misc.enemyAlter_title;

            GameObject enemyAlterSizeMenu = enemyAlterMenu.Child("Size Options");
            TextMeshProUGUI enemyAlterSizeTitle = enemyAlterSizeMenu.Child("Title (1)").TMP();
            enemyAlterSizeTitle.text = LanguageManager.CurrentLanguage.misc.enemyAlter_sizeTitle;

            TextMeshProUGUI enemyAlterSizeUniform = enemyAlterSizeMenu.FindPath("Toggle", "Label").TMP();
            enemyAlterSizeUniform.text = LanguageManager.CurrentLanguage.misc.enemyAlter_uniformToggle;

            GameObject enemyAlterSizeUniformContainer = enemyAlterSizeMenu.FindPath("Uniform Container", "Image");
            TextMeshProUGUI enemyAlterSizeUniformContainerSmaller = enemyAlterSizeUniformContainer.FindPath("Divide By Two Button", "Text").TMP();
            TextMeshProUGUI enemyAlterSizeUniformContainerDefault = enemyAlterSizeUniformContainer.FindPath("Default Size Button", "Text").TMP();
            TextMeshProUGUI enemyAlterSizeUniformContainerLarger = enemyAlterSizeUniformContainer.FindPath("Time Two Button", "Text").TMP();
            enemyAlterSizeUniformContainerSmaller.text = LanguageManager.CurrentLanguage.misc.enemyAlter_uniformSmall;
            enemyAlterSizeUniformContainerDefault.text = LanguageManager.CurrentLanguage.misc.enemyAlter_uniformDefault;
            enemyAlterSizeUniformContainerLarger.text = LanguageManager.CurrentLanguage.misc.enemyAlter_uniformLarge;

            GameObject enemyAlterMeta = enemyAlterMenu.Child("Meta Options");
            //Prop
            TextMeshProUGUI enemyAlterMetaTitle = enemyAlterMeta.Child("Title (1)").TMP();
            enemyAlterMetaTitle.text = LanguageManager.CurrentLanguage.misc.enemyAlter_metaTitle;

            TextMeshProUGUI enemyAlterMetaFrozen = enemyAlterMeta.FindPath("Frozen Toggle", "Label").TMP();
            enemyAlterMetaFrozen.text = LanguageManager.CurrentLanguage.misc.enemyAlter_metaFrozen;

            TextMeshProUGUI enemyAlterMetaDisallowManipulation = enemyAlterMeta.FindPath("Disallow Manipulation Toggle", "Label").TMP();
            enemyAlterMetaDisallowManipulation.text = LanguageManager.CurrentLanguage.misc.enemyAlter_metaDisallowManipulation;

            TextMeshProUGUI enemyAlterMetaDisallowFreezing = enemyAlterMeta.FindPath("Disallow Freezing Toggle", "Label").TMP();
            enemyAlterMetaDisallowFreezing.text = LanguageManager.CurrentLanguage.misc.enemyAlter_metaDisallowFreezing;

            //Jumppad
            GameObject enemyAlterJumpPad = enemyAlterMenu.Child("Jump Pad Options");
            TextMeshProUGUI enemyAlterJumpPadTitle = enemyAlterJumpPad.Child("Title (1)").TMP();
            enemyAlterJumpPadTitle.text = LanguageManager.CurrentLanguage.misc.enemyAlter_jumpPadTitle;

            TextMeshProUGUI enemyAlterJumpPadPower = enemyAlterJumpPad.FindPath("Slider", "Title (4)").TMP();
            enemyAlterJumpPadPower.text = LanguageManager.CurrentLanguage.misc.enemyAlter_power;

            //Radiance options
            GameObject enemyAlterRadiance = enemyAlterMenu.Child("Radiance Options");
            TextMeshProUGUI enemyAlterRadianceTitle = enemyAlterRadiance.Child("Title (1)").TMP();
            enemyAlterRadianceTitle.text = LanguageManager.CurrentLanguage.misc.enemyAlter_radianceTitle;

            TextMeshProUGUI enemyAlterRadianceEnable = enemyAlterRadiance.FindPath("Toggle", "Label").TMP();
            enemyAlterRadianceEnable.text = LanguageManager.CurrentLanguage.misc.enemyAlter_radianceEnable;

            //Radiance details
            GameObject enemyAlterRadianceDetails = enemyAlterMenu.FindPath("Radiance Details", "Radiance Settings");
            TextMeshProUGUI enemyAlterRadianceDetailsTier = enemyAlterRadianceDetails.FindPath("Radiance Tier Container", "Title (4)").TMP();
            enemyAlterRadianceDetailsTier.text = LanguageManager.CurrentLanguage.misc.enemyAlter_radianceDetails_tier;

            TextMeshProUGUI enemyAlterRadianceHealth = enemyAlterRadianceDetails.FindPath("Health Multi Container", "Title (4)").TMP();
            enemyAlterRadianceHealth.text = LanguageManager.CurrentLanguage.misc.enemyAlter_radianceHealth_tier;

            TextMeshProUGUI enemyAlterRadianceDamage = enemyAlterRadianceDetails.FindPath("Damage Multi Container", "Title (4)").TMP();
            enemyAlterRadianceDamage.text = LanguageManager.CurrentLanguage.misc.enemyAlter_radianceDamage_tier;

            TextMeshProUGUI enemyAlterRadianceSpeed = enemyAlterRadianceDetails.FindPath("Speed Multi Container", "Title (4)").TMP();
            enemyAlterRadianceSpeed.text = LanguageManager.CurrentLanguage.misc.enemyAlter_radianceSpeed_tier;

            //Close button
            TextMeshProUGUI enemyAlterClose = panel.FindPath("Close Button", "Text").TMP();
            enemyAlterClose.text = LanguageManager.CurrentLanguage.options.save_close;

            //Note: Stuff for jump pads, props and enemy boss HP bars are located in SandboxPatches because of dynamic object creation by the game
        }

        public Sandbox(ref GameObject canvasObj)
        {
            PatchSandboxDupeMenu(ref canvasObj);
            PatchMisc(ref canvasObj);
            PatchAlterMenu();
        }

    }
}
