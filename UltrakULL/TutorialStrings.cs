using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    class TutorialStrings
    {
        public string IntroFirstPage;

        //+ "Y/N ~ \n \n"
        //+ " AUDIO§         Ä½ \n"
        //+ " VIDEO§         Ä½ \n"
        //+ "MECHANICS§     Ä½ \n"
        //+ "+ CALIBRATION COMPLETE_ \n"
        //+ "+PRIMARY SETTINGS UPDATED_ \n"
        //+ "(±ASSIST OPTIONS_ AVAILABLE IN PAUSE MENU)½ \n"
        //+ "+ALL SYSTEMS OPERATIONAL_½ \n"
        //+ "LOADING STATUS UPDATE§ \n";

        public string IntroSecondPage;

        /*
         *  MACHINE ID:            V1½½
            LOCATION:              APPROACHING HELL½½@
            CURRENT OBJECTIVE:     FIND A WEAPON½½

            *MANKIND IS DEAD._½½
            *BLOOD IS FUEL._½½
            *HELL IS FULL._½½&
         */

        public static string GetMessage(string inputMessage, string inputMessage2, string input)
        {
            string fullMessage = inputMessage + inputMessage2;

            if (fullMessage.Contains("PUNCH"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_punch1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.tutorial.tutorial_punch2);
            }
            else if (fullMessage.Contains("SLIDE"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_slide1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.tutorial.tutorial_slide2);
            }
            else if (fullMessage.Contains("DASH"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_dash1 + "<color=#00DFFF>" + input + "</color>" + LanguageManager.CurrentLanguage.tutorial.tutorial_dash2 + "\n" + LanguageManager.CurrentLanguage.tutorial.tutorial_dash3);
            }
            else if (fullMessage.Contains("HEALTH"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_health1 + "\n" + LanguageManager.CurrentLanguage.tutorial.tutorial_health2);
            }
            else if (fullMessage.Contains("JUMP"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_walljump);
            }
            else if (fullMessage.Contains("SHOCKWAVE"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_shockwave1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.tutorial.tutorial_shockwave2 + "\n" + LanguageManager.CurrentLanguage.tutorial.tutorial_shockwave3);
            }
            else if (fullMessage.Contains("ORBS"))
            {
                return (LanguageManager.CurrentLanguage.tutorial.tutorial_orb1 + "\n" + LanguageManager.CurrentLanguage.tutorial.tutorial_orb2);
            }
            else
            {
                return ("Unimplemented string");
            }
        }

        //IMPORTANT CHARACTERS TO USE:
        // # - 3 repeating dots
        // § - Indent
        // + - Lime green text
        // * - Red text
        // ± - Blue text
        // _ - Close color
        // ½ - Half second pause
        // @ - Begins to fade out intro music
        // ~ - Wait for recalibration input
        // & - Ends intro text and loads the tutorial
        // β - Recalibration yes (automatically shows keyboard or controller button depending on what the player is using)
        // δ - Recalibration no (automatically shows keyboard or controller button depending on what the player is using)
		// Ä - Show calibration menu

        public void PatchCalibrationWindows(ref GameObject canvasObj)
        {
            try
            {
                GameObject calibrationIntro = GetGameObjectChild(canvasObj, "Intro");
                GameObject calibrationAudioWindow = GetGameObjectChild(calibrationIntro, "Audio Calibration");
                GameObject calibrationAudioWindowWarning = GetGameObjectChild(calibrationAudioWindow, "Warning");
                GameObject calibrationVideoWindow = GetGameObjectChild(calibrationIntro, "Video Calibration");
                GameObject calibrationMechanicsWindow = GetGameObjectChild(GetGameObjectChild(calibrationIntro, "Difficulty Select"), "Interactables");
                GameObject calibrationControllerWindow = GetGameObjectChild(calibrationIntro, "Auto-Aim Settings");

                TextMeshProUGUI nofade = GetTextMeshProUGUI(GetGameObjectChild(calibrationIntro, "Page 2 NoFade"));
                nofade.text = 
                    "<color=red> " + LanguageManager.CurrentLanguage.tutorial.tutorial_introRed1 + "\n "
                    + LanguageManager.CurrentLanguage.tutorial.tutorial_introRed2 + "\n "
                    + LanguageManager.CurrentLanguage.tutorial.tutorial_introRed3 + "</color>";

                //Audio
                TextMeshProUGUI calibrationAudioTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationAudioWindow, "Text"));
                calibrationAudioTitle.text = LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationTitle;

                TextMeshProUGUI calibrationAudioMaster = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "Master Volume (1)"), "Text"));
                calibrationAudioMaster.text = LanguageManager.CurrentLanguage.options.audio_globalVolume;

                TextMeshProUGUI calibrationAudioSFX = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "SFX Volume (1)"), "Text"));
                calibrationAudioSFX.text = LanguageManager.CurrentLanguage.options.audio_soundEffectsVolume;

                TextMeshProUGUI calibrationAudioMusic = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "Music Volume (1)"), "Text"));
                calibrationAudioMusic.text = LanguageManager.CurrentLanguage.options.audio_musicVolume;

                TextMeshProUGUI calibrationAudioDone = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "Bone (1)"), "Text"));
                calibrationAudioDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone.ToUpper();
                
                TextMeshProUGUI calibrationAudioDoneAlt = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "Done"), "Text"));
                calibrationAudioDoneAlt.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone.ToUpper();

                SliderValueToText calibrationAudioMasterSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "Master Volume (1)"), "Button"), "Slider (1)"), "Text (2)").GetComponentInChildren<SliderValueToText>();
				calibrationAudioMasterSlider.ifMin = LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationSliderOFF;

                SliderValueToText calibrationAudioSFXSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "SFX Volume (1)"), "Button"), "Slider (1)"), "Text (2)").GetComponentInChildren<SliderValueToText>();
                calibrationAudioSFXSlider.ifMin = LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationSliderOFF;

                SliderValueToText calibrationAudioMusicSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindow, "Music Volume (1)"), "Button"), "Slider (1)"), "Text (2)").GetComponentInChildren<SliderValueToText>();
                calibrationAudioMusicSlider.ifMin = LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationSliderOFF;

                //Audio warning
                TextMeshProUGUI calibrationMasterAudioWarningPrompt = GetTextMeshProUGUI(GetGameObjectChild(calibrationAudioWindowWarning, "Text (No Master)"));
                calibrationMasterAudioWarningPrompt.text =
                    "<color=red>" + LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationWarning1 +"</color>" + "\n\n"
                     + LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationWarning2 + "\n\n" +
                     LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationWarning3;

                TextMeshProUGUI calibrationSFXAudioWarningPrompt = GetTextMeshProUGUI(GetGameObjectChild(calibrationAudioWindowWarning, "Text (No SFX)"));
                calibrationSFXAudioWarningPrompt.text =
                    "<color=red>" + LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationSFXWarning1 + "</color>" + "\n\n"
                     + LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationSFXWarning2 + "\n\n" +
                     LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationSFXWarning3;

                TextMeshProUGUI calibrationAudioWarningPromptYes = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindowWarning, "Done (1)"), "Text"));
                calibrationAudioWarningPromptYes.text = LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationWarningPromptYes;

                TextMeshProUGUI calibrationAudioWarningPromptNo = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationAudioWindowWarning, "Done (2)"), "Text"));
                calibrationAudioWarningPromptNo.text = LanguageManager.CurrentLanguage.tutorial.tutorial_audioCalibrationWarningPromptNo;

                //Video
                TextMeshProUGUI calibrationVideoTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationVideoWindow, "Text"));
                calibrationVideoTitle.text = LanguageManager.CurrentLanguage.tutorial.tutorial_videoCalibrationTitle;

                TextMeshProUGUI calibrationVideoPcDescription = GetTextMeshProUGUI(GetGameObjectChild(calibrationVideoWindow, "Text (1)"));
                calibrationVideoPcDescription.text = LanguageManager.CurrentLanguage.tutorial.tutorial_videoCalibrationPcDescription;

                TextMeshProUGUI calibrationVideoPsxDescription = GetTextMeshProUGUI(GetGameObjectChild(calibrationVideoWindow, "Text (2)"));
                calibrationVideoPsxDescription.text = LanguageManager.CurrentLanguage.tutorial.tutorial_videoCalibrationPsxDescription;

                //Mechanics (difficulty)
                TextMeshProUGUI calibrationMechanicsTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationMechanicsWindow, "Title"));
                calibrationMechanicsTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_title + "--";

                TextMeshProUGUI calibrationMechanicsEasy = GetTextMeshProUGUI(GetGameObjectChild(calibrationMechanicsWindow, "Easy"));
                calibrationMechanicsEasy.text = LanguageManager.CurrentLanguage.frontend.difficulty_easy;

                TextMeshProUGUI calibrationMechanicsMedium = GetTextMeshProUGUI(GetGameObjectChild(calibrationMechanicsWindow, "Normal"));
                calibrationMechanicsMedium.text = LanguageManager.CurrentLanguage.frontend.difficulty_normal;

                TextMeshProUGUI calibrationMechanicsHard = GetTextMeshProUGUI(GetGameObjectChild(calibrationMechanicsWindow, "Hard"));
                calibrationMechanicsHard.text = LanguageManager.CurrentLanguage.frontend.difficulty_hard;

                TextMeshProUGUI calibrationMechanicsHarmless = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "Casual Easy"), "Name"));
                calibrationMechanicsHarmless.text = LanguageManager.CurrentLanguage.frontend.difficulty_harmless;

                TextMeshProUGUI calibrationMechanicsLenient = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "Casual Hard"), "Name"));
                calibrationMechanicsLenient.text = LanguageManager.CurrentLanguage.frontend.difficulty_lenient;

                TextMeshProUGUI calibrationMechanicsStandard = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "Standard"), "Name"));
                calibrationMechanicsStandard.text = LanguageManager.CurrentLanguage.frontend.difficulty_standard + " <color=orange>*</color>";

                TextMeshProUGUI calibrationMechanicsViolent = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "Violent"), "Name"));
                calibrationMechanicsViolent.text = LanguageManager.CurrentLanguage.frontend.difficulty_violent;

                TextMeshProUGUI calibrationMechanicsBrutal = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "Brutal"), "Name"));
                calibrationMechanicsBrutal.text = LanguageManager.CurrentLanguage.frontend.difficulty_brutal;

                TextMeshProUGUI calibrationMechanicsUmd = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "V1 Must Die"), "Name"));
                calibrationMechanicsUmd.text = LanguageManager.CurrentLanguage.frontend.difficulty_umd;

                //Harmless info
                GameObject calibrationHarmlessInfo = GetGameObjectChild(calibrationMechanicsWindow, "Harmless Info");
                TextMeshProUGUI harmlessTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationHarmlessInfo, "Title (1)"));
                harmlessTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_harmless + "--";

                //Harmless descriptor
                TextMeshProUGUI harmlessDescriptor = GetTextMeshProUGUI(GetGameObjectChild(calibrationHarmlessInfo, "Text"));
                harmlessDescriptor.text =
                    LanguageManager.CurrentLanguage.frontend.difficulty_harmlessDescription1
                    + "\n\n"
                    + LanguageManager.CurrentLanguage.frontend.difficulty_harmlessDescription2
                    + "\n\n"
                    + "<color=green>" + LanguageManager.CurrentLanguage.frontend.difficulty_harmlessDescription3 + "</color>";

                //Lenient title
                GameObject calibrationLenientInfo = GetGameObjectChild(calibrationMechanicsWindow, "Lenient Info");
                TextMeshProUGUI lenientTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationLenientInfo, "Title (1)"));
                lenientTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_lenient + "--";

                //Lenient descriptor
                TextMeshProUGUI lenientDescriptor = GetTextMeshProUGUI(GetGameObjectChild(calibrationLenientInfo, "Text"));
                lenientDescriptor.text =
                    LanguageManager.CurrentLanguage.frontend.difficulty_lenientDescription1
                    + "\n\n"
                    + LanguageManager.CurrentLanguage.frontend.difficulty_lenientDescription2
                    + "\n\n"
                    + "<color=yellow>" + LanguageManager.CurrentLanguage.frontend.difficulty_lenientDescription3 + "</color>";

                //Standard title
                GameObject calibrationStandardInfo = GetGameObjectChild(calibrationMechanicsWindow, "Standard Info");
                TextMeshProUGUI standardTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationStandardInfo, "Title (1)"));
                standardTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_standard + "--";

                //Standard descriptor
                TextMeshProUGUI standardDescriptor = GetTextMeshProUGUI(GetGameObjectChild(calibrationStandardInfo, "Text"));
                standardDescriptor.text =
                    LanguageManager.CurrentLanguage.frontend.difficulty_standardDescription1
                    + "\n\n"
                    + LanguageManager.CurrentLanguage.frontend.difficulty_standardDescription2
                    + "\n\n"
                    + "<color=orange>" + LanguageManager.CurrentLanguage.frontend.difficulty_standardDescription3 + "</color>";

                //Violent title
                GameObject calibrationViolentInfo = GetGameObjectChild(calibrationMechanicsWindow, "Violent Info");
                TextMeshProUGUI violentTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationViolentInfo, "Title (1)"));
                violentTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_violent + "--";

                //Violent descriptor
                TextMeshProUGUI violentDescriptor = GetTextMeshProUGUI(GetGameObjectChild(calibrationViolentInfo, "Text"));
                violentDescriptor.text =
                    LanguageManager.CurrentLanguage.frontend.difficulty_violentDescription1
                    + "\n\n"
                    + LanguageManager.CurrentLanguage.frontend.difficulty_violentDescription2
                    + "\n\n"
                    + "<color=red>" + LanguageManager.CurrentLanguage.frontend.difficulty_violentDescription3 + "</color>";

                //Brutal title
                GameObject calibrationBrutalInfo = GetGameObjectChild(calibrationMechanicsWindow, "Brutal Info");
                TextMeshProUGUI brutalTitle = GetTextMeshProUGUI(GetGameObjectChild(calibrationBrutalInfo, "Title (1)"));
                brutalTitle.text = "--" + LanguageManager.CurrentLanguage.frontend.difficulty_brutal + "--";
                //Brutal descriptor
                TextMeshProUGUI brutalDescriptor = GetTextMeshProUGUI(GetGameObjectChild(calibrationBrutalInfo, "Text"));
                brutalDescriptor.text =
                    "<color=white>" + LanguageManager.CurrentLanguage.frontend.difficulty_brutalDescription1
                    + "\n\n"
                    + LanguageManager.CurrentLanguage.frontend.difficulty_brutalDescription2 + "</color>"
                    + "\n\n"
                    + "<b>" + LanguageManager.CurrentLanguage.frontend.difficulty_brutalDescription3 + "</b>";

                TextMeshProUGUI underConstructionText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationMechanicsWindow, "V1 Must Die"), "Under Construction"));
                underConstructionText.text = LanguageManager.CurrentLanguage.frontend.difficulty_underConstruction;

                //Controller/autoaim settings //Updated patch to objects in REVAMP update
                calibrationControllerWindow.SetActive(true); //Fast on-off for load all childs in GameObject
                calibrationControllerWindow.SetActive(false);
                TextMeshProUGUI calibrationControllerTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(calibrationControllerWindow, "Contents"), "! Controller Detected !"), "Text"));
                calibrationControllerTitle.text = "! " + LanguageManager.CurrentLanguage.tutorial.tutorial_controllerCalibrationTitle + " !\n<size=16>" + LanguageManager.CurrentLanguage.tutorial.tutorial_controllerCalibrationSubtitle + "</size>";

                TextMeshProUGUI calibrationControllerAutoAimToggle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(calibrationControllerWindow, "Contents"), "Auto Aim"), "Text"));
                calibrationControllerAutoAimToggle.text = LanguageManager.CurrentLanguage.options.assists_autoAim;

                GameObject calibrationControllerAutoAimAmount = GetGameObjectChild(GetGameObjectChild(calibrationControllerWindow, "Contents"), "Auto Aim Amount");
                TextMeshProUGUI calibrationControllerAutoAimPercent = GetTextMeshProUGUI(GetGameObjectChild(calibrationControllerAutoAimAmount, "Text"));
                calibrationControllerAutoAimPercent.text = LanguageManager.CurrentLanguage.options.assists_autoAimPercent;
                SliderValueToText autoAimSlider = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(calibrationControllerAutoAimAmount, "Slider Button(Clone)"), "Slider"), "Text").GetComponentInChildren<SliderValueToText>();
                autoAimSlider.ifMin = "0";
                autoAimSlider.ifMax = "100";

                TextMeshProUGUI calibrationAssistDone = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(calibrationControllerWindow, "Done"), "Text"));
                calibrationAssistDone.text = LanguageManager.CurrentLanguage.shop.shop_colorsDone.ToUpper();

                TextMeshProUGUI calibrationControllerAutoAimReminder = GetTextMeshProUGUI(GetGameObjectChild(calibrationControllerWindow, "Text (2)"));
                calibrationControllerAutoAimReminder.text = LanguageManager.CurrentLanguage.tutorial.tutorial_controllerCalibrationTooltip;

                //Tooltip
                GameObject assistTip = GetGameObjectChild(calibrationMechanicsWindow, "Assist Tip");
                TextMeshProUGUI assistTipText = GetTextMeshProUGUI(assistTip);
                assistTipText.text = LanguageManager.CurrentLanguage.frontend.difficulty_tweakReminder;
            }
            catch(Exception e)
            {
                Logging.Error("Failed to patch tutorial panels");
                Logging.Error(e.ToString());
            }
        }

        public TutorialStrings(ref GameObject canvasObj)
        {
            this.IntroFirstPage =
                LanguageManager.CurrentLanguage.tutorial.tutorial_introStartup1 + "#" + LanguageManager.CurrentLanguage.tutorial.tutorial_introStartup2 + "½ \n\n"

                + LanguageManager.CurrentLanguage.tutorial.tutorial_introVersion1 + "# \n"
                + "+" + LanguageManager.CurrentLanguage.tutorial.tutorial_introVersion2 + "_½ \n\n"

                + LanguageManager.CurrentLanguage.tutorial.tutorial_introCalibration1 + "#\n"
                + "+" + LanguageManager.CurrentLanguage.tutorial.tutorial_introCalibration2 + "_\n\n"

                + LanguageManager.CurrentLanguage.tutorial.tutorial_recalibrationPrompt + "\n β/δ~ \n"

                + LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationAudio + "§Ä½ \n"
                + LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationVideo + "§Ä½ \n"
                + LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationMechanics + "§Ä½ \n\n"

                + "+" + LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationComplete1 + "_ \n"
                + "+" + LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationComplete2 + "_ \n"
                + "(±" + LanguageManager.CurrentLanguage.tutorial.tutorial_introReminder1 + " _" + LanguageManager.CurrentLanguage.tutorial.tutorial_introReminder2 + " )½ \n\n"

                + "+" + LanguageManager.CurrentLanguage.tutorial.tutorial_systemsOperational + "_½ \n"
                + LanguageManager.CurrentLanguage.tutorial.tutorial_introLoadStatus + "§";

            this.IntroSecondPage = " " +
            LanguageManager.CurrentLanguage.tutorial.tutorial_introStatusUpdate + ":½\n\n " +
            LanguageManager.CurrentLanguage.tutorial.tutorial_introID1 + ":     " + LanguageManager.CurrentLanguage.tutorial.tutorial_introID2 + "½½\n "
            + LanguageManager.CurrentLanguage.tutorial.tutorial_introLocation1 + ":     " + LanguageManager.CurrentLanguage.tutorial.tutorial_introLocation2 + "½½@\n "
            + LanguageManager.CurrentLanguage.tutorial.tutorial_introObjective1 + ":    " + LanguageManager.CurrentLanguage.tutorial.tutorial_introObjective2 + "½½\n\n"
            + "*" + LanguageManager.CurrentLanguage.tutorial.tutorial_introRed1 + "_½½\n"
            + "*" + LanguageManager.CurrentLanguage.tutorial.tutorial_introRed2 + "_½½\n"
            + "*" + LanguageManager.CurrentLanguage.tutorial.tutorial_introRed3 + "_½½&";

            PatchCalibrationWindows(ref canvasObj);

        }
    }
}
