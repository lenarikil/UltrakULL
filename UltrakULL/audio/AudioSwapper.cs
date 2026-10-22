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
        // Folder where the active language's audio lives (inside the language pack folder).
        // Make writable because LanguageManager updates this when switching languages.
        public static string SpeechFolder { get; set; } = LanguageManager.GetLanguageAssetFolder("audio") + Path.DirectorySeparatorChar;

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

        /// <summary>
        /// The game's own clip of every AudioSource the mod translates. A language
        /// switch hands the clip that is currently in the source (the translation of
        /// the previous language) to the swap as the fallback, so without remembering
        /// the original the built-in English and a missing replacement would both
        /// keep the previous language playing.
        /// </summary>
        private static readonly Dictionary<AudioSource, AudioClip> sourceOriginals = new Dictionary<AudioSource, AudioClip>();

        /// <summary>
        /// The game's own clip every translated clip replaced. The boss voices are
        /// single AudioSources whose clip the game replaces for every line, so a
        /// language switch needs this map to replace the clip of a line that is in
        /// the middle of playing.
        /// </summary>
        private static readonly Dictionary<AudioClip, AudioClip> translatedToOriginal = new Dictionary<AudioClip, AudioClip>();

        /// <summary>
        /// The translation path of the game's own clips, relative to the speech
        /// folder. The folder belongs to the active language, the rest of the path
        /// does not, so the path of the new language can be built from it.
        /// </summary>
        private static readonly Dictionary<AudioClip, string> originalToRelativePath = new Dictionary<AudioClip, string>();

        /// <summary>
        /// Replaces the clip of an AudioSource with the translation of the active
        /// language.
        ///
        /// The line that is playing keeps its playback time: the replacement starts
        /// at the same timer, so a language switch does not restart the cutscene
        /// voice-over. The game shows the subtitles from the playback time as well,
        /// so they stay in sync with the new clip on their own.
        /// </summary>
        /// <param name="source">The AudioSource whose clip is replaced.</param>
        /// <param name="audioFilePath">The audio file of the active language.</param>
        /// <param name="onSwapped">Called with the clip that was applied, for diagnostics.</param>
        public static void SwapSourceClip(AudioSource source, string audioFilePath, Action<AudioClip> onSwapped = null)
        {
            if (source == null)
                return;

            try
            {
                AudioClip original;
                if (!sourceOriginals.TryGetValue(source, out original))
                {
                    // The first swap sees the game's own clip, the later ones (a
                    // language switch) see the translation of the previous language.
                    original = source.clip;
                    sourceOriginals[source] = original;
                }

                // The game's own clip is the fallback, so the built-in English keeps
                // it and a missing replacement returns to it instead of staying on
                // the previous language.
                SwapClipWithFileAsync(original, audioFilePath, delegate(AudioClip newClip)
                {
                    RegisterTranslatedClip(newClip, original, audioFilePath);
                    ApplySourceClip(source, newClip);

                    if (onSwapped != null && newClip != null && !ReferenceEquals(newClip, original))
                        onSwapped(newClip);
                });
            }
            catch (Exception e)
            {
                Logging.Warn("[AudioSwap] SwapSourceClip failed: " + e.Message);
            }
        }

        /// <summary>
        /// Puts the given clip into the source, keeping the playback time and the
        /// play state, so the replacement continues where the previous language
        /// stopped.
        /// </summary>
        public static void ApplySourceClip(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null || ReferenceEquals(source.clip, clip))
                return;

            try
            {
                bool wasPlaying = source.isPlaying;
                float time = source.time;

                source.clip = clip;

                if (wasPlaying)
                {
                    // Assigning the clip alone does not reliably switch the sound
                    // that is playing, so the voice is triggered again with the new
                    // clip.
                    source.Play();
                }

                // The same timer as before. The seek happens in the same frame, so
                // nothing that is already rendered reaches the player.
                if (time > 0f)
                    source.time = time;
            }
            catch (Exception e)
            {
                Logging.Warn("[AudioSwap] ApplySourceClip failed: " + e.Message);
            }
        }

        /// <summary>
        /// The clip the game itself put into this source, or null when the mod has
        /// not translated this source yet. Callers that pick sources by clip name
        /// must use this one, because the name of the current clip changes with the
        /// language.
        /// </summary>
        public static AudioClip GetOriginalClip(AudioSource source)
        {
            AudioClip original;
            return source != null && sourceOriginals.TryGetValue(source, out original) ? original : null;
        }

        /// <summary>
        /// Drops the tracked AudioSources and clip mappings that are gone. Called
        /// when a scene is left, because the sources belong to that scene. The
        /// mappings survive a language change: they link the clips of the game to
        /// their translation paths and belong to no language.
        /// </summary>
        public static void PruneDeadSourceOriginals()
        {
            if (sourceOriginals.Count > 0)
            {
                List<AudioSource> deadSources = null;
                foreach (KeyValuePair<AudioSource, AudioClip> pair in sourceOriginals)
                {
                    if (pair.Key == null)
                    {
                        if (deadSources == null)
                            deadSources = new List<AudioSource>();
                        deadSources.Add(pair.Key);
                    }
                }

                if (deadSources != null)
                {
                    for (int i = 0; i < deadSources.Count; i++)
                        sourceOriginals.Remove(deadSources[i]);
                }
            }

            if (translatedToOriginal.Count > 0)
                PruneDeadClipKeys(translatedToOriginal);

            if (originalToRelativePath.Count > 0)
                PruneDeadClipKeys(originalToRelativePath);
        }

        private static void PruneDeadClipKeys<TValue>(Dictionary<AudioClip, TValue> map)
        {
            List<AudioClip> dead = null;
            foreach (KeyValuePair<AudioClip, TValue> pair in map)
            {
                if (pair.Key == null)
                {
                    if (dead == null)
                        dead = new List<AudioClip>();
                    dead.Add(pair.Key);
                }
            }

            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                map.Remove(dead[i]);
        }

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
                RegisterTranslatedClip(clip, clips[index], audioFilePath);
                try { clips[index] = clip; } catch { }
            });
        }

        /// <summary>
        /// Remembers which clip of the game a translated clip replaced, and where
        /// its translation lives. At a re-swap (a language switch) the fallback is
        /// the translation of the previous language, so the mapping is chased to
        /// the clip of the game itself.
        /// </summary>
        private static void RegisterTranslatedClip(AudioClip translated, AudioClip fallback, string fullPath)
        {
            if (translated == null || fallback == null || ReferenceEquals(translated, fallback))
                return;

            AudioClip root = fallback;
            AudioClip parent;
            int guard = 0;
            while (translatedToOriginal.TryGetValue(root, out parent) && parent != null && !ReferenceEquals(parent, root) && guard++ < 8)
                root = parent;

            if (ReferenceEquals(root, translated))
                return;

            string relative = GetPathRelativeToSpeechFolder(fullPath);
            if (string.IsNullOrEmpty(relative))
                return;

            translatedToOriginal[translated] = root;
            originalToRelativePath[root] = relative;
        }

        private static string GetPathRelativeToSpeechFolder(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return null;

            string folder = SpeechFolder;
            if (string.IsNullOrEmpty(folder))
                return null;

            return fullPath.StartsWith(folder, StringComparison.OrdinalIgnoreCase)
                ? fullPath.Substring(folder.Length)
                : null;
        }

        /// <summary>
        /// Applies the active language to the voice lines that are currently held by
        /// the boss voices.
        ///
        /// The boss voices are single AudioSources whose clip the game replaces for
        /// every line (taunts, hurt sounds, attack calls), so a language switch has
        /// to replace the clip of a line that is in the middle of playing itself -
        /// at the same playback time, so the voice does not restart. The line may
        /// be a translation of the previous language or the game's own clip (the
        /// switch may come from the built-in English), and it may be paused or
        /// virtualized by the game's audio manager - a source like that reports
        /// that it is not playing, so the play state is not used as a condition
        /// here: swapping the clip of an idle source changes nothing audible, the
        /// game assigns the next line itself.
        /// </summary>
        public static void ReapplyPlayingVoices()
        {
            if (translatedToOriginal.Count == 0 && originalToRelativePath.Count == 0)
                return;

            AudioSource[] sources = UnityEngine.Object.FindObjectsOfType<AudioSource>(true);
            int applied = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (source == null)
                    continue;

                AudioClip playing = source.clip;
                if (playing == null)
                    continue;

                // The game's own clip of the line: the playing clip itself (the
                // switch may come from the built-in English), or the original a
                // translation replaced. It is also the fallback: the built-in
                // English returns to it, and a missing file returns to it instead
                // of keeping the previous language.
                AudioClip original = playing;
                AudioClip parent;
                int guard = 0;
                while (translatedToOriginal.TryGetValue(original, out parent) && parent != null &&
                       !ReferenceEquals(parent, original) && guard++ < 8)
                {
                    original = parent;
                }

                string relative;
                if (!originalToRelativePath.TryGetValue(original, out relative) || string.IsNullOrEmpty(relative))
                    continue;

                try
                {
                    // The speech folder already belongs to the new language, the
                    // rest of the path does not.
                    PreloadClipAsync(SpeechFolder + relative, original, delegate(AudioClip clip)
                    {
                        ApplySourceClip(source, clip);
                    });
                    applied++;
                }
                catch (Exception e)
                {
                    Logging.Warn("[AudioSwap] ReapplyPlayingVoices failed: " + e.Message);
                }
            }

            if (applied > 0)
                Logging.Message("[AudioSwap] Applied the active language to " + applied + " playing voice line(s).");
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
