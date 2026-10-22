using System.Collections.Generic;
using TMPro;

namespace UltrakULL
{
    /// <summary>
    /// Keeps the subtitles that are on screen in sync with the active language.
    ///
    /// A subtitle is a short lived object that the game fills once, when it appears,
    /// and never touches again, so a language switch did not reach it and it kept the
    /// previous language until it faded out. Every place that localizes a subtitle
    /// knows the key of the line, so the key is registered together with the localized
    /// text here. When the subtitle is shown, the key is found by the text and
    /// remembered for the object, so a language switch can write the same line of the
    /// new language into it.
    /// </summary>
    public static class LiveSubtitles
    {
        private sealed class Entry
        {
            /// <summary>The field name of the line in the subtitles of a language.</summary>
            public string Key;

            /// <summary>The color the line is wrapped in, null when it has none.</summary>
            public string Color;
        }

        /// <summary>The localized text of a line, pointing back to its key.</summary>
        private static readonly Dictionary<string, Entry> textToKey = new Dictionary<string, Entry>();

        /// <summary>The subtitles that are on screen, with the key of their line.</summary>
        private static readonly Dictionary<Subtitle, Entry> live = new Dictionary<Subtitle, Entry>();

        /// <summary>
        /// Remembers which key a localized subtitle text belongs to. Called by the
        /// places that localize a subtitle line.
        /// </summary>
        /// <param name="key">The field name of the line.</param>
        /// <param name="text">The localized text, with the color already applied.</param>
        /// <param name="color">The color the text is wrapped in, null when it has none.</param>
        public static void RegisterText(string key, string text, string color = null)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(text))
                return;

            textToKey[text] = new Entry { Key = key, Color = color };
        }

        /// <summary>
        /// Remembers the key of the line a shown subtitle displays. Called when the
        /// subtitle appears on screen.
        /// </summary>
        public static void OnDisplayed(Subtitle subtitle, string caption)
        {
            if (subtitle == null || string.IsNullOrEmpty(caption))
                return;

            Entry entry;
            if (!textToKey.TryGetValue(caption, out entry))
                return;

            live[subtitle] = entry;
        }

        /// <summary>
        /// Writes the active language into the subtitles that are on screen.
        ///
        /// The line itself stays the same, only its language changes, so the chain of
        /// the subtitles and their timing are not touched.
        /// </summary>
        public static void Reapply()
        {
            if (live.Count == 0)
                return;

            List<Subtitle> dead = null;
            foreach (KeyValuePair<Subtitle, Entry> pair in live)
            {
                Subtitle subtitle = pair.Key;
                if (subtitle == null)
                {
                    // The subtitle faded out on its own.
                    if (dead == null)
                        dead = new List<Subtitle>();
                    dead.Add(subtitle);
                    continue;
                }

                Entry entry = pair.Value;
                string text = SubtitlesHelper.GetText(entry.Key);
                if (string.IsNullOrEmpty(text))
                {
                    // The language has no value for the line, the text stays.
                    continue;
                }

                if (!string.IsNullOrEmpty(entry.Color))
                {
                    text = "<color=#" + entry.Color + ">" + text + "</color>";
                }

                TMP_Text subtitleText = subtitle.GetComponentInChildren<TMP_Text>(true);
                if (subtitleText == null || subtitleText.text == text)
                    continue;

                subtitleText.text = text;

                // The box of the subtitle is sized for the text it was created with,
                // so it is sized again for the new language. The sizing is a private
                // coroutine of the game, it is started the same way the game does.
                FitSubtitle(subtitle);
            }

            RemoveDead(dead);
        }

        /// <summary>
        /// Drops the subtitles that are gone. Called when a scene is left, because the
        /// subtitles on screen belong to that scene.
        /// </summary>
        public static void PruneDead()
        {
            if (live.Count == 0)
                return;

            List<Subtitle> dead = null;
            foreach (KeyValuePair<Subtitle, Entry> pair in live)
            {
                if (pair.Key == null)
                {
                    if (dead == null)
                        dead = new List<Subtitle>();
                    dead.Add(pair.Key);
                }
            }

            RemoveDead(dead);
        }

        private static void RemoveDead(List<Subtitle> dead)
        {
            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                live.Remove(dead[i]);
        }

        private static System.Reflection.MethodInfo fitMethod;

        /// <summary>
        /// Starts the sizing of the subtitle the game itself uses. It is private, so
        /// it is called through reflection. The lookup happens once, the subtitles are
        /// only re-sized on a language switch.
        /// </summary>
        private static void FitSubtitle(Subtitle subtitle)
        {
            try
            {
                if (fitMethod == null)
                {
                    fitMethod = typeof(Subtitle).GetMethod("Fit",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                }

                if (fitMethod != null)
                    fitMethod.Invoke(subtitle, null);
            }
            catch (System.Exception e)
            {
                Logging.Warn("[LiveSubtitles] Failed to re-size a subtitle: " + e.Message);
            }
        }
    }
}
