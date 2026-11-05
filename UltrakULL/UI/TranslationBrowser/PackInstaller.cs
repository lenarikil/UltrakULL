using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UltrakULL.json;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>Outcome kinds of an install attempt.</summary>
    public enum PackInstallStatus
    {
        Success,
        Failed,
        Cancelled,
        ExternalPage,
        Unsupported
    }

    /// <summary>Final result of an install attempt.</summary>
    public sealed class InstallResult
    {
        public PackInstallStatus Status;
        /// <summary>Detail text (error message or the external page URL).</summary>
        public string Message;
        /// <summary>Pack key (folder name) of the installed/updated pack on success.</summary>
        public string InstalledPackKey;
        /// <summary>True when the replaced pack is the currently active language.</summary>
        public bool ReplacedActivePack;
    }

    /// <summary>
    /// Downloads and installs a translation pack from a catalog entry. A
    /// shared classifier (Content-Type, Content-Disposition, URL extension)
    /// plus magic-byte sniffing of unlabeled binary responses decides between
    /// a bare JSON file and a ZIP archive, and resolves "page" links of known
    /// hosts (see DownloadUrlResolver). When nothing direct is possible,
    /// links of known storage hosts are handed to the external browser and
    /// every other source is rejected as unsupported. Installs always go
    /// through a staging directory so a failed install never destroys an
    /// existing pack.
    ///
    /// All awaits continue on the Unity main thread (InstallAsync is started from
    /// the UI); only the ZIP extraction runs on a background thread (pure
    /// System.IO, cancellable).
    /// </summary>
    public static class PackInstaller
    {
        private const long MaxDownloadBytes = 3L * 1024 * 1024 * 1024; // 3 GB
        private const int HeadersTimeoutSeconds = 30;
        private const int MaxHtmlBytes = 2 * 1024 * 1024; // interstitial pages only
        private const long ProgressReportChunk = 256 * 1024;
        private const long MaxImageBytes = 16L * 1024 * 1024; // logos/flags/gallery

        // Serializes the target-folder pick + the staging swap of parallel
        // installs: the folder-ownership check inside PackFolderPolicy and the
        // backup/move must never interleave with another install claiming the
        // same folder.
        private static readonly object FolderLock = new object();

        /// <summary>Root folder that holds every language pack.</summary>
        public static string PacksRootFolder
        {
            get { return Path.Combine(Paths.ConfigPath, "ultrakull"); }
        }

        private static readonly HttpClient Client = CreateClient();

        private static HttpClient CreateClient()
        {
            // net472: HttpClientHandler with a CookieContainer (the Drive download
            // session may set cookies; SocketsHttpHandler is .NET Core only).
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.AllowAutoRedirect = true;
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseCookies = true;

            HttpClient client = new HttpClient(handler);
            // No whole-request timeout: a large pack download may legitimately take
            // longer than any fixed bound. Headers phase gets its own timeout below,
            // the body is bounded by the size limit and the cancel button.
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("UltrakULL-TranslationBrowser");
            return client;
        }

        public static async Task<InstallResult> InstallAsync(
            TranslationEntry entry,
            IProgress<InstallProgress> progress,
            CancellationToken ct)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.downloadUrl))
            {
                return Failed("The catalog entry has no download URL.");
            }

            Report(progress, "resolving", 0, null);

            // --- Step 0: resolve indirect links of known hosts (may hit the
            // Yandex Disk API; the only extra host ever contacted). ---
            DownloadResolution resolution = await DownloadUrlResolver.ResolveAsync(Client, entry.downloadUrl);
            if (resolution.ExternalPageUrl != null)
            {
                return NotFileResult("The link could not be resolved to a direct download", entry.downloadUrl);
            }

            string stagingDir = null;
            string downloadedTempFile = null;
            try
            {
                ct.ThrowIfCancellationRequested();

                stagingDir = Path.Combine(Path.GetTempPath(), "ultrakull-staging-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                Directory.CreateDirectory(stagingDir);

                // --- Steps 1-3: fetch (shared classifier, magic-byte sniffing,
            // one Drive interstitial fallback, one Dropbox st-retry),
            // download to a temp file, parse. ---
            string cacheFile = GetCachedArchive(entry.id);
            FetchedDownload fetched;
            if (cacheFile != null && File.Exists(cacheFile))
            {
                Logging.Info("[PackInstaller] Using cached archive for '" + entry.id + "': " + cacheFile);
                fetched = new FetchedDownload
                {
                    Kind = ClassifyFile(cacheFile),
                    FilePath = cacheFile
                };
            }
            else
            {
                fetched = await FetchWithDropboxRetryAsync(resolution.CandidateUrl, entry.downloadUrl, progress, ct);
                if (fetched.NotFile)
                {
                    return NotFileResult(fetched.Reason, entry.downloadUrl);
                }
                downloadedTempFile = fetched.FilePath;
                CacheArchive(entry.id, fetched.FilePath);
            }

                Report(progress, "extracting", 0, null);

                // --- Step 3a/3b: turn the downloaded file into a staged pack folder. ---
                string packRoot;
                if (string.Equals(fetched.Kind, "json", StringComparison.Ordinal))
                {
                    packRoot = stagingDir;
                    string target = Path.Combine(stagingDir, "translation.json");
                    File.Move(fetched.FilePath, target);
                }
                else if (string.Equals(fetched.Kind, "zip", StringComparison.Ordinal))
                {
                    packRoot = await StageZipAsync(fetched.FilePath, stagingDir, progress, ct);
                }
                else if (string.Equals(fetched.Kind, "7z", StringComparison.Ordinal))
                {
                    packRoot = await Stage7zAsync(fetched.FilePath, stagingDir, progress, ct);
                }
                else
                {
                    CleanupPath(fetched.FilePath);
                    return Failed(BrowserText.DownloadInvalid);
                }

                // The folder name the archive shipped (when it wrapped its
                // contents in a single top-level folder) is preferred over the
                // locale/id based name so the installed pack keeps its layout.
                string archiveFolderName = (packRoot != null && !string.Equals(packRoot, stagingDir, StringComparison.Ordinal))
                    ? Path.GetFileName(packRoot)
                    : null;

                string translationFile = Path.Combine(packRoot, "translation.json");
                JsonFormat pack = LoadPackMetadata(translationFile);
                if (pack == null)
                {
                    return Failed(BrowserText.DownloadInvalid);
                }

                // --- Steps 4-5: pick the target folder and swap the staging in. ---
                Report(progress, "installing", 0, null);

                // The folder pick (which verifies folder ownership) and the
                // swap itself run atomically under a static lock, so two
                // parallel installs can never pick or overwrite the same folder.
                string targetFolderName;
                string targetFolder;
                bool replacedActive;
                lock (FolderLock)
                {
                    targetFolderName = ResolveTargetFolder(entry, PacksRootFolder, archiveFolderName);
                    targetFolder = Path.Combine(PacksRootFolder, targetFolderName);

                    replacedActive = LanguageManager.CurrentLanguage != null &&
                        string.Equals(LanguageManager.CurrentLanguage.PackKey, targetFolderName, StringComparison.OrdinalIgnoreCase);

                    FinalizeInstall(packRoot, targetFolder, entry);
                }

                // --- Step 5a: download the catalog's logo, flag and gallery images
                // into the pack's images/ folder so the Installed tab can show
                // them without re-fetching from the network. ---
                Report(progress, "installing", 0, null);
                string[] galleryUrls = entry.gallery != null ? entry.gallery.ToArray() : null;
                string[] savedPaths = await DownloadPackImagesAsync(targetFolder, entry.logoUrl, entry.flagUrl, galleryUrls, ct);
                WriteManifest(targetFolder, entry, savedPaths[0], savedPaths[1], savedPaths.Length > 2 ? savedPaths[2] : null);
                DeleteLegacyImageFiles(targetFolder);

                // --- Step 6: make the new pack visible everywhere. ---
                LanguageManager.LoadLanguages();
                BrowserHost.RefreshInstalled();

                Logging.Info("[PackInstaller] Installed translation '" + entry.id + "' into " + targetFolderName +
                    (replacedActive ? " (replaced the active language pack)" : ""));

                return new InstallResult
                {
                    Status = PackInstallStatus.Success,
                    InstalledPackKey = targetFolderName,
                    ReplacedActivePack = replacedActive
                };
            }
            catch (OperationCanceledException)
            {
                return new InstallResult { Status = PackInstallStatus.Cancelled };
            }
            catch (Exception e)
            {
                Logging.Error("[PackInstaller] Install of '" + entry.id + "' failed: " + e);
                return Failed(e.Message);
            }
            finally
            {
                CleanupPath(stagingDir);
                CleanupPath(downloadedTempFile);
            }
        }

        // -------------------------------------------------------------------
        // Fetch + dispatch
        // -------------------------------------------------------------------

        private sealed class FetchedDownload
        {
            /// <summary>"json", "zip", or "7z" when a file was downloaded.</summary>
            public string Kind;
            /// <summary>Temp file the download was written to.</summary>
            public string FilePath;
            /// <summary>True when the URL did not yield a downloadable file.</summary>
            public bool NotFile;
            /// <summary>Why the URL is not a file (English, for the log).</summary>
            public string Reason;
        }

        private static FetchedDownload NotFile(string reason)
        {
            return new FetchedDownload { NotFile = true, Reason = reason };
        }

        /// <summary>
        /// FetchAsync plus the single Dropbox "expired st" retry: when a
        /// Dropbox candidate URL carries the expirable st parameter and the
        /// response is not a file (typically an HTML preview/error page for a
        /// link whose st token has expired), the same URL is fetched once more
        /// without st; rlkey and dl=1 are kept.
        /// </summary>
        private static async Task<FetchedDownload> FetchWithDropboxRetryAsync(
            string candidateUrl,
            string originalUrl,
            IProgress<InstallProgress> progress,
            CancellationToken ct)
        {
            FetchedDownload fetched = await FetchAsync(candidateUrl, originalUrl, progress, ct);

            if (fetched.NotFile && IsDropboxUrlWithSt(candidateUrl))
            {
                string retryUrl = RemoveStParameter(candidateUrl);
                Logging.Info("[PackInstaller] The Dropbox response was not a file; retrying once without the expirable 'st' parameter.");
                fetched = await FetchAsync(retryUrl, originalUrl, progress, ct);
            }

            return fetched;
        }

        /// <summary>
        /// Fetches the URL and decides what it is with the shared response
        /// classifier. Explicit JSON/ZIP responses are downloaded directly;
        /// unlabeled binaries (octet-stream, empty type, text/plain) are
        /// downloaded and sniffed by their first bytes; HTML responses from
        /// Drive hosts get one interstitial form-POST fallback; anything that
        /// is not a file is reported for the storage-host policy.
        /// </summary>
        private static async Task<FetchedDownload> FetchAsync(
            string url,
            string originalUrl,
            IProgress<InstallProgress> progress,
            CancellationToken ct)
        {
            using (HttpResponseMessage response = await GetResponseAsync(url, ct))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("HTTP " + (int)response.StatusCode + " (" + response.StatusCode + ")");
                }

                string contentType = (response.Content.Headers.ContentType != null)
                    ? (response.Content.Headers.ContentType.MediaType ?? "").ToLowerInvariant()
                    : "";
                string fileName = ContentFileName(response);
                string urlExtension = UrlExtension(url);

                ResponseKind kind = ClassifyResponse(contentType, fileName, urlExtension);
                LogResponse("GET", url, response, contentType, fileName, kind);

                switch (kind)
                {
                    case ResponseKind.Json:
                        return await DownloadToFileAsync(response, ".json", "json", progress, ct);

                    case ResponseKind.Zip:
                        return await DownloadToFileAsync(response, ".zip", "zip", progress, ct);

                    case ResponseKind.SevenZip:
                        return await DownloadToFileAsync(response, ".7z", "7z", progress, ct);

                    case ResponseKind.Binary:
                        return await DownloadToFileAsync(response, ".download", null, progress, ct);

                    case ResponseKind.Html:
                        return NotFile("The response is an HTML page, not a file");

                    default:
                        return NotFile("The response content type '" + contentType + "' is not a downloadable translation pack");
                }
            }
        }

        /// <summary>
        /// Streams the response body to a temp file. A null kind means the
        /// response was unlabeled (octet-stream, empty type, text/plain): the
        /// first bytes are then sniffed while reading - "PK" means a ZIP
        /// archive, a significant "{" a JSON document - and a body that is
        /// neither aborts the download early (the partial file is deleted) and
        /// reports a not-a-file outcome for the storage-host policy.
        /// </summary>
        private static async Task<FetchedDownload> DownloadToFileAsync(
            HttpResponseMessage response,
            string extension,
            string kind,
            IProgress<InstallProgress> progress,
            CancellationToken ct)
        {
            Report(progress, "downloading", 0, response.Content.Headers.ContentLength);

            string filePath = Path.Combine(Path.GetTempPath(), "ultrakull-" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension);
            try
            {
                string sniffAbortReason = null;

                using (Stream input = await response.Content.ReadAsStreamAsync())
                using (FileStream output = File.Create(filePath))
                {
                    byte[] buffer = new byte[81920];
                    // Sniff window: the first bytes of the body across chunk
                    // boundaries (a single read may return fewer than needed).
                    byte[] sniffWindow = new byte[64];
                    int sniffLength = 0;

                    long totalRead = 0;
                    long lastReported = 0;
                    int read;
                    while ((read = await input.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        if (kind == null)
                        {
                            int copy = Math.Min(read, sniffWindow.Length - sniffLength);
                            if (copy > 0)
                            {
                                Buffer.BlockCopy(buffer, 0, sniffWindow, sniffLength, copy);
                                sniffLength += copy;
                            }

                            SniffVerdict verdict = SniffPrefix(sniffWindow, sniffLength);
                            if (verdict == SniffVerdict.Zip || verdict == SniffVerdict.SevenZip || verdict == SniffVerdict.Json)
                            {
                                kind = verdict == SniffVerdict.Zip ? "zip" : (verdict == SniffVerdict.SevenZip ? "7z" : "json");
                                Logging.Info("[PackInstaller] Sniffed the unlabeled download as " + kind + ".");
                            }
                            else if (verdict == SniffVerdict.NotAFile)
                            {
                                Logging.Info("[PackInstaller] The sniffed data is neither a ZIP archive nor a JSON document nor a 7z archive; aborting the download.");
                                sniffAbortReason = "The downloaded data is neither a ZIP archive nor a JSON document nor a 7z archive";
                                break;
                            }
                            // NeedMoreBytes: keep reading, sniff again on the
                            // next chunk.
                        }

                        totalRead += read;
                        if (totalRead > MaxDownloadBytes)
                        {
                            throw new IOException("The download exceeds the " + (MaxDownloadBytes / (1024 * 1024 * 1024)) + " GB limit.");
                        }
                        await output.WriteAsync(buffer, 0, read, ct);

                        if (totalRead - lastReported >= ProgressReportChunk)
                        {
                            lastReported = totalRead;
                            Report(progress, "downloading", totalRead, response.Content.Headers.ContentLength);
                        }
                    }
                }

                if (kind == null)
                {
                    // Sniff abort, or an empty/blank body: not a pack file.
                    CleanupPath(filePath);
                    return NotFile(sniffAbortReason ?? "The response body is empty");
                }

                return new FetchedDownload { Kind = kind, FilePath = filePath };
            }
            catch
            {
                CleanupPath(filePath);
                throw;
            }
        }

        private static async Task<HttpResponseMessage> GetResponseAsync(string url, CancellationToken ct)
        {
            // Bounded header phase; the body phase is bounded by the size limit,
            // progress reporting and the cancel button instead of a fixed timeout.
            using (CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                timeout.CancelAfter(TimeSpan.FromSeconds(HeadersTimeoutSeconds));
                return await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            }
        }

        // -------------------------------------------------------------------
        // Response classification, sniffing and the not-a-file policy
        // -------------------------------------------------------------------

        /// <summary>Verdict of the shared response classifier.</summary>
        private enum ResponseKind
        {
            /// <summary>A JSON document (a bare translation.json).</summary>
            Json,
            /// <summary>A ZIP archive.</summary>
            Zip,
            /// <summary>A 7z archive.</summary>
            SevenZip,
            /// <summary>Unlabeled bytes: octet-stream, application/binary, an
            /// empty type or text/plain - the caller sniffs the downloaded
            /// bytes.</summary>
            Binary,
            /// <summary>An HTML page.</summary>
            Html,
            /// <summary>Anything else: not a downloadable pack file.</summary>
            NotAFile
        }

        /// <summary>
        /// The single response classifier for both the GET and the Drive
        /// form-POST paths. A concrete Content-Type (json/zip/html) is
        /// authoritative; the Content-Disposition file name and the URL
        /// extension act as hints; unlabeled binaries go to the magic-byte
        /// sniffer. text/plain is treated as binary: "silent" hosts serve
        /// bare translation.json files that way.
        /// </summary>
        private static ResponseKind ClassifyResponse(string contentType, string fileName, string urlExtension)
        {
            if (contentType.Contains("json") || urlExtension == ".json")
            {
                return ResponseKind.Json;
            }

            if (contentType.Contains("zip") || HasZipExtension(fileName) || urlExtension == ".zip")
            {
                return ResponseKind.Zip;
            }

            if (contentType.Contains("x-7z-compressed") || contentType.Contains("7z") || Has7zExtension(fileName) || urlExtension == ".7z")
            {
                return ResponseKind.SevenZip;
            }

            if (contentType.Contains("html"))
            {
                return ResponseKind.Html;
            }

            if (contentType.Length == 0 ||
                contentType.Contains("octet-stream") ||
                contentType.Contains("application/binary") ||
                contentType.Contains("text/plain"))
            {
                return ResponseKind.Binary;
            }

            return ResponseKind.NotAFile;
        }

        /// <summary>
        /// Logs one classifier decision: the requested URL, the final URL
        /// after redirects, the HTTP status, the content type, the
        /// Content-Disposition file name and the verdict.
        /// </summary>
        private static void LogResponse(
            string method,
            string requestUrl,
            HttpResponseMessage response,
            string contentType,
            string fileName,
            ResponseKind verdict)
        {
            string finalUrl = requestUrl;
            try
            {
                if (response.RequestMessage != null && response.RequestMessage.RequestUri != null)
                {
                    finalUrl = response.RequestMessage.RequestUri.ToString();
                }
            }
            catch (Exception)
            {
                // Diagnostics only: keep the requested URL when the final one
                // is unavailable.
            }

            Logging.Info("[PackInstaller] " + method + " " + requestUrl
                + " -> HTTP " + (int)response.StatusCode
                + ", final URL " + finalUrl
                + ", content type '" + contentType + "'"
                + ", file name '" + (fileName ?? "") + "'"
                + ", verdict " + verdict);
        }

        /// <summary>Verdict of the magic-byte sniffer.</summary>
        private enum SniffVerdict
        {
            Zip,
            SevenZip,
            Json,
            /// <summary>More bytes are needed (only a BOM/whitespace prefix
            /// has arrived so far).</summary>
            NeedMoreBytes,
            NotAFile
        }

        /// <summary>
        /// Looks at the first bytes of a download: "PK" (0x50 0x4B) is a ZIP
        /// local file header; a first significant byte of "{" (after a UTF-8
        /// BOM and whitespace) opens a JSON document. Hosts that serve packs
        /// as a bare octet-stream (Drive without a Content-Disposition, Yandex
        /// API hrefs without an extension) are recognized this way.
        /// </summary>
        private static SniffVerdict SniffPrefix(byte[] prefix, int length)
        {
            if (length == 0)
            {
                return SniffVerdict.NeedMoreBytes;
            }

            // 7z signature: '7','z',0xBC,0xAF,0x27,0x1C
            if (prefix[0] == 0x37)
            {
                if (length < 6)
                {
                    return SniffVerdict.NeedMoreBytes;
                }
                if (prefix[1] == 0x7A && prefix[2] == 0xBC && prefix[3] == 0xAF && prefix[4] == 0x27 && prefix[5] == 0x1C)
                {
                    return SniffVerdict.SevenZip;
                }
            }

            // ZIP local file header.
            if (prefix[0] == 0x50)
            {
                if (length < 2)
                {
                    return SniffVerdict.NeedMoreBytes;
                }
                if (prefix[1] == 0x4B)
                {
                    return SniffVerdict.Zip;
                }
            }

            int start = 0;
            // UTF-8 byte order mark.
            if (prefix[0] == 0xEF)
            {
                if (length < 3)
                {
                    return SniffVerdict.NeedMoreBytes;
                }
                if (prefix[1] == 0xBB && prefix[2] == 0xBF)
                {
                    start = 3;
                }
            }

            for (int i = start; i < length; i++)
            {
                byte b = prefix[i];
                if (b == 0x20 || b == 0x09 || b == 0x0D || b == 0x0A)
                {
                    continue;
                }
                return b == (byte)'{' ? SniffVerdict.Json : SniffVerdict.NotAFile;
            }

            // Only BOM/whitespace so far: a full window of nothing
            // significant means this is not a pack file either.
            return length >= prefix.Length ? SniffVerdict.NotAFile : SniffVerdict.NeedMoreBytes;
        }

        /// <summary>
        /// Install result for a link that did not yield a downloadable file:
        /// when the entry's original URL points at a known file storage, it is
        /// handed to the external browser (the user can download manually);
        /// every other host is rejected as an unsupported source and nothing
        /// is opened at all.
        /// </summary>
        private static InstallResult NotFileResult(string reason, string originalUrl)
        {
            string host = TryGetUrlHost(originalUrl);
            if (host != null && UrlPolicy.IsKnownStorageHost(host))
            {
                Logging.Info("[PackInstaller] " + reason + ". '" + host + "' is a known file storage: opening the original link in the external browser.");
                return new InstallResult { Status = PackInstallStatus.ExternalPage, Message = originalUrl };
            }

            Logging.Info("[PackInstaller] " + reason + ". '" + (host ?? "unknown host") + "' is not a known file storage: rejecting the download as unsupported.");
            return new InstallResult { Status = PackInstallStatus.Unsupported };
        }

        private static string TryGetUrlHost(string url)
        {
            try
            {
                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || string.IsNullOrEmpty(uri.Host))
                {
                    return null;
                }
                return uri.Host.ToLowerInvariant();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsDropboxUrlWithSt(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return false;
            }

            string host = uri.Host.ToLowerInvariant();
            bool isDropbox = host.Equals("dropbox.com") ||
                host.EndsWith(".dropbox.com", StringComparison.Ordinal) ||
                host.EndsWith(".dropboxusercontent.com", StringComparison.Ordinal);
            if (!isDropbox)
            {
                return false;
            }

            string query = uri.Query;
            return query.IndexOf("?st=", StringComparison.Ordinal) >= 0 ||
                query.IndexOf("&st=", StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Removes the Dropbox st parameter, keeping every other parameter
        /// (rlkey, dl=1) intact. Manual query surgery for the same reason as
        /// DownloadUrlResolver.ForceDropboxDirect: generic query parsing may
        /// alter '+' and other characters.
        /// </summary>
        private static string RemoveStParameter(string url)
        {
            int queryStart = url.IndexOf('?');
            if (queryStart < 0)
            {
                return url;
            }

            string baseUrl = url.Substring(0, queryStart);
            string[] parameters = url.Substring(queryStart + 1).Split('&');

            List<string> kept = new List<string>();
            foreach (string parameter in parameters)
            {
                if (!parameter.StartsWith("st=", StringComparison.OrdinalIgnoreCase))
                {
                    kept.Add(parameter);
                }
            }

            if (kept.Count == 0)
            {
                return baseUrl;
            }
            return baseUrl + "?" + string.Join("&", kept.ToArray());
        }

        // -------------------------------------------------------------------
        // 7z staging
        // -------------------------------------------------------------------

        private static async Task<string> Stage7zAsync(string sevenZipPath, string stagingDir, IProgress<InstallProgress> progress, CancellationToken ct)
        {
            ZipLimits limits = new ZipLimits();

            string rootFullPath = Path.GetFullPath(stagingDir);
            if (!rootFullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                rootFullPath += Path.DirectorySeparatorChar;
            }

            string sevenZipExe = Find7zExe();
            if (sevenZipExe == null)
            {
                throw new InvalidDataException("7z.exe not found next to the mod DLL or in PATH. Cannot extract 7z archive.");
            }

            // Extract with 7z.exe (native speed). 7z handles both 7z and zip archives.
            await ExtractWith7zExe(sevenZipExe, sevenZipPath, stagingDir, 0, 0, progress, ct);

            // Post-extraction safety scan: reject any denied file type that
            // 7z.exe may have extracted (defense in depth).
            ScanStagingForDeniedFiles(stagingDir, limits);

            string translationFile = Path.Combine(stagingDir, "translation.json");
            if (File.Exists(translationFile))
            {
                return stagingDir;
            }

            string[] subdirectories = Directory.GetDirectories(stagingDir);
            string[] files = Directory.GetFiles(stagingDir);
            if (files.Length == 0 && subdirectories.Length == 1 &&
                File.Exists(Path.Combine(subdirectories[0], "translation.json")))
            {
                return subdirectories[0];
            }

            throw new InvalidDataException("translation.json was not found in the archive.");
        }

        private static string Find7zExe()
        {
            // Ship 7z.exe next to the mod DLL.
            string pluginDir = Path.GetDirectoryName(typeof(PackInstaller).Assembly.Location);
            string candidate = Path.Combine(pluginDir, "tools", "7z.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            // Fall back to PATH.
            foreach (var pathDir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
            {
                if (string.IsNullOrEmpty(pathDir)) continue;
                candidate = Path.Combine(pathDir.Trim('"'), "7z.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static async Task ExtractWith7zExe(string sevenZipExe, string sevenZipPath, string stagingDir, int totalFiles, long totalUncompressed, IProgress<InstallProgress> progress, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = sevenZipExe,
                Arguments = "x \"" + sevenZipPath + "\" -o\"" + stagingDir + "\" -y -bb0",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = stagingDir
            };

            using (var process = Process.Start(psi))
            {
                ct.Register(() =>
                {
                    try { process.Kill(); } catch { }
                });

                var stderr = new StringBuilder();

                // Read stderr on a thread pool thread to avoid the classic
                // .NET Process deadlock: ReadLine() blocks on \r-only output
                // (-bb1 uses \r, not \n), while WaitForExit blocks on the
                // process never exiting because its stderr buffer is full.
                var stderrTask = Task.Run(() =>
                {
                    try { stderr.Append(process.StandardError.ReadToEnd()); } catch { }
                }, ct);

                // Wait for exit on another thread pool thread.
                var exitTask = Task.Run(() => process.WaitForExit(), ct);

                await Task.WhenAll(exitTask, stderrTask);

                if (process.ExitCode != 0)
                {
                    throw new InvalidDataException("7z.exe failed with exit code " + process.ExitCode + ". stderr: " + stderr.ToString());
                }

                // Report completion; 7z.exe -bb0 doesn't emit per-file progress,
                // so we mark 100% when it exits.
                Report(progress, "extracting", totalUncompressed, totalUncompressed > 0 ? totalUncompressed : (long?)null);
            }
        }

        private static void ScanStagingForDeniedFiles(string stagingDir, ZipLimits limits)
        {
            foreach (var file in Directory.GetFiles(stagingDir, "*.*", SearchOption.AllDirectories))
            {
                string name = file.Replace(Path.DirectorySeparatorChar, '/');
                if (SafeZipExtractor.IsDenied(name))
                {
                    try { File.Delete(file); } catch { }
                    throw new InvalidDataException("The archive was rejected: extracted file '" + name + "' has a forbidden file type");
                }
            }
        }

        /// <summary>
        /// Validates and extracts the archive into the staging directory (on a
        /// background thread) and locates the pack root: the staging itself when
        /// translation.json lies at the top, or the single subfolder that contains
        /// it (the usual "everything wrapped in one folder" layout).
        /// </summary>
        private static async Task<string> StageZipAsync(string zipPath, string stagingDir, IProgress<InstallProgress> progress, CancellationToken ct)
        {
            using (FileStream file = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ZipArchive archive = new ZipArchive(file, ZipArchiveMode.Read))
            {
                string problem = SafeZipExtractor.ValidateArchive(archive);
                if (problem != null)
                {
                    throw new InvalidDataException("The archive was rejected: " + problem);
                }

                await Task.Run(delegate
                {
                    SafeZipExtractor.ExtractToStaging(archive, stagingDir, ct, progress: progress);
                }, ct);
            }

            string translationFile = Path.Combine(stagingDir, "translation.json");
            if (File.Exists(translationFile))
            {
                return stagingDir;
            }

            string[] subdirectories = Directory.GetDirectories(stagingDir);
            string[] files = Directory.GetFiles(stagingDir);
            if (files.Length == 0 && subdirectories.Length == 1 &&
                File.Exists(Path.Combine(subdirectories[0], "translation.json")))
            {
                return subdirectories[0];
            }

            throw new InvalidDataException("translation.json was not found in the archive.");
        }

        // -------------------------------------------------------------------
        // Pack validation and finalization
        // -------------------------------------------------------------------

        /// <summary>
        /// Loads a staged translation.json and validates that it is a usable pack
        /// (readable JSON, metadata, non-empty languageCode). Uses the same
        /// lenient deserialization as the language manager, but read-only.
        /// </summary>
        private static JsonFormat LoadPackMetadata(string translationFile)
        {
            try
            {
                JsonFormat pack = JsonConvert.DeserializeObject<JsonFormat>(File.ReadAllText(translationFile));
                if (pack == null || pack.metadata == null || string.IsNullOrWhiteSpace(pack.metadata.languageCode))
                {
                    return null;
                }
                return pack;
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Staged translation.json is not valid: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Picks the installation folder name (pure policy; see
        /// <see cref="PackFolderPolicy.ResolveTargetFolder"/>).
        /// </summary>
        public static string ResolveTargetFolder(TranslationEntry entry, string packsRoot, string archiveFolderName = null)
        {
            return PackFolderPolicy.ResolveTargetFolder(entry, packsRoot, archiveFolderName);
        }

        /// <summary>
        /// Moves the staged pack into place. An existing folder of the same
        /// translation is renamed to ".bak" first and restored if anything goes
        /// wrong; on success the backup is deleted. Folders starting with "." are
        /// ignored by the pack scanner, which is what makes the swap safe.
        /// </summary>
        private static void FinalizeInstall(string packRoot, string targetFolder, TranslationEntry entry)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetFolder));

            string backupFolder = targetFolder + ".bak";
            bool hadExisting = Directory.Exists(targetFolder);
            if (hadExisting)
            {
                if (Directory.Exists(backupFolder))
                {
                    Directory.Delete(backupFolder, true);
                }
                Directory.Move(targetFolder, backupFolder);
            }

            try
            {
                MoveDirectory(packRoot, targetFolder);
            }
            catch
            {
                if (hadExisting && !Directory.Exists(targetFolder))
                {
                    Directory.Move(backupFolder, targetFolder);
                }
                throw;
            }

            if (hadExisting)
            {
                try
                {
                    Directory.Delete(backupFolder, true);
                }
                catch (Exception e)
                {
                    Logging.Warn("[PackInstaller] Could not delete the old pack backup: " + e.Message);
                }
            }

            WriteManifest(targetFolder, entry);
            StampTranslationId(Path.Combine(targetFolder, "translation.json"), entry.id);
        }

        private static void WriteManifest(string targetFolder, TranslationEntry entry,
            string logoPath = null, string flagPath = null, string galleryPathsJson = null)
        {
            try
            {
                List<string> galleryList = null;
                if (!string.IsNullOrEmpty(galleryPathsJson))
                {
                    galleryList = new List<string>();
                    foreach (string p in galleryPathsJson.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(p))
                        {
                            galleryList.Add(p);
                        }
                    }
                }

                TranslationManifest manifest = new TranslationManifest
                {
                    translationId = entry.id,
                    name = entry.name,
                    languageCode = entry.languageTag,
                    authors = entry.author,
                    description = entry.readme,
                    discordUrl = entry.discordUrl,
                    version = entry.version,
                    source = "catalog",
                    installedAt = DateTime.UtcNow.ToString("o"),
                    likes = entry.likes,
                    featuresJson = TranslationManifest.FeaturesToJson(entry.features),
                    socialLinksJson = TranslationManifest.SocialLinksToJson(entry.socialLinks),
                    logoPath = logoPath,
                    flagPath = flagPath,
                    galleryPaths = galleryList
                };
                TranslationManifest.Save(Path.Combine(targetFolder, TranslationManifest.FileName), manifest);
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Could not write the pack manifest: " + e.Message);
            }
        }

        /// <summary>
        /// Downloads the logo, flag and gallery images from the catalog URLs
        /// in parallel and saves them into <paramref name="packFolder"/>/images.
        /// Returns an array: [0] = logo relative path (or null), [1] = flag
        /// relative path (or null), [2] = gallery paths joined with '|' (or
        /// null). Failed downloads are reported as null entries but never
        /// abort the install; cancellation does abort it.
        /// </summary>
        private static async Task<string[]> DownloadPackImagesAsync(string packFolder, string logoUrl, string flagUrl,
            string[] galleryUrls, CancellationToken ct)
        {
            int galleryCount = galleryUrls != null ? galleryUrls.Length : 0;
            Task<string>[] galleryTasks = new Task<string>[galleryCount];
            for (int i = 0; i < galleryCount; i++)
            {
                galleryTasks[i] = DownloadImageToPackAsync(packFolder, galleryUrls[i], "gallery" + i, ct);
            }

            // Logo and flag download alongside the gallery images (all awaited
            // below, so every image finishes before the install continues).
            string logoPath = await DownloadImageToPackAsync(packFolder, logoUrl, "logo", ct);
            string flagPath = await DownloadImageToPackAsync(packFolder, flagUrl, "flag", ct);

            string galleryJoined = null;
            List<string> galleryPaths = new List<string>();
            foreach (Task<string> task in galleryTasks)
            {
                string p = await task;
                if (!string.IsNullOrEmpty(p))
                {
                    galleryPaths.Add(p);
                }
            }
            if (galleryPaths.Count > 0)
            {
                galleryJoined = string.Join("|", galleryPaths.ToArray());
            }

            return new string[] { logoPath, flagPath, galleryJoined };
        }

        /// <summary>
        /// Downloads one image URL into the pack's images folder. The file
        /// extension comes from the response Content-Type, falling back to
        /// magic-byte sniffing of the body; an unrecognized image keeps .bin.
        /// Returns the path relative to the pack folder, or null on failure.
        /// Never throws except for cancellation.
        /// </summary>
        private static async Task<string> DownloadImageToPackAsync(string packFolder, string imageUrl, string fileNameHint, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return null;
            }

            // Defense in depth (21.1): CatalogParser already validates image URLs
            // against the allowlist, but this code also consumes entries built
            // locally (installed packs), so the host is re-checked here. A URL
            // from any other source is skipped instead of fetched (SSRF guard).
            if (!UrlPolicy.IsAllowedImageUrl(imageUrl))
            {
                Logging.Warn("[PackInstaller] Skipping image from a non-allowlisted host: " + imageUrl);
                return null;
            }

            try
            {
                using (HttpResponseMessage response = await GetResponseAsync(imageUrl, ct))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Logging.Warn("[PackInstaller] Could not download image " + imageUrl + ": HTTP " + (int)response.StatusCode);
                        return null;
                    }

                    string contentType = response.Content.Headers.ContentType != null
                        ? (response.Content.Headers.ContentType.MediaType ?? "").ToLowerInvariant()
                        : "";

                    byte[] data;
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    using (MemoryStream memory = new MemoryStream())
                    {
                        byte[] buffer = new byte[81920];
                        int read;
                        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                        {
                            if (memory.Length + read > MaxImageBytes)
                            {
                                Logging.Warn("[PackInstaller] Image " + imageUrl + " exceeds the "
                                    + (MaxImageBytes / (1024 * 1024)) + " MB limit.");
                                return null;
                            }
                            memory.Write(buffer, 0, read);
                        }
                        data = memory.ToArray();
                    }

                    if (data.Length == 0)
                    {
                        return null;
                    }

                    string ext = ExtensionFromContentType(contentType) ?? ExtensionFromMagicBytes(data) ?? ".bin";
                    string fileName = fileNameHint + ext;

                    string imagesFolder = Path.Combine(packFolder, "images");
                    Directory.CreateDirectory(imagesFolder);
                    File.WriteAllBytes(Path.Combine(imagesFolder, fileName), data);
                    return "images/" + fileName;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Could not download image " + imageUrl + ": " + e.Message);
                return null;
            }
        }

        /// <summary>Maps an image Content-Type to a file extension, or null when unknown.</summary>
        private static string ExtensionFromContentType(string contentType)
        {
            switch (contentType)
            {
                case "image/png":
                    return ".png";
                case "image/jpeg":
                case "image/jpg":
                    return ".jpg";
                case "image/gif":
                    return ".gif";
                case "image/webp":
                    return ".webp";
                default:
                    return null;
            }
        }

        /// <summary>Sniffs the file extension from the image magic bytes, or null when unknown.</summary>
        private static string ExtensionFromMagicBytes(byte[] data)
        {
            if (data.Length >= 4 &&
                data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                return ".png"; // PNG signature
            }
            if (data.Length >= 3 &&
                data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return ".jpg"; // JPEG SOI
            }
            if (data.Length >= 4 &&
                data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
            {
                return ".gif"; // GIF8
            }
            if (data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
            {
                return ".webp"; // RIFF....WEBP
            }
            return null;
        }

        /// <summary>
        /// Removes the image files that installs used to write into the pack
        /// root (logo.bin / flag.bin / galleryN.bin) before the images folder
        /// existed. New images always live in images/, so a root-level .bin
        /// can only come from an archive that shipped one.
        /// </summary>
        private static void DeleteLegacyImageFiles(string packFolder)
        {
            try
            {
                string[] patterns = { "logo.bin", "flag.bin", "gallery*.bin" };
                foreach (string pattern in patterns)
                {
                    foreach (string file in Directory.GetFiles(packFolder, pattern))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Could not delete the legacy image files: " + e.Message);
            }
        }

        /// <summary>
        /// Stamps the catalog's stable translationId into the installed
        /// translation.json so the pack can be identified without its manifest.
        /// Only translationId is touched: the display identity (name, code,
        /// authors) lives in the manifest, and overwriting the pack's own
        /// metadata with catalog values produced mismatched stamps. Every
        /// other key keeps its exact value. Reads and writes the JSON as a
        /// JObject.
        /// </summary>
        private static void StampTranslationId(string translationFile, string translationId)
        {
            try
            {
                JObject root = JObject.Parse(File.ReadAllText(translationFile));
                JObject metadata = root["metadata"] as JObject;
                if (metadata == null)
                {
                    metadata = new JObject();
                    root["metadata"] = metadata;
                }
                if (!string.IsNullOrWhiteSpace(translationId))
                {
                    metadata["translationId"] = translationId;
                }
                File.WriteAllText(translationFile, root.ToString(Formatting.Indented));
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Could not stamp metadata into translation.json: " + e.Message);
            }
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static void MoveDirectory(string source, string target)
        {
            try
            {
                Directory.Move(source, target);
            }
            catch (IOException)
            {
                // Across volumes (temp on another drive): fall back to copy+delete.
                CopyDirectoryRecursive(source, target);
                Directory.Delete(source, true);
            }
        }

        private static void CopyDirectoryRecursive(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }
            foreach (string dir in Directory.GetDirectories(source))
            {
                CopyDirectoryRecursive(dir, Path.Combine(target, Path.GetFileName(dir)));
            }
        }

        private static void CleanupPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Could not clean up '" + path + "': " + e.Message);
            }
        }

        // -------------------------------------------------------------------
        // Archive cache: keeps downloaded packs in BepInEx/config/ultrakull/cache/
        // so repeated install attempts of the same translation don't re-download.
        // -------------------------------------------------------------------
        private static string CacheDir
        {
            get
            {
                string dir = Path.Combine(Paths.ConfigPath, "ultrakull", "cache");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string GetCachedArchive(string entryId)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return null;
            }
            // Cache file name is a sanitized hash of the entry id; extension unknown,
            // so we probe for any file matching "cache_<entryId>.*".
            string safeId = entryId.Replace("/", "_").Replace("\\", "_");
            string pattern = "cache_" + safeId + ".*";
            foreach (string file in Directory.GetFiles(CacheDir, pattern))
            {
                if (File.Exists(file))
                {
                    return file;
                }
            }
            return null;
        }

        private static void CacheArchive(string entryId, string sourcePath)
        {
            if (string.IsNullOrEmpty(entryId) || string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
            {
                return;
            }
            try
            {
                string safeId = entryId.Replace("/", "_").Replace("\\", "_");
                string ext = Path.GetExtension(sourcePath);
                string dest = Path.Combine(CacheDir, "cache_" + safeId + ext);
                if (!File.Exists(dest))
                {
                    File.Copy(sourcePath, dest, true);
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[PackInstaller] Could not cache archive for '" + entryId + "': " + e.Message);
            }
        }

        private static string ClassifyFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".json")
            {
                return "json";
            }
            if (ext == ".zip")
            {
                return "zip";
            }
            if (ext == ".7z")
            {
                return "7z";
            }
            // Fall back to magic-byte sniffing for extensionless files.
            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    byte[] header = new byte[6];
                    int read = fs.Read(header, 0, header.Length);
                    if (read >= 2 && header[0] == 0x50 && header[1] == 0x4B)
                    {
                        return "zip";
                    }
                    if (read >= 6 && header[0] == 0x37 && header[1] == 0x7A && header[2] == 0xBC && header[3] == 0xAF && header[4] == 0x27 && header[5] == 0x1C)
                    {
                        return "7z";
                    }
                    if (read >= 1 && header[0] == 0x7B)
                    {
                        return "json";
                    }
                }
            }
            catch
            {
                // Ignore; return null below.
            }
            return null;
        }

        private static string ContentFileName(HttpResponseMessage response)
        {
            try
            {
                System.Net.Http.Headers.ContentDispositionHeaderValue disposition = response.Content.Headers.ContentDisposition;
                if (disposition == null)
                {
                    return null;
                }
                return disposition.FileNameStar ?? disposition.FileName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool HasZipExtension(string fileName)
        {
            return fileName != null && fileName.ToLowerInvariant().EndsWith(".zip");
        }

        private static bool Has7zExtension(string fileName)
        {
            return fileName != null && fileName.ToLowerInvariant().EndsWith(".7z");
        }

        private static string UrlExtension(string url)
        {
            try
            {
                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    return "";
                }
                string extension = Path.GetExtension(uri.AbsolutePath);
                return string.IsNullOrEmpty(extension) ? "" : extension.ToLowerInvariant();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static void Report(IProgress<InstallProgress> progress, string stage, long bytesRead, long? totalBytes)
        {
            if (progress != null)
            {
                progress.Report(new InstallProgress { Stage = stage, BytesRead = bytesRead, TotalBytes = totalBytes });
            }
        }

        private static InstallResult Failed(string message)
        {
            return new InstallResult { Status = PackInstallStatus.Failed, Message = message };
        }
    }
}
