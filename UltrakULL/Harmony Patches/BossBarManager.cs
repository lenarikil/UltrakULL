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

        public static class BossNameHelper
        {
            // Rebuilt only when the active language changes; avoids reflecting over the
            // enemyNames fields on every boss-bar update (called every frame).
            private static object cachedSource;
            private static HashSet<string> cachedLocalizedNames;

            public static bool IsAlreadyLocalized(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return false;

                var enemyNames = LanguageManager.CurrentLanguage.enemyNames;
                if (!ReferenceEquals(cachedSource, enemyNames))
                {
                    cachedSource = enemyNames;
                    cachedLocalizedNames = BuildLocalizedNames(enemyNames);
                }

                return cachedLocalizedNames != null && cachedLocalizedNames.Contains(name);
            }

            private static HashSet<string> BuildLocalizedNames(object enemyNames)
            {
                var localizedNames = new HashSet<string>();
                if (enemyNames == null)
                    return localizedNames;

                foreach (var field in enemyNames.GetType().GetFields())
                {
                    var value = field.GetValue(enemyNames)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                        localizedNames.Add(value);
                }

                return localizedNames;
            }
        }

        private static readonly HashSet<string> loggedNames = new HashSet<string>();

        private static void LocalizeName(BossHealthBar bossBar)
        {
            if (!BossNameHelper.IsAlreadyLocalized(bossBar.bossName))
            {
                string translatedName = BossStrings.GetBossName(bossBar.bossName);
                if (!string.IsNullOrEmpty(translatedName))
                {
                    bossBar.bossName = translatedName;
                }
                else
                {
                    if (loggedNames.Add(bossBar.bossName)) // adds and returns true if not present
                    {
                        Logging.Warn($"Boss name '{bossBar.bossName}' not found in localization. Using default.");
                    }
                }
            }
            else
            {
                if (loggedNames.Add(bossBar.bossName))
                {
                    Logging.Info($"Boss name '{bossBar.bossName}' is already localized. Skipping translation.");
                }
            }
        }
    }
}
