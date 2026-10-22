using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    public static class Shop
    {
        private static void PatchShopFrontEnd(GameObject shopObject)
        {
            try
            {
                GameObject shopPanel = shopObject.FindPath("Background", "Main Panel");

                //Tip panel
                GameObject tipPanel = shopPanel.Child("Tip of the Day");
                TextMeshProUGUI tipTitle = tipPanel.Child("Title").TMP();
                tipTitle.text = LanguageManager.CurrentLanguage.shop.shop_tipofthedayTitle;

                TextMeshProUGUI tipDescription = tipPanel.FindPath("Panel", "Text Inset", "TipText").TMP();
                string tipDescriptionText = tipDescription.text;
                //V-Rank Check, do nothing if "V-Rank" is in them, otherwise replace by the correct text
                if (tipDescriptionText.Contains("V-Rank")) { tipDescription.text = tipDescriptionText; }
                else { tipDescription.text = StringsParent.GetLevelTip(tipDescriptionText); }

                //--MENU--
                // removed and replaced with SmileOS 2.0 in patch 16
                //TextMeshProUGUI menuText = GetTextMeshProUGUI(GetGameObjectChild(shopObject, "Menu Title"));
                //menuText.text = "--" + LanguageManager.CurrentLanguage.shop.shop_menu + "--";

                //Weapons button
                GameObject mainButtons = shopPanel.FindPath("Main Menu", "Buttons");

                TextMeshProUGUI weaponsButtonTitle = mainButtons.FindPath("WeaponsButton", "Text").TMP();
                weaponsButtonTitle.text = LanguageManager.CurrentLanguage.shop.shop_weapons;

                //Enemies button
                TextMeshProUGUI enemiesButtonTitle = mainButtons.FindPath("EnemiesButton", "Text").TMP();
                enemiesButtonTitle.text = LanguageManager.CurrentLanguage.shop.shop_monsters;

                //CG buttons
                TextMeshProUGUI cgButtonTitle = mainButtons.FindPath("CyberGrindButton", "Text").TMP();
                cgButtonTitle.text = LanguageManager.CurrentLanguage.shop.shop_cybergrind;

                TextMeshProUGUI cgReturnButtonTitle = mainButtons.FindPath("ReturnButton", "Text").TMP();
                cgReturnButtonTitle.text = LanguageManager.CurrentLanguage.shop.shop_returnToMission;

                //Sandbox button
                TextMeshProUGUI sandboxButtonTitle = mainButtons.FindPath("SandboxButton", "Text").TMP();
                sandboxButtonTitle.text = LanguageManager.CurrentLanguage.shop.shop_sandbox;
                
                //Enemies title
                TextMeshProUGUI enemiesTitle = shopPanel.FindPath("Enemies", "Enemies Panel", "Title").TMP();
                enemiesTitle.text = LanguageManager.CurrentLanguage.shop.shop_monsters;
                

                //Sandbox enter description
                GameObject sandboxEnter = shopPanel.FindPath("Sandbox", "Sandbox Panel", "Panel");

                TextMeshProUGUI sandboxTitle = shopPanel.FindPath("Sandbox", "Sandbox Panel", "Title").TMP();
                sandboxTitle.text = LanguageManager.CurrentLanguage.shop.shop_sandbox;

                TextMeshProUGUI sandboxEnterDescription = sandboxEnter.FindPath("Text Inset", "Text").TMP();

                sandboxEnterDescription.text = LanguageManager.CurrentLanguage.shop.shop_sandboxDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_sandboxDescription2;

                TextMeshProUGUI sandboxEnterButton = sandboxEnter.FindPath("Enter Button", "Text").TMP();
                sandboxEnterButton.text = LanguageManager.CurrentLanguage.shop.shop_sandboxEnter;

                //CG enter description
                GameObject cgEnter = shopPanel.FindPath("The Cyber Grind", "Cyber Grind Panel", "Panel");

                TextMeshProUGUI cgEnterTitle = shopPanel.FindPath("The Cyber Grind", "Cyber Grind Panel", "Title").TMP();
                cgEnterTitle.text = LanguageManager.CurrentLanguage.shop.shop_cybergrindEnterTitle;

                TextMeshProUGUI cgEnterDescription = cgEnter.FindPath("Text Inset", "Text").TMP();

                cgEnterDescription.text = LanguageManager.CurrentLanguage.shop.shop_cybergrindDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_cybergrindDescription2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_cybergrindDescription3;

                TextMeshProUGUI cgEnterButton = cgEnter.FindPath("Enter Button", "Text").TMP();
                cgEnterButton.text = LanguageManager.CurrentLanguage.shop.shop_cybergrindEnter;

                //CG exit description
                GameObject cgExit = shopPanel.FindPath("Return from Cyber Grind", "Return from Cyber Grind Panel");

                TextMeshProUGUI cgExitTitle = cgExit.Child("Title").TMP();
                cgExitTitle.text = LanguageManager.CurrentLanguage.shop.shop_cybergrindExitTitle;

                TextMeshProUGUI cgExitButton = cgExit.FindPath("Panel", "Exit Button", "Text").TMP();
                if (GetCurrentSceneName() == "uk_construct")
                {
                    cgExitButton.text = LanguageManager.CurrentLanguage.frontend.mainmenu_quit;
                }
                else
                {
                    cgExitButton.text = LanguageManager.CurrentLanguage.shop.shop_cybergrindExit;
                }

                //Enemies back button 
                TextMeshProUGUI enemiesBackText = shopPanel.FindPath("Enemies", "Back Button", "Text").TMP();
                enemiesBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //EnemyInfo back button
                TextMeshProUGUI enemyInfoBackText = shopPanel.FindPath("Enemies", "Info Screen", "Main Window", "Back Button", "Text").TMP();
                enemyInfoBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Sandbox back button
                TextMeshProUGUI sandboxBackText = shopPanel.FindPath("Sandbox", "Back Button", "Text").TMP();
                sandboxBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Enter CG back text
                TextMeshProUGUI cgEnterBackButtonText = shopPanel.FindPath("The Cyber Grind", "Back Button", "Text").TMP();
                cgEnterBackButtonText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Exit CG back text
                TextMeshProUGUI cgExitBackButtonText = shopPanel.FindPath("Return from Cyber Grind", "Back Button", "Text").TMP();
                cgExitBackButtonText.text = LanguageManager.CurrentLanguage.shop.shop_back;
            }
            catch (Exception e)
            {
                Logging.Error("An error occured while translating shop texts.");
                Logging.Error(e.ToString());
            }

        }
        

        private static void PatchWeapons(GameObject shopObject)
        {
            try
            {
                GameObject shopPanel = shopObject.FindPath("Background", "Main Panel");

                float addWidth = 110f;
                //weapons
                GameObject shopWeaponsObject  = shopPanel.Child("Weapons");
                
                GameObject shopWeaponsButtonsObject = shopPanel.FindPath("Weapons", "Weapons Panel").ChildAt(4);
                
                TextMeshProUGUI weaponTitleText = shopWeaponsObject.FindPath("Weapons Panel", "Menu Title").TMP();
                weaponTitleText.text = LanguageManager.CurrentLanguage.shop.shop_weapons;
                
                TextMeshProUGUI weaponBackText = shopWeaponsButtonsObject.FindPath("BackButton", "Text").TMP();
                weaponBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("maranara_project_prophet") ||
                    BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("plonk.straymode"))
                    return;


                TextMeshProUGUI weaponRevolverText = shopWeaponsButtonsObject.FindPath("RevolverButton", "Text").TMP();
                weaponRevolverText.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRevolver;
                
                TextMeshProUGUI weaponShotgunText = shopWeaponsButtonsObject.FindPath("ShotgunButton", "Text").TMP();
                weaponShotgunText.text = LanguageManager.CurrentLanguage.shop.shop_weaponsShotgun;
                
                TextMeshProUGUI weaponNailgunText = shopWeaponsButtonsObject.FindPath("NailgunButton", "Text").TMP();
                weaponNailgunText.text = LanguageManager.CurrentLanguage.shop.shop_weaponsNailgun;

                //Slight problem - not all the text fits in the box.
                //The longer text is, the more we'll need to reduce the font size to compensate.
                TextMeshProUGUI weaponRailcannonText = shopWeaponsButtonsObject.FindPath("RailcannonButton", "Text").TMP();
                weaponRailcannonText.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRailcannon;

                TextMeshProUGUI rocketLauncherText = shopWeaponsButtonsObject.FindPath("RocketLauncherButton", "Text").TMP();
                rocketLauncherText.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRocketLauncher;

                TextMeshProUGUI weaponArmText = shopWeaponsButtonsObject.FindPath("ArmButton", "Text").TMP();
                weaponArmText.text = LanguageManager.CurrentLanguage.shop.shop_weaponsArms;

                // Revolver
                // Piercer(Blue)
                // Marksman(Green)
                // Sharpshooter(Red)

                //Revolver window and descriptions
                GameObject revolverWindow = shopWeaponsObject.Child("Revolver Window");
                GameObject revolverVariations = revolverWindow.FindPath("Variation Screen", "Variations");

                TextMeshProUGUI revolverWindowTitle = revolverVariations.transform.parent.gameObject.Child("Title").TMP();
                revolverWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRevolver;
                
                //Piercer
                GameObject piercer = revolverVariations.Child("Variation Panel (Blue)");
                TextMeshProUGUI piercerName = piercer.Child("Variation Name").TMP();
                piercerName.text = LanguageManager.CurrentLanguage.shop.shop_revolverPiercer.ToUpper();

                GameObject piercerWindow = revolverWindow.FindPath("Variation Info (Blue)", "Panel");
                TextMeshProUGUI piercerWindowTitle = piercerWindow.transform.parent.gameObject.Child("Title").TMP();
                piercerWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_revolverPiercer;
                TextMeshProUGUI piercerWindowName = piercerWindow.Child("Name").TMP();
                piercerWindowName.enableAutoSizing = true;
                piercerWindowName.fontSizeMax = piercerWindowName.fontSize;
                piercerWindowName.fontSizeMin = 0f;
                piercerWindowName.text = piercerName.text;

                TextMeshProUGUI piercerWindowDescription = piercerWindow.Child("Description").TMP();
                piercerWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_revolverPiercerDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_revolverPiercerDescription2;

                TextMeshProUGUI piercerWindowDescriptionBack = piercerWindow.FindPath("Back Button", "Text").TMP();
                piercerWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Marksman
                GameObject marksman = revolverVariations.Child("Variation Panel (Green)");
                TextMeshProUGUI marksmanName = marksman.Child("Variation Name").TMP();
                marksmanName.text = LanguageManager.CurrentLanguage.shop.shop_revolverMarksman.ToUpper();
                
                GameObject marksmanWindow = revolverWindow.FindPath("Variation Info (Green)", "Panel");
                TextMeshProUGUI marksmanWindowTitle = marksmanWindow.transform.parent.gameObject.Child("Title").TMP();
                marksmanWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_revolverMarksman;
                TextMeshProUGUI marksmanWindowName = marksmanWindow.Child("Name").TMP();
                marksmanWindowName.enableAutoSizing = true;
                marksmanWindowName.fontSizeMax = marksmanWindowName.fontSize;
                marksmanWindowName.fontSizeMin = 0f;
                marksmanWindowName.text = marksmanName.text;

                TextMeshProUGUI marksmanWindowDescription = marksmanWindow.Child("Description").TMP();
                marksmanWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_revolverMarksmanDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_revolverMarksmanDescription2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_revolverMarksmanDescription3;

                TextMeshProUGUI marksmanWindowDescriptionBack = marksmanWindow.FindPath("Back Button", "Text").TMP();
                marksmanWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Sharpshooter
                GameObject sharpshooter = revolverVariations.Child("Variation Panel (Red)");
                TextMeshProUGUI sharpshooterName = sharpshooter.Child("Variation Name").TMP();
                sharpshooterName.text = LanguageManager.CurrentLanguage.shop.shop_revolverSharpshooter.ToUpper();
                
                GameObject sharpshooterWindow = revolverWindow.FindPath("Variation Info (Red)", "Panel");
                TextMeshProUGUI sharpshooterWindowTitle = sharpshooterWindow.transform.parent.gameObject.Child("Title").TMP();
                sharpshooterWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_revolverSharpshooter;
                TextMeshProUGUI sharpshooterWindowName = sharpshooterWindow.Child("Name").TMP();
                sharpshooterWindowName.enableAutoSizing = true;
                sharpshooterWindowName.fontSizeMax = sharpshooterWindowName.fontSize;
                sharpshooterWindowName.fontSizeMin = 0f;
                sharpshooterWindowName.text = sharpshooterName.text;

                TextMeshProUGUI sharpshooterWindowDescription = sharpshooterWindow.Child("Description").TMP();
                sharpshooterWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_revolverSharpshooterDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_revolverSharpshooterDescription2 + "\n\n";

                //just in case.
                TextMeshProUGUI redrevolverBackText = sharpshooterWindow.FindPath("Back Button", "Text").TMP();
                redrevolverBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Revolver info & color tabs
                GameObject revolverExtra = revolverVariations.Child("Info and Color Panel");
                GameObject revolverExtraInfo = revolverExtra.Child("InfoButton");
                GameObject revolverExtraColor = revolverExtra.Child("ColorButton");

                TextMeshProUGUI revolverExtraInfoText = revolverExtraInfo.Child("Text").TMP();
                revolverExtraInfoText.text = LanguageManager.CurrentLanguage.shop.shop_weaponInfo;

                TextMeshProUGUI revolverExtraInfoColors = revolverExtraColor.Child("Text").TMP();
                revolverExtraInfoColors.text = LanguageManager.CurrentLanguage.shop.shop_weaponColors;

                //Revolver lore
                GameObject revolverLore = revolverWindow.Child("Info Screen");
                TextMeshProUGUI revolverLoreName = revolverLore.Child("Title").TMP();
                RectTransform rl = revolverLoreName.GetComponent<RectTransform>();
                rl.sizeDelta = new Vector2(rl.sizeDelta.x + addWidth, rl.sizeDelta.y);
                revolverLoreName.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRevolverInfo;// + info
                TextMeshProUGUI revolverLoreTitle = revolverLore.FindPath("Main Window", "Name").TMP();
                revolverLoreTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRevolver;

                TextMeshProUGUI revolverLoreInfo = revolverLore.FindPath("Main Window", "Scroll View", "Viewport", "Text").TMP();

                revolverLoreInfo.text =
                    "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_data + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver3 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver4 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver5 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_strategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver6 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver7 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_advancedStrategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver8 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver9 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRevolver10;

                TextMeshProUGUI revolverLoreBack = revolverLore.FindPath("Main Window", "Back Button", "Text").TMP();
                revolverLoreBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Revolver preset colors
                GameObject revolverColorWindow = revolverWindow.FindPath("Color Screen", "Main Window");

                TextMeshProUGUI revolverColorWindowTitle = revolverColorWindow.transform.parent.gameObject.Child("Title").TMP();
                RectTransform rc = revolverColorWindowTitle.GetComponent<RectTransform>();
                rc.sizeDelta = new Vector2(rc.sizeDelta.x + addWidth, rc.sizeDelta.y);
                revolverColorWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRevolverColors; //+ color

                GameObject revolverTemplates = revolverColorWindow.FindPath("Window", "Presets");
                TextMeshProUGUI revolverTemplate1 = revolverTemplates.FindPath("Template 1", "Text").TMP();
                TextMeshProUGUI revolverTemplate2 = revolverTemplates.FindPath("Template 2", "Text").TMP();
                TextMeshProUGUI revolverTemplate3 = revolverTemplates.FindPath("Template 3", "Text").TMP();
                TextMeshProUGUI revolverTemplate4 = revolverTemplates.FindPath("Template 4", "Text").TMP();
                TextMeshProUGUI revolverTemplate5 = revolverTemplates.FindPath("Template 5", "Text").TMP();

                revolverTemplate1.text = LanguageManager.CurrentLanguage.shop.shop_revolverPreset1;
                revolverTemplate2.text = LanguageManager.CurrentLanguage.shop.shop_revolverPreset2;
                revolverTemplate3.text = LanguageManager.CurrentLanguage.shop.shop_revolverPreset3;
                revolverTemplate4.text = LanguageManager.CurrentLanguage.shop.shop_revolverPreset4;
                revolverTemplate5.text = LanguageManager.CurrentLanguage.shop.shop_revolverPreset5;

                /*  Patch GunColorTypeGetter.ToggleAlternate() instead
                TextMeshProUGUI revolverColorSwitchToAlternative = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(revolverColorWindow, "Standard"),"AlternateButton"),"Text"));
                revolverColorSwitchToAlternative.text = LanguageManager.CurrentLanguage.shop.shop_colorsAlternative;

                TextMeshProUGUI revolverColorSwitchToStandard = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(revolverColorWindow, "Alternate"), "AlternateButton"), "Text"));
                revolverColorSwitchToStandard.text = LanguageManager.CurrentLanguage.shop.shop_colorsAlternative;
                */

                GameObject revolverTypeButtons = revolverTemplates.transform.parent.gameObject.Child("Type Selection");
                TextMeshProUGUI revolverColorPreset = revolverTypeButtons.FindPath("Preset Button", "Text").TMP();
                revolverColorPreset.text = LanguageManager.CurrentLanguage.shop.shop_colorsPreset;

                TextMeshProUGUI revolverColorCustom = revolverTypeButtons.FindPath("Custom Button", "Text").TMP();
                revolverColorCustom.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustom;

                TextMeshProUGUI revolverColorDone = revolverTemplates.transform.parent.gameObject.FindPath("Done", "Text").TMP();
                revolverColorDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone;

                //Revolver custom color unlock prompt
                TextMeshProUGUI revolverCustomColorPrompt = revolverTemplates.transform.parent.gameObject.FindPath("Custom", "Locked", "Text").TMP();
                revolverCustomColorPrompt.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustomUnlockPrompt + " " + LanguageManager.CurrentLanguage.shop.shop_weaponsRevolver;

                // SHOTGUN
                // Core Eject(Blue)
                // Pump Charge(Green)
                // Sawed-On(Red)

                //Shotgun window and descriptions
                GameObject shotgunWindow = shopWeaponsObject.Child("Shotgun Window");
                GameObject shotgunVariations = shotgunWindow.FindPath("Variation Screen", "Variations");

                TextMeshProUGUI shotgunWindowTitle = shotgunVariations.transform.parent.gameObject.Child("Title").TMP();
                shotgunWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsShotgun;

                //Core Eject
                GameObject coreEject = shotgunVariations.Child("Variation Panel (Blue)");
                TextMeshProUGUI coreEjectName = coreEject.Child("Variation Name").TMP();
                coreEjectName.text = LanguageManager.CurrentLanguage.shop.shop_shotgunCoreEject.ToUpper();

                GameObject coreEjectWindow = shotgunWindow.FindPath("Variation Info (Blue)", "Panel");
                TextMeshProUGUI coreEjectWindowTitle = coreEjectWindow.transform.parent.gameObject.Child("Title").TMP();
                coreEjectWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_shotgunCoreEject;
                TextMeshProUGUI coreEjectWindowName = coreEjectWindow.Child("Name").TMP();
                coreEjectWindowName.enableAutoSizing = true;
                coreEjectWindowName.fontSizeMax = coreEjectWindowName.fontSize;
                coreEjectWindowName.fontSizeMin = 0f;
                coreEjectWindowName.text = coreEjectName.text;

                TextMeshProUGUI coreEjectWindowDescription = coreEjectWindow.Child("Description").TMP();
                coreEjectWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_shotgunCoreEjectDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_shotgunCoreEjectDescription2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_shotgunCoreEjectDescription3;

                TextMeshProUGUI coreEjectWindowDescriptionBack = coreEjectWindow.FindPath("Back Button", "Text").TMP();
                coreEjectWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Pump Charge
                GameObject pumpCharge = shotgunVariations.Child("Variation Panel (Green)");
                TextMeshProUGUI pumpChargeName = pumpCharge.Child("Variation Name").TMP();
                pumpChargeName.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPumpCharge.ToUpper();

                GameObject pumpChargeWindow = shotgunWindow.FindPath("Variation Info (Green)", "Panel");
                TextMeshProUGUI pumpChargeWindowTitle = pumpChargeWindow.transform.parent.gameObject.Child("Title").TMP();
                pumpChargeWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPumpCharge;
                TextMeshProUGUI pumpChargeWindowName = pumpChargeWindow.Child("Name").TMP();
                pumpChargeWindowName.enableAutoSizing = true;
                pumpChargeWindowName.fontSizeMax = pumpChargeWindowName.fontSize;
                pumpChargeWindowName.fontSizeMin = 0f;
                pumpChargeWindowName.text = pumpChargeName.text;

                TextMeshProUGUI pumpChargeWindowDescription = pumpChargeWindow.Child("Description").TMP();
                pumpChargeWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPumpChargeDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_shotgunPumpChargeDescription2;

                TextMeshProUGUI pumpChargeWindowDescriptionBack = pumpChargeWindow.FindPath("Back Button", "Text").TMP();
                pumpChargeWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Sawed-On
                GameObject sawedOn = shotgunVariations.Child("Variation Panel (Red)");
                TextMeshProUGUI sawedOnName = sawedOn.Child("Variation Name").TMP();
                sawedOnName.text = LanguageManager.CurrentLanguage.shop.shop_shotgunSawedOn.ToUpper();

                GameObject sawedOnWindow = shotgunWindow.FindPath("Variation Info (Red)", "Panel");
                TextMeshProUGUI sawedOnWindowTitle = sawedOnWindow.transform.parent.gameObject.Child("Title").TMP();
                sawedOnWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_shotgunSawedOn;
                TextMeshProUGUI sawedOnWindowName = sawedOnWindow.Child("Name").TMP();
                sawedOnWindowName.enableAutoSizing = true;
                sawedOnWindowName.fontSizeMax = sawedOnWindowName.fontSize;
                sawedOnWindowName.fontSizeMin = 0f;
                sawedOnWindowName.text = sawedOnName.text;

                TextMeshProUGUI sawedOnWindowDescription = sawedOnWindow.Child("Description").TMP();
                sawedOnWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_shotgunSawedOnDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_shotgunSawedOnDescription2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_shotgunSawedOnDescription3;

                TextMeshProUGUI sawedOnWindowDescriptionBack = sawedOnWindow.FindPath("Back Button", "Text").TMP();
                sawedOnWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Shotgun info & color tabs
                GameObject shotgunExtra = shotgunVariations.Child("Info and Color Panel");
                GameObject shotgunExtraInfo = shotgunExtra.Child("InfoButton");
                GameObject shotgunExtraColor = shotgunExtra.Child("ColorButton");

                TextMeshProUGUI shotgunExtraInfoText = shotgunExtraInfo.Child("Text").TMP();
                shotgunExtraInfoText.text = LanguageManager.CurrentLanguage.shop.shop_weaponInfo;

                TextMeshProUGUI shotgunExtraInfoColors = shotgunExtraColor.Child("Text").TMP();
                shotgunExtraInfoColors.text = LanguageManager.CurrentLanguage.shop.shop_weaponColors;

                //Shotgun lore
                GameObject shotgunLore = shotgunWindow.FindPath("Info Screen", "Main Window");
                TextMeshProUGUI shotgunLoreName = shotgunLore.transform.parent.gameObject.Child("Title").TMP();
                RectTransform sl = shotgunLoreName.GetComponent<RectTransform>();
                sl.sizeDelta = new Vector2(sl.sizeDelta.x + addWidth, sl.sizeDelta.y);
                shotgunLoreName.text = LanguageManager.CurrentLanguage.shop.shop_weaponsShotgunInfo;
                TextMeshProUGUI shotgunLoreTitle = shotgunLore.Child("Name").TMP();
                shotgunLoreTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsShotgun;

                TextMeshProUGUI shotgunLoreInfo = shotgunLore.FindPath("Scroll View", "Viewport", "Text").TMP();

                shotgunLoreInfo.text =
                    "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_data + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun3 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun4 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_strategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun5 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun6 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_advancedStrategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun7 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun8 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreShotgun9;

                TextMeshProUGUI shotgunLoreBack = shotgunLore.FindPath("Back Button", "Text").TMP();
                shotgunLoreBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Shotgun preset colors
                GameObject shotgunColorWindow = shotgunWindow.FindPath("Color Screen", "Main Window");

                TextMeshProUGUI shotgunColorWindowTitle = shotgunColorWindow.transform.parent.gameObject.Child("Title").TMP();
                RectTransform sc = shotgunColorWindowTitle.GetComponent<RectTransform>();
                sc.sizeDelta = new Vector2(sc.sizeDelta.x + addWidth, sc.sizeDelta.y);
                shotgunColorWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsShotgunColors; //+ color

                GameObject shotgunTemplates = shotgunColorWindow.FindPath("Window", "Presets");
                TextMeshProUGUI shotgunTemplate1 = shotgunTemplates.FindPath("Template 1", "Text").TMP();
                TextMeshProUGUI shotgunTemplate2 = shotgunTemplates.FindPath("Template 2", "Text").TMP();
                TextMeshProUGUI shotgunTemplate3 = shotgunTemplates.FindPath("Template 3", "Text").TMP();
                TextMeshProUGUI shotgunTemplate4 = shotgunTemplates.FindPath("Template 4", "Text").TMP();
                TextMeshProUGUI shotgunTemplate5 = shotgunTemplates.FindPath("Template 5", "Text").TMP();

                shotgunTemplate1.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPreset1;
                shotgunTemplate2.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPreset2;
                shotgunTemplate3.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPreset3;
                shotgunTemplate4.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPreset4;
                shotgunTemplate5.text = LanguageManager.CurrentLanguage.shop.shop_shotgunPreset5;

                GameObject shotgunTypeButtons = shotgunTemplates.transform.parent.gameObject.Child("Type Selection");
                TextMeshProUGUI shotgunColorPreset = shotgunTypeButtons.FindPath("Preset Button", "Text").TMP();
                shotgunColorPreset.text = LanguageManager.CurrentLanguage.shop.shop_colorsPreset;

                TextMeshProUGUI shotgunColorCustom = shotgunTypeButtons.FindPath("Custom Button", "Text").TMP();
                shotgunColorCustom.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustom;

                TextMeshProUGUI shotgunColorDone = shotgunTemplates.transform.parent.gameObject.FindPath("Done", "Text").TMP();
                shotgunColorDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone;

                //shotgun custom color unlock prompt
                TextMeshProUGUI shotgunCustomColorPrompt = shotgunTemplates.transform.parent.gameObject.FindPath("Custom", "Locked", "Text").TMP();
                shotgunCustomColorPrompt.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustomUnlockPrompt + " " + LanguageManager.CurrentLanguage.shop.shop_weaponsShotgun;

                // Nailgun
                // Attractor(Blue)
                // Overheat(Green)
                // Jumpstart(Red)

                //Nailgun window and descriptions
                GameObject nailgunWindow = shopWeaponsObject.Child("Nailgun Window");
                GameObject nailgunVariations = nailgunWindow.FindPath("Variation Screen", "Variations");

                TextMeshProUGUI nailgunWindowTitle = nailgunVariations.transform.parent.gameObject.Child("Title").TMP();
                nailgunWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsNailgun;

                //Attractor
                GameObject attractor = nailgunVariations.Child("Variation Panel (Blue)");
                TextMeshProUGUI attractorName = attractor.Child("Variation Name").TMP();
                attractorName.text = LanguageManager.CurrentLanguage.shop.shop_nailgunMagnet.ToUpper();

                GameObject attractorWindow = nailgunWindow.FindPath("Variation Info (Blue)", "Panel");
                TextMeshProUGUI attractorWindowTitle = attractorWindow.transform.parent.gameObject.Child("Title").TMP();
                attractorWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_nailgunMagnet;
                TextMeshProUGUI attractorWindowName = attractorWindow.Child("Name").TMP();
                attractorWindowName.enableAutoSizing = true;
                attractorWindowName.fontSizeMax = attractorWindowName.fontSize;
                attractorWindowName.fontSizeMin = 0f;
                attractorWindowName.text = attractorName.text;

                TextMeshProUGUI attractorWindowDescription = attractorWindow.Child("Description").TMP();
                attractorWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_nailgunMagnetDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_nailgunMagnetDescription2;

                TextMeshProUGUI attractorWindowDescriptionBack = attractorWindow.FindPath("Back Button", "Text").TMP();
                attractorWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Overheat
                GameObject overheat = nailgunVariations.Child("Variation Panel (Green)");
                TextMeshProUGUI overheatName = overheat.Child("Variation Name").TMP();
                overheatName.text = LanguageManager.CurrentLanguage.shop.shop_nailgunOverheat.ToUpper();

                GameObject overheatWindow = nailgunWindow.FindPath("Variation Info (Green)", "Panel");
                TextMeshProUGUI overheatWindowTitle = overheatWindow.transform.parent.gameObject.Child("Title").TMP();
                overheatWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_nailgunOverheat;
                TextMeshProUGUI overheatWindowName = overheatWindow.Child("Name").TMP();
                overheatWindowName.enableAutoSizing = true;
                overheatWindowName.fontSizeMax = overheatWindowName.fontSize;
                overheatWindowName.fontSizeMin = 0f;
                overheatWindowName.text = overheatName.text;
                

                TextMeshProUGUI overheatWindowDescription = overheatWindow.Child("Description").TMP();
                overheatWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_nailgunOverheatDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_nailgunOverheatDescription2;

                TextMeshProUGUI overheatWindowDescriptionBack = overheatWindow.FindPath("Back Button", "Text").TMP();
                overheatWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Jumpstarter
                GameObject jumpStart = nailgunVariations.Child("Variation Panel (Red)");
                TextMeshProUGUI jumpStartName = jumpStart.Child("Variation Name").TMP();
                jumpStartName.text = LanguageManager.CurrentLanguage.shop.shop_nailgunJumpStart.ToUpper();

                GameObject jumpStartWindow = nailgunWindow.FindPath("Variation Info (Red)", "Panel");
                TextMeshProUGUI jumpStartWindowTitle = jumpStartWindow.transform.parent.gameObject.Child("Title").TMP();
                jumpStartWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_nailgunJumpStart;
                TextMeshProUGUI jumpStartWindowName = jumpStartWindow.Child("Name").TMP();
                jumpStartWindowName.enableAutoSizing = true;
                jumpStartWindowName.fontSizeMax = jumpStartWindowName.fontSize;
                jumpStartWindowName.fontSizeMin = 0f;
                jumpStartWindowName.text = jumpStartName.text;

                TextMeshProUGUI jumpStartWindowDescription = jumpStartWindow.Child("Description").TMP();
                jumpStartWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_nailgunJumpStartDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_nailgunJumpStartDescription2;

                TextMeshProUGUI jumpStartWindowDescriptionBack = jumpStartWindow.FindPath("Back Button", "Text").TMP();
                jumpStartWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Nailgun info & color tabs
                GameObject nailgunExtra = nailgunVariations.Child("Info and Color Panel");
                GameObject nailgunExtraInfo = nailgunExtra.Child("InfoButton");
                GameObject nailgunExtraColor = nailgunExtra.Child("ColorButton");

                TextMeshProUGUI nailgunExtraInfoText = nailgunExtraInfo.Child("Text").TMP();
                nailgunExtraInfoText.text = LanguageManager.CurrentLanguage.shop.shop_weaponInfo;

                TextMeshProUGUI nailgunExtraInfoColors = nailgunExtraColor.Child("Text").TMP();
                nailgunExtraInfoColors.text = LanguageManager.CurrentLanguage.shop.shop_weaponColors;

                //Nailgun lore
                GameObject nailgunLore = nailgunWindow.FindPath("Info Screen", "Main Window");
                TextMeshProUGUI nailgunLoreName = nailgunLore.transform.parent.gameObject.Child("Title").TMP();
                RectTransform nl = nailgunLoreName.GetComponent<RectTransform>();
                nl.sizeDelta = new Vector2(nl.sizeDelta.x + addWidth, nl.sizeDelta.y);
                nailgunLoreName.text = LanguageManager.CurrentLanguage.shop.shop_weaponsNailgunInfo;
                TextMeshProUGUI nailgunLoreTitle = nailgunLore.Child("Name").TMP();
                nailgunLoreTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsNailgun;

                TextMeshProUGUI NailgunLoreInfo = nailgunLore.FindPath("Scroll View", "Viewport", "Text").TMP();

                NailgunLoreInfo.text =
                    "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_data + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun3 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun4 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_strategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun5 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun6 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun7 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_advancedStrategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun8 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreNailgun9;

                TextMeshProUGUI NailgunLoreBack = nailgunLore.FindPath("Back Button", "Text").TMP();
                NailgunLoreBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //nailgun preset colors
                GameObject nailgunColorWindow = nailgunWindow.FindPath("Color Screen", "Main Window");

                TextMeshProUGUI nailgunColorWindowTitle = nailgunColorWindow.transform.parent.gameObject.Child("Title").TMP();
                RectTransform nc = nailgunColorWindowTitle.GetComponent<RectTransform>();
                nc.sizeDelta = new Vector2(nc.sizeDelta.x + addWidth, nc.sizeDelta.y);
                nailgunColorWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsNailgunColors; //+ color

                GameObject nailgunTemplates = nailgunColorWindow.FindPath("Window", "Presets");
                TextMeshProUGUI nailgunTemplate1 = nailgunTemplates.FindPath("Template 1", "Text").TMP();
                TextMeshProUGUI nailgunTemplate2 = nailgunTemplates.FindPath("Template 2", "Text").TMP();
                TextMeshProUGUI nailgunTemplate3 = nailgunTemplates.FindPath("Template 3", "Text").TMP();
                TextMeshProUGUI nailgunTemplate4 = nailgunTemplates.FindPath("Template 4", "Text").TMP();
                TextMeshProUGUI nailgunTemplate5 = nailgunTemplates.FindPath("Template 5", "Text").TMP();

                nailgunTemplate1.text = LanguageManager.CurrentLanguage.shop.shop_nailgunPreset1;
                nailgunTemplate2.text = LanguageManager.CurrentLanguage.shop.shop_nailgunPreset2;
                nailgunTemplate3.text = LanguageManager.CurrentLanguage.shop.shop_nailgunPreset3;
                nailgunTemplate4.text = LanguageManager.CurrentLanguage.shop.shop_nailgunPreset4;
                nailgunTemplate5.text = LanguageManager.CurrentLanguage.shop.shop_nailgunPreset5;

                GameObject nailgunTypeButtons = nailgunTemplates.transform.parent.gameObject.Child("Type Selection");
                TextMeshProUGUI nailgunColorPreset = nailgunTypeButtons.FindPath("Preset Button", "Text").TMP();
                nailgunColorPreset.text = LanguageManager.CurrentLanguage.shop.shop_colorsPreset;

                TextMeshProUGUI nailgunColorCustom = nailgunTypeButtons.FindPath("Custom Button", "Text").TMP();
                nailgunColorCustom.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustom;

                TextMeshProUGUI nailgunColorDone = nailgunTemplates.transform.parent.gameObject.FindPath("Done", "Text").TMP();
                nailgunColorDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone;

                //nailgun custom color unlock prompt
                TextMeshProUGUI nailgunCustomColorPrompt = nailgunTemplates.transform.parent.gameObject.FindPath("Custom", "Locked", "Text").TMP();
                nailgunCustomColorPrompt.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustomUnlockPrompt + " " + LanguageManager.CurrentLanguage.shop.shop_weaponsNailgun;

                // Railcannon
                // Electric(Blue)
                // Screwdriver(Green)
                // Malicious(Red)

                //Railcannon window and descriptions
                GameObject railcannonWindow = shopWeaponsObject.Child("Railcannon Window");
                GameObject railcannonVariations = railcannonWindow.FindPath("Variation Screen", "Variations");

                TextMeshProUGUI railcannonWindowTitle = railcannonVariations.transform.parent.gameObject.Child("Title").TMP();
                railcannonWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRailcannon;

                //Electric
                GameObject electric = railcannonVariations.Child("Variation Panel (Blue)");
                TextMeshProUGUI electricName = electric.Child("Variation Name").TMP();
                electricName.text = LanguageManager.CurrentLanguage.shop.shop_railcannonElectric.ToUpper();

                GameObject electricWindow = railcannonWindow.FindPath("Variation Info (Blue)", "Panel");
                TextMeshProUGUI electricWindowTitle = electricWindow.transform.parent.gameObject.Child("Title").TMP();
                electricWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_railcannonElectric;
                TextMeshProUGUI electricWindowName = electricWindow.Child("Name").TMP();
                electricWindowName.enableAutoSizing = true;
                electricWindowName.fontSizeMax = electricWindowName.fontSize;
                electricWindowName.fontSizeMin = 0f;
                electricWindowName.text = electricName.text;

                TextMeshProUGUI electricWindowDescription = electricWindow.Child("Description").TMP();
                electricWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_railcannonElectricDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_railcannonElectricDescription2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_railcannonElectricDescription3;

                TextMeshProUGUI electricWindowDescriptionBack = electricWindow.FindPath("Back Button", "Text").TMP();
                electricWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Screwdriver
                GameObject screwdriver = railcannonVariations.Child("Variation Panel (Green)");
                TextMeshProUGUI screwdriverName = screwdriver.Child("Variation Name").TMP();
                screwdriverName.text = LanguageManager.CurrentLanguage.shop.shop_railcannonScrewdriver.ToUpper();

                GameObject screwdriverWindow = railcannonWindow.FindPath("Variation Info (Green)", "Panel");
                TextMeshProUGUI screwdriverWindowTitle = screwdriverWindow.transform.parent.gameObject.Child("Title").TMP();
                screwdriverWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_railcannonScrewdriver;
                TextMeshProUGUI screwdriverWindowName = screwdriverWindow.Child("Name").TMP();
                screwdriverWindowName.enableAutoSizing = true;
                screwdriverWindowName.fontSizeMax = screwdriverWindowName.fontSize;
                screwdriverWindowName.fontSizeMin = 0f;
                screwdriverWindowName.text = screwdriverName.text;

                TextMeshProUGUI screwdriverWindowDescription = screwdriverWindow.Child("Description").TMP();
                screwdriverWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_railcannonScrewdriverDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_railcannonScrewdriverDescription2;

                TextMeshProUGUI screwdriverWindowDescriptionBack = screwdriverWindow.FindPath("Back Button", "Text").TMP();
                screwdriverWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Malicious
                GameObject malicious = railcannonVariations.Child("Variation Panel (Red)");
                TextMeshProUGUI maliciousName = malicious.Child("Variation Name").TMP();
                maliciousName.text = LanguageManager.CurrentLanguage.shop.shop_railcannonMalicious.ToUpper();

                GameObject maliciousWindow = railcannonWindow.FindPath("Variation Info (Red)", "Panel");
                TextMeshProUGUI maliciousWindowTitle = maliciousWindow.transform.parent.gameObject.Child("Title").TMP();
                maliciousWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_railcannonMalicious;
                TextMeshProUGUI maliciousWindowName = maliciousWindow.Child("Name").TMP();
                maliciousWindowName.enableAutoSizing = true;
                maliciousWindowName.fontSizeMax = maliciousWindowName.fontSize;
                maliciousWindowName.fontSizeMin = 0f;
                maliciousWindowName.text = maliciousName.text;

                TextMeshProUGUI maliciousWindowDescription = maliciousWindow.Child("Description").TMP();
                maliciousWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_railcannonMaliciousDescription1 + "\n\n"
                    +  LanguageManager.CurrentLanguage.shop.shop_railcannonMaliciousDescription2;

                TextMeshProUGUI maliciousWindowDescriptionBack = maliciousWindow.FindPath("Back Button", "Text").TMP();
                maliciousWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Railcannon info & color tabs
                GameObject railcannonExtra = railcannonVariations.Child("Info and Color Panel");
                GameObject railcannonExtraInfo = railcannonExtra.Child("InfoButton");
                GameObject railcannonExtraColor = railcannonExtra.Child("ColorButton");

                TextMeshProUGUI railcannonExtraInfoText = railcannonExtraInfo.Child("Text").TMP();
                railcannonExtraInfoText.text = LanguageManager.CurrentLanguage.shop.shop_weaponInfo;

                TextMeshProUGUI railcannonExtraInfoColors = railcannonExtraColor.Child("Text").TMP();
                railcannonExtraInfoColors.text = LanguageManager.CurrentLanguage.shop.shop_weaponColors;

                //Railcannon lore
                GameObject railcannonLore = railcannonWindow.FindPath("Info Screen", "Main Window");
                TextMeshProUGUI railcannonLoreName = railcannonLore.transform.parent.gameObject.Child("Title").TMP();
                RectTransform rcl = railcannonLoreName.GetComponent<RectTransform>();
                rcl.sizeDelta = new Vector2(rcl.sizeDelta.x + addWidth, rcl.sizeDelta.y);
                railcannonLoreName.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRailcannonInfo;
                TextMeshProUGUI railcannonLoreTitle = railcannonLore.Child("Name").TMP();
                railcannonLoreTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRailcannon;

                TextMeshProUGUI railcannonLoreInfo = railcannonLore.FindPath("Scroll View", "Viewport", "Text").TMP();

                railcannonLoreInfo.text =
                     "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_data + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon3 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon4 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_strategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon5 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon6 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_advancedStrategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon7 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon8 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRailcannon9;

                TextMeshProUGUI railcannonLoreBack = railcannonLore.FindPath("Back Button", "Text").TMP();
                railcannonLoreBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Railcannon preset colors
                GameObject railcannonColorWindow = railcannonWindow.FindPath("Color Screen", "Main Window");

                TextMeshProUGUI railcannonColorWindowTitle = railcannonColorWindow.transform.parent.gameObject.Child("Title").TMP();
                RectTransform rcc = railcannonColorWindowTitle.GetComponent<RectTransform>();
                rcc.sizeDelta = new Vector2(rcc.sizeDelta.x + addWidth, rcc.sizeDelta.y);
                railcannonColorWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRailcannonColors; //+ color

                GameObject railcannonTemplates = railcannonColorWindow.FindPath("Window", "Presets");
                TextMeshProUGUI railcannonTemplate1 = railcannonTemplates.FindPath("Template 1", "Text").TMP();
                TextMeshProUGUI railcannonTemplate2 = railcannonTemplates.FindPath("Template 2", "Text").TMP();
                TextMeshProUGUI railcannonTemplate3 = railcannonTemplates.FindPath("Template 3", "Text").TMP();
                TextMeshProUGUI railcannonTemplate4 = railcannonTemplates.FindPath("Template 4", "Text").TMP();
                TextMeshProUGUI railcannonTemplate5 = railcannonTemplates.FindPath("Template 5", "Text").TMP();

                railcannonTemplate1.text = LanguageManager.CurrentLanguage.shop.shop_railcannonPreset1;
                railcannonTemplate2.text = LanguageManager.CurrentLanguage.shop.shop_railcannonPreset2;
                railcannonTemplate3.text = LanguageManager.CurrentLanguage.shop.shop_railcannonPreset3;
                railcannonTemplate4.text = LanguageManager.CurrentLanguage.shop.shop_railcannonPreset4;
                railcannonTemplate5.text = LanguageManager.CurrentLanguage.shop.shop_railcannonPreset5;

                GameObject railcannonTypeButtons = railcannonTemplates.transform.parent.gameObject.Child("Type Selection");
                TextMeshProUGUI railcannonColorPreset = railcannonTypeButtons.FindPath("Preset Button", "Text").TMP();
                railcannonColorPreset.text = LanguageManager.CurrentLanguage.shop.shop_colorsPreset;

                TextMeshProUGUI railcannonColorCustom = railcannonTypeButtons.FindPath("Custom Button", "Text").TMP();
                railcannonColorCustom.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustom;

                TextMeshProUGUI railcannonColorDone = railcannonTemplates.transform.parent.gameObject.FindPath("Done", "Text").TMP();
                railcannonColorDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone;

                //railcannon custom color unlock prompt
                TextMeshProUGUI railcannonCustomColorPrompt = railcannonTemplates.transform.parent.gameObject.FindPath("Custom", "Locked", "Text").TMP();
                railcannonCustomColorPrompt.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustomUnlockPrompt + " " + LanguageManager.CurrentLanguage.shop.shop_weaponsRailcannon;

                // Rocket Launcher
                // Freezeframe(Blue)
                // S.R.S Cannon(Green)
                // Firestarter(Red)

                //Rocket launcher window & descriptions
                GameObject rocketlauncherWindow = shopWeaponsObject.Child("Rocket Launcher Window");
                GameObject rocketlauncherVariations = rocketlauncherWindow.FindPath("Variation Screen", "Variations");

                TextMeshProUGUI rocketlauncherWindowTitle = rocketlauncherVariations.transform.parent.gameObject.Child("Title").TMP();
                rocketlauncherWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRocketLauncher;

                //Freezeframe
                GameObject freezeframe = rocketlauncherVariations.Child("Variation Panel (Blue)");
                TextMeshProUGUI freezeframeName = freezeframe.Child("Variation Name").TMP();
                freezeframeName.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFreeze.ToUpper();

                GameObject freezeframeInfo = rocketlauncherWindow.FindPath("Variation Info (Blue)", "Panel");
                TextMeshProUGUI freezeframeInfoTitle = freezeframeInfo.transform.parent.gameObject.Child("Title").TMP();
                freezeframeInfoTitle.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFreeze;
                TextMeshProUGUI freezeframeWindowName = freezeframeInfo.Child("Name").TMP();
                freezeframeWindowName.enableAutoSizing = true;
                freezeframeWindowName.fontSizeMax = freezeframeWindowName.fontSize;
                freezeframeWindowName.fontSizeMin = 0f;
                freezeframeWindowName.text = freezeframeName.text;
                TextMeshProUGUI freezeframeDescription = freezeframeInfo.Child("Description").TMP();
                freezeframeDescription.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFreezeDescription1 + "\n\n" + 
                LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFreezeDescription2;

                TextMeshProUGUI freezeframeDescriptionBack = freezeframeInfo.FindPath("Back Button", "Text").TMP();
                freezeframeDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Rocket Launcher green variation
                GameObject srsCannon = rocketlauncherVariations.Child("Variation Panel (Green)");
                TextMeshProUGUI srsCannonName = srsCannon.Child("Variation Name").TMP();
                srsCannonName.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherSrsCannon.ToUpper();
                
                GameObject srsCannonInfo = rocketlauncherWindow.FindPath("Variation Info (Green)", "Panel");
                TextMeshProUGUI srsCannonInfoTitle = srsCannonInfo.transform.parent.gameObject.Child("Title").TMP();
                srsCannonInfoTitle.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherSrsCannon;
                TextMeshProUGUI srsCannonWindowName = srsCannonInfo.Child("Name").TMP();
                srsCannonWindowName.enableAutoSizing = true;
                srsCannonWindowName.fontSizeMax = srsCannonWindowName.fontSize;
                srsCannonWindowName.fontSizeMin = 0f;
                srsCannonWindowName.text = srsCannonName.text;
                TextMeshProUGUI srsCannonInfoDescription = srsCannonInfo.Child("Description").TMP();
                srsCannonInfoDescription.text =
                    LanguageManager.CurrentLanguage.shop.shop_rocketLauncherSrsCannonDescription1 + "\n\n" +
                    LanguageManager.CurrentLanguage.shop.shop_rocketLauncherSrsCannonDescription2 + "\n\n" +
                    LanguageManager.CurrentLanguage.shop.shop_rocketLauncherSrsCannonDescription3;

                TextMeshProUGUI srsCannonBackText = srsCannonInfo.FindPath("Back Button", "Text").TMP();
                srsCannonBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Firestarter a.k.a Gasoline
                GameObject fireStarter = rocketlauncherVariations.Child("Variation Panel (Red)");
                TextMeshProUGUI fireStarterName = fireStarter.Child("Variation Name").TMP();
                fireStarterName.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFireStarter.ToUpper();

                GameObject fireStarterInfo = rocketlauncherWindow.FindPath("Variation Info (Red)", "Panel");
                TextMeshProUGUI fireStarterInfoTitle = fireStarterInfo.transform.parent.gameObject.Child("Title").TMP();
                fireStarterInfoTitle.text = LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFireStarter;
                TextMeshProUGUI fireStarterInfoName = fireStarterInfo.Child("Name").TMP();
                fireStarterInfoName.enableAutoSizing = true;
                fireStarterInfoName.fontSizeMax = fireStarterInfoName.fontSize;
                fireStarterInfoName.fontSizeMin = 0f;
                fireStarterInfoName.text = fireStarterName.text;
                TextMeshProUGUI fireStarterInfoDescription = fireStarterInfo.Child("Description").TMP();
                fireStarterInfoDescription.text =
                    LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFireStarterDescription1 + "\n\n" +
                    LanguageManager.CurrentLanguage.shop.shop_rocketLauncherFireStarterDescription2;
                TextMeshProUGUI fireStarterBackText = fireStarterInfo.FindPath("Back Button", "Text").TMP();
                fireStarterBackText.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //Rocket launcher info & color tabs
                GameObject rocketlauncherExtra = rocketlauncherVariations.Child("Info and Color Panel");
                GameObject rocketlauncherExtraInfo = rocketlauncherExtra.Child("InfoButton");
                GameObject rocketlauncherExtraColor = rocketlauncherExtra.Child("ColorButton");

                TextMeshProUGUI rocketlauncherExtraInfoText = rocketlauncherExtraInfo.Child("Text").TMP();
                rocketlauncherExtraInfoText.text = LanguageManager.CurrentLanguage.shop.shop_weaponInfo;

                TextMeshProUGUI rocketlauncherExtraInfoColors = rocketlauncherExtraColor.Child("Text").TMP();
                rocketlauncherExtraInfoColors.text = LanguageManager.CurrentLanguage.shop.shop_weaponColors;

                //RocketLauncher lore
                GameObject rocketlauncherLore = rocketlauncherWindow.FindPath("Info Screen", "Main Window");
                TextMeshProUGUI rocketlauncherLoreName = rocketlauncherLore.transform.parent.gameObject.Child("Title").TMP();
                RectTransform rll = rocketlauncherLoreName.GetComponent<RectTransform>();
                rll.sizeDelta = new Vector2(rll.sizeDelta.x + addWidth, rll.sizeDelta.y);
                rocketlauncherLoreName.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRocketLauncherInfo;
                TextMeshProUGUI rocketlauncherLoreTitle = rocketlauncherLore.Child("Name").TMP();
                rocketlauncherLoreTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRocketLauncher;

                TextMeshProUGUI rocketlauncherLoreInfo = rocketlauncherLore.FindPath("Scroll View", "Viewport", "Text").TMP();

                rocketlauncherLoreInfo.text =
                      "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_data + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher2 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher3 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher4 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher5 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher6 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher7 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_strategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher8 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher9 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher10 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher11 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher12 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher13 + "\n\n"
                    + "<color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_advancedStrategy + "</color>\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher14 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher15 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_loreRocketLauncher16;

                TextMeshProUGUI rocketlauncherLoreBack = rocketlauncherLore.FindPath("Back Button", "Text").TMP();
                rocketlauncherLoreBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //RocketLauncher preset colors
                GameObject rocketlauncherColorWindow = rocketlauncherWindow.FindPath("Color Screen", "Main Window");

                TextMeshProUGUI rocketlauncherColorWindowTitle = rocketlauncherColorWindow.transform.parent.gameObject.Child("Title").TMP();
                RectTransform rlc = rocketlauncherColorWindowTitle.GetComponent<RectTransform>();
                rlc.sizeDelta = new Vector2(rlc.sizeDelta.x + addWidth, rlc.sizeDelta.y);
                rocketlauncherColorWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsRocketLauncherColors; //+ color

                GameObject rocketlauncherTemplates = rocketlauncherColorWindow.FindPath("Window", "Presets");
                TextMeshProUGUI rocketlauncherTemplate1 = rocketlauncherTemplates.FindPath("Template 1", "Text").TMP();
                TextMeshProUGUI rocketlauncherTemplate2 = rocketlauncherTemplates.FindPath("Template 2", "Text").TMP();
                TextMeshProUGUI rocketlauncherTemplate3 = rocketlauncherTemplates.FindPath("Template 3", "Text").TMP();
                TextMeshProUGUI rocketlauncherTemplate4 = rocketlauncherTemplates.FindPath("Template 4", "Text").TMP();
                TextMeshProUGUI rocketlauncherTemplate5 = rocketlauncherTemplates.FindPath("Template 5", "Text").TMP();

                rocketlauncherTemplate1.text = LanguageManager.CurrentLanguage.shop.shop_rocketlauncherPreset1;
                rocketlauncherTemplate2.text = LanguageManager.CurrentLanguage.shop.shop_rocketlauncherPreset2;
                rocketlauncherTemplate3.text = LanguageManager.CurrentLanguage.shop.shop_rocketlauncherPreset3;
                rocketlauncherTemplate4.text = LanguageManager.CurrentLanguage.shop.shop_rocketlauncherPreset4;
                rocketlauncherTemplate5.text = LanguageManager.CurrentLanguage.shop.shop_rocketlauncherPreset5;

                GameObject rocketlauncherTypeButtons = rocketlauncherTemplates.transform.parent.gameObject.Child("Type Selection");
                TextMeshProUGUI rocketlauncherColorPreset = rocketlauncherTypeButtons.FindPath("Preset Button", "Text").TMP();
                rocketlauncherColorPreset.text = LanguageManager.CurrentLanguage.shop.shop_colorsPreset;

                TextMeshProUGUI rocketlauncherColorCustom = rocketlauncherTypeButtons.FindPath("Custom Button", "Text").TMP();
                rocketlauncherColorCustom.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustom;

                TextMeshProUGUI rocketlauncherColorDone = rocketlauncherTemplates.transform.parent.gameObject.FindPath("Done", "Text").TMP();
                rocketlauncherColorDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone;

                //rocketlauncher custom color unlock prompt
                TextMeshProUGUI rocketlauncherCustomColorPrompt = rocketlauncherTemplates.transform.parent.gameObject.FindPath("Custom", "Locked", "Text").TMP();
                rocketlauncherCustomColorPrompt.text = LanguageManager.CurrentLanguage.shop.shop_colorsCustomUnlockPrompt + " " + LanguageManager.CurrentLanguage.shop.shop_weaponsRocketLauncher;

                // Arm
                // Feedbacker(Blue)
                // Knuckleblaster(Red)
                // Whiplash(Green)
                // ???(Yellow)

                //Arm window and descriptions
                GameObject armWindow = shopWeaponsObject.Child("Arm Window");
                GameObject armVariations = armWindow.FindPath("Variation Screen", "Variations");

                TextMeshProUGUI armWindowTitle = armVariations.transform.parent.gameObject.Child("Title").TMP();
                armWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_weaponsArms;

                //Feedbacker
                GameObject feedbacker = armVariations.Child("Arm Panel (Blue)");
                TextMeshProUGUI feedbackerName = feedbacker.Child("Variation Name").TMP();
                feedbackerName.text = LanguageManager.CurrentLanguage.shop.shop_armFeedbacker.ToUpper();

                GameObject feedbackerWindow = armWindow.FindPath("Arm Info (Blue)", "Panel");
                TextMeshProUGUI feedbackerWindowTitle = feedbackerWindow.transform.parent.gameObject.Child("Title").TMP();
                feedbackerWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_armFeedbacker;
                TextMeshProUGUI feedbackerWindowName = feedbackerWindow.Child("Name").TMP();
                feedbackerWindowName.enableAutoSizing = true;
                feedbackerWindowName.fontSizeMax = feedbackerWindowName.fontSize;
                feedbackerWindowName.fontSizeMin = 0f;
                feedbackerWindowName.text = feedbackerName.text;

                TextMeshProUGUI feedbackerWindowDescription = feedbackerWindow.Child("Description").TMP();
                feedbackerWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_armFeedbackerDescription1 + "\n\n" + LanguageManager.CurrentLanguage.shop.shop_armFeedbackerDescription2;

                TextMeshProUGUI feedbackerWindowDescriptionBack = feedbackerWindow.FindPath("Back Button", "Text").TMP();
                feedbackerWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;
                
                //Knuckleblaster
                GameObject knuckleblaster = armVariations.Child("Arm Panel (Red)");
                TextMeshProUGUI knuckleblasterName = knuckleblaster.Child("Variation Name").TMP();
                knuckleblasterName.text = LanguageManager.CurrentLanguage.shop.shop_armKnuckleblaster.ToUpper();

                GameObject knuckleblasterWindow = armWindow.FindPath("Arm Info (Red)", "Panel");
                TextMeshProUGUI knuckleblasterWindowTitle = knuckleblasterWindow.transform.parent.gameObject.Child("Title").TMP();
                knuckleblasterWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_armKnuckleblaster;
                TextMeshProUGUI knuckleblasterWindowName = knuckleblasterWindow.Child("Name").TMP();
                knuckleblasterWindowName.enableAutoSizing = true;
                knuckleblasterWindowName.fontSizeMax = knuckleblasterWindowName.fontSize;
                knuckleblasterWindowName.fontSizeMin = 0f;
                knuckleblasterWindowName.text = knuckleblasterName.text;

                TextMeshProUGUI knuckleblasterWindowDescription = knuckleblasterWindow.Child("Description").TMP();
                knuckleblasterWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_armKnuckleblasterDescription1 + "\n\n" + LanguageManager.CurrentLanguage.shop.shop_armKnuckleblasterDescription2;

                TextMeshProUGUI knuckleblasterWindowDescriptionBack = knuckleblasterWindow.FindPath("Back Button", "Text").TMP();
                knuckleblasterWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;
                
                //Whiplash
                GameObject whiplash = armVariations.Child("Arm Panel (Green)");
                TextMeshProUGUI whiplashName = whiplash.Child("Variation Name").TMP();
                whiplashName.text = LanguageManager.CurrentLanguage.shop.shop_armWhiplash.ToUpper();

                GameObject whiplashWindow = armWindow.FindPath("Arm Info (Green)", "Panel");
                TextMeshProUGUI whiplashWindowTitle = whiplashWindow.transform.parent.gameObject.Child("Title").TMP();
                whiplashWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_armWhiplash;
                TextMeshProUGUI whiplashWindowName = whiplashWindow.Child("Name").TMP();
                whiplashWindowName.enableAutoSizing = true;
                whiplashWindowName.fontSizeMax = whiplashWindowName.fontSize;
                whiplashWindowName.fontSizeMin = 0f;
                whiplashWindowName.text = whiplashName.text;

                TextMeshProUGUI whiplashWindowDescription = whiplashWindow.Child("Description").TMP();
                whiplashWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_armWhiplashDescription1 + "\n\n"
                    + LanguageManager.CurrentLanguage.shop.shop_armWhiplashDescription2;
                
                TextMeshProUGUI whiplashWindowDescriptionBack = whiplashWindow.FindPath("Back Button", "Text").TMP();
                whiplashWindowDescriptionBack.text = LanguageManager.CurrentLanguage.shop.shop_back;

                //it's "???" and placeholders so comment it for future
                /*//Gold arm (under construction)
                GameObject goldArm = GetGameObjectChild(armVariations, "Arm Panel (Gold)");
                TextMeshProUGUI goldArmUnderConstruction = GetTextMeshProUGUI(GetGameObjectChild(goldArm, "Variation Name"));
                goldArmUnderConstruction.text = LanguageManager.CurrentLanguage.shop.shop_armGold.ToUpper();

                GameObject goldArmWindow = GetGameObjectChild(GetGameObjectChild(armWindow, "Arm Info (Green)"), "Panel");
                TextMeshProUGUI goldArmWindowTitle = GetTextMeshProUGUI(GetGameObjectChild(goldArmWindow.transform.parent.gameObject, "Title"));
                goldArmWindowTitle.text = LanguageManager.CurrentLanguage.shop.shop_armGold;
                TextMeshProUGUI goldArmWindowName = GetTextMeshProUGUI(GetGameObjectChild(goldArmWindow, "Name"));
                goldArmWindowName.enableAutoSizing = true;
                goldArmWindowName.fontSizeMax = goldArmWindowName.fontSize;
                goldArmWindowName.fontSizeMin = 0f;
                goldArmWindowName.text = goldArmUnderConstruction.text;

                TextMeshProUGUI goldArmWindowDescription = GetTextMeshProUGUI(GetGameObjectChild(goldArmWindow, "Description"));
                goldArmWindowDescription.text = LanguageManager.CurrentLanguage.shop.shop_armGoldDescription;
                */

                //Usually it's VariationInfo's job but it disabled in gold arm so
                try
                {
                    GameObject goldArm = armVariations.Child("Arm Panel (Gold)");
                    TextMeshProUGUI goldArmUnderConstruction = goldArm.Child("Purchase Status").TMP();
                    goldArmUnderConstruction.text = LanguageManager.CurrentLanguage.misc.weapons_underConstruction;

                }
                catch (Exception e)
                {
                    Logging.Warn("An error occured while patching gold arm's under construction text.");
                    Logging.Warn(e.ToString());
                }

            }
            catch (Exception e)
            {
                Logging.Error("An error occured while translating shop weapons texts.");
                Logging.Error(e.ToString());
            }
                
        }

        /// <summary>
        /// Translates the shop terminal.
        ///
        /// The shop is not part of the localization pass: the game fills it when the
        /// player turns the terminal on and never touches it again, so the texts kept
        /// the language that was active before a switch. The whole shop is registered
        /// as a live group, so a language switch puts the game's own text back and
        /// writes the new language on top of it - the player does not have to leave
        /// and enter the shop again.
        /// </summary>
        public static void PatchShopRefactor(GameObject shopObject)
        {
            PatchShopFrontEnd(shopObject);
            PatchWeapons(shopObject);
        }
        
    }
}
