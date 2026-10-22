using System;
using System.Collections.Generic;
using TMPro;
using UltrakULL.json;

namespace UltrakULL
{
    /// <summary>
    /// Keeps the option lists of the game's dropdowns in sync with the active
    /// language.
    ///
    /// The option texts live in <see cref="TMP_Dropdown.OptionData"/>, not in a text
    /// component, so neither the localization pass nor the live text groups can reach
    /// them. On top of that a closed dropdown shows the selected option in its
    /// caption, and the game only refreshes that caption when the player picks an
    /// option - a language switch has to do it itself, or the caption keeps the
    /// previous language until the setting is changed.
    /// </summary>
    public static class LocalizedDropdowns
    {
        private sealed class State
        {
            /// <summary>The option texts the game itself put into the dropdown.</summary>
            public string[] GameTexts;

            /// <summary>The option texts the mod wrote last.</summary>
            public string[] WrittenTexts;

            /// <summary>The language the option texts were written for, null when the mod wrote none.</summary>
            public object Language;
        }

        private static readonly Dictionary<TMP_Dropdown, State> states = new Dictionary<TMP_Dropdown, State>();

        /// <summary>
        /// Writes one option of a dropdown in the active language and refreshes the
        /// caption, so a closed dropdown follows a language switch as well.
        ///
        /// The texts the game itself wrote are remembered on the first write, so the
        /// built-in English can be put back later.
        /// </summary>
        public static void SetOptionText(TMP_Dropdown dropdown, int index, string text)
        {
            if (dropdown == null || dropdown.options == null || index < 0 || index >= dropdown.options.Count)
                return;

            State state;
            if (!states.TryGetValue(dropdown, out state))
            {
                state = new State();
                states[dropdown] = state;
            }

            if (state.Language == null)
            {
                // The mod has not written into this dropdown yet, so the texts it
                // shows are the game's own. They are the baseline the built-in
                // English returns to.
                state.GameTexts = CaptureTexts(dropdown);
                state.WrittenTexts = new string[dropdown.options.Count];
            }

            dropdown.options[index].text = text;
            state.WrittenTexts[index] = text;
            state.Language = LanguageManager.CurrentLanguage;

            RefreshCaption(dropdown);
        }

        /// <summary>
        /// Puts the game's own option texts back into every tracked dropdown.
        ///
        /// The option pages skip their patch for the built-in English, so the options
        /// of the dropdowns would keep the previous language. The texts of the game
        /// that were remembered on the first write are written back, and the captions
        /// follow.
        /// </summary>
        public static void Reapply()
        {
            if (states.Count == 0)
                return;

            if (!CommonFunctions.isUsingEnglish())
            {
                // Every other language re-writes the options through the patches of
                // the option pages, which refresh the captions on their own.
                return;
            }

            List<TMP_Dropdown> dead = null;
            foreach (KeyValuePair<TMP_Dropdown, State> pair in states)
            {
                TMP_Dropdown dropdown = pair.Key;
                if (dropdown == null)
                {
                    if (dead == null)
                        dead = new List<TMP_Dropdown>();
                    dead.Add(dropdown);
                    continue;
                }

                RestoreGameTexts(dropdown, pair.Value);
            }

            RemoveDead(dead);
        }

        /// <summary>
        /// Drops the dropdowns that are gone. Called when a scene is left, because
        /// the dropdowns of the options pages belong to that scene.
        /// </summary>
        public static void PruneDead()
        {
            if (states.Count == 0)
                return;

            List<TMP_Dropdown> dead = null;
            foreach (KeyValuePair<TMP_Dropdown, State> pair in states)
            {
                if (pair.Key == null)
                {
                    if (dead == null)
                        dead = new List<TMP_Dropdown>();
                    dead.Add(pair.Key);
                }
            }

            RemoveDead(dead);
        }

        private static void RestoreGameTexts(TMP_Dropdown dropdown, State state)
        {
            if (state == null || state.GameTexts == null || dropdown.options == null)
                return;

            for (int i = 0; i < state.GameTexts.Length && i < dropdown.options.Count; i++)
            {
                dropdown.options[i].text = state.GameTexts[i];
            }

            // The mod no longer owns the texts of this dropdown.
            state.WrittenTexts = new string[dropdown.options.Count];
            state.Language = null;

            RefreshCaption(dropdown);
        }

        private static string[] CaptureTexts(TMP_Dropdown dropdown)
        {
            string[] texts = new string[dropdown.options.Count];
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                texts[i] = dropdown.options[i].text;
            }

            return texts;
        }

        private static void RefreshCaption(TMP_Dropdown dropdown)
        {
            try
            {
                // Copies the text of the selected option into the caption of the
                // closed dropdown. The game only does this when the player picks an
                // option, so a language switch has to do it itself.
                dropdown.RefreshShownValue();
            }
            catch (Exception e)
            {
                Logging.Warn("[LocalizedDropdowns] Failed to refresh a dropdown caption: " + e.Message);
            }
        }

        private static void RemoveDead(List<TMP_Dropdown> dead)
        {
            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                states.Remove(dead[i]);
        }
    }
}
