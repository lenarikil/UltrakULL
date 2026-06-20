using UltrakULL.json;
using static UltrakULL.CommonFunctions;
using static UnityEngine.InputSystem.InputRemoting;

namespace UltrakULL
{
    public static class Act2Strings
    {
        private static string _previousMessage = "";

        private static string Level41(string message, string message2)
        {
            string fullMessage = message + message2;
            if (fullMessage.Contains("An eye opens."))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greed_secretDoor);
            }
            return ("Unimplemented string");
        }
        
        private static string Level42(string message, string message2)
        {
            string fullMessage = message + message2;
            if (fullMessage.Contains("BLEED"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedSecond_sand);
            }
            if (fullMessage.Contains("A door opens."))
            {
                return LanguageManager.CurrentLanguage.act3.act3_violenceFirst_doorOpens;
            }
            return ("Unimplemented string");
        }
        
        private static string Level43(string message, string message2)
        {
            string fullMessage = message + message2;
            if (fullMessage.Contains("FILTH"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedThird_wallClip);
            }
            if (fullMessage.Contains("wicked"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedThird_troll1);
            }
            if (fullMessage.Contains("kidding"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedThird_troll2);
            }
            if (fullMessage.Contains("TOMB"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedThird_tombOfKings);
            }
            return ("Unimplemented string");
        }

        private static string Level44(string message, string message2, string input)
        {
            string fullMessage = message + message2;

            //Bandaid fix for some edge cases people have been reporting.
            if (fullMessage == "")
            {
                fullMessage = _previousMessage;
                return fullMessage;
            }

            if (message.Contains("versions"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_alternateVersion;
            }

            if (fullMessage.Contains("ALTERNATE NAILGUN"))
            {
                return "<color=orange>" + LanguageManager.CurrentLanguage.act2.act2_greedFourth_alternateNailgun;
            }

            if (fullMessage.Contains("You're"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedFourth_v2);
            }
            if (fullMessage.Contains("Hold"))
            {
                _previousMessage = "<color=green>" + LanguageManager.CurrentLanguage.act2.act2_greedFourth_whiplash1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.act2.act2_greedFourth_whiplash2;
                return LanguageManager.CurrentLanguage.act2.act2_greedFourth_whiplash1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.act2.act2_greedFourth_whiplash2;
            }
            if (fullMessage.Contains("HEAVY"))
            {
                _previousMessage = "<color=green>" + LanguageManager.CurrentLanguage.act2.act2_greedFourth_whiplash3;
                return LanguageManager.CurrentLanguage.act2.act2_greedFourth_whiplash3;
            }
           
            return ("Unimplemented string");
        }

        private static string Level4Secret(string message, string message2, string input)
        {
            string fullMessage = message + message2;
            if (fullMessage.Contains("HOLD"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_greedSecret_holdToJump1 + "<color=orange>" + input + "</color>" + LanguageManager.CurrentLanguage.act2.act2_greedSecret_holdToJump2);
            }

            return ("Unimplemented string");
        }

        private static string Level51(string message, string message2)
        {
            string fullMessage = message + message2;
            if (fullMessage.Contains("HOOKPOINT"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_wrathFirst_slingshot);
            }
            if (fullMessage.Contains("SENTRIES"))
            {
                return ("<color=green>" + LanguageManager.CurrentLanguage.act2.act2_wrathFirst_sentry);
            }
            if (fullMessage.Contains("drained"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_wrathFirst_waterDrained);
            }
            if (fullMessage.Contains("REDUCE")) 
            {
                return "<color=green>" + LanguageManager.CurrentLanguage.act2.act2_wrathFirst_whiplashHardDamage1 + "\n" // Renamed to act2_wrathFirst_whiplashHardDamage1 because message moved from 4-4 to 5-1
                + LanguageManager.CurrentLanguage.act2.act2_wrathFirst_whiplashHardDamage2; // Renamed to act2_wrathFirst_whiplashHardDamage2 because message moved from 4-4 to 5-1
            }
            if (fullMessage.Contains("UNDERWATER"))
            {
                return ("<color=green>" + LanguageManager.CurrentLanguage.act2.act2_wrathFirst_whiplashUnderwater);
            }
            if (fullMessage.Contains("A door opens."))
            {
                return LanguageManager.CurrentLanguage.act3.act3_violenceFirst_doorOpens;
            }
            return ("Unimplemented string");
        }

        private static string Level52(string message, string message2)
        {
            string fullMessage = message + message2;
            if (fullMessage.Contains("JAKITO"))
            {
                return "<color=red>" + LanguageManager.CurrentLanguage.act2.act2_wrathSecond_jakito1 + "</color>";
            }
            if (fullMessage.Contains("THANK"))
            {
                return "<color=red>" + LanguageManager.CurrentLanguage.act2.act2_wrathSecond_jakito2 + "</color>";
            }
            if (fullMessage.Contains("NO"))
            {
                return "<color=red>" + LanguageManager.CurrentLanguage.act2.act2_wrathSecond_jakito3 + "</color>";
            }
            if (fullMessage.Contains("Hark"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_wrathSecond_neptune);
            }
            if (fullMessage.Contains("IDOL"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_wrathSecond_idol);
            }
            return ("Unimplemented string");
        }

        private static string Level53(string message, string message2)
        {
            string fullMessage = message + message2;

            if (fullMessage.Contains("Indirect"))
            {
                return ("<color=#40E7FF>" + LanguageManager.CurrentLanguage.act2.act2_wrathThird_rocketLauncher);
            }
            if (fullMessage.Contains("FALLING"))
            {
                return ("<color=#40E7FF>" + LanguageManager.CurrentLanguage.act2.act2_wrathThird_rocketLauncherMidair);
            }
            if (fullMessage.Contains("Soldiers"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_wrathThird_soldierBlock);
            }
            if (fullMessage.Contains("Hank"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_wrathThird_hank);
            }

            return ("Unimplemented string");
        }

        private static string Level54(string message, string message2)
        {
            string fullMessage = message + message2;
            return ("Unimplemented string");
        }

        private static string Level5Secret(string message)
        {
            if (message.Contains("living"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_living);
            }
            if (message.Contains("Too small"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_tooSmall);
            }
            if (message.Contains("This bait"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_baitNotWork);
            }
            if (message.Contains("A fish took"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_baitTaken);
            }
            if (message.Contains("Fishing interrupted"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_interrupted);
            }
            if (message.Contains("Cooking failed"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_cookingFailed);
            }
            if (message.Contains("Nothing seems"))
            {
                return (LanguageManager.CurrentLanguage.fishing.fish_noFishBiting);
            }


            return ("Unimplemented string");
        }

        private static string Level61(string message, string message2)
        {
            string fullMessage = message + message2;

            if (fullMessage.Contains("A R M B O Y"))
            {
                return (LanguageManager.CurrentLanguage.act2.act2_heresyFirst_armboy);
            }
            return ("Unimplemented string");
        }

        private static string Level62(string message, string message2)
        {
            string fullMessage = message + message2;
            return ("Unimplemented string");
        }

        public static string GetMessage(string message, string message2, string input)
        {
            string currentLevel = GetCurrentSceneName();
            string fullMessage = message + message2;

            switch (currentLevel)
            {
                case "Level 4-1":
                    {
                        return Level41(message, message2);
                    }
                case "Level 4-2":
                    {
                        return Level42(message, message2);
                    }
                case "Level 4-3":
                    {
                        return Level43(message, message2);
                    }
                case "Level 4-4":
                    {
                        return Level44(message, message2, input);
                    }
                case "Level 4-S":
                    {
                        return Level4Secret(message, message2, input);
                    }
                case "Level 5-1":
                    {
                        return Level51(message, message2);
                    }
                case "Level 5-2":
                    {
                        return Level52(message, message2);
                    }
                case "Level 5-3":
                    {
                        return Level53(message, message2);
                    }
                case "Level 5-4":
                    {
                        return Level54(message, message2);
                    }
                case "Level 5-S":
                    {
                        return Level5Secret(message);
                    }
                case "Level 6-1":
                    {
                        return Level61(message, message2);
                    }
                case "Level 6-2":
                    {
                        return Level62(message, message2);
                    }
                default:
                    {
                        //Console.WriteLine("Unknown ACT 2 string in " + currentLevel + ": \n" + message + message2);
                        return ("Unimplemented string");
                    }
            }
        }

        public static string GetLevelChallenge(string currentLevel)
        {
            switch (currentLevel)
            {
                case "Level 4-1": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_greedFirst; }
                case "Level 4-2": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_greedSecond; }
                case "Level 4-3": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_greedThird; }
                case "Level 4-4": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_greedFourth; }

                case "Level 5-1": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_wrathFirst; }
                case "Level 5-2": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_wrathSecond; }
                case "Level 5-3": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_wrathThird; }
                case "Level 5-4": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_wrathFourth; }

                case "Level 6-1": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_heresyFirst; }
                case "Level 6-2": { return LanguageManager.CurrentLanguage.levelChallenges.challenges_heresySecond; }

                default: { return "Unknown challenge description"; }
            }
        }

        public static string GetLevelName()
        {
            string currentLevel = GetCurrentSceneName();

            switch (currentLevel)
            {
                case "Level 4-1": { return "4-1: " + LanguageManager.CurrentLanguage.levelNames.levelName_greedFirst; }
                case "Level 4-2": { return "4-2: " + LanguageManager.CurrentLanguage.levelNames.levelName_greedSecond; }
                case "Level 4-3": { return "4-3: " + LanguageManager.CurrentLanguage.levelNames.levelName_greedThird; }
                case "Level 4-4": { return "4-4: " + LanguageManager.CurrentLanguage.levelNames.levelName_greedFourth; }

                case "Level 5-1": { return "5-1: " + LanguageManager.CurrentLanguage.levelNames.levelName_wrathFirst; }
                case "Level 5-2": { return "5-2: " + LanguageManager.CurrentLanguage.levelNames.levelName_wrathSecond; }
                case "Level 5-3": { return "5-3: " + LanguageManager.CurrentLanguage.levelNames.levelName_wrathThird; }
                case "Level 5-4": { return "5-4: " + LanguageManager.CurrentLanguage.levelNames.levelName_wrathFourth; }

                case "Level 6-1": { return "6-1: " + LanguageManager.CurrentLanguage.levelNames.levelName_heresyFirst; }
                case "Level 6-2": { return "6-2: " + LanguageManager.CurrentLanguage.levelNames.levelName_heresySecond; }
                default: { return "Unknown level name"; }
            }
        }
    }
}
