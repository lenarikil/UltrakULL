using BepInEx.Bootstrap;
using HarmonyLib;
using System;
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
            // Built-in English keeps the game's original pause menu text.
            if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

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
                RegisterPage(__instance);
            }
            catch (Exception e)
            {
                Logging.Error("Something went wrong while patching options.");
                Logging.Error(e.ToString());
            }

        }

        /// <summary>
        /// Translates the options page the game has built.
        ///
        /// The game builds a page once, when the options menu starts, and never fills
        /// it again, so the page kept the language that was active before a switch.
        /// The page is registered as a live group: a language switch puts the game's
        /// own text back and writes the new language on top of it, so the page that is
        /// open does not have to be closed and opened again.
        /// </summary>
        private static void RegisterPage(SettingsPageBuilder builder)
        {
            if (builder == null)
            {
                return;
            }

            LiveText.ApplyGroup(builder.gameObject, delegate { ApplyPage(builder); });
        }

        /// <summary>
        /// Writes the translation of the active language into the options page.
        /// </summary>
        private static void ApplyPage(SettingsPageBuilder builder)
        {
            GameObject optionsObject = builder.gameObject;
            switch (builder.name.ToUpper())
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
                        Logging.Warn("Unknown Option page name: " + builder.name);
                        break;
                    }

            }
        }
    }
}
