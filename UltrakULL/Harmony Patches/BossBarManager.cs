using System.Collections.Generic;
using HarmonyLib;
using UltrakULL.json;


namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch]
    public static class LocalizeBossBar
    {
        // Patch for CreateBossBar
        [HarmonyPatch(typeof(BossBarManager), "CreateBossBar")]
        [HarmonyPrefix]
        public static void CreateBossBar_Prefix(BossHealthBar bossBar)
        {
            LocalizeName(bossBar);
        }

        // Patch for UpdateBossBar
        [HarmonyPatch(typeof(BossBarManager), "UpdateBossBar")]
        [HarmonyPrefix]
        public static void UpdateBossBar_Prefix(BossHealthBar bossBar)
        {
            LocalizeName(bossBar);
        }

        /// <summary>
        /// The name of one boss bar, tracked across languages.
        /// </summary>
        private sealed class BossNameState
        {
            /// <summary>The name the game itself wrote last (spawn, phase rename).</summary>
            public string GameName;

            /// <summary>The translation the mod wrote last, null when it wrote none.</summary>
            public string WrittenName;

            /// <summary>The language the translation was written for.</summary>
            public object Language;
        }

        private static readonly Dictionary<BossHealthBar, BossNameState> nameStates = new Dictionary<BossHealthBar, BossNameState>();

        private static readonly HashSet<string> loggedNames = new HashSet<string>();

        /// <summary>
        /// Translates the boss names of the boss bars that are currently in the
        /// scene again. The boss bar is only updated by the game while the game
        /// runs, so a language switch has to do it itself.
        /// </summary>
        internal static void Reapply()
        {
            BossHealthBar[] bossBars = UnityEngine.Object.FindObjectsOfType<BossHealthBar>(true);
            for (int i = 0; i < bossBars.Length; i++)
                LocalizeName(bossBars[i]);
        }

        /// <summary>
        /// Drops the tracked names of the boss bars that are gone. Called when a
        /// scene is left, because the boss bars belong to that scene.
        /// </summary>
        internal static void PruneDead()
        {
            if (nameStates.Count == 0)
                return;

            List<BossHealthBar> dead = null;
            foreach (KeyValuePair<BossHealthBar, BossNameState> pair in nameStates)
            {
                if (pair.Key == null)
                {
                    if (dead == null)
                        dead = new List<BossHealthBar>();
                    dead.Add(pair.Key);
                }
            }

            if (dead == null)
                return;

            for (int i = 0; i < dead.Count; i++)
                nameStates.Remove(dead[i]);
        }

        private static void LocalizeName(BossHealthBar bossBar)
        {
            if (bossBar == null)
                return;

            BossNameState state;
            if (!nameStates.TryGetValue(bossBar, out state))
            {
                state = new BossNameState();
                nameStates[bossBar] = state;
            }

            string current = bossBar.bossName;
            object language = LanguageManager.CurrentLanguage;

            // The common case: the name on the bar is the translation of the active
            // language and the game has not renamed the boss. Nothing to do.
            if (state.WrittenName != null && current == state.WrittenName && ReferenceEquals(state.Language, language))
                return;

            // The game wrote a name of its own: a boss spawned, or a phase gave the
            // boss a new name. That name is the baseline every language works from.
            if (state.WrittenName == null || current != state.WrittenName)
                state.GameName = current;

            // Built-in English keeps the game's own boss names.
            if (UltrakULL.CommonFunctions.isUsingEnglish())
            {
                if (state.WrittenName != null && state.GameName != null && bossBar.bossName != state.GameName)
                    bossBar.bossName = state.GameName;

                state.WrittenName = null;
                state.Language = null;
                return;
            }

            string translatedName = state.GameName != null ? BossStrings.GetBossName(state.GameName) : null;
            if (string.IsNullOrEmpty(translatedName) || translatedName == state.GameName)
            {
                // The boss has no translation, the game's own name stays. A name of
                // the previous language is replaced instead of being kept.
                if (loggedNames.Add(state.GameName))
                    Logging.Warn($"Boss name '{state.GameName}' not found in localization. Using default.");

                if (bossBar.bossName != state.GameName)
                    bossBar.bossName = state.GameName;

                state.WrittenName = null;
                state.Language = null;
                return;
            }

            bossBar.bossName = translatedName;
            state.WrittenName = translatedName;
            state.Language = language;
        }
    }
}
