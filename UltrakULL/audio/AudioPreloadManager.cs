using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using UltrakULL.Harmony_Patches.AudioSwaps;
using UltrakULL.json;
using UnityEngine.SceneManagement;

namespace UltrakULL.audio
{
    public static class AudioPreloadManager
    {
        private static readonly Dictionary<string, string[]> SceneFolders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Level 3-2", new[] { "gabrielBossFirst" } },
            { "Level 6-2", new[] { "gabrielBossSecond" } },
            { "Level P-1", new[] { "minosPrime" } },
            { "Level P-2", new[] { "sisyphusPrime" } },
            { "Level 4-3", new[] { "mandalore" } },
            { "Level 8-3", new[] { "power" } }
        };

        private static readonly HashSet<string> completedScenePreloads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> pendingScenePreloads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<Action>> pendingSceneCallbacks = new Dictionary<string, List<Action>>(StringComparer.OrdinalIgnoreCase);
        private static bool initialized;
        private static bool startupAllStarted;
        private static bool startupAllCompleted;
        private static readonly List<Action> pendingStartupAllCallbacks = new List<Action>();

        public static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void OnLanguageChanged()
        {
            completedScenePreloads.Clear();
            pendingScenePreloads.Clear();
            pendingSceneCallbacks.Clear();
            startupAllStarted = false;
            startupAllCompleted = false;
            pendingStartupAllCallbacks.Clear();
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!ActiveDubbingEnabled() || CommonFunctions.isUsingEnglish())
                return;

            string preloadMode = GetPreloadMode();
            if (preloadMode.Equals("Off", StringComparison.OrdinalIgnoreCase))
                return;

            if (preloadMode.Equals("StartupAll", StringComparison.OrdinalIgnoreCase))
            {
                PreloadAllAudioOnce();
                return;
            }

            EnsureScenePreloaded(scene.name);
        }

        public static void PreloadCurrentScene(Action onComplete = null)
        {
            EnsureCurrentScenePreloaded(onComplete);
        }

        public static void PreloadScene(string sceneName, Action onComplete = null)
        {
            EnsureScenePreloaded(sceneName, onComplete);
        }

        public static void EnsureCurrentScenePreloaded(Action onReady = null)
        {
            EnsureScenePreloaded(CommonFunctions.GetCurrentSceneName(), onReady);
        }

        public static bool IsScenePreloaded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            string preloadMode = GetPreloadMode();
            if (preloadMode.Equals("StartupAll", StringComparison.OrdinalIgnoreCase))
                return startupAllCompleted;

            return completedScenePreloads.Contains(GetPreloadKey(sceneName));
        }

        public static void EnsureScenePreloaded(string sceneName, Action onReady = null)
        {
            if (string.IsNullOrEmpty(sceneName) || !ActiveDubbingEnabled() || CommonFunctions.isUsingEnglish())
            {
                onReady?.Invoke();
                return;
            }

            string preloadMode = GetPreloadMode();
            if (preloadMode.Equals("Off", StringComparison.OrdinalIgnoreCase))
            {
                onReady?.Invoke();
                return;
            }

            if (preloadMode.Equals("StartupAll", StringComparison.OrdinalIgnoreCase))
            {
                PreloadAllAudioOnce(onReady);
                return;
            }

            string preloadKey = GetPreloadKey(sceneName);
            if (completedScenePreloads.Contains(preloadKey))
            {
                onReady?.Invoke();
                return;
            }

            if (pendingScenePreloads.Contains(preloadKey))
            {
                if (onReady != null)
                {
                    List<Action> callbacks;
                    if (!pendingSceneCallbacks.TryGetValue(preloadKey, out callbacks))
                    {
                        callbacks = new List<Action>();
                        pendingSceneCallbacks[preloadKey] = callbacks;
                    }
                    callbacks.Add(onReady);
                }
                return;
            }

            pendingScenePreloads.Add(preloadKey);
            if (onReady != null)
                pendingSceneCallbacks[preloadKey] = new List<Action> { onReady };

            Logging.Info("[AudioPreload] Preloading audio files for scene '" + sceneName + "'.");
            AudioSwapper.PreloadPathsAsync(GetScenePreloadPaths(sceneName), delegate
            {
                pendingScenePreloads.Remove(preloadKey);
                completedScenePreloads.Add(preloadKey);
                Logging.Info("[AudioPreload] Finished preloading scene '" + sceneName + "'.");
                PowerAudioSwap.RebindCachedInstances();

                List<Action> callbacks;
                if (!pendingSceneCallbacks.TryGetValue(preloadKey, out callbacks))
                    return;

                pendingSceneCallbacks.Remove(preloadKey);
                foreach (var callback in callbacks)
                {
                    try { callback?.Invoke(); }
                    catch (Exception e) { Logging.Warn("[AudioPreload] Completion callback failed: " + e.Message); }
                }
            });
        }

        /// <summary>
        /// Lazy enumeration so directory scans happen when the path list is materialized
        /// on the audio worker thread rather than on the main thread.
        /// </summary>
        private static IEnumerable<string> GetScenePreloadPaths(string sceneName)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string[] folders;
            if (SceneFolders.TryGetValue(sceneName, out folders))
            {
                foreach (string folder in folders)
                {
                    string folderPath = Path.Combine(AudioSwapper.SpeechFolder, folder);
                    if (!Directory.Exists(folderPath))
                        continue;

                    foreach (string filePath in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        if (AudioSwapper.TryGetAudioType(filePath) != UnityEngine.AudioType.UNKNOWN && seen.Add(filePath))
                            yield return filePath;
                    }
                }
            }

            List<SubtitledObjectReference> references;
            if (SubtitledAudioSourcesReplacer.Config != null &&
                SubtitledAudioSourcesReplacer.Config.Scenes != null &&
                SubtitledAudioSourcesReplacer.Config.Scenes.TryGetValue(sceneName, out references))
            {
                foreach (var reference in references)
                {
                    if (!string.IsNullOrEmpty(reference.AudioPath))
                    {
                        string path = Path.Combine(AudioSwapper.SpeechFolder, reference.AudioPath);
                        if (seen.Add(path))
                            yield return path;
                    }
                }
            }
        }

        private static void PreloadAllAudioOnce(Action onComplete = null)
        {
            if (startupAllCompleted)
            {
                onComplete?.Invoke();
                return;
            }

            if (onComplete != null)
                pendingStartupAllCallbacks.Add(onComplete);

            if (startupAllStarted)
                return;

            startupAllStarted = true;
            string folder = AudioSwapper.SpeechFolder;
            if (!Directory.Exists(folder))
            {
                startupAllCompleted = true;
                FlushStartupAllCallbacks();
                return;
            }

            IEnumerable<string> paths = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(path => AudioSwapper.TryGetAudioType(path) != UnityEngine.AudioType.UNKNOWN);

            Logging.Info("[AudioPreload] StartupAll preload started.");
            AudioSwapper.PreloadPathsAsync(paths, delegate
            {
                startupAllCompleted = true;
                Logging.Info("[AudioPreload] StartupAll preload finished.");
                PowerAudioSwap.RebindCachedInstances();
                FlushStartupAllCallbacks();
            });
        }

        private static void FlushStartupAllCallbacks()
        {
            Action[] callbacks = pendingStartupAllCallbacks.ToArray();
            pendingStartupAllCallbacks.Clear();
            foreach (var callback in callbacks)
            {
                try { callback?.Invoke(); }
                catch (Exception e) { Logging.Warn("[AudioPreload] StartupAll completion callback failed: " + e.Message); }
            }
        }

        private static string GetPreloadKey(string sceneName)
        {
            string language = LanguageManager.CurrentLanguage != null ? LanguageManager.CurrentLanguage.metadata.langName : "";
            return language + "|" + sceneName;
        }

        private static string GetPreloadMode()
        {
            try
            {
                return LanguageManager.configFile.Bind("General", "audioPreloadMode", "Scene", (ConfigDescription)null).Value;
            }
            catch
            {
                return "Scene";
            }
        }

        private static bool ActiveDubbingEnabled()
        {
            try
            {
                return LanguageManager.configFile.Bind("General", "activeDubbing", "False", (ConfigDescription)null).Value != "False";
            }
            catch
            {
                return false;
            }
        }
    }
}
