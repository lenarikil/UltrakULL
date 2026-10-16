using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.Networking;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.audio
{
    public static class AudioSwapper
    {
        // Folder where language audio is stored: <BepInEx.ConfigPath>/ultrakull/audio/<lang>/
        // Make writable because LanguageManager updates this when switching languages.
        public static string SpeechFolder { get; set; } = Path.Combine(Paths.ConfigPath, "ultrakull", "audio", LanguageManager.CurrentLanguage.metadata.langName) + Path.DirectorySeparatorChar;

        private class CoroutineHost : MonoBehaviour { }
        private class PendingClipLoad
        {
            public AudioClip Fallback;
            public Action<AudioClip> Callback;
        }

        // Holds the result of a background path enumeration so the coroutine can
        // observe completion without a blocking wait on the main thread.
        private class PathEnumerationState
        {
            public List<string> Paths;
            public Exception Error;
            public volatile bool Done;
        }

        private static CoroutineHost coroutineHost;
        private static readonly Dictionary<string, string> resolvedFileCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<PendingClipLoad>> pendingLoads = new Dictionary<string, List<PendingClipLoad>>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> missingPathWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> diagnosticLogKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> legacyAudioAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { Path.Combine("power", "powerSpecialWave1"), Path.Combine("power", "pow_SpecialIntro1") },
            { Path.Combine("power", "powerSpecialWave2"), Path.Combine("power", "pow_SpecialIntro2") },
            { Path.Combine("power", "powerSpecialWave3"), Path.Combine("power", "pow_SpecialIntro3") },
            { Path.Combine("power", "powerSpecialWave4"), Path.Combine("power", "pow_SpecialIntro4") }
        };
        private static int cacheGeneration;

        private static void EnsureHost()
        {
            if (coroutineHost != null)
                return;

            var go = new GameObject("AudioSwapper_CoroutineHost");
            UnityEngine.Object.DontDestroyOnLoad(go);
            coroutineHost = go.AddComponent<CoroutineHost>();
        }

        public static void SwapClipWithFileAsync(AudioClip sourceClip, string audioFilePath, Action<AudioClip> onComplete)
        {
            PreloadClipAsync(audioFilePath, sourceClip, onComplete);
        }

        public static void PreloadClipAsync(string audioFilePath, AudioClip fallback, Action<AudioClip> onComplete)
        {
            try
            {
                BindAudioConfigDefaults();

                if (isUsingEnglish())
                {
                    onComplete?.Invoke(fallback);
                    return;
                }

                EnsureHost();
                string filePath = ResolveAudioFilePath(audioFilePath);
                if (string.IsNullOrEmpty(filePath))
                {
                    WarnMissingOnce(audioFilePath);
                    onComplete?.Invoke(fallback);
                    return;
                }

                AudioClip cachedClip;
                if (clipCache.TryGetValue(filePath, out cachedClip) && cachedClip != null)
                {
                    onComplete?.Invoke(cachedClip);
                    return;
                }

                List<PendingClipLoad> callbacks;
                if (pendingLoads.TryGetValue(filePath, out callbacks))
                {
                    callbacks.Add(new PendingClipLoad { Fallback = fallback, Callback = onComplete });
                    return;
                }

                pendingLoads[filePath] = new List<PendingClipLoad>
                {
                    new PendingClipLoad { Fallback = fallback, Callback = onComplete }
                };
                coroutineHost.StartCoroutine(LoadClipCoroutine(filePath, fallback, cacheGeneration));
            }
            catch (Exception e)
            {
                Logging.Warn("[AudioSwap] PreloadClipAsync failed: " + e.Message);
                onComplete?.Invoke(fallback);
            }
        }

        public static void PreloadFolderAsync(string folderPath, Action onComplete = null)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    onComplete?.Invoke();
                    return;
                }

                IEnumerable<string> paths = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(IsSupportedAudioFile);
                PreloadPathsAsync(paths, onComplete);
            }
            catch (Exception e)
            {
                Logging.Warn("[AudioSwap] Failed to preload folder '" + folderPath + "': " + e.Message);
                onComplete?.Invoke();
            }
        }

        public static void PreloadPathsAsync(IEnumerable<string> audioPaths, Action onComplete = null)
        {
            if (audioPaths == null)
            {
                onComplete?.Invoke();
                return;
            }

            EnsureHost();
            coroutineHost.StartCoroutine(PreloadPathsCoroutine(audioPaths, onComplete));
        }

        /// <summary>
        /// Materializes the path list on a worker thread (directory scans and LINQ can be
        /// expensive) and then starts clip loading on the main thread.
        /// </summary>
        private static IEnumerator PreloadPathsCoroutine(IEnumerable<string> audioPaths, Action onComplete)
        {
            var state = new PathEnumerationState();

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    state.Paths = audioPaths
                        .Where(path => !string.IsNullOrEmpty(path))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (Exception e)
                {
                    state.Error = e;
                }
                finally
                {
                    state.Done = true;
                }
            });

            while (!state.Done)
                yield return null;

            if (state.Error != null)
            {
                Logging.Warn("[AudioSwap] Failed to enumerate audio paths: " + state.Error.Message);
                onComplete?.Invoke();
                yield break;
            }

            List<string> paths = state.Paths;
            if (paths == null || paths.Count == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            int remaining = paths.Count;
            Action<AudioClip> completeOne = delegate
            {
                remaining--;
                if (remaining <= 0)
                    onComplete?.Invoke();
            };

            foreach (string path in paths)
                PreloadClipAsync(path, null, completeOne);
        }

        public static void ClearCacheForLanguageChange()
        {
            resolvedFileCache.Clear();
            clipCache.Clear();
            pendingLoads.Clear();
            missingPathWarnings.Clear();
            diagnosticLogKeys.Clear();
            cacheGeneration++;
            Logging.Info("[AudioSwap] Cleared audio caches after language change.");
        }

        public static void SwapClipInArrayAsync(AudioClip[] clips, int index, string audioFilePath)
        {
            if (clips == null || index < 0 || index >= clips.Length)
                return;

            PreloadClipAsync(audioFilePath, clips[index], delegate(AudioClip clip)
            {
                try { clips[index] = clip; } catch { }
            });
        }

        public static void LogAudioSourceDiagnostics(AudioSource audioSource, string context)
        {
            BindAudioConfigDefaults();
            if (!DebugAudioSwapEnabled() || audioSource == null)
                return;

            string key = context + "|" + audioSource.GetInstanceID();
            if (!diagnosticLogKeys.Add(key))
                return;

            Component virtualFilter = audioSource.GetComponent("VirtualAudioFilter");
            Logging.Info("[AudioSwap] " + context + " AudioSource: " +
                         "spatialBlend=" + audioSource.spatialBlend +
                         ", minDistance=" + audioSource.minDistance +
                         ", maxDistance=" + audioSource.maxDistance +
                         ", rolloffMode=" + audioSource.rolloffMode +
                         ", dopplerLevel=" + audioSource.dopplerLevel +
                         ", hasVirtualAudioFilter=" + (virtualFilter != null));
        }

        private static IEnumerator LoadClipCoroutine(string filePath, AudioClip fallback, int generation)
        {
            AudioType type = TryGetAudioType(filePath);
            if (type == AudioType.UNKNOWN)
            {
                Logging.Warn("[AudioSwap] Unsupported audio type: " + filePath);
                CompletePendingLoad(filePath, null);
                yield break;
            }

            string fileUrl = "file://" + filePath;
            if (Logging.Verbose)
                Logging.Message("Async swapping: " + fileUrl);

            using (var req = UnityWebRequestMultimedia.GetAudioClip(fileUrl, type))
            {
                var op = req.SendWebRequest();
                while (!op.isDone)
                    yield return null;

#if UNITY_2020_1_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
#pragma warning disable 0618
                if (req.isNetworkError || req.isHttpError)
#pragma warning restore 0618
#endif
                {
                    Logging.Warn(req.error + "\n Expected path: " + filePath);
                    CompletePendingLoad(filePath, null);
                    yield break;
                }

                var newClip = DownloadHandlerAudioClip.GetContent(req);
                if (generation != cacheGeneration)
                    yield break;

                if (newClip != null)
                    clipCache[filePath] = newClip;

                CompletePendingLoad(filePath, newClip);
            }
        }

        private static void CompletePendingLoad(string filePath, AudioClip clip)
        {
            List<PendingClipLoad> callbacks;
            if (!pendingLoads.TryGetValue(filePath, out callbacks))
                return;

            pendingLoads.Remove(filePath);
            foreach (var pending in callbacks)
            {
                try { pending.Callback?.Invoke(clip ?? pending.Fallback); }
                catch (Exception e) { Logging.Warn("[AudioSwap] Completion callback failed: " + e.Message); }
            }
        }

        public static AudioType TryGetAudioType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return AudioType.UNKNOWN;

            var parts = path.Split('.');
            for (int i = 1; i <= parts.Length; i++)
            {
                var result = GetUnityAudioType(parts[parts.Length - i].ToLowerInvariant());
                if (result != AudioType.UNKNOWN)
                    return result;
            }
            return AudioType.UNKNOWN;
        }

        private static string ResolveAudioFilePath(string audioFilePath)
        {
            if (string.IsNullOrEmpty(audioFilePath))
                return null;

            string cached;
            if (resolvedFileCache.TryGetValue(audioFilePath, out cached))
                return cached;

            try
            {
                if (File.Exists(audioFilePath))
                {
                    string directPath = IsSupportedAudioFile(audioFilePath) ? audioFilePath : null;
                    resolvedFileCache[audioFilePath] = directPath;
                    return directPath;
                }

                string directory = Path.GetDirectoryName(audioFilePath);
                string fileName = Path.GetFileName(audioFilePath);
                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName) || !Directory.Exists(directory))
                {
                    resolvedFileCache[audioFilePath] = null;
                    return null;
                }

                string filePath = Directory.GetFiles(directory, fileName + ".*").FirstOrDefault(IsSupportedAudioFile);
                if (string.IsNullOrEmpty(filePath))
                    filePath = ResolveLegacyAlias(audioFilePath);

                resolvedFileCache[audioFilePath] = filePath;
                return filePath;
            }
            catch (Exception e)
            {
                Logging.Warn("[AudioSwap] Failed to resolve audio path '" + audioFilePath + "': " + e.Message);
                resolvedFileCache[audioFilePath] = null;
                return null;
            }
        }

        private static bool IsSupportedAudioFile(string filePath)
        {
            return TryGetAudioType(filePath) != AudioType.UNKNOWN;
        }

        private static void WarnMissingOnce(string audioFilePath)
        {
            string language = LanguageManager.CurrentLanguage != null ? LanguageManager.CurrentLanguage.metadata.langName : "";
            string key = language + "|" + (audioFilePath ?? string.Empty);
            if (missingPathWarnings.Add(key))
                Logging.Warn("[AudioSwap] Replacement not found for expected path: " + audioFilePath);
        }

        private static string ResolveLegacyAlias(string audioFilePath)
        {
            string normalized = audioFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            foreach (var alias in legacyAudioAliases)
            {
                string legacySuffix = alias.Key.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (!normalized.EndsWith(legacySuffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string replacementSuffix = alias.Value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                string replacementPath = normalized.Substring(0, normalized.Length - legacySuffix.Length) + replacementSuffix;
                if (File.Exists(replacementPath) && IsSupportedAudioFile(replacementPath))
                    return replacementPath;

                string directory = Path.GetDirectoryName(replacementPath);
                string fileName = Path.GetFileName(replacementPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    return Directory.GetFiles(directory, fileName + ".*").FirstOrDefault(IsSupportedAudioFile);
            }

            return null;
        }

        private static bool configBound = false;

        private static void BindAudioConfigDefaults()
        {
            if (configBound) return;
            configBound = true;

            try
            {
                LanguageManager.configFile.Bind("General", "debugAudioSwap", "False", (ConfigDescription)null);
                LanguageManager.configFile.Bind("General", "voiceSpatialOverride", "Original", (ConfigDescription)null);
                LanguageManager.configFile.Bind("General", "audioPreloadMode", "Scene", (ConfigDescription)null);
            }
            catch
            {
            }
        }

        private static bool DebugAudioSwapEnabled()
        {
            try
            {
                return Convert.ToBoolean(LanguageManager.configFile.Bind("General", "debugAudioSwap", "False", (ConfigDescription)null).Value);
            }
            catch
            {
                return false;
            }
        }

        private static AudioType GetUnityAudioType(string extension)
        {
            switch (extension)
            {
                case "aac":  return AudioType.ACC;
                case "aiff": return AudioType.AIFF;
                case "aif":  return AudioType.AIFF;
                case "aifc": return AudioType.AIFF;
                case "it":   return AudioType.IT;
                case "mod":  return AudioType.MOD;
                case "mp3":  return AudioType.MPEG;
                case "mpga": return AudioType.MPEG;
                case "mpeg": return AudioType.MPEG;
                case "ogg":  return AudioType.OGGVORBIS;
                case "s3m":  return AudioType.S3M;
                case "wav":  return AudioType.WAV;
                case "xm":   return AudioType.XM;
                case "xma":  return AudioType.XMA;
                case "vag":  return AudioType.VAG;
                default:     return AudioType.UNKNOWN;
            }
        }
    }
}
