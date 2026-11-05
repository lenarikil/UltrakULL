using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace UltrakULL.json
{
    /// <summary>
    /// Result of parsing one catalog snapshot. A snapshot is either accepted
    /// (Error == null, Entries/Warnings populated) or rejected as a whole
    /// (Error != null): a rejected snapshot never replaces a previously loaded
    /// in-memory list or the on-disk cache.
    /// </summary>
    public sealed class CatalogParseResult
    {
        public List<TranslationEntry> Entries = new List<TranslationEntry>();
        public DateTime? DocumentUpdatedAt;
        public List<string> Warnings = new List<string>();
        public string Error;

        public bool IsError
        {
            get { return !string.IsNullOrEmpty(Error); }
        }
    }

    /// <summary>
    /// Pure parser and validator of the online catalog JSON (schema_version 1).
    /// No logging, no file system, no Unity/BepInEx dependencies, so it can be
    /// compiled into the test project. Defensive by design: a few broken records
    /// never reject the whole snapshot, but an unsupported schema version or
    /// structurally broken document does.
    /// </summary>
    public static class CatalogParser
    {
        public const int SupportedSchemaVersion = 1;

        private const int MaxNameLength = 200;
        private const int MaxAuthorsLength = 400;
        private const int MaxDescriptionLength = 8000;
        private const int MaxGalleryItems = 12;
        private const int MaxSocialLinks = 3;

        public static CatalogParseResult Parse(string json)
        {
            CatalogParseResult result = new CatalogParseResult();

            if (string.IsNullOrWhiteSpace(json))
            {
                result.Error = "Catalog snapshot is empty.";
                return result;
            }

            JObject document;
            try
            {
                document = JObject.Parse(json);
            }
            catch (Exception e)
            {
                result.Error = "Catalog snapshot is not valid JSON: " + e.Message;
                return result;
            }

            int schemaVersion;
            if (!TryReadSchemaVersion(document, out schemaVersion) || schemaVersion != SupportedSchemaVersion)
            {
                result.Error = "Unsupported catalog schema version: " + (schemaVersion != 0 ? schemaVersion.ToString(CultureInfo.InvariantCulture) : "missing")
                    + " (supported: " + SupportedSchemaVersion + ").";
                return result;
            }

            result.DocumentUpdatedAt = ParseDateTime(TrimToNull(document.Value<string>("updated_at")));

            JToken translationsToken;
            if (!document.TryGetValue("translations", out translationsToken) || translationsToken.Type == JTokenType.Null)
            {
                // No translations key at all is treated as a valid empty catalog.
                return result;
            }

            if (translationsToken.Type != JTokenType.Array)
            {
                result.Error = "Catalog 'translations' is not an array.";
                return result;
            }

            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (JToken item in translationsToken)
            {
                if (item == null || item.Type != JTokenType.Object)
                {
                    result.Warnings.Add("Skipped a catalog translation that is not a JSON object.");
                    continue;
                }

                CloudCatalogTranslation source;
                try
                {
                    source = item.ToObject<CloudCatalogTranslation>();
                }
                catch (Exception e)
                {
                    result.Warnings.Add("Skipped a broken catalog translation record: " + e.Message);
                    continue;
                }

                if (source == null)
                {
                    result.Warnings.Add("Skipped an empty catalog translation record.");
                    continue;
                }

                string id = TrimToNull(source.id);
                if (id == null)
                {
                    result.Warnings.Add("Skipped a catalog translation without an id.");
                    continue;
                }

                if (!seenIds.Add(id))
                {
                    result.Warnings.Add("Skipped a duplicate catalog id '" + id + "'; the first occurrence wins.");
                    continue;
                }

                result.Entries.Add(MapEntry(id, source, result.Warnings));
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Mapping
        // -------------------------------------------------------------------

        private static TranslationEntry MapEntry(string id, CloudCatalogTranslation source, List<string> warnings)
        {
            string language = TrimToNull(source.language);
            string name = TrimToNull(source.name);

            TranslationEntry entry = new TranslationEntry();
            entry.id = id;
            entry.name = name != null ? Limit(name, MaxNameLength, "name", id, warnings) : null;
            entry.languageTag = language;
            entry.languageName = null; // the catalog has no separate display name field
            entry.version = TrimToNull(source.version);
            entry.author = Limit(TrimToNull(source.authors), MaxAuthorsLength, "authors", id, warnings);
            entry.readme = Limit(TrimToNull(source.description), MaxDescriptionLength, "description", id, warnings);
            entry.likes = source.likes;
            entry.updatedAt = ParseDateTime(TrimToNull(source.updated_at));
            entry.downloadUrl = UrlPolicy.IsHttpUrl(source.download) ? TrimToNull(source.download) : null;
            entry.logoUrl = UrlPolicy.IsAllowedImageUrl(source.logo) ? TrimToNull(source.logo) : null;
            entry.discordUrl = BuildDiscordUrl(source.discord);
            entry.gallery = MapGallery(source.gallery, id, warnings);
            entry.features = MapFeatures(source.features);
            entry.socialLinks = MapSocialLinks(source.social_links);
            return entry;
        }

        private static List<string> MapGallery(List<string> gallery, string id, List<string> warnings)
        {
            if (gallery == null)
            {
                return null;
            }

            List<string> result = null;
            int count = 0;
            foreach (string raw in gallery)
            {
                if (count >= MaxGalleryItems)
                {
                    warnings.Add("Entry '" + id + "': gallery is limited to " + MaxGalleryItems + " images.");
                    break;
                }

                string url = TrimToNull(raw);
                if (url == null || !UrlPolicy.IsAllowedImageUrl(url))
                {
                    continue;
                }

                if (result == null)
                {
                    result = new List<string>();
                }
                result.Add(url);
                count++;
            }
            return result;
        }

        private static TranslationFeatures MapFeatures(CloudCatalogFeatures source)
        {
            // A missing features object means "no information": every flag Unknown.
            TranslationFeatures features = new TranslationFeatures();
            if (source == null)
            {
                return features;
            }

            features.dubbing = ParseFeatureStatus(source.voice_over);
            features.extraDubbing = ParseFeatureStatus(source.additional_voice_over);
            features.textures = ParseFeatureStatus(source.replacement_textures);
            features.fonts = ParseFeatureStatus(source.custom_fonts);
            return features;
        }

        private static List<KeyValuePair<string, string>> MapSocialLinks(List<List<string>> source)
        {
            if (source == null)
            {
                return null;
            }
            List<KeyValuePair<string, string>> result = null;
            int count = 0;
            foreach (var pair in source)
            {
                if (count >= MaxSocialLinks)
                {
                    break;
                }
                if (pair == null || pair.Count < 2)
                {
                    continue;
                }
                string display = TrimToNull(pair[0]);
                string url = TrimToNull(pair[1]);
                if (display == null || url == null)
                {
                    continue;
                }
                if (result == null)
                {
                    result = new List<KeyValuePair<string, string>>();
                }
                result.Add(new KeyValuePair<string, string>(display, url));
                count++;
            }
            return result;
        }

        private static FeatureStatus ParseFeatureStatus(string raw)
        {
            string value = TrimToNull(raw);
            if (value == null)
            {
                return FeatureStatus.Unknown;
            }

            switch (value.ToLowerInvariant())
            {
                case "none": return FeatureStatus.None;
                case "partial": return FeatureStatus.Partial;
                case "full": return FeatureStatus.Full;
                default: return FeatureStatus.Unknown;
            }
        }

        private static string BuildDiscordUrl(CloudCatalogDiscord discord)
        {
            if (discord == null)
            {
                return null;
            }

            ulong guild;
            ulong thread;
            if (!ulong.TryParse(TrimToNull(discord.guild_id), NumberStyles.Integer, CultureInfo.InvariantCulture, out guild))
            {
                return null;
            }
            if (!ulong.TryParse(TrimToNull(discord.thread_id), NumberStyles.Integer, CultureInfo.InvariantCulture, out thread))
            {
                return null;
            }

            return "https://discord.com/channels/"
                + guild.ToString(CultureInfo.InvariantCulture) + "/"
                + thread.ToString(CultureInfo.InvariantCulture);
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static bool TryReadSchemaVersion(JObject document, out int version)
        {
            version = 0;
            JToken token;
            if (!document.TryGetValue("schema_version", out token) || token == null || token.Type == JTokenType.Null)
            {
                return false;
            }

            try
            {
                if (token.Type == JTokenType.Integer)
                {
                    version = token.Value<int>();
                    return true;
                }
                return int.TryParse(TrimToNull(token.ToString()), NumberStyles.Integer, CultureInfo.InvariantCulture, out version);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static DateTime? ParseDateTime(string raw)
        {
            if (raw == null)
            {
                return null;
            }

            DateTime parsed;
            // InvariantCulture + RoundtripKind: the catalog is machine-written ISO
            // timestamps; parsing must not depend on the OS locale.
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
            {
                return parsed;
            }
            return null;
        }

        private static string Limit(string value, int max, string field, string id, List<string> warnings)
        {
            if (value == null || value.Length <= max)
            {
                return value;
            }

            warnings.Add("Entry '" + id + "': " + field + " exceeded " + max + " characters and was truncated.");
            return value.Substring(0, max);
        }

        private static string TrimToNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value.Trim();
        }
    }
}
