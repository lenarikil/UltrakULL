using System;
using UltrakULL.json;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Queries about the currently active translation pack, so views never poke
    /// LanguageManager state directly.
    /// </summary>
    public static class ActiveLanguageState
    {
        /// <summary>
        /// True when the given pack key (language folder name) is the currently
        /// selected language. Comparison is ordinal-ignore-case, like everywhere
        /// else pack folders are matched.
        /// </summary>
        public static bool IsPackInUse(string packKey)
        {
            if (string.IsNullOrEmpty(packKey))
            {
                return false;
            }
            return LanguageManager.CurrentLanguage != null
                && string.Equals(LanguageManager.CurrentLanguage.PackKey, packKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
