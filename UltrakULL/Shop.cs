﻿using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UltrakULL;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    class Shop
    {
        public void patchShopFrontEnd(ref GameObject coreGame, JsonParser language)
        {
            Console.WriteLine("Starting patchShop");

            //NOTE
            //Harmless, but still present, exception that happens, in a normal level when trying to patch the Return From Cyber Grind button
            //since it isn't active. Will need to fix since an exception here prevents any code to come from running
            //Commented out the offending block for now. Scroll down a bit to see it.

            //FirstRoom/Room/Shop/Canvas
            GameObject shopObject;
            if (SceneManager.GetActiveScene().name == "uk_construct")
            {
                Console.WriteLine("Using sandbox shop");
                shopObject = getGameObjectChild(GameObject.Find("Shop"),"Canvas");
            }
            else
            {
                shopObject = getGameObjectChild(getGameObjectChild(getGameObjectChild(coreGame, "Room"), "Shop"), "Canvas");
            }



            //Tip panel
            GameObject tipPanel = getGameObjectChild(getGameObjectChild(shopObject, "TipBox"), "Panel");
            Text tipTitle = getTextfromGameObject(getGameObjectChild(tipPanel, "Title"));
            tipTitle.text = language.currentLanguage.shop.shop_tipofthedayTitle;

            Text tipDescription = getTextfromGameObject(getGameObjectChild(tipPanel, "TipText"));
            Console.WriteLine("Original tip text: \n" + tipDescription.text);
            StringsParent levelTipStrings = new StringsParent();
            tipDescription.text = levelTipStrings.getLevelTip(language);


            //Weapons button
            GameObject mainButtons = getGameObjectChild(shopObject, "Main Menu");
            Console.WriteLine(mainButtons.name);

            Text weaponsButtonTitle = getTextfromGameObject(getGameObjectChild(getGameObjectChild(mainButtons, "WeaponsButton"), "Text"));
            weaponsButtonTitle.text = language.currentLanguage.shop.shop_weapons;

            //Enemies button
            Text enemiesButtonTitle = getTextfromGameObject(getGameObjectChild(getGameObjectChild(mainButtons, "EnemiesButton"), "Text"));
            enemiesButtonTitle.text = language.currentLanguage.shop.shop_monsters;

            //CG buttons
            Text cgButtonTitle = getTextfromGameObject(getGameObjectChild(getGameObjectChild(mainButtons, "CyberGrindButton"), "Text"));
            cgButtonTitle.text = language.currentLanguage.shop.shop_cybergrind;

            Text cgReturnButtonTitle = getTextfromGameObject(getGameObjectChild(getGameObjectChild(mainButtons, "ReturnButton"), "Text"));
            cgReturnButtonTitle.text = language.currentLanguage.shop.shop_returnToMission;

            //Sandbox button
            Text sandboxButtonTitle = getTextfromGameObject(getGameObjectChild(getGameObjectChild(mainButtons, "SandboxButton"), "Text"));
            sandboxButtonTitle.text = language.currentLanguage.shop.shop_sandbox;

            //Enemies title
            Text enemiesTitle = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shopObject, "Enemies"), "Panel"),"Title"));
            enemiesTitle.text = language.currentLanguage.shop.shop_monsters;

            //Enemies back button
            Text enemiesBackButtonText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shopObject, "Enemies"),"BackButton (2)"),"Text"));

            //Sandbox enter description
            GameObject sandboxEnter = getGameObjectChild(getGameObjectChild(shopObject, "Sandbox"), "Panel");

            Text sandboxEnterTitle = getTextfromGameObject(getGameObjectChild(sandboxEnter, "Title"));
            sandboxButtonTitle.text = sandboxButtonTitle.text;

            Text sandboxEnterDescription = getTextfromGameObject(getGameObjectChild(sandboxEnter, "Text"));

            sandboxEnterDescription.text = "Le <color=#4C99E6>Bac à sable</color> est un niveau vide qui peut être utilisé pour l'entraînement.\n\n" +
                "<color=red>Tout progrès dans le mission actuel sera perdu.</color>";

            Text sandboxEnterButton = getTextfromGameObject(getGameObjectChild(getGameObjectChild(sandboxEnter, "SandboxButton (1)"), "Text"));
            sandboxEnterButton.text = "ENTREZ LE BAC À SABLE";


            //CG enter description

            GameObject cgEnter = getGameObjectChild(getGameObjectChild(shopObject, "The Cyber Grind"), "Panel");

            Text cgEnterTitle = getTextfromGameObject(getGameObjectChild(cgEnter, "Title"));
            cgEnterTitle.text = "LE CYBERGRIND";

            Text cgEnterDescription = getTextfromGameObject(getGameObjectChild(cgEnter, "Text"));

            cgEnterDescription.text = language.currentLanguage.shop.shop_cybergrindDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_cybergrindDescription2 + "\n\n"
                + language.currentLanguage.shop.shop_cybergrindDescription3;

            Text cgEnterButton = getTextfromGameObject(getGameObjectChild(getGameObjectChild(cgEnter, "CyberGrindButton (1)"), "Text"));
            cgEnterButton.text = language.currentLanguage.shop.shop_cybergrindEnter;

            /*
            //CG exit description
            GameObject cgExit = getGameObjectChild(getGameObjectChild(shopObject, "Return from Cyber Grind"), "Panel");

            Text cgExitTitle = getTextfromGameObject(getGameObjectChild(cgExit, "Title"));
            cgExitTitle.text = "FUCK FUCK GO BACK";

            //Disable the LevelNameFinder component so it doesn't remove the translated string!
            GameObject levelText = getGameObjectChild(cgExit, "Text");
            LevelNameFinder comp = levelText.GetComponent<LevelNameFinder>();
            comp.enabled = false;

            Text cgExitDescriptionText = getTextfromGameObject(getGameObjectChild(cgExit, "Text"));
            StringsParent returningLevel = new StringsParent();
            cgExitDescriptionText.text = "<color=red> Going back to </color>: \n" + returningLevel.getReturningLevelName(cgExitDescriptionText.text);

            Text cgExitDescription = getTextfromGameObject(getGameObjectChild(getGameObjectChild(cgExit, "CyberGrindButton (1)"), "Text"));
            cgExitDescription.text = "SEE YOU NEXT UPDATE pepePoint";
            */


            //Enemies back button 
            Text enemiesBackText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shopObject, "Enemies"), "BackButton (2)"), "Text"));
            enemiesBackText.text = language.currentLanguage.shop.shop_back;

            //Sandbox back button
            Text sandboxBackText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shopObject, "Sandbox"), "BackButton (2)"), "Text"));
            sandboxBackText.text = language.currentLanguage.shop.shop_back;

            //EnemyInfo back button
            Text enemyInfoBackText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shopObject, "EnemyInfo"),"Button"),"Text"));
            enemyInfoBackText.text = language.currentLanguage.shop.shop_back;


        }


        public void patchWeapons(ref GameObject coreGame, JsonParser language)
        {
            GameObject shopWeaponsObject;
            if (SceneManager.GetActiveScene().name == "uk_construct")
            {
                Console.WriteLine("Using sandbox shop");
                shopWeaponsObject = getGameObjectChild(getGameObjectChild(GameObject.Find("Shop"), "Canvas"),"Weapons");
            }
            else
            {
                shopWeaponsObject = getGameObjectChild(getGameObjectChild(getGameObjectChild(getGameObjectChild(coreGame, "Room"), "Shop"), "Canvas"), "Weapons");
            }


            Text weaponBackText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "BackButton (1)"), "Text"));
            weaponBackText.text = language.currentLanguage.shop.shop_back;

            Text weaponRevolverText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "RevolverButton"), "Text"));
            weaponRevolverText.text = language.currentLanguage.shop.shop_weaponsRevolver;

            Text weaponShotgunText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "ShotgunButton"), "Text"));
            weaponShotgunText.text = language.currentLanguage.shop.shop_weaponsShotgun;

            Text weaponNailgunText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "NailgunButton"), "Text"));
            weaponNailgunText.text = language.currentLanguage.shop.shop_weaponsNailgun;

            //Slight problem - not all the text fits in the box.
            //The longer text is, the more we'll need to reduce the font size to compensate.
            Text weaponRailcannonText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "RailcannonButton"), "Text"));
            weaponRailcannonText.text = language.currentLanguage.shop.shop_weaponsRailcannon;
            weaponRailcannonText.fontSize = 16;

            Text rocketLauncherText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "RocketLauncherButton"), "Text"));
            rocketLauncherText.text = language.currentLanguage.shop.shop_weaponsRocketLauncher;
            rocketLauncherText.fontSize = 16;

            Text weaponArmText = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shopWeaponsObject, "ArmButton"), "Text"));
            weaponArmText.text = language.currentLanguage.shop.shop_weaponsArms;

            //Revolver window and descriptions
            GameObject revolverWindow = getGameObjectChild(shopWeaponsObject, "RevolverWindow");

            //Piercer
            GameObject piercer = getGameObjectChild(revolverWindow, "Variation Panel (Blue)");
            Text piercerName = getTextfromGameObject(getGameObjectChild(piercer, "Text"));
            piercerName.text = language.currentLanguage.shop.shop_revolverPiercer;

            GameObject piercerWindow = getGameObjectChild(revolverWindow, "Variation Info (Blue)");
            Text piercerWindowName = getTextfromGameObject(getGameObjectChild(piercerWindow, "Name"));
            piercerWindowName.text = piercerName.text;

            Text piercerWindowDescription = getTextfromGameObject(getGameObjectChild(piercerWindow, "Description"));
            piercerWindowDescription.text = language.currentLanguage.shop.shop_revolverPiercerDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_revolverPiercerDescription2;

            //Marksman

            GameObject marksman = getGameObjectChild(revolverWindow, "Variation Panel (Green)");
            Text marksmanName = getTextfromGameObject(getGameObjectChild(marksman, "Text"));
            marksmanName.text = language.currentLanguage.shop.shop_revolverMarksman;
            marksmanName.fontSize = 14;

            GameObject marksmanWindow = getGameObjectChild(revolverWindow, "Variation Info (Green)");
            Text marksmanWindowName = getTextfromGameObject(getGameObjectChild(marksmanWindow, "Name"));
            marksmanWindowName.text = marksmanName.text;

            Text marksmanWindowDescription = getTextfromGameObject(getGameObjectChild(marksmanWindow, "Description"));
            marksmanWindowDescription.text = language.currentLanguage.shop.shop_revolverMarksmanDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_revolverMarksmanDescription2 + "\n\n"
                + language.currentLanguage.shop.shop_revolverMarksmanDescription3;
            marksmanWindowDescription.fontSize = 14;

            //Revolver info & color tabs
            GameObject revolverExtra = getGameObjectChild(revolverWindow, "Info and Color Panel");
            GameObject revolverExtraInfo = getGameObjectChild(revolverExtra, "InfoButton");
            GameObject revolverExtraColor = getGameObjectChild(revolverExtra, "ColorButton");

            Text revolverExtraInfoText = getTextfromGameObject(getGameObjectChild(revolverExtraInfo, "Text"));
            revolverExtraInfoText.text = language.currentLanguage.shop.shop_weaponInfo;

            Text revolverExtraInfoColors = getTextfromGameObject(getGameObjectChild(revolverExtraColor, "Text"));
            revolverExtraInfoColors.text = language.currentLanguage.shop.shop_weaponColors;

            //Revolver lore
            GameObject revolverLore = getGameObjectChild(revolverWindow, "Info Screen");
            Text revolverLoreName = getTextfromGameObject(getGameObjectChild(revolverLore, "Name"));
            revolverLoreName.text = language.currentLanguage.shop.shop_weaponsRevolver;

            Text revolverLoreInfo = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverLore,"Scroll View"),"Viewport"),"Text"));

            revolverLoreInfo.text =
                language.currentLanguage.shop.shop_data + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver1 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver2 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver3 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver4 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver5 + "\n\n"
                + language.currentLanguage.shop.shop_strategy + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver6 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver7 + "\n\n"
                + language.currentLanguage.shop.shop_advancedStrategy + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver8 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver9 + "\n\n"
                + language.currentLanguage.shop.shop_loreRevolver10;

            //Revolver preset colors
            GameObject revolverColorWindow = getGameObjectChild(revolverWindow, "Color Screen");

            Text revolverColorWindowTitle = getTextfromGameObject(getGameObjectChild(revolverColorWindow,"Title"));
            revolverColorWindowTitle.text = "--" + language.currentLanguage.shop.shop_weaponsRevolver + "--";

            GameObject revolverStandardTemplates = getGameObjectChild(getGameObjectChild(revolverColorWindow, "Standard"),"Template");
            Text revolverStandardTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "Template 1"),"Button (Selectable)"),"Text"));
            Text revolverStandardTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text revolverStandardTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text revolverStandardTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text revolverStandardTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            revolverStandardTemplate1.text = language.currentLanguage.shop.shop_revolverPreset1;
            revolverStandardTemplate2.text = language.currentLanguage.shop.shop_revolverPreset2;
            revolverStandardTemplate3.text = language.currentLanguage.shop.shop_revolverPreset3;
            revolverStandardTemplate4.text = language.currentLanguage.shop.shop_revolverPreset4;
            revolverStandardTemplate5.text = language.currentLanguage.shop.shop_revolverPreset5;

            Text revolverColorSwitchToAlternative = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverColorWindow, "Standard"),"AlternateButton"),"Text"));
            revolverColorSwitchToAlternative.text = language.currentLanguage.shop.shop_colorsAlternative;

            Text revolverColorSwitchToStandard = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverColorWindow, "Alternate"), "AlternateButton"), "Text"));
            revolverColorSwitchToStandard.text = language.currentLanguage.shop.shop_colorsAlternative;

            GameObject revolverAlternateTemplates = getGameObjectChild(getGameObjectChild(revolverColorWindow, "Alternate"), "Template");
            Text revolverAlternateTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "Template 1"), "Button (Selectable)"), "Text"));
            Text revolverAlternateTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text revolverAlternateTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text revolverAlternateTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text revolverAlternateTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            revolverAlternateTemplate1.text = language.currentLanguage.shop.shop_revolverPreset1;
            revolverAlternateTemplate2.text = language.currentLanguage.shop.shop_revolverPreset2;
            revolverAlternateTemplate3.text = language.currentLanguage.shop.shop_revolverPreset3;
            revolverAlternateTemplate4.text = language.currentLanguage.shop.shop_revolverPreset4;
            revolverAlternateTemplate5.text = language.currentLanguage.shop.shop_revolverPreset5;

            Text revolverColorStandardPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "TemplateButton"),"Text"));
            revolverColorStandardPreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text revolverColorStandardCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverStandardTemplates, "CustomButton"),"Text"));
            revolverColorStandardCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text revolverColorAlternatePreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "TemplateButton"), "Text"));
            revolverColorAlternatePreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text revolverColorAlternateCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverAlternateTemplates, "CustomButton"), "Text"));
            revolverColorAlternateCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text revolverColorDone = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverColorWindow, "Done"),"Text"));
            revolverColorDone.text = language.currentLanguage.shop.shop_colorsDone;

            //Revolver custom colors
            GameObject revolverStandardCustom = getGameObjectChild(getGameObjectChild(revolverColorWindow, "Standard"),"Custom");
            Text revolverStandardCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverStandardCustom, "TemplateButton"), "Text"));
            revolverStandardCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text revolverStandardCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverStandardCustom, "CustomButton"), "Text"));
            revolverStandardCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            GameObject revolverAlternateCustom = getGameObjectChild(getGameObjectChild(revolverColorWindow, "Alternate"), "Custom");
            Text revolverAlternateCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverAlternateCustom, "TemplateButton"), "Text"));
            revolverAlternateCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text revolverAlternateCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(revolverAlternateCustom, "CustomButton"), "Text"));
            revolverAlternateCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            //Shotgun window and descriptions
            GameObject shotgunWindow = getGameObjectChild(shopWeaponsObject, "ShotgunWindow");

            GameObject coreEject = getGameObjectChild(shotgunWindow, "Variation Panel (Blue)");
            Text coreEjectName = getTextfromGameObject(getGameObjectChild(coreEject, "Text"));
            coreEjectName.text = language.currentLanguage.shop.shop_shotgunCoreEject;

            GameObject coreEjectWindow = getGameObjectChild(shotgunWindow, "Variation Info (Blue)");
            Text coreEjectWindowName = getTextfromGameObject(getGameObjectChild(coreEjectWindow, "Name"));
            coreEjectWindowName.text = language.currentLanguage.shop.shop_shotgunCoreEject;

            Text coreEjectWindowDescription = getTextfromGameObject(getGameObjectChild(coreEjectWindow, "Description"));
            coreEjectWindowDescription.text = language.currentLanguage.shop.shop_shotgunCoreEjectDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_shotgunCoreEjectDescription2 + "\n\n"
                + language.currentLanguage.shop.shop_shotgunCoreEjectDescription3;

            GameObject pumpCharge = getGameObjectChild(shotgunWindow, "Variation Panel (Green)");
            Text pumpChargeName = getTextfromGameObject(getGameObjectChild(pumpCharge, "Text"));
            pumpChargeName.text = language.currentLanguage.shop.shop_shotgunPumpCharge;
            pumpChargeName.fontSize = 16;

            GameObject pumpChargeWindow = getGameObjectChild(shotgunWindow, "Variation Info (Green)");
            Text pumpChargeWindowName = getTextfromGameObject(getGameObjectChild(pumpChargeWindow, "Name"));
            pumpChargeWindowName.text = language.currentLanguage.shop.shop_shotgunPumpCharge;

            Text pumpChargeWindowDescription = getTextfromGameObject(getGameObjectChild(pumpChargeWindow, "Description"));
            pumpChargeWindowDescription.text = language.currentLanguage.shop.shop_shotgunPumpChargeDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_shotgunPumpChargeDescription2;
            pumpChargeWindowDescription.fontSize = 14;

            //Shotgun info & color tabs
            GameObject shotgunExtra = getGameObjectChild(shotgunWindow, "Info and Color Panel");
            GameObject shotgunExtraInfo = getGameObjectChild(shotgunExtra, "InfoButton");
            GameObject shotgunExtraColor = getGameObjectChild(shotgunExtra, "ColorButton");

            Text shotgunExtraInfoText = getTextfromGameObject(getGameObjectChild(shotgunExtraInfo, "Text"));
            shotgunExtraInfoText.text = language.currentLanguage.shop.shop_weaponInfo;

            Text shotgunExtraInfoColors = getTextfromGameObject(getGameObjectChild(shotgunExtraColor, "Text"));
            shotgunExtraInfoColors.text = language.currentLanguage.shop.shop_weaponColors;

            //Shotgun lore
            GameObject shotgunLore = getGameObjectChild(shotgunWindow, "Info Screen");
            Text shotgunLoreName = getTextfromGameObject(getGameObjectChild(shotgunLore, "Name"));
            shotgunLoreName.text = language.currentLanguage.shop.shop_weaponsShotgun;

            Text shotgunLoreInfo = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shotgunLore, "Scroll View"), "Viewport"), "Text"));

            shotgunLoreInfo.text =
                language.currentLanguage.shop.shop_data + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun1 + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun2 + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun3 + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun4 + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun5 + "\n\n"
                + language.currentLanguage.shop.shop_strategy + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun6 + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun7 + "\n\n"
                + language.currentLanguage.shop.shop_advancedStrategy + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun8 + "\n\n"
                + language.currentLanguage.shop.shop_loreShotgun9;

            //Shotgun preset colors
            GameObject shotgunColorWindow = getGameObjectChild(shotgunWindow, "Color Screen");

            Text shotgunColorWindowTitle = getTextfromGameObject(getGameObjectChild(shotgunColorWindow, "Title"));
            shotgunColorWindowTitle.text = "--" + language.currentLanguage.shop.shop_weaponsShotgun + "--";

            GameObject shotgunStandardTemplates = getGameObjectChild(getGameObjectChild(shotgunColorWindow, "Standard"), "Template");
            Text shotgunStandardTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "Template 1"), "Button (Selectable)"), "Text"));
            Text shotgunStandardTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text shotgunStandardTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text shotgunStandardTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text shotgunStandardTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            shotgunStandardTemplate1.text = language.currentLanguage.shop.shop_shotgunPreset1;
            shotgunStandardTemplate2.text = language.currentLanguage.shop.shop_shotgunPreset2;
            shotgunStandardTemplate3.text = language.currentLanguage.shop.shop_shotgunPreset3;
            shotgunStandardTemplate4.text = language.currentLanguage.shop.shop_shotgunPreset4;
            shotgunStandardTemplate5.text = language.currentLanguage.shop.shop_shotgunPreset5;

            Text shotgunColorStandardPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "TemplateButton"), "Text"));
            shotgunColorStandardPreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text shotgunColorStandardCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shotgunStandardTemplates, "CustomButton"), "Text"));
            shotgunColorStandardCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text shotgunColorDone = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shotgunColorWindow, "Done"), "Text"));
            shotgunColorDone.text = language.currentLanguage.shop.shop_colorsDone;

            //Shotgun custom colors
            GameObject shotgunStandardCustom = getGameObjectChild(getGameObjectChild(shotgunColorWindow, "Standard"), "Custom");
            Text shotgunStandardCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shotgunStandardCustom, "TemplateButton"), "Text"));
            shotgunStandardCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text shotgunStandardCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(shotgunStandardCustom, "CustomButton"), "Text"));
            shotgunStandardCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            //Nailgun window and descriptions
            GameObject nailgunWindow = getGameObjectChild(shopWeaponsObject, "NailgunWindow");

            GameObject attractor = getGameObjectChild(nailgunWindow, "Variation Panel (Blue)");
            Text attractorName = getTextfromGameObject(getGameObjectChild(attractor, "Text"));
            attractorName.text = language.currentLanguage.shop.shop_nailgunMagnet;

            GameObject attractorWindow = getGameObjectChild(nailgunWindow, "Variation Info (Blue)");
            Text attractorWindowName = getTextfromGameObject(getGameObjectChild(attractorWindow, "Name"));
            attractorWindowName.text = language.currentLanguage.shop.shop_nailgunMagnet;

            Text attractorWindowDescription = getTextfromGameObject(getGameObjectChild(attractorWindow, "Description"));
            attractorWindowDescription.text = language.currentLanguage.shop.shop_nailgunMagnetDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_nailgunMagnetDescription2;
            attractorWindowDescription.fontSize = 16;

            GameObject overheat = getGameObjectChild(nailgunWindow, "Variation Panel (Green)");
            Text overheatName = getTextfromGameObject(getGameObjectChild(overheat, "Text"));
            overheatName.text = language.currentLanguage.shop.shop_nailgunOverheat;
            overheatName.fontSize = 16;

            GameObject overheatWindow = getGameObjectChild(nailgunWindow, "Variation Info (Green)");
            Text overheatWindowName = getTextfromGameObject(getGameObjectChild(overheatWindow, "Name"));
            overheatWindowName.text = language.currentLanguage.shop.shop_nailgunOverheat;

            Text overheatWindowDescription = getTextfromGameObject(getGameObjectChild(overheatWindow, "Description"));
            overheatWindowDescription.text = language.currentLanguage.shop.shop_nailgunOverheatDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_nailgunOverheatDescription2;
            overheatWindowDescription.fontSize = 14;

            //Nailgun info & color tabs
            GameObject nailgunExtra = getGameObjectChild(nailgunWindow, "Info and Color Panel");
            GameObject nailgunExtraInfo = getGameObjectChild(nailgunExtra, "InfoButton");
            GameObject nailgunExtraColor = getGameObjectChild(nailgunExtra, "ColorButton");

            Text nailgunExtraInfoText = getTextfromGameObject(getGameObjectChild(nailgunExtraInfo, "Text"));
            nailgunExtraInfoText.text = language.currentLanguage.shop.shop_weaponInfo;

            Text nailgunExtraInfoColors = getTextfromGameObject(getGameObjectChild(nailgunExtraColor, "Text"));
            nailgunExtraInfoColors.text = language.currentLanguage.shop.shop_weaponColors;

            //Nailgun lore
            GameObject nailgunLore = getGameObjectChild(nailgunWindow, "Info Screen");
            Text nailgunLoreName = getTextfromGameObject(getGameObjectChild(nailgunLore, "Name"));
            nailgunLoreName.text = language.currentLanguage.shop.shop_weaponsNailgun;

            Text nailgunLoreInfo = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunLore, "Scroll View"), "Viewport"), "Text"));
            nailgunLoreInfo.text =
                language.currentLanguage.shop.shop_data + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun1 + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun2 + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun3 + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun4 + "\n\n"
                + language.currentLanguage.shop.shop_strategy + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun5 + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun6 + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun7 + "\n\n"
                + language.currentLanguage.shop.shop_advancedStrategy + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun8 + "\n\n"
                + language.currentLanguage.shop.shop_loreNailgun9;

            //Nailgun preset colors
            GameObject nailgunColorWindow = getGameObjectChild(nailgunWindow, "Color Screen");

            Text nailgunColorWindowTitle = getTextfromGameObject(getGameObjectChild(nailgunColorWindow, "Title"));
            nailgunColorWindowTitle.text = "--" + language.currentLanguage.shop.shop_weaponsNailgun + "--";

            GameObject nailgunStandardTemplates = getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Standard"), "Template");
            Text nailgunStandardTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "Template 1"), "Button (Selectable)"), "Text"));
            Text nailgunStandardTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text nailgunStandardTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text nailgunStandardTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text nailgunStandardTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            nailgunStandardTemplate1.text = language.currentLanguage.shop.shop_nailgunPreset1;
            nailgunStandardTemplate2.text = language.currentLanguage.shop.shop_nailgunPreset2;
            nailgunStandardTemplate3.text = language.currentLanguage.shop.shop_nailgunPreset3;
            nailgunStandardTemplate4.text = language.currentLanguage.shop.shop_nailgunPreset4;
            nailgunStandardTemplate5.text = language.currentLanguage.shop.shop_nailgunPreset5;

            Text nailgunColorSwitchToAlternative = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Standard"), "AlternateButton"), "Text"));
            nailgunColorSwitchToAlternative.text = language.currentLanguage.shop.shop_colorsAlternative;

            Text nailgunColorSwitchToStandard = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Alternate"), "AlternateButton"), "Text"));
            nailgunColorSwitchToStandard.text = language.currentLanguage.shop.shop_colorsAlternative;

            GameObject nailgunAlternateTemplates = getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Alternate"), "Template");
            Text nailgunAlternateTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "Template 1"), "Button (Selectable)"), "Text"));
            Text nailgunAlternateTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text nailgunAlternateTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text nailgunAlternateTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text nailgunAlternateTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            nailgunAlternateTemplate1.text = language.currentLanguage.shop.shop_nailgunPreset1;
            nailgunAlternateTemplate2.text = language.currentLanguage.shop.shop_nailgunPreset2;
            nailgunAlternateTemplate3.text = language.currentLanguage.shop.shop_nailgunPreset3;
            nailgunAlternateTemplate4.text = language.currentLanguage.shop.shop_nailgunPreset4;
            nailgunAlternateTemplate5.text = language.currentLanguage.shop.shop_nailgunPreset5;

            Text nailgunColorStandardPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "TemplateButton"), "Text"));
            nailgunColorStandardPreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text nailgunColorStandardCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunStandardTemplates, "CustomButton"), "Text"));
            nailgunColorStandardCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text nailgunColorAlternatePreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "TemplateButton"), "Text"));
            nailgunColorAlternatePreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text nailgunColorAlternateCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunAlternateTemplates, "CustomButton"), "Text"));
            nailgunColorAlternateCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text nailgunColorDone = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Done"), "Text"));
            nailgunColorDone.text = language.currentLanguage.shop.shop_colorsDone;

            //Nailgun custom colors
            GameObject nailgunStandardCustom = getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Standard"), "Custom");
            Text nailgunStandardCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunStandardCustom, "TemplateButton"), "Text"));
            nailgunStandardCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text nailgunStandardCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunStandardCustom, "CustomButton"), "Text"));
            nailgunStandardCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            GameObject nailgunAlternateCustom = getGameObjectChild(getGameObjectChild(nailgunColorWindow, "Alternate"), "Custom");
            Text nailgunAlternateCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunAlternateCustom, "TemplateButton"), "Text"));
            nailgunAlternateCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text nailgunAlternateCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(nailgunAlternateCustom, "CustomButton"), "Text"));
            nailgunAlternateCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;


            //Railcannon window and descriptions
            GameObject railcannonWindow = getGameObjectChild(shopWeaponsObject, "RailcannonWindow");

            GameObject electric = getGameObjectChild(railcannonWindow, "Variation Panel (Blue)");
            Text electricName = getTextfromGameObject(getGameObjectChild(electric, "Text"));
            electricName.text = language.currentLanguage.shop.shop_railcannonElectric;

            GameObject electricWindow = getGameObjectChild(railcannonWindow, "Variation Info (Blue)");
            Text electricWindowName = getTextfromGameObject(getGameObjectChild(electricWindow, "Name"));
            electricWindowName.text = language.currentLanguage.shop.shop_railcannonElectric;

            Text electricWindowDescription = getTextfromGameObject(getGameObjectChild(electricWindow, "Description"));
            electricWindowDescription.text = language.currentLanguage.shop.shop_railcannonElectricDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_railcannonElectricDescription2 + "\n\n"
                + language.currentLanguage.shop.shop_railcannonElectricDescription3;
            electricWindowDescription.fontSize = 16;

            GameObject screwdriver = getGameObjectChild(railcannonWindow, "Variation Panel (Green)");
            Text screwdriverName = getTextfromGameObject(getGameObjectChild(screwdriver, "Text"));
            screwdriverName.text = language.currentLanguage.shop.shop_railcannonScrewdriver;

            GameObject screwdriverWindow = getGameObjectChild(railcannonWindow, "Variation Info (Green)");
            Text screwdriverWindowName = getTextfromGameObject(getGameObjectChild(screwdriverWindow, "Name"));
            screwdriverWindowName.text = language.currentLanguage.shop.shop_railcannonScrewdriver;

            Text screwdriverWindowDescription = getTextfromGameObject(getGameObjectChild(screwdriverWindow, "Description"));
            screwdriverWindowDescription.text = language.currentLanguage.shop.shop_railcannonScrewdriverDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_railcannonScrewdriverDescription2;
            screwdriverWindowDescription.fontSize = 16;

            GameObject malicious = getGameObjectChild(railcannonWindow, "Variation Panel (Red)");
            Text maliciousName = getTextfromGameObject(getGameObjectChild(malicious, "Text"));
            maliciousName.text = language.currentLanguage.shop.shop_railcannonMalicious;

            GameObject maliciousWindow = getGameObjectChild(railcannonWindow, "Variation Info (Red)");
            Text maliciousWindowName = getTextfromGameObject(getGameObjectChild(maliciousWindow, "Name"));
            maliciousWindowName.text = language.currentLanguage.shop.shop_railcannonMalicious;

            Text maliciousWindowDescription = getTextfromGameObject(getGameObjectChild(maliciousWindow, "Description"));
            maliciousWindowDescription.text = language.currentLanguage.shop.shop_railcannonMaliciousDescription1 + "\n\n"
                +  language.currentLanguage.shop.shop_railcannonMaliciousDescription2;
            maliciousWindowDescription.fontSize = 16;

            //Railcannon info & color tabs
            GameObject railcannonExtra = getGameObjectChild(railcannonWindow, "Info and Color Panel");
            GameObject railcannonExtraInfo = getGameObjectChild(railcannonExtra, "InfoButton");
            GameObject railcannonExtraColor = getGameObjectChild(railcannonExtra, "ColorButton");

            Text railcannonExtraInfoText = getTextfromGameObject(getGameObjectChild(railcannonExtraInfo, "Text"));
            railcannonExtraInfoText.text = language.currentLanguage.shop.shop_weaponInfo;

            Text railcannonExtraInfoColors = getTextfromGameObject(getGameObjectChild(railcannonExtraColor, "Text"));
            railcannonExtraInfoColors.text = language.currentLanguage.shop.shop_weaponColors;

            //Railcannon lore
            GameObject railcannonLore = getGameObjectChild(railcannonWindow, "Info Screen");
            Text railcannonLoreName = getTextfromGameObject(getGameObjectChild(railcannonLore, "Name"));
            railcannonLoreName.text = language.currentLanguage.shop.shop_weaponsRailcannon;

            Text railcannonLoreInfo = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(railcannonLore, "Scroll View"), "Viewport"), "Text"));
            railcannonLoreInfo.text =
                 language.currentLanguage.shop.shop_data + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon1 + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon2 + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon3 + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon4 + "\n\n"
                + language.currentLanguage.shop.shop_strategy + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon5 + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon6 + "\n\n"
                + language.currentLanguage.shop.shop_advancedStrategy + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon7 + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon8 + "\n\n"
                + language.currentLanguage.shop.shop_loreRailcannon9;


            //Railcannon preset colors
            GameObject railcannonColorWindow = getGameObjectChild(railcannonWindow, "Color Screen");

            Text railcannonColorWindowTitle = getTextfromGameObject(getGameObjectChild(railcannonColorWindow, "Title"));
            railcannonColorWindowTitle.text = "--" + language.currentLanguage.shop.shop_weaponsRailcannon + "--";

            GameObject railcannonStandardTemplates = getGameObjectChild(getGameObjectChild(railcannonColorWindow, "Standard"), "Template");
            Text railcannonStandardTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "Template 1"), "Button (Selectable)"), "Text"));
            Text railcannonStandardTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text railcannonStandardTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text railcannonStandardTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text railcannonStandardTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            railcannonStandardTemplate1.text = language.currentLanguage.shop.shop_railcannonPreset1;
            railcannonStandardTemplate2.text = language.currentLanguage.shop.shop_railcannonPreset2;
            railcannonStandardTemplate3.text = language.currentLanguage.shop.shop_railcannonPreset3;
            railcannonStandardTemplate4.text = language.currentLanguage.shop.shop_railcannonPreset4;
            railcannonStandardTemplate5.text = language.currentLanguage.shop.shop_railcannonPreset5;

            Text railcannonColorStandardPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "TemplateButton"), "Text"));
            railcannonColorStandardPreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text railcannonColorStandardCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(railcannonStandardTemplates, "CustomButton"), "Text"));
            railcannonColorStandardCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text railcannonColorDone = getTextfromGameObject(getGameObjectChild(getGameObjectChild(railcannonColorWindow, "Done"), "Text"));
            railcannonColorDone.text = language.currentLanguage.shop.shop_colorsDone;

            //Railcannon custom colors
            GameObject railcannonStandardCustom = getGameObjectChild(getGameObjectChild(railcannonColorWindow, "Standard"), "Custom");
            Text railcannonStandardCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(railcannonStandardCustom, "TemplateButton"), "Text"));
            railcannonStandardCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text railcannonStandardCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(railcannonStandardCustom, "CustomButton"), "Text"));
            railcannonStandardCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;


            //Rocket launcher window & descriptions
            GameObject rocketlauncherWindow = getGameObjectChild(shopWeaponsObject, "RocketLauncherWindow");

            GameObject freezeframe = getGameObjectChild(rocketlauncherWindow, "Variation Panel (Blue)");
            Text freezeframeName = getTextfromGameObject(getGameObjectChild(freezeframe, "Text"));
            freezeframeName.text = language.currentLanguage.shop.shop_rocketLauncherFreeze;

            GameObject freezeframeInfo = getGameObjectChild(rocketlauncherWindow, "Variation Info (Blue)");
            Text freezeframeDescription = getTextfromGameObject(getGameObjectChild(freezeframeInfo, "Description"));
            freezeframeDescription.text = language.currentLanguage.shop.shop_rocketLauncherFreezeDescription1 + "\n\n" + 
            language.currentLanguage.shop.shop_rocketLauncherFreezeDescription2;
            freezeframeDescription.fontSize = 16;

            //Rocket launcher info & color tabs
            GameObject rocketlauncherExtra = getGameObjectChild(rocketlauncherWindow, "Info and Color Panel");
            GameObject rocketlauncherExtraInfo = getGameObjectChild(rocketlauncherExtra, "InfoButton");
            GameObject rocketlauncherExtraColor = getGameObjectChild(rocketlauncherExtra, "ColorButton");

            Text rocketlauncherExtraInfoText = getTextfromGameObject(getGameObjectChild(rocketlauncherExtraInfo, "Text"));
            rocketlauncherExtraInfoText.text = language.currentLanguage.shop.shop_weaponInfo;

            Text rocketlauncherExtraInfoColors = getTextfromGameObject(getGameObjectChild(rocketlauncherExtraColor, "Text"));
            rocketlauncherExtraInfoColors.text = language.currentLanguage.shop.shop_weaponColors;

            //Rocket launcher lore
            GameObject rocketlauncherLore = getGameObjectChild(rocketlauncherWindow, "Info Screen");
            Text rocketlauncherLoreName = getTextfromGameObject(getGameObjectChild(rocketlauncherLore, "Name"));
            rocketlauncherLoreName.text = language.currentLanguage.shop.shop_weaponsRocketLauncher;

            Text rocketlauncherLoreInfo = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(rocketlauncherLore, "Scroll View"), "Viewport"), "Text"));
            rocketlauncherLoreInfo.text =
                  language.currentLanguage.shop.shop_data + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher1 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher2 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher3 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher4 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher5 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher6 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher7 + "\n\n"
                + language.currentLanguage.shop.shop_strategy + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher8 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher9 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher10 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher11 + "\n\n"
                + language.currentLanguage.shop.shop_advancedStrategy + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher12 + "\n\n"
                + language.currentLanguage.shop.shop_loreRocketLauncher13;


            //Rocket launcher preset colors
            //Shotgun preset colors
            GameObject RLColorWindow = getGameObjectChild(rocketlauncherWindow, "Color Screen");

            Text RLColorWindowTitle = getTextfromGameObject(getGameObjectChild(RLColorWindow, "Title"));
            RLColorWindowTitle.text = "--" + language.currentLanguage.shop.shop_weaponsRocketLauncher + "--";

            GameObject RLStandardTemplates = getGameObjectChild(getGameObjectChild(RLColorWindow, "Standard"), "Template");
            Text RLStandardTemplate1 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "Template 1"), "Button (Selectable)"), "Text"));
            Text RLStandardTemplate2 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "Template 2"), "Button (Selectable)"), "Text"));
            Text RLStandardTemplate3 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "Template 3"), "Button (Selectable)"), "Text"));
            Text RLStandardTemplate4 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "Template 4"), "Button (Selectable)"), "Text"));
            Text RLStandardTemplate5 = getTextfromGameObject(getGameObjectChild(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "Template 5"), "Button (Selectable)"), "Text"));

            RLStandardTemplate1.text = language.currentLanguage.shop.shop_rocketlauncherPreset1;
            RLStandardTemplate2.text = language.currentLanguage.shop.shop_rocketlauncherPreset2;
            RLStandardTemplate3.text = language.currentLanguage.shop.shop_rocketlauncherPreset3;
            RLStandardTemplate4.text = language.currentLanguage.shop.shop_rocketlauncherPreset4;
            RLStandardTemplate5.text = language.currentLanguage.shop.shop_rocketlauncherPreset5;

            Text RLColorStandardPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "TemplateButton"), "Text"));
            RLColorStandardPreset.text = language.currentLanguage.shop.shop_colorsPreset;

            Text RLColorStandardCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(RLStandardTemplates, "CustomButton"), "Text"));
            RLColorStandardCustom.text = language.currentLanguage.shop.shop_colorsCustom;

            Text RLColorDone = getTextfromGameObject(getGameObjectChild(getGameObjectChild(RLColorWindow, "Done"), "Text"));
            RLColorDone.text = language.currentLanguage.shop.shop_colorsDone;

            //Rocket launcher custom colors
            GameObject RLStandardCustom = getGameObjectChild(getGameObjectChild(RLColorWindow, "Standard"), "Custom");
            Text RLStandardCustomPreset = getTextfromGameObject(getGameObjectChild(getGameObjectChild(RLStandardCustom, "TemplateButton"), "Text"));
            RLStandardCustomPreset.text = language.currentLanguage.shop.shop_colorsPreset;
            Text RLStandardCustomCustom = getTextfromGameObject(getGameObjectChild(getGameObjectChild(RLStandardCustom, "CustomButton"), "Text"));
            RLStandardCustomCustom.text = language.currentLanguage.shop.shop_colorsCustom;


            //Arm window and descriptions
            GameObject armWindow = getGameObjectChild(shopWeaponsObject, "ArmWindow");

            GameObject feedbacker = getGameObjectChild(armWindow, "Variation Panel 1 (New)");
            Text feedbackerName = getTextfromGameObject(getGameObjectChild(feedbacker, "Text"));
            feedbackerName.text = language.currentLanguage.shop.shop_armFeedbacker;

            GameObject feedbackerWindow = getGameObjectChild(armWindow, "Variation 1 Info (New)");
            Text feedbackerWindowName = getTextfromGameObject(getGameObjectChild(feedbackerWindow, "Name"));
            feedbackerWindowName.text = language.currentLanguage.shop.shop_armFeedbacker;

            Text feedbackerWindowDescription = getTextfromGameObject(getGameObjectChild(feedbackerWindow, "Description"));
            feedbackerWindowDescription.text = language.currentLanguage.shop.shop_armFeedbackerDescription1 + "\n\n" + language.currentLanguage.shop.shop_armFeedbackerDescription2;

            GameObject knuckleblaster = getGameObjectChild(armWindow, "Variation Panel 2 (New)");
            Text knuckleblasterName = getTextfromGameObject(getGameObjectChild(knuckleblaster, "Text"));
            knuckleblasterName.text = language.currentLanguage.shop.shop_armKnuckleblaster;

            GameObject knuckleblasterWindow = getGameObjectChild(armWindow, "Variation 2 Info (New)");
            Text knuckleblasterWindowName = getTextfromGameObject(getGameObjectChild(knuckleblasterWindow, "Name"));
            knuckleblasterWindowName.text = language.currentLanguage.shop.shop_armKnuckleblaster;

            Text knuckleblasterWindowDescription = getTextfromGameObject(getGameObjectChild(knuckleblasterWindow, "Description"));
            knuckleblasterWindowDescription.text = language.currentLanguage.shop.shop_armKnuckleblasterDescription1 + "\n\n" + language.currentLanguage.shop.shop_armKnuckleblasterDescription2;

            GameObject whiplash = getGameObjectChild(armWindow, "Variation Panel 3 (New)");
            Text whiplashName = getTextfromGameObject(getGameObjectChild(whiplash, "Text"));
            whiplashName.text = language.currentLanguage.shop.shop_armWhiplash;

            //here
            GameObject whiplashWindow = getGameObjectChild(armWindow, "Variation 3 Info (New)");
            Text whiplashWindowName = getTextfromGameObject(getGameObjectChild(whiplashWindow, "Name"));
            whiplashWindowName.text = language.currentLanguage.shop.shop_armWhiplash;

            Text whiplashWindowDescription = getTextfromGameObject(getGameObjectChild(whiplashWindow, "Description"));
            whiplashWindowDescription.text = language.currentLanguage.shop.shop_armWhiplashDescription1 + "\n\n"
                + language.currentLanguage.shop.shop_armWhiplashDescription2;
            whiplashWindowDescription.fontSize = 16;

        }


        public Shop(ref GameObject level, JsonParser language)
        {

            this.patchShopFrontEnd(ref level,language);
            this.patchWeapons(ref level,language);

        }


    }
}
