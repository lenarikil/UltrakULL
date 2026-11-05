using System;
using System.Globalization;

namespace UltrakULL
{
    /// <summary>
    /// Human-friendly display names for language tags. Converts BCP-47 tags
    /// ("ru-RU") into the language's own name ("Русский") through the .NET
    /// culture database; unknown tags fall back to the short uppercase code
    /// ("RU"). Used wherever the browser shows or sorts by a language.
    /// </summary>
    public static class LanguageDisplay
    {
        /// <summary>
        /// "ru-RU" -> "Русский" (the language's own name, region part dropped);
        /// unknown or malformed tags fall back to <see cref="ShortCode"/>.
        /// </summary>
        public static string Name(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return "";
            }

            try
            {
                CultureInfo culture = new CultureInfo(tag.Trim());
                string native = culture.NativeName;
                if (!string.IsNullOrEmpty(native))
                {
                    int paren = native.IndexOf('(');
                    if (paren > 0)
                    {
                        native = native.Substring(0, paren).Trim();
                    }
                    if (native.Length > 0)
                    {
                        return Capitalize(native);
                    }
                }
            }
            catch (CultureNotFoundException)
            {
            }
            catch (ArgumentException)
            {
            }

            return ShortCode(tag);
        }

        /// <summary>"ru-RU" -> "RU" (primary subtag only, uppercased).</summary>
        public static string ShortCode(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return "";
            }
            string trimmed = tag.Trim();
            int dash = trimmed.IndexOf('-');
            string primary = dash > 0 ? trimmed.Substring(0, dash) : trimmed;
            return primary.ToUpperInvariant();
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }
    }
}
