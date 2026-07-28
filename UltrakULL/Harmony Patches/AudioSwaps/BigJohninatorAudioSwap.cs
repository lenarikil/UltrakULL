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
    [HarmonyPatch(typeof(Radio), "Start")]
    public static class BigJohninatorAudioSwap
    {
        private static readonly Dictionary<int, AudioClip[]> _savedSongs = new Dictionary<int, AudioClip[]>();
        private static readonly object _lock = new object();

        private static void SaveOriginals(Radio instance)
        {
            if (instance == null || instance.songs == null || instance.songs.Length == 0) return;
            int id = instance.GetInstanceID();
            if (!_savedSongs.ContainsKey(id))
            {
                _savedSongs[id] = (AudioClip[])instance.songs.Clone();
                Logging.Info($"[BigJohninatorAudioSwap] Saved {instance.songs.Length} original songs for instance {id}");
            }
        }

        private static void RestoreOriginals(Radio instance)
        {
            if (instance == null || instance.songs == null) return;
            int id = instance.GetInstanceID();

            lock (_lock)
            {
                if (_savedSongs.TryGetValue(id, out AudioClip[] saved))
                {
                    if (instance.songs.Length == saved.Length)
                    {
                        Array.Copy(saved, instance.songs, saved.Length);
                        Logging.Info($"[BigJohninatorAudioSwap] Restored {saved.Length} original songs for instance {id}");
                    }
                    _savedSongs.Remove(id);
                }
            }
        }

        [HarmonyPostfix]
        static void Radio_SwapSongs(Radio __instance)
        {
            SaveOriginals(__instance);

            if (__instance.songs == null || __instance.songs.Length == 0)
                return;

            if (LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value == "False" || isUsingEnglish())
                return;

            AudioSwapper.LogAudioSourceDiagnostics(__instance.GetComponent<AudioSource>(), "BigJohninatorRadio");
            string radioFolder = AudioSwapper.SpeechFolder + "BigJohninator" + Path.DirectorySeparatorChar;

            var inst = __instance;
            for (int i = 0; i < inst.songs.Length; i++)
            {
                int ix = i;
                var clip = inst.songs[ix];
                if (clip == null)
                    continue;

                string clipPath = radioFolder + clip.name;
                AudioSwapper.SwapClipInArrayAsync(inst.songs, ix, clipPath);
            }
        }

        /// <summary>
        /// Re-applies audio swaps to all existing Radio instances in the scene.
        /// </summary>
        public static void RebindExistingInstances()
        {
            Radio[] instances = UnityEngine.Object.FindObjectsOfType<Radio>(true);

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
                if (instance.songs == null || instance.songs.Length == 0) continue;
                SaveOriginals(instance);

                AudioSwapper.LogAudioSourceDiagnostics(instance.GetComponent<AudioSource>(), "BigJohninatorRadio");
                string radioFolder = AudioSwapper.SpeechFolder + "BigJohninator" + Path.DirectorySeparatorChar;

                for (int i = 0; i < instance.songs.Length; i++)
                {
                    int ix = i;
                    var clip = instance.songs[ix];
                    if (clip == null) continue;
                    string clipPath = radioFolder + clip.name;
                    AudioSwapper.SwapClipInArrayAsync(instance.songs, ix, clipPath);
                }
            }
        }
    }
}
