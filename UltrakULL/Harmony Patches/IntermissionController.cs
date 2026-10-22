using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UltrakULL.CommonFunctions;
using static UnityEngine.UI.Image;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(IntermissionController))]
    public static class LocalizeIntermission
    {
        private static int currentIndex = 1;
        private static bool firstPlayed = false;
        private static bool previousWaitingForInput = false;
        private static int lastPlayedIndex = 0;
        private static bool isPlaying = false;
        private static AudioSource currentAudioSource = null;

        // Identifies the newest audio request. A clip that is still loading cannot be
        // cancelled (AudioSwapper loads it asynchronously), so a newer request or a
        // skip invalidates the pending one through this token instead. currentIndex
        // alone cannot do that: the skip path does not advance the index.
        private static int pendingRequestId = 0;

        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool Start_MyPatch(IntermissionController __instance, ref string ___preText, ref string ___fullString, ref Text ___txt)
        {
            ResetAudio();
            CoroutineHelper.Init();

            /*
            In the original, there is no ▼ (wait input) between act2_intermission_fourth8 and act2_intermission_fourth9.
            Therefore, I disable the update stop for English(to ensure that the behavior is the same between English and other languages.
            If this option is not suitable, remove the ▼ between act2_intermission_fourth8 and act2_intermission_fourth9)
            */

            //if (isUsingEnglish()) return true; 

            ___txt = __instance.GetComponent<Text>();
            ___txt.verticalOverflow = VerticalWrapMode.Overflow;
            ___fullString = ___txt.text;
            ___txt.text = "";

            IntermissionStrings intStrings = new IntermissionStrings();
            ___fullString = intStrings.GetIntermissionString(___fullString);
            ___txt.text = ___fullString;

            if (GetCurrentSceneName() == "Level 2-S")
            {
                string openingTag = "<color=grey>";
                string closingTag = "</color>";
                string mirageName = Regex.Replace(___preText, @"<[^>]*>", "");

                switch (mirageName)
                {
                    case "JUST SOMEONE:":
                        ___preText = $"{LanguageManager.CurrentLanguage.visualnovel.visualnovel_mirageName1}:";
                        break;
                    case "THE PRETTIEST GIRL IN TOWN:":
                        ___preText = $"{LanguageManager.CurrentLanguage.visualnovel.visualnovel_mirageName2}:";
                        break;
                    case "MIRAGE:":
                        ___preText = $"{LanguageManager.CurrentLanguage.visualnovel.visualnovel_mirageName3}:";
                        break;
                }
            }

            return true;
        }


        private static bool wasPausedLastFrame = false;

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static void Update_MyPatch(IntermissionController __instance, ref bool ___waitingForInput, ref bool ___skipToInput)
        {
            if (!__instance.gameObject.activeInHierarchy)
                return;

            bool isPaused = MonoSingleton<OptionsManager>.Instance != null && MonoSingleton<OptionsManager>.Instance.paused;

            // ▶️ Transition from pause to normal → UnPause
            if (wasPausedLastFrame && !isPaused)
            {
                if (currentAudioSource != null && currentAudioSource.clip != null && currentAudioSource.isPlaying == false)
                {
                    currentAudioSource.UnPause();
                }
            }
            // ⏸ Entering pause → Pause
            else if (!wasPausedLastFrame && isPaused)
            {
                if (currentAudioSource != null && currentAudioSource.isPlaying)
                {
                    currentAudioSource.Pause();
                }
            }

            wasPausedLastFrame = isPaused;

            // ❌ Don't continue processing if the game is paused
            if (isPaused)
                return;

            bool waitingForInput = ___waitingForInput;
            bool skipToInput = ___skipToInput;

            if (!firstPlayed)
            {
                PlayAudioForIndex(currentIndex, GetSanitizedHierarchyPath(__instance.transform, 2));
                lastPlayedIndex = currentIndex;
                firstPlayed = true;
                previousWaitingForInput = waitingForInput;
                return;
            }

            if (!waitingForInput && skipToInput && isPlaying)
            {
                StopCurrentAudio();
                return;
            }

            if (previousWaitingForInput && !waitingForInput && !skipToInput)
            {
                StopCurrentAudio();
                currentIndex++;

                if (currentIndex != lastPlayedIndex)
                {
                    PlayAudioForIndex(currentIndex, GetSanitizedHierarchyPath(__instance.transform, 2));
                    lastPlayedIndex = currentIndex;
                }
            }

            previousWaitingForInput = waitingForInput;
        }


        private static string GetSanitizedHierarchyPath(Transform transform, int depth)
        {
            List<string> names = new List<string>();
            Transform current = transform;

            for (int i = 0; i < depth && current.parent != null; i++)
            {
                current = current.parent;
                names.Insert(0, SanitizeFileName(current.name));
            }

            names.Add(SanitizeFileName(transform.gameObject.name));

            return Path.Combine(names.ToArray());
        }

        private static void StopCurrentAudio()
        {
            // A clip that is still loading cannot be cancelled, so the pending request
            // is invalidated here. Its callback sees the new id and drops the clip.
            pendingRequestId++;

            if (currentAudioSource != null)
            {
                currentAudioSource.Stop();
                UnityEngine.Object.Destroy(currentAudioSource.gameObject);
                currentAudioSource = null;
                isPlaying = false;
                CoroutineHelper.StopCurrentCoroutine();
                Logging.Info("[LocalizeIntermission] Audio stopped (skipped).");
            }
        }

        private static void PlayAudioForIndex(int index, string gameObjectName)
        {
            if (!ExtraDubbingEnabled())
            {
                Logging.Info("[LocalizeIntermission] Audio dubbing or Extra dubbing is disabled :(");
                return;
            }

            // The built-in English is "no translation": the game's own voice lines are
            // used, exactly like in every other dubbing path of the mod.
            if (isUsingEnglish())
                return;

            isPlaying = true;

            // Language packs keep their audio inside the pack folder (one folder = one
            // language), so the scene folder is resolved from the pack root instead of
            // the old "config/ultrakull/audio/<langName>" layout.
            string folderPath = Path.Combine(
                LanguageManager.GetLanguageAssetFolder("audio"),
                GetCurrentSceneName(),
                gameObjectName
            );

            EnsureIntermissionAudioFolder(folderPath);

            // The file name is requested without an extension: AudioSwapper then picks
            // up whichever format the pack ships (ogg, mp3, wav, ...).
            string audioFileName = $"line{index}";
            string audioPath = Path.Combine(folderPath, audioFileName);

            Logging.Info($"[LocalizeIntermission] Attempting to play: {audioFileName}");

            int requestId = ++pendingRequestId;
            AudioSwapper.PreloadClipAsync(audioPath, null, delegate(AudioClip clip)
            {
                OnClipLoaded(clip, requestId, index);
            });
        }

        /// <summary>
        /// Creates the folder the extra dubbing lines are expected in. Only packs that
        /// are actually installed on disk get one - the built-in English has no pack
        /// folder, so nothing is created for it.
        /// </summary>
        private static void EnsureIntermissionAudioFolder(string folderPath)
        {
            JsonFormat current = LanguageManager.CurrentLanguage;
            if (current == null || string.IsNullOrEmpty(current.RootFolder))
                return;

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        /// <summary>
        /// The extra dubbing is only played when both the regular dubbing and the
        /// extra dubbing are switched on.
        /// </summary>
        private static bool ExtraDubbingEnabled()
        {
            try
            {
                return IsConfigEnabled("activeDubbing") && IsConfigEnabled("extraDubbing");
            }
            catch (Exception e)
            {
                Logging.Warn("[LocalizeIntermission] Failed to read the dubbing options: " + e.Message);
                return false;
            }
        }

        private static bool IsConfigEnabled(string key)
        {
            string value = LanguageManager.configFile.Bind("General", key, "False", (ConfigDescription)null).Value;
            return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.Ordinal);
        }

        private static string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new System.Text.StringBuilder();

            foreach (char c in name)
            {
                if (!char.IsWhiteSpace(c) && !invalidChars.Contains(c))
                {
                    sanitized.Append(c);
                }
            }

            return sanitized.ToString();
        }

        private static void OnClipLoaded(AudioClip clip, int requestId, int expectedIndex)
        {
            if (requestId != pendingRequestId)
            {
                Logging.Info($"[LocalizeIntermission] Request superseded (expected {expectedIndex}, current {currentIndex}), dropping clip.");
                isPlaying = false;
                return;
            }

            if (clip == null)
            {
                // AudioSwapper already logged the missing file together with its path.
                Logging.Info($"[LocalizeIntermission] No dubbing available for 'line{expectedIndex}'.");
                isPlaying = false;
                return;
            }

            if (clip.length <= 0f)
            {
                Logging.Warn($"[LocalizeIntermission] File 'line{expectedIndex}' is corrupted or empty.");
                isPlaying = false;
                return;
            }

            string fileName = $"line{expectedIndex}";
            GameObject audioObject = new GameObject($"Audio_{fileName}");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;

            currentAudioSource = source;
            UnityEngine.Object.DontDestroyOnLoad(audioObject);

            Logging.Info($"[LocalizeIntermission] Playing: {fileName}");
            source.Play();

            CoroutineHelper.StartSafeCoroutine(WaitForPlaybackEnd(source, clip));
        }

        private static IEnumerator WaitForPlaybackEnd(AudioSource source, AudioClip clip)
        {
            float timer = 0f;
            while (timer < clip.length)
            {
                if (currentAudioSource != source || source == null) break;
                timer += Time.deltaTime;
                yield return null;
            }

            if (currentAudioSource == source && source != null)
            {
                UnityEngine.Object.Destroy(source.gameObject);
                currentAudioSource = null;
            }

            isPlaying = false;
        }

        public static void ResetAudio()
        {
            currentIndex = 1;
            firstPlayed = false;
            previousWaitingForInput = false;
            isPlaying = false;

            StopCurrentAudio();
            Logging.Info("[LocalizeIntermission] Resetting audio state on scene start.");
        }

        private class CoroutineHelper : MonoBehaviour
        {
            private static CoroutineHelper instance;
            private static Coroutine currentCoroutine;

            public static void Init()
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CoroutineHelper");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    instance = go.AddComponent<CoroutineHelper>();
                }
            }

            public static void StartSafeCoroutine(IEnumerator coroutine)
            {
                Init();
                if (currentCoroutine != null)
                {
                    instance.StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }
                currentCoroutine = instance.StartCoroutine(coroutine);
            }

            public static void StopCurrentCoroutine()
            {
                if (currentCoroutine != null)
                {
                    instance.StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }
            }
        }
    }
}
