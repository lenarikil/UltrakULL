using System;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    public static class HUDMessages
    {
        public static string GetHUDToolTip(string message)
        {
            //Cross-compatibility with V-Ranks, pretty sure every HUD message has at least one time "V-Rank" in them
            if (message.Contains("V-Rank"))
            {
                return message;
            }

            if (message.Contains("PUNCH"))
            {
                return "<color=red>" + LanguageManager.CurrentLanguage.misc.hud_noArm1 + "</color>\n"
                    + LanguageManager.CurrentLanguage.misc.hud_noArm2;
            }
            if (message.Contains("MAJOR"))
            {
                return "<color=#4C99E6>" + LanguageManager.CurrentLanguage.misc.hud_majorAssists + "</color>";
            }
            if (message.Contains("200"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_overhealOrb1 + "\n"
                    + LanguageManager.CurrentLanguage.misc.hud_overhealOrb2;
            }
            if (message.Contains("ERROR"))
            {
                return "<color=red>" + LanguageManager.CurrentLanguage.misc.hud_itemGrabError + "</color>";
            }
            if (message.Contains("TAB"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_levelStats1 + "\n"
                    + LanguageManager.CurrentLanguage.misc.hud_levelStats2;
            }
            if (message.Contains("Whoops"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_outOfBounds;
            }
            if (message.Contains("CLASH"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_clashMode;
            }
            if (message.Contains("DRONE HAUNTING"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_droneHaunting;
            }
            if (message.Contains("EQUIPPED"))
            {
                return LanguageManager.CurrentLanguage.misc.hud_weaponVariation;
            }
            if (message.Contains("Altered"))
            {
                return "<color=red>" + LanguageManager.CurrentLanguage.misc.enemyAlter_alteredDestroyed + "</color>";
            }
            if (message.Contains("=>")) //4-S transaction complete
            {
                return message; //4-S transaction complete
            }
            //For some reason 5-S passes through this function instead of passing through HudMessage. So we'll do this
            if (GetCurrentSceneName() == "Level 5-S")
            {
                return StringsParent.GetMessage(message, "", "");
            }

            //chessTip
            if (GetCurrentSceneName() == "CreditsMuseum2")
            {
                return StringsParent.GetMessage(message, "", "");
            }


            //Cybergrind custom pattern fix
            if (GetCurrentSceneName() == "Endless")
            {
                if (message.Contains("NO PATTERNS"))
                {
                    return LanguageManager.CurrentLanguage.cyberGrind.cybergrind_noPatternsSelected;
                }
                return message;
            }

            Logging.Warn("Couldn't find string for message: " + message);
            return message;
        }
        public static void PatchDeathScreen(ref GameObject canvasObj)
        {
            try
            {
                GameObject deathScreen = canvasObj.FindPath("BlackScreen", "YouDiedText");
                //Need to disable the TextOverride component.
                Component[] test = deathScreen.GetComponents(typeof(Component));
                Behaviour bhvr = (Behaviour)test[3];
                bhvr.enabled = false;

                //why only EarlyAccessEnd?
                if (GetCurrentSceneName() == "EarlyAccessEnd")
                {
                    deathScreen.TMP().text = LanguageManager.CurrentLanguage.misc.youDied1 + "\n\n\n\n\n" + LanguageManager.CurrentLanguage.misc.youDied2;
                }
                else
                {
                    try
                    {
                        Text youDiedText = deathScreen.Text();
                        youDiedText.text = LanguageManager.CurrentLanguage.misc.youDied1 + "\n\n\n\n\n" + LanguageManager.CurrentLanguage.misc.youDied2;
                    }
                    catch
                    {
                        Logging.Warn("Failed to patch deathScreen");
                        Logging.Warn("trying to get TextMeshProUGUI component.");
                        deathScreen.TMP().text = LanguageManager.CurrentLanguage.misc.youDied1 + "\n\n\n\n\n" + LanguageManager.CurrentLanguage.misc.youDied2;
                    }
                }
                GameObject deathSequence = canvasObj.FindPath("DeathSequence", "Text (TMP)");
                TextMeshProUGUI deathSequenceText = deathSequence.TMP();
                deathSequenceText.text = LanguageManager.CurrentLanguage.misc.DeathSequence;
            }
            catch (Exception e)
            {
                Logging.Error("Failed to patch death screen");
                Logging.Error(e.ToString());
            }
        }



        public static void PatchMisc(ref GameObject canvasObj)
        {
            string currentLevel = GetCurrentSceneName();
            GameObject player = GameObject.Find("Player");
            GameObject styleMeter = player.FindPath("Main Camera", "HUD Camera", "HUD", "StyleCanvas", "Panel (1)", "Panel", "Text (1)", "Text");
            TextMeshProUGUI styleMeterMultiplierText = styleMeter.TMP();
            styleMeterMultiplierText.text = LanguageManager.CurrentLanguage.style.stylemeter_multiplier;

            //Classic HUD
            GameObject classicHudBw = canvasObj.FindPath("Crosshair Filler", "AltHud", "Filler");
            GameObject classicHudColor = canvasObj.FindPath("Crosshair Filler", "AltHud (2)", "Filler");

            TextMeshProUGUI classicHudBwHealth = classicHudBw.FindPath("Health", "Title").TMP();
            TextMeshProUGUI classicHudColorHealth = classicHudColor.FindPath("Health (1)", "Title").TMP();
            classicHudBwHealth.text = LanguageManager.CurrentLanguage.misc.classicHud_health;
            classicHudColorHealth.text = LanguageManager.CurrentLanguage.misc.classicHud_health;

            TextMeshProUGUI classicHudBwStamina = classicHudBw.FindPath("Stamina", "Title").TMP();
            TextMeshProUGUI classicHudColorStamina = classicHudColor.FindPath("Stamina (1)", "Title").TMP();
            classicHudBwStamina.text = LanguageManager.CurrentLanguage.misc.classicHud_stamina;
            classicHudColorStamina.text = LanguageManager.CurrentLanguage.misc.classicHud_stamina;

            TextMeshProUGUI classicHudBwWeapon = classicHudBw.FindPath("Weapon", "Title").TMP();
            TextMeshProUGUI classicHudColorWeapon = classicHudColor.FindPath("Weapon (1)", "Title").TMP();
            classicHudBwWeapon.text = LanguageManager.CurrentLanguage.misc.classicHud_weapon;
            classicHudColorWeapon.text = LanguageManager.CurrentLanguage.misc.classicHud_weapon;

            TextMeshProUGUI classicHudBwArm = classicHudBw.FindPath("Arm", "Title").TMP();
            TextMeshProUGUI classicHudColorArm = classicHudColor.FindPath("Arm (1)", "Title").TMP();
            classicHudBwArm.text = LanguageManager.CurrentLanguage.misc.classicHud_arm;
            classicHudColorArm.text = LanguageManager.CurrentLanguage.misc.classicHud_arm;

            TextMeshProUGUI classicHudBwRailcannon = classicHudBw.FindPath("RailcannonMeter (1)", "Title").TMP();
            TextMeshProUGUI classicHudColorRailcannon = classicHudColor.FindPath("RailcannonMeter (2)", "Title").TMP();
            classicHudBwRailcannon.text = LanguageManager.CurrentLanguage.misc.classicHud_railcannonMeter;
            classicHudColorRailcannon.text = LanguageManager.CurrentLanguage.misc.classicHud_railcannonMeter;

            TextMeshProUGUI classicHudBwSpeedometer = classicHudBw.FindPath("Speedometer", "Title").TMP();
            TextMeshProUGUI classicHudColorSpeedometer = classicHudColor.FindPath("Speedometer", "Title").TMP();
            classicHudBwSpeedometer.text = LanguageManager.CurrentLanguage.misc.classicHud_speed;
            classicHudColorSpeedometer.text = LanguageManager.CurrentLanguage.misc.classicHud_speed;

            //Close prompt when reading book
            TextBinds bookPanelBinds = canvasObj.FindPath("ScanningStuff", "ReadingScanned", "Panel", "Text (1)").GetComponent<TextBinds>();
            bookPanelBinds.text1 = LanguageManager.CurrentLanguage.books.books_pressToClose1 + "<color=orange>";
            bookPanelBinds.text2 = "</color>" + LanguageManager.CurrentLanguage.books.books_pressToClose2;

            if (currentLevel.Contains("7-3")) // Feed It Message
            {
                GameObject feedItMessage = canvasObj.Child("Feed It");
                feedItMessage.GetComponent<TextMeshProUGUI>().text = "<color=red>" + LanguageManager.CurrentLanguage.act3.act3_violenceThird_feedIt + "</color>";
                feedItMessage.GetComponent<ScrollingText>().message = "<color=red>" + LanguageManager.CurrentLanguage.act3.act3_violenceThird_feedIt + "</color>";
            }

        }
    }
}
