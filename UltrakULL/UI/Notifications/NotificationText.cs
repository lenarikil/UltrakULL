namespace UltrakULL.UI.Notifications
{
    // Built-in English fallbacks for localized notification strings.
    // Language files may leave these keys empty/null; then the English text is used.
    public static class NotificationText
    {
        public const string StartupTitle = "UltrakULL v{version} loaded";
        public const string StartupLanguageLine = "Language: {lang}";
        public const string StartupUpToDate = "You are using the latest version.";
        public const string StartupUpdateAvailable = "Update available: v{latestVersion}.";
        public const string StartupUpdateCheckFailed = "Unable to check for updates. Check console for info.";
        public const string StartupViewUpdate = "VIEW UPDATE";

        public const string LanguageChangedTitle = "Language changed to {lang}";
        public const string LanguageChangedMenuBody = "The new language is now active.";
        public const string LanguageChangedLevelBody = "Language changes will not fully take effect until the current mission is quit or restarted.";

        public const string Downloading = "Downloading language file...";
        public const string DownloadUpdated = "Language file \"{lang}\" has been updated.";
        public const string DownloadNew = "A new language file \"{lang}\" has been downloaded.";
        public const string DownloadSelectHint = " Select it in Language tab.";
        public const string DownloadError = "Download failed. Check console for details.";

        // Resolves a localized notification string: empty/null falls back to the
        // built-in English text, then {placeholders} are substituted.
        // Placeholders are passed as key/value pairs: Localize(s, def, "lang", name).
        public static string Localize(string localized, string englishDefault, params string[] keysAndValues)
        {
            string result = string.IsNullOrEmpty(localized) ? englishDefault : localized;
            for (int i = 0; i + 1 < keysAndValues.Length; i += 2)
            {
                result = result.Replace("{" + keysAndValues[i] + "}", keysAndValues[i + 1] ?? string.Empty);
            }
            return result;
        }
    }
}
