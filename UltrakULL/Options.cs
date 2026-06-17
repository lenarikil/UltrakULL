using System;
using System.Collections.Generic;
using TMPro;
using UltrakULL.Harmony_Patches;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    class Options
    {
        private GameObject optionsMenu;

        static public void PatchGeneralOptions(GameObject generalOptions)
        {
            //General options
            GameObject generalContent = GetGameObjectChild(GetGameObjectChild(generalOptions, "Scroll Rect"), "Contents");
            //-- WEAPONS -- 
            TextMeshProUGUI generalText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "-- Weapons --"), "Text"));
            generalText.text = "-- " + LanguageManager.CurrentLanguage.options.controls_weapons + " --";

            TextMeshProUGUI rememberWeaponText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Remember Last Used Weapon Variation"), "Text"));
            rememberWeaponText.text = LanguageManager.CurrentLanguage.options.general_rememberWeapon;

            TextMeshProUGUI weaponPosText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Weapon Position"), "Text"));
            weaponPosText.text = LanguageManager.CurrentLanguage.options.general_weaponPosition;

            //Have to patch directly from the Dropdown.OptionData list.
            GameObject weaponPosList = GetGameObjectChild(GetGameObjectChild(generalContent, "Weapon Position"), "Dropdown(Clone)");
            TMP_Dropdown weaponPosDropdown = weaponPosList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> weaponPosListText = weaponPosDropdown.options;
            weaponPosListText[0].text = LanguageManager.CurrentLanguage.options.general_weaponPositionRight;
            weaponPosListText[1].text = LanguageManager.CurrentLanguage.options.general_weaponPositionMiddle;
            weaponPosListText[2].text = LanguageManager.CurrentLanguage.options.general_weaponPositionLeft;

            //-- SCREEN -- goes here
            TextMeshProUGUI screenText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "-- Screen --"), "Text"));
            screenText.text = "-- " + LanguageManager.CurrentLanguage.options.general_screen + " --";

            TextMeshProUGUI screenshakeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Screenshake"), "Text"));
            screenshakeText.text = LanguageManager.CurrentLanguage.options.general_screenShake;

            SliderValueToText screenshakeSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(generalContent, "Screenshake"), "Slider Button(Clone)"), "Slider"), "Text").GetComponentInChildren<SliderValueToText>();
            screenshakeSlider.ifMin = LanguageManager.CurrentLanguage.options.general_screenShakeMinimum;
            screenshakeSlider.ifMax = LanguageManager.CurrentLanguage.options.general_screenShakeMaximum;

            TextMeshProUGUI parryFlashText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Parry Screen Flash"), "Text"));
            parryFlashText.text = LanguageManager.CurrentLanguage.options.general_parryFlash;

            TextMeshProUGUI cameraTiltText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Camera Tilt"), "Text"));
            cameraTiltText.text = LanguageManager.CurrentLanguage.options.general_cameraTilt;

            //-- MISC --
            TextMeshProUGUI miscText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "-- Misc --"), "Text"));
            miscText.text = "-- " + LanguageManager.CurrentLanguage.options.general_misc + " --";
            
            TextMeshProUGUI seasonEventText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Seasonal Events"), "Text"));
            seasonEventText.text = LanguageManager.CurrentLanguage.options.general_seasonalEvent;

            TextMeshProUGUI levelLeaderboardsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Level Leaderboards"), "Text"));
            levelLeaderboardsText.text = LanguageManager.CurrentLanguage.options.general_levelLeaderboards;

            TextMeshProUGUI restartWarningText = GetTextMeshProUGUI(GetGameObjectChild(generalContent.transform.GetChild(10).gameObject, "Text"));
            restartWarningText.text = LanguageManager.CurrentLanguage.options.general_restartWarning;

            GameObject restartWarningList = GetGameObjectChild(generalContent.transform.GetChild(10).gameObject, "Dropdown(Clone)");
            TMP_Dropdown restartWarningDropdown = restartWarningList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> restartWarningListText = restartWarningDropdown.options;
            restartWarningListText[0].text = LanguageManager.CurrentLanguage.options.general_restartWarningAlwaysOn;
            restartWarningListText[1].text = LanguageManager.CurrentLanguage.options.general_restartWarningOnlyCG;
            restartWarningListText[2].text = LanguageManager.CurrentLanguage.options.general_restartWarningAlwaysOff;

            TextMeshProUGUI sandboxOverwriteText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Sandbox Save Overwrite Warning"), "Text"));
            sandboxOverwriteText.text = LanguageManager.CurrentLanguage.options.general_sandboxOverwrite;

            TextMeshProUGUI discordText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Discord Integration"), "Text"));
            discordText.text = LanguageManager.CurrentLanguage.options.general_discordRpc;

            TextMeshProUGUI advancedOptionsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(generalContent, "Advanced Options"), "Text"));
            advancedOptionsText.text = LanguageManager.CurrentLanguage.options.general_advancedOptions;

            TextMeshProUGUI advancedOptionsCustomizeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(generalContent, "Advanced Options"), "Action Button(Clone)"), "Text"));
            advancedOptionsCustomizeText.text = LanguageManager.CurrentLanguage.options.general_advancedOptionsCustomize;
        }
        static public void PatchControlOptions(GameObject optionsMenu)
        {   
            //Control options
            GameObject controlContent = GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents");

            //-- GENERAL --
            TextMeshProUGUI controlText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "-- General --"), "Text"));
            controlText.text = "-- " + LanguageManager.CurrentLanguage.options.category_general + " --";

            TextMeshProUGUI mouseSensitivityText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Look Sensitivity"), "Text"));
            mouseSensitivityText.text = LanguageManager.CurrentLanguage.options.controls_mouseSensitivity;

            TextMeshProUGUI invertXAxisText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Invert X Axis"), "Text"));
            invertXAxisText.text = LanguageManager.CurrentLanguage.options.controls_xInversion;

            TextMeshProUGUI invertYAxisText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Invert Y Axis"), "Text"));
            invertYAxisText.text = LanguageManager.CurrentLanguage.options.controls_yInversion;

            TextMeshProUGUI controllerRumbleText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Controller Rumble"), "Text"));
            controllerRumbleText.text = LanguageManager.CurrentLanguage.options.controls_controllerRumble;

            TextMeshProUGUI controllerRumbleTextCustomize = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(controlContent, "Controller Rumble"), "Action Button(Clone)"), "Text"));
            controllerRumbleTextCustomize.text = LanguageManager.CurrentLanguage.options.controls_controllerRumbleCustomize;

            TextMeshProUGUI weaponsTitle = GetTextMeshProUGUI(GetGameObjectChild(controlContent.transform.GetChild(5).gameObject, "Text"));
            weaponsTitle.text = "-- " + LanguageManager.CurrentLanguage.options.controls_weapons + " --";

            GameObject mouseWheelContent = GetGameObjectChild(controlContent, "Scroll Weapons with Mouse Wheel");
            TextMeshProUGUI changeWeaponMouseWheel = GetTextMeshProUGUI(GetGameObjectChild(mouseWheelContent, "Text"));
            changeWeaponMouseWheel.text = LanguageManager.CurrentLanguage.options.controls_mouseWheelToChangeWeapon;

            TextMeshProUGUI weaponScrollType = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Weapon Scroll Type"), "Text"));
            weaponScrollType.text = LanguageManager.CurrentLanguage.options.controls_scrollType;

            //Dropdown here
            GameObject scrollTypeList = (GetGameObjectChild(GetGameObjectChild(controlContent, "Weapon Scroll Type"), "Dropdown(Clone)"));

            TMP_Dropdown scrollTypeDropdown = scrollTypeList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> scrollTypeDropdownText = scrollTypeDropdown.options;
            scrollTypeDropdownText[0].text = LanguageManager.CurrentLanguage.options.controls_scrollTypeWeapons;
            scrollTypeDropdownText[1].text = LanguageManager.CurrentLanguage.options.controls_scrollTypeVariations;
            scrollTypeDropdownText[2].text = LanguageManager.CurrentLanguage.options.controls_scrollTypeAll;

            TextMeshProUGUI reverseScrollDirection = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Reverse Scroll Direction"), "Text"));
            reverseScrollDirection.text = LanguageManager.CurrentLanguage.options.controls_reverseScroll;

            GameObject redrawBehaviour = GetGameObjectChild(controlContent, "On Swap To Already Drawn Weapon");
            TextMeshProUGUI redrawBehaviourTitle = GetTextMeshProUGUI(GetGameObjectChild(redrawBehaviour, "Text"));
            redrawBehaviourTitle.text = LanguageManager.CurrentLanguage.options.controls_redrawBehaviour;

            TMP_Dropdown redrawBehaviourDropdown = GetGameObjectChild(redrawBehaviour, "Dropdown(Clone)").GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> redrawBehaviourDropdownText = redrawBehaviourDropdown.options;
            redrawBehaviourDropdownText[0].text = LanguageManager.CurrentLanguage.options.controls_redrawNext;
            redrawBehaviourDropdownText[1].text = LanguageManager.CurrentLanguage.options.controls_redrawFirst;
            redrawBehaviourDropdownText[2].text = LanguageManager.CurrentLanguage.options.controls_redrawSame;

            TextMeshProUGUI invertRocketControls = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(controlContent, "Invert Rocket Controls"), "Text"));
            invertRocketControls.text = LanguageManager.CurrentLanguage.options.controls_invertRocketControls;

            //unused after patch 16
            //TextMeshProUGUI bindsTitle = GetTextMeshProUGUI(GetGameObjectChild(controlContent.transform.GetChild(10).gameObject, "Text"));
            //bindsTitle.text = "-- " + LanguageManager.CurrentLanguage.options.controls_bindings + " --";


            //Tried to use a foreach loop but it just wouldn't work, that'll do for now, just have to add things manually once they get added
            //Commented this out for now due to it causing out of bound issues. Will investigate later

            /*TextMeshProUGUI bindMove = GetTextMeshProUGUI(controlContent.transform.GetChild(8).gameObject);
            TextMeshProUGUI bindDodge = GetTextMeshProUGUI(controlContent.transform.GetChild(9).gameObject);
            TextMeshProUGUI bindSlide = GetTextMeshProUGUI(controlContent.transform.GetChild(10).gameObject);
            TextMeshProUGUI bindJump = GetTextMeshProUGUI(controlContent.transform.GetChild(11).gameObject);

            TextMeshProUGUI bindPrimary = GetTextMeshProUGUI(controlContent.transform.GetChild(13).gameObject);
            TextMeshProUGUI bindSecondary = GetTextMeshProUGUI(controlContent.transform.GetChild(14).gameObject);
            TextMeshProUGUI bindChangeVariation = GetTextMeshProUGUI(controlContent.transform.GetChild(15).gameObject);
            TextMeshProUGUI bindSlot0 = GetTextMeshProUGUI(controlContent.transform.GetChild(16).gameObject);
            TextMeshProUGUI bindSlot1 = GetTextMeshProUGUI(controlContent.transform.GetChild(17).gameObject);
            TextMeshProUGUI bindSlot2 = GetTextMeshProUGUI(controlContent.transform.GetChild(18).gameObject);
            TextMeshProUGUI bindSlot3 = GetTextMeshProUGUI(controlContent.transform.GetChild(19).gameObject);
            TextMeshProUGUI bindSlot4 = GetTextMeshProUGUI(controlContent.transform.GetChild(20).gameObject);
            TextMeshProUGUI bindSlot5 = GetTextMeshProUGUI(controlContent.transform.GetChild(21).gameObject);
            TextMeshProUGUI bindSlot6 = GetTextMeshProUGUI(controlContent.transform.GetChild(22).gameObject);
            TextMeshProUGUI bindSlot7 = GetTextMeshProUGUI(controlContent.transform.GetChild(23).gameObject);
            TextMeshProUGUI bindSlot8 = GetTextMeshProUGUI(controlContent.transform.GetChild(24).gameObject);
            TextMeshProUGUI bindSlot9 = GetTextMeshProUGUI(controlContent.transform.GetChild(25).gameObject);
            TextMeshProUGUI bindNext = GetTextMeshProUGUI(controlContent.transform.GetChild(26).gameObject);
            TextMeshProUGUI bindPrevious = GetTextMeshProUGUI(controlContent.transform.GetChild(27).gameObject);
            TextMeshProUGUI bindLast = GetTextMeshProUGUI(controlContent.transform.GetChild(28).gameObject);

            TextMeshProUGUI bindChangeFist = GetTextMeshProUGUI(controlContent.transform.GetChild(30).gameObject);
            TextMeshProUGUI bindPunch = GetTextMeshProUGUI(controlContent.transform.GetChild(31).gameObject);
            TextMeshProUGUI bindHook = GetTextMeshProUGUI(controlContent.transform.GetChild(32).gameObject);

            bindMove.text = LanguageManager.CurrentLanguage.options.controls_move;
            bindDodge.text = LanguageManager.CurrentLanguage.options.controls_dash;
            bindSlide.text = LanguageManager.CurrentLanguage.options.controls_slide;
            bindJump.text = LanguageManager.CurrentLanguage.options.controls_jump;

            bindPrimary.text = LanguageManager.CurrentLanguage.options.controls_primaryFire;
            bindSecondary.text = LanguageManager.CurrentLanguage.options.controls_secondaryFire;
            bindChangeVariation.text = LanguageManager.CurrentLanguage.options.controls_changeVariation;
            bindSlot0.text = LanguageManager.CurrentLanguage.options.controls_slot0;
            bindSlot1.text = LanguageManager.CurrentLanguage.options.controls_slot1;
            bindSlot2.text = LanguageManager.CurrentLanguage.options.controls_slot2;
            bindSlot3.text = LanguageManager.CurrentLanguage.options.controls_slot3;
            bindSlot4.text = LanguageManager.CurrentLanguage.options.controls_slot4;
            bindSlot5.text = LanguageManager.CurrentLanguage.options.controls_slot5;
            bindSlot6.text = LanguageManager.CurrentLanguage.options.controls_slot6;
            bindSlot7.text = LanguageManager.CurrentLanguage.options.controls_slot7;
            bindSlot8.text = LanguageManager.CurrentLanguage.options.controls_slot8;
            bindSlot9.text = LanguageManager.CurrentLanguage.options.controls_slot9;
            bindNext.text = LanguageManager.CurrentLanguage.options.controls_nextWeapon;
            bindPrevious.text = LanguageManager.CurrentLanguage.options.controls_previousWeapon;
            bindLast.text = LanguageManager.CurrentLanguage.options.controls_lastUsedWeapon;

            bindChangeFist.text = LanguageManager.CurrentLanguage.options.controls_changeArm;
            bindPunch.text = LanguageManager.CurrentLanguage.options.controls_punch;
            bindHook.text = LanguageManager.CurrentLanguage.options.controls_whiplash;*/
        }
        static public void PatchGraphicsOptions(GameObject optionsMenu)
        {
            //Graphics options
            GameObject graphicsContent = GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents");

            //--GENERAL--
            TextMeshProUGUI graphicsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "-- General --"), "Text"));
            graphicsText.text = "--" + LanguageManager.CurrentLanguage.options.category_general + "--";

            TextMeshProUGUI resolutionText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Resolution"), "Text"));
            resolutionText.text = LanguageManager.CurrentLanguage.options.graphics_resolution;

            TextMeshProUGUI fullscreenText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Fullscreen"), "Text"));
            fullscreenText.text = LanguageManager.CurrentLanguage.options.graphics_fullscreen;

            TextMeshProUGUI fpslimitText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Target Framerate"), "Text"));
            fpslimitText.text = LanguageManager.CurrentLanguage.options.graphics_maxFps;

            GameObject fpsObject = GetGameObjectChild(GetGameObjectChild(graphicsContent, "Target Framerate"), "Dropdown(Clone)");
            TMP_Dropdown fpsDropdown = fpsObject.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> fpsDropdownListText = fpsDropdown.options;
            fpsDropdownListText[0].text = LanguageManager.CurrentLanguage.options.graphics_maxFpsNone;
            fpsDropdownListText[1].text = LanguageManager.CurrentLanguage.options.graphics_maxFps2x;

            TextMeshProUGUI vsyncText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "VSync"), "Text"));
            vsyncText.text = LanguageManager.CurrentLanguage.options.graphics_vsync;

            TextMeshProUGUI fovText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Field of View"), "Text"));
            fovText.text = LanguageManager.CurrentLanguage.options.graphics_fieldOfVision;

            TextMeshProUGUI gammaCorrectionText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Gamma (Brightness)"), "Text"));
            gammaCorrectionText.text = LanguageManager.CurrentLanguage.options.graphics_gamma;

            TextMeshProUGUI disableNewShadersText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Use Fallback Shaders (Requires Reload)"), "Text"));
            disableNewShadersText.text = LanguageManager.CurrentLanguage.options.graphics_useFallbackShaders;

            //--PSX--
            TextMeshProUGUI psxFilterSettingsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "-- PSX --"), "Text"));
            psxFilterSettingsText.text = "--" + LanguageManager.CurrentLanguage.options.graphics_filters + "--\n<size=16>"
                                        + LanguageManager.CurrentLanguage.options.graphics_filtersDescription + "</size>";


            TextMeshProUGUI downscalingText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Downscaling"), "Text"));
            downscalingText.text = LanguageManager.CurrentLanguage.options.graphics_pixelisation;

            GameObject resolution = GetGameObjectChild(GetGameObjectChild(graphicsContent, "Downscaling"), "Dropdown(Clone)");
            TMP_Dropdown resolutionDropdown = resolution.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> resolutionDropdownListText = resolutionDropdown.options;

            resolutionDropdownListText[0].text = LanguageManager.CurrentLanguage.options.graphics_pixelisationNone;
            resolutionDropdownListText[1].text = LanguageManager.CurrentLanguage.options.graphics_pixelisation720p;
            resolutionDropdownListText[2].text = LanguageManager.CurrentLanguage.options.graphics_pixelisation480p;
            resolutionDropdownListText[3].text = LanguageManager.CurrentLanguage.options.graphics_pixelisation360p;
            resolutionDropdownListText[4].text = LanguageManager.CurrentLanguage.options.graphics_pixelisation240p;
            resolutionDropdownListText[5].text = LanguageManager.CurrentLanguage.options.graphics_pixelisation144p;
            resolutionDropdownListText[6].text = LanguageManager.CurrentLanguage.options.graphics_pixelisation36p;


            TextMeshProUGUI ditheringText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Dithering"), "Text"));
            ditheringText.text = LanguageManager.CurrentLanguage.options.graphics_dithering;

            SliderValueToText ditheringSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Dithering"), "Slider Button(Clone)"), "Slider"), "Text").GetComponentInChildren<SliderValueToText>();
            ditheringSlider.ifMin = LanguageManager.CurrentLanguage.options.graphics_ditheringMinimum;

            TextMeshProUGUI textureWarpingText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Texture Warping"), "Text"));
            textureWarpingText.text = LanguageManager.CurrentLanguage.options.graphics_textureWarping;

            TextMeshProUGUI vertexWarpingText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Vertex Warping"), "Text"));
            vertexWarpingText.text = LanguageManager.CurrentLanguage.options.graphics_vertexWarping;

            GameObject vertexWarping = GetGameObjectChild(GetGameObjectChild(graphicsContent, "Vertex Warping"), "Dropdown(Clone)");
            TMP_Dropdown vertexWarpingDropdown = vertexWarping.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> vertexWarpingDropdownListText = vertexWarpingDropdown.options;

            vertexWarpingDropdownListText[0].text = LanguageManager.CurrentLanguage.options.graphics_vertexWarpingNone;
            vertexWarpingDropdownListText[1].text = LanguageManager.CurrentLanguage.options.graphics_vertexWarpingLight;
            vertexWarpingDropdownListText[2].text = LanguageManager.CurrentLanguage.options.graphics_vertexWarpingMedium;
            vertexWarpingDropdownListText[3].text = LanguageManager.CurrentLanguage.options.graphics_vertexWarpingStrong;
            vertexWarpingDropdownListText[4].text = LanguageManager.CurrentLanguage.options.graphics_vertexWarpingVeryStrong;
            vertexWarpingDropdownListText[5].text = LanguageManager.CurrentLanguage.options.graphics_vertexWarpingAbsurd;

            TextMeshProUGUI customColorPalette = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Custom Color Palette"), "Text"));
            customColorPalette.text = LanguageManager.CurrentLanguage.options.graphics_customColorPalette;

            TextMeshProUGUI customPaletteTexture = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Color Palette Texture"), "Text"));
            customPaletteTexture.text = LanguageManager.CurrentLanguage.options.graphics_customPaletteTexture;

            TextMeshProUGUI customColorPaletteSelect = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Color Palette Texture"), "Action Button(Clone)"), "Text"));
            customColorPaletteSelect.text = LanguageManager.CurrentLanguage.options.graphics_customColorPaletteSelect;

            TextMeshProUGUI colorCompressionText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Color Compression"), "Text"));
            colorCompressionText.text = LanguageManager.CurrentLanguage.options.graphics_colorCompression;

            GameObject colorCompression = GetGameObjectChild(GetGameObjectChild(graphicsContent, "Color Compression"), "Dropdown(Clone)");
            TMP_Dropdown colorCompressionDropdown = colorCompression.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> colorCompressionDropdownListText = colorCompressionDropdown.options;

            colorCompressionDropdownListText[0].text = LanguageManager.CurrentLanguage.options.graphics_colorCompressionNone;
            colorCompressionDropdownListText[1].text = LanguageManager.CurrentLanguage.options.graphics_colorCompressionLight;
            colorCompressionDropdownListText[2].text = LanguageManager.CurrentLanguage.options.graphics_colorCompressionMedium;
            colorCompressionDropdownListText[3].text = LanguageManager.CurrentLanguage.options.graphics_colorCompressionStrong;
            colorCompressionDropdownListText[4].text = LanguageManager.CurrentLanguage.options.graphics_colorCompressionVeryStrong;
            colorCompressionDropdownListText[5].text = LanguageManager.CurrentLanguage.options.graphics_colorCompressionAbsurd;

            TextMeshProUGUI performanceText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "-- Performance --"), "Text"));
            performanceText.text = "--" + LanguageManager.CurrentLanguage.options.graphics_performance + "--";

            TextMeshProUGUI simplifiedExplosionsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Simpler Explosions"), "Text"));
            simplifiedExplosionsText.text = LanguageManager.CurrentLanguage.options.graphics_performanceSimpleExplosions;

            TextMeshProUGUI simplifiedFireText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Simpler Fire"), "Text"));
            simplifiedFireText.text = LanguageManager.CurrentLanguage.options.graphics_performanceSimpleFire;

            TextMeshProUGUI simplifiedSpawnText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Simpler Spawn Effects"), "Text"));
            simplifiedSpawnText.text = LanguageManager.CurrentLanguage.options.graphics_performanceSimpleSpawn;

            TextMeshProUGUI disabledParticlesText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Disable Environmental Particle Effects"), "Text"));
            disabledParticlesText.text = LanguageManager.CurrentLanguage.options.graphics_performanceDisableEnviParticles;

            TextMeshProUGUI simplehitParticlesText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Disable Environmental Hit Particles"), "Text"));
            simplehitParticlesText.text = LanguageManager.CurrentLanguage.options.graphics_performanceDisableEnviHitParticles;

            TextMeshProUGUI disableHeatWavesText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Disable Heat Waves"), "Text"));
            disableHeatWavesText.text = LanguageManager.CurrentLanguage.options.graphics_performanceDisableHeatWaves;

            //--GORE--
            TextMeshProUGUI goreSettingsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "-- Gore --"), "Text"));
            goreSettingsText.text = "--" + LanguageManager.CurrentLanguage.options.graphics_gore + "--\n<size=16>"
                + LanguageManager.CurrentLanguage.options.graphics_goreNote + "</size>";

            TextMeshProUGUI enableBloodandGoreText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Enable Blood & Gore"), "Text"));
            enableBloodandGoreText.text = LanguageManager.CurrentLanguage.options.graphics_goreEnable;

            TextMeshProUGUI freezeGoreText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Freeze Gore Physics"), "Text"));
            freezeGoreText.text = LanguageManager.CurrentLanguage.options.graphics_goreDisablePhysics;

            TextMeshProUGUI maxbloodstainText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Max Bloodstains"), "Text"));
            maxbloodstainText.text = LanguageManager.CurrentLanguage.options.graphics_goreMaxBloodStains;

            TextMeshProUGUI bloodstainChanceText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Bloodstain Chance"), "Text"));
            bloodstainChanceText.text = LanguageManager.CurrentLanguage.options.graphics_goreBloodChance;

            TextMeshProUGUI maxBloodText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(graphicsContent, "Max Gore Per Room"), "Text"));
            maxBloodText.text = LanguageManager.CurrentLanguage.options.graphics_goreMaxGore;
        }
        static public void PatchAudioOptions(GameObject optionsMenu)
        {
            //Audio options
            GameObject audioContent = GetGameObjectChild(optionsMenu, "Container");

            //-- Volume --
            TextMeshProUGUI audioTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "-- Volume --"), "Text"));
            audioTitle.text = "-- " + LanguageManager.CurrentLanguage.options.audio_volume + " --";

            TextMeshProUGUI masterVolumeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "Master"), "Text"));
            masterVolumeText.text = LanguageManager.CurrentLanguage.options.audio_globalVolume;

            TextMeshProUGUI soundEffectsVolumeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "Sound Effects"), "Text"));
            soundEffectsVolumeText.text = LanguageManager.CurrentLanguage.options.audio_soundEffectsVolume;

            TextMeshProUGUI sfxVolumeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "Music"), "Text"));
            sfxVolumeText.text = LanguageManager.CurrentLanguage.options.audio_musicVolume;

            //-- MISC --
            TextMeshProUGUI miscText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "-- Misc --"), "Text"));
            miscText.text = "-- " + LanguageManager.CurrentLanguage.options.general_misc + " --";

            TextMeshProUGUI subtitlesText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "Subtitles"), "Text"));
            subtitlesText.text = LanguageManager.CurrentLanguage.options.audio_subtitles;
            
            TextMeshProUGUI muffleMusicText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(audioContent, "Muffle Music While Underwater"), "Text"));
            muffleMusicText.text = LanguageManager.CurrentLanguage.options.audio_muffleMusic; 

        }
        static public void PatchAssistOptions(GameObject optionsMenu)
        {
            //Assist options

            GameObject assistMajorAssistPanel = GetGameObjectChild(GetGameObjectChild(optionsMenu, "Major Assists Consent"), "Panel");

            //Major Assist Consent panel
            TextMeshProUGUI assistDisclaimerText = GetTextMeshProUGUI(GetGameObjectChild(assistMajorAssistPanel, "Description Block"));
            assistDisclaimerText.text =

                LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimer1
                + "\n\n"
                + LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimer2
                + "\n\n"
                + LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimer3;
            assistDisclaimerText.fontSize = 18;

            TextMeshProUGUI assistDisclaimerConfirmText = GetTextMeshProUGUI(GetGameObjectChild(assistMajorAssistPanel, "Summary"));
            assistDisclaimerConfirmText.text = LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimerConfirm;
            assistDisclaimerConfirmText.fontSize = 24;

            TextMeshProUGUI assistDisclaimerYesText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistMajorAssistPanel, "Yes"), "Text"));
            assistDisclaimerYesText.text = LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimerConfirmYes;

            TextMeshProUGUI assistDisclaimerNoText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistMajorAssistPanel, "No"), "Text"));
            assistDisclaimerNoText.text = LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimerConfirmNo;

            //Assist Options
            GameObject assistContent = GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents");

            TextMeshProUGUI assistMinorAssistText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "-- Minor Assists --"), "Text"));
            assistMinorAssistText.text = "--" + LanguageManager.CurrentLanguage.options.assists_minor + "--";

            TextMeshProUGUI assistAutoAimText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Auto Aim"), "Text"));
            assistAutoAimText.text = LanguageManager.CurrentLanguage.options.assists_autoAim;

            TextMeshProUGUI assistAutoAimAmountText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Auto Aim Amount"), "Text"));
            assistAutoAimAmountText.text = LanguageManager.CurrentLanguage.options.assists_autoAimPercent;

            SliderValueToText autoAimSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(assistContent, "Auto Aim Amount"), "Slider Button(Clone)"), "Slider"), "Text").GetComponentInChildren<SliderValueToText>();
            autoAimSlider.ifMin = LanguageManager.CurrentLanguage.options.assists_autoAimPercentMinimum;
            autoAimSlider.ifMax = LanguageManager.CurrentLanguage.options.assists_autoAimPercentMaximum;

            TextMeshProUGUI assistEnemySilhouettesTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Enemy Silhouettes"), "Text"));
            assistEnemySilhouettesTitle.text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlines;

            GameObject assistEnemySilhouettes = GetGameObjectChild(assistContent, "Enemy Silhouettes"); 

            TextMeshProUGUI assistEnemySilhouettesOutlineText = GetTextMeshProUGUI(GetGameObjectChild(assistEnemySilhouettes, "Text"));
            assistEnemySilhouettesOutlineText.text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettes;

            GameObject silhouetteList = GetGameObjectChild(assistEnemySilhouettes, "Dropdown(Clone)");
            TMP_Dropdown silhouetteDropdown = silhouetteList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> silhouetteListText = silhouetteDropdown.options;
            silhouetteListText[0].text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesNone;
            silhouetteListText[1].text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlinesOnly;
            silhouetteListText[2].text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesFull;

            TextMeshProUGUI assistEnemySilhouettesDistance = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Activation Distance"),"Text"));
            assistEnemySilhouettesDistance.text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesDistance;

            TextMeshProUGUI assistEnemySilhouettesOutlineThickness = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Outline Thickness"), "Text"));
            assistEnemySilhouettesOutlineThickness.text =
            LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlineThickness;

            SliderValueToText assistEnemySilhouettesDistanceSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(assistContent, "Activation Distance"), "Slider Button(Clone)"), "Slider"), "Text").GetComponentInChildren<SliderValueToText>();
            assistEnemySilhouettesDistanceSlider.ifMin = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesDistanceMinimum;

            //TextMeshProUGUI assistEnemySilhouettesOutlinesOnlyText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistEnemySilhouettesExtra, "Extra"), "Text (2)"));
            //assistEnemySilhouettesOutlinesOnlyText.text = LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlinesOnly;

            GameObject assistsMajorTitleObject = GetGameObjectChild(assistContent, "-- Major Assists --");
            TextMeshProUGUI assistsMajorTitle = GetTextMeshProUGUI(GetGameObjectChild(assistsMajorTitleObject, "Text"));
            assistsMajorTitle.text = "--" + LanguageManager.CurrentLanguage.options.assists_major + "--";
            assistsMajorTitle.fontSize = 20;
            TextMeshProUGUI assistsMajorActivateText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistsMajorTitleObject, "Enable Group"), "Text"));
            assistsMajorActivateText.text = LanguageManager.CurrentLanguage.options.assists_majorActivate;

            TextMeshProUGUI assistsMajorGameSpeedText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Game Speed"), "Text"));
            assistsMajorGameSpeedText.text = LanguageManager.CurrentLanguage.options.assists_gameSpeed;

            TextMeshProUGUI assistsDamageTakenText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Damage Taken"), "Text"));
            assistsDamageTakenText.text = LanguageManager.CurrentLanguage.options.assists_damageTaken;

            GameObject bossOverride = GetGameObjectChild(assistContent, "Boss Fight Difficulty Override");

            TextMeshProUGUI assistsBossOverrideText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Boss Fight Difficulty Override"), "Text"));
            assistsBossOverrideText.text = LanguageManager.CurrentLanguage.options.assists_bossOverride;

            TextMeshProUGUI assistsBossRestartText = GetTextMeshProUGUI(GetGameObjectChild(bossOverride, "Side Note"));
            assistsBossRestartText.text = LanguageManager.CurrentLanguage.options.assists_bossRestartRequired;

            TMP_Dropdown bossOverrideDropdown = GetGameObjectChild(bossOverride, "Dropdown(Clone)").GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> bossOverrideDropdownListText = bossOverrideDropdown.options;

            bossOverrideDropdownListText[0].text = LanguageManager.CurrentLanguage.options.assists_bossOverrideNone;
            bossOverrideDropdownListText[1].text = LanguageManager.CurrentLanguage.frontend.difficulty_harmless;
            bossOverrideDropdownListText[2].text = LanguageManager.CurrentLanguage.frontend.difficulty_lenient;
            bossOverrideDropdownListText[3].text = LanguageManager.CurrentLanguage.frontend.difficulty_standard;
            bossOverrideDropdownListText[4].text = LanguageManager.CurrentLanguage.frontend.difficulty_violent;
            bossOverrideDropdownListText[5].text = LanguageManager.CurrentLanguage.frontend.difficulty_brutal;
            //bossOverrideDropdownListText[6].text = LanguageManager.CurrentLanguage.frontend.difficulty_umd;

            TextMeshProUGUI assistsInfiniteStaminaText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Infinite Stamina"), "Text"));
            assistsInfiniteStaminaText.text = LanguageManager.CurrentLanguage.options.assists_infiniteEnergy;

            TextMeshProUGUI assistsDisableWhiplashHardDamageText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Disable Whiplash Hard Damage"), "Text"));
            assistsDisableWhiplashHardDamageText.text = LanguageManager.CurrentLanguage.options.assists_disableWhiplashHardDamage;

            TextMeshProUGUI assistsDisableHardDamageText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Disable All Hard Damage"), "Text"));
            assistsDisableHardDamageText.text = LanguageManager.CurrentLanguage.options.assists_disableHardDamage;

            TextMeshProUGUI assistsDisableWeaponFreshnessText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Disable Weapon Freshness"), "Text"));
            assistsDisableWeaponFreshnessText.text = LanguageManager.CurrentLanguage.options.assists_disableWeaponFreshness;

            TextMeshProUGUI assistsDisablePopupText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(assistContent, "Disable Assist Popup"), "Text"));
            assistsDisablePopupText.text = LanguageManager.CurrentLanguage.options.assists_disablePopupHints;

        }
        static public void PatchSavesOptions(GameObject optionMenu)
        {
            //Save options
            GameObject saveReloadPanel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionMenu, "Reload Consent Blocker"), "Consent"), "Panel");
            
            TextMeshProUGUI saveReloadText = GetTextMeshProUGUI(GetGameObjectChild(saveReloadPanel, "Text"));
            TextMeshProUGUI saveReloadYes = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(saveReloadPanel, "Yes"), "Text"));
            TextMeshProUGUI saveReloadNo = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(saveReloadPanel, "No"), "Text"));
            
            saveReloadText.text =
                "<color=red>" + LanguageManager.CurrentLanguage.options.save_warning1 + "</color>\n\n" +
                LanguageManager.CurrentLanguage.options.save_warning2;

            saveReloadYes.text = LanguageManager.CurrentLanguage.options.save_reloadYes;
            saveReloadNo.text = LanguageManager.CurrentLanguage.options.save_reloadNo;
            
            GameObject saveDeletePanel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionMenu, "Wipe Consent Blocker"), "Consent"), "Panel");
            
            TextMeshProUGUI saveDeleteYes = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(saveDeletePanel, "Yes"), "Text"));
            saveDeleteYes.text = "<color=red>" + LanguageManager.CurrentLanguage.options.save_deleteYes + "</color>";

            TextMeshProUGUI saveDeleteNo = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(saveDeletePanel, "No"), "Text"));
            saveDeleteNo.text = LanguageManager.CurrentLanguage.options.save_deleteNo;

            TextMeshProUGUI saveSlotsClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(optionMenu, "Close"), "Text"));
            saveSlotsClose.text = LanguageManager.CurrentLanguage.options.save_close;
        }
        //general end
        //customization starts here
        static public void PatchHUDOptions(GameObject optionsMenu)
        {
            //HUD options
            GameObject hudContent = GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents");

            TextMeshProUGUI hudTitle = GetTextMeshProUGUI(GetGameObjectChild(hudContent.transform.GetChild(0).gameObject, "Text"));
            hudTitle.text = "--" + LanguageManager.CurrentLanguage.options.category_general + "--";

            TextMeshProUGUI hudTypeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "HUD Type"), "Text"));
            hudTypeText.text = LanguageManager.CurrentLanguage.options.hud_type;

            GameObject hudType = GetGameObjectChild(GetGameObjectChild(hudContent, "HUD Type"), "Dropdown(Clone)");
            TMP_Dropdown hudTypeDropdown = hudType.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> hudTypeDropdownListText = hudTypeDropdown.options;

            hudTypeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.hud_typeNone;
            hudTypeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.hud_typeStandard;
            hudTypeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.hud_typeClassicColor;
            hudTypeDropdownListText[3].text = LanguageManager.CurrentLanguage.options.hud_typeClassicWhite;

            TextMeshProUGUI backgroundOpacityText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Background Opacity"), "Text"));
            backgroundOpacityText.text = LanguageManager.CurrentLanguage.options.hud_backgroundOpacity;

            SliderValueToText backgroundOpacitySlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(hudContent, "Background Opacity"), "Slider Button(Clone)"), "Slider").GetComponentInChildren<SliderValueToText>();

            backgroundOpacitySlider.ifMin = LanguageManager.CurrentLanguage.options.hud_backgroundOpacityMinimum;
            backgroundOpacitySlider.ifMax = LanguageManager.CurrentLanguage.options.hud_backgroundOpacityMaximum;

            TextMeshProUGUI alwaysOnTopText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Always On Top"), "Text"));
            alwaysOnTopText.text = LanguageManager.CurrentLanguage.options.hud_alwaysOnTop;

            GameObject iconsObject = GetGameObjectChild(hudContent, "Cheat & Sandbox Icons");
            TextMeshProUGUI iconsText = GetTextMeshProUGUI(GetGameObjectChild(iconsObject, "Text"));
            iconsText.text = LanguageManager.CurrentLanguage.options.hud_icons;

            TextMeshProUGUI reduceHudMotion = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "REDUCE HUD MOTION"), "Text"));
            reduceHudMotion.text = LanguageManager.CurrentLanguage.options.hud_reduceHudMotion;

            TMP_Dropdown iconsDropdown = iconsObject.GetComponentInChildren<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> iconsDropdownListText = iconsDropdown.options;

            iconsDropdownListText[0].text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_default;
            iconsDropdownListText[1].text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_pitr;

            TextMeshProUGUI hudElements = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "-- Elements --"), "Text"));
            hudElements.text = "--" + LanguageManager.CurrentLanguage.options.hud_hudElements + "--";

            TextMeshProUGUI weaponIconText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Weapon Icon"), "Text"));
            weaponIconText.text = LanguageManager.CurrentLanguage.options.hud_weaponIcon;

            TextMeshProUGUI armIconText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Arm Icon"), "Text"));
            armIconText.text = LanguageManager.CurrentLanguage.options.hud_armIcon;

            TextMeshProUGUI railcannonMeterText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Railcannon Meter"), "Text"));
            railcannonMeterText.text = LanguageManager.CurrentLanguage.options.hud_railcannonMeter;

            TextMeshProUGUI styleMeterText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Style Meter"), "Text"));
            styleMeterText.text = LanguageManager.CurrentLanguage.options.hud_styleMeter;

            TextMeshProUGUI styleInfoText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Style Info"), "Text"));
            styleInfoText.text = LanguageManager.CurrentLanguage.options.hud_styleInfo;

            GameObject speedoMeterDD = GetGameObjectChild(hudContent, "Speedometer");
            TextMeshProUGUI speedoMeterText = GetTextMeshProUGUI(GetGameObjectChild(speedoMeterDD, "Text"));
            speedoMeterText.text = LanguageManager.CurrentLanguage.options.hud_speedoMeterText;

            TMP_Dropdown speedoMeterTypeDropdown = speedoMeterDD.GetComponentInChildren<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> speedoMeterTypeDropdownListText = speedoMeterTypeDropdown.options;
            speedoMeterTypeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeOff;
            speedoMeterTypeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeOn;
            speedoMeterTypeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeHorizonal;
            speedoMeterTypeDropdownListText[3].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeVertical;
            
            //Crosshair settings

            TextMeshProUGUI crosshairTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "-- Crosshair --"),"Text"));
            crosshairTitle.text = "--" + LanguageManager.CurrentLanguage.options.crosshair_title + "--";

            TextMeshProUGUI crosshairTypeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Type"), "Text"));
            crosshairTypeText.text = LanguageManager.CurrentLanguage.options.crosshair_type;

            GameObject crosshairType = GetGameObjectChild(GetGameObjectChild(hudContent, "Type"), "Dropdown(Clone)");
            TMP_Dropdown crosshairTypeDropdown = crosshairType.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> crosshairTypeDropdownListText = crosshairTypeDropdown.options;

            crosshairTypeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.crosshair_typeNone;
            crosshairTypeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.crosshair_typeSmall;
            crosshairTypeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.crosshair_typeLarge;

            TextMeshProUGUI crosshairColorText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Color"), "Text"));
            crosshairColorText.text = LanguageManager.CurrentLanguage.options.crosshair_color;

            GameObject crosshairColor = GetGameObjectChild(GetGameObjectChild(hudContent, "Color"), "Dropdown(Clone)");
            TMP_Dropdown crosshairColorDropdown = crosshairColor.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> crosshairColorDropdownListText = crosshairColorDropdown.options;

            crosshairColorDropdownListText[0].text = LanguageManager.CurrentLanguage.options.crosshair_colorInverted;
            crosshairColorDropdownListText[1].text = LanguageManager.CurrentLanguage.options.crosshair_colorWhite;
            crosshairColorDropdownListText[2].text = LanguageManager.CurrentLanguage.options.crosshair_colorGrey;
            crosshairColorDropdownListText[3].text = LanguageManager.CurrentLanguage.options.crosshair_colorBlack;
            crosshairColorDropdownListText[4].text = LanguageManager.CurrentLanguage.options.crosshair_colorRed;
            crosshairColorDropdownListText[5].text = LanguageManager.CurrentLanguage.options.crosshair_colorGreen;
            crosshairColorDropdownListText[6].text = LanguageManager.CurrentLanguage.options.crosshair_colorBlue;
            crosshairColorDropdownListText[7].text = LanguageManager.CurrentLanguage.options.crosshair_colorCyan;
            crosshairColorDropdownListText[8].text = LanguageManager.CurrentLanguage.options.crosshair_colorYellow;
            crosshairColorDropdownListText[9].text = LanguageManager.CurrentLanguage.options.crosshair_colorMagenta;

            TextMeshProUGUI crosshairHudSizeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Crosshair HUD Size"), "Text"));
            crosshairHudSizeText.text = LanguageManager.CurrentLanguage.options.crosshair_size;

            GameObject crosshairSize = GetGameObjectChild(GetGameObjectChild(hudContent, "Crosshair HUD Size"), "Dropdown(Clone)");
            TMP_Dropdown crosshairSizeDropdown = crosshairSize.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> crosshairSizeDropdownListText = crosshairSizeDropdown.options;

            crosshairSizeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.crosshair_sizeNone;
            crosshairSizeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.crosshair_sizeThin;
            crosshairSizeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.crosshair_sizeMedium;
            crosshairSizeDropdownListText[3].text = LanguageManager.CurrentLanguage.options.crosshair_sizeThick;
            crosshairSizeDropdownListText[4].text = LanguageManager.CurrentLanguage.options.crosshair_sizeVeryThick;

            TextMeshProUGUI crosshairHudFadeText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Crosshair HUD Fade"), "Text"));
            crosshairHudFadeText.text = LanguageManager.CurrentLanguage.options.crosshair_hudFade;

            TextMeshProUGUI crosshairPowerupText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(hudContent, "Powerup Meter"), "Text"));
            crosshairPowerupText.text = LanguageManager.CurrentLanguage.options.crosshair_powerupBar;

        }
        
        private void PatchColorsOptions(GameObject optionsMenu)
        {
            //Colors options
            //TextMeshProUGUI colorsPanel = GetTextMeshProUGUI(GetGameObjectChild(optionsMenu, "Text (1)"));
            //colorsPanel.text = "--" + LanguageManager.CurrentLanguage.options.colors_title + "--";

            TextMeshProUGUI colorsResetDefaultText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents"), "Default"), "Text"));
            colorsResetDefaultText.text = LanguageManager.CurrentLanguage.options.colors_reset;

            //HUD Text
            GameObject colorsHudObject = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents"), "HUD");

            TextMeshProUGUI colorsHudText = GetTextMeshProUGUI(colorsHudObject);
            colorsHudText.text = "--" + LanguageManager.CurrentLanguage.options.colors_hud + "--";

            TextMeshProUGUI colorsHudHealthText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Health"), "Text"));
            colorsHudHealthText.text = LanguageManager.CurrentLanguage.options.colors_hudHealth;

            TextMeshProUGUI colorsHudHealthNumberText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "HpText"), "Text"));
            colorsHudHealthNumberText.text = LanguageManager.CurrentLanguage.options.colors_hudHealthNumber;

            TextMeshProUGUI colorsHudSoftDamageText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "AfterImage"), "Text"));
            colorsHudSoftDamageText.text = LanguageManager.CurrentLanguage.options.colors_hudDamage;

            TextMeshProUGUI colorsHudHardDamageText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "AntiHp"), "Text"));
            colorsHudHardDamageText.text = LanguageManager.CurrentLanguage.options.colors_hudHardDamage;

            TextMeshProUGUI colorsHudOverhealText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Overheal"), "Text"));
            colorsHudOverhealText.text = LanguageManager.CurrentLanguage.options.colors_hudOverheal;

            TextMeshProUGUI colorsHudStaminaText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Stamina"), "Text"));
            colorsHudStaminaText.text = LanguageManager.CurrentLanguage.options.colors_hudEnergyFull;

            TextMeshProUGUI colorsHudStaminaChargingText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "StaminaCharging"), "Text"));
            colorsHudStaminaChargingText.text = LanguageManager.CurrentLanguage.options.colors_hudEnergyPartial;

            TextMeshProUGUI colorsHudStaminaEmptyText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "StaminaEmpty"), "Text"));
            colorsHudStaminaEmptyText.text = LanguageManager.CurrentLanguage.options.colors_hudEnergyEmpty;

            TextMeshProUGUI colorsHudRailcannonFullText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "RailcannonFull"), "Text"));
            colorsHudRailcannonFullText.text = LanguageManager.CurrentLanguage.options.colors_railcannonFull;

            TextMeshProUGUI colorsHudRailcannonChargingText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "RailcannonCharging"), "Text"));
            colorsHudRailcannonChargingText.text = LanguageManager.CurrentLanguage.options.colors_railcannonPartial;

            TextMeshProUGUI colorsHudVarBlueText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Blue Variation"), "Text"));
            colorsHudVarBlueText.text = LanguageManager.CurrentLanguage.options.colors_variationBlue;

            TextMeshProUGUI colorsHudVarGreenText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Green Variation"), "Text"));
            colorsHudVarGreenText.text = LanguageManager.CurrentLanguage.options.colors_variationGreen;

            TextMeshProUGUI colorsHudVarRedText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Red Variation"), "Text"));
            colorsHudVarRedText.text = LanguageManager.CurrentLanguage.options.colors_variationRed;

            TextMeshProUGUI colorsHudVarGoldText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsHudObject, "Gold Variation"), "Text"));
            colorsHudVarGoldText.text = LanguageManager.CurrentLanguage.options.colors_variationGold;

            //Enemy names text
            //Later down the line, could be better to get the names from EnemyBios.
            GameObject colorsEnemiesObject = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionsMenu, "Scroll Rect"), "Contents"), "Enemies");

            TextMeshProUGUI colorsEnemiesText = GetTextMeshProUGUI(colorsEnemiesObject);
            colorsEnemiesText.text = "--" + LanguageManager.CurrentLanguage.options.colors_enemies + "--";

            TextMeshProUGUI colorsEnemiesFilthText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Filth"), "Text"));
            colorsEnemiesFilthText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_filth;

            TextMeshProUGUI colorsEnemiesStrayText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Stray"), "Text"));
            colorsEnemiesStrayText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_stray;

            TextMeshProUGUI colorsEnemiesMalFaceText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Malicious Face"), "Text"));
            colorsEnemiesMalFaceText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_malFace;

            TextMeshProUGUI colorsEnemiesSchismText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Schism"), "Text"));
            colorsEnemiesSchismText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_schism;

            TextMeshProUGUI colorsEnemiesSwordsmachineText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Swordsmachine"), "Text"));
            colorsEnemiesSwordsmachineText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_swordsmachine;

            TextMeshProUGUI colorsEnemiesCerberusText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Cerberus"), "Text"));
            colorsEnemiesCerberusText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_cerberus;

            TextMeshProUGUI colorsEnemiesDroneText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Drone"), "Text"));
            colorsEnemiesDroneText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_drone;

            TextMeshProUGUI colorsEnemiesStreetcleanerText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Streetcleaner"), "Text"));
            colorsEnemiesStreetcleanerText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_streetCleaner;

            TextMeshProUGUI colorsEnemiesSoldierText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Shotgunner"), "Text"));
            colorsEnemiesSoldierText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_soldier;

            TextMeshProUGUI colorsEnemiesV2Text = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "V2"), "Text"));
            colorsEnemiesV2Text.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_v2;

            TextMeshProUGUI colorsEnemiesMindflayerText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Mindflayer"), "Text"));
            colorsEnemiesMindflayerText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_mindFlayer;

            TextMeshProUGUI colorsEnemiesVirtueText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Virtue"), "Text"));
            colorsEnemiesVirtueText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_virtue;

            TextMeshProUGUI colorsEnemiesStalkerText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Stalker"), "Text"));
            colorsEnemiesStalkerText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_stalker;

            TextMeshProUGUI colorsEnemiesSisyphusText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Sisyphus"), "Text"));
            colorsEnemiesSisyphusText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_insurrectionist;

            TextMeshProUGUI colorsEnemiesSentryText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Sentry"), "Text"));
            colorsEnemiesSentryText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_sentry;

            TextMeshProUGUI colorsEnemiesIdolText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Idol"), "Text"));
            colorsEnemiesIdolText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_idol;

            TextMeshProUGUI colorsEnemiesFerrymanText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Ferryman"), "Text"));
            colorsEnemiesFerrymanText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_ferryman;

            TextMeshProUGUI colorsEnemiesMannequinText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Mannequin"), "Text"));
            colorsEnemiesMannequinText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_mannequin;

            TextMeshProUGUI colorsEnemiesGuttermanText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Gutterman"), "Text"));
            colorsEnemiesGuttermanText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_gutterman;

            TextMeshProUGUI colorsEnemiesGuttertankText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(colorsEnemiesObject, "Guttertank"), "Text"));
            colorsEnemiesGuttertankText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_guttertank;

        }
        

        //Does not work for some reason, nothing gets translated
        private void PatchRumbleOptions(GameObject optionMenu)
        {
            TextMeshProUGUI rumbleSettingsTitle = GetTextMeshProUGUI(GetGameObjectChild(optionMenu, "Text (1)"));
            rumbleSettingsTitle.text = LanguageManager.CurrentLanguage.options.rumble_title;

            TextMeshProUGUI rumbleFinalMultiplier = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(optionMenu, "Total"), "Text"));
            rumbleFinalMultiplier.text = LanguageManager.CurrentLanguage.options.rumble_finalMultiplier;

            TextMeshProUGUI rumbleCloseButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(optionMenu, "Close"), "Text"));
            rumbleCloseButton.text = LanguageManager.CurrentLanguage.options.save_close;

            //Loop through each entry
            GameObject rumbleEntryList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionMenu, "Scroll View"), "Viewport"), "Content");
            try
            {
                for (int x = 0; x < 21; x++) //Hardcoded, amount may increase in future updates
                {
                    GameObject entry = rumbleEntryList.transform.GetChild(x).gameObject;
                    //Throws an out of bounds error, but still swaps the text correctly...
                    TextMeshProUGUI entryIntensity = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(entry, "Button"), "Text (1)"));
                    entryIntensity.text = LanguageManager.CurrentLanguage.options.rumble_intensity;

                    TextMeshProUGUI entryResetIntensity = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(entry, "Default Button (1)"), "Text"));
                    entryResetIntensity.text = LanguageManager.CurrentLanguage.options.rumble_reset;

                    TextMeshProUGUI entryEndDelay = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(entry, "End Delay Container"), "Text (2)"));
                    entryEndDelay.text = LanguageManager.CurrentLanguage.options.rumble_endDelay;

                    TextMeshProUGUI entryResetEndDelay = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(entry, "End Delay Container"), "Default Button"), "Text"));
                    entryResetEndDelay.text = LanguageManager.CurrentLanguage.options.rumble_reset;
                }
            }
            catch (Exception)
            {
                Logging.Warn("Rumble options exception, should be harmless unless if console is spammed with this");
            }

        }
        
        private void PatchAdvancedOptions(GameObject optionMenu)
        {
            GameObject advancedOptions = optionMenu;

            TextMeshProUGUI advancedOptionsTitle = GetTextMeshProUGUI(GetGameObjectChild(advancedOptions, "Title"));
            advancedOptionsTitle.text = "--" + LanguageManager.CurrentLanguage.options.advanced_title + "--";

            TextMeshProUGUI advancedOptionsClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptions, "Close"), "Text"));
            advancedOptionsClose.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_returnButton;

            //Cybergrind Reset Confirm
            GameObject cybergrindResetPanel = GetGameObjectChild(GetGameObjectChild(advancedOptions, "Reset Cyber Grind Dialog"), "Panel");

            TextMeshProUGUI cybergrindResetText1 = GetTextMeshProUGUI(GetGameObjectChild(cybergrindResetPanel, "Text (2)"));
            TextMeshProUGUI cybergrindResetText2 = GetTextMeshProUGUI(GetGameObjectChild(cybergrindResetPanel, "Text (1)"));
            cybergrindResetText1.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetText1;
            cybergrindResetText2.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetText2;

            TextMeshProUGUI cybergrindResetCancel = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cybergrindResetPanel, "Cancel") , "Text"));
            TextMeshProUGUI cybergrindResetConfirm = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cybergrindResetPanel, "Confirm"), "Text"));
            cybergrindResetCancel.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetCancel;
            cybergrindResetConfirm.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetConfirm;

            //The Actual Options
            GameObject advancedOptionsSub = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptions, "Scroll View"), "Viewport"), "Content");

            TextMeshProUGUI advancedCybergrindTitle = GetTextMeshProUGUI(GetGameObjectChild(advancedOptionsSub, "Cyber Grind Category"));
            advancedCybergrindTitle.text = LanguageManager.CurrentLanguage.levelNames.levelName_cybergrind;

            TextMeshProUGUI advancedCybergrindReset = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "Cyber Grind Options"), "Local High Scores"), "Text"));
            TextMeshProUGUI advancedCybergrindResetButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "Cyber Grind Options"), "Local High Scores"), "Reset"), "Text"));
            advancedCybergrindReset.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindLocalHighScore;
            advancedCybergrindResetButton.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetButton;

            TextMeshProUGUI advancedSteamTitle = GetTextMeshProUGUI(GetGameObjectChild(advancedOptionsSub, "Steam Category"));
            advancedSteamTitle.text = LanguageManager.CurrentLanguage.options.advanced_steam;

            TextMeshProUGUI advancedSteamLeaderboardManage = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "Leaderboards"), "Leaderboards"), "Text"));
            TextMeshProUGUI advancedSteamLeaderboardManageButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "Leaderboards"), "Leaderboards"), "Manage Button"), "Text"));
            advancedSteamLeaderboardManage.text = LanguageManager.CurrentLanguage.options.advanced_steamLeaderboardManage;
            advancedSteamLeaderboardManageButton.text = LanguageManager.CurrentLanguage.options.advanced_steamLeaderboardManageButton;

            //"Current" thingy and the level titles
            TextMeshProUGUI advancedCurrent52 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "5-2 Options"), "Level 5-2 Category"), "Current Level Indicator"));
            TextMeshProUGUI advancedTitle52 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "5-2 Options"), "Level 5-2 Category"));
            TextMeshProUGUI advancedCurrent71 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-1 Options"), "Level 7-1 Category"), "Current Level Indicator"));
            TextMeshProUGUI advancedTitle71 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-1 Options"), "Level 7-1 Category"));
            TextMeshProUGUI advancedCurrent73 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-3 Options"), "Level 7-3 Category"), "Current Level Indicator"));
            TextMeshProUGUI advancedTitle73 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-3 Options"), "Level 7-3 Category"));
            TextMeshProUGUI advancedCurrent7S = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-S Options"), "Level 7-S Category"), "Current Level Indicator"));
            TextMeshProUGUI advancedTitle84 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "8-4 Options"), "Level 8-4 Category"));
            TextMeshProUGUI advancedCurrent84 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "8-4 Options"), "Level 8-4 Category"), "Current Level Indicator"));
            TextMeshProUGUI advancedTitle7S = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-S Options"), "Level 7-S Category"));
            TextMeshProUGUI advancedCurrentP2 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "P-2 Options"), "Level P-2 Category"), "Current Level Indicator"));
            TextMeshProUGUI advancedTitleP2 = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "P-2 Options"), "Level P-2 Category"));
            advancedCurrent52.text = LanguageManager.CurrentLanguage.options.advanced_currentLevel;
            advancedTitle52.text = LanguageManager.CurrentLanguage.options.advanced_level52;
            advancedCurrent71.text = LanguageManager.CurrentLanguage.options.advanced_currentLevel;
            advancedTitle71.text = LanguageManager.CurrentLanguage.options.advanced_level71;
            advancedCurrent73.text = LanguageManager.CurrentLanguage.options.advanced_currentLevel;
            advancedTitle73.text = LanguageManager.CurrentLanguage.options.advanced_level73;
            advancedCurrent84.text = LanguageManager.CurrentLanguage.options.advanced_currentLevel;
            advancedTitle84.text = LanguageManager.CurrentLanguage.options.advanced_level84;
            advancedCurrent7S.text = LanguageManager.CurrentLanguage.options.advanced_currentLevel;
            advancedTitle7S.text = LanguageManager.CurrentLanguage.options.advanced_level7S;
            advancedCurrentP2.text = LanguageManager.CurrentLanguage.options.advanced_currentLevel;
            advancedTitleP2.text = LanguageManager.CurrentLanguage.options.advanced_levelP2;

            //Levels
            TextMeshProUGUI advanced52WaterScrolling = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "5-2 Options"), "Disable Water Scrolling"), "Text"));
            TextMeshProUGUI advanced52WaterWaves = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "5-2 Options"), "Disable Water Waves"), "Text"));
            advanced52WaterScrolling.text = LanguageManager.CurrentLanguage.options.advanced_52WaterScrolling;
            advanced52WaterWaves.text = LanguageManager.CurrentLanguage.options.advanced_52WaterWaves;

            TextMeshProUGUI advanced71Dark = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-1 Options"), "Local High Scores"), "Text"));
            advanced71Dark.text = LanguageManager.CurrentLanguage.options.advanced_71Dark;

            TextMeshProUGUI advanced73Grass = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-3 Options"), "Local High Scores"), "Text"));
            advanced73Grass.text = LanguageManager.CurrentLanguage.options.advanced_73Grass;

            TextMeshProUGUI advanced84DisableArenaScrolling = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "8-4 Options"), "Local High Scores (1)"), "Text"));
            TextMeshProUGUI advanced84DisableArenaRotation = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "8-4 Options"), "Local High Scores"), "Text"));
            advanced84DisableArenaScrolling.text = LanguageManager.CurrentLanguage.options.advanced_84DisableArenaScrolling;
            advanced84DisableArenaRotation.text = LanguageManager.CurrentLanguage.options.advanced_84DisableArenaRotation;

            TextMeshProUGUI advanced7SHard = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "7-S Options"), "Local High Scores"), "Text"));
            advanced7SHard.text = LanguageManager.CurrentLanguage.options.advanced_7SHard;

            TextMeshProUGUI advanced_P2DisableTunnelScrolling = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(advancedOptionsSub, "P-2 Options"), "Local High Scores"), "Text"));
            advanced_P2DisableTunnelScrolling.text = LanguageManager.CurrentLanguage.options.advanced_P2DisableTunnelScrolling;
            
        }
        
        private void PatchOptions(GameObject optionsMenu)
        {
            if (optionsMenu != null)
            {
                //Main buttons and text
                if (GetGameObjectChild(optionsMenu, "Text") != null)
                {
                    TextMeshProUGUI optionsText = GetTextMeshProUGUI(GetGameObjectChild(optionsMenu, "Text"));
                    optionsText.text = "--" + LanguageManager.CurrentLanguage.options.options_title + "--";
                }

                GameObject leftColumn = GetGameObjectChild(optionsMenu, "Navigation Rail");

                TextMeshProUGUI generalText = GetTextMeshProUGUI(GetGameObjectChild(leftColumn, "Text (7)"));
                generalText.text = "-- " + LanguageManager.CurrentLanguage.options.category_general + " --";

                TextMeshProUGUI generalButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "General"), "Text"));
                generalButton.text = LanguageManager.CurrentLanguage.options.category_general;

                TextMeshProUGUI controlButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Controls"), "Text"));
                controlButton.text = LanguageManager.CurrentLanguage.options.category_controls;

                TextMeshProUGUI graphicsButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Video"), "Text"));
                graphicsButton.text = LanguageManager.CurrentLanguage.options.category_graphics;

                TextMeshProUGUI audioButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Audio"), "Text"));
                audioButton.text = LanguageManager.CurrentLanguage.options.category_audio;

                TextMeshProUGUI assistButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Assist"), "Text"));
                assistButton.text = LanguageManager.CurrentLanguage.options.category_assists;

                TextMeshProUGUI savesButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Saves"), "Text"));
                savesButton.text = LanguageManager.CurrentLanguage.options.category_saves;

                TextMeshProUGUI customizationText = GetTextMeshProUGUI(GetGameObjectChild(leftColumn, "Text (8)"));
                customizationText.text = "-- " + LanguageManager.CurrentLanguage.options.category_customization + " --";

                TextMeshProUGUI hudButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "HUD"), "Text"));
                hudButton.text = LanguageManager.CurrentLanguage.options.category_hud;

                TextMeshProUGUI colorsButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Colors"), "Text"));
                colorsButton.text = LanguageManager.CurrentLanguage.options.category_colors;

                TextMeshProUGUI backText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(leftColumn, "Back"), "Text"));
                backText.text = LanguageManager.CurrentLanguage.options.options_back;

                TextMeshProUGUI paletteSelectorClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionsMenu, "Palette Selector"),"Close"),"Text"));
                paletteSelectorClose.text = LanguageManager.CurrentLanguage.options.save_close;

                try
                {
                    GameObject savesOptions = GetGameObjectChild(optionsMenu, "Save Slots");
                    try { PatchSavesOptions(savesOptions); } catch (Exception e) { Logging.Error("Failed to patch save options."); Logging.Error(e.ToString()); }
                    GameObject colorblindOptions = GetGameObjectChild(GetGameObjectChild(optionsMenu, "Pages"), "ColorBlindness Options");
                    try { PatchColorsOptions(colorblindOptions); } catch (Exception e) { Logging.Error("Failed to patch color options."); Logging.Error(e.ToString()); }
                    GameObject rumbleOptions = GetGameObjectChild(optionsMenu, "Rumble Settings");
                    try { PatchRumbleOptions(rumbleOptions); } catch (Exception e) { Logging.Error("Failed to patch rumble options."); Logging.Error(e.ToString()); }
                    GameObject advancedOptions = GetGameObjectChild(optionsMenu, "Advanced Options");
                    try { PatchAdvancedOptions(advancedOptions); } catch (Exception e) { Logging.Error("Failed to patch advanced options."); Logging.Error(e.ToString()); }
                    GameObject steamOptions = GetGameObjectChild(optionsMenu, "Leaderboard Manager");
                    try { PatchSteamLeaderboard(steamOptions); } catch (Exception e) { Logging.Error("Failed to patch steam leaderboard."); Logging.Error(e.ToString()); }
                }
                catch (Exception e)
                {
                    Logging.Error("Something went wrong while patching options.");
                    Logging.Error(e.ToString());
                }

            }
            else
            {
                Logging.Error("An error occured while patching options menu");
            }

        }

        private void PatchSteamLeaderboard(GameObject optionMenu)
        {
            TextMeshProUGUI steamLeaderboardTitle = GetTextMeshProUGUI(GetGameObjectChild(optionMenu, "Title"));
            steamLeaderboardTitle.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_title;

            TextMeshProUGUI steamLeaderboardRefreshButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(optionMenu, "Refresh Button"), "Text"));
            steamLeaderboardRefreshButton.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_refreshButton;

            TextMeshProUGUI steamLeaderboardReturnButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(optionMenu, "Close"), "Text"));
            steamLeaderboardReturnButton.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_returnButton;

            //Loop through each entry
            GameObject SteamEntryList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionMenu, "Scroll View"), "Viewport"), "Content");
            try
            {
                for (int x = 0; x < 35; x++) //Hardcoded, amount may increase in future updates
                {
                    GameObject entry = SteamEntryList.transform.GetChild(x).gameObject;

                    TextMeshProUGUI entryAnyLabel = GetTextMeshProUGUI(GetGameObjectChild(entry, "Any Label"));
                    entryAnyLabel.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_anyLabel;

                    TextMeshProUGUI entryPLabel = GetTextMeshProUGUI(GetGameObjectChild(entry, "P Label"));
                    entryPLabel.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_pLabel;

                    TextMeshProUGUI entryAnyReset = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(entry, "Any Reset"), "Text"));
                    entryAnyReset.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_reset;

                    TextMeshProUGUI entryPReset = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(entry, "P Reset Button"), "Text"));
                    entryPReset.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_reset;
                }
            }
            catch (Exception e)
            {
                Logging.Error("Something went wrong while patching Steam Leaderboard.");
                Logging.Error(e.ToString());
            }

        }

        public Options(ref GameObject game)
        {
            //Options are in two different locations.
            //On the main menu, it's root/Canvas/OptionsMenu.
            //In-game it's root/Canvas/OptionsMenu.
            if (GetCurrentSceneName() == "Main Menu")
            {
                this.optionsMenu = GetGameObjectChild(game, "OptionsMenu");
            }
            else
            {
                List<GameObject> rootObjects = new List<GameObject>();
                SceneManager.GetActiveScene().GetRootGameObjects(rootObjects);
                GameObject pauseObject = null;
                foreach (GameObject a in rootObjects)
                {
                    if (a.gameObject.name == "Canvas")
                    {
                        pauseObject = a.gameObject;
                        break;
                    }
                }
                this.optionsMenu = GetGameObjectChild(pauseObject, "OptionsMenu");
            }
            this.PatchOptions(this.optionsMenu);
        }
    }
}
