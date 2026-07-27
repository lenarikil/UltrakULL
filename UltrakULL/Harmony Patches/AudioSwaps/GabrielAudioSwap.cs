using HarmonyLib;
using System.IO;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
    [HarmonyPatch(typeof(Gabriel),"Start")]
    public static class GabrielAudioSwap
    {
        [HarmonyPostfix]
        public static void Gabriel_VoiceSwap(ref Gabriel __instance)
        {
            if(LanguageManager.configFile.Bind("General","activeDubbing","False").Value == "False" || isUsingEnglish())
                return;

            Gabriel instance = __instance;
            AudioPreloadManager.EnsureCurrentScenePreloaded(delegate
            {
                ApplyVoiceSwap(instance);
                ApplyOutroSwap(instance);
            });
        }

        /// <summary>
        /// Re-applies audio swaps to all existing Gabriel instances in the scene.
        /// Called when the language is changed without reloading the scene.
        /// </summary>
        public static void RebindExistingInstances()
        {
            if (LanguageManager.configFile.Bind("General","activeDubbing","False").Value == "False" || isUsingEnglish())
                return;

            Gabriel[] instances = UnityEngine.Object.FindObjectsOfType<Gabriel>(true);
            foreach (var instance in instances)
            {
                if (instance == null) continue;
                AudioPreloadManager.EnsureCurrentScenePreloaded(delegate
                {
                    ApplyVoiceSwap(instance);
                    ApplyOutroSwap(instance);
                });
            }
        }

        private static void ApplyVoiceSwap(Gabriel __instance)
        {
            if (__instance == null)
                return;
            
            string gabeFirstFolder =  AudioSwapper.SpeechFolder + "gabrielBossFirst" + Path.DirectorySeparatorChar;
            
            var gabeBase = __instance.gabe;
            if (gabeBase == null) return;
            
            GabrielVoice voice = null;
            try
            {
                var voiceProperty = gabeBase.GetType().GetProperty("voice");
                if (voiceProperty != null)
                {
                    voice = voiceProperty.GetValue(gabeBase) as GabrielVoice;
                }
                else
                {
                    var voiceField = gabeBase.GetType().GetField("voice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (voiceField != null)
                    {
                        voice = voiceField.GetValue(gabeBase) as GabrielVoice;
                    }
                }
            }
            catch
            {
                return;
            }
            
            if (voice == null) return;

            AudioSwapper.LogAudioSourceDiagnostics(voice.GetComponent<AudioSource>(), "GabrielVoice");
            
            //Taunts
            AudioClip[] gabeTaunts = voice.taunt;
            
            //Line order is based on line order of the, so it's not alphabetical.
            string[] tauntLines =
            {
                "gabrielTaunt_YouDefyTheLight",
                "gabrielTaunt_AMereObject",
                "gabrielTaunt_ThereCanBeOnlyLight",
                "gabrielTaunt_Foolishness",
                "gabrielTaunt_AnImperfection",
                "gabrielTaunt_NotEvenMortal",
                "gabrielTaunt_YouAreLessThanNothing",
                "gabrielTaunt_YoureAnError",
                "gabrielTaunt_TheLightIsPerfection",
                "gabrielTaunt_YouAreOutclassed",
                "gabrielTaunt_YourCrimeIsExistence",
                "gabrielTaunt_YouMakeEven"
            };
            for(int x = 0; x < gabeTaunts.Length; x++)
            {
                int ix = x;
                string gabrielTauntString = gabeFirstFolder + tauntLines[ix];
                AudioSwapper.SwapClipInArrayAsync(gabeTaunts, ix, gabrielTauntString);
            }
            
            //Phase change - need to use ref otherwise it gets swapped back to original
            AudioClip tmpPhase = voice.phaseChange;
            string gabrielPhaseChangeString = gabeFirstFolder + "gabrielPhaseChange";
            AudioSwapper.SwapClipWithFileAsync(tmpPhase, gabrielPhaseChangeString, (clip) => { try { voice.phaseChange = clip; } catch { } });

            //Big hurt
            AudioClip[] gabeBigHurt = voice.bigHurt;
            for(int x = 0; x < gabeBigHurt.Length; x++)
            {
                int ix = x;
                string gabrielBigHurtString = gabeFirstFolder + "gabrielBigHurt" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(gabeBigHurt, ix, gabrielBigHurtString);
            }

            //Hurt
            AudioClip[] gabeHurt = voice.hurt;
            for(int x = 0; x < gabeHurt.Length; x++)
            {
                int ix = x;
                string gabrielHurtString = gabeFirstFolder + "gabrielHurt" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(gabeHurt, ix, gabrielHurtString);
            }
        }
        private static void ApplyOutroSwap(Gabriel __instance)
        {
            if (__instance == null)
                return;

            string folder = AudioSwapper.SpeechFolder + "gabrielBossFirst" + Path.DirectorySeparatorChar;

            GabrielOutro outro = UnityEngine.Object.FindObjectOfType<GabrielOutro>(true);

            if (outro == null)
                return;

            AudioSource[] sources = outro.GetComponentsInChildren<AudioSource>(true);

            foreach (AudioSource source in sources)
            {
                if (source == null || source.clip == null)
                    continue;

                if (source.clip.name != "gab_BigHurt1")
                    continue;

                string path = folder + "gabrielBigHurt1";

                AudioSwapper.SwapClipWithFileAsync(source.clip, path, (clip) =>
                {
                    if (clip == null)
                        return;

                    try
                    {
                        if (source != null)
                        {
                            source.clip = clip;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }
        }
    }
}
