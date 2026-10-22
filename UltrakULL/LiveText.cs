using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UltrakULL
{
    /// <summary>
    /// Keeps the texts the mod translated in reaction to a game event up to date.
    ///
    /// Most of the mod's translations are written by the localization pass, and that
    /// pass runs again on every language switch. The texts that are written when the
    /// game does something (opening a book, picking an enemy, showing a checklist,
    /// ...) are not part of that pass: the game writes its own text into those
    /// components once and never touches them again, so they kept the language that
    /// was active before the switch.
    ///
    /// Every one of those translations is registered here together with the text the
    /// game had written before. A language switch then puts the game's own text back
    /// and writes the new language on top of it, so the player does not have to
    /// reopen anything.
    /// </summary>
    public static class LiveText
    {
        private sealed class Entry
        {
            public TMP_Text Tmp;
            public UnityEngine.UI.Text Ui;

            /// <summary>The text the game itself wrote into the component.</summary>
            public string GameText;

            /// <summary>Builds the translation for the active language.</summary>
            public Func<string> Producer;

            /// <summary>The translation that was written last.</summary>
            public string TranslatedText;
        }

        private static readonly List<Entry> entries = new List<Entry>();

        /// <summary>
        /// Writes the translation into the component and remembers how to write it
        /// again after a language switch.
        ///
        /// <paramref name="gameText"/> is the text the game wrote into the component
        /// before the mod took it over. It is put back when the built-in English is
        /// selected, because that language means "the game's own text".
        /// </summary>
        public static void Set(TMP_Text target, string gameText, string translation, Func<string> producer)
        {
            if (target == null)
                return;

            Entry entry = Find(target);
            if (entry == null)
            {
                entry = new Entry { Tmp = target };
                entries.Add(entry);
            }

            entry.GameText = gameText;
            entry.Producer = producer;
            entry.TranslatedText = translation;

            target.text = translation;
        }

        /// <summary>
        /// Writes the translation into the component and remembers how to write it
        /// again after a language switch.
        ///
        /// The text the game had written is taken from the component itself, so it
        /// has to be called before the component is overwritten.
        /// </summary>
        public static void Set(TMP_Text target, string translation, Func<string> producer)
        {
            if (target == null)
                return;

            Set(target, target.text, translation, producer);
        }

        /// <summary>
        /// Writes the translation into the legacy text component and remembers how to
        /// write it again after a language switch.
        /// </summary>
        public static void Set(UnityEngine.UI.Text target, string gameText, string translation, Func<string> producer)
        {
            if (target == null)
                return;

            Entry entry = Find(target);
            if (entry == null)
            {
                entry = new Entry { Ui = target };
                entries.Add(entry);
            }

            entry.GameText = gameText;
            entry.Producer = producer;
            entry.TranslatedText = translation;

            target.text = translation;
        }

        /// <summary>
        /// Writes the active language into every registered component again.
        ///
        /// A component that no longer shows the translation of the mod belongs to the
        /// game now and is left alone.
        /// </summary>
        public static void Reapply()
        {
            ReapplyEntries();
            ReapplyGroups();
        }

        private static void ReapplyEntries()
        {
            if (entries.Count == 0)
                return;

            int applied = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry == null)
                    continue;

                string current = GetText(entry);
                if (current == null)
                    continue;

                // The game has written its own value on top of the translation, so
                // the value belongs to the game now and must not be replaced.
                if (current != entry.TranslatedText)
                    continue;

                string text;
                if (CommonFunctions.isUsingEnglish())
                {
                    // The built-in English is the game's own text.
                    text = entry.GameText;
                }
                else
                {
                    if (entry.Producer == null)
                        continue;

                    try
                    {
                        text = entry.Producer();
                    }
                    catch (Exception e)
                    {
                        CommonFunctions.HandleError(e);
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(text) || text == current)
                    continue;

                SetText(entry, text);
                entry.TranslatedText = text;
                applied++;
            }

            if (applied > 0)
                Logging.Message($"[LiveText] Applied the active language to {applied} live text(s).");
        }

        /// <summary>
        /// Drops the components that are gone. Called when a scene is left, because
        /// the registered components belong to that scene.
        /// </summary>
        public static void PruneDead()
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                Entry entry = entries[i];
                if (entry == null || GetText(entry) == null)
                    entries.RemoveAt(i);
            }

            for (int i = groups.Count - 1; i >= 0; i--)
            {
                Group group = groups[i];
                if (group == null || group.Root == null)
                    groups.RemoveAt(i);
            }
        }

        /// <summary>
        /// Drops every registered component. Used when the mod's own state is reset.
        /// </summary>
        public static void Clear()
        {
            entries.Clear();
            groups.Clear();
        }

        private static Entry Find(Component target)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry == null)
                    continue;

                if (entry.Tmp == target || entry.Ui == target)
                    return entry;
            }

            return null;
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

        #region Groups

        /// <summary>
        /// A whole UI subtree that the mod translates in one go (the shop terminal,
        /// the options page, ...).
        /// </summary>
        private sealed class Group
        {
            public GameObject Root;

            /// <summary>Writes the translation of the active language into the subtree.</summary>
            public Action Patch;

            public readonly List<GroupEntry> Texts = new List<GroupEntry>();
        }

        private sealed class GroupEntry
        {
            public TMP_Text Tmp;
            public UnityEngine.UI.Text Ui;

            /// <summary>The text the game itself wrote into the component.</summary>
            public string Original;

            /// <summary>The translation that was written last.</summary>
            public string Translated;
        }

        private static readonly List<Group> groups = new List<Group>();

        /// <summary>
        /// Translates a whole UI subtree and keeps it in sync with the active language.
        ///
        /// Some screens are not part of the localization pass: the game fills them when
        /// the player opens them (the shop terminal, the options page, ...) and never
        /// touches them again. Registering the subtree here remembers the text the game
        /// wrote, so a language switch can put that text back and let the same patch
        /// write the new language on top of it. The screen does not have to be closed
        /// and opened again, and nothing is destroyed and rebuilt.
        ///
        /// The patch is run once here and again on every language switch.
        /// </summary>
        /// <param name="root">The root of the subtree that is translated.</param>
        /// <param name="patch">Writes the translation of the active language.</param>
        public static void ApplyGroup(GameObject root, Action patch)
        {
            if (root == null || patch == null)
                return;

            Group group = FindGroup(root);
            if (group == null)
            {
                group = new Group { Root = root, Patch = patch };
                groups.Add(group);

                // The game wrote this text, so it is the baseline the patch works on.
                CaptureOriginals(group, root);
            }
            else
            {
                group.Patch = patch;
            }

            if (CommonFunctions.isUsingEnglish())
            {
                // The built-in English is the game's own text, so the patch must not
                // write anything. The group stays registered, so a later switch to a
                // translated language finds the game's own text again.
                return;
            }

            RunPatch(group);
            CaptureTranslated(group, root);
        }

        /// <summary>
        /// Writes the active language into every registered subtree again.
        ///
        /// The game's own text is put back first, so the patch starts from the same
        /// state it saw when the screen was opened. The built-in English then keeps
        /// the game's own text, because its patch does not write anything.
        /// </summary>
        private static void ReapplyGroups()
        {
            if (groups.Count == 0)
                return;

            int applied = 0;
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                Group group = groups[i];
                if (group == null || group.Root == null)
                {
                    // The subtree died with the scene it belonged to.
                    groups.RemoveAt(i);
                    continue;
                }

                RestoreGroup(group);

                if (CommonFunctions.isUsingEnglish())
                {
                    // The built-in English is the game's own text, the patch does not
                    // write anything, so the restore above is all that is needed.
                    continue;
                }

                RunPatch(group);
                CaptureTranslated(group, group.Root);
                applied++;
            }

            if (applied > 0)
                Logging.Message($"[LiveText] Applied the active language to {applied} live group(s).");
        }

        private static void RunPatch(Group group)
        {
            if (group.Patch == null)
                return;

            try
            {
                group.Patch();
            }
            catch (Exception e)
            {
                CommonFunctions.HandleError(e);
            }
        }

        private static void RestoreGroup(Group group)
        {
            for (int i = 0; i < group.Texts.Count; i++)
            {
                GroupEntry entry = group.Texts[i];
                if (entry == null)
                    continue;

                string current = GetGroupText(entry);
                if (current == null)
                    continue;

                // The game has written its own value on top of the translation, so the
                // value belongs to the game now and must not be replaced.
                if (entry.Translated != null && current != entry.Translated)
                    continue;

                if (current == entry.Original)
                    continue;

                SetGroupText(entry, entry.Original);
            }
        }

        private static void CaptureOriginals(Group group, GameObject root)
        {
            group.Texts.Clear();

            TMP_Text[] tmpTexts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                TMP_Text tmp = tmpTexts[i];
                if (tmp == null)
                    continue;

                group.Texts.Add(new GroupEntry { Tmp = tmp, Original = tmp.text });
            }

            UnityEngine.UI.Text[] uiTexts = root.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            for (int i = 0; i < uiTexts.Length; i++)
            {
                UnityEngine.UI.Text ui = uiTexts[i];
                if (ui == null)
                    continue;

                group.Texts.Add(new GroupEntry { Ui = ui, Original = ui.text });
            }
        }

        /// <summary>
        /// Remembers what the patch wrote, so the next language switch knows which
        /// components still show the translation of the mod.
        /// </summary>
        private static void CaptureTranslated(Group group, GameObject root)
        {
            TMP_Text[] tmpTexts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < tmpTexts.Length; i++)
            {
                TMP_Text tmp = tmpTexts[i];
                if (tmp == null)
                    continue;

                GroupEntry entry = FindGroupEntry(group, tmp, null);
                if (entry == null)
                {
                    // A component the game added after the group was registered. The
                    // text it shows now is the game's own text.
                    entry = new GroupEntry { Tmp = tmp, Original = tmp.text };
                    group.Texts.Add(entry);
                }

                entry.Translated = tmp.text;
            }

            UnityEngine.UI.Text[] uiTexts = root.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            for (int i = 0; i < uiTexts.Length; i++)
            {
                UnityEngine.UI.Text ui = uiTexts[i];
                if (ui == null)
                    continue;

                GroupEntry entry = FindGroupEntry(group, null, ui);
                if (entry == null)
                {
                    entry = new GroupEntry { Ui = ui, Original = ui.text };
                    group.Texts.Add(entry);
                }

                entry.Translated = ui.text;
            }
        }

        private static Group FindGroup(GameObject root)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                Group group = groups[i];
                if (group == null)
                    continue;

                if (group.Root == root)
                    return group;
            }

            return null;
        }

        private static GroupEntry FindGroupEntry(Group group, TMP_Text tmp, UnityEngine.UI.Text ui)
        {
            for (int i = 0; i < group.Texts.Count; i++)
            {
                GroupEntry entry = group.Texts[i];
                if (entry == null)
                    continue;

                if (tmp != null && entry.Tmp == tmp)
                    return entry;

                if (ui != null && entry.Ui == ui)
                    return entry;
            }

            return null;
        }

        private static string GetGroupText(GroupEntry entry)
        {
            // Unity's overloaded == reports destroyed objects as null, so a component
            // that died with the previous scene is skipped here.
            if (entry.Tmp != null)
                return entry.Tmp.text;

            if (entry.Ui != null)
                return entry.Ui.text;

            return null;
        }

        private static void SetGroupText(GroupEntry entry, string text)
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

        #endregion
    }
}
