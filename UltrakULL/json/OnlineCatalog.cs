using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UltrakULL.json
{
    /// <summary>
    /// State of a dubbing/textures/fonts capability of an online translation.
    ///
    /// The values are deliberately ordered so that the default (missing data in JSON)
    /// is <see cref="Unknown"/> rather than <see cref="None"/>. This lets the UI
    /// distinguish "feature is absent" (None) from "no information about it was
    /// provided by the data source" (Unknown), as required by the interface spec.
    /// </summary>
    public enum FeatureStatus
    {
        Unknown,
        None,
        Full,
        Partial,
        Broken
    }

    /// <summary>
    /// The four capability flags shown on every translation card and in the details panel.
    /// Fields are nullable-safe: a missing JSON value is serialized as Unknown.
    /// </summary>
    public class TranslationFeatures
    {
        [JsonConverter(typeof(FeatureStatusConverter))]
        public FeatureStatus dubbing;
        [JsonConverter(typeof(FeatureStatusConverter))]
        public FeatureStatus extraDubbing;
        [JsonConverter(typeof(FeatureStatusConverter))]
        public FeatureStatus textures;
        [JsonConverter(typeof(FeatureStatusConverter))]
        public FeatureStatus fonts;
    }

    /// <summary>
    /// A single contributor of a translation (translator, editor, voice actor, etc.).
    /// Every field is optional.
    /// </summary>
    public class TranslationAuthor
    {
        public string name;
        public string role;
        public string avatarUrl;
        public string link;
    }

    /// <summary>
    /// One translation entry. Shared by the online catalog (mapped by
    /// CatalogParser from the Cloudflare catalog.json) and by the browser's
    /// Installed tab (synthesized from local pack data).
    ///
    /// The <see cref="id"/> is the unique identifier of a translation and is NOT
    /// the target language: multiple teams may provide separate translations for
    /// the same language, and each one is its own entry with its own logo,
    /// version and likes.
    /// </summary>
    public class TranslationEntry
    {
        public string id;

        /// <summary>Display name of the translation itself (catalog "name").</summary>
        public string name;

        public string languageTag;

        /// <summary>
        /// Display name of the language. Only set for locally synthesized entries;
        /// the online catalog has no such field and leaves it null.
        /// </summary>
        public string languageName;

        // --- Legacy v1-era fields. The online catalog never fills them; only the
        // locally synthesized Installed-tab entries (and any future legacy data
        // source) may use them. Kept for compatibility, not removed. ---
        public string teamName;
        public string author;
        public string version;

        // Null when the source does not report like counts.
        public int? likes;

        public string logoUrl;

        // Small flag image of the target language, drawn over the bottom-right corner
        // of the logo. Null means "no flag artwork" (the UI then shows a code badge).
        // Legacy field: the online catalog has no flagUrl, flags come from FlagProvider.
        public string flagUrl;

        // Gallery/preview images. Null/empty means "no gallery".
        public List<string> gallery;

        public TranslationFeatures features;

        // Ordered list of contributors, first entries are considered the main authors.
        // Legacy field: the online catalog ships authors as one plain string (author).
        public List<TranslationAuthor> authors;

        // Free-form text provided by the authors (catalog "description").
        public string readme;

        // Direct download URL of the language pack (a file or an indirect page on a
        // known host; resolved by PackInstaller). Null hides the download button.
        public string downloadUrl;

        // Link to the original Discord publication thread, when the catalog provided
        // valid coordinates.
        public string discordUrl;

        // Last update time of this entry as reported by the catalog.
        public DateTime? updatedAt;

        /// <summary>
        /// True for the built-in English pseudo-entry (the original game text,
        /// not a user translation). The UI shows a dedicated preview without
        /// translation metadata: no version, authors, features or likes.
        /// </summary>
        public bool isBuiltinEnglish;

        // Social links from the catalog entry: list of (display text, URL).
        // Null/empty means "no social links".
        public List<KeyValuePair<string, string>> socialLinks;
    }

    /// <summary>
    /// Safely converts feature status strings ("full", "partial", "none", "broken",
    /// "unknown") to <see cref="FeatureStatus"/>. Unknown values and JSON nulls fall
    /// back to <see cref="FeatureStatus.Unknown"/> so a malformed entry never breaks
    /// deserialization of the whole catalog.
    /// </summary>
    public class FeatureStatusConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FeatureStatus);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string raw = (string)reader.Value;
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    switch (raw.Trim().ToLowerInvariant())
                    {
                        case "full": return FeatureStatus.Full;
                        case "partial": return FeatureStatus.Partial;
                        case "none": return FeatureStatus.None;
                        case "broken": return FeatureStatus.Broken;
                        case "unknown": return FeatureStatus.Unknown;
                    }
                }
            }

            return FeatureStatus.Unknown;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            FeatureStatus status = (FeatureStatus)value;
            string text;
            switch (status)
            {
                case FeatureStatus.Full: text = "full"; break;
                case FeatureStatus.Partial: text = "partial"; break;
                case FeatureStatus.None: text = "none"; break;
                case FeatureStatus.Broken: text = "broken"; break;
                default: text = "unknown"; break;
            }
            writer.WriteValue(text);
        }
    }
}