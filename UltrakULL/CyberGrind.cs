using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

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
            GameObject cubeCanvas = everythingList[4].Child("Canvas");
            foreach (Transform child in cubeCanvas.transform)
            {
                cubeCanvasList.Add(child.gameObject);
            }
            GameObject cgBoard = cubeCanvasList[1];

            //Patch all the strings here.
            Text waveText = cgBoard.Child("Wave Title").Text();
            waveText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wave;

            Text enemiesLeftText = cgBoard.Child("Enemies Left Title").Text();
            enemiesLeftText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_enemiesRemaining;

        }

        private static void PatchResults()
        {
            GameObject level = GameObject.Find("Player");

            GameObject resultsPanel = level.FindPath("FinishCanvas (1)", "Panel");
            GameObject lastResult = resultsPanel.Child("Panel");
            GameObject bestResult = resultsPanel.FindPath("Panel (1)", "Filler");
            GameObject pointsPanel = resultsPanel.Child("Total Points");
            GameObject leaderboardsPanel = resultsPanel.Child("Cyber Grind Leaderboards");

            //Both result panels use the same strings, so declare them here to avoid redundancy.
            string wave = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wave;
            string kills = LanguageManager.CurrentLanguage.misc.levelstats_kills;
            string style = LanguageManager.CurrentLanguage.misc.levelstats_style;
            string time = LanguageManager.CurrentLanguage.misc.levelstats_time;


            //Title
            TextMeshProUGUI titleText= resultsPanel.FindPath("Title", "Text").TMP();
            titleText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_cgTitle;

            //Last result panel
            TextMeshProUGUI lastTitle = lastResult.Child("Text").TMP();
            lastTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_previousRun;

            TextMeshProUGUI lastWave = lastResult.FindPath("Wave - Info", "Text").TMP();
            lastWave.text = wave;

            TextMeshProUGUI lastKills = lastResult.FindPath("Kills - Info", "Text").TMP();
            lastKills.text = kills;

            TextMeshProUGUI lastStyle = lastResult.FindPath("Style - Info", "Text").TMP();
            lastStyle.text = style;

            TextMeshProUGUI lastTime = lastResult.FindPath("Time - Info", "Text").TMP();
            lastTime.text = time;

            //Best result panel
            TextMeshProUGUI bestTitle = bestResult.Child("Text (1)").TMP();
            bestTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_bestRun;

            TextMeshProUGUI bestWave = bestResult.FindPath("Wave - Info (1)", "Text").TMP();
            bestWave.text = wave;

            TextMeshProUGUI bestKills = bestResult.FindPath("Kills - Info (1)", "Text").TMP();
            bestKills.text = kills;

            TextMeshProUGUI bestStyle = bestResult.FindPath("Style - Info (1)", "Text").TMP();
            bestStyle.text = style;

            TextMeshProUGUI bestTime = bestResult.FindPath("Time - Info (1)", "Text").TMP();
            bestTime.text = time;

            //Points panel
            TextMeshProUGUI totalPointsText = pointsPanel.Child("Text (1)").TMP();
            totalPointsText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_total;

            TextMeshProUGUI totalPoints = pointsPanel.Child("Text").TMP();
            totalPoints.text = "+0" + "<color=orange>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>";

            //Leaderboards

            string connecting = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_connectingToSteam;

            GameObject friendScores = leaderboardsPanel.Child("Friend High Scores");
            GameObject globalScores = leaderboardsPanel.Child("Global High Scores");

            TextMeshProUGUI friendScoresTitle = friendScores.Child("Text").TMP();
            friendScoresTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_friendScores;

            TextMeshProUGUI globalScoresTitle = globalScores.Child("Text").TMP();
            globalScoresTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_globalScores;

            TextMeshProUGUI friendsConnectingText = friendScores.Child("Connecting").TMP();
            friendsConnectingText.text = connecting;

            TextMeshProUGUI globalConnectingText = globalScores.Child("Connecting").TMP();
            globalConnectingText.text = connecting;


        }

        private static void PatchTerminal()
        {
            GameObject level = GameObject.Find("FirstRoom");
            GameObject cgTerminal = level.FindPath("Room", "Cybergrind Shop", "Canvas");

            GameObject cgTerminalMainPanel = cgTerminal.FindPath("Background", "Main Panel");

            //Terminal description(I just ripped off from shop.cs lol)
            GameObject tipPanel = cgTerminalMainPanel.Child("Stats");
            TextMeshProUGUI cgTerminalTipboxTitle = tipPanel.Child("Title").TMP();
            cgTerminalTipboxTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_settings;

            TextMeshProUGUI cgTerminalTipboxDescription = tipPanel.FindPath("Panel", "Text Inset", "Text").TMP();
            cgTerminalTipboxDescription.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_settingsDescription;

            //Main menu
            GameObject mainButtons = cgTerminalMainPanel.FindPath("Main Menu", "Buttons");

            TextMeshProUGUI cgTerminalThemesText = mainButtons.FindPath("Themes Button", "Text").TMP();
            cgTerminalThemesText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themes;

            TextMeshProUGUI cgTerminalMusicText = mainButtons.FindPath("Music Button", "Text").TMP();
            cgTerminalMusicText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_music;

            TextMeshProUGUI cgTerminalPatternsText = mainButtons.FindPath("Patterns Button", "Text").TMP();
            cgTerminalPatternsText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patterns;

            TextMeshProUGUI cgTerminalWaveText = mainButtons.FindPath("Waves Button", "Text").TMP();
            cgTerminalWaveText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_waves;

            //Themes
            GameObject cgTerminalThemes = cgTerminalMainPanel.FindPath("Themes", "Preset Panel");

            TextMeshProUGUI cgTerminalThemesTitle = cgTerminalThemes.Child("Title").TMP();
            cgTerminalThemesTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesTitle;

            GameObject cgTerminalThemesPanel = cgTerminalThemes.Child("Panel");

            TextMeshProUGUI cgTerminalThemesDescription = cgTerminalThemesPanel.Child("Text").TMP();
            cgTerminalThemesDescription.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesDescription;
            
            GameObject cgTerminalThemesButton = cgTerminalThemesPanel.Child("Buttons");

            TextMeshProUGUI cgTerminalThemesLight = cgTerminalThemesButton.FindPath("Light Button", "Text").TMP();
            cgTerminalThemesLight.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesLight;

            TextMeshProUGUI cgTerminalThemesDark = cgTerminalThemesButton.FindPath("Dark Button", "Text").TMP();
            cgTerminalThemesDark.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesDark;

            TextMeshProUGUI cgTerminalThemesCustom = cgTerminalThemesButton.FindPath("Custom Button", "Text").TMP();
            cgTerminalThemesCustom.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustom;

            TextMeshProUGUI cgTerminalThemesBack = cgTerminalThemes.transform.parent.gameObject.FindPath("Back Button", "Text").TMP();
            cgTerminalThemesBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;
			//Да почему оно сбрасывает всю ветку themes, я же всё правильно по айди ввёл и сравнил с предыдущими, всё идентично. Ещё я хз как сделать перехват текста, чтобы он заменял отображаемые имена папок на свой текст, ибо они тот же TMP используют для отображения, но у них у всех одинаковый подраздел Folder(Clone), и я также хз как их распределить по содержанию строки как в книгах или сообщениях, а не по подразделу, т.к. во всех случаях они весь раздел сбрасывают
			//TextMeshProUGUI cgTerminalThemesDefault = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalThemesButton, "Default"), "Text"));
			//cgTerminalThemesDefault.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesDefault;

            //Playlist
            GameObject cgMusic = cgTerminalMainPanel.FindPath("Playlist", "Panel");
            
            TextMeshProUGUI cgMusicTitle = cgMusic.transform.parent.gameObject.Child("Title").TMP();
            cgMusicTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicTitle;
            
            TextMeshProUGUI cgMusicClose = cgMusic.FindPath("Close Button", "Text").TMP();
            cgMusicClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            //Songs Type Selection(+ button in playlist will show this up)
            GameObject cgMusicTypeCanvas = cgTerminalMainPanel.FindPath("Songs Type Selection", "Panel");

            TextMeshProUGUI cgMusicTypeTitle = cgMusicTypeCanvas.transform.parent.gameObject.Child("Title").TMP();
            cgMusicTypeTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicType;

            GameObject cgMusicTypeButtons = cgMusicTypeCanvas.FindPath("Inset", "Type Selection Buttons");
            TextMeshProUGUI cgMusicTypeULTRAKILL = cgMusicTypeButtons.FindPath("Soundtrack Button", "Text").TMP();
            cgMusicTypeULTRAKILL.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicSoundtrack;

            TextMeshProUGUI cgMusicTypeCustom = cgMusicTypeButtons.FindPath("Custom Button", "Text").TMP();
            cgMusicTypeCustom.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustom;

            TextMeshProUGUI cgMusicTypeClose = cgMusicTypeCanvas.FindPath("Close Button", "Text").TMP();
            cgMusicTypeClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            GameObject cgMusicSoundtrack = cgTerminalMainPanel.FindPath("Songs Soundtrack", "Panel");
            TextMeshProUGUI cgMusicSoundtrackTitle = cgMusicSoundtrack.transform.parent.gameObject.Child("Title").TMP();
            cgMusicSoundtrackTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicSoundtrack;

            TextMeshProUGUI cgMusicSoundtrackClose = cgMusicSoundtrack.FindPath("Close Button", "Text").TMP();
            cgMusicSoundtrackClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            GameObject cgMusicSoundtrackAddMenu = cgMusicSoundtrack.FindPath("Inset", "Songs");

            //CustomMusic
            GameObject cgCustomMusic = cgTerminalMainPanel.FindPath("Songs Custom", "Panel");

            TextMeshProUGUI cgCustomMusicTitle = cgCustomMusic.transform.parent.gameObject.Child("Title").TMP();
            cgCustomMusicTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustom;

            TextMeshProUGUI cgCustomMusicClose = cgCustomMusic.FindPath("Close Button", "Text").TMP();
            cgCustomMusicClose.text = LanguageManager.CurrentLanguage.devMuseum.museum_chessSettingsclose;

            //Changes the "Unlocked" string under songs that are unlocked

            foreach (Transform child in cgMusicSoundtrackAddMenu.transform)
            {
                if (child.name == "Song Template(Clone)")
                {
                    TextMeshProUGUI cgMusicSoundtrackTask = child.gameObject.Child("Requirement").TMP();
                    if (cgMusicSoundtrackTask.text == "Unlocked") { cgMusicSoundtrackTask.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_musicUnlocked; }
                }
            }
            Button[] aas = cgMusicSoundtrack.GetComponentsInChildren<Button>(true);
            foreach (Button button in aas)
            {
                button.onClick.AddListener(delegate { PatchTerminalFolder(); });
            }
            
            
            //Customize theme
            GameObject cgCustomTheme = cgTerminalMainPanel.FindPath("Theme Custom", "Panel");
            TextMeshProUGUI cgCustomThemeTitle = cgCustomTheme.transform.parent.gameObject.Child("Title").TMP();
            //"Custom", replace this later
            cgCustomThemeTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesModify;

            GameObject cgCustomThemeButtons = cgCustomTheme.Child("Sidebar");
            GameObject cgCustomThemeSelectorButtons = cgCustomThemeButtons.Child("Selector Buttons");
            TextMeshProUGUI cgCustomGrid = cgCustomThemeSelectorButtons.FindPath("Grid Button", "Text").TMP();
            cgCustomGrid.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomGrid;

            TextMeshProUGUI cgCustomGridGlow = cgCustomThemeSelectorButtons.FindPath("Glow Button", "Text").TMP();
            cgCustomGridGlow.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomGridGlow;

            TextMeshProUGUI cgCustomSkybox = cgCustomThemeSelectorButtons.FindPath("Skybox Button", "Text").TMP();
            cgCustomSkybox.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomSkybox;

            TextMeshProUGUI cgCustomFog = cgCustomThemeSelectorButtons.FindPath("Fog Button", "Text").TMP();
            cgCustomFog.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFog;

            TextMeshProUGUI cgCustomThemeBack = cgCustomThemeButtons.FindPath("Back Button", "Text").TMP();
            cgCustomThemeBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;

            //Leftside Buttons(Custom Theme)
            GameObject cgCustomAdditionalRows = cgCustomTheme.Child("Window");

            TextMeshProUGUI cgCustomRefresh = cgCustomAdditionalRows.FindPath("Grid Wrapper", "Refresh Button", "Text").TMP();
            cgCustomRefresh.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsRefresh;

            GameObject cgCustomGridTypeSelection = cgCustomAdditionalRows.Child("Grid Type Selection");

            TextMeshProUGUI cgCustomAdditionalBase = cgCustomGridTypeSelection.FindPath("Base Button", "Text").TMP();
            cgCustomAdditionalBase.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBase;

            TextMeshProUGUI cgCustomAdditionalTopRow = cgCustomGridTypeSelection.FindPath("Top Row Button", "Text").TMP();
            cgCustomAdditionalTopRow.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomTopRow;

            TextMeshProUGUI cgCustomAdditionalTop = cgCustomGridTypeSelection.FindPath("Top Button", "Text").TMP();
            cgCustomAdditionalTop.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomTop;

            TextMeshProUGUI cgCustomAdditionalGlowIntensity = cgCustomAdditionalRows.FindPath("Glow Intensity", "Title").TMP();
            cgCustomAdditionalGlowIntensity.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomGlowIntensity;
            //Fog Control goes here, add it later

            GameObject cgCustomFogWindow = cgCustomAdditionalRows.Child("Fog Control");
            GameObject cgCustomFogSlider = cgCustomFogWindow.Child("Sliders");
            GameObject cgCustomFogSliderLayoutGroup = cgCustomFogSlider.Child("Layout Group"); //This is the parent of all sliders
            GameObject cgCustomFogTabs = cgCustomFogWindow.Child("Tabs"); //Now this GameObject contains all buttons to switch fog type. "Disable", "Static", "Dynamic"

            //Patch Color
            TextMeshProUGUI cgCustomFogColor = cgCustomFogWindow.FindPath("Color", "Text").TMP(); //Color moved to Window in 16d Patch
            cgCustomFogColor.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogColor;

            //Patch Sliders Text and "Disable" Description
            TextMeshProUGUI cgCustomFogDisableDesc = cgCustomFogSlider.Child("Fog Disabled Text").TMP();
            cgCustomFogDisableDesc.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDisableDesc;

            TextMeshProUGUI cgCustomFogStart = cgCustomFogSliderLayoutGroup.FindPath("Start Distance", "Text").TMP();
            cgCustomFogStart.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogStart;

            TextMeshProUGUI cgCustomFogEnd = cgCustomFogSliderLayoutGroup.FindPath("End Distance", "Text").TMP();
            cgCustomFogEnd.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogEnd;

            //Patch Buttons in Tabs. "Disable", "Static", "Dynamic"
            TextMeshProUGUI cgCustomFogDisable = cgCustomFogTabs.FindPath("Disabled Button", "Text").TMP();
            cgCustomFogDisable.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDisable;

            TextMeshProUGUI cgCustomFogStatic = cgCustomFogTabs.FindPath("Static Button", "Text").TMP();
            cgCustomFogStatic.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogStatic;

            TextMeshProUGUI cgCustomFogDynamic = cgCustomFogTabs.FindPath("Dynamic Button", "Text").TMP();
            cgCustomFogDynamic.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDynamic;

            //"Set to default" button
            TextMeshProUGUI cgCustomFogDefault = cgCustomFogWindow.FindPath("Default Button", "Text").TMP();
            cgCustomFogDefault.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomFogDefault;

            //Patterns
            GameObject cgTerminalPatterns = cgTerminalMainPanel.FindPath("Patterns", "Patterns Window", "Panel");

            TextMeshProUGUI cgTerminalPatternsTitle = cgTerminalPatterns.transform.parent.gameObject.Child("Title").TMP();
            cgTerminalPatternsTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsTitle;

            TextMeshProUGUI cgPatternsWarning = cgTerminalPatterns.Child("Warning Text").TMP();
            cgPatternsWarning.text = "<color=red>" + LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsWarning + "</color>";

            bool customPatternMode = MonoSingleton<EndlessGrid>.Instance.customPatternMode;
            TextMeshProUGUI cgPatternsSwitchButton = cgTerminalPatterns.transform.Cast<Transform>().FirstOrDefault(t => t.name == "Enable/Disable Button")?.Find("Text")?.GetComponent<TextMeshProUGUI>();
            cgPatternsSwitchButton.text = customPatternMode
                ? LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsSwitchButtonNot
                : LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsSwitchButton;

            TextMeshProUGUI cgPatternsBack = cgTerminalMainPanel.FindPath("Patterns", "Back Button", "Text").TMP();
            cgPatternsBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;

            //TextMeshProUGUI cgCustomStateButton = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(cgTerminalPatterns, "StateButton"), "Text"));
            //bool customPatternMode = MonoSingleton<EndlessGrid>.Instance.customPatternMode;
            //cgCustomStateButton.text = (customPatternMode ? LanguageManager.CurrentLanguage.misc.state_activated : LanguageManager.CurrentLanguage.misc.state_deactivated);
            //it seems broken vanilla rn, so skipping it

            TextMeshProUGUI cgTerminalPatternsEditor = cgTerminalPatterns.FindPath("Patterns", "Editor Button", "Text").TMP();
            cgTerminalPatternsEditor.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_patternsLaunchExternalEditor;

            //Waves
            GameObject cgTerminalWaves = cgTerminalMainPanel.FindPath("Waves", "Waves Window", "Panel");

            TextMeshProUGUI cgTerminalWavesTitle = cgTerminalWaves.transform.parent.gameObject.Child("Title").TMP();
            cgTerminalWavesTitle.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wavesTitle;

            TextMeshProUGUI cgTerminalWavesText = cgTerminalWaves.Child("Select Wave Text").TMP();
            cgTerminalWavesText.text =
                LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wavesDescription1;
            cgTerminalWavesText.fontSize = 16;
            TextMeshProUGUI cgTerminalWavesReqText = cgTerminalWaves.Child("Wave Requirement Text").TMP();
            cgTerminalWavesReqText.text =
                LanguageManager.CurrentLanguage.cyberGrind.cybergrind_wavesDescription2;

            TextMeshProUGUI cgWavesBack = cgTerminalMainPanel.FindPath("Waves", "Back Button", "Text").TMP();
            cgWavesBack.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_themesCustomBack;
        }

        public async static void PatchTerminalFolder()
        {
            //Changes all folders' own names based on their original name
            GameObject level = GameObject.Find("FirstRoom");
            GameObject cgTerminalMainPanel = level.FindPath("Room", "Cybergrind Shop", "Canvas", "Background", "Main Panel");
            GameObject cgMusicSoundtrack = cgTerminalMainPanel.FindPath("Songs Soundtrack", "Panel");
            GameObject cgMusicSoundtrackAddMenu = cgMusicSoundtrack.FindPath("Inset", "Songs");
            await Task.Delay(5);
            foreach (Transform child in cgMusicSoundtrackAddMenu.transform)
            {
                if (child.name == "Folder Template(Clone)")
                {
                    Button a = child.GetComponent<Button>();
                    a.onClick.AddListener(delegate { PatchTerminalFolder(); });
                    TextMeshProUGUI cgMusicSoundtrackFolderTitle = child.gameObject.Child("Title").TMP();
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
                    TextMeshProUGUI cgMusicSoundtrackTask = child.gameObject.Child("Requirement").TMP();
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
