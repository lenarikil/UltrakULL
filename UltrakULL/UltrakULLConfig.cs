using BepInEx.Configuration;

namespace UltrakULL
{
    /// <summary>
    /// Central registration of every entry written to BepInEx/config/ultrakull/Settings.cfg.
    /// Registering all entries in one place guarantees that each option gets a helpful
    /// comment (description) in the config file. Other code only reads these entries,
    /// so registering them here first is enough.
    /// </summary>
    public static class UltrakULLConfig
    {
        public const string GeneralSection = "General";
        public const string PerformanceSection = "Performance";

        public static void Register(ConfigFile config)
        {
            if (config == null)
                return;

            Define(config, GeneralSection, "LastLanguage", "en-GB",
                "Language code of the last selected language. Managed automatically; you normally do not need to edit this.\n" +
                "Allowed values: any language folder name under BepInEx/config/ultrakull (for example en-GB, ru-RU, fr-FR).");

            Define(config, GeneralSection, "activeDubbing", "False",
                "Enable dubbed voice lines by replacing the original voice audio with the language pack.\n" +
                "Allowed values: True, False.");

            Define(config, GeneralSection, "extraDubbing", "False",
                "Enable extra dubbing for additional, non-essential voiced lines.\n" +
                "Allowed values: True, False.");

            Define(config, GeneralSection, "bookAudioDubbing", "False",
                "Enable dubbed audio for books and terminals.\n" +
                "Allowed values: True, False.");

            Define(config, GeneralSection, "audioPreloadMode", "Scene",
                "Determines when dubbed audio is loaded into memory.\n" +
                "Allowed values: Scene (load on each scene), StartupAll (load everything at startup; slower start but no in-game hitches), Off (do not preload).");

            Define(config, GeneralSection, "debugAudioSwap", "False",
                "Write detailed audio-swap diagnostics to the BepInEx log. Useful when debugging missing or wrong audio.\n" +
                "Allowed values: True, False.");

            var verboseEntry = Define(config, GeneralSection, "verboseLogging", "False",
                "Write high-frequency per-item messages (each texture, clip and audio file loaded) to the BepInEx log.\n" +
                "Leave off to keep the log small and avoid logging overhead during play.\n" +
                "Allowed values: True, False.");
            Logging.Verbose = verboseEntry.Value != "False";

            Define(config, GeneralSection, "voiceSpatialOverride", "Original",
                "Spatial settings applied to swapped voice lines. Reserved option kept for compatibility.\n" +
                "Allowed values: Original (keep the game's own spatial settings).");

            Define(config, PerformanceSection, "texturePatchFrameBudgetMs", "Auto",
                "Maximum main-thread time (in milliseconds) spent replacing textures per frame during normal play and late object activations.\n" +
                "Higher values make replacements appear sooner but each frame does more work; lower values keep frames smoother but replacements take longer.\n" +
                "Allowed values: Auto, or a positive number (for example 4, 8, 12, 16). Auto adapts to your CPU/RAM/VRAM.");

            Define(config, PerformanceSection, "initialPassBudgetMs", "Auto",
                "Frame budget (in milliseconds) for the initial texture pass right after a scene loads.\n" +
                "The initial pass is a one-time operation, so a larger budget makes replacements show up faster with little cost.\n" +
                "Allowed values: Auto (uses 3x the normal budget, capped at 24 ms), or a positive number.");

            Define(config, PerformanceSection, "activationHooks", "True",
                "Hook the game's trigger/event methods so objects activated by events get their textures replaced immediately.\n" +
                "Allowed values: True, False.");

            Define(config, PerformanceSection, "rescanEnabled", "True",
                "Periodically rescan the scene for newly activated or missed objects as a safety net.\n" +
                "Allowed values: True, False.");

            Define(config, PerformanceSection, "rescanIntervalSeconds", "Auto",
                "Seconds between background rescans.\n" +
                "Allowed values: Auto (adapts to your CPU/RAM/VRAM), or a positive number (for example 0.25, 0.5, 1, 2).");

            config.Save();
        }

        private static ConfigEntry<string> Define(ConfigFile config, string section, string key, string defaultValue, string description)
        {
            return config.Bind(section, key, defaultValue, new ConfigDescription(description));
        }
    }
}
