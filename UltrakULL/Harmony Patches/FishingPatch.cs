using HarmonyLib;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(FishingHUD),"ShowHUD")]
    public class FishingHUDPatch
    {
        /// <summary>
        /// The texts of the fishing hud and the text the game itself puts in them.
        /// They are kept, so the messages can be translated again after a language
        /// switch.
        /// </summary>
        private static Text outOfWaterText;
        private static Text hookedText;
        private static string outOfWaterGameText;
        private static string hookedGameText;

        [HarmonyPostfix]
        public static void FishingHUDPatch_Postfix(ref GameObject ___outOfWaterMessage, ref GameObject ___hookedContainer)
        {
            if (outOfWaterText == null)
            {
                outOfWaterText = ___outOfWaterMessage.Child("Text").Text();
                hookedText = ___hookedContainer.Child("Text").Text();

                // The messages still show the text of the game at this point.
                if (outOfWaterText != null)
                {
                    outOfWaterGameText = outOfWaterText.text;
                }
                if (hookedText != null)
                {
                    hookedGameText = hookedText.text;
                }
            }

            ApplyTranslations();
        }

        /// <summary>
        /// Applies the active language to the messages of the fishing hud.
        ///
        /// The game only shows the messages, it never fills them, so a language switch
        /// did not reach them and they kept the previous language.
        /// </summary>
        public static void Reapply()
        {
            ApplyTranslations();
        }

        private static void ApplyTranslations()
        {
            if (outOfWaterText == null || hookedText == null)
            {
                return;
            }

            if (isUsingEnglish())
            {
                // The built-in English is the game's own text.
                if (!string.IsNullOrEmpty(outOfWaterGameText))
                {
                    outOfWaterText.text = outOfWaterGameText;
                }
                if (!string.IsNullOrEmpty(hookedGameText))
                {
                    hookedText.text = hookedGameText;
                }
                return;
            }

            outOfWaterText.text = LanguageManager.CurrentLanguage.fishing.fish_outOfWater;
            hookedText.text = LanguageManager.CurrentLanguage.fishing.fish_rodHooked;
        }
    }

    [HarmonyPatch(typeof(FishingHUD),"ShowFishCaught")]
    public class ShowFishPatch
    {
        [HarmonyPostfix]
        public static void ShowFish_Postfix(ref Text ___fishCaughtText,ref GameObject ___fishSizeContainer,bool show = true, FishObject fish = null)
        {
            if(isUsingEnglish())
            {
                return;
            }
            if(show && fish != null)
            {
                string fishName = FishingStrings.GetFishName(fish.fishName);
                if (fishName == null)
                {
                    // A fish without a translation keeps the name of the game.
                    fishName = fish.fishName;
                }

                ___fishCaughtText.text = "<size=56>" + LanguageManager.CurrentLanguage.fishing.fish_fishCaught + "</size> <color=#FFA500><size=80>" + fishName + "</size></color>";
                ___fishCaughtText.resizeTextForBestFit = true;
                
                Text fishSizeText = ___fishSizeContainer.Text();
                fishSizeText.text = "<b>" + LanguageManager.CurrentLanguage.fishing.fish_size + "</b>";
                fishSizeText.resizeTextForBestFit = true;
            }
        }
    }
    
    [HarmonyPatch(typeof(FishingRodWeapon),"Update")]
    public class DisplayWaterType
    {
        [HarmonyPostfix]
        public static void DisplayWaterType_Postfix(ref FishingRodWeapon __instance, ref FishingRodTarget ___targetingCircle, ref FishDB ___currentFishPool, float ___selectedPower)
        {
            if(isUsingEnglish())
            {
                return;
            }
            if(___currentFishPool != null)
            {
                switch(___currentFishPool.fullName)
                {
                    case "The Ocean": {___targetingCircle.waterNameText.text = LanguageManager.CurrentLanguage.fishing.fish_ocean; break;}
                    case "Cave Pool": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_cavePool; break;}
                    case "Stream": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_stream; break;}
                    case "Lake Waters": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_lake; break;}
                    case "Lower Bloods": {___targetingCircle.waterNameText.text = LanguageManager.CurrentLanguage.fishing.fish_lakeBloodLower; break;}
                    case "Lake of Blood": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_lakeBlood; break;}
                    case "Water Well": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_waterWell; break;}
                    case "Pan Oil": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_panOil; break;}
                    case "Holy Lake": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_lakeHoly; break;}
                    case "Deeper Lake Waters": {___targetingCircle.waterNameText.text  = LanguageManager.CurrentLanguage.fishing.fish_lakeDeep; break;}
                }
            }
            
        }

    }
    
    /*[HarmonyPatch(typeof(FishDB),"SetupWater")]
    public class DisplayWaterType
    {
        [HarmonyPostfix]
        public static void DisplayWaterType_Postfix(ref FishDB __instance)
        {
            if(!isUsingEnglish())
            {
                return;
            }
            switch(__instance.fullName)
            {
                case "The Ocean": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_ocean; break;}
                case "Cave Pool": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_cavePool; break;}
                case "Stream": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_stream; break;}
                case "Lake Waters": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_lake; break;}
                case "Lower Bloods": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_lakeBloodLower; break;}
                case "Lake of Blood": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_lakeBlood; break;}
                case "Water Well": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_waterWell; break;}
                case "Pan Oil": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_panOil; break;}
                case "Holy Lake": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_lakeHoly; break;}
                case "Deeper Lake Waters": {__instance.fullName = LanguageManager.CurrentLanguage.fishing.fish_lakeDeep; break;}
            }
        }
    }*/
    
    [HarmonyPatch(typeof(FishEncyclopedia),"SelectFish")]
    public class SelectFish
    {
        /// <summary>
        /// The fish that is open in the encyclopedia and the texts it is shown in.
        /// They are kept, so the fish can be translated again after a language switch.
        /// </summary>
        private static FishObject openFish;
        private static TMP_Text openFishName;
        private static TMP_Text openFishDescription;

        /// <summary>
        /// Applies the active language to the fish that is open.
        ///
        /// The game fills the encyclopedia only when a fish is picked, so a language
        /// switch did not reach it and it kept the previous language until the fish
        /// was picked again.
        ///
        /// Only the text on screen is written. The fish of the game keeps its own
        /// name and description, so the fish that is caught later is still found by
        /// its name.
        /// </summary>
        [HarmonyPostfix]
        public static void SelectFish_Postfix(FishObject fish, FishEncyclopedia __instance, ref TMP_Text ___fishName, ref TMP_Text ___fishDescription)
        {
            openFish = fish;
            openFishName = ___fishName;
            openFishDescription = ___fishDescription;

            ApplyTranslations(fish, ___fishName, ___fishDescription);
        }

        /// <summary>
        /// Applies the active language to the fish that is open.
        /// </summary>
        public static void Reapply()
        {
            if (openFish == null || openFishName == null || openFishDescription == null)
            {
                return;
            }

            if (!openFishName.gameObject.activeInHierarchy)
            {
                // The fish is not on screen, so there is nothing to translate.
                return;
            }

            ApplyTranslations(openFish, openFishName, openFishDescription);
        }

        private static void ApplyTranslations(FishObject fish, TMP_Text fishName, TMP_Text fishDescription)
        {
            if (fish == null || fishName == null || fishDescription == null)
            {
                return;
            }

            bool recognized = MonoSingleton<FishManager>.Instance.recognizedFishes[fish];

            if (isUsingEnglish())
            {
                // The built-in English is the game's own text.
                fishName.text = recognized ? fish.fishName : "???";
                fishDescription.text = fish.description;
                return;
            }

            if (!recognized)
            {
                // The game hides the name of a fish that is not recognized yet.
                fishName.text = "???";
                return;
            }

            string name = FishingStrings.GetFishName(fish.fishName);
            if (name != null)
            {
                fishName.text = name;
            }

            string description = FishingStrings.GetFishDescription(fish.fishName);
            if (description != null)
            {
                fishDescription.text = description;
            }
        }
    }

    /// <summary>
    /// The name and the description of the fish in the active language.
    /// </summary>
    public static class FishingStrings
    {
        /// <summary>
        /// The name of the fish in the active language, or null when the fish has no
        /// translation.
        /// </summary>
        public static string GetFishName(string englishName)
        {
            switch (englishName)
            {
                case "Funny Stupid Fish (Friend)":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_funnyStupidFish;
                    }
                case "PITR Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_pitrFish;
                    }
                case "Trout":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_trout;
                    }
                case "Metal Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_amidEvilFish;
                    }
                case "Chomper":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_chomper;
                    }
                case "Bomb Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_bombFish;
                    }
                case "Eyeball":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_gibeye;
                    }
                case "Frog (?)":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_ironLungFish;
                    }
                case "Dope Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_dopeFish;
                    }
                case "Stickfish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_stickFish;
                    }
                case "Cooked Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_cookedFish;
                    }
                case "Shark":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_shark;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// The description of the fish in the active language, or null when the fish
        /// has no translation.
        /// </summary>
        public static string GetFishDescription(string englishName)
        {
            switch (englishName)
            {
                case "Funny Stupid Fish (Friend)":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_funnyStupidFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_funnyStupidFishDescription2;
                    }
                case "PITR Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_pitrFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_pitrFishDescription2;
                    }
                case "Trout":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_troutDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_troutDescription2;
                    }
                case "Metal Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_amidEvilFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_amidEvilFishDescription2;
                    }
                case "Chomper":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_chomperDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_chomperDescription2;
                    }
                case "Bomb Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_bombFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_bombFishDescription2;
                    }
                case "Eyeball":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_gibeyeDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_gibeyeDescription2;
                    }
                case "Frog (?)":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_ironLungFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_ironLungFishDescription2;
                    }
                case "Dope Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_dopeFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_dopeFishDescription2;
                    }
                case "Stickfish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_stickFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_stickFishDescription2;
                    }
                case "Cooked Fish":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_cookedFishDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_cookedFishDescription2;
                    }
                case "Shark":
                    {
                        return LanguageManager.CurrentLanguage.fishing.fish_sharkDescription1 + "\n\n"
                            + LanguageManager.CurrentLanguage.fishing.fish_sharkDescription2;
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
