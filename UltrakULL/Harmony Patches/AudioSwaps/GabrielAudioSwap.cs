using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
    [HarmonyPatch(typeof(Gabriel),"Start")]
    public static class GabrielAudioSwap
    {
        private static readonly BindingFlags VoiceFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        private struct VoiceOriginals
        {
            public AudioClip[] taunt;
            public AudioClip[] bigHurt;
            public AudioClip[] hurt;
            public AudioClip phaseChange;
        }

        private static readonly Dictionary<int, VoiceOriginals> _savedVoiceClips = new Dictionary<int, VoiceOriginals>();
        private static readonly Dictionary<int, AudioClip> _savedOutroClips = new Dictionary<int, AudioClip>();
        private static readonly object _lock = new object();

        private static GabrielVoice GetVoice(Gabriel instance)
        {
            var gabeBase = instance.gabe;
            if (gabeBase == null) return null;

            try
            {
                var voiceProperty = gabeBase.GetType().GetProperty("voice");
                if (voiceProperty != null)
                    return voiceProperty.GetValue(gabeBase) as GabrielVoice;

                var voiceField = gabeBase.GetType().GetField("voice", VoiceFlags);
                if (voiceField != null)
                    return voiceField.GetValue(gabeBase) as GabrielVoice;
            }
            catch { }
            return null;
        }

        private static void SaveOriginals(Gabriel instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            GabrielVoice voice = GetVoice(instance);
            if (voice != null && !_savedVoiceClips.ContainsKey(id))
            {
                var vo = new VoiceOriginals();
                if (voice.taunt != null) vo.taunt = (AudioClip[])voice.taunt.Clone();
                if (voice.bigHurt != null) vo.bigHurt = (AudioClip[])voice.bigHurt.Clone();
                if (voice.hurt != null) vo.hurt = (AudioClip[])voice.hurt.Clone();
                if (voice.phaseChange != null) vo.phaseChange = voice.phaseChange;
                _savedVoiceClips[id] = vo;
                Logging.Info($"[GabrielAudioSwap] Saved voice originals for instance {id}");
            }

            GabrielOutro outro = UnityEngine.Object.FindObjectOfType<GabrielOutro>(true);
            if (outro != null)
            {
                AudioSource[] sources = outro.GetComponentsInChildren<AudioSource>(true);
                foreach (var source in sources)
                {
                    if (source != null && source.clip != null && source.clip.name == "gab_BigHurt1" && !_savedOutroClips.ContainsKey(id))
                    {
                        _savedOutroClips[id] = source.clip;
                        Logging.Info($"[GabrielAudioSwap] Saved outro clip '{source.clip.name}' for instance {id}");
                    }
                }
            }
        }

        private static void RestoreOriginals(Gabriel instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            lock (_lock)
            {
                GabrielVoice voice = GetVoice(instance);
                if (voice != null && _savedVoiceClips.TryGetValue(id, out VoiceOriginals vo))
                {
                    if (vo.taunt != null && voice.taunt != null && vo.taunt.Length == voice.taunt.Length)
                        Array.Copy(vo.taunt, voice.taunt, vo.taunt.Length);
                    if (vo.bigHurt != null && voice.bigHurt != null && vo.bigHurt.Length == voice.bigHurt.Length)
                        Array.Copy(vo.bigHurt, voice.bigHurt, vo.bigHurt.Length);
                    if (vo.hurt != null && voice.hurt != null && vo.hurt.Length == voice.hurt.Length)
                        Array.Copy(vo.hurt, voice.hurt, vo.hurt.Length);
                    if (vo.phaseChange != null) voice.phaseChange = vo.phaseChange;
                    Logging.Info($"[GabrielAudioSwap] Restored voice originals for instance {id}");
                }
                _savedVoiceClips.Remove(id);

                if (_savedOutroClips.TryGetValue(id, out AudioClip savedOutro))
                {
                    GabrielOutro outro = UnityEngine.Object.FindObjectOfType<GabrielOutro>(true);
                    if (outro != null)
                    {
                        AudioSource[] sources = outro.GetComponentsInChildren<AudioSource>(true);
                        foreach (var source in sources)
                        {
                            if (source != null && source.clip != null && source.clip.name == "gab_BigHurt1")
                            {
                                source.clip = savedOutro;
                                Logging.Info($"[GabrielAudioSwap] Restored outro clip '{savedOutro.name}' for instance {id}");
                            }
                        }
                    }
                    _savedOutroClips.Remove(id);
                }
            }
        }

        [HarmonyPostfix]
        public static void Gabriel_VoiceSwap(ref Gabriel __instance)
        {
            SaveOriginals(__instance);

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
            Gabriel[] instances = UnityEngine.Object.FindObjectsOfType<Gabriel>(true);

            if (LanguageManager.configFile.Bind("General","activeDubbing","False").Value == "False" || isUsingEnglish())
            {
                foreach (var instance in instances)
                {
                    if (instance == null) continue;
                    RestoreOriginals(instance);
                }
                return;
            }

            foreach (var instance in instances)
            {
                if (instance == null) continue;
                SaveOriginals(instance);
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
            
            GabrielVoice voice = GetVoice(__instance);
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
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }
        }
    }
}
