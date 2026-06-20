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
                if (aud == null || aud.clip == null)
                    return;

                string clipName = aud.clip.name;
                if (clipName != "gab_HologramFiltered")
                    return;
                AudioSwapper.LogAudioSourceDiagnostics(aud, "GabrielBoat:Flicker");
                string localizedPath = Path.Combine(AudioSwapper.SpeechFolder, "gabrielBoat", "gabrielBoat");

                AudioSwapper.SwapClipWithFileAsync(aud.clip, localizedPath, newClip =>
                {
                    if (newClip != null && ActiveDubbingEnabled())
                    {
                        aud.clip = newClip;
                        Logging.Message($"[Flicker] Swapped clip '{clipName}' → '{newClip.name}'");
                    }
                });
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
                if (aud == null || aud.clip == null)
                    return;

                string clipName = aud.clip.name;
                if (clipName != "gab_HologramFiltered")
                    return;
                AudioSwapper.LogAudioSourceDiagnostics(aud, "GabrielBoat:RandomPitchStart");
                string localizedPath = Path.Combine(AudioSwapper.SpeechFolder, "gabrielBoat", "gabrielBoat");

                AudioSwapper.SwapClipWithFileAsync(aud.clip, localizedPath, newClip =>
                {
                    if (newClip != null && ActiveDubbingEnabled())
                    {
                        aud.clip = newClip;
                        Logging.Message($"[RandomPitch] Swapped clip '{clipName}' → '{newClip.name}'");
                    }
                });
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
                if (aud == null || aud.clip == null)
                    return;

                string clipName = aud.clip.name;
                if (clipName != "gab_HologramFiltered")
                    return;
                AudioSwapper.LogAudioSourceDiagnostics(aud, "GabrielBoat:RandomPitchOnEnable");
                string localizedPath = Path.Combine(AudioSwapper.SpeechFolder, "gabrielBoat", "gabrielBoat");

                AudioSwapper.SwapClipWithFileAsync(aud.clip, localizedPath, newClip =>
                {
                    if (newClip != null && ActiveDubbingEnabled())
                    {
                        aud.clip = newClip;
                        Logging.Message($"[RandomPitch Enable] Swapped clip '{clipName}' → '{newClip.name}'");
                    }
                });
            }
            catch (System.Exception e)
            {
                Logging.Warn("[RandomPitch OnEnable] Swap error: " + e.Message);
            }
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
