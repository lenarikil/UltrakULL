using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    public static class CyberGrind
    {
        private static void PatchWaveBoard()
        {
            //Get the object containing all the wave board strings.
            //If there's a better way of doing this someone let me know

            GameObject coreGame = GameObject.Find("Everything");
            List<GameObject> everythingList = new List<GameObject>();

            foreach(Transform child in coreGame.transform)
            {
                everythingList.Add(child.gameObject);
            }

            List<GameObject> cubeCanvasList = new List<GameObject>();
            GameObject cubeCanvas = GetGameObjectChild(everythingList[4],"Canvas");
            foreach (Transform child in cubeCanvas.transform)
            {
                cubeCanvasList.Add(child.gameObject);
            }
            GameObject cgBoard = cubeCanvasList[1];

            //Patch all the strings here.
            Text waveText = GetTextfromGameObject(GetGameObjectChild(cgBoard, "Wave Title"));
            waveText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wave +  ":";

            Text enemiesLeftText = GetTextfromGameObject(GetGameObjectChild(cgBoard, "Enemies Left Title"));
            enemiesLeftText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_enemiesRemaining + ":";

        }

        private static void PatchResults()
        {
            GameObject level = GameObject.Find("Player");

            GameObject resultsPanel = GetGameObjectChild(GetGameObjectChild(level, "FinishCanvas (1)"), "Panel");
            GameObject lastResult = GetGameObjectChild(resultsPanel, "Panel");
            GameObject bestResult = GetGameObjectChild(GetGameObjectChild(resultsPanel, "Panel (1)"),"Filler");
            GameObject pointsPanel = GetGameObjectChild(resultsPanel, "Total Points");
            GameObject leaderboardsPanel = GetGameObjectChild(resultsPanel, "Cyber Grind Leaderboards");

            //Both result panels use the same strings, so declare them here to avoid redundancy.
            string wave = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wave;
            string kills = LanguageManager.CurrentLanguage.misc.levelstats_kills;
            string style = LanguageManager.CurrentLanguage.misc.levelstats_style;
            string time = LanguageManager.CurrentLanguage.misc.levelstats_time;


            //Title
            TextMeshProUGUI titleText= GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(resultsPanel, "Title"),"Text"));
            titleText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_cgTitle;

            //Last result panel
            TextMeshProUGUI lastTitle = GetTextMeshProUGUI(GetGameObjectChild(lastResult, "Text"));
            lastTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_previousRun;

            TextMeshProUGUI lastWave = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(lastResult, "Wave - Info"),"Text"));
            lastWave.text = wave;

            TextMeshProUGUI lastKills = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(lastResult, "Kills - Info"), "Text"));
            lastKills.text = kills;

            TextMeshProUGUI lastStyle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(lastResult, "Style - Info"), "Text"));
            lastStyle.text = style;

            TextMeshProUGUI lastTime = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(lastResult, "Time - Info"), "Text"));
            lastTime.text = time;

            //Best result panel
            TextMeshProUGUI bestTitle = GetTextMeshProUGUI(GetGameObjectChild(bestResult, "Text (1)"));
            bestTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_bestRun;

            TextMeshProUGUI bestWave = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(bestResult, "Wave - Info (1)"), "Text"));
            bestWave.text = wave;

            TextMeshProUGUI bestKills = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(bestResult, "Kills - Info (1)"), "Text"));
            bestKills.text = kills;

            TextMeshProUGUI bestStyle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(bestResult, "Style - Info (1)"), "Text"));
            bestStyle.text = style;

            TextMeshProUGUI bestTime = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(bestResult, "Time - Info (1)"), "Text"));
            bestTime.text = time;

            //Points panel
            TextMeshProUGUI totalPointsText = GetTextMeshProUGUI(GetGameObjectChild(pointsPanel, "Text (1)"));
            totalPointsText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_total;

            TextMeshProUGUI totalPoints = GetTextMeshProUGUI(GetGameObjectChild(pointsPanel, "Text"));
            totalPoints.text = "+0" + "<color=orange>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>";

            //Leaderboards

            string connecting = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_connectingToSteam;

            GameObject friendScores = GetGameObjectChild(leaderboardsPanel, "Friend High Scores");
            GameObject globalScores = GetGameObjectChild(leaderboardsPanel, "Global High Scores");

            TextMeshProUGUI friendScoresTitle = GetTextMeshProUGUI(GetGameObjectChild(friendScores, "Text"));
            friendScoresTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_friendScores;

            TextMeshProUGUI globalScoresTitle = GetTextMeshProUGUI(GetGameObjectChild(globalScores, "Text"));
            globalScoresTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_globalScores;

            TextMeshProUGUI friendsConnectingText = GetTextMeshProUGUI(GetGameObjectChild(friendScores, "Connecting"));
            friendsConnectingText.text = connecting;

            TextMeshProUGUI globalConnectingText = GetTextMeshProUGUI(GetGameObjectChild(globalScores, "Connecting"));
            globalConnectingText.text = connecting;


        }

        private static void PatchTerminal()
        {
            GameObject level = GameObject.Find("FirstRoom");
            GameObject cgTerminal = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(level, "Room"), "Cybergrind Shop"), "Canvas");

            GameObject cgTerminalMainPanel = GetGameObjectChild(GetGameObjectChild(cgTerminal, "Background"), "Main Panel");

            //Terminal description(I just ripped off from shop.cs lol)
            GameObject tipPanel = GetGameObjectChild(cgTerminalMainPanel, "Stats");
            TextMeshProUGUI cgTerminalTipboxTitle = GetTextMeshProUGUI(GetGameObjectChild(tipPanel, "Title"));
            cgTerminalTipboxTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_settings;

            TextMeshProUGUI cgTerminalTipboxDescription = GetTextMeshProUGUI((GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(tipPanel, "Panel"), "Text Inset"), "Text")));
            cgTerminalTipboxDescription.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_settingsDescription;

            //Main menu
            GameObject mainButtons = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Main Menu"), "Buttons");

            TextMeshProUGUI cgTerminalThemesText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(mainButtons, "Themes Button"), "Text"));
            cgTerminalThemesText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themes;

            TextMeshProUGUI cgTerminalMusicText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(mainButtons, "Music Button"), "Text"));
            cgTerminalMusicText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_music;

            TextMeshProUGUI cgTerminalPatternsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(mainButtons, "Patterns Button"), "Text"));
            cgTerminalPatternsText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patterns;

            TextMeshProUGUI cgTerminalWaveText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(mainButtons, "Waves Button"), "Text"));
            cgTerminalWaveText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_waves;

            //Themes
            GameObject cgTerminalThemes = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Themes"),"Preset Panel");

            TextMeshProUGUI cgTerminalThemesTitle = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalThemes, "Title"));
            cgTerminalThemesTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesTitle;

            GameObject cgTerminalThemesPanel = GetGameObjectChild(cgTerminalThemes, "Panel");

            TextMeshProUGUI cgTerminalThemesDescription = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalThemesPanel, "Text"));
            cgTerminalThemesDescription.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesDescription;
            
            GameObject cgTerminalThemesButton = GetGameObjectChild(cgTerminalThemesPanel, "Buttons");

            TextMeshProUGUI cgTerminalThemesLight = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalThemesButton, "Light Button"), "Text"));
            cgTerminalThemesLight.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesLight;

            TextMeshProUGUI cgTerminalThemesDark = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalThemesButton, "Dark Button"), "Text"));
            cgTerminalThemesDark.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesDark;

            TextMeshProUGUI cgTerminalThemesCustom = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalThemesButton, "Custom Button"), "Text"));
            cgTerminalThemesCustom.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustom;

            TextMeshProUGUI cgTerminalThemesBack = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalThemes.transform.parent.gameObject, "Back Button"), "Text"));
            cgTerminalThemesBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;

            //Playlist
            GameObject cgMusic = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Playlist"),"Panel");
            
            TextMeshProUGUI cgMusicTitle = GetTextMeshProUGUI(GetGameObjectChild(cgMusic.transform.parent.gameObject,"Title"));
            cgMusicTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicTitle;
            
            TextMeshProUGUI cgMusicClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgMusic, "Close Button"),"Text"));
            cgMusicClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            //Songs Type Selection(+ button in playlist will show this up)
            GameObject cgMusicTypeCanvas = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Songs Type Selection"), "Panel");

            TextMeshProUGUI cgMusicTypeTitle = GetTextMeshProUGUI(GetGameObjectChild(cgMusicTypeCanvas.transform.parent.gameObject, "Title"));
            cgMusicTypeTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicType;

            GameObject cgMusicTypeButtons = GetGameObjectChild(GetGameObjectChild(cgMusicTypeCanvas, "Inset"), "Type Selection Buttons");
            TextMeshProUGUI cgMusicTypeULTRAKILL = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgMusicTypeButtons, "Soundtrack Button"), "Text"));
            cgMusicTypeULTRAKILL.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicSoundtrack;

            TextMeshProUGUI cgMusicTypeCustom = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgMusicTypeButtons, "Custom Button"), "Text"));
            cgMusicTypeCustom.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustom;

            TextMeshProUGUI cgMusicTypeClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgMusicTypeCanvas, "Close Button"), "Text"));
            cgMusicTypeClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            GameObject cgMusicSoundtrack = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Songs Soundtrack"),"Panel");
            TextMeshProUGUI cgMusicSoundtrackTitle = GetTextMeshProUGUI(GetGameObjectChild(cgMusicSoundtrack.transform.parent.gameObject,"Title"));
            cgMusicSoundtrackTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicSoundtrack;

            TextMeshProUGUI cgMusicSoundtrackClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgMusicSoundtrack,"Close Button"),"Text"));
            cgMusicSoundtrackClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            GameObject cgMusicSoundtrackAddMenu = GetGameObjectChild(GetGameObjectChild(cgMusicSoundtrack,"Inset"),"Songs");

            //CustomMusic
            GameObject cgCustomMusic = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Songs Custom"), "Panel");

            TextMeshProUGUI cgCustomMusicTitle = GetTextMeshProUGUI(GetGameObjectChild(cgCustomMusic.transform.parent.gameObject, "Title"));
            cgCustomMusicTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustom;

            TextMeshProUGUI cgCustomMusicClose = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomMusic, "Close Button"), "Text"));
            cgCustomMusicClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            //Changes the "Unlocked" string under songs that are unlocked

            foreach (Transform child in cgMusicSoundtrackAddMenu.transform)
            {
                if (child.name == "Song Template(Clone)")
                {
                    TextMeshProUGUI cgMusicSoundtrackTask = GetTextMeshProUGUI(GetGameObjectChild(child.gameObject, "Requirement"));
                    if (cgMusicSoundtrackTask.text == "Unlocked") { cgMusicSoundtrackTask.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicUnlocked; }
                }
            }
            Button[] aas = cgMusicSoundtrack.GetComponentsInChildren<Button>(true);
            foreach (Button button in aas)
            {
                button.onClick.AddListener(delegate { PatchTerminalFolder(); });
            }
            
            
            //Customize theme
            GameObject cgCustomTheme = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Theme Custom"),"Panel");
            TextMeshProUGUI cgCustomThemeTitle = GetTextMeshProUGUI(GetGameObjectChild(cgCustomTheme.transform.parent.gameObject, "Title"));
            //"Custom", replace this later
            cgCustomThemeTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesModify;

            GameObject cgCustomThemeButtons = GetGameObjectChild(cgCustomTheme, "Sidebar");
            GameObject cgCustomThemeSelectorButtons = GetGameObjectChild(cgCustomThemeButtons, "Selector Buttons");
            TextMeshProUGUI cgCustomGrid = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomThemeSelectorButtons, "Grid Button"),"Text"));
            cgCustomGrid.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomGrid;

            TextMeshProUGUI cgCustomGridGlow = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomThemeSelectorButtons, "Glow Button"), "Text"));
            cgCustomGridGlow.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomGridGlow;

            TextMeshProUGUI cgCustomSkybox = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomThemeSelectorButtons, "Skybox Button"), "Text"));
            cgCustomSkybox.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomSkybox;

            TextMeshProUGUI cgCustomFog = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomThemeSelectorButtons, "Fog Button"), "Text"));
            cgCustomFog.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFog;

            TextMeshProUGUI cgCustomThemeBack = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomThemeButtons, "Back Button"), "Text"));
            cgCustomThemeBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;

            //Leftside Buttons(Custom Theme)
            GameObject cgCustomAdditionalRows = GetGameObjectChild(cgCustomTheme, "Window");

            TextMeshProUGUI cgCustomRefresh = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(cgCustomAdditionalRows, "Grid Wrapper"),"Refresh Button"),"Text"));
            cgCustomRefresh.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsRefresh;

            GameObject cgCustomGridTypeSelection = GetGameObjectChild(cgCustomAdditionalRows, "Grid Type Selection");

            TextMeshProUGUI cgCustomAdditionalBase = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomGridTypeSelection, "Base Button"), "Text"));
            cgCustomAdditionalBase.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBase;

            TextMeshProUGUI cgCustomAdditionalTopRow = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomGridTypeSelection, "Top Row Button"), "Text"));
            cgCustomAdditionalTopRow.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomTopRow;

            TextMeshProUGUI cgCustomAdditionalTop = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomGridTypeSelection, "Top Button"), "Text"));
            cgCustomAdditionalTop.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomTop;

            TextMeshProUGUI cgCustomAdditionalGlowIntensity = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomAdditionalRows, "Glow Intensity"),"Title"));
            cgCustomAdditionalGlowIntensity.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomGlowIntensity;
            //Fog Control goes here, add it later

            GameObject cgCustomFogWindow = GetGameObjectChild(cgCustomAdditionalRows, "Fog Control");
            GameObject cgCustomFogSlider = GetGameObjectChild(cgCustomFogWindow, "Sliders");
            GameObject cgCustomFogSliderLayoutGroup = GetGameObjectChild(cgCustomFogSlider, "Layout Group"); //This is the parent of all sliders
            GameObject cgCustomFogTabs = GetGameObjectChild(cgCustomFogWindow, "Tabs"); //Now this GameObject contains all buttons to switch fog type. "Disable", "Static", "Dynamic"

            //Patch Color
            TextMeshProUGUI cgCustomFogColor = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogWindow, "Color"), "Text")); //Color moved to Window in 16d Patch
            cgCustomFogColor.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogColor;

            //Patch Sliders Text and "Disable" Description
            TextMeshProUGUI cgCustomFogDisableDesc = GetTextMeshProUGUI(GetGameObjectChild(cgCustomFogSlider, "Fog Disabled Text"));
            cgCustomFogDisableDesc.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDisableDesc;

            TextMeshProUGUI cgCustomFogStart = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogSliderLayoutGroup, "Start Distance"), "Text"));
            cgCustomFogStart.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogStart;

            TextMeshProUGUI cgCustomFogEnd = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogSliderLayoutGroup, "End Distance"), "Text"));
            cgCustomFogEnd.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogEnd;

            //Patch Buttons in Tabs. "Disable", "Static", "Dynamic"
            TextMeshProUGUI cgCustomFogDisable = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogTabs, "Disabled Button"), "Text"));
            cgCustomFogDisable.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDisable;

            TextMeshProUGUI cgCustomFogStatic = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogTabs, "Static Button"), "Text"));
            cgCustomFogStatic.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogStatic;

            TextMeshProUGUI cgCustomFogDynamic = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogTabs, "Dynamic Button"), "Text"));
            cgCustomFogDynamic.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDynamic;

            //"Set to default" button
            TextMeshProUGUI cgCustomFogDefault = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgCustomFogWindow, "Default Button"), "Text"));
            cgCustomFogDefault.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDefault;

            //Patterns
            GameObject cgTerminalPatterns = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Patterns"), "Patterns Window"), "Panel");

            TextMeshProUGUI cgTerminalPatternsTitle = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalPatterns.transform.parent.gameObject, "Title"));
            cgTerminalPatternsTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsTitle;

            TextMeshProUGUI cgPatternsWarning = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalPatterns,"Warning Text"));
            cgPatternsWarning.text = "<color=red>" + LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsWarning + "</color>";

            bool customPatternMode = MonoSingleton<EndlessGrid>.Instance.customPatternMode;
            TextMeshProUGUI cgPatternsSwitchButton = cgTerminalPatterns.transform.Cast<Transform>().FirstOrDefault(t => t.name == "Enable/Disable Button")?.Find("Text")?.GetComponent<TextMeshProUGUI>();
            cgPatternsSwitchButton.text = customPatternMode
                ? LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsSwitchButtonNot
                : LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsSwitchButton;

            TextMeshProUGUI cgPatternsBack = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Patterns"), "Back Button"), "Text"));
            cgPatternsBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;

            //TextMeshProUGUI cgCustomStateButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalPatterns, "StateButton"), "Text"));
            //bool customPatternMode = MonoSingleton<EndlessGrid>.Instance.customPatternMode;
            //cgCustomStateButton.text = (customPatternMode ? LanguageManager.CurrentLanguage.misc.state_activated : LanguageManager.CurrentLanguage.misc.state_deactivated);
            //it seems broken vanilla rn, so skipping it

            TextMeshProUGUI cgTerminalPatternsEditor = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(cgTerminalPatterns, "Patterns"), "Editor Button"), "Text"));
            cgTerminalPatternsEditor.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsLaunchExternalEditor;

            //Waves
            GameObject cgTerminalWaves = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Waves"), "Waves Window"), "Panel");

            TextMeshProUGUI cgTerminalWavesTitle = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalWaves.transform.parent.gameObject, "Title"));
            cgTerminalWavesTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wavesTitle;

            TextMeshProUGUI cgTerminalWavesText = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalWaves, "Select Wave Text"));
            cgTerminalWavesText.text =
                LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wavesDescription1;
            cgTerminalWavesText.fontSize = 16;
            TextMeshProUGUI cgTerminalWavesReqText = GetTextMeshProUGUI(GetGameObjectChild(cgTerminalWaves, "Wave Requirement Text"));
            cgTerminalWavesReqText.text =
                LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wavesDescription2;

            TextMeshProUGUI cgWavesBack = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Waves"), "Back Button"), "Text"));
            cgWavesBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;
        }

        public async static void PatchTerminalFolder()
        {
            //Changes all folders' own names based on their original name
            GameObject level = GameObject.Find("FirstRoom");
            GameObject cgTerminalMainPanel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(level, "Room"), "Cybergrind Shop"), "Canvas"),"Background"),"Main Panel");
            GameObject cgMusicSoundtrack = GetGameObjectChild(GetGameObjectChild(cgTerminalMainPanel, "Songs Soundtrack"), "Panel");
            GameObject cgMusicSoundtrackAddMenu = GetGameObjectChild(GetGameObjectChild(cgMusicSoundtrack, "Inset"), "Songs");
            await Task.Delay(5);
            foreach (Transform child in cgMusicSoundtrackAddMenu.transform)
            {
                if (child.name == "Folder Template(Clone)")
                {
                    Button a = child.GetComponent<Button>();
                    a.onClick.AddListener(delegate { PatchTerminalFolder(); });
                    TextMeshProUGUI cgMusicSoundtrackFolderTitle = GetTextMeshProUGUI(GetGameObjectChild(child.gameObject, "Title"));
                    switch (cgMusicSoundtrackFolderTitle.text.ToUpper())
                    {
                        case "THE CYBER GRIND": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameCyberGrind; break; }
                        case "PRELUDE": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNamePrelude; break; }
                        case "ACT 1": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameAct1; break; }
                        case "ACT 2": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameAct2; break; }
                        case "ACT 3": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameAct3; break; }
                        case "SECRET LEVELS": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameSecret; break; }
                        case "PRIME SANCTUMS": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNamePrime; break; }
                        case "MISCELLANEOUS TRACKS": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameMisc; break; }
                        case "ENCORES": { cgMusicSoundtrackFolderTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicFolderNameEncores; break; }

                        default: {Logging.Warn("Missing CG music folder name: " + cgMusicSoundtrackFolderTitle.text); break; }
                    }
                }
                if (child.name == "Song Template(Clone)")
                {
                    TextMeshProUGUI cgMusicSoundtrackTask = GetTextMeshProUGUI(GetGameObjectChild(child.gameObject, "Requirement"));
                    if (cgMusicSoundtrackTask.text == "Unlocked") { cgMusicSoundtrackTask.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicUnlocked; }
                }
            }
            return;
        }
        public static void PatchCg()
        {
            try { PatchWaveBoard(); }catch (Exception e) { Console.WriteLine("Failed to patch CG wave board"); Console.WriteLine(e.ToString());}
            try { PatchResults(); }catch (Exception e) { Console.WriteLine("Failed to patch CG results"); Console.WriteLine(e.ToString());}
            try { PatchTerminal(); }catch (Exception e) { Console.WriteLine("Failed to patch CG terminal"); Console.WriteLine(e.ToString());}
            try { PatchTerminalFolder(); }catch (Exception e) { Console.WriteLine("Failed to patch CG terminal folders"); Console.WriteLine(e.ToString());}
            
        }
    }
}
