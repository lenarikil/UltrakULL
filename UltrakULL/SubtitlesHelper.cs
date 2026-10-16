using UltrakULL.json;

namespace UltrakULL
{
    /// <summary>
    /// Helper class for accessing subtitle text at runtime.
    /// Used by Harmony Transpilers to get current-language subtitle text
    /// instead of capturing it at IL patch time.
    /// </summary>
    public static class SubtitlesHelper
    {
        /// <summary>
        /// Gets a subtitle string from the current language by field name.
        /// Thread-safe. Returns null if the field doesn't exist.
        /// </summary>
        public static string GetText(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return null;

            var currentLang = LanguageManager.CurrentLanguage;
            if (currentLang == null)
                return null;

            var subtitles = currentLang.subtitles;
            if (subtitles == null)
                return null;

            // Use the existing GetField method on Subtitles class
            return subtitles.GetField(fieldName);
        }

        /// <summary>
        /// Gets a subtitle string with color tags wrapped around it.
        /// </summary>
        public static string GetTextWithColor(string fieldName, string colorHex)
        {
            var text = GetText(fieldName);
            if (string.IsNullOrEmpty(text))
                return null;

            if (string.IsNullOrEmpty(colorHex))
                return text;

            return "<color=#" + colorHex + ">" + text + "</color>";
        }
    }
}