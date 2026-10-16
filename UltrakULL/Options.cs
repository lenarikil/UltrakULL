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

        private static void SetText(GameObject obj, string text)
        {
            if (obj == null) return;
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }

        private static TMP_Dropdown GetDropdown(GameObject obj)
        {
            if (obj == null) return null;
            return obj.GetComponent<TMP_Dropdown>();
        }

        private static void SetDropdownText(TMP_Dropdown dropdown, int index, string text)
        {
            if (dropdown == null) return;
            if (dropdown.options != null && index < dropdown.options.Count)
                dropdown.options[index].text = text;
        }

        private static void SetSliderMinText(GameObject obj, string minText)
        {
            if (obj == null) return;
            SliderValueToText slider = obj.GetComponentInChildren<SliderValueToText>();
            if (slider != null) slider.ifMin = minText;
        }

        private static void SetSliderMinMaxText(GameObject obj, string minText, string maxText)
        {
            if (obj == null) return;
            SliderValueToText slider = obj.GetComponentInChildren<SliderValueToText>();
            if (slider != null)
            {
                slider.ifMin = minText;
                slider.ifMax = maxText;
            }
        }

        static public void PatchGeneralOptions(GameObject generalOptions)
        {
            try
            {
            //General options
            GameObject generalContent = generalOptions.FindPath("Scroll Rect", "Contents");
            //-- WEAPONS -- 
            TextMeshProUGUI generalText = generalContent.FindPath("-- Weapons --", "Text").TMP();
            generalText.text = "-- " + LanguageManager.CurrentLanguage.options.controls_weapons + " --";

            TextMeshProUGUI rememberWeaponText = generalContent.FindPath("Remember Last Used Weapon Variation", "Text").TMP();
            rememberWeaponText.text = LanguageManager.CurrentLanguage.options.general_rememberWeapon;

            TextMeshProUGUI weaponPosText = generalContent.FindPath("Weapon Position", "Text").TMP();
            weaponPosText.text = LanguageManager.CurrentLanguage.options.general_weaponPosition;

            //Have to patch directly from the Dropdown.OptionData list.
            GameObject weaponPosList = generalContent.FindPath("Weapon Position", "Dropdown(Clone)");
            TMP_Dropdown weaponPosDropdown = weaponPosList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> weaponPosListText = weaponPosDropdown.options;
            weaponPosListText[0].text = LanguageManager.CurrentLanguage.options.general_weaponPositionRight;
            weaponPosListText[1].text = LanguageManager.CurrentLanguage.options.general_weaponPositionMiddle;
            weaponPosListText[2].text = LanguageManager.CurrentLanguage.options.general_weaponPositionLeft;

            //-- SCREEN -- goes here
            TextMeshProUGUI screenText = generalContent.FindPath("-- Screen --", "Text").TMP();
            screenText.text = "-- " + LanguageManager.CurrentLanguage.options.general_screen + " --";

            TextMeshProUGUI screenshakeText = generalContent.FindPath("Screenshake", "Text").TMP();
            screenshakeText.text = LanguageManager.CurrentLanguage.options.general_screenShake;

            SliderValueToText screenshakeSlider = generalContent.FindPath("Screenshake", "Slider Button(Clone)", "Slider", "Text").GetComponentInChildren<SliderValueToText>();
            screenshakeSlider.ifMin = LanguageManager.CurrentLanguage.options.general_screenShakeMinimum;
            screenshakeSlider.ifMax = LanguageManager.CurrentLanguage.options.general_screenShakeMaximum;

            TextMeshProUGUI parryFlashText = generalContent.FindPath("Parry Screen Flash", "Text").TMP();
            parryFlashText.text = LanguageManager.CurrentLanguage.options.general_parryFlash;

            TextMeshProUGUI cameraTiltText = generalContent.FindPath("Camera Tilt", "Text").TMP();
            cameraTiltText.text = LanguageManager.CurrentLanguage.options.general_cameraTilt;

            //-- MISC --
            TextMeshProUGUI miscText = generalContent.FindPath("-- Misc --", "Text").TMP();
            miscText.text = "-- " + LanguageManager.CurrentLanguage.options.general_misc + " --";
            
            TextMeshProUGUI seasonEventText = generalContent.FindPath("Seasonal Events", "Text").TMP();
            seasonEventText.text = LanguageManager.CurrentLanguage.options.general_seasonalEvent;

            TextMeshProUGUI levelLeaderboardsText = generalContent.FindPath("Level Leaderboards", "Text").TMP();
            levelLeaderboardsText.text = LanguageManager.CurrentLanguage.options.general_levelLeaderboards;

            TextMeshProUGUI restartWarningText = generalContent.ChildAt(10).Child("Text").TMP();
            restartWarningText.text = LanguageManager.CurrentLanguage.options.general_restartWarning;

            GameObject restartWarningList = generalContent.ChildAt(10).Child("Dropdown(Clone)");
            TMP_Dropdown restartWarningDropdown = restartWarningList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> restartWarningListText = restartWarningDropdown.options;
            restartWarningListText[0].text = LanguageManager.CurrentLanguage.options.general_restartWarningAlwaysOn;
            restartWarningListText[1].text = LanguageManager.CurrentLanguage.options.general_restartWarningOnlyCG;
            restartWarningListText[2].text = LanguageManager.CurrentLanguage.options.general_restartWarningAlwaysOff;

            TextMeshProUGUI sandboxOverwriteText = generalContent.FindPath("Sandbox Save Overwrite Warning", "Text").TMP();
            sandboxOverwriteText.text = LanguageManager.CurrentLanguage.options.general_sandboxOverwrite;

            TextMeshProUGUI discordText = generalContent.FindPath("Discord Integration", "Text").TMP();
            discordText.text = LanguageManager.CurrentLanguage.options.general_discordRpc;

            TextMeshProUGUI advancedOptionsText = generalContent.FindPath("Advanced Options", "Text").TMP();
            advancedOptionsText.text = LanguageManager.CurrentLanguage.options.general_advancedOptions;

            TextMeshProUGUI advancedOptionsCustomizeText = generalContent.FindPath("Advanced Options", "Action Button(Clone)", "Text").TMP();
            advancedOptionsCustomizeText.text = LanguageManager.CurrentLanguage.options.general_advancedOptionsCustomize;
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to patch general options.");
                Logging.Warn(e.ToString());
            }
        }
        static public void PatchControlOptions(GameObject optionsMenu)
        {
            try
            {
            //Control options
            GameObject controlContent = optionsMenu.FindPath("Scroll Rect", "Contents");

            //-- GENERAL --
            TextMeshProUGUI controlText = controlContent.FindPath("-- General --", "Text").TMP();
            controlText.text = "-- " + LanguageManager.CurrentLanguage.options.category_general + " --";

            TextMeshProUGUI mouseSensitivityText = controlContent.FindPath("Look Sensitivity", "Text").TMP();
            mouseSensitivityText.text = LanguageManager.CurrentLanguage.options.controls_mouseSensitivity;

            TextMeshProUGUI invertXAxisText = controlContent.FindPath("Invert X Axis", "Text").TMP();
            invertXAxisText.text = LanguageManager.CurrentLanguage.options.controls_xInversion;

            TextMeshProUGUI invertYAxisText = controlContent.FindPath("Invert Y Axis", "Text").TMP();
            invertYAxisText.text = LanguageManager.CurrentLanguage.options.controls_yInversion;

            TextMeshProUGUI controllerRumbleText = controlContent.FindPath("Controller Rumble", "Text").TMP();
            controllerRumbleText.text = LanguageManager.CurrentLanguage.options.controls_controllerRumble;

            TextMeshProUGUI controllerRumbleTextCustomize = controlContent.FindPath("Controller Rumble", "Action Button(Clone)", "Text").TMP();
            controllerRumbleTextCustomize.text = LanguageManager.CurrentLanguage.options.controls_controllerRumbleCustomize;

            TextMeshProUGUI weaponsTitle = controlContent.ChildAt(5).Child("Text").TMP();
            weaponsTitle.text = "-- " + LanguageManager.CurrentLanguage.options.controls_weapons + " --";

            GameObject mouseWheelContent = controlContent.Child("Scroll Weapons with Mouse Wheel");
            TextMeshProUGUI changeWeaponMouseWheel = mouseWheelContent.Child("Text").TMP();
            changeWeaponMouseWheel.text = LanguageManager.CurrentLanguage.options.controls_mouseWheelToChangeWeapon;

            TextMeshProUGUI weaponScrollType = controlContent.FindPath("Weapon Scroll Type", "Text").TMP();
            weaponScrollType.text = LanguageManager.CurrentLanguage.options.controls_scrollType;

            //Dropdown here
            GameObject scrollTypeList = (controlContent.FindPath("Weapon Scroll Type", "Dropdown(Clone)"));

            TMP_Dropdown scrollTypeDropdown = scrollTypeList.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> scrollTypeDropdownText = scrollTypeDropdown.options;
            scrollTypeDropdownText[0].text = LanguageManager.CurrentLanguage.options.controls_scrollTypeWeapons;
            scrollTypeDropdownText[1].text = LanguageManager.CurrentLanguage.options.controls_scrollTypeVariations;
            scrollTypeDropdownText[2].text = LanguageManager.CurrentLanguage.options.controls_scrollTypeAll;

            TextMeshProUGUI reverseScrollDirection = controlContent.FindPath("Reverse Scroll Direction", "Text").TMP();
            reverseScrollDirection.text = LanguageManager.CurrentLanguage.options.controls_reverseScroll;

            GameObject redrawBehaviour = controlContent.Child("On Swap To Already Drawn Weapon");
            TextMeshProUGUI redrawBehaviourTitle = redrawBehaviour.Child("Text").TMP();
            redrawBehaviourTitle.text = LanguageManager.CurrentLanguage.options.controls_redrawBehaviour;

            TMP_Dropdown redrawBehaviourDropdown = redrawBehaviour.Child("Dropdown(Clone)").GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> redrawBehaviourDropdownText = redrawBehaviourDropdown.options;
            redrawBehaviourDropdownText[0].text = LanguageManager.CurrentLanguage.options.controls_redrawNext;
            redrawBehaviourDropdownText[1].text = LanguageManager.CurrentLanguage.options.controls_redrawFirst;
            redrawBehaviourDropdownText[2].text = LanguageManager.CurrentLanguage.options.controls_redrawSame;

            TextMeshProUGUI invertRocketControls = controlContent.FindPath("Invert Rocket Controls", "Text").TMP();
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
            catch (Exception e)
            {
                Logging.Warn("Failed to patch control options.");
                Logging.Warn(e.ToString());
            }
        }
        static public void PatchGraphicsOptions(GameObject optionsMenu)
        {
            try
            {
                GameObject graphicsContent = optionsMenu.FindPath("Scroll Rect", "Contents");
                if (graphicsContent == null) return;

                SetText(graphicsContent.FindPath("-- General --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.category_general + " --");
                SetText(graphicsContent.FindPath("Resolution", "Text"), LanguageManager.CurrentLanguage.options.graphics_resolution);
                SetText(graphicsContent.FindPath("Fullscreen", "Text"), LanguageManager.CurrentLanguage.options.graphics_fullscreen);
                SetText(graphicsContent.FindPath("Target Framerate", "Text"), LanguageManager.CurrentLanguage.options.graphics_maxFps);

                TMP_Dropdown fpsDropdown = GetDropdown(graphicsContent.FindPath("Target Framerate", "Dropdown(Clone)"));
                if (fpsDropdown != null)
                {
                    SetDropdownText(fpsDropdown, 0, LanguageManager.CurrentLanguage.options.graphics_maxFpsNone);
                    SetDropdownText(fpsDropdown, 1, LanguageManager.CurrentLanguage.options.graphics_maxFps2x);
                }

                SetText(graphicsContent.FindPath("VSync", "Text"), LanguageManager.CurrentLanguage.options.graphics_vsync);
                SetText(graphicsContent.FindPath("Field of View", "Text"), LanguageManager.CurrentLanguage.options.graphics_fieldOfVision);
                SetText(graphicsContent.FindPath("Gamma (Brightness)", "Text"), LanguageManager.CurrentLanguage.options.graphics_gamma);
                SetText(graphicsContent.FindPath("Use Fallback Shaders (Requires Reload)", "Text"), LanguageManager.CurrentLanguage.options.graphics_useFallbackShaders);

                SetText(graphicsContent.FindPath("-- PSX --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.graphics_filters + " --\n<size=16>"
                                            + LanguageManager.CurrentLanguage.options.graphics_filtersDescription + "</size>");

                SetText(graphicsContent.FindPath("Downscaling", "Text"), LanguageManager.CurrentLanguage.options.graphics_pixelisation);

                TMP_Dropdown resolutionDropdown = GetDropdown(graphicsContent.FindPath("Downscaling", "Dropdown(Clone)"));
                if (resolutionDropdown != null)
                {
                    SetDropdownText(resolutionDropdown, 0, LanguageManager.CurrentLanguage.options.graphics_pixelisationNone);
                    SetDropdownText(resolutionDropdown, 1, LanguageManager.CurrentLanguage.options.graphics_pixelisation720p);
                    SetDropdownText(resolutionDropdown, 2, LanguageManager.CurrentLanguage.options.graphics_pixelisation480p);
                    SetDropdownText(resolutionDropdown, 3, LanguageManager.CurrentLanguage.options.graphics_pixelisation360p);
                    SetDropdownText(resolutionDropdown, 4, LanguageManager.CurrentLanguage.options.graphics_pixelisation240p);
                    SetDropdownText(resolutionDropdown, 5, LanguageManager.CurrentLanguage.options.graphics_pixelisation144p);
                    SetDropdownText(resolutionDropdown, 6, LanguageManager.CurrentLanguage.options.graphics_pixelisation36p);
                }

                SetText(graphicsContent.FindPath("Dithering", "Text"), LanguageManager.CurrentLanguage.options.graphics_dithering);
                SetSliderMinText(graphicsContent.FindPath("Dithering", "Slider Button(Clone)", "Slider", "Text"), LanguageManager.CurrentLanguage.options.graphics_ditheringMinimum);

                SetText(graphicsContent.FindPath("Texture Warping", "Text"), LanguageManager.CurrentLanguage.options.graphics_textureWarping);
                SetText(graphicsContent.FindPath("Vertex Warping", "Text"), LanguageManager.CurrentLanguage.options.graphics_vertexWarping);

                TMP_Dropdown vertexWarpingDropdown = GetDropdown(graphicsContent.FindPath("Vertex Warping", "Dropdown(Clone)"));
                if (vertexWarpingDropdown != null)
                {
                    SetDropdownText(vertexWarpingDropdown, 0, LanguageManager.CurrentLanguage.options.graphics_vertexWarpingNone);
                    SetDropdownText(vertexWarpingDropdown, 1, LanguageManager.CurrentLanguage.options.graphics_vertexWarpingLight);
                    SetDropdownText(vertexWarpingDropdown, 2, LanguageManager.CurrentLanguage.options.graphics_vertexWarpingMedium);
                    SetDropdownText(vertexWarpingDropdown, 3, LanguageManager.CurrentLanguage.options.graphics_vertexWarpingStrong);
                    SetDropdownText(vertexWarpingDropdown, 4, LanguageManager.CurrentLanguage.options.graphics_vertexWarpingVeryStrong);
                    SetDropdownText(vertexWarpingDropdown, 5, LanguageManager.CurrentLanguage.options.graphics_vertexWarpingAbsurd);
                }

                SetText(graphicsContent.FindPath("Custom Color Palette", "Text"), LanguageManager.CurrentLanguage.options.graphics_customColorPalette);
                SetText(graphicsContent.FindPath("Color Palette Texture", "Text"), LanguageManager.CurrentLanguage.options.graphics_customPaletteTexture);
                SetText(graphicsContent.FindPath("Color Palette Texture", "Action Button(Clone)", "Text"), LanguageManager.CurrentLanguage.options.graphics_customColorPaletteSelect);

                SetText(graphicsContent.FindPath("Color Compression", "Text"), LanguageManager.CurrentLanguage.options.graphics_colorCompression);

                TMP_Dropdown colorCompressionDropdown = GetDropdown(graphicsContent.FindPath("Color Compression", "Dropdown(Clone)"));
                if (colorCompressionDropdown != null)
                {
                    SetDropdownText(colorCompressionDropdown, 0, LanguageManager.CurrentLanguage.options.graphics_colorCompressionNone);
                    SetDropdownText(colorCompressionDropdown, 1, LanguageManager.CurrentLanguage.options.graphics_colorCompressionLight);
                    SetDropdownText(colorCompressionDropdown, 2, LanguageManager.CurrentLanguage.options.graphics_colorCompressionMedium);
                    SetDropdownText(colorCompressionDropdown, 3, LanguageManager.CurrentLanguage.options.graphics_colorCompressionStrong);
                    SetDropdownText(colorCompressionDropdown, 4, LanguageManager.CurrentLanguage.options.graphics_colorCompressionVeryStrong);
                    SetDropdownText(colorCompressionDropdown, 5, LanguageManager.CurrentLanguage.options.graphics_colorCompressionAbsurd);
                }

                SetText(graphicsContent.FindPath("-- Performance --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.graphics_performance + " --");
                SetText(graphicsContent.FindPath("Simpler Explosions", "Text"), LanguageManager.CurrentLanguage.options.graphics_performanceSimpleExplosions);
                SetText(graphicsContent.FindPath("Simpler Fire", "Text"), LanguageManager.CurrentLanguage.options.graphics_performanceSimpleFire);
                SetText(graphicsContent.FindPath("Simpler Spawn Effects", "Text"), LanguageManager.CurrentLanguage.options.graphics_performanceSimpleSpawn);
                SetText(graphicsContent.FindPath("Disable Environmental Particle Effects", "Text"), LanguageManager.CurrentLanguage.options.graphics_performanceDisableEnviParticles);
                SetText(graphicsContent.FindPath("Disable Environmental Hit Particles", "Text"), LanguageManager.CurrentLanguage.options.graphics_performanceDisableEnviHitParticles);
                SetText(graphicsContent.FindPath("Disable Heat Waves", "Text"), LanguageManager.CurrentLanguage.options.graphics_performanceDisableHeatWaves);

                SetText(graphicsContent.FindPath("-- Gore --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.graphics_gore + " --\n<size=16>"
                    + LanguageManager.CurrentLanguage.options.graphics_goreNote + "</size>");
                SetText(graphicsContent.FindPath("Enable Blood & Gore", "Text"), LanguageManager.CurrentLanguage.options.graphics_goreEnable);
                SetText(graphicsContent.FindPath("Freeze Gore Physics", "Text"), LanguageManager.CurrentLanguage.options.graphics_goreDisablePhysics);
                SetText(graphicsContent.FindPath("Max Bloodstains", "Text"), LanguageManager.CurrentLanguage.options.graphics_goreMaxBloodStains);
                SetText(graphicsContent.FindPath("Bloodstain Chance", "Text"), LanguageManager.CurrentLanguage.options.graphics_goreBloodChance);
                SetText(graphicsContent.FindPath("Max Gore Per Room", "Text"), LanguageManager.CurrentLanguage.options.graphics_goreMaxGore);
            }
            catch (Exception e)
            {
                Logging.Error("Failed to patch graphics options.");
                Logging.Error(e.ToString());
            }
        }
        static public void PatchAudioOptions(GameObject optionsMenu)
        {
            try
            {
                GameObject audioContent = optionsMenu.Child("Container");
                if (audioContent == null) return;

                SetText(audioContent.FindPath("-- Volume --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.audio_volume + " --");
                SetText(audioContent.FindPath("Master", "Text"), LanguageManager.CurrentLanguage.options.audio_globalVolume);
                SetText(audioContent.FindPath("Sound Effects", "Text"), LanguageManager.CurrentLanguage.options.audio_soundEffectsVolume);
                SetText(audioContent.FindPath("Music", "Text"), LanguageManager.CurrentLanguage.options.audio_musicVolume);

                SetText(audioContent.FindPath("-- Misc --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.general_misc + " --");
                SetText(audioContent.FindPath("Subtitles", "Text"), LanguageManager.CurrentLanguage.options.audio_subtitles);
                SetText(audioContent.FindPath("Muffle Music While Underwater", "Text"), LanguageManager.CurrentLanguage.options.audio_muffleMusic);
            }
            catch (Exception e)
            {
                Logging.Error("Failed to patch audio options.");
                Logging.Error(e.ToString());
            }
        }
        static public void PatchAssistOptions(GameObject optionsMenu)
        {
            try
            {
                GameObject assistMajorAssistPanel = optionsMenu.FindPath("Major Assists Consent", "Panel");
                if (assistMajorAssistPanel != null)
                {
                    TextMeshProUGUI assistDisclaimerText = assistMajorAssistPanel.Child("Description Block").TMP();
                    if (assistDisclaimerText != null)
                    {
                        assistDisclaimerText.text =
                            LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimer1
                            + "\n\n"
                            + LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimer2
                            + "\n\n"
                            + LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimer3;
                        assistDisclaimerText.fontSize = 20;
                    }

                    TextMeshProUGUI assistDisclaimerConfirmText = assistMajorAssistPanel.Child("Summary").TMP();
                    if (assistDisclaimerConfirmText != null)
                    {
                        assistDisclaimerConfirmText.text = LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimerConfirm;
                        assistDisclaimerConfirmText.fontSize = 24;
                    }

                    SetText(assistMajorAssistPanel.FindPath("Yes", "Text"), LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimerConfirmYes);
                    SetText(assistMajorAssistPanel.FindPath("No", "Text"), LanguageManager.CurrentLanguage.options.assists_majorAssistsDisclaimerConfirmNo);
                }

                GameObject assistContent = optionsMenu.FindPath("Scroll Rect", "Contents");
                if (assistContent == null) return;

                SetText(assistContent.FindPath("-- Minor Assists --", "Text"), "-- " + LanguageManager.CurrentLanguage.options.assists_minor + " --");
                SetText(assistContent.FindPath("Auto Aim", "Text"), LanguageManager.CurrentLanguage.options.assists_autoAim);
                SetText(assistContent.FindPath("Auto Aim Amount", "Text"), LanguageManager.CurrentLanguage.options.assists_autoAimPercent);

                SetSliderMinMaxText(assistContent.FindPath("Auto Aim Amount", "Slider Button(Clone)", "Slider", "Text"), "0", "100");

                GameObject assistEnemySilhouettes = assistContent.Child("Enemy Silhouettes");
                if (assistEnemySilhouettes != null)
                {
                    SetText(assistEnemySilhouettes.Child("Text"), LanguageManager.CurrentLanguage.options.assists_enemySilhouettes);

                    TMP_Dropdown silhouetteDropdown = GetDropdown(assistEnemySilhouettes.Child("Dropdown(Clone)"));
                    if (silhouetteDropdown != null)
                    {
                        SetDropdownText(silhouetteDropdown, 0, LanguageManager.CurrentLanguage.options.assists_enemySilhouettesNone);
                        SetDropdownText(silhouetteDropdown, 1, LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlinesOnly);
                        SetDropdownText(silhouetteDropdown, 2, LanguageManager.CurrentLanguage.options.assists_enemySilhouettesFull);
                    }
                }

                SetText(assistContent.FindPath("Enemy Silhouettes", "Text"), LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlines);
                SetText(assistContent.FindPath("Activation Distance", "Text"), LanguageManager.CurrentLanguage.options.assists_enemySilhouettesDistance);
                SetText(assistContent.FindPath("Outline Thickness", "Text"), LanguageManager.CurrentLanguage.options.assists_enemySilhouettesOutlineThickness);
                SetSliderMinText(assistContent.FindPath("Activation Distance", "Slider Button(Clone)", "Slider", "Text"), LanguageManager.CurrentLanguage.options.assists_enemySilhouettesDistanceMinimum);

                GameObject assistsMajorTitleObject = assistContent.Child("-- Major Assists --");
                if (assistsMajorTitleObject != null)
                {
                    TextMeshProUGUI assistsMajorTitle = assistsMajorTitleObject.Child("Text").TMP();
                    if (assistsMajorTitle != null)
                    {
                        assistsMajorTitle.text = "-- " + LanguageManager.CurrentLanguage.options.assists_major + " --";
                        assistsMajorTitle.fontSize = 24;
                    }
                    SetText(assistsMajorTitleObject.FindPath("Enable Group", "Text"), LanguageManager.CurrentLanguage.options.assists_majorActivate);
                }

                SetText(assistContent.FindPath("Game Speed", "Text"), LanguageManager.CurrentLanguage.options.assists_gameSpeed);
                SetText(assistContent.FindPath("Damage Taken", "Text"), LanguageManager.CurrentLanguage.options.assists_damageTaken);

                GameObject bossOverride = assistContent.Child("Boss Fight Difficulty Override");
                if (bossOverride != null)
                {
                    SetText(assistContent.FindPath("Boss Fight Difficulty Override", "Text"), LanguageManager.CurrentLanguage.options.assists_bossOverride);
                    SetText(bossOverride.Child("Side Note"), LanguageManager.CurrentLanguage.options.assists_bossRestartRequired);

                    TMP_Dropdown bossOverrideDropdown = GetDropdown(bossOverride.Child("Dropdown(Clone)"));
                    if (bossOverrideDropdown != null)
                    {
                        SetDropdownText(bossOverrideDropdown, 0, LanguageManager.CurrentLanguage.options.assists_bossOverrideNone);
                        SetDropdownText(bossOverrideDropdown, 1, LanguageManager.CurrentLanguage.frontend.difficulty_harmless);
                        SetDropdownText(bossOverrideDropdown, 2, LanguageManager.CurrentLanguage.frontend.difficulty_lenient);
                        SetDropdownText(bossOverrideDropdown, 3, LanguageManager.CurrentLanguage.frontend.difficulty_standard);
                        SetDropdownText(bossOverrideDropdown, 4, LanguageManager.CurrentLanguage.frontend.difficulty_violent);
                        SetDropdownText(bossOverrideDropdown, 5, LanguageManager.CurrentLanguage.frontend.difficulty_brutal);
                    }
                }

                SetText(assistContent.FindPath("Infinite Stamina", "Text"), LanguageManager.CurrentLanguage.options.assists_infiniteEnergy);
                SetText(assistContent.FindPath("Disable Whiplash Hard Damage", "Text"), LanguageManager.CurrentLanguage.options.assists_disableWhiplashHardDamage);
                SetText(assistContent.FindPath("Disable All Hard Damage", "Text"), LanguageManager.CurrentLanguage.options.assists_disableHardDamage);
                SetText(assistContent.FindPath("Disable Weapon Freshness", "Text"), LanguageManager.CurrentLanguage.options.assists_disableWeaponFreshness);
                SetText(assistContent.FindPath("Disable Assist Popup", "Text"), LanguageManager.CurrentLanguage.options.assists_disablePopupHints);
            }
            catch (Exception e)
            {
                Logging.Error("Failed to patch assist options.");
                Logging.Error(e.ToString());
            }
        }
        static public void PatchSavesOptions(GameObject optionMenu)
        {
            try
            {
            //Save options
            GameObject saveReloadPanel = optionMenu.FindPath("Reload Consent Blocker", "Consent", "Panel");
            
            TextMeshProUGUI saveReloadText = saveReloadPanel.Child("Text").TMP();
            TextMeshProUGUI saveReloadYes = saveReloadPanel.FindPath("Yes", "Text").TMP();
            TextMeshProUGUI saveReloadNo = saveReloadPanel.FindPath("No", "Text").TMP();
            
            saveReloadText.text =
                "<color=red>" + LanguageManager.CurrentLanguage.options.save_warning1 + "</color>\n\n" +
                LanguageManager.CurrentLanguage.options.save_warning2;

            saveReloadYes.text = LanguageManager.CurrentLanguage.options.save_reloadYes;
            saveReloadNo.text = LanguageManager.CurrentLanguage.options.save_reloadNo;
            
            GameObject saveDeletePanel = optionMenu.FindPath("Wipe Consent Blocker", "Consent", "Panel");
            
            TextMeshProUGUI saveDeleteYes = saveDeletePanel.FindPath("Yes", "Text").TMP();
            saveDeleteYes.text = "<color=red>" + LanguageManager.CurrentLanguage.options.save_deleteYes + "</color>";

            TextMeshProUGUI saveDeleteNo = saveDeletePanel.FindPath("No", "Text").TMP();
            saveDeleteNo.text = LanguageManager.CurrentLanguage.options.save_deleteNo;

            TextMeshProUGUI saveSlotsClose = optionMenu.FindPath("Close", "Text").TMP();
            saveSlotsClose.text = LanguageManager.CurrentLanguage.options.save_close;
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to patch saves options.");
                Logging.Warn(e.ToString());
            }
        }
        //general end
        //customization starts here
        static public void PatchHUDOptions(GameObject optionsMenu)
        {
            try
            {
            //HUD options
            GameObject hudContent = optionsMenu.FindPath("Scroll Rect", "Contents");

            TextMeshProUGUI hudTitle = hudContent.ChildAt(0).Child("Text").TMP();
            hudTitle.text = "-- " + LanguageManager.CurrentLanguage.options.category_general + " --";

            TextMeshProUGUI hudTypeText = hudContent.FindPath("HUD Type", "Text").TMP();
            hudTypeText.text = LanguageManager.CurrentLanguage.options.hud_type;

            GameObject hudType = hudContent.FindPath("HUD Type", "Dropdown(Clone)");
            TMP_Dropdown hudTypeDropdown = hudType.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> hudTypeDropdownListText = hudTypeDropdown.options;

            hudTypeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.hud_typeNone;
            hudTypeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.hud_typeStandard;
            hudTypeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.hud_typeClassicColor;
            hudTypeDropdownListText[3].text = LanguageManager.CurrentLanguage.options.hud_typeClassicWhite;

            TextMeshProUGUI backgroundOpacityText = hudContent.FindPath("Background Opacity", "Text").TMP();
            backgroundOpacityText.text = LanguageManager.CurrentLanguage.options.hud_backgroundOpacity;

            SliderValueToText backgroundOpacitySlider = hudContent.FindPath("Background Opacity", "Slider Button(Clone)", "Slider").GetComponentInChildren<SliderValueToText>();

            backgroundOpacitySlider.ifMin = LanguageManager.CurrentLanguage.options.hud_backgroundOpacityMinimum;
            backgroundOpacitySlider.ifMax = LanguageManager.CurrentLanguage.options.hud_backgroundOpacityMaximum;

            TextMeshProUGUI alwaysOnTopText = hudContent.FindPath("Always On Top", "Text").TMP();
            alwaysOnTopText.text = LanguageManager.CurrentLanguage.options.hud_alwaysOnTop;

            GameObject iconsObject = hudContent.Child("Cheat & Sandbox Icons");
            TextMeshProUGUI iconsText = iconsObject.Child("Text").TMP();
            iconsText.text = LanguageManager.CurrentLanguage.options.hud_icons;

            TextMeshProUGUI reduceHudMotion = hudContent.FindPath("REDUCE HUD MOTION", "Text").TMP();
            reduceHudMotion.text = LanguageManager.CurrentLanguage.options.hud_reduceHudMotion;

            TMP_Dropdown iconsDropdown = iconsObject.GetComponentInChildren<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> iconsDropdownListText = iconsDropdown.options;

            iconsDropdownListText[0].text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_default.ToUpper();
            iconsDropdownListText[1].text = LanguageManager.CurrentLanguage.sandbox.sandbox_shop_pitr;

            TextMeshProUGUI hudElements = hudContent.FindPath("-- Elements --", "Text").TMP();
            hudElements.text = "-- " + LanguageManager.CurrentLanguage.options.hud_hudElements + " --";

            TextMeshProUGUI weaponIconText = hudContent.FindPath("Weapon Icon", "Text").TMP();
            weaponIconText.text = LanguageManager.CurrentLanguage.options.hud_weaponIcon;

            TextMeshProUGUI armIconText = hudContent.FindPath("Arm Icon", "Text").TMP();
            armIconText.text = LanguageManager.CurrentLanguage.options.hud_armIcon;

            TextMeshProUGUI railcannonMeterText = hudContent.FindPath("Railcannon Meter", "Text").TMP();
            railcannonMeterText.text = LanguageManager.CurrentLanguage.options.hud_railcannonMeter;

            TextMeshProUGUI styleMeterText = hudContent.FindPath("Style Meter", "Text").TMP();
            styleMeterText.text = LanguageManager.CurrentLanguage.options.hud_styleMeter;

            TextMeshProUGUI styleInfoText = hudContent.FindPath("Style Info", "Text").TMP();
            styleInfoText.text = LanguageManager.CurrentLanguage.options.hud_styleInfo;

            GameObject speedoMeterDD = hudContent.Child("Speedometer");
            TextMeshProUGUI speedoMeterText = speedoMeterDD.Child("Text").TMP();
            speedoMeterText.text = LanguageManager.CurrentLanguage.options.hud_speedoMeterText;

            TMP_Dropdown speedoMeterTypeDropdown = speedoMeterDD.GetComponentInChildren<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> speedoMeterTypeDropdownListText = speedoMeterTypeDropdown.options;
            speedoMeterTypeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeOff;
            speedoMeterTypeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeOn;
            speedoMeterTypeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeHorizonal;
            speedoMeterTypeDropdownListText[3].text = LanguageManager.CurrentLanguage.options.hud_speedoMeterTypeVertical;
            
            //Crosshair settings

            TextMeshProUGUI crosshairTitle = hudContent.FindPath("-- Crosshair --", "Text").TMP();
            crosshairTitle.text = "-- " + LanguageManager.CurrentLanguage.options.crosshair_title + " --";

            TextMeshProUGUI crosshairTypeText = hudContent.FindPath("Type", "Text").TMP();
            crosshairTypeText.text = LanguageManager.CurrentLanguage.options.crosshair_type;

            GameObject crosshairType = hudContent.FindPath("Type", "Dropdown(Clone)");
            TMP_Dropdown crosshairTypeDropdown = crosshairType.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> crosshairTypeDropdownListText = crosshairTypeDropdown.options;

            crosshairTypeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.crosshair_typeNone;
            crosshairTypeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.crosshair_typeSmall;
            crosshairTypeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.crosshair_typeLarge;

            TextMeshProUGUI crosshairColorText = hudContent.FindPath("Color", "Text").TMP();
            crosshairColorText.text = LanguageManager.CurrentLanguage.options.crosshair_color;

            GameObject crosshairColor = hudContent.FindPath("Color", "Dropdown(Clone)");
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

            TextMeshProUGUI crosshairHudSizeText = hudContent.FindPath("Crosshair HUD Size", "Text").TMP();
            crosshairHudSizeText.text = LanguageManager.CurrentLanguage.options.crosshair_size;

            GameObject crosshairSize = hudContent.FindPath("Crosshair HUD Size", "Dropdown(Clone)");
            TMP_Dropdown crosshairSizeDropdown = crosshairSize.GetComponent<TMP_Dropdown>();
            List<TMP_Dropdown.OptionData> crosshairSizeDropdownListText = crosshairSizeDropdown.options;

            crosshairSizeDropdownListText[0].text = LanguageManager.CurrentLanguage.options.crosshair_sizeNone;
            crosshairSizeDropdownListText[1].text = LanguageManager.CurrentLanguage.options.crosshair_sizeThin;
            crosshairSizeDropdownListText[2].text = LanguageManager.CurrentLanguage.options.crosshair_sizeMedium;
            crosshairSizeDropdownListText[3].text = LanguageManager.CurrentLanguage.options.crosshair_sizeThick;
            crosshairSizeDropdownListText[4].text = LanguageManager.CurrentLanguage.options.crosshair_sizeVeryThick;

            TextMeshProUGUI crosshairHudFadeText = hudContent.FindPath("Crosshair HUD Fade", "Text").TMP();
            crosshairHudFadeText.text = LanguageManager.CurrentLanguage.options.crosshair_hudFade;

            TextMeshProUGUI crosshairPowerupText = hudContent.FindPath("Powerup Meter", "Text").TMP();
            crosshairPowerupText.text = LanguageManager.CurrentLanguage.options.crosshair_powerupBar;
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to patch HUD options.");
                Logging.Warn(e.ToString());
            }
        }
        
        private void PatchColorsOptions(GameObject optionsMenu)
        {
            //Colors options
            //TextMeshProUGUI colorsPanel = GetTextMeshProUGUI(GetGameObjectChild(optionsMenu, "Text (1)"));
            //colorsPanel.text = "--" + LanguageManager.CurrentLanguage.options.colors_title + "--";

            TextMeshProUGUI colorsResetDefaultText = optionsMenu.FindPath("Scroll Rect", "Contents", "Default", "Text").TMP();
            colorsResetDefaultText.text = LanguageManager.CurrentLanguage.options.colors_reset.ToUpper();

            //HUD Text
            GameObject colorsHudObject = optionsMenu.FindPath("Scroll Rect", "Contents", "HUD");

            TextMeshProUGUI colorsHudText = colorsHudObject.TMP();
            colorsHudText.text = "-- " + LanguageManager.CurrentLanguage.options.colors_hud.ToUpper() + " --";

            TextMeshProUGUI colorsHudHealthText = colorsHudObject.FindPath("Health", "Text").TMP();
            colorsHudHealthText.text = LanguageManager.CurrentLanguage.options.colors_hudHealth.ToUpper();

            TextMeshProUGUI colorsHudHealthNumberText = colorsHudObject.FindPath("HpText", "Text").TMP();
            colorsHudHealthNumberText.text = LanguageManager.CurrentLanguage.options.colors_hudHealthNumber.ToUpper();

            TextMeshProUGUI colorsHudSoftDamageText = colorsHudObject.FindPath("AfterImage", "Text").TMP();
            colorsHudSoftDamageText.text = LanguageManager.CurrentLanguage.options.colors_hudDamage.ToUpper();

            TextMeshProUGUI colorsHudHardDamageText = colorsHudObject.FindPath("AntiHp", "Text").TMP();
            colorsHudHardDamageText.text = LanguageManager.CurrentLanguage.options.colors_hudHardDamage.ToUpper();

            TextMeshProUGUI colorsHudOverhealText = colorsHudObject.FindPath("Overheal", "Text").TMP();
            colorsHudOverhealText.text = LanguageManager.CurrentLanguage.options.colors_hudOverheal.ToUpper();

            TextMeshProUGUI colorsHudStaminaText = colorsHudObject.FindPath("Stamina", "Text").TMP();
            colorsHudStaminaText.text = LanguageManager.CurrentLanguage.options.colors_hudEnergyFull.ToUpper();

            TextMeshProUGUI colorsHudStaminaChargingText = colorsHudObject.FindPath("StaminaCharging", "Text").TMP();
            colorsHudStaminaChargingText.text = LanguageManager.CurrentLanguage.options.colors_hudEnergyPartial.ToUpper();

            TextMeshProUGUI colorsHudStaminaEmptyText = colorsHudObject.FindPath("StaminaEmpty", "Text").TMP();
            colorsHudStaminaEmptyText.text = LanguageManager.CurrentLanguage.options.colors_hudEnergyEmpty.ToUpper();

            TextMeshProUGUI colorsHudRailcannonFullText = colorsHudObject.FindPath("RailcannonFull", "Text").TMP();
            colorsHudRailcannonFullText.text = LanguageManager.CurrentLanguage.options.colors_railcannonFull.ToUpper();

            TextMeshProUGUI colorsHudRailcannonChargingText = colorsHudObject.FindPath("RailcannonCharging", "Text").TMP();
            colorsHudRailcannonChargingText.text = LanguageManager.CurrentLanguage.options.colors_railcannonPartial.ToUpper();

            TextMeshProUGUI colorsHudVarBlueText = colorsHudObject.FindPath("Blue Variation", "Text").TMP();
            colorsHudVarBlueText.text = LanguageManager.CurrentLanguage.options.colors_variationBlue.ToUpper();

            TextMeshProUGUI colorsHudVarGreenText = colorsHudObject.FindPath("Green Variation", "Text").TMP();
            colorsHudVarGreenText.text = LanguageManager.CurrentLanguage.options.colors_variationGreen.ToUpper();

            TextMeshProUGUI colorsHudVarRedText = colorsHudObject.FindPath("Red Variation", "Text").TMP();
            colorsHudVarRedText.text = LanguageManager.CurrentLanguage.options.colors_variationRed.ToUpper();

            TextMeshProUGUI colorsHudVarGoldText = colorsHudObject.FindPath("Gold Variation", "Text").TMP();
            colorsHudVarGoldText.text = LanguageManager.CurrentLanguage.options.colors_variationGold.ToUpper();

            //Enemy names text
            //Later down the line, could be better to get the names from EnemyBios.
            GameObject colorsEnemiesObject = optionsMenu.FindPath("Scroll Rect", "Contents", "Enemies");

            TextMeshProUGUI colorsEnemiesText = colorsEnemiesObject.TMP();
            colorsEnemiesText.text = "-- " + LanguageManager.CurrentLanguage.options.colors_enemies.ToUpper() + " --";

            TextMeshProUGUI colorsEnemiesFilthText = colorsEnemiesObject.FindPath("Filth", "Text").TMP();
            colorsEnemiesFilthText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_filth.ToUpper();

            TextMeshProUGUI colorsEnemiesStrayText = colorsEnemiesObject.FindPath("Stray", "Text").TMP();
            colorsEnemiesStrayText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_stray.ToUpper();

            TextMeshProUGUI colorsEnemiesMalFaceText = colorsEnemiesObject.FindPath("Malicious Face", "Text").TMP();
            colorsEnemiesMalFaceText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_malFace.ToUpper();

            TextMeshProUGUI colorsEnemiesSchismText = colorsEnemiesObject.FindPath("Schism", "Text").TMP();
            colorsEnemiesSchismText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_schism.ToUpper();

            TextMeshProUGUI colorsEnemiesSwordsmachineText = colorsEnemiesObject.FindPath("Swordsmachine", "Text").TMP();
            colorsEnemiesSwordsmachineText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_swordsmachine.ToUpper();

            TextMeshProUGUI colorsEnemiesCerberusText = colorsEnemiesObject.FindPath("Cerberus", "Text").TMP();
            colorsEnemiesCerberusText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_cerberus.ToUpper();

            TextMeshProUGUI colorsEnemiesDroneText = colorsEnemiesObject.FindPath("Drone", "Text").TMP();
            colorsEnemiesDroneText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_drone.ToUpper();

            TextMeshProUGUI colorsEnemiesStreetcleanerText = colorsEnemiesObject.FindPath("Streetcleaner", "Text").TMP();
            colorsEnemiesStreetcleanerText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_streetCleaner.ToUpper();

            TextMeshProUGUI colorsEnemiesSoldierText = colorsEnemiesObject.FindPath("Shotgunner", "Text").TMP();
            colorsEnemiesSoldierText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_soldier.ToUpper();

            TextMeshProUGUI colorsEnemiesV2Text = colorsEnemiesObject.FindPath("V2", "Text").TMP();
            colorsEnemiesV2Text.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_v2.ToUpper();

            TextMeshProUGUI colorsEnemiesMindflayerText = colorsEnemiesObject.FindPath("Mindflayer", "Text").TMP();
            colorsEnemiesMindflayerText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_mindFlayer.ToUpper();

            TextMeshProUGUI colorsEnemiesVirtueText = colorsEnemiesObject.FindPath("Virtue", "Text").TMP();
            colorsEnemiesVirtueText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_virtue.ToUpper();

            TextMeshProUGUI colorsEnemiesStalkerText = colorsEnemiesObject.FindPath("Stalker", "Text").TMP();
            colorsEnemiesStalkerText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_stalker.ToUpper();

            TextMeshProUGUI colorsEnemiesSisyphusText = colorsEnemiesObject.FindPath("Sisyphus", "Text").TMP();
            colorsEnemiesSisyphusText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_insurrectionist.ToUpper();

            TextMeshProUGUI colorsEnemiesSentryText = colorsEnemiesObject.FindPath("Sentry", "Text").TMP();
            colorsEnemiesSentryText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_sentry.ToUpper();

            TextMeshProUGUI colorsEnemiesIdolText = colorsEnemiesObject.FindPath("Idol", "Text").TMP();
            colorsEnemiesIdolText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_idol.ToUpper();

            TextMeshProUGUI colorsEnemiesFerrymanText = colorsEnemiesObject.FindPath("Ferryman", "Text").TMP();
            colorsEnemiesFerrymanText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_ferryman.ToUpper();

            TextMeshProUGUI colorsEnemiesMannequinText = colorsEnemiesObject.FindPath("Mannequin", "Text").TMP();
            colorsEnemiesMannequinText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_mannequin.ToUpper();

            TextMeshProUGUI colorsEnemiesGuttermanText = colorsEnemiesObject.FindPath("Gutterman", "Text").TMP();
            colorsEnemiesGuttermanText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_gutterman.ToUpper();

            TextMeshProUGUI colorsEnemiesGuttertankText = colorsEnemiesObject.FindPath("Guttertank", "Text").TMP();
            colorsEnemiesGuttertankText.text = LanguageManager.CurrentLanguage.enemyNames.enemyname_guttertank.ToUpper();

        }
        

        //Does not work for some reason, nothing gets translated
        private void PatchRumbleOptions(GameObject optionMenu)
        {
            TextMeshProUGUI rumbleSettingsTitle = optionMenu.Child("Text (1)").TMP();
            rumbleSettingsTitle.text = LanguageManager.CurrentLanguage.options.rumble_title;

            TextMeshProUGUI rumbleFinalMultiplier = optionMenu.FindPath("Total", "Text").TMP();
            rumbleFinalMultiplier.text = LanguageManager.CurrentLanguage.options.rumble_finalMultiplier;

            TextMeshProUGUI rumbleCloseButton = optionMenu.FindPath("Close", "Text").TMP();
            rumbleCloseButton.text = LanguageManager.CurrentLanguage.options.save_close;

            //Loop through each entry
            GameObject rumbleEntryList = optionMenu.FindPath("Scroll View", "Viewport", "Content");
            try
            {
                for (int x = 0; x < 21; x++) //Hardcoded, amount may increase in future updates
                {
                    GameObject entry = rumbleEntryList.ChildAt(x);
                    //Throws an out of bounds error, but still swaps the text correctly...
                    TextMeshProUGUI entryIntensity = entry.FindPath("Button", "Text (1)").TMP();
                    entryIntensity.text = LanguageManager.CurrentLanguage.options.rumble_intensity;

                    TextMeshProUGUI entryResetIntensity = entry.FindPath("Default Button (1)", "Text").TMP();
                    entryResetIntensity.text = LanguageManager.CurrentLanguage.options.rumble_reset;

                    TextMeshProUGUI entryEndDelay = entry.FindPath("End Delay Container", "Text (2)").TMP();
                    entryEndDelay.text = LanguageManager.CurrentLanguage.options.rumble_endDelay;

                    TextMeshProUGUI entryResetEndDelay = entry.FindPath("End Delay Container", "Default Button", "Text").TMP();
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

            TextMeshProUGUI advancedOptionsTitle = advancedOptions.Child("Title").TMP();
            advancedOptionsTitle.text = "--" + LanguageManager.CurrentLanguage.options.advanced_title + "--";

            TextMeshProUGUI advancedOptionsClose = advancedOptions.FindPath("Close", "Text").TMP();
            advancedOptionsClose.text = LanguageManager.CurrentLanguage.options.steamLeaderboard_returnButton;

            //Cybergrind Reset Confirm
            GameObject cybergrindResetPanel = advancedOptions.FindPath("Reset Cyber Grind Dialog", "Panel");

            TextMeshProUGUI cybergrindResetText1 = cybergrindResetPanel.Child("Text (2)").TMP();
            TextMeshProUGUI cybergrindResetText2 = cybergrindResetPanel.Child("Text (1)").TMP();
            cybergrindResetText1.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetText1;
            cybergrindResetText2.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetText2;

            TextMeshProUGUI cybergrindResetCancel = cybergrindResetPanel.FindPath("Cancel", "Text").TMP();
            TextMeshProUGUI cybergrindResetConfirm = cybergrindResetPanel.FindPath("Confirm", "Text").TMP();
            cybergrindResetCancel.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetCancel;
            cybergrindResetConfirm.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetConfirm;

            //The Actual Options
            GameObject advancedOptionsSub = advancedOptions.FindPath("Scroll View", "Viewport", "Content");

            TextMeshProUGUI advancedCybergrindTitle = advancedOptionsSub.Child("Cyber Grind Category").TMP();
            advancedCybergrindTitle.text = LanguageManager.CurrentLanguage.levelNames.levelName_cybergrind;

            TextMeshProUGUI advancedCybergrindReset = advancedOptionsSub.FindPath("Cyber Grind Options", "Local High Scores", "Text").TMP();
            TextMeshProUGUI advancedCybergrindResetButton = advancedOptionsSub.FindPath("Cyber Grind Options", "Local High Scores", "Reset", "Text").TMP();
            advancedCybergrindReset.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindLocalHighScore;
            advancedCybergrindResetButton.text = LanguageManager.CurrentLanguage.options.advanced_cybergrindResetButton;

            TextMeshProUGUI advancedSteamTitle = advancedOptionsSub.Child("Steam Category").TMP();
            advancedSteamTitle.text = LanguageManager.CurrentLanguage.options.advanced_steam;

            TextMeshProUGUI advancedSteamLeaderboardManage = advancedOptionsSub.FindPath("Leaderboards", "Leaderboards", "Text").TMP();
            TextMeshProUGUI advancedSteamLeaderboardManageButton = advancedOptionsSub.FindPath("Leaderboards", "Leaderboards", "Manage Button", "Text").TMP();
            advancedSteamLeaderboardManage.text = LanguageManager.CurrentLanguage.options.advanced_steamLeaderboardManage;
            advancedSteamLeaderboardManageButton.text = LanguageManager.CurrentLanguage.options.advanced_steamLeaderboardManageButton;

            //"Current" thingy and the level titles
            TextMeshProUGUI advancedCurrent52 = advancedOptionsSub.FindPath("5-2 Options", "Level 5-2 Category", "Current Level Indicator").TMP();
            TextMeshProUGUI advancedTitle52 = advancedOptionsSub.FindPath("5-2 Options", "Level 5-2 Category").TMP();
            TextMeshProUGUI advancedCurrent71 = advancedOptionsSub.FindPath("7-1 Options", "Level 7-1 Category", "Current Level Indicator").TMP();
            TextMeshProUGUI advancedTitle71 = advancedOptionsSub.FindPath("7-1 Options", "Level 7-1 Category").TMP();
            TextMeshProUGUI advancedCurrent73 = advancedOptionsSub.FindPath("7-3 Options", "Level 7-3 Category", "Current Level Indicator").TMP();
            TextMeshProUGUI advancedTitle73 = advancedOptionsSub.FindPath("7-3 Options", "Level 7-3 Category").TMP();
            TextMeshProUGUI advancedCurrent7S = advancedOptionsSub.FindPath("7-S Options", "Level 7-S Category", "Current Level Indicator").TMP();
            TextMeshProUGUI advancedTitle84 = advancedOptionsSub.FindPath("8-4 Options", "Level 8-4 Category").TMP();
            TextMeshProUGUI advancedCurrent84 = advancedOptionsSub.FindPath("8-4 Options", "Level 8-4 Category", "Current Level Indicator").TMP();
            TextMeshProUGUI advancedTitle7S = advancedOptionsSub.FindPath("7-S Options", "Level 7-S Category").TMP();
            TextMeshProUGUI advancedCurrentP2 = advancedOptionsSub.FindPath("P-2 Options", "Level P-2 Category", "Current Level Indicator").TMP();
            TextMeshProUGUI advancedTitleP2 = advancedOptionsSub.FindPath("P-2 Options", "Level P-2 Category").TMP();
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
            TextMeshProUGUI advanced52WaterScrolling = advancedOptionsSub.FindPath("5-2 Options", "Disable Water Scrolling", "Text").TMP();
            TextMeshProUGUI advanced52WaterWaves = advancedOptionsSub.FindPath("5-2 Options", "Disable Water Waves", "Text").TMP();
            advanced52WaterScrolling.text = LanguageManager.CurrentLanguage.options.advanced_52WaterScrolling;
            advanced52WaterWaves.text = LanguageManager.CurrentLanguage.options.advanced_52WaterWaves;

            TextMeshProUGUI advanced71Dark = advancedOptionsSub.FindPath("7-1 Options", "Local High Scores", "Text").TMP();
            advanced71Dark.text = LanguageManager.CurrentLanguage.options.advanced_71Dark;

            TextMeshProUGUI advanced73Grass = advancedOptionsSub.FindPath("7-3 Options", "Local High Scores", "Text").TMP();
            advanced73Grass.text = LanguageManager.CurrentLanguage.options.advanced_73Grass;

            TextMeshProUGUI advanced84DisableArenaScrolling = advancedOptionsSub.FindPath("8-4 Options", "Local High Scores (1)", "Text").TMP();
            TextMeshProUGUI advanced84DisableArenaRotation = advancedOptionsSub.FindPath("8-4 Options", "Local High Scores", "Text").TMP();
            advanced84DisableArenaScrolling.text = LanguageManager.CurrentLanguage.options.advanced_84DisableArenaScrolling;
            advanced84DisableArenaRotation.text = LanguageManager.CurrentLanguage.options.advanced_84DisableArenaRotation;

            TextMeshProUGUI advanced7SHard = advancedOptionsSub.FindPath("7-S Options", "Local High Scores", "Text").TMP();
            advanced7SHard.text = LanguageManager.CurrentLanguage.options.advanced_7SHard;

            TextMeshProUGUI advanced_P2DisableTunnelScrolling = advancedOptionsSub.FindPath("P-2 Options", "Local High Scores", "Text").TMP();
            advanced_P2DisableTunnelScrolling.text = LanguageManager.CurrentLanguage.options.advanced_P2DisableTunnelScrolling;
            
        }
        
        private void PatchOptions(GameObject optionsMenu)
        {
            if (optionsMenu != null)
            {
                //Main buttons and text
                if (optionsMenu.Child("Text") != null)
                {
                    TextMeshProUGUI optionsText = optionsMenu.Child("Text").TMP();
                    optionsText.text = "--" + LanguageManager.CurrentLanguage.options.options_title + "--";
                }

                GameObject leftColumn = optionsMenu.Child("Navigation Rail");

                TextMeshProUGUI generalText = leftColumn.Child("Text (7)").TMP();
                generalText.text = "-- " + LanguageManager.CurrentLanguage.options.category_general + " --";

                TextMeshProUGUI generalButton = leftColumn.FindPath("General", "Text").TMP();
                generalButton.text = LanguageManager.CurrentLanguage.options.category_general;

                TextMeshProUGUI controlButton = leftColumn.FindPath("Controls", "Text").TMP();
                controlButton.text = LanguageManager.CurrentLanguage.options.category_controls;

                TextMeshProUGUI graphicsButton = leftColumn.FindPath("Video", "Text").TMP();
                graphicsButton.text = LanguageManager.CurrentLanguage.options.category_graphics;

                TextMeshProUGUI audioButton = leftColumn.FindPath("Audio", "Text").TMP();
                audioButton.text = LanguageManager.CurrentLanguage.options.category_audio;

                TextMeshProUGUI assistButton = leftColumn.FindPath("Assist", "Text").TMP();
                assistButton.text = LanguageManager.CurrentLanguage.options.category_assists;

                TextMeshProUGUI savesButton = leftColumn.FindPath("Saves", "Text").TMP();
                savesButton.text = LanguageManager.CurrentLanguage.options.category_saves;

                TextMeshProUGUI customizationText = leftColumn.Child("Text (8)").TMP();
                customizationText.text = "-- " + LanguageManager.CurrentLanguage.options.category_customization + " --";

                TextMeshProUGUI hudButton = leftColumn.FindPath("HUD", "Text").TMP();
                hudButton.text = LanguageManager.CurrentLanguage.options.category_hud;

                TextMeshProUGUI colorsButton = leftColumn.FindPath("Colors", "Text").TMP();
                colorsButton.text = LanguageManager.CurrentLanguage.options.category_colors;

                TextMeshProUGUI backText = leftColumn.FindPath("Back", "Text").TMP();
                backText.text = LanguageManager.CurrentLanguage.options.options_back;

                TextMeshProUGUI paletteSelectorClose = optionsMenu.FindPath("Palette Selector", "Close", "Text").TMP();
                paletteSelectorClose.text = LanguageManager.CurrentLanguage.options.save_close;

                try
                {
                    GameObject savesOptions = optionsMenu.Child("Save Slots");
                    try { PatchSavesOptions(savesOptions); } catch (Exception e) { Logging.Error("Failed to patch save options."); Logging.Error(e.ToString()); }
                    GameObject colorblindOptions = optionsMenu.FindPath("Pages", "ColorBlindness Options");
                    try { PatchColorsOptions(colorblindOptions); } catch (Exception e) { Logging.Error("Failed to patch color options."); Logging.Error(e.ToString()); }
                    GameObject rumbleOptions = optionsMenu.Child("Rumble Settings");
                    try { PatchRumbleOptions(rumbleOptions); } catch (Exception e) { Logging.Error("Failed to patch rumble options."); Logging.Error(e.ToString()); }
                    GameObject advancedOptions = optionsMenu.Child("Advanced Options");
                    try { PatchAdvancedOptions(advancedOptions); } catch (Exception e) { Logging.Error("Failed to patch advanced options."); Logging.Error(e.ToString()); }
                    GameObject steamOptions = optionsMenu.Child("Leaderboard Manager");
                    try { PatchSteamLeaderboard(steamOptions); } catch (Exception e) { Logging.Error("Failed to patch steam leaderboard."); Logging.Error(e.ToString()); }

                    GameObject pages = optionsMenu.Child("Pages");
                    if (pages != null)
                    {
                        GameObject graphicsPage = pages.Child("Graphics");
                        try { PatchGraphicsOptions(graphicsPage); } catch (Exception e) { Logging.Error("Failed to patch graphics options."); Logging.Error(e.ToString()); }
                        GameObject audioPage = pages.Child("Audio");
                        try { PatchAudioOptions(audioPage); } catch (Exception e) { Logging.Error("Failed to patch audio options."); Logging.Error(e.ToString()); }
                        GameObject assistPage = pages.Child("Assist");
                        try { PatchAssistOptions(assistPage); } catch (Exception e) { Logging.Error("Failed to patch assist options."); Logging.Error(e.ToString()); }
                    }
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
            try
            {
                SetText(optionMenu.Child("Title"), "--" + LanguageManager.CurrentLanguage.options.steamLeaderboard_title + "--");
                SetText(optionMenu.FindPath("Refresh Button", "Text"), LanguageManager.CurrentLanguage.options.steamLeaderboard_refreshButton);
                SetText(optionMenu.FindPath("Close", "Text"), LanguageManager.CurrentLanguage.options.steamLeaderboard_returnButton);

                GameObject SteamEntryList = optionMenu.FindPath("Scroll View", "Viewport", "Content");
                if (SteamEntryList == null) return;

                int childCount = SteamEntryList.transform.childCount;
                for (int x = 0; x < childCount; x++)
                {
                    GameObject entry = SteamEntryList.ChildAt(x);

                    SetText(entry.Child("Any Label"), LanguageManager.CurrentLanguage.options.steamLeaderboard_anyLabel);
                    SetText(entry.Child("P Label"), LanguageManager.CurrentLanguage.options.steamLeaderboard_pLabel);
                    SetText(entry.FindPath("Any Reset", "Text"), LanguageManager.CurrentLanguage.options.steamLeaderboard_reset);
                    SetText(entry.FindPath("P Reset Button", "Text"), LanguageManager.CurrentLanguage.options.steamLeaderboard_reset);
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
                this.optionsMenu = game.Child("OptionsMenu");
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
                this.optionsMenu = pauseObject.Child("OptionsMenu");
            }
            this.PatchOptions(this.optionsMenu);
        }
    }
}
