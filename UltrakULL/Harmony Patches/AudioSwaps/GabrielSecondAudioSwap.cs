using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
    [HarmonyPatch(typeof(GabrielSecond), "Awake")]
    public static class GabrielSecondAudioSwap
    {
        private struct VoiceOriginals
        {
            public AudioClip[] taunt;
            public AudioClip[] bigHurt;
            public AudioClip[] hurt;
            public AudioClip[] tauntSecondPhase;
            public AudioClip phaseChange;
        }

        private static readonly Dictionary<int, VoiceOriginals> _savedVoiceClips = new Dictionary<int, VoiceOriginals>();
        private static readonly Dictionary<int, AudioClip> _savedOutroClips = new Dictionary<int, AudioClip>();
        private static readonly object _lock = new object();

        private static void SaveOriginals(GabrielSecond instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            GabrielVoice voice = instance.GetComponent<GabrielVoice>();
            if (voice != null && !_savedVoiceClips.ContainsKey(id))
            {
                var vo = new VoiceOriginals();
                if (voice.taunt != null) vo.taunt = (AudioClip[])voice.taunt.Clone();
                if (voice.bigHurt != null) vo.bigHurt = (AudioClip[])voice.bigHurt.Clone();
                if (voice.hurt != null) vo.hurt = (AudioClip[])voice.hurt.Clone();
                if (voice.tauntSecondPhase != null) vo.tauntSecondPhase = (AudioClip[])voice.tauntSecondPhase.Clone();
                if (voice.phaseChange != null) vo.phaseChange = voice.phaseChange;
                _savedVoiceClips[id] = vo;
                Logging.Info($"[GabrielSecondAudioSwap] Saved voice originals for instance {id}");
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
                        Logging.Info($"[GabrielSecondAudioSwap] Saved outro clip '{source.clip.name}' for instance {id}");
                    }
                }
            }
        }

        private static void RestoreOriginals(GabrielSecond instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            lock (_lock)
            {
                GabrielVoice voice = instance.GetComponent<GabrielVoice>();
                if (voice != null && _savedVoiceClips.TryGetValue(id, out VoiceOriginals vo))
                {
                    if (vo.taunt != null && voice.taunt != null && vo.taunt.Length == voice.taunt.Length)
                        Array.Copy(vo.taunt, voice.taunt, vo.taunt.Length);
                    if (vo.bigHurt != null && voice.bigHurt != null && vo.bigHurt.Length == voice.bigHurt.Length)
                        Array.Copy(vo.bigHurt, voice.bigHurt, vo.bigHurt.Length);
                    if (vo.hurt != null && voice.hurt != null && vo.hurt.Length == voice.hurt.Length)
                        Array.Copy(vo.hurt, voice.hurt, vo.hurt.Length);
                    if (vo.tauntSecondPhase != null && voice.tauntSecondPhase != null && vo.tauntSecondPhase.Length == voice.tauntSecondPhase.Length)
                        Array.Copy(vo.tauntSecondPhase, voice.tauntSecondPhase, vo.tauntSecondPhase.Length);
                    if (vo.phaseChange != null) voice.phaseChange = vo.phaseChange;
                    Logging.Info($"[GabrielSecondAudioSwap] Restored voice originals for instance {id}");
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
                                Logging.Info($"[GabrielSecondAudioSwap] Restored outro clip '{savedOutro.name}' for instance {id}");
                            }
                        }
                    }
                    _savedOutroClips.Remove(id);
                }
            }
        }

        [HarmonyPostfix]
        public static void GabrielSecond_VoiceSwap(ref GabrielSecond __instance)
        {
            SaveOriginals(__instance);

            if (LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value == "False" || isUsingEnglish())
            {
                return;
            }
            GabrielSecond instance = __instance;
            AudioPreloadManager.EnsureCurrentScenePreloaded(delegate
            {
                ApplyVoiceSwap(instance);
                ApplyOutroSwap(instance);
            });
        }

        /// <summary>
        /// Re-applies audio swaps to all existing GabrielSecond instances in the scene.
        /// </summary>
        public static void RebindExistingInstances()
        {
            GabrielSecond[] instances = UnityEngine.Object.FindObjectsOfType<GabrielSecond>(true);

            if (LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value == "False" || isUsingEnglish())
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

        private static void ApplyVoiceSwap(GabrielSecond __instance)
        {
            if (__instance == null)
                return;

            GabrielVoice voice = __instance.GetComponent<GabrielVoice>();
            if (voice == null)
            {
                Debug.LogWarning("[UltrakULL] GabrielVoice component not found on GabrielSecond!");
                return;
            }
            AudioSwapper.LogAudioSourceDiagnostics(voice.GetComponent<AudioSource>(), "GabrielSecondVoice");
            string gabeSecondFolder = AudioSwapper.SpeechFolder + "gabrielBossSecond" + Path.DirectorySeparatorChar;
            
            // Taunts
            AudioClip[] gabeSecondTaunts = voice.taunt;
            string[] tauntLines = new string[]
            {
                "gabrielSecondTaunt_IsThisWhatILostTo",
                "gabrielSecondTaunt_YoureGettingRusty",
                "gabrielSecondTaunt_LetsSettleThis",
                "gabrielSecondTaunt_NothingButScrap",
                "gabrielSecondTaunt_IllShowYouDivine",
                "gabrielSecondTaunt_TimeToRight",
                "gabrielSecondTaunt_YouNeedMorePower"
            };
            for (int i = 0; i < gabeSecondTaunts.Length; i++)
            {
                int ix = i;
                string gabrielSecondTauntString = gabeSecondFolder + tauntLines[ix];
                AudioSwapper.SwapClipInArrayAsync(gabeSecondTaunts, ix, gabrielSecondTauntString);
            }
            
            // Phase change
            AudioClip tmpPhase = voice.phaseChange;
            string gabrielSecondPhaseChangeString = gabeSecondFolder + "gabrielSecondPhaseChange";
            AudioSwapper.SwapClipWithFileAsync(tmpPhase, gabrielSecondPhaseChangeString, (clip) =>
            {
                try { voice.phaseChange = clip; }
                catch { }
            });
            
            // Big hurt
            AudioClip[] gabeSecondBigHurt = voice.bigHurt;
            for (int i = 0; i < gabeSecondBigHurt.Length; i++)
            {
                int ix = i;
                string gabrielSecondBigHurtString = gabeSecondFolder + "gabrielSecondBigHurt" + (ix + 1).ToString();
                AudioSwapper.SwapClipInArrayAsync(gabeSecondBigHurt, ix, gabrielSecondBigHurtString);
            }
            
            // Hurt
            AudioClip[] gabeSecondHurt = voice.hurt;
            for (int i = 0; i < gabeSecondHurt.Length; i++)
            {
                int ix = i;
                string gabrielSecondHurtString = gabeSecondFolder + "gabrielSecondHurt" + (ix + 1).ToString();
                AudioSwapper.SwapClipInArrayAsync(gabeSecondHurt, ix, gabrielSecondHurtString);
            }
            
            // Taunts second phase
            string[] tauntLinesSecondPhase = new string[]
            {
                "gabrielSecondTaunt_IveNeverHadAFight",
                "gabrielSecondTaunt_ShowMeWhat",
                "gabrielSecondTaunt_NowThisIsAFight",
                "gabrielSecondTaunt_WhatIsThisFeeling",
                "gabrielSecondTaunt_ComeGetSomeBlood",
                "gabrielSecondTaunt_ComeOnMachine",
                "gabrielSecondTaunt_IllShowYouTrueSplendor"
            };
            AudioClip[] gabeSecondTauntsSecondPhase = voice.tauntSecondPhase;
            for (int i = 0; i < gabeSecondTauntsSecondPhase.Length; i++)
            {
                int ix = i;
                string gabeSecondTauntsSecondPhaseString = gabeSecondFolder + tauntLinesSecondPhase[ix];
                AudioSwapper.SwapClipInArrayAsync(gabeSecondTauntsSecondPhase, ix, gabeSecondTauntsSecondPhaseString);
            }
        }
        private static void ApplyOutroSwap(GabrielSecond __instance)
        {
            if (__instance == null)
                return;

            string folder = AudioSwapper.SpeechFolder + "gabrielBossSecond" + Path.DirectorySeparatorChar;

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

                string path = folder + "gabrielSecondBigHurt1";

                AudioSwapper.SwapClipWithFileAsync(source.clip, path, (clip) =>
                {
                    if (clip == null)
                    {
                        return;
                    }

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
