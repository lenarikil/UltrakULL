using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UltrakULL.json
{
    /// <summary>
    /// How the online translation list is ordered.
    /// </summary>
    public enum CatalogSortMode
    {
        Popularity,
        Language,
        Team
    }

    /// <summary>
    /// Loads, caches and queries the online translation catalog
    /// (languagesMaster.json, schema v2). The legacy v1 format is still parsed so
    /// old deployments keep working until the data source is migrated.
    ///
    /// This class is UI-agnostic: BrowserState (UI layer) consumes the resulting
    /// <see cref="TranslationEntry"/> list.
    /// </summary>
    public static class CatalogService
    {
        public const string MasterCatalogUrl = "https://clearwateruk.github.io/mods/ultrakULL/languagesMaster.json";

        // Legacy download URL scheme, used when an entry has no explicit downloadUrl.
        private const string LegacyLanguageFileUrl = "https://clearwateruk.github.io/mods/ultrakULL/{0}.json";

        private static readonly HttpClient Client = new HttpClient();

        private static List<TranslationEntry> _entries = new List<TranslationEntry>();
        private static volatile bool _loading;
        private static bool _loaded;
        private static string _lastError;

        public static IReadOnlyList<TranslationEntry> Entries
        {
            get { return _entries; }
        }

        public static int Count
        {
            get { return _entries.Count; }
        }

        public static bool IsLoaded
        {
            get { return _loaded; }
        }

        public static bool IsLoading
        {
            get { return _loading; }
        }

        /// <summary>Human-readable error of the last failed attempt, or null if none.</summary>
        public static string LastError
        {
            get { return _lastError; }
        }

        /// <summary>
        /// Fetches the catalog (unless already cached) and returns whether the list is ready.
        /// Never throws; failures are recorded in <see cref="LastError"/>.
        /// </summary>
        public static async Task<bool> LoadAsync(bool force = false)
        {
            if (_loading)
            {
                return _loaded;
            }
            if (_loaded && !force)
            {
                return true;
            }

            _loading = true;
            _lastError = null;
            try
            {
                Client.Timeout = TimeSpan.FromSeconds(10);
                Logging.Warn("Obtaining online translations from UltrakULL repo...");
                string responseJsonRaw = await Client.GetStringAsync(MasterCatalogUrl);
                _entries = Parse(responseJsonRaw);
                _loaded = true;
                Logging.Info("[CatalogService] Loaded " + _entries.Count + " translation entries.");
                return true;
            }
            catch (Exception e)
            {
                _lastError = e.Message;
                Logging.Error("[CatalogService] Failed to load online translation catalog: " + e);
                return false;
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>
        /// Returns entries whose language name/tag, team, author or contributor list
        /// matches the query (case-insensitive). An empty/whitespace query returns all.
        /// </summary>
        public static List<TranslationEntry> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<TranslationEntry>(_entries);
            }

            string q = query.Trim();
            var result = new List<TranslationEntry>();
            foreach (TranslationEntry entry in _entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (ContainsIgnoreCase(entry.languageName, q) ||
                    ContainsIgnoreCase(entry.languageTag, q) ||
                    ContainsIgnoreCase(entry.teamName, q) ||
                    ContainsIgnoreCase(entry.author, q) ||
                    AnyAuthorContains(entry.authors, q))
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// Stable sort of a translation list. Entries without sortable data (null likes,
        /// missing names) sink to the end instead of being dropped.
        /// </summary>
        public static List<TranslationEntry> Sort(List<TranslationEntry> source, CatalogSortMode mode)
        {
            if (source == null)
            {
                return new List<TranslationEntry>();
            }

            switch (mode)
            {
                case CatalogSortMode.Popularity:
                    return source
                        .OrderByDescending(e => e.likes ?? int.MinValue)
                        .ToList();
                case CatalogSortMode.Language:
                    return source
                        .OrderBy(e => e.languageName ?? "", StringComparer.OrdinalIgnoreCase)
                        .ThenBy(e => e.languageTag ?? "", StringComparer.OrdinalIgnoreCase)
                        .ToList();
                case CatalogSortMode.Team:
                    return source
                        .OrderBy(e => e.teamName ?? "", StringComparer.OrdinalIgnoreCase)
                        .ThenBy(e => e.author ?? "", StringComparer.OrdinalIgnoreCase)
                        .ToList();
                default:
                    return new List<TranslationEntry>(source);
            }
        }

        /// <summary>
        /// True when the specific translation (not merely the language) from the online
        /// catalog is installed locally. Matching is by the stable catalog id only —
        /// the local pack is stored under that same id as its <c>langName</c> — so two
        /// different translations of the same language are never confused. The built-in
        /// synthetic English pack (no <c>RootFolder</c>) is never treated as an installed
        /// download, so an online <c>en-GB</c> entry correctly reads as available.
        /// </summary>
        public static bool IsInstalled(TranslationEntry entry)
        {
            return FindInstalledPack(entry) != null;
        }

        /// <summary>
        /// True when the translation is installed but the catalog lists a newer version
        /// than the one stored locally. Never true for packs with no comparable version.
        /// </summary>
        public static bool IsOutdated(TranslationEntry entry)
        {
            JsonFormat local = FindInstalledPack(entry);
            if (local == null || local.metadata == null)
            {
                return false;
            }

            string catalogVersion = NormalizeVersion(entry != null ? entry.version : null);
            string localVersion = NormalizeVersion(local.metadata.langVersion);
            if (catalogVersion.Length == 0 || localVersion.Length == 0)
            {
                return false;
            }
            return !string.Equals(catalogVersion, localVersion, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finds the locally installed pack that corresponds to a catalog entry, or null.
        /// Only packs actually present on disk (a real <c>RootFolder</c>) are considered;
        /// the in-memory synthetic English placeholder is skipped.
        /// </summary>
        private static JsonFormat FindInstalledPack(TranslationEntry entry)
        {
            if (entry == null || LanguageManager.allLanguages == null)
            {
                return null;
            }

            // Primary key: the stable catalog id, which the local pack stores as langName.
            if (!string.IsNullOrEmpty(entry.id))
            {
                JsonFormat pack;
                if (LanguageManager.allLanguages.TryGetValue(entry.id, out pack) && IsRealPack(pack))
                {
                    return pack;
                }
            }
            return null;
        }

        /// <summary>A pack is a real download only if it lives on disk (has a RootFolder).</summary>
        private static bool IsRealPack(JsonFormat pack)
        {
            return pack != null && !string.IsNullOrEmpty(pack.RootFolder);
        }

        /// <summary>Normalizes a version string for comparison ("v1.3.1" -> "1.3.1").</summary>
        private static string NormalizeVersion(string version)
        {
            string v = (version ?? "").Trim();
            if (v.Length > 1 && (v[0] == 'v' || v[0] == 'V'))
            {
                v = v.Substring(1).Trim();
            }
            return v;
        }

        public static TranslationEntry FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (TranslationEntry entry in _entries)
            {
                if (entry != null && string.Equals(entry.id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// Resolves the download URL of an entry: explicit downloadUrl when provided,
        /// otherwise the legacy per-language-tag URL scheme.
        /// </summary>
        public static string GetDownloadUrl(TranslationEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(entry.downloadUrl))
            {
                return entry.downloadUrl;
            }

            if (string.IsNullOrWhiteSpace(entry.languageTag))
            {
                return null;
            }

            return string.Format(LegacyLanguageFileUrl, entry.languageTag);
        }

        private static List<TranslationEntry> Parse(string responseJsonRaw)
        {
            if (string.IsNullOrWhiteSpace(responseJsonRaw))
            {
                return new List<TranslationEntry>();
            }

            // Schema v2: { "schemaVersion": 2, "translations": [ ... ] }
            OnlineCatalogDocument doc = JsonConvert.DeserializeObject<OnlineCatalogDocument>(responseJsonRaw);
            if (doc != null && doc.translations != null && doc.translations.Count > 0)
            {
                var v2 = new List<TranslationEntry>(doc.translations.Count);
                foreach (TranslationEntry entry in doc.translations)
                {
                    TranslationEntry normalized = Normalize(entry);
                    if (normalized != null && !string.IsNullOrWhiteSpace(normalized.id))
                    {
                        v2.Add(normalized);
                    }
                }
                return v2;
            }

            // Legacy schema v1: { "availableLanguages": [ { "languageTag": ... } ] }
            MasterLanguages legacy = JsonConvert.DeserializeObject<MasterLanguages>(responseJsonRaw);
            if (legacy != null && legacy.availableLanguages != null && legacy.availableLanguages.Length > 0)
            {
                var v1 = new List<TranslationEntry>(legacy.availableLanguages.Length);
                foreach (LanguageInfo info in legacy.availableLanguages)
                {
                    if (info == null)
                    {
                        continue;
                    }

                    // Schema v1 keys entries by the language tag and has no rich metadata;
                    // everything except tag/name/author/version is left Unknown/missing.
                    TranslationEntry entry = Normalize(new TranslationEntry
                    {
                        id = info.languageTag,
                        languageTag = info.languageTag,
                        languageName = info.languageFullName,
                        author = info.author,
                        version = info.versionNumber
                    });
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.id))
                    {
                        v1.Add(entry);
                    }
                }
                return v1;
            }

            Logging.Warn("[CatalogService] Unrecognized catalog format; got no usable entries.");
            return new List<TranslationEntry>();
        }

        /// <summary>
        /// Fills in default values and trims user-provided strings so the UI can rely
        /// on consistent data (null instead of empty/whitespace, features never null).
        /// </summary>
        private static TranslationEntry Normalize(TranslationEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            entry.id = TrimToNull(entry.id) ?? TrimToNull(entry.languageTag);
            entry.languageTag = TrimToNull(entry.languageTag);
            entry.languageName = TrimToNull(entry.languageName) ?? entry.languageTag;
            entry.teamName = TrimToNull(entry.teamName);
            entry.author = TrimToNull(entry.author);
            entry.version = TrimToNull(entry.version);
            entry.logoUrl = TrimToNull(entry.logoUrl);
            entry.flagUrl = TrimToNull(entry.flagUrl);
            entry.readme = TrimToNull(entry.readme);
            entry.downloadUrl = TrimToNull(entry.downloadUrl);

            if (entry.features == null)
            {
                entry.features = new TranslationFeatures();
            }

            if (entry.gallery == null || entry.gallery.Count == 0)
            {
                entry.gallery = null;
            }

            if (entry.authors != null && entry.authors.Count > 0)
            {
                int removed = entry.authors.RemoveAll(a => a == null || string.IsNullOrWhiteSpace(a.name));
                if (removed > 0)
                {
                    Logging.Debug("[CatalogService] Dropped " + removed + " unnamed authors for '" + entry.id + "'.");
                }
                if (entry.authors.Count == 0)
                {
                    entry.authors = null;
                }
            }

            return entry;
        }

        private static string TrimToNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value.Trim();
        }

        private static bool ContainsIgnoreCase(string haystack, string needle)
        {
            return haystack != null && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool AnyAuthorContains(List<TranslationAuthor> authors, string needle)
        {
            if (authors == null)
            {
                return false;
            }

            foreach (TranslationAuthor author in authors)
            {
                if (ContainsIgnoreCase(author.name, needle) || ContainsIgnoreCase(author.role, needle))
                {
                    return true;
                }
            }
            return false;
        }
    }
}