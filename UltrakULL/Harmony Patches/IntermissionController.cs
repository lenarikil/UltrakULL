using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private static bool isPlaying = false;
        private static AudioSource currentAudioSource = null;

        // Identifies the newest audio request. A clip that is still loading cannot be
        // cancelled (AudioSwapper loads it asynchronously), so a newer request or a
        // skip invalidates the pending one through this token instead.
        private static int pendingRequestId = 0;

        /// <summary>
        /// Per-box line tracking of the extra dubbing (11.5). Several boxes can
        /// be alive at the same time, and a box that is disabled and enabled
        /// again restarts its typing from scratch, so the line that is typing
        /// is tracked per box instead of through one global counter: the old
        /// global counter reset on every box start and drifted on re-enabled
        /// boxes, which let the dubbing lag behind or run ahead of the
        /// displayed lines.
        /// </summary>
        private sealed class BoxAudioTracker
        {
            /// <summary>Line that is typing, or that is fully typed and waits at its pause.</summary>
            public int lineIndex = 1;

            /// <summary>True once the box's first line request was made.</summary>
            public bool firstPlayed;

            /// <summary>waitingForInput as seen on the previous tracked frame.</summary>
            public bool previousWaitingForInput;

            /// <summary>Line of the last request, so a line is never requested twice.</summary>
            public int lastPlayedIndex;
        }

        private static readonly Dictionary<IntermissionController, BoxAudioTracker> boxTrackers =
            new Dictionary<IntermissionController, BoxAudioTracker>();

        private static BoxAudioTracker GetTracker(IntermissionController instance)
        {
            BoxAudioTracker tracker;
            if (!boxTrackers.TryGetValue(instance, out tracker) || tracker == null)
            {
                tracker = new BoxAudioTracker();
                boxTrackers[instance] = tracker;
            }
            return tracker;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool Start_MyPatch(IntermissionController __instance, ref string ___preText, ref string ___fullString, ref Text ___txt)
        {
            ResetAudio(__instance);
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

            // The game's own text is what every translation is built from, so it is
            // remembered here: a language switch has to translate it again to reach
            // a box that is in the middle of typing.
            RememberOriginalText(__instance, ___fullString);

            IntermissionStrings intStrings = new IntermissionStrings();
            ___fullString = intStrings.GetIntermissionString(___fullString);
            ___txt.text = ___fullString;
            translatedTexts[__instance] = ___fullString;

            ___preText = TranslatePreText(___preText);

            return true;
        }

        /// <summary>
        /// The game's own text of every intermission box that started typing, so a
        /// language switch can translate it again.
        /// </summary>
        private static readonly Dictionary<IntermissionController, string> originalTexts =
            new Dictionary<IntermissionController, string>();

        /// <summary>
        /// The translated text the box started typing with. The input pause
        /// markers it holds let a language switch count the lines the old
        /// language already consumed, so the same line boundary can be found
        /// in the new text instead of mapping the visible letter count.
        /// </summary>
        private static readonly Dictionary<IntermissionController, string> translatedTexts =
            new Dictionary<IntermissionController, string>();

        private static void RememberOriginalText(IntermissionController instance, string original)
        {
            originalTexts[instance] = original;
        }

        /// <summary>
        /// Translates the speaker name that is shown above the box (the Mirage
        /// names of the visual novel). Unknown names are kept as they are.
        /// </summary>
        private static string TranslatePreText(string preText)
        {
            if (GetCurrentSceneName() != "Level 2-S" || string.IsNullOrEmpty(preText))
                return preText;

            string mirageName = Regex.Replace(preText, @"<[^>]*>", "");

            switch (mirageName)
            {
                case "JUST SOMEONE:":
                    return $"{LanguageManager.CurrentLanguage.visualnovel.visualnovel_mirageName1}:";
                case "THE PRETTIEST GIRL IN TOWN:":
                    return $"{LanguageManager.CurrentLanguage.visualnovel.visualnovel_mirageName2}:";
                case "MIRAGE:":
                    return $"{LanguageManager.CurrentLanguage.visualnovel.visualnovel_mirageName3}:";
                default:
                    return preText;
            }
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

            // The line state belongs to this box alone: two boxes that update
            // on the same click can never advance each other's line counter.
            BoxAudioTracker tracker = GetTracker(__instance);

            if (!tracker.firstPlayed)
            {
                PlayAudioForIndex(tracker.lineIndex, GetSanitizedHierarchyPath(__instance.transform, 2));
                tracker.lastPlayedIndex = tracker.lineIndex;
                tracker.firstPlayed = true;
                tracker.previousWaitingForInput = waitingForInput;
                return;
            }

            if (!waitingForInput && skipToInput && isPlaying)
            {
                StopCurrentAudio();
                return;
            }

            if (tracker.previousWaitingForInput && !waitingForInput && !skipToInput)
            {
                StopCurrentAudio();
                tracker.lineIndex++;

                if (tracker.lineIndex != tracker.lastPlayedIndex)
                {
                    PlayAudioForIndex(tracker.lineIndex, GetSanitizedHierarchyPath(__instance.transform, 2));
                    tracker.lastPlayedIndex = tracker.lineIndex;
                }
            }

            tracker.previousWaitingForInput = waitingForInput;
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
                Logging.Info($"[LocalizeIntermission] Request superseded (line {expectedIndex}), dropping clip.");
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

        /// <summary>
        /// Starts the audio tracking of a box that begins typing: its own line
        /// counting begins at line 1, and the line that is still playing (of
        /// the previous box) stops, like the game itself moves on.
        /// </summary>
        public static void ResetAudio(IntermissionController instance)
        {
            if (instance != null)
            {
                boxTrackers.Remove(instance);
            }

            isPlaying = false;

            StopCurrentAudio();
            Logging.Info("[LocalizeIntermission] Resetting audio state on scene start.");
        }

        [HarmonyPatch("OnDisable"), HarmonyPostfix]
        public static void OnDisable_MyPatch(IntermissionController __instance)
        {
            // The game restarts a box's typing from scratch when it is enabled
            // again (OnEnable -> TextAppear), so the box's line tracking starts
            // over with it; otherwise the dubbing would resume at a line the
            // restarted text is not showing.
            BoxAudioTracker tracker;
            if (boxTrackers.TryGetValue(__instance, out tracker) && tracker != null)
            {
                tracker.lineIndex = 1;
                tracker.firstPlayed = false;
                tracker.previousWaitingForInput = false;
                tracker.lastPlayedIndex = 0;
            }
        }

        /// <summary>
        /// Applies the active language to every intermission box that already typed,
        /// is typing, or is waiting for input. Called on a language switch: the boxes
        /// are not part of the localization pass (their text is only known when the
        /// game writes it), so without this they kept the language they started with.
        /// </summary>
        public static void ReapplyTyping()
        {
            try
            {
                // Drop the boxes that died with their scene.
                List<IntermissionController> dead = null;
                foreach (KeyValuePair<IntermissionController, string> pair in originalTexts)
                {
                    if (pair.Key == null)
                    {
                        if (dead == null)
                            dead = new List<IntermissionController>();
                        dead.Add(pair.Key);
                    }
                }

                if (dead != null)
                {
                    foreach (IntermissionController instance in dead)
                    {
                        originalTexts.Remove(instance);
                        translatedTexts.Remove(instance);
                        boxTrackers.Remove(instance);
                    }
                }

                // Snapshot the boxes before processing: the force-finish of a
                // box fires the game's own panel and completion events, which
                // can reveal the next box and register it while this loop runs.
                List<IntermissionController> boxes = new List<IntermissionController>(originalTexts.Keys);
                foreach (IntermissionController instance in boxes)
                {
                    if (instance != null)
                        ReapplyInstance(instance);
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[LocalizeIntermission] Failed to apply the active language to the intermission boxes: " + e);
            }
        }

        private static void ReapplyInstance(IntermissionController instance)
        {
            string original;
            if (!originalTexts.TryGetValue(instance, out original))
                return;

            // The text of the new language. This is the same lookup the box was
            // translated with when it started, so it also holds for the built-in
            // English.
            string newFull = new IntermissionStrings().GetIntermissionString(original);
            if (string.IsNullOrEmpty(newFull))
                return;

            Text txt = ReflectionUtils.GetPrivate<IntermissionController, Text>(instance, typeof(IntermissionController), "txt");
            if (txt == null)
                return;

            // The speaker name above the box follows the new language as well.
            ApplyPreText(instance);

            string oldFull = ReflectionUtils.GetPrivate<IntermissionController, string>(instance, typeof(IntermissionController), "fullString");
            bool waitingForInput = ReflectionUtils.GetPrivate<IntermissionController, bool>(instance, typeof(IntermissionController), "waitingForInput");

            if (!instance.gameObject.activeInHierarchy)
            {
                // The box is not on screen: it types the new language from the
                // beginning when it comes back.
                ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", newFull);
                return;
            }

            // A box that typed everything out keeps showing its full text, so it is
            // rewritten like any static text. The game replaces the pause and event
            // markers with spaces while typing, so the same is done here.
            bool pendingMarkers = oldFull != null && (oldFull.IndexOf('▼') >= 0 || oldFull.IndexOf('}') >= 0);
            if (!pendingMarkers && !waitingForInput)
            {
                txt.text = ConsumeMarkers(newFull);
                ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", txt.text);
                return;
            }

            // The box is in the middle of typing or waits at a pause. The current
            // line is force-completed first, exactly like the game's skip path
            // types without delays, and the new language starts at the next
            // input pause (11.4). The boundary is found by counting pause
            // markers - they are part of the translated structure, so a line
            // boundary of the old text is the same line boundary of the new
            // text - instead of mapping the visible letter count, which broke
            // the timing and let the dubbing drift.
            instance.StopAllCoroutines();

            string oldTranslated;
            bool hasOldTranslated = translatedTexts.TryGetValue(instance, out oldTranslated)
                && !string.IsNullOrEmpty(oldTranslated);

            string oldTemp = ReflectionUtils.GetPrivate<IntermissionController, string>(instance, typeof(IntermissionController), "tempString");
            int position = waitingForInput
                ? (oldTemp == null ? 0 : oldTemp.Length)
                : (txt.text == null ? 0 : txt.text.Length);

            // Lines that were started before the current one; each of them
            // consumed one input pause marker.
            int consumedPauses = hasOldTranslated ? CountMarkers(oldTranslated, '▼', position) : 0;

            if (!waitingForInput)
            {
                // Finish the line that is typing instantly. The events inside it
                // fire in order, exactly like the game's own typing does.
                if (ForceFinishLine(instance, txt, position))
                {
                    // The line was the last one, so the box typed everything
                    // out; it is rewritten like a static text in the new
                    // language and the flow the completion event started goes
                    // on with the next box.
                    txt.text = ConsumeMarkers(newFull);
                    ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", txt.text);
                    return;
                }
            }

            // The box now waits at the input pause after line
            // (consumedPauses + 1); the same pause is looked up in the new
            // text. The marker itself becomes a space, like the game's typing
            // does when it reaches it, and the rest keeps its markers.
            int boundary = FindMarkerIndex(newFull, '▼', consumedPauses + 1);
            if (boundary < 0)
            {
                // The new text has fewer pauses than lines already shown: a
                // broken translation. The box keeps the old language instead
                // of desyncing the pauses, the events and the dubbing; the
                // typing continues after the pause it is waiting at.
                Logging.Warn("[LocalizeIntermission] The new language has fewer input pauses than lines shown; keeping the previous language for this box.");
                string resumed = ReflectionUtils.GetPrivate<IntermissionController, string>(instance, typeof(IntermissionController), "tempString");
                int resumeAt = resumed == null ? 0 : resumed.Length;
                SetWaitingForInput(instance, true);
                instance.StartCoroutine(DotAppearRoutine(instance));
                instance.StartCoroutine(TextAppearFrom(instance, resumeAt + 1, true));
                return;
            }

            string prefix = ConsumeMarkers(newFull.Substring(0, boundary));
            string rest = newFull.Substring(boundary + 1);

            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", prefix + " " + rest);
            txt.text = prefix;
            SetTempString(instance, prefix);
            SetSkipToInput(instance, false);
            SetWaitingForInput(instance, true);

            // The blinking pause arrow follows the wait, and the typing
            // continues from the pause on the next click, in the new language.
            instance.StartCoroutine(DotAppearRoutine(instance));
            instance.StartCoroutine(TextAppearFrom(instance, boundary + 1, true));
        }

        /// <summary>
        /// Instantly completes the line that is currently typing: everything up
        /// to the next input pause is written out without delays, the event
        /// markers inside are consumed (their events fire in order, like the
        /// game's own typing does), and the box enters the pause's wait state.
        /// Returns true when the line was the last one and the whole box
        /// finished typing instead.
        /// </summary>
        private static bool ForceFinishLine(IntermissionController instance, Text txt, int from)
        {
            string full = GetFullString(instance);
            if (full == null || from > full.Length)
            {
                return false;
            }

            int pause = full.IndexOf('▼', from);
            int end = pause >= 0 ? pause : full.Length;

            UnityEngine.Events.UnityEvent onTextEvent = ReflectionUtils.GetPrivate<IntermissionController, UnityEngine.Events.UnityEvent>(instance, typeof(IntermissionController), "onTextEvent");

            StringBuilder sb = new StringBuilder(full);
            for (int i = from; i < end; i++)
            {
                if (sb[i] == '}')
                {
                    sb[i] = ' ';
                    if (onTextEvent != null)
                        onTextEvent.Invoke();
                }
            }
            full = sb.ToString();
            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", full);

            if (pause < 0)
            {
                // No further input pause: the line was the last one, the box
                // typed everything out. The game's own coroutine fires its
                // completion now, and the box stops waiting.
                txt.text = full;
                SetWaitingForInput(instance, false);
                UnityEngine.Events.UnityEvent onComplete = ReflectionUtils.GetPrivate<IntermissionController, UnityEngine.Events.UnityEvent>(instance, typeof(IntermissionController), "onComplete");
                if (onComplete != null)
                    onComplete.Invoke();
                Logging.Info("[LocalizeIntermission] Force-finished the last line of a box for the language switch.");
                return true;
            }

            // Enter the pause: the marker is consumed, the typed prefix is
            // the visible text, and the box waits for input.
            sb = new StringBuilder(full);
            sb[pause] = ' ';
            full = sb.ToString();
            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", full);

            txt.text = full.Substring(0, pause);
            SetTempString(instance, txt.text);
            SetSkipToInput(instance, false);
            SetWaitingForInput(instance, true);
            Logging.Info("[LocalizeIntermission] Force-finished a line up to its input pause for the language switch.");
            return false;
        }

        /// <summary>
        /// Number of markers of one kind before <paramref name="position"/> in
        /// the string. Used to count the input pauses a box already consumed.
        /// </summary>
        private static int CountMarkers(string text, char marker, int position)
        {
            int count = 0;
            int limit = text == null ? 0 : Math.Min(position, text.Length);
            for (int i = 0; i < limit; i++)
            {
                if (text[i] == marker)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Index of the n-th (1-based) marker of one kind in the string, or -1
        /// when the string does not have that many.
        /// </summary>
        private static int FindMarkerIndex(string text, char marker, int occurrence)
        {
            if (string.IsNullOrEmpty(text) || occurrence <= 0)
            {
                return -1;
            }

            int seen = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == marker)
                {
                    seen++;
                    if (seen == occurrence)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Writes the translated speaker name into the box. The game only writes it
        /// when its typing starts, so a language switch has to write it again.
        /// </summary>
        private static void ApplyPreText(IntermissionController instance)
        {
            try
            {
                string preText = instance.preText;
                string translated = TranslatePreText(preText);
                if (translated != preText)
                    instance.preText = translated;

                Text preTextText = ReflectionUtils.GetPrivate<IntermissionController, Text>(instance, typeof(IntermissionController), "preTextText");
                if (preTextText != null)
                    preTextText.text = instance.preText;
            }
            catch (Exception e)
            {
                Logging.Warn("[LocalizeIntermission] Failed to apply the active language to the speaker name: " + e.Message);
            }
        }

        private static bool IsMarker(char c)
        {
            return c == '▼' || c == '}';
        }

        private static string ConsumeMarkers(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.IndexOf('▼') < 0 && text.IndexOf('}') < 0)
                return text;

            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (IsMarker(chars[i]))
                    chars[i] = ' ';
            }

            return new string(chars);
        }

        private static bool GetWaitingForInput(IntermissionController instance)
        {
            return ReflectionUtils.GetPrivate<IntermissionController, bool>(instance, typeof(IntermissionController), "waitingForInput");
        }

        private static void SetWaitingForInput(IntermissionController instance, bool value)
        {
            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "waitingForInput", value);
        }

        private static bool GetSkipToInput(IntermissionController instance)
        {
            return ReflectionUtils.GetPrivate<IntermissionController, bool>(instance, typeof(IntermissionController), "skipToInput");
        }

        private static void SetSkipToInput(IntermissionController instance, bool value)
        {
            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "skipToInput", value);
        }

        private static string GetFullString(IntermissionController instance)
        {
            return ReflectionUtils.GetPrivate<IntermissionController, string>(instance, typeof(IntermissionController), "fullString");
        }

        private static void SetFullString(IntermissionController instance, string value)
        {
            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "fullString", value);
        }

        private static void SetTempString(IntermissionController instance, string value)
        {
            ReflectionUtils.SetPrivate(instance, typeof(IntermissionController), "tempString", value);
        }

        /// <summary>
        /// The blinking pause arrow of the game, started through reflection so the
        /// language switch can restart it after the coroutines of the box were
        /// stopped.
        /// </summary>
        private static IEnumerator DotAppearRoutine(IntermissionController instance)
        {
            if (DotAppearMethod == null)
                yield break;

            IEnumerator routine = (IEnumerator)DotAppearMethod.Invoke(instance, null);
            if (routine == null)
                yield break;

            while (routine.MoveNext())
                yield return routine.Current;
        }

        private static readonly System.Reflection.MethodInfo DotAppearMethod =
            typeof(IntermissionController).GetMethod("DotAppear", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        /// <summary>
        /// The typing of IntermissionController.TextAppear, continued from a given
        /// letter of the full string. The body is the game's own logic: the pause
        /// marker waits for input, the event marker fires the panel event, the click
        /// sound plays per typed character, and skipping types without delays.
        /// </summary>
        private static IEnumerator TextAppearFrom(IntermissionController instance, int startLetter, bool resumeFromWait)
        {
            Text txt = ReflectionUtils.GetPrivate<IntermissionController, Text>(instance, typeof(IntermissionController), "txt");
            AudioSource aud = ReflectionUtils.GetPrivate<IntermissionController, AudioSource>(instance, typeof(IntermissionController), "aud");
            float origPitch = ReflectionUtils.GetPrivate<IntermissionController, float>(instance, typeof(IntermissionController), "origPitch");

            if (resumeFromWait)
            {
                // The switch happened while the box was waiting at a pause: the wait
                // is kept exactly as the game had it, the typing continues after the
                // click.
                yield return new WaitUntil(() => !GetWaitingForInput(instance) || !instance.gameObject.scene.isLoaded);
                aud.SetPitch(UnityEngine.Random.Range(origPitch - 0.05f, origPitch + 0.05f));
                aud.Play(tracked: true);
                yield return new WaitForSecondsRealtime(0.05f);
            }

            int j = GetFullString(instance).Length;
            for (int i = startLetter; i < j; i++)
            {
                string fullString = GetFullString(instance);
                char c = fullString[i];
                float waitTime = 0.05f;
                bool playSound = true;
                if (MonoSingleton<OptionsManager>.Instance.paused)
                {
                    yield return new WaitUntil(() => MonoSingleton<OptionsManager>.Instance == null || !MonoSingleton<OptionsManager>.Instance.paused);
                }
                switch (c)
                {
                    case '▼':
                        {
                            StringBuilder sb = new StringBuilder(fullString);
                            sb[i] = ' ';
                            fullString = sb.ToString();
                            SetFullString(instance, fullString);
                            txt.text = fullString.Substring(0, i);
                            SetTempString(instance, txt.text);
                            SetSkipToInput(instance, false);
                            SetWaitingForInput(instance, true);
                            instance.StartCoroutine(DotAppearRoutine(instance));
                            yield return new WaitUntil(() => !GetWaitingForInput(instance) || !instance.gameObject.scene.isLoaded);
                            break;
                        }
                    case '}':
                        {
                            StringBuilder sb = new StringBuilder(fullString);
                            sb[i] = ' ';
                            fullString = sb.ToString();
                            SetFullString(instance, fullString);
                            playSound = false;
                            waitTime = 0f;
                            txt.text = fullString.Substring(0, i);
                            UnityEngine.Events.UnityEvent onTextEvent = ReflectionUtils.GetPrivate<IntermissionController, UnityEngine.Events.UnityEvent>(instance, typeof(IntermissionController), "onTextEvent");
                            if (onTextEvent != null)
                                onTextEvent.Invoke();
                            break;
                        }
                    case ' ':
                        waitTime = 0f;
                        txt.text = fullString.Substring(0, i);
                        break;
                    default:
                        txt.text = fullString.Substring(0, i);
                        break;
                }
                j = GetFullString(instance).Length;
                if ((waitTime != 0f) & playSound)
                {
                    aud.SetPitch(UnityEngine.Random.Range(origPitch - 0.05f, origPitch + 0.05f));
                    aud.Play(tracked: true);
                }
                if (GetSkipToInput(instance))
                {
                    waitTime = 0f;
                }
                yield return new WaitForSecondsRealtime(waitTime);
            }

            UnityEngine.Events.UnityEvent onComplete = ReflectionUtils.GetPrivate<IntermissionController, UnityEngine.Events.UnityEvent>(instance, typeof(IntermissionController), "onComplete");
            if (onComplete != null)
                onComplete.Invoke();
        }

        /// <summary>
        /// Follows the dubbing through a language switch. The line that is
        /// playing belongs to the line of the old language the box was
        /// force-finished to (the same approach as the text, 11.4): it is kept
        /// playing to its end so the dubbing never swaps mid-sentence, and the
        /// next line automatically starts in the new language because the line
        /// counter stays aligned with the input pauses. Only the built-in
        /// English and disabled dubbing stop the line, exactly like they do
        /// when a segment starts.
        /// </summary>
        public static void ReapplyAudio()
        {
            try
            {
                if (currentAudioSource == null)
                    return;

                if (isUsingEnglish() || !ExtraDubbingEnabled())
                {
                    StopCurrentAudio();
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[LocalizeIntermission] Failed to apply the active language to the intermission dubbing: " + e.Message);
            }
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
