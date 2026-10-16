namespace UltrakULL
{
    /// <summary>
    /// Single source of truth for the mod's identity and human-readable metadata.
    /// Both the BepInEx plugin attribute and the assembly attributes (Windows file
    /// properties) read their values from here, so version and author information
    /// only need to be maintained in one place.
    /// </summary>
    internal static class PluginInfo
    {
        // Used by BepInEx at load time and as the Harmony instance id. Keep these exact
        // values: changing them changes the plugin identity other mods may check for.
        public const string Guid = "clearwater.ultrakill.ultrakull";
        public const string Name = "clearwater.ultrakull.ultrakULL";
        public const string Version = "1.3.1";

        // Human-readable metadata written into the DLL.
        public const string DisplayName = "UltrakULL";
        public const string Description = "A translation mod for ULTRAKILL that supports text, string, audio and texture replacement.";
        public const string Company = "UltrakULL ReFORKED";
        public const string Copyright = "Copyright © 2021-2026 Clearwater and UltrakULL contributors";
    }
}
