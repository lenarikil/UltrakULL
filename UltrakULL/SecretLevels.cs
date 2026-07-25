using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    class SecretLevels
    {
        private string currentLevel;
        public static string GetAbbreviation(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string[] words = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder abbreviation = new StringBuilder();

            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word) && word.Length > 0)
                {
                    abbreviation.Append(char.ToUpper(word[0]));
                }
            }

            return abbreviation.ToString();
        }

        private void PatchTestament(ref GameObject testamentRoom)
        {
            TextMeshProUGUI testamentPanelText = null;
            TextMeshProUGUI testamentPanelText4S1 = null;
            TextMeshProUGUI testamentPanelText4S2 = null;
            //TextMeshProUGUI testamentPanelTitle = null;

            //0-S
            if (GetCurrentSceneName() == "Level 0-S")
            {

                //testamentPanelTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(finalRoom, "Room"), "Testament Shop"), "Canvas"), "Border"), "TipBox"), "Panel"), "Title"));

                testamentPanelText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(testamentRoom, "Room"), "Testament Shop (1)"), "Canvas"), "Text (TMP)"));

                //testamentPanelTitle.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testamentTitle;
            }
            //1-S
            else if (GetCurrentSceneName() == "Level 1-S")
            {
                GameObject finalRoom = GetGameObjectChild(testamentRoom, "FinalRoomSecretExit");
                testamentPanelText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(finalRoom, "Room"), "Testament Shop (1)"), "Canvas"), "Text (TMP)"));
            }
            //4-S
            else if (GetCurrentSceneName() == "Level 4-S")
            {
                //testamentPanelTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(testamentRoom, "4 Stuff"), "FinalRoom 2"), "Room"), "Testament Shop"), "Canvas"), "Border"), "TipBox"), "Panel"), "Title"));
                Transform[] allChildren = testamentRoom.GetComponentsInChildren<Transform>(true);
                List<GameObject> stuff = new List<GameObject>();
                int errorCount = 0;
                foreach (Transform child in allChildren)
                {
                    Logging.Message($"Current object -- {child.gameObject.name}");
                    if (child.name.Contains("4 Stuff"))
                    {
                        stuff.Add(child.gameObject);
                        Logging.Warn($"Add {child.gameObject.name} to Array");
                    }
                }
                foreach (GameObject stuffObject in stuff)
                {
                    if ((testamentPanelText4S1 == null) & (errorCount == 0))
                    {
                        try
                        {
                            testamentPanelText4S1 = GetTextMeshProUGUI(GetGameObjectChild(
                                GetGameObjectChild(
                                    GetGameObjectChild(
                                        GetGameObjectChild(
                                            GetGameObjectChild(stuffObject, "FinalRoom SecretExit"),
                                            "Room"),
                                        "Testament Shop (1)"),
                                    "Canvas"),
                                "Text (TMP)"));
                        }
                        catch (Exception ex)
                        {
                            Logging.Warn("An error occurred during the search for the first object");
                            errorCount++;
                        }
                    }
                    else if ((testamentPanelText4S2 == null) & (errorCount < 2))
                    {
                        try
                        {
                            testamentPanelText4S2 = GetTextMeshProUGUI(GetGameObjectChild(
                                GetGameObjectChild(
                                    GetGameObjectChild(
                                        GetGameObjectChild(
                                            GetGameObjectChild(stuffObject, "FinalRoom SecretExit"),
                                            "Room"),
                                        "Testament Shop (1)"),
                                    "Canvas"),
                                "Text (TMP)"));
                        }
                        catch (Exception ex)
                        {
                            Logging.Warn("An error occurred while searching for the second object");
                            errorCount++;
                        }
                    }

                    if (errorCount >= 2)
                    {
                        Logging.Error("The number of attempts to find the Text (TMP) object has been exhausted");
                    }
                }
            }
            //5-S   
            else if (GetCurrentSceneName() == "Level 5-S")
            {
                //testamentPanelTitle = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(finalRoom, "Room"), "Testament Shop"), "Canvas"), "Border"), "TipBox"), "Panel"), "Title"));

                testamentPanelText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(testamentRoom, "Room"), "Testament Shop (1)"), "Canvas"), "Text (TMP)"));

            }
            else if (GetCurrentSceneName() == "Level 7-S")
            {
                testamentPanelText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(testamentRoom, "Room"), "Testament Shop (1)"), "Canvas"), "Text (TMP)"));

            }

            switch (this.currentLevel)
            {
                case "Level 0-S":
                    {
                        testamentPanelText.text =
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testamentTitle
                            + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testament1
                            + "\n\n\n" + 
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testament2
                            + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testament3
                            + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testament4;

                        //testamentPanelTitle.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_prelude_testamentTitle;

                        break;
                    }
                case "Level 1-S":
                    {
                        testamentPanelText.text =
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_first_testamentTitle
                            + "\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_first_testament1
                            + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_first_testament2
                            + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_first_testament3
                            + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_first_testament4;

                       // testamentPanelTitle.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_first_testamentTitle;
                        break;
                    }

                case "Level 4-S":
                    {
                        if (!(testamentPanelText4S1 == null))
                        {
                            testamentPanelText4S1.text =
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testamentTitle + "\n\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament1 + "\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament2 + "\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament3 + "\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament4 + "\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament5 + "\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament6 + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament7;

                            //testamentPanelTitle.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testamentTitle;
                        }
                        if (!(testamentPanelText4S2 == null))
                        {
                            testamentPanelText4S2.text =
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testamentTitle + "\n\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament1 + "\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament2 + "\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament3 + "\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament4 + "\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament5 + "\n\n" +

                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament6 + "\n\n\n" +
                            LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testament7;

                            //testamentPanelTitle.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_fourth_testamentTitle;
                        }
                        break;
                    }
                case "Level 5-S":
                    {
                        testamentPanelText.text =
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testamentTitle + "\n\n" + 
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament1 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament2 + "\n\n" + 

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament3 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament4 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament5 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament6 + "\n\n" + 

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament7 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament8 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament9 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament10 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament11 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testament12;

                        //testamentPanelTitle.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_fifth_testamentTitle;

                        break;
                    }
                case "Level 7-S":
                    {
                        testamentPanelText.text =
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testamentTitle + "\n\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament1 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament2 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament3 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament4 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament5 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament6 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament7 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament8 + "\n\n" +

                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament9 + "\n" +
                                LanguageManager.CurrentLanguage.secretLevels.secretLevels_seventh_testament10;
                        
                        break;
                    }
            }
        }

        public void Patch5S(ref GameObject canvasObj)
        {
            GameObject powerGauge = GetGameObjectChild(GetInactiveRootObject("FishingCanvas"), "Power Meter");
            TextMeshProUGUI distanceFar = GetTextMeshProUGUI(GetGameObjectChild(powerGauge, "Text (TMP)")); 
            distanceFar.text = LanguageManager.CurrentLanguage.fishing.fish_rodFar;
            TextMeshProUGUI distanceClose = GetTextMeshProUGUI(GetGameObjectChild(powerGauge, "Text (TMP) (1)"));
            distanceClose.text = LanguageManager.CurrentLanguage.fishing.fish_rodClose;

            //Localize buttons in Balancing Minigame
            GameObject balancingMinigame = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("FishingCanvas"), "Struggle Mini Game"), "Balancing Minigame");
            TextMeshProUGUI RMB = GetTextMeshProUGUI(GetGameObjectChild(balancingMinigame, "Text (TMP)"));
            RMB.text = GetAbbreviation(LanguageManager.CurrentLanguage.inputStrings.input_RMB);
            TextMeshProUGUI LMB = GetTextMeshProUGUI(GetGameObjectChild(balancingMinigame, "Text (TMP) (1)"));
            LMB.text = GetAbbreviation(LanguageManager.CurrentLanguage.inputStrings.input_LMB);

            GameObject fishingLeaderboard = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Exit Lobby Interior"), "Fish Scores"), "Canvas"), "Border"), "TipBox"), "Panel");

            TextMeshProUGUI fishingLeaderboardTitle = GetTextMeshProUGUI(GetGameObjectChild(fishingLeaderboard, "Title"));
            fishingLeaderboardTitle.text = LanguageManager.CurrentLanguage.fishing.fish_leaderboard;

            //Trying change Fishing Leaderboard "Global" and "Friend" title. Not work now, need create Harmony Patch for original Fishing Leaderboad code
            /*TextMeshProUGUI fishingLeaderboardGlobal = GetTextMeshProUGUI(GetGameObjectChild(fishingLeaderboard, "Global Text"));
            TextMeshProUGUI fishingLeaderboardFriends = GetTextMeshProUGUI(GetGameObjectChild(fishingLeaderboard, "Friends Text"));
            var rx = new Regex(@"\bGLOBAL\b", RegexOptions.IgnoreCase);
            var rw = new Regex(@"\bFRIENDS\b", RegexOptions.IgnoreCase);
            fishingLeaderboardGlobal.text = rx.Replace(fishingLeaderboardGlobal.text, "GLOBAL TEST", 1);
            fishingLeaderboardFriends.text = rw.Replace(fishingLeaderboardFriends.text, "FRIENDS TEST", 1);*/

            GameObject fishingTerminal = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Fishing Enc Terminal"), "Canvas"), "Background"), "Main Window");
            TextMeshProUGUI fishingTerminalTitle = GetTextMeshProUGUI(GetGameObjectChild(fishingTerminal, "Title"));
            fishingTerminalTitle.text = LanguageManager.CurrentLanguage.fishing.fish_terminalTitle;
            GameObject fishingTerminalBackButton = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(fishingTerminal, "Fish Info"), "Window"), "Back Button");
            TextMeshProUGUI fishingTerminalBackButtonText = GetTextMeshProUGUI(GetGameObjectChild(fishingTerminalBackButton, "Text"));
            fishingTerminalBackButtonText.text = LanguageManager.CurrentLanguage.shop.shop_back;
        }
        public void Patch7S(ref GameObject canvasObj)
        {
            try
            {   
                //BloodCleanText
                GameObject washcanvas = GameObject.Find("WashingCanvas");
                TextMeshProUGUI BloodCleanText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(washcanvas, "Painter Completion Meter"), "Slider Group"), "Blood Cleaned"));
                BloodCleanText.text = LanguageManager.CurrentLanguage.washing.wash_bloodClean;
                GameObject chklst = GetGameObjectChild(washcanvas, "CheckList");

                TextMeshProUGUI LitterCount = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(chklst, "Litter"), "Litter Count:"));
                LitterCount.text = LanguageManager.CurrentLanguage.washing.wash_littercount;

                //Faxeexittext
                GameObject fakeexitCanvas = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Fake Exit"), "PuzzleScreen"), "Canvas"); 
                TextMeshProUGUI fakeexittext = GetTextMeshProUGUI(GetGameObjectChild(fakeexitCanvas, "Cleaning Prompt Text"));
                fakeexittext.text = "<size=12><color=#7f0000><u><b>" + LanguageManager.CurrentLanguage.washing.wash_fakeexittext1 + "</u></b></color></size>\n\n"
                + LanguageManager.CurrentLanguage.washing.wash_fakeexittext2 + "\n"
                + LanguageManager.CurrentLanguage.washing.wash_fakeexittext3 + "\n"
                + LanguageManager.CurrentLanguage.washing.wash_fakeexittext4 + "\n"
                + LanguageManager.CurrentLanguage.washing.wash_fakeexittext5 + "\n"
                + LanguageManager.CurrentLanguage.washing.wash_fakeexittext6;

                TextMeshProUGUI thxtext = GetTextMeshProUGUI(GetGameObjectChild(fakeexitCanvas, "Thank You Text"));
                thxtext.text = "<size=12><color=#7f0000><u><b>" + LanguageManager.CurrentLanguage.washing.wash_exitOpenText1 + "</u></b></color></size>\n\n"
                + LanguageManager.CurrentLanguage.washing.wash_exitOpenText2 + "\n\n"
                + LanguageManager.CurrentLanguage.washing.wash_exitOpenText3;

            }
            catch (Exception e) 
            {
                Logging.Warn("Failed to Patch 7-S(SecretLevels.cs)");
                if (LanguageManager.CurrentLanguage.washing == null)
                { Logging.Warn("Category is missing from the language file! Please Update the language file!"); return; }
                Logging.Warn(e.ToString());
            }
        }
        // SecretFirstRoom/Player/Main Camera/HUD Camera/HUD/FinishCanvas/Panel/Title/Text
        // Note - it uses a separate panel that has the same name as the normal result panel.
        public SecretLevels(ref GameObject canvasObj)
        {
            GameObject player = GetInactiveRootObject("Player");
            this.currentLevel = GetCurrentSceneName();
            GameObject testamentRoom;

            switch (this.currentLevel)
            {
                case "Level 0-S": {testamentRoom = GameObject.Find("FinalRoom SecretExit"); PatchTestament(ref testamentRoom); break; }
                case "Level 1-S": {testamentRoom = GameObject.Find("5 - Finale"); PatchTestament(ref testamentRoom); break; }
                case "Level 2-S": {Act1Vn.PatchPrompts(ref canvasObj); break; }
                case "Level 4-S": {testamentRoom = GetInactiveRootObject("4 - Boulder Run");PatchTestament(ref testamentRoom); break; }
                case "Level 5-S": {testamentRoom = GetInactiveRootObject("FinalRoom SecretExit");PatchTestament(ref testamentRoom); Patch5S(ref canvasObj); break; }
                case "Level 7-S": { testamentRoom = GetInactiveRootObject("FinalRoom SecretExit");PatchTestament(ref testamentRoom); Patch7S(ref canvasObj); break; }  
            }
            Logging.Info("gugi");
            GameObject secretLevelResults = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(player, "Main Camera"), "HUD Camera"), "HUD"), "FinishCanvas");

            Logging.Info("guga");
            GameObject secretLevelResultsPanel = secretLevelResults.transform.GetChild(2).gameObject;

            Logging.Info("gooo");
            TextMeshProUGUI secretLevelResultsName = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(secretLevelResultsPanel, "Title"), "Text"));
            secretLevelResultsName.text = GetSecretLevelName();

            TextMeshProUGUI secretLevelResultsInfo = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(secretLevelResultsPanel, "Time - Info"), "Text"));
            secretLevelResultsInfo.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_complete1;

            TextMeshProUGUI secretLevelComplete = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(secretLevelResultsPanel, "Time - Rank"), "Text"));
            secretLevelComplete.text = LanguageManager.CurrentLanguage.secretLevels.secretLevels_complete2;

        }

        public string GetSecretLevelName()
        {
            switch(this.currentLevel)
            {
                case ("Level 0-S"): { return "0-S: " + LanguageManager.CurrentLanguage.levelNames.levelName_preludeSecret; }
                case ("Level 1-S"): { return "1-S: " + LanguageManager.CurrentLanguage.levelNames.levelName_limboSecret; }
                case ("Level 2-S"): { return "2-S: " + LanguageManager.CurrentLanguage.levelNames.levelName_lustSecret; }
                case ("Level 4-S"): { return "4-S: " + LanguageManager.CurrentLanguage.levelNames.levelName_greedSecret;}
                case ("Level 5-S"): { return "5-S: " + LanguageManager.CurrentLanguage.levelNames.levelName_wrathSecret;}
                case ("Level 7-S"): { return "7-S: " + LanguageManager.CurrentLanguage.levelNames.levelName_violenceSecret; }
                default: { return "UNKNOWN"; }
            }
        }

    }
}
