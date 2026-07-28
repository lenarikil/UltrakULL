using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
    [HarmonyPatch(typeof(MinosPrime),"Start")]
    public class MinosPrimeAudioSwap
    {
        private static readonly Dictionary<string, AudioClip[]> _savedArrays = new Dictionary<string, AudioClip[]>();
        private static readonly Dictionary<string, AudioClip> _savedSingles = new Dictionary<string, AudioClip>();
        private static readonly object _lock = new object();

        private static readonly string[] ArrayFieldNames = {
            "riderKickVoice", "dropkickVoice", "dropAttackVoice",
            "boxingVoice", "comboVoice", "hurtVoice"
        };

        private static void SaveOriginals(MinosPrime instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            foreach (string fieldName in ArrayFieldNames)
            {
                var field = typeof(MinosPrime).GetField(fieldName);
                if (field == null) continue;
                AudioClip[] clips = field.GetValue(instance) as AudioClip[];
                if (clips == null) continue;
                string key = $"{id}_{fieldName}";
                if (!_savedArrays.ContainsKey(key))
                {
                    _savedArrays[key] = (AudioClip[])clips.Clone();
                    Logging.Info($"[MinosPrimeAudioSwap] Saved {clips.Length} original clips for {fieldName} (instance {id})");
                }
            }

            string singleKey = $"{id}_phaseChangeVoice";
            if (!_savedSingles.ContainsKey(singleKey) && instance.phaseChangeVoice != null)
            {
                _savedSingles[singleKey] = instance.phaseChangeVoice;
                Logging.Info($"[MinosPrimeAudioSwap] Saved original phaseChangeVoice '{instance.phaseChangeVoice.name}' (instance {id})");
            }
        }

        private static void RestoreOriginals(MinosPrime instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            lock (_lock)
            {
                foreach (string fieldName in ArrayFieldNames)
                {
                    string key = $"{id}_{fieldName}";
                    if (_savedArrays.TryGetValue(key, out AudioClip[] saved))
                    {
                        var field = typeof(MinosPrime).GetField(fieldName);
                        if (field == null) continue;
                        AudioClip[] current = field.GetValue(instance) as AudioClip[];
                        if (current != null && current.Length == saved.Length)
                        {
                            Array.Copy(saved, current, saved.Length);
                            Logging.Info($"[MinosPrimeAudioSwap] Restored {saved.Length} original clips for {fieldName} (instance {id})");
                        }
                    }
                }

                string singleKey = $"{id}_phaseChangeVoice";
                if (_savedSingles.TryGetValue(singleKey, out AudioClip savedClip))
                {
                    instance.phaseChangeVoice = savedClip;
                    Logging.Info($"[MinosPrimeAudioSwap] Restored phaseChangeVoice '{savedClip.name}' (instance {id})");
                }

                _savedArrays.Clear();
                _savedSingles.Clear();
            }
        }

        [HarmonyPostfix]
        public static void MinosPrime_VoiceSwap(ref MinosPrime __instance)
        {
            SaveOriginals(__instance);

            if(LanguageManager.configFile.Bind("General","activeDubbing","False").Value == "False" || isUsingEnglish())
                return;

            MinosPrime instance = __instance;
            AudioPreloadManager.EnsureCurrentScenePreloaded(delegate { ApplyVoiceSwap(instance); });
        }

        /// <summary>
        /// Re-applies audio swaps to all existing MinosPrime instances in the scene.
        /// Called when the language is changed without reloading the scene.
        /// </summary>
        public static void RebindExistingInstances()
        {
            MinosPrime[] instances = UnityEngine.Object.FindObjectsOfType<MinosPrime>(true);

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
                AudioPreloadManager.EnsureCurrentScenePreloaded(delegate { ApplyVoiceSwap(instance); });
            }
        }

        private static void ApplyVoiceSwap(MinosPrime __instance)
        {
            if (__instance == null)
                return;

            AudioSwapper.LogAudioSourceDiagnostics(__instance.GetComponent<AudioSource>(), "MinosPrime");
            string minosPrimeFolder =  AudioSwapper.SpeechFolder + "minosPrime" + Path.DirectorySeparatorChar;


            //Rider Kick (Die)
            AudioClip[] minosPrimeKick = __instance.riderKickVoice;
            for(int x = 0; x < minosPrimeKick.Length; x++)
            {
                int ix = x;
                string minosPrimeKickString = minosPrimeFolder + "minosPrimeDie" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(minosPrimeKick, ix, minosPrimeKickString);
            }
            
            //Dropkick (Judgement)
            AudioClip[] minosPrimeJudgement = __instance.dropkickVoice;
            for(int x = 0; x < minosPrimeJudgement.Length; x++)
            {
                int ix = x;
                string minosPrimeJudgementString = minosPrimeFolder + "minosPrimeJudgement" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(minosPrimeJudgement, ix, minosPrimeJudgementString);
            }
            
            //Crush attack (Crush)
            AudioClip[] minosPrimeCrush = __instance.dropAttackVoice;
            for(int x = 0; x < minosPrimeCrush.Length; x++)
            {
                int ix = x;
                string minosPrimeCrushString = minosPrimeFolder + "minosPrimeCrush" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(minosPrimeCrush, ix, minosPrimeCrushString);
            }
            
            //Punches/Boxing (Thy end is now)
            AudioClip[] minosPrimePunch = __instance.boxingVoice;
            for(int x = 0; x < minosPrimePunch.Length; x++)
            {
                int ix = x;
                string minosPrimePunchString = minosPrimeFolder + "minosPrimeThyEndIsNow" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(minosPrimePunch, ix, minosPrimePunchString);
            }
            
            //Combo (prepare thyself)
            AudioClip[] minosPrimeCombo = __instance.comboVoice;
            for(int x = 0; x < minosPrimeCombo.Length; x++)
            {
                int ix = x;
                string minosPrimeComboString = minosPrimeFolder + "minosPrimePrepareThyself" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(minosPrimeCombo, ix, minosPrimeComboString);
            }
            
            
            //Phase change - need to use ref otherwise it gets swapped back to original
            var inst = __instance;
            AudioClip tmpPhase = inst.phaseChangeVoice;
            string minosPrimePhaseChangeString = minosPrimeFolder + "minosPrimePhaseChange";
            AudioSwapper.SwapClipWithFileAsync(tmpPhase, minosPrimePhaseChangeString, (clip) => { try { inst.phaseChangeVoice = clip; } catch { } });
            
            
            //Hurt
            AudioClip[] minosPrimeHurt = __instance.hurtVoice;
            for(int x = 0; x < minosPrimeHurt.Length; x++)
            {
                int ix = x;
                string minosPrimeHurtString = minosPrimeFolder + "minosPrimeHurt" + (ix+1).ToString();
                AudioSwapper.SwapClipInArrayAsync(minosPrimeHurt, ix, minosPrimeHurtString);
            }
        }
    }
}
