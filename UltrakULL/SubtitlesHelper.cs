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
        /// Gets a subtitle string for the current language, falling back to the supplied
        /// original (vanilla) text when the language has no value for the field. This lets
        /// translated transpilers keep the game's own string in built-in English or when a
        /// translation is missing a key, instead of producing a null/blank subtitle.
        /// </summary>
        public static string GetTextOrFallback(string fieldName, string fallback)
        {
            var text = GetText(fieldName);

            // The key is remembered with the text, so a subtitle that is on screen can
            // be written again in another language.
            LiveSubtitles.RegisterText(fieldName, text);

            return string.IsNullOrEmpty(text) ? fallback : text;
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
            {
                LiveSubtitles.RegisterText(fieldName, text);
                return text;
            }

            var colored = "<color=#" + colorHex + ">" + text + "</color>";
            LiveSubtitles.RegisterText(fieldName, colored, colorHex);
            return colored;
        }
    }
}