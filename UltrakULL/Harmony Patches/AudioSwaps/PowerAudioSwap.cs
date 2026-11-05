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
            if (!KnownInstances.Contains(__instance))
                KnownInstances.Add(__instance);

            bool dubbingEnabled = LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value != "False" && !CommonFunctions.isUsingEnglish();

            AudioSwapper.LogAudioSourceDiagnostics(__instance.GetComponent<AudioSource>(), "PowerVoiceController");
            Logging.Info($"[PowerAudioSwap] Processing PowerVoiceController instance {__instance.GetInstanceID()} in scene '{GetCurrentSceneName()}' (dubbing={dubbingEnabled})");

            if (dubbingEnabled)
            {
                // The Main Menu creates a PowerVoiceController (the menu V1 rig) but never
                // plays Power voice lines there. Preloading the whole "power" folder on every
                // menu entry loaded dozens of clips and caused a noticeable hitch, so rebinding
                // is skipped in that scene. Clips still load lazily if they are ever needed.
                if (GetCurrentSceneName() != "Main Menu")
                {
                    string powerFolder = Path.Combine(AudioSwapper.SpeechFolder, "power");
                    AudioSwapper.PreloadFolderAsync(powerFolder, () =>
                    {
                        RebindPowerClips(__instance);
                    });
                }
            }
            else
            {
                SaveAllOriginals(__instance);
            }
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
        /// Saves all original clips from all fields on a PowerVoiceController instance.
        /// Called when dubbing is disabled to ensure originals are captured for later restoration.
        /// </summary>
        private static void SaveAllOriginals(PowerVoiceController instance)
        {
            if (instance == null) return;
            int instanceId = instance.GetInstanceID();

            foreach (string fieldName in ArrayFields)
            {
                FieldInfo field = typeof(PowerVoiceController).GetField(fieldName, FieldFlags);
                if (field == null) continue;

                AudioClip[] clips = field.GetValue(instance) as AudioClip[];
                if (clips == null) continue;

                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null)
                        SaveOriginalClip(instanceId, fieldName, i, clips[i]);
                }
            }

            FieldInfo fallScreamField = typeof(PowerVoiceController).GetField("fallScream", FieldFlags);
            if (fallScreamField != null)
            {
                AudioClip fallScream = fallScreamField.GetValue(instance) as AudioClip;
                if (fallScream != null)
                    SaveOriginalClip(instanceId, "fallScream", fallScream);
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
                    if (Logging.Verbose)
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
                    if (Logging.Verbose)
                        Logging.Info($"[PowerAudioSwap] Saved original clip '{original.name}' for {key}");
                }
            }
        }

        /// <summary>
        /// The game's own clip that a field slot held before the first replacement,
        /// or null when none was saved. Re-swaps must build their lookup path from
        /// this clip: the clip that is currently in the field may be a translation
        /// of the previous language, and downloaded clips only carry a usable name
        /// because LoadClipCoroutine names them after their file.
        /// </summary>
        private static AudioClip GetSavedOriginalClip(int instanceId, string fieldName, int index)
        {
            lock (_originalClipsLock)
            {
                AudioClip clip;
                return _originalClips.TryGetValue($"{instanceId}:{fieldName}:{index}", out clip) ? clip : null;
            }
        }

        private static AudioClip GetSavedOriginalClip(int instanceId, string fieldName)
        {
            lock (_originalClipsLock)
            {
                AudioClip clip;
                return _originalClips.TryGetValue($"{instanceId}:{fieldName}", out clip) ? clip : null;
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

                // The lookup path and the fallback always come from the game's own clip
                // of this slot. The clip in the array may already be a translation of the
                // previous language, whose name does not identify the line reliably.
                AudioClip gameClip = GetSavedOriginalClip(instanceId, fieldName, index);
                if (gameClip == null)
                    gameClip = original;

                if (string.IsNullOrEmpty(gameClip.name))
                {
                    Logging.Warn($"[PowerAudioSwap] Original clip in field '{fieldName}'[{index}] has no name, skipping.");
                    continue;
                }

                string path = Path.Combine(folder, gameClip.name);
                if (Logging.Verbose)
                    Logging.Info($"[PowerAudioSwap] Attempting to preload clip '{gameClip.name}' from '{path}'.");
                AudioSwapper.PreloadClipAsync(path, gameClip, delegate (AudioClip newClip)
                {
                    if (newClip == null)
                    {
                        Logging.Warn($"[PowerAudioSwap] Preload returned null for clip '{gameClip.name}' in field '{fieldName}'. Keeping original.");
                        return;
                    }

                    if (ReferenceEquals(newClip, gameClip))
                    {
                        // No replacement exists for this line: the game's own clip is the
                        // documented fallback, not the translation of the previous language.
                        if (!ReferenceEquals(clips[index], gameClip))
                        {
                            clips[index] = gameClip;
                            field.SetValue(instance, clips);
                        }

                        if (Logging.Verbose)
                            Logging.Info($"[PowerAudioSwap] No localized clip found for '{gameClip.name}' in field '{fieldName}'. Restored the original.");
                        return;
                    }

                    if (Logging.Verbose)
                        Logging.Info($"[PowerAudioSwap] Successfully preloaded localized clip '{newClip.name}' for field '{fieldName}'.");
                    clips[index] = newClip;
                    field.SetValue(instance, clips);
                    // The registry lets ReapplyPlayingVoices resolve this line while
                    // it is playing, so a language switch swaps it at the same
                    // playback time.
                    AudioSwapper.RegisterTranslatedClip(newClip, gameClip, path);
                    // Note: the clip is NOT registered as handled. The suppression
                    // prefixes in PowerSubtitlesSwap skip the whole game method (the
                    // caller would not get a clip to play), and they check the index of
                    // the PREVIOUS line, so a populated set would silence every voice
                    // line after the first one. The set has always been empty in
                    // practice because downloaded clips used to have empty names.
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
            int instanceId = instance.GetInstanceID();
            SaveOriginalClip(instanceId, fieldName, original);

            // The fallback is the game's own clip, not the translation of the previous
            // language that may currently sit in the field.
            AudioClip gameClip = GetSavedOriginalClip(instanceId, fieldName);
            if (gameClip == null)
                gameClip = original;

            string fullPath = Path.Combine(folder, replacementName);
            if (Logging.Verbose)
                Logging.Info($"[PowerAudioSwap] Attempting to preload single clip '{gameClip.name}' from '{fullPath}'.");
            AudioSwapper.PreloadClipAsync(fullPath, gameClip, delegate (AudioClip newClip)
            {
                if (newClip == null)
                {
                    Logging.Warn($"[PowerAudioSwap] Preload failed for '{fieldName}'. Keeping original clip.");
                    return;
                }

                if (ReferenceEquals(newClip, gameClip))
                {
                    // No replacement exists: the game's own clip is the documented fallback.
                    if (!ReferenceEquals(field.GetValue(instance), gameClip))
                    {
                        try { field.SetValue(instance, gameClip); } catch { }
                    }

                    if (Logging.Verbose)
                        Logging.Info($"[PowerAudioSwap] No localized clip found for '{fieldName}'. Restored the original.");
                    return;
                }

                if (Logging.Verbose)
                    Logging.Info($"[PowerAudioSwap] Successfully replaced '{fieldName}' with localized clip '{newClip.name}'.");
                try { field.SetValue(instance, newClip); } catch { }
                AudioSwapper.RegisterTranslatedClip(newClip, gameClip, fullPath);
                // Not registered as handled - see the note in SwapArrayField.
            });
        }
    }

    [HarmonyPatch(typeof(PowerIntro), "Activate")]
    public static class PowerIntroSwap
    {
        private static readonly BindingFlags fieldFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static bool wasPerformedIntro = false;

        private static readonly Dictionary<int, AudioClip> _savedIntroOverrides = new Dictionary<int, AudioClip>();
        // Keyed by the persistent data object instead of the PowerIntro instance:
        // Activate() destroys its own component, but the persistent data survives it
        // and the game reuses it, so the originals have to be restorable afterwards.
        private static readonly Dictionary<PowerPersistentData, AudioClip[]> _savedRepeatedIntroClips = new Dictionary<PowerPersistentData, AudioClip[]>();
        private static readonly object _introLock = new object();

        private static void SaveIntroOriginals(PowerIntro instance)
        {
            if (instance == null) return;
            int id = instance.GetInstanceID();

            FieldInfo introOverrideField = typeof(PowerIntro).GetField("introOverride", fieldFlags);
            if (introOverrideField != null)
            {
                AudioClip introOverride = (AudioClip)introOverrideField.GetValue(instance);
                if (introOverride != null && !_savedIntroOverrides.ContainsKey(id))
                {
                    _savedIntroOverrides[id] = introOverride;
                    Logging.Info($"[PowerIntroSwap] Saved original introOverride '{introOverride.name}' for instance {id}");
                }
            }

            FieldInfo persistentDataField = typeof(PowerIntro).GetField("persistentData", fieldFlags);
            PowerPersistentData persistentData = (PowerPersistentData)persistentDataField.GetValue(instance);
            if (persistentData?.RepeatedIntroClips != null && persistentData.RepeatedIntroClips.Length > 0 && !_savedRepeatedIntroClips.ContainsKey(persistentData))
            {
                _savedRepeatedIntroClips[persistentData] = (AudioClip[])persistentData.RepeatedIntroClips.Clone();
                Logging.Info($"[PowerIntroSwap] Saved {persistentData.RepeatedIntroClips.Length} original RepeatedIntroClips for instance {id}");
            }
        }

        public static void RestoreOriginals()
        {
            lock (_introLock)
            {
                if (_savedIntroOverrides.Count == 0 && _savedRepeatedIntroClips.Count == 0)
                {
                    Logging.Info("[PowerIntroSwap] No original clips to restore.");
                    return;
                }

                PowerIntro[] instances = UnityEngine.Object.FindObjectsOfType<PowerIntro>(true);
                foreach (var instance in instances)
                {
                    if (instance == null) continue;
                    int id = instance.GetInstanceID();

                    if (_savedIntroOverrides.TryGetValue(id, out AudioClip savedOverride))
                    {
                        FieldInfo introOverrideField = typeof(PowerIntro).GetField("introOverride", fieldFlags);
                        if (introOverrideField != null)
                        {
                            introOverrideField.SetValue(instance, savedOverride);
                            Logging.Info($"[PowerIntroSwap] Restored introOverride '{savedOverride.name}' for instance {id}");
                        }
                    }
                }

                // The repeated intro clips live on the persistent data object, which
                // outlives the PowerIntro component, so they are restored even for
                // intros that already played.
                foreach (var pair in _savedRepeatedIntroClips)
                {
                    PowerPersistentData persistentData = pair.Key;
                    AudioClip[] savedClips = pair.Value;
                    if (persistentData?.RepeatedIntroClips != null && persistentData.RepeatedIntroClips.Length == savedClips.Length)
                    {
                        Array.Copy(savedClips, persistentData.RepeatedIntroClips, savedClips.Length);
                        Logging.Info($"[PowerIntroSwap] Restored {savedClips.Length} RepeatedIntroClips");
                    }
                }

                _savedIntroOverrides.Clear();
                _savedRepeatedIntroClips.Clear();
                Logging.Info("[PowerIntroSwap] All original clips restored and cache cleared.");
            }
        }

        public static void RebindExistingInstances()
        {
            if (LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value == "False" || CommonFunctions.isUsingEnglish())
            {
                RestoreOriginals();
                return;
            }

            PowerIntro[] instances = UnityEngine.Object.FindObjectsOfType<PowerIntro>(true);
            foreach (var instance in instances)
            {
                if (instance == null) continue;
                AudioPreloadManager.EnsureCurrentScenePreloaded(delegate
                {
                    ApplyIntroSwap(instance);
                });
            }
        }

        private static void ApplyIntroSwap(PowerIntro __instance)
        {
            if (__instance == null) return;

            FieldInfo introOverrideField = typeof(PowerIntro).GetField("introOverride", fieldFlags);
            AudioClip introOverride = (AudioClip)introOverrideField.GetValue(__instance);

            if (introOverride == null)
            {
                Logging.Warn("[PowerIntroSwap] introOverride is null, skipping.");
                return;
            }

            SwapIntroOverrideClip(__instance, introOverrideField, introOverride);

            FieldInfo persistentDataField = typeof(PowerIntro).GetField("persistentData", fieldFlags);
            PowerPersistentData persistentData = (PowerPersistentData)persistentDataField.GetValue(__instance);
            SwapRepeatedIntroClips(persistentData);
        }

        /// <summary>
        /// Swaps the intro override clip of an instance to the active language. The
        /// lookup path and the fallback come from the saved original clip: the field
        /// may already hold a translation of the previous language, whose name does
        /// not identify the line reliably.
        /// </summary>
        private static void SwapIntroOverrideClip(PowerIntro instance, FieldInfo introOverrideField, AudioClip introOverride)
        {
            AudioClip gameClip;
            lock (_introLock)
            {
                if (!_savedIntroOverrides.TryGetValue(instance.GetInstanceID(), out gameClip))
                    gameClip = null;
            }
            if (gameClip == null)
                gameClip = introOverride;

            if (string.IsNullOrEmpty(gameClip.name))
            {
                Logging.Warn("[PowerIntroSwap] Original introOverride clip has no name, skipping.");
                return;
            }

            string folder = Path.Combine(AudioSwapper.SpeechFolder, "power");
            string path = Path.Combine(folder, gameClip.name);

            AudioSwapper.PreloadClipAsync(path, gameClip, (AudioClip newClip) =>
            {
                if (newClip != null && newClip != gameClip)
                {
                    introOverrideField.SetValue(instance, newClip);
                    Logging.Warn("PowerIntroSwap: Successfully replaced introOverride with localized version: " + newClip.name);
                    AudioSwapper.RegisterTranslatedClip(newClip, gameClip, path);
                }
                else
                {
                    // No replacement exists: the game's own clip is the documented fallback.
                    if (!ReferenceEquals(introOverrideField.GetValue(instance), gameClip))
                        introOverrideField.SetValue(instance, gameClip);
                    Logging.Warn("PowerIntroSwap: No localized clip found for introOverride. Keeping original.");
                }
            });
        }

        /// <summary>
        /// Swaps the repeated intro clips of a persistent data object to the active
        /// language. The array on the persistent data may already hold translations
        /// of the previous language, so the lookups go through the saved originals.
        /// </summary>
        private static void SwapRepeatedIntroClips(PowerPersistentData persistentData)
        {
            if (persistentData == null || persistentData.RepeatedIntroClips == null || persistentData.RepeatedIntroClips.Length == 0)
                return;

            AudioClip[] savedClips;
            lock (_introLock)
            {
                if (!_savedRepeatedIntroClips.TryGetValue(persistentData, out savedClips))
                    savedClips = null;
            }

            string folder = Path.Combine(AudioSwapper.SpeechFolder, "power");
            for (int i = 0; i < persistentData.RepeatedIntroClips.Length; i++)
            {
                AudioClip gameClip = (savedClips != null && i < savedClips.Length && savedClips[i] != null)
                    ? savedClips[i]
                    : persistentData.RepeatedIntroClips[i];
                if (gameClip == null) continue;

                if (string.IsNullOrEmpty(gameClip.name))
                {
                    Logging.Warn($"[PowerIntroSwap] Original RepeatedIntroClips[{i}] has no name, skipping.");
                    continue;
                }

                int index = i;
                string clipPath = Path.Combine(folder, gameClip.name);
                AudioSwapper.PreloadClipAsync(clipPath, gameClip, (AudioClip newClip) =>
                {
                    if (newClip != null && newClip != gameClip)
                    {
                        persistentData.RepeatedIntroClips[index] = newClip;
                        Logging.Warn($"PowerIntroSwap: Successfully replaced RepeatedIntroClips[{index}] with localized version: " + newClip.name);
                        AudioSwapper.RegisterTranslatedClip(newClip, gameClip, clipPath);
                    }
                    else
                    {
                        // No replacement exists: the game's own clip is the documented fallback.
                        if (!ReferenceEquals(persistentData.RepeatedIntroClips[index], gameClip))
                            persistentData.RepeatedIntroClips[index] = gameClip;
                        Logging.Warn($"PowerIntroSwap: No localized clip found for RepeatedIntroClips[{index}]. Keeping original.");
                    }
                });
            }
        }

        [HarmonyPrefix]
        private static void Prefix(PowerIntro __instance)
        {
            FieldInfo persistentDataField = typeof(PowerIntro).GetField("persistentData", fieldFlags);
            PowerPersistentData persistentData = (PowerPersistentData)persistentDataField.GetValue(__instance);
            wasPerformedIntro = persistentData != null && persistentData.PerformedIntro && persistentData.RepeatedIntroOverrideClip;

            SaveIntroOriginals(__instance);

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
                        SwapIntroOverrideClip(__instance, introOverrideField, introOverride);
                    }
                    else
                    {
                        AudioPreloadManager.EnsureCurrentScenePreloaded();
                    }
                }
                else
                {
                    SwapIntroOverrideClip(__instance, introOverrideField, introOverride);
                }
            }

            Logging.Warn($"PowerIntro: persistentData.PerformedIntro = {persistentData?.PerformedIntro}");
            Logging.Warn($"PowerIntro: persistentData.RepeatedIntroOverrideClip = {persistentData?.RepeatedIntroOverrideClip}");

            SwapRepeatedIntroClips(persistentData);
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
