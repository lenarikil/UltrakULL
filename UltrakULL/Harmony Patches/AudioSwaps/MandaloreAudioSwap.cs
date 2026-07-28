using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
    [HarmonyPatch(typeof(Mandalore), "Start")]
    public static class MandaloreAudioSwap
    {
        private static string _deathSubtitleText;
        private static readonly HashSet<int> _deathHandledInstances = new HashSet<int>();
        private static int _tauntSequenceIndex = 0;

        private static int GetSequentialTauntIndex(int max)
        {
            return _tauntSequenceIndex++ % max;
        }

        private static readonly Dictionary<string, string> ClipNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "fullauto", "mandaloreFullAuto" },
            { "fullerauto", "mandaloreFullerAuto" },
            { "speed1_shammy", "mandalorePhaseChange1Owl" },
            { "speed2_shammy", "mandalorePhaseChange2Owl" },
            { "speed3_shammy", "mandalorePhaseChangeFinalOwl" },
            { "death_shammy", "mandaloreDefeatedOwl" },
            { "taunt1_shammy", "mandaloreTaunt_ImGonnaShootThem" },
            { "taunt2_shammy", "mandaloreTaunt_WhyAreWeInThePast" },
            { "taunt3_shammy", "mandaloreTaunt_ImGonnaPoisonYou" },
            { "taunt4_shammy", "mandaloreTaunt4Owl" },
            { "speed1_mandy", "mandalorePhaseChange1Manda" },
            { "speed2_mandy", "mandalorePhaseChange2Manda" },
            { "speed3_mandy", "mandalorePhaseChangeFinalManda" },
            { "death_mandy", "mandaloreDefeatedManda" },
            { "taunt1_mandy", "mandaloreTaunt_YouCannotImagine" },
            { "taunt2_mandy", "mandaloreTaunt2Manda" },
            { "taunt3_mandy", "mandaloreTaunt_What" },
            { "taunt4_mandy", "mandaloreTaunt_HoldStill" },
        };

        private struct VoiceOriginals
        {
            public AudioClip secondPhase;
            public AudioClip thirdPhase;
            public AudioClip finalPhase;
            public AudioClip death;
            public AudioClip[] taunts;
        }

        private static readonly Dictionary<int, AudioClip> _savedSingleClips = new Dictionary<int, AudioClip>();
        private static readonly Dictionary<int, Dictionary<int, VoiceOriginals>> _savedVoiceClips = new Dictionary<int, Dictionary<int, VoiceOriginals>>();
        private static readonly object _lock = new object();

        private static void SaveOriginals(Mandalore instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            if (instance.voiceFull != null && !_savedSingleClips.ContainsKey(id * 100 + 0))
            {
                _savedSingleClips[id * 100 + 0] = instance.voiceFull;
                Logging.Info($"[MandaloreAudioSwap] Saved original voiceFull '{instance.voiceFull.name}' (instance {id})");
            }
            if (instance.voiceFuller != null && !_savedSingleClips.ContainsKey(id * 100 + 1))
            {
                _savedSingleClips[id * 100 + 1] = instance.voiceFuller;
                Logging.Info($"[MandaloreAudioSwap] Saved original voiceFuller '{instance.voiceFuller.name}' (instance {id})");
            }

            if (instance.voices == null) return;
            var voiceDict = new Dictionary<int, VoiceOriginals>();
            for (int v = 0; v < instance.voices.Length; v++)
            {
                if (instance.voices[v] == null) continue;
                var vo = new VoiceOriginals();
                if (instance.voices[v].secondPhase != null) vo.secondPhase = instance.voices[v].secondPhase;
                if (instance.voices[v].thirdPhase != null) vo.thirdPhase = instance.voices[v].thirdPhase;
                if (instance.voices[v].finalPhase != null) vo.finalPhase = instance.voices[v].finalPhase;
                if (instance.voices[v].death != null) vo.death = instance.voices[v].death;
                if (instance.voices[v].taunts != null) vo.taunts = (AudioClip[])instance.voices[v].taunts.Clone();
                voiceDict[v] = vo;
                Logging.Info($"[MandaloreAudioSwap] Saved voice[{v}] originals for instance {id}");
            }
            if (!_savedVoiceClips.ContainsKey(id))
                _savedVoiceClips[id] = voiceDict;
        }

        private static void RestoreOriginals(Mandalore instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            lock (_lock)
            {
                if (_savedSingleClips.TryGetValue(id * 100 + 0, out AudioClip savedFull))
                {
                    instance.voiceFull = savedFull;
                    Logging.Info($"[MandaloreAudioSwap] Restored voiceFull '{savedFull.name}' (instance {id})");
                }
                if (_savedSingleClips.TryGetValue(id * 100 + 1, out AudioClip savedFuller))
                {
                    instance.voiceFuller = savedFuller;
                    Logging.Info($"[MandaloreAudioSwap] Restored voiceFuller '{savedFuller.name}' (instance {id})");
                }
                _savedSingleClips.Remove(id * 100 + 0);
                _savedSingleClips.Remove(id * 100 + 1);

                if (_savedVoiceClips.TryGetValue(id, out var voiceDict) && instance.voices != null)
                {
                    foreach (var kvp in voiceDict)
                    {
                        int v = kvp.Key;
                        if (v >= instance.voices.Length || instance.voices[v] == null) continue;
                        var vo = kvp.Value;
                        if (vo.secondPhase != null) instance.voices[v].secondPhase = vo.secondPhase;
                        if (vo.thirdPhase != null) instance.voices[v].thirdPhase = vo.thirdPhase;
                        if (vo.finalPhase != null) instance.voices[v].finalPhase = vo.finalPhase;
                        if (vo.death != null) instance.voices[v].death = vo.death;
                        if (vo.taunts != null && instance.voices[v].taunts != null && vo.taunts.Length == instance.voices[v].taunts.Length)
                            Array.Copy(vo.taunts, instance.voices[v].taunts, vo.taunts.Length);
                        Logging.Info($"[MandaloreAudioSwap] Restored voice[{v}] originals (instance {id})");
                    }
                    _savedVoiceClips.Remove(id);
                }
            }
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MandaloreStartTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var randomRange = AccessTools.Method(typeof(UnityEngine.Random), "Range", new[] { typeof(int), typeof(int) });
            var sequentialTaunt = AccessTools.Method(typeof(MandaloreAudioSwap), nameof(GetSequentialTauntIndex), new[] { typeof(int) });

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand != null && codes[i].operand.Equals(randomRange))
                {
                    int minPush = FindPrecedingLdcI4_0(codes, i);
                    if (minPush >= 0)
                    {
                        codes.RemoveAt(minPush);
                        i--;
                    }
                    codes[i].operand = sequentialTaunt;
                    break;
                }
            }
            return codes;
        }

        private static int FindPrecedingLdcI4_0(List<CodeInstruction> codes, int fromIndex)
        {
            for (int j = fromIndex - 1; j >= 0; j--)
            {
                if (codes[j].opcode == OpCodes.Ldc_I4_0)
                {
                    if (j + 1 < fromIndex && codes[j + 1].opcode == OpCodes.Ldelem_Ref)
                        continue;
                    return j;
                }
            }
            return -1;
        }

        [HarmonyPrefix]
        public static bool Mandalore_AudioSwap(Mandalore __instance)
        {
            try
            {
                SaveOriginals(__instance);

                if (LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value == "False" || isUsingEnglish())
                    return true;

                ApplyAudioSwap(__instance);
            }
            catch (Exception e)
            {
                Logging.Warn("[MandaloreAudioSwap] Exception in patch: " + e.Message);
            }
            return true;
        }

        /// <summary>
        /// Re-applies audio swaps to all existing Mandalore instances in the scene.
        /// Called when the language is changed without reloading the scene.
        /// </summary>
        public static void RebindExistingInstances()
        {
            try
            {
                Mandalore[] instances = UnityEngine.Object.FindObjectsOfType<Mandalore>(true);

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
                    ApplyAudioSwap(instance);
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[MandaloreAudioSwap] Exception in RebindExistingInstances: " + e.Message);
            }
        }

        private static string ResolveClipPath(string clipName, string folder)
        {
            string mappedName;
            if (ClipNameMap.TryGetValue(clipName, out mappedName))
                return folder + mappedName;
            return folder + clipName;
        }

        private static void ApplyAudioSwap(Mandalore __instance)
        {
            if (__instance == null) return;

            string mandaloreFolder = AudioSwapper.SpeechFolder + "mandalore" + Path.DirectorySeparatorChar;

            if (__instance.voices != null)
            {
                for (int i = 0; i < __instance.voices.Length; i++)
                {
                    if (__instance.voices[i] != null)
                    {
                        AudioSwapper.LogAudioSourceDiagnostics(__instance.voices[i].GetComponent<AudioSource>(), "MandaloreVoice" + i);
                        if (__instance.voices[i].taunts != null)
                        {
                            Logging.Warn("[MandaloreAudioSwap] Voice " + i + " taunts array Length: " + __instance.voices[i].taunts.Length);
                            for (int t = 0; t < __instance.voices[i].taunts.Length; t++)
                            {
                                if (__instance.voices[i].taunts[t] != null)
                                    Logging.Warn("[MandaloreAudioSwap] Voice " + i + " taunt[" + t + "] clip name: " + __instance.voices[i].taunts[t].name);
                            }
                        }
                    }
                }
            }

            var inst = __instance;

            SwapClip(inst.voiceFull, mandaloreFolder, (clip) => { inst.voiceFull = clip; });
            SwapClip(inst.voiceFuller, mandaloreFolder, (clip) => { inst.voiceFuller = clip; });

            if (inst.voices != null && inst.voices.Length >= 2)
            {
                for (int v = 0; v < 2; v++)
                {
                    int vi = v;
                    SwapClip(inst.voices[vi].secondPhase, mandaloreFolder, (clip) => { inst.voices[vi].secondPhase = clip; });
                    SwapClip(inst.voices[vi].thirdPhase, mandaloreFolder, (clip) => { inst.voices[vi].thirdPhase = clip; });
                    SwapClip(inst.voices[vi].finalPhase, mandaloreFolder, (clip) => { inst.voices[vi].finalPhase = clip; });
                    SwapClip(inst.voices[vi].death, mandaloreFolder, (clip) => { inst.voices[vi].death = clip; });
                    SwapArray(inst.voices[vi].taunts, mandaloreFolder);
                }

                SetupDeathMonitor(inst);
            }
        }

        private static void SwapClip(AudioClip original, string folder, Action<AudioClip> assign)
        {
            if (original == null) return;
            AudioSwapper.SwapClipWithFileAsync(original, ResolveClipPath(original.name, folder), assign);
        }

        private static void SwapArray(AudioClip[] clips, string folder)
        {
            if (clips == null) return;
            for (int i = 0; i < clips.Length; i++)
            {
                int ix = i;
                if (clips[ix] == null) continue;
                AudioSwapper.SwapClipInArrayAsync(clips, ix, ResolveClipPath(clips[ix].name, folder));
            }
        }

        private static void SetupDeathMonitor(Mandalore instance)
        {
            if (instance == null || instance.voices == null || instance.voices.Length < 2)
                return;

            _deathSubtitleText = "<color=#9EE6FF>" + LanguageManager.CurrentLanguage.subtitles.subtitles_mandalore_defeated + "</color>";
        }

        [HarmonyPatch(typeof(Mandalore), "Update")]
        [HarmonyPostfix]
        public static void OnUpdatePostfix(Mandalore __instance)
        {
            if (__instance.voices != null && __instance.voices.Length >= 2 && __instance.voices[0] != null && __instance.voices[0].dying)
            {
                int id = __instance.GetInstanceID();
                if (_deathHandledInstances.Add(id) && !string.IsNullOrEmpty(_deathSubtitleText))
                {
                    MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(_deathSubtitleText, DeathAudioPlayer.AudioSource);
                }
            }
        }

        [HarmonyPatch(typeof(MandaloreVoice), "Death")]
        [HarmonyPostfix]
        public static void OnDeathPostfix(MandaloreVoice __instance)
        {
            if (__instance.death != null)
            {
                var aud = __instance.GetComponent<AudioSource>();
                if (aud != null)
                {
                    DeathAudioPlayer.PlayOneShot(__instance.death, aud.volume);
                    aud.Stop();
                }
            }
        }
    }
}
