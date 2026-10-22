using HarmonyLib;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using System.IO;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch]
    public static class GabrielBoatRandomPitchAudioSwap
    {
        // === PATCH 1: Flicker.Start ===
        [HarmonyPatch(typeof(Flicker), "Start")]
        [HarmonyPostfix]
        public static void Flicker_Start_Postfix(Flicker __instance)
        {
            try
            {
                if (!ActiveDubbingEnabled())
                    return;

                var aud = __instance.GetComponent<AudioSource>();
                if (!IsHologramVoice(aud))
                    return;

                AudioSwapper.LogAudioSourceDiagnostics(aud, "GabrielBoat:Flicker");
                string localizedPath = Path.Combine(AudioSwapper.SpeechFolder, "gabrielBoat", "gabrielBoat");

                // The swap keeps the playback time, so the speech of the hologram
                // follows a language switch in the middle of the line.
                AudioSwapper.SwapSourceClip(aud, localizedPath);
            }
            catch (System.Exception e)
            {
                Logging.Warn("[Flicker Patch] Swap error: " + e.Message);
            }
        }

        // === PATCH 2: RandomPitch.Start ===
        [HarmonyPatch(typeof(RandomPitch), "Start")]
        [HarmonyPostfix]
        public static void RandomPitch_Start_Postfix(RandomPitch __instance)
        {
            try
            {
                if (!ActiveDubbingEnabled())
                    return;

                var aud = __instance.GetComponent<AudioSource>();
                if (!IsHologramVoice(aud))
                    return;

                AudioSwapper.LogAudioSourceDiagnostics(aud, "GabrielBoat:RandomPitchStart");
                string localizedPath = Path.Combine(AudioSwapper.SpeechFolder, "gabrielBoat", "gabrielBoat");

                AudioSwapper.SwapSourceClip(aud, localizedPath);
            }
            catch (System.Exception e)
            {
                Logging.Warn("[RandomPitch Patch] Swap error: " + e.Message);
            }
        }

        // === PATCH 3: RandomPitch.OnEnable ===
        [HarmonyPatch(typeof(RandomPitch), "OnEnable")]
        [HarmonyPostfix]
        public static void RandomPitch_OnEnable_Postfix(RandomPitch __instance)
        {
            try
            {
                if (!ActiveDubbingEnabled())
                    return;

                var aud = __instance.GetComponent<AudioSource>();
                if (!IsHologramVoice(aud))
                    return;

                AudioSwapper.LogAudioSourceDiagnostics(aud, "GabrielBoat:RandomPitchOnEnable");
                string localizedPath = Path.Combine(AudioSwapper.SpeechFolder, "gabrielBoat", "gabrielBoat");

                AudioSwapper.SwapSourceClip(aud, localizedPath);
            }
            catch (System.Exception e)
            {
                Logging.Warn("[RandomPitch OnEnable] Swap error: " + e.Message);
            }
        }

        /// <summary>
        /// True when the source plays the speech of the boat hologram.
        ///
        /// The name of the game's own clip is checked, because the name of the
        /// current clip changes with the language.
        /// </summary>
        private static bool IsHologramVoice(AudioSource aud)
        {
            if (aud == null)
                return false;

            AudioClip gameClip = AudioSwapper.GetOriginalClip(aud);
            if (gameClip == null)
                gameClip = aud.clip;

            return gameClip != null && gameClip.name == "gab_HologramFiltered";
        }

        private static bool ActiveDubbingEnabled()
        {
            try
            {
                return !isUsingEnglish() &&
                       LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value != "False";
            }
            catch
            {
                return false;
            }
        }
    }
}
