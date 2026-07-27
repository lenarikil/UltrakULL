using BepInEx.Configuration;
using HarmonyLib;
using ScriptableObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using static UltrakULL.CommonFunctions;
using static UltrakULL.ReflectionUtils;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
    [HarmonyPatch(typeof(PowerVoiceController), "Awake")]
    public static class PowerAudioSwap
    {
        private static readonly BindingFlags FieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly List<PowerVoiceController> KnownInstances = new List<PowerVoiceController>();

        private static readonly string[] ArrayFields =
        {
            "intro",
            "enrage",
            "taunt",
            "cheapShot",
            "hurt",
            "hurtBig",
            "death",
            "rapier",
            "greatsword",
            "spear",
            "spearThrow",
            "glaive",
            "glaiveThrow"
        };

        // Stores original (non-localized) clips so they can be restored when dubbing is disabled.
        // Key: $"{instanceId}:{fieldName}:{index}" for array fields, or $"{instanceId}:{fieldName}" for single fields.
        private static readonly Dictionary<string, AudioClip> _originalClips = new Dictionary<string, AudioClip>();
        private static readonly object _originalClipsLock = new object();

        [HarmonyPostfix]
        private static void Postfix(PowerVoiceController __instance)
        {
            if (LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value == "False" || CommonFunctions.isUsingEnglish())
                return;

            if (!KnownInstances.Contains(__instance))
                KnownInstances.Add(__instance);

            AudioSwapper.LogAudioSourceDiagnostics(__instance.GetComponent<AudioSource>(), "PowerVoiceController");
            // Detailed logging for debugging audio swaps
            Logging.Info($"[PowerAudioSwap] Processing PowerVoiceController instance {__instance.GetInstanceID()} in scene '{GetCurrentSceneName()}'");

            string powerFolder = Path.Combine(AudioSwapper.SpeechFolder, "power");
            AudioSwapper.PreloadFolderAsync(powerFolder, () =>
            {
                RebindPowerClips(__instance);
            });
        }

        public static void RebindCachedInstances()
        {
            for (int i = KnownInstances.Count - 1; i >= 0; i--)
            {
                PowerVoiceController instance = KnownInstances[i];
                if (instance == null)
                {
                    KnownInstances.RemoveAt(i);
                    continue;
                }

                RebindPowerClips(instance);
            }
        }

        /// <summary>
        /// Saves the original clip before it gets replaced by a localized version.
        /// </summary>
        private static void SaveOriginalClip(int instanceId, string fieldName, int index, AudioClip original)
        {
            if (original == null) return;
            string key = $"{instanceId}:{fieldName}:{index}";
            lock (_originalClipsLock)
            {
                if (!_originalClips.ContainsKey(key))
                {
                    _originalClips[key] = original;
                    Logging.Info($"[PowerAudioSwap] Saved original clip '{original.name}' for {key}");
                }
            }
        }

        /// <summary>
        /// Saves the original single-field clip before it gets replaced.
        /// </summary>
        private static void SaveOriginalClip(int instanceId, string fieldName, AudioClip original)
        {
            if (original == null) return;
            string key = $"{instanceId}:{fieldName}";
            lock (_originalClipsLock)
            {
                if (!_originalClips.ContainsKey(key))
                {
                    _originalClips[key] = original;
                    Logging.Info($"[PowerAudioSwap] Saved original clip '{original.name}' for {key}");
                }
            }
        }

        /// <summary>
        /// Restores all saved original clips to their respective PowerVoiceController instances.
        /// Called when dubbing is disabled or English is selected.
        /// </summary>
        public static void RestoreOriginalClips()
        {
            lock (_originalClipsLock)
            {
                if (_originalClips.Count == 0)
                {
                    Logging.Info("[PowerAudioSwap] No original clips to restore.");
                    return;
                }

                Logging.Info($"[PowerAudioSwap] Restoring {_originalClips.Count} original clip(s)...");

                // Group saved clips by instanceId for efficient restoration
                var byInstance = new Dictionary<int, Dictionary<string, AudioClip>>();
                foreach (var kvp in _originalClips)
                {
                    string[] parts = kvp.Key.Split(':');
                    if (parts.Length < 2) continue;
                    int instanceId = int.Parse(parts[0]);
                    string fieldKey = parts[1] + (parts.Length >= 3 ? ":" + parts[2] : "");

                    if (!byInstance.ContainsKey(instanceId))
                        byInstance[instanceId] = new Dictionary<string, AudioClip>();
                    byInstance[instanceId][fieldKey] = kvp.Value;
                }

                foreach (var instanceGroup in byInstance)
                {
                    // Find the instance by instance ID
                    PowerVoiceController target = null;
                    foreach (var known in KnownInstances)
                    {
                        if (known != null && known.GetInstanceID() == instanceGroup.Key)
                        {
                            target = known;
                            break;
                        }
                    }

                    if (target == null)
                    {
                        Logging.Warn($"[PowerAudioSwap] Instance {instanceGroup.Key} no longer exists, skipping.");
                        continue;
                    }

                    foreach (var fieldEntry in instanceGroup.Value)
                    {
                        string fieldKey = fieldEntry.Key;
                        AudioClip originalClip = fieldEntry.Value;

                        // Check if it's an array field (format: "fieldName:index") or single field (format: "fieldName")
                        int colonIndex = fieldKey.IndexOf(':');
                        if (colonIndex >= 0)
                        {
                            // Array field
                            string fieldName = fieldKey.Substring(0, colonIndex);
                            int arrayIndex = int.Parse(fieldKey.Substring(colonIndex + 1));

                            FieldInfo field = typeof(PowerVoiceController).GetField(fieldName, FieldFlags);
                            if (field != null && field.GetValue(target) is AudioClip[] clips && arrayIndex < clips.Length)
                            {
                                clips[arrayIndex] = originalClip;
                                field.SetValue(target, clips);
                                Logging.Info($"[PowerAudioSwap] Restored original clip '{originalClip.name}' to {fieldName}[{arrayIndex}]");
                            }
                        }
                        else
                        {
                            // Single field
                            FieldInfo field = typeof(PowerVoiceController).GetField(fieldKey, FieldFlags);
                            if (field != null)
                            {
                                field.SetValue(target, originalClip);
                                Logging.Info($"[PowerAudioSwap] Restored original clip '{originalClip.name}' to {fieldKey}");
                            }
                        }
                    }
                }

                _originalClips.Clear();
                Logging.Info("[PowerAudioSwap] All original clips restored and cache cleared.");
            }
        }

        /// <summary>
        /// Re-applies audio swaps to all existing PowerVoiceController instances in the scene.
        /// Called when the language is changed without reloading the scene.
        /// If dubbing is disabled or English is selected, restores original clips instead.
        /// </summary>
        public static void RebindExistingInstances()
        {
            if (LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value == "False" || CommonFunctions.isUsingEnglish())
            {
                RestoreOriginalClips();
                return;
            }

            // First, scan for any PowerVoiceController instances not yet in KnownInstances
            PowerVoiceController[] allInstances = UnityEngine.Object.FindObjectsOfType<PowerVoiceController>(true);
            foreach (var instance in allInstances)
            {
                if (instance != null && !KnownInstances.Contains(instance))
                {
                    KnownInstances.Add(instance);
                }
            }

            // Then rebind all known instances
            RebindCachedInstances();
        }

        private static bool ShouldUseScenePreload()
        {
            if (GetCurrentSceneName() == "Level 8-3")
                return true;

            try
            {
                return LanguageManager.configFile.Bind<string>("General", "audioPreloadMode", "Scene", (ConfigDescription)null).Value == "StartupAll";
            }
            catch
            {
                return false;
            }
        }

        private static void RebindPowerClips(PowerVoiceController __instance)
        {
            string folder = Path.Combine(AudioSwapper.SpeechFolder, "power");
            foreach (string fieldName in ArrayFields)
                SwapArrayField(__instance, fieldName, folder);

            SwapSingleField(__instance, "fallScream", folder, "pow_ScreamContinuous");
        }

        private static void SwapArrayField(PowerVoiceController instance, string fieldName, string folder)
        {
            FieldInfo field = typeof(PowerVoiceController).GetField(fieldName, FieldFlags);
            if (field == null)
            {
                Logging.Error("[AudioSwap] PowerVoiceController field not found: " + fieldName);
                return;
            }

            AudioClip[] clips = field.GetValue(instance) as AudioClip[];
            if (clips == null)
            {
                Logging.Warn($"[PowerAudioSwap] Field '{fieldName}' returned null array.");
                return;
            }

            int instanceId = instance.GetInstanceID();

            for (int i = 0; i < clips.Length; i++)
            {
                int index = i;
                AudioClip original = clips[index];
                if (original == null)
                {
                    Logging.Warn($"[PowerAudioSwap] Clip at index {index} in field '{fieldName}' is null, skipping.");
                    continue;
                }

                // Save the original clip before attempting replacement, so it can be restored later
                // when dubbing is disabled or English is selected.
                SaveOriginalClip(instanceId, fieldName, index, original);

                string path = Path.Combine(folder, original.name);
                Logging.Info($"[PowerAudioSwap] Attempting to preload clip '{original.name}' from '{path}'.");
                AudioSwapper.PreloadClipAsync(path, original, delegate (AudioClip newClip)
                {
                    if (newClip != null)
                    {
                        // Only register as handled if the clip was actually replaced with a different (localized) clip.
                        // When using English or a language without dubbing, PreloadClipAsync returns the original clip
                        // as fallback. Registering the original clip as handled would suppress its playback.
                        if (newClip != original)
                        {
                            Logging.Info($"[PowerAudioSwap] Successfully preloaded localized clip '{newClip.name}' for field '{fieldName}'.");
                            clips[index] = newClip;
                            field.SetValue(instance, clips);
                            UltrakULL.Harmony_Patches.Subtitles.PowerSubtitlesSwap.RegisterPowerClipAsHandled(newClip.name);
                        }
                        else
                        {
                            Logging.Info($"[PowerAudioSwap] No localized clip found for '{original.name}' in field '{fieldName}'. Keeping original.");
                        }
                    }
                    else
                    {
                        Logging.Warn($"[PowerAudioSwap] Preload returned null for clip '{original.name}' in field '{fieldName}'. Keeping original.");
                    }
                });
            }
        }

        private static void SwapSingleField(PowerVoiceController instance, string fieldName, string folder, string replacementName)
        {
            FieldInfo field = typeof(PowerVoiceController).GetField(fieldName, FieldFlags);
            if (field == null)
            {
                Logging.Error("[AudioSwap] PowerVoiceController field not found: " + fieldName);
                return;
            }

            AudioClip original = field.GetValue(instance) as AudioClip;
            if (original == null)
            {
                Logging.Warn($"[PowerAudioSwap] Single field '{fieldName}' is null, nothing to swap.");
                return;
            }

            // Save the original clip before attempting replacement.
            SaveOriginalClip(instance.GetInstanceID(), fieldName, original);

            string fullPath = Path.Combine(folder, replacementName);
            Logging.Info($"[PowerAudioSwap] Attempting to preload single clip '{original.name}' from '{fullPath}'.");
            AudioSwapper.PreloadClipAsync(fullPath, original, delegate (AudioClip newClip)
            {
                if (newClip != null)
                {
                    // Only register as handled if the clip was actually replaced with a different (localized) clip.
                    if (newClip != original)
                    {
                        Logging.Info($"[PowerAudioSwap] Successfully replaced '{fieldName}' with localized clip '{newClip.name}'.");
                        try { field.SetValue(instance, newClip); } catch { }
                        UltrakULL.Harmony_Patches.Subtitles.PowerSubtitlesSwap.RegisterPowerClipAsHandled(newClip.name);
                    }
                    else
                    {
                        Logging.Info($"[PowerAudioSwap] No localized clip found for '{fieldName}'. Keeping original.");
                    }
                }
                else
                {
                    Logging.Warn($"[PowerAudioSwap] Preload failed for '{fieldName}'. Keeping original clip.");
                }
            });
        }
    }

    [HarmonyPatch(typeof(PowerIntro), "Activate")]
    public static class PowerIntroSwap
    {
        private static readonly BindingFlags fieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static bool wasPerformedIntro = false;

        [HarmonyPrefix]
        private static void Prefix(PowerIntro __instance)
        {
            // early return 전에 먼저 세팅
            FieldInfo persistentDataField = typeof(PowerIntro).GetField("persistentData", fieldFlags);
            PowerPersistentData persistentData = (PowerPersistentData)persistentDataField.GetValue(__instance);
            wasPerformedIntro = persistentData != null && persistentData.PerformedIntro && persistentData.RepeatedIntroOverrideClip;

            if (LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value == "False" || CommonFunctions.isUsingEnglish())
                return;

            FieldInfo introOverrideField = typeof(PowerIntro).GetField("introOverride", fieldFlags);
            AudioClip introOverride = (AudioClip)introOverrideField.GetValue(__instance);

            Logging.Warn("PowerIntro: introOverride = " + (introOverride != null ? introOverride.name : "null"));

            if (introOverride == null)
            {
                Logging.Warn("PowerIntro: introOverride is null, will use PowerVoiceController.Intro()");
            }
            else
            {
                string folder = Path.Combine(AudioSwapper.SpeechFolder, "power");
                string path = Path.Combine(folder, introOverride.name);

                AudioSwapper.LogAudioSourceDiagnostics(__instance.GetComponent<AudioSource>(), "PowerIntro (introOverride)");

                bool shouldUseScenePreload = false;
                try
                {
                    shouldUseScenePreload = LanguageManager.configFile.Bind<string>("General", "audioPreloadMode", "Scene", (ConfigDescription)null).Value == "StartupAll";
                }
                catch { shouldUseScenePreload = false; }

                if (shouldUseScenePreload || GetCurrentSceneName() == "Level 8-3")
                {
                    if (AudioPreloadManager.IsScenePreloaded(GetCurrentSceneName()))
                    {
                        AudioSwapper.PreloadClipAsync(path, introOverride, (AudioClip newClip) =>
                        {
                            if (newClip != null && newClip != introOverride)
                            {
                                introOverrideField.SetValue(__instance, newClip);
                                Logging.Warn("PowerIntro: Successfully replaced introOverride with localized version: " + newClip.name);
                                UltrakULL.Harmony_Patches.Subtitles.PowerSubtitlesSwap.RegisterPowerClipAsHandled(newClip.name);
                            }
                            else
                            {
                                Logging.Warn("PowerIntro: No localized clip found for introOverride. Keeping original.");
                            }
                        });
                    }
                    else
                    {
                        AudioPreloadManager.EnsureCurrentScenePreloaded();
                    }
                }
                else
                {
                    AudioSwapper.PreloadClipAsync(path, introOverride, (AudioClip newClip) =>
                    {
                        if (newClip != null && newClip != introOverride)
                        {
                            introOverrideField.SetValue(__instance, newClip);
                            Logging.Warn("PowerIntro: Successfully replaced introOverride with localized version: " + newClip.name);
                            UltrakULL.Harmony_Patches.Subtitles.PowerSubtitlesSwap.RegisterPowerClipAsHandled(newClip.name);
                        }
                        else
                        {
                            Logging.Warn("PowerIntro: No localized clip found for introOverride. Keeping original.");
                        }
                    });
                }
            }

            Logging.Warn($"PowerIntro: persistentData.PerformedIntro = {persistentData?.PerformedIntro}");
            Logging.Warn($"PowerIntro: persistentData.RepeatedIntroOverrideClip = {persistentData?.RepeatedIntroOverrideClip}");

            if (persistentData != null && persistentData.RepeatedIntroClips != null && persistentData.RepeatedIntroClips.Length > 0)
            {
                string folder = Path.Combine(AudioSwapper.SpeechFolder, "power");
                for (int i = 0; i < persistentData.RepeatedIntroClips.Length; i++)
                {
                    AudioClip originalClip = persistentData.RepeatedIntroClips[i];
                    if (originalClip == null) continue;

                    string path = Path.Combine(folder, originalClip.name);
                    AudioSwapper.PreloadClipAsync(path, originalClip, (AudioClip newClip) =>
                    {
                        if (newClip != null && newClip != originalClip)
                        {
                            persistentData.RepeatedIntroClips[i] = newClip;
                            Logging.Warn($"PowerIntro: Successfully replaced RepeatedIntroClips[{i}] with localized version: " + newClip.name);
                            UltrakULL.Harmony_Patches.Subtitles.PowerSubtitlesSwap.RegisterPowerClipAsHandled(newClip.name);
                        }
                        else
                        {
                            Logging.Warn($"PowerIntro: No localized clip found for RepeatedIntroClips[{i}]. Keeping original.");
                        }
                    });
                }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(PowerIntro __instance)
        {
            if (CommonFunctions.isUsingEnglish()) return;
            if (!wasPerformedIntro) return;

            SubtitledAudioSource subtitledSource = __instance.GetComponent<SubtitledAudioSource>();
            if (subtitledSource == null) return;

            var currentData = (SubtitledAudioSource.SubtitleData)typeof(SubtitledAudioSource)
                .GetField("subtitles", fieldFlags)
                .GetValue(subtitledSource);

            if (currentData?.lines == null || currentData.lines.Length == 0) return;

            SetPrivate(subtitledSource, typeof(SubtitledAudioSource), "subtitles",
                new SubtitledAudioSource.SubtitleData
                {
                    lines = new[] { currentData.lines[0] }
                });
        }
    }
}
