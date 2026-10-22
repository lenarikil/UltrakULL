using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltrakULL
{
    /// <summary>
    /// Baseline snapshot of the text the game itself puts on screen.
    ///
    /// The mod writes translations straight into the game's text components, so the
    /// original string is gone as soon as a language is applied. This snapshot keeps
    /// the game's own text so a language switch can put it back before the new
    /// language is written on top of it. That is what makes the built-in "English"
    /// entry show the game's own strings again instead of the previous translation.
    ///
    /// Only the components the localization pass itself writes to are taken back on a
    /// language switch. Everything else belongs to the game (health, money, timer, the
    /// cheat list on the HUD, ...) and keeps the value the game put there.
    ///
    /// The snapshot is captured once per scene, right before the first localization
    /// pass, and only keeps components that already had text at that moment. Text the
    /// game writes later on (money counter, boss name, ...) belongs to the game and is
    /// tracked by the code that translates it, because the game itself keeps that
    /// value in a field and writes it out again.
    /// </summary>
    public static class OriginalTextSnapshot
    {
        private sealed class Entry
        {
            public TMP_Text Tmp;
            public UnityEngine.UI.Text Ui;

            /// <summary>
            /// The text the game owns. Captured right before the last localization
            /// pass, so it is the value the game had when the pass took the component
            /// over.
            /// </summary>
            public string GameText;

            /// <summary>True while the component shows the translation of the last pass.</summary>
            public bool Translated;

            /// <summary>The translation the last pass wrote into the component.</summary>
            public string TranslatedText;
        }

        private static readonly List<Entry> entries = new List<Entry>();

        /// <summary>Number of captured text components.</summary>
        public static int Count
        {
            get { return entries.Count; }
        }

        /// <summary>
        /// Captures the current text of every text component in the active scene.
        /// Components without text are skipped: they are either empty by design or
        /// filled by the game later on, and writing an empty string back would wipe
        /// text the game owns.
        /// </summary>
        public static void Capture()
        {
            entries.Clear();

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
                return;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                    continue;

                TMP_Text[] tmpTexts = root.GetComponentsInChildren<TMP_Text>(true);
                for (int j = 0; j < tmpTexts.Length; j++)
                {
                    TMP_Text tmp = tmpTexts[j];
                    if (tmp == null)
                        continue;

                    string text = tmp.text;
                    if (string.IsNullOrEmpty(text))
                        continue;

                    entries.Add(new Entry { Tmp = tmp, GameText = text });
                }

                UnityEngine.UI.Text[] uiTexts = root.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                for (int j = 0; j < uiTexts.Length; j++)
                {
                    UnityEngine.UI.Text ui = uiTexts[j];
                    if (ui == null)
                        continue;

                    string text = ui.text;
                    if (string.IsNullOrEmpty(text))
                        continue;

                    entries.Add(new Entry { Ui = ui, GameText = text });
                }
            }
        }

        /// <summary>
        /// Remembers what the captured components show right now. Called right before
        /// a localization pass, so the pass can be told apart from the values the game
        /// writes on its own.
        /// </summary>
        public static void CapturePassBaseline()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry == null)
                    continue;

                string current = GetText(entry);
                if (current == null)
                    continue;

                entry.GameText = current;
            }
        }

        /// <summary>
        /// Marks the components the localization pass has written to. A component that
        /// still shows the text it had before the pass was not touched by the mod, so
        /// the game owns it and it must survive a language switch untouched.
        ///
        /// Called right after the pass, while the values of the game cannot have
        /// changed in between.
        /// </summary>
        public static void MarkTranslated()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry == null)
                    continue;

                string current = GetText(entry);
                if (current == null)
                    continue;

                if (current == entry.GameText)
                {
                    entry.Translated = false;
                    entry.TranslatedText = null;
                    continue;
                }

                entry.Translated = true;
                entry.TranslatedText = current;
            }
        }

        /// <summary>
        /// Writes the game's own text back into the components the mod translated, so
        /// the new language is written on a clean baseline. Used on every language
        /// switch: components the game owns (health, money, timer, ...) keep the value
        /// the game gave them.
        /// </summary>
        public static void Restore()
        {
            if (entries.Count == 0)
                return;

            int restored = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry == null || !entry.Translated)
                    continue;

                string current = GetText(entry);
                if (current == null)
                    continue;

                // An empty text means the game cleared it (a dynamic label for
                // example), so the snapshot is not written back over it.
                if (string.IsNullOrEmpty(current))
                    continue;

                // The game has written its own value on top of the translation, so the
                // value belongs to the game now and must not be replaced.
                if (current != entry.TranslatedText)
                    continue;

                if (string.IsNullOrEmpty(entry.GameText) || current == entry.GameText)
                    continue;

                SetText(entry, entry.GameText);
                restored++;

                entry.Translated = false;
                entry.TranslatedText = null;
            }

            if (restored > 0)
                Logging.Message($"[OriginalTextSnapshot] Restored {restored} original text value(s).");
        }

        /// <summary>
        /// Drops the captured text. Called when the scene changes so the next scene
        /// starts with a fresh baseline.
        /// </summary>
        public static void Clear()
        {
            entries.Clear();
        }

        private static string GetText(Entry entry)
        {
            // Unity's overloaded == reports destroyed objects as null, so a component
            // that died with the previous scene is skipped here.
            if (entry.Tmp != null)
                return entry.Tmp.text;

            if (entry.Ui != null)
                return entry.Ui.text;

            return null;
        }

        private static void SetText(Entry entry, string text)
        {
            if (entry.Tmp != null)
            {
                entry.Tmp.text = text;
            }
            else if (entry.Ui != null)
            {
                entry.Ui.text = text;
            }
        }
    }
}
