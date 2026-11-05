using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace UltrakULL.json
{
    /// <summary>
    /// How the online translation list is ordered.
    /// </summary>
    public enum CatalogSortMode
    {
        Popularity,
        Updated,
        Language
    }

    /// <summary>
    /// Loads, caches and queries the online translation catalog
    /// (catalog.json served from Cloudflare Pages).
    ///
    /// Download strategy: bounded body (8 MB), up to three attempts with backoff
    /// on transient failures (DNS/timeout/5xx/429), then fallback to the on-disk
    /// cache so the browser still works offline. A snapshot is only accepted when
    /// <see cref="CatalogParser"/> fully validates it; a rejected snapshot never
    /// overwrites the in-memory list or the disk cache.
    ///
    /// This class is UI-agnostic: BrowserState (UI layer) consumes the resulting
    /// <see cref="TranslationEntry"/> list.
    /// </summary>
    public static class CatalogService
    {
        public const string CatalogUrl = "https://ultrakull-catalog.pages.dev/catalog.json";

        private const int MaxCatalogBytes = 8 * 1024 * 1024;
        private const int MaxAttempts = 3;
        private static readonly int[] RetryDelaysMs = { 1000, 3000 };

        private static readonly HttpClient Client = new HttpClient();

        private static List<TranslationEntry> _entries = new List<TranslationEntry>();
        private static volatile bool _loading;
        private static bool _loaded;
        private static string _lastError;
        private static bool _loadedFromCache;
        private static DateTime? _documentUpdatedAt;
        private static DateTime? _cacheFileTimeUtc;

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
        /// True when the current list came from the disk cache (offline fallback);
        /// the UI shows an "offline, cached snapshot" notice.
        /// </summary>
        public static bool LoadedFromCache
        {
            get { return _loadedFromCache; }
        }

        /// <summary>"updated_at" timestamp of the loaded catalog document, when present.</summary>
        public static DateTime? DocumentUpdatedAt
        {
            get { return _documentUpdatedAt; }
        }

        /// <summary>Write time of the disk cache file, when one exists.</summary>
        public static DateTime? CacheFileTimeUtc
        {
            get { return _cacheFileTimeUtc; }
        }

        /// <summary>Folder that stores the offline catalog cache (outside the pack scan).</summary>
        public static string CacheDirectory
        {
            get { return Path.Combine(Paths.ConfigPath, "ultrakull", "cache"); }
        }

        public static string CacheFilePath
        {
            get { return Path.Combine(CacheDirectory, "catalog.json"); }
        }

        /// <summary>
        /// Fetches the catalog (unless already cached) and returns whether the list
        /// is ready. Never throws; network failures fall back to the disk cache and
        /// failures are recorded in <see cref="LastError"/>.
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
                string url = ResolveCatalogUrl();
                Logging.Info("[CatalogService] Loading online translation catalog from " + url);

                string json = await DownloadWithRetryAsync(url);

                CatalogParseResult result = CatalogParser.Parse(json);
                if (result.IsError)
                {
                    // Broken snapshot: do not touch the in-memory list or the cache.
                    _lastError = result.Error;
                    Logging.Error("[CatalogService] Rejected catalog snapshot: " + result.Error);
                    return false;
                }

                LogWarnings(result);

                _entries = result.Entries;
                _documentUpdatedAt = result.DocumentUpdatedAt;
                _loaded = true;
                _loadedFromCache = false;
                WriteCacheAtomic(json);
                Logging.Info("[CatalogService] Loaded " + _entries.Count + " translation entries.");
                return true;
            }
            catch (Exception e)
            {
                _lastError = e.Message;
                Logging.Error("[CatalogService] Failed to load online translation catalog: " + e);
                return TryLoadFromCache();
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>
        /// Returns entries whose name, language tag or author list matches the
        /// query (case-insensitive). An empty/whitespace query returns all.
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

                if (ContainsIgnoreCase(entry.name, q) ||
                    ContainsIgnoreCase(entry.languageName, q) ||
                    ContainsIgnoreCase(entry.languageTag, q) ||
                    ContainsIgnoreCase(entry.author, q) ||
                    ContainsIgnoreCase(entry.teamName, q) ||
                    AnyAuthorContains(entry.authors, q))
                {
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// Stable sort of a translation list. Entries without sortable data (null
        /// likes/timestamps) sink to the end instead of being dropped.
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
                        .ThenBy(e => e.name ?? "", StringComparer.OrdinalIgnoreCase)
                        .ToList();
                case CatalogSortMode.Updated:
                    return source
                        .OrderByDescending(e => e.updatedAt ?? DateTime.MinValue)
                        .ThenBy(e => e.name ?? "", StringComparer.OrdinalIgnoreCase)
                        .ToList();
                case CatalogSortMode.Language:
                    return source
                        .OrderBy(e => LanguageDisplay.Name(e.languageTag ?? ""), StringComparer.CurrentCulture)
                        .ThenBy(e => e.name ?? "", StringComparer.OrdinalIgnoreCase)
                        .ToList();
                default:
                    return new List<TranslationEntry>(source);
            }
        }

        public static TranslationEntry FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (TranslationEntry entry in _entries)
            {
                if (entry != null && string.Equals(entry.id, id, StringComparison.Ordinal))
                {
                    return entry;
                }
            }
            return null;
        }

        // -------------------------------------------------------------------
        // Download
        // -------------------------------------------------------------------

        private sealed class CatalogHttpException : Exception
        {
            public readonly int? StatusCode;

            public CatalogHttpException(string message, int? statusCode)
                : base(message)
            {
                StatusCode = statusCode;
            }
        }

        private static async Task<string> DownloadWithRetryAsync(string url)
        {
            Exception lastError = null;
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (attempt > 0)
                {
                    int delay = RetryDelaysMs[Math.Min(attempt - 1, RetryDelaysMs.Length - 1)];
                    await Task.Delay(delay);
                }

                try
                {
                    return await DownloadOnceAsync(url);
                }
                catch (Exception e)
                {
                    lastError = e;
                    if (!IsRetryable(e))
                    {
                        throw;
                    }
                    Logging.Warn("[CatalogService] Catalog download attempt " + (attempt + 1) + " failed: " + e.Message);
                }
            }
            throw lastError;
        }

        private static bool IsRetryable(Exception e)
        {
            // HTTP status failures: only server errors and rate limiting retry.
            CatalogHttpException http = e as CatalogHttpException;
            if (http != null)
            {
                if (!http.StatusCode.HasValue)
                {
                    return false;
                }
                int code = http.StatusCode.Value;
                return code == 429 || code >= 500;
            }

            // Timeouts surface as TaskCanceledException, DNS/socket failures as
            // HttpRequestException; both are transient.
            return e is TaskCanceledException || e is HttpRequestException;
        }

        private static async Task<string> DownloadOnceAsync(string url)
        {
            Client.Timeout = TimeSpan.FromSeconds(10);
            using (HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                if (!response.IsSuccessStatusCode)
                {
                    int code = (int)response.StatusCode;
                    throw new CatalogHttpException("HTTP " + code + " (" + response.StatusCode + ")", code);
                }

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (MemoryStream memory = new MemoryStream())
                {
                    byte[] buffer = new byte[81920];
                    while (true)
                    {
                        int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }
                        if (memory.Length + read > MaxCatalogBytes)
                        {
                            throw new CatalogHttpException("Catalog exceeds the " + (MaxCatalogBytes / (1024 * 1024)) + " MB size limit.", null);
                        }
                        memory.Write(buffer, 0, read);
                    }
                    return Encoding.UTF8.GetString(memory.ToArray());
                }
            }
        }

        // -------------------------------------------------------------------
        // Disk cache
        // -------------------------------------------------------------------

        private static bool TryLoadFromCache()
        {
            try
            {
                if (!File.Exists(CacheFilePath))
                {
                    return false;
                }

                CatalogParseResult result = CatalogParser.Parse(File.ReadAllText(CacheFilePath));
                if (result.IsError)
                {
                    Logging.Error("[CatalogService] Cached catalog snapshot is unusable: " + result.Error);
                    return false;
                }

                LogWarnings(result);
                _entries = result.Entries;
                _documentUpdatedAt = result.DocumentUpdatedAt;
                _cacheFileTimeUtc = File.GetLastWriteTimeUtc(CacheFilePath);
                _loaded = true;
                _loadedFromCache = true;
                Logging.Info("[CatalogService] Loaded " + _entries.Count + " translation entries from the offline cache.");
                return true;
            }
            catch (Exception e)
            {
                Logging.Error("[CatalogService] Failed to read the catalog cache: " + e.Message);
                return false;
            }
        }

        private static void WriteCacheAtomic(string json)
        {
            try
            {
                Directory.CreateDirectory(CacheDirectory);
                string tempPath = CacheFilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(CacheFilePath))
                {
                    File.Replace(tempPath, CacheFilePath, null);
                }
                else
                {
                    File.Move(tempPath, CacheFilePath);
                }
                _cacheFileTimeUtc = File.GetLastWriteTimeUtc(CacheFilePath);
            }
            catch (Exception e)
            {
                // The cache is an optimization, not a requirement: a failed write
                // must not fail the load.
                Logging.Warn("[CatalogService] Failed to write the catalog cache: " + e.Message);
            }
        }

        private static void LogWarnings(CatalogParseResult result)
        {
            if (result.Warnings == null)
            {
                return;
            }
            foreach (string warning in result.Warnings)
            {
                Logging.Warn("[CatalogService] " + warning);
            }
        }

        /// <summary>
        /// The catalog URL: the "catalogUrl" config entry when set (mirrors/local
        /// testing), otherwise the default constant. An empty value also means the
        /// default.
        /// </summary>
        private static string ResolveCatalogUrl()
        {
            try
            {
                ConfigFile config = LanguageManager.configFile;
                if (config != null)
                {
                    string value = config.Bind("General", "catalogUrl", CatalogUrl).Value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value.Trim();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[CatalogService] Could not read the catalogUrl config: " + e.Message);
            }
            return CatalogUrl;
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
