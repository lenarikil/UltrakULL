using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using static UltrakULL.CommonFunctions;
using SettingsMenu.Components;
using UnityEngine;
using TMPro;
using UltrakULL.json;

namespace UltrakULL.Harmony_Patches
{

    [HarmonyPatch(typeof(PauseMenu))]
    public static class PauseMenuPatch
    {
        [HarmonyPatch("OnEnable"), HarmonyPostfix]
        public static void PauseMenuOnEnablePostfix(TMP_Text ___checkpointText)
        {
            try
            {
                if (___checkpointText.text.Contains("SKIP"))
                {
                    ___checkpointText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_skip;
                }
            }
            catch (Exception e)
            { 
                Logging.Warn("Failed to patch SKIP button in pause menu");
                Logging.Warn(e.ToString());
            }
        }
    }
    [HarmonyPatch(typeof(SettingsMenu.Components.SettingsPageBuilder))]
    public static class OptionsPatch
    {
        private static bool? _noMusicStopParryLoaded = null;

        private static bool IsNoMusicStopParryLoaded()
        {
            if (_noMusicStopParryLoaded == null)
                _noMusicStopParryLoaded = Chainloader.PluginInfos.ContainsKey("com.AquaEther.NoMusicStopParry");
            return _noMusicStopParryLoaded.Value;
        }

        [HarmonyPatch("BuildPage"), HarmonyPostfix]
        public static void OptionsSetSelectedPostfix(SettingsPageBuilder __instance) {
            try
            {
                if (IsNoMusicStopParryLoaded())
                {
                    Logging.Debug("NoMusicStopParry detected, some option patches may be skipped for compatibility.");
                }

                Logging.Debug("Patching Option menu...");
                GameObject optionsObject = __instance.gameObject;
                switch (__instance.name.ToUpper())
                {
                    case "GENERAL":
                        {
                            Logging.Debug("GENERAL");
                            Options.PatchGeneralOptions(optionsObject);
                            break;
                        }
                    case "CONTROLS":
                        {
                            Logging.Debug("CONTROLS");
                            Options.PatchControlOptions(optionsObject);
                            break;
                        }
                    case "GRAPHICS":
                        {
                            Logging.Debug("GRAPHICS");
                            Options.PatchGraphicsOptions(optionsObject);
                            break;
                        }
                    case "AUDIO":
                        {
                            Logging.Debug("AUDIO");
                            Options.PatchAudioOptions(optionsObject);
                            break;
                        }
                    case "ASSIST":
                        {
                            Logging.Debug("ASSIST");
                            Options.PatchAssistOptions(optionsObject);
                            break;
                        }
                    case "HUD":
                        {
                            Logging.Debug("HUD");
                            Options.PatchHUDOptions(optionsObject);
                            break;
                        }
                    default:
                        {
                            Logging.Warn("Unknown Option page name: " + __instance.name);
                            break;
                        }

                }
            }
            catch (Exception e)
            {
                Logging.Error("Something went wrong while patching options.");
                Logging.Error(e.ToString());
            }

        }
    }
}
