using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UltrakULL.json
{
    /// <summary>
    /// The installation record of a language pack, written as
    /// ultrakull_manifest.json next to translation.json. It binds the pack
    /// folder to the catalog's stable translationId (the only reliable
    /// identity of "this exact translation": several translations can share
    /// one locale, and folders can be renamed) and carries the display data
    /// of the pack (name, authors, description, links, local images).
    ///
    /// The manifest is mandatory for every pack. Installs from the online
    /// catalog write it (source "catalog"); packs found without one are
    /// migrated by <see cref="ManifestMigrator"/>, which generates it from
    /// the pack's own metadata (source "manual"); the built-in English keeps
    /// an in-memory manifest (source "builtin"). Readers treat the manifest
    /// as the authoritative identity and fall back to translation.json
    /// metadata only when the manifest is missing or broken.
    ///
    /// Version history:
    ///  - v1: identity + local image paths (logo/flag/gallery).
    ///  - v2: adds authors, description, discordUrl and source.
    /// </summary>
    public class TranslationManifest
    {
        public const string FileName = "ultrakull_manifest.json";
        public const int CurrentVersion = 2;

        public int manifestVersion = CurrentVersion;
        public string translationId;
        public string name;
        public string languageCode;
        public string authors;
        public string version;
        public string description;
        public string discordUrl;

        /// <summary>
        /// Where the pack came from: "catalog" (online browser install),
        /// "manual" (migrated hand-made pack) or "builtin" (built-in English).
        /// </summary>
        public string source;

        public string installedAt;

        /// <summary>Like count from the catalog at install time (null when unknown).</summary>
        public int? likes;

        /// <summary>
        /// Social links at install time: JSON array of [display_text, url] pairs.
        /// Null when unknown.
        /// </summary>
        public string socialLinksJson;

        /// <summary>
        /// Capability statuses at install time, one JSON object per feature:
        /// {"voice_over":"Full","textures":"Partial",...}. Null when unknown.
        /// </summary>
        public string featuresJson;

        /// <summary>Local path to the logo image (relative to the pack folder), or null.</summary>
        public string logoPath;

        /// <summary>Local path to the flag image (relative to the pack folder), or null.</summary>
        public string flagPath;

        /// <summary>Local paths to gallery images (relative to the pack folder), or null/empty.</summary>
        public List<string> galleryPaths;

        /// <summary>
        /// Reads a manifest. Returns null when the file does not exist; throws when
        /// it exists but cannot be parsed, so callers can distinguish a missing
        /// manifest (not migrated yet) from a broken one.
        /// </summary>
        public static TranslationManifest Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<TranslationManifest>(File.ReadAllText(filePath));
        }

        public static void Save(string filePath, TranslationManifest manifest)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
        }

        /// <summary>Serializes a <see cref="TranslationFeatures"/> into the manifest's featuresJson.</summary>
        public static string FeaturesToJson(TranslationFeatures features)
        {
            if (features == null)
            {
                return null;
            }
            var dict = new Dictionary<string, string>
            {
                ["voice_over"] = StatusToString(features.dubbing),
                ["additional_voice_over"] = StatusToString(features.extraDubbing),
                ["replacement_textures"] = StatusToString(features.textures),
                ["custom_fonts"] = StatusToString(features.fonts),
            };
            return JsonConvert.SerializeObject(dict);
        }

        /// <summary>Deserializes the manifest's featuresJson into a <see cref="TranslationFeatures"/>.</summary>
        public static TranslationFeatures FeaturesFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (dict == null)
                {
                    return null;
                }
                var features = new TranslationFeatures();
                if (dict.TryGetValue("voice_over", out var v))
                {
                    features.dubbing = ParseStatus(v);
                }
                if (dict.TryGetValue("additional_voice_over", out var a))
                {
                    features.extraDubbing = ParseStatus(a);
                }
                if (dict.TryGetValue("replacement_textures", out var t))
                {
                    features.textures = ParseStatus(t);
                }
                if (dict.TryGetValue("custom_fonts", out var f))
                {
                    features.fonts = ParseStatus(f);
                }
                return features;
            }
            catch
            {
                return null;
            }
        }

        private static string StatusToString(FeatureStatus s)
        {
            switch (s)
            {
                case FeatureStatus.Full:
                    return "Full";
                case FeatureStatus.Partial:
                    return "Partial";
                case FeatureStatus.None:
                    return "None";
                case FeatureStatus.Broken:
                    return "Broken";
                default:
                    return "Unknown";
            }
        }

        private static FeatureStatus ParseStatus(string s)
        {
            switch (s?.ToLowerInvariant())
            {
                case "full":
                    return FeatureStatus.Full;
                case "partial":
                    return FeatureStatus.Partial;
                case "none":
                    return FeatureStatus.None;
                case "broken":
                    return FeatureStatus.Broken;
                default:
                    return FeatureStatus.Unknown;
            }
        }

        /// <summary>Serializes <see cref="List{KeyValuePair{string, string}}"/> social links into JSON.</summary>
        public static string SocialLinksToJson(List<KeyValuePair<string, string>> links)
        {
            if (links == null || links.Count == 0)
            {
                return null;
            }
            var arr = new List<object>();
            foreach (var kvp in links)
            {
                arr.Add(new[] { kvp.Key, kvp.Value });
            }
            return JsonConvert.SerializeObject(arr);
        }

        /// <summary>Deserializes social links JSON into <see cref="List{KeyValuePair{string, string}}"/>.</summary>
        public static List<KeyValuePair<string, string>> SocialLinksFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            try
            {
                var arr = JsonConvert.DeserializeObject<List<List<string>>>(json);
                if (arr == null)
                {
                    return null;
                }
                var links = new List<KeyValuePair<string, string>>();
                foreach (var pair in arr)
                {
                    if (pair != null && pair.Count >= 2 && !string.IsNullOrEmpty(pair[0]) && !string.IsNullOrEmpty(pair[1]))
                    {
                        links.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
                    }
                }
                return links.Count > 0 ? links : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
