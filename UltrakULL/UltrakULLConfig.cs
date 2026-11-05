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
                "Language key of the last selected language (the language pack folder name).\n" +
                "Managed automatically; you normally do not need to edit this.\n" +
                "Allowed values: any language folder name under BepInEx/config/ultrakull (for example en-GB, ru-RU, fr-FR).");

            Define(config, GeneralSection, "catalogUrl", UltrakULL.json.CatalogService.CatalogUrl,
                "URL of the online translation catalog (catalog.json) used by the in-game browser.\n" +
                "Leave the default value unless you want to point the browser at a mirror or a local test server.\n" +
                "Allowed values: any http(s) URL, or empty to use the default.");

            Define(config, GeneralSection, "activeDubbing", "False",
                "Enable dubbed voice lines by replacing the original voice audio with the language pack.\n" +
                "Allowed values: True, False.");

            Define(config, GeneralSection, "extraDubbing", "False",
                "Enable extra dubbing for additional, non-essential voiced lines.\n" +
                "Allowed values: True, False.");

            Define(config, GeneralSection, "bookAudioDubbing", "False",
                "Enable dubbed audio for books and terminals.\n" +
                "Allowed values: True, False.");

            Define(config, GeneralSection, "replaceTextures", "True",
                "Replace game textures with the language pack's translated textures.\n" +
                "Can also be toggled live from the mod's Settings tab in the options menu.\n" +
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
                "Allowed values: Auto, or a positive number (for example 3, 4, 8). Auto uses a conservative budget based on your CPU/RAM/VRAM.");

            Define(config, PerformanceSection, "initialPassBudgetMs", "Auto",
                "Frame budget (in milliseconds) for the initial texture pass right after a scene loads.\n" +
                "A larger budget makes replacements show up faster but can still cause frame drops while the pass runs.\n" +
                "Allowed values: Auto (uses up to 2x the normal budget, capped at 8 ms), or a positive number.");

            Define(config, PerformanceSection, "activationHooks", "True",
                "Hook the game's trigger/event methods so objects activated by events get their textures replaced immediately.\n" +
                "Allowed values: True, False.");

            Define(config, PerformanceSection, "rescanEnabled", "True",
                "Periodically rescan the scene for newly activated or missed objects as a safety net.\n" +
                "Allowed values: True, False.");

            Define(config, PerformanceSection, "rescanIntervalSeconds", "Auto",
                "Seconds between background rescans.\n" +
                "Allowed values: Auto (uses a conservative interval based on your CPU/RAM/VRAM), or a positive number (for example 0.5, 1, 2).");

            config.Save();
        }

        private static ConfigEntry<string> Define(ConfigFile config, string section, string key, string defaultValue, string description)
        {
            return config.Bind(section, key, defaultValue, new ConfigDescription(description));
        }
    }
}
