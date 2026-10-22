using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UltrakULL.ReflectionUtils;
using static UltrakULL.CommonFunctions;
using static UltrakULL.audio.AudioSwapper;
using static System.IO.Path;

namespace UltrakULL
{
    public static class SubtitledAudioSourcesReplacer
    {
        public static string SpeechFolder = LanguageManager.GetLanguageAssetFolder("audio") + DirectorySeparatorChar;
        
        public static SubtitledSourcesConfig Config;
        private static readonly HashSet<string> subtitleTimingWarnings = new HashSet<string>();

        public static async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            await Task.Delay(250);
            ReplaceSubsAndAudio();
        }

        public static void ReplaceSubsAndAudio()
        {
            AudioPreloadManager.EnsureCurrentScenePreloaded(ApplySubsAndAudio);
        }

        private static void ApplySubsAndAudio()
        {
            if (!TryLoadMetadata(out var objectReferences)) 
                return;

            foreach (var objectReference in objectReferences)
            {
                foreach (var gameObject in objectReference.Objects)
                {
                    var obj = GetObject(gameObject);
                    if (obj == null)
                    {
                        Logging.Warn($"[UAK] GetObject('{gameObject}') return NULL");
                        continue;
                    }

                    var subtitledAudioSource = obj.GetComponent<SubtitledAudioSource>();
                    var audioSource = obj.GetComponentInChildren<AudioSource>();

                    if (ActiveDubbingEnabled())
                    {
                        if (audioSource != null)
                        {
                            var src = audioSource;
                            string requestedAudioPath = Combine(SpeechFolder, objectReference.AudioPath);
                            string objectPath = gameObject;
                            LogAudioSourceDiagnostics(src, "SubtitledAudioSource:" + obj.name);

                            // The swap keeps the playback time of a line that is
                            // playing, so a language switch does not restart the
                            // cutscene voice-over, and the subtitles stay in sync.
                            SwapSourceClip(src, requestedAudioPath, delegate(AudioClip newClip)
                            {
                                LogSubtitleTimingDiagnostics(objectReference, newClip, objectPath, requestedAudioPath);
                            });
                        }
                        else
                        {
                            Logging.Warn($"[UAK] AudioSource not founded in '{obj.name}'");
                        }
                    }

                    if (subtitledAudioSource != null)
                    {
                        SetPrivate(subtitledAudioSource, typeof(SubtitledAudioSource), "subtitles", objectReference.ToSubtitleData());
                    }
                    else
                    {
                        Logging.Warn($"[UAK] SubtitledAudioSource not founded in '{obj.name}'");
                    }
                }
            }
        }

        private static bool ActiveDubbingEnabled()
        {
            return LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value != "False";
        }

        private static bool TryLoadMetadata(out List<SubtitledObjectReference> references)
        {
            if (Config != null && Config.Scenes.TryGetValue(GetCurrentSceneName(), out references))
                return true;

            references = default;
            return false;
        }

        private static void LogSubtitleTimingDiagnostics(SubtitledObjectReference objectReference, AudioClip clip, string objectPath, string audioPath)
        {
            if (objectReference == null || objectReference.Lines == null || objectReference.Lines.Count == 0 || clip == null)
                return;

            Line latestLine = objectReference.Lines.OrderByDescending(line => line.Delay).FirstOrDefault();
            if (latestLine == null || latestLine.Delay <= clip.length)
                return;

            string language = LanguageManager.CurrentLanguage != null ? LanguageManager.CurrentLanguage.metadata.langName : "";
            string key = language + "|" + GetCurrentSceneName() + "|" + objectPath + "|" + audioPath + "|" + latestLine.Reference;
            if (!subtitleTimingWarnings.Add(key))
                return;

            Logging.Warn("[AudioSwap] Subtitle timing exceeds replacement clip length: scene='" + GetCurrentSceneName() +
                         "', object='" + objectPath +
                         "', audio='" + audioPath +
                         "', clipLength=" + clip.length.ToString("0.00") +
                         ", subtitle='" + latestLine.Reference +
                         "', delay=" + latestLine.Delay.ToString("0.00") + ".");
        }
    }
}
