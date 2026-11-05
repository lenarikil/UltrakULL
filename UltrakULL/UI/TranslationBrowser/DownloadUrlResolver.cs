using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UltrakULL.json;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>Outcome of resolving an entry's download URL.</summary>
    public sealed class DownloadResolution
    {
        /// <summary>
        /// Best candidate for a direct download. May still turn out to be an HTML
        /// page (the dispatcher in PackInstaller decides by Content-Type).
        /// </summary>
        public string CandidateUrl;

        /// <summary>
        /// Non-null when the link cannot be turned into a direct download: the UI
        /// then offers to open this page in the external browser instead.
        /// </summary>
        public string ExternalPageUrl;
    }

    /// <summary>
    /// Converts "page" links of well-known file hosts into direct download URLs.
    /// Only officially documented, dependency-free transforms:
///  - Dropbox: dl=0 -> dl=1 (documented sharing parameter);
///  - Yandex Disk: official public REST API (no key needed) returns a direct
///    time-limited href;
///  - GitHub: blob -> raw.githubusercontent.
/// Unknown hosts pass through untouched: the Content-Type dispatcher decides
    /// whether the URL is actually a file.
    ///
    /// No JavaScript scraping, no captcha solving, no API keys, no new NuGet
    /// packages (net472: HttpClientHandler with a CookieContainer instead of
    /// .NET Core's SocketsHttpHandler, classic using blocks for C# 7.3).
    /// </summary>
    public static class DownloadUrlResolver
    {
        /// <summary>
        /// The only non-catalog host the resolver ever talks to: the official
        /// Yandex Disk public API (documented, keyless).
        /// </summary>
        public const string YandexApiBase = "https://cloud-api.yandex.net/v1/disk/public/resources/download";

        private const int YandexTimeoutSeconds = 15;
        private const int YandexAttempts = 2;

        // The Yandex Disk host set lives in UrlPolicy (YandexDiskHosts): one
        // shared source for the resolver and the storage-host policy.

        /// <summary>
        /// Applies the offline (no network) transforms for known hosts. Yandex
        /// links are returned unchanged here; resolving them needs the API call
        /// (<see cref="ResolveYandexAsync"/>).
        /// </summary>
        public static DownloadResolution ResolveOffline(string url)
        {
            string trimmed = TrimToNull(url);
            if (trimmed == null)
            {
                return new DownloadResolution { ExternalPageUrl = url };
            }

            Uri uri;
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out uri) ||
                (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)))
            {
                // Not a usable web URL at all: the storage-host policy in
                // PackInstaller decides what happens with it.
                return new DownloadResolution { ExternalPageUrl = url };
            }

            string host = uri.Host.ToLowerInvariant();

            if (host.Equals("www.dropbox.com") || host.Equals("dl.dropboxusercontent.com") || host.Equals("dropbox.com"))
            {
                if (host.Equals("dl.dropboxusercontent.com"))
                {
                    // Already a direct content host.
                    return new DownloadResolution { CandidateUrl = trimmed };
                }
                return new DownloadResolution { CandidateUrl = ForceDropboxDirect(trimmed) };
            }

            // Google Drive is no longer supported: treat its links as an
            // unsupported source instead of trying to extract a file id.
            if (host.Equals("drive.google.com") || host.Equals("docs.google.com") ||
                host.Equals("drive.usercontent.google.com"))
            {
                return new DownloadResolution { ExternalPageUrl = trimmed };
            }

            if (host.Equals("github.com") || host.Equals("www.github.com"))
            {
                string raw = ConvertGithubBlobToRaw(trimmed);
                return new DownloadResolution { CandidateUrl = raw };
            }

            if (UrlPolicy.IsYandexDiskHost(host))
            {
                // Marked as Yandex, but the actual resolution happens online.
                return new DownloadResolution { CandidateUrl = trimmed };
            }

            // Unknown host: may be a direct link to a file; the Content-Type
            // dispatcher in PackInstaller makes the final call.
            return new DownloadResolution { CandidateUrl = trimmed };
        }

        /// <summary>
        /// Full resolution: offline transforms first, then the Yandex API when the
        /// link points at Yandex Disk. Never throws; failures degrade to
        /// ExternalPage.
        /// </summary>
        public static async Task<DownloadResolution> ResolveAsync(HttpClient client, string url)
        {
            DownloadResolution resolution = ResolveOffline(url);

            if (resolution.CandidateUrl != null && IsYandexLink(resolution.CandidateUrl))
            {
                string href = await ResolveYandexAsync(client, resolution.CandidateUrl);
                if (href == null)
                {
                    Logging.Warn("[DownloadUrlResolver] Yandex Disk API did not return a download link.");
                    return new DownloadResolution { ExternalPageUrl = resolution.CandidateUrl };
                }
                return new DownloadResolution { CandidateUrl = href };
            }

            return resolution;
        }

        public static bool IsYandexLink(string url)
        {
            string trimmed = TrimToNull(url);
            if (trimmed == null)
            {
                return false;
            }
            Uri uri;
            return Uri.TryCreate(trimmed, UriKind.Absolute, out uri) && UrlPolicy.IsYandexDiskHost(uri.Host.ToLowerInvariant());
        }

        /// <summary>
        /// Calls the official Yandex Disk public API: GET download?public_key=&lt;url&gt;
        /// returns a short-lived direct href. One retry on transient failure; the
        /// href is used immediately and never cached (it expires).
        /// </summary>
        public static async Task<string> ResolveYandexAsync(HttpClient client, string publicUrl)
        {
            if (client == null || string.IsNullOrEmpty(publicUrl))
            {
                return null;
            }

            string requestUrl = YandexApiBase + "?public_key=" + Uri.EscapeDataString(publicUrl);

            Exception lastError = null;
            for (int attempt = 0; attempt < YandexAttempts; attempt++)
            {
                try
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
                    using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException("Yandex API returned HTTP " + (int)response.StatusCode);
                        }

                        string json = await response.Content.ReadAsStringAsync();
                        return ExtractYandexHref(json);
                    }
                }
                catch (Exception e)
                {
                    lastError = e;
                    Logging.Warn("[DownloadUrlResolver] Yandex API attempt " + (attempt + 1) + " failed: " + e.Message);
                }
            }

            if (lastError != null)
            {
                Logging.Warn("[DownloadUrlResolver] Yandex API resolution failed: " + lastError);
            }
            return null;
        }

        /// <summary>
        /// Extracts the "href" field from a Yandex API response. Pure so it can be
        /// unit tested with a canned response body.
        /// </summary>
        public static string ExtractYandexHref(string apiJson)
        {
            if (string.IsNullOrWhiteSpace(apiJson))
            {
                return null;
            }

            try
            {
                Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(apiJson);
                string href = TrimToNull((string)obj["href"]);
                return UrlPolicy.IsHttpUrl(href) ? href : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // -------------------------------------------------------------------
        // Dropbox
        // -------------------------------------------------------------------

        /// <summary>
        /// Documented Dropbox transform: the sharing parameter dl=0 (web preview)
        /// becomes dl=1 (direct download). Works for both the old /s/ links and
        /// the current /scl/fi/ links; a missing parameter is appended.
        /// </summary>
        public static string ForceDropboxDirect(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            int queryStart = url.IndexOf('?');
            if (queryStart < 0)
            {
                return url + "?dl=1";
            }

            string baseUrl = url.Substring(0, queryStart);
            string query = url.Substring(queryStart + 1);

            // Split manually: Uri query strings may contain '+' etc. that
            // NameValueCollection parsing would alter.
            string[] parameters = query.Split('&');
            bool replaced = false;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].StartsWith("dl=", StringComparison.OrdinalIgnoreCase))
                {
                    parameters[i] = "dl=1";
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                return baseUrl + "?" + query + "&dl=1";
            }
            return baseUrl + "?" + string.Join("&", parameters);
        }

        // -------------------------------------------------------------------
        // Google Drive
        // -------------------------------------------------------------------

        /// <summary>
        /// Extracts the file id from the known Drive URL shapes:
        /// /file/d/{id}/view, ?id={id}, /open?id={id}, /uc?export=download&amp;id={id}.
        /// </summary>
        public static string ExtractDriveFileId(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            string path = uri.AbsolutePath;

            // /file/d/{id}/... and /d/{id}/...
            int marker = path.IndexOf("/file/d/", StringComparison.OrdinalIgnoreCase);
            if (marker < 0)
            {
                marker = path.IndexOf("/d/", StringComparison.OrdinalIgnoreCase);
            }
            if (marker >= 0)
            {
                string rest = path.Substring(marker);
                string[] parts = rest.Split('/');
                // ["", "file", "d", "{id}", ...] or ["", "d", "{id}", ...]
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if ((parts[i].Equals("d", StringComparison.OrdinalIgnoreCase)) && parts[i + 1].Length > 0)
                    {
                        return parts[i + 1];
                    }
                }
            }

            // Query parameter id={id}
            string query = uri.Query;
            if (!string.IsNullOrEmpty(query))
            {
                foreach (string parameter in query.TrimStart('?').Split('&'))
                {
                    if (parameter.StartsWith("id=", StringComparison.OrdinalIgnoreCase) && parameter.Length > 3)
                    {
                        return parameter.Substring(3);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Direct-download candidate for a Drive file. confirm=t skips the
        /// "this file can't be scanned for viruses" interstitial for most files;
        /// the form-POST fallback in PackInstaller handles the rest.
        /// </summary>
        public static string BuildDriveDownloadUrl(string fileId)
        {
            return "https://drive.usercontent.google.com/download?id=" + Uri.EscapeDataString(fileId)
                + "&export=download&confirm=t";
        }

        // -------------------------------------------------------------------
        // GitHub
        // -------------------------------------------------------------------

        /// <summary>
        /// github.com/&lt;user&gt;/&lt;repo&gt;/blob/... -> raw.githubusercontent.com/... .
        /// Release download links (and raw links) carry no /blob/ segment and
        /// pass through unchanged.
        /// </summary>
        public static string ConvertGithubBlobToRaw(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            const string hostMarker = "github.com/";
            int index = url.IndexOf(hostMarker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return url;
            }

            // "user/repo/blob/<ref>/<path>" -> "user/repo/<ref>/<path>"
            string rest = url.Substring(index + hostMarker.Length);
            int blobAt = rest.IndexOf("/blob/", StringComparison.OrdinalIgnoreCase);
            if (blobAt < 0)
            {
                return url;
            }

            string path = rest.Substring(0, blobAt) + "/" + rest.Substring(blobAt + "/blob/".Length);
            return "https://raw.githubusercontent.com/" + path;
        }

        // -------------------------------------------------------------------
        // Drive interstitial fallback
        // -------------------------------------------------------------------

        /// <summary>Parsed Drive interstitial form.</summary>
        public sealed class DriveForm
        {
            public string ActionUrl;
            public Dictionary<string, string> Fields = new Dictionary<string, string>();
        }

        // Hidden input tag with name and value attributes in either order.
        private static readonly Regex InputTagRegex = new Regex(
            "<input\\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AttributeRegex = new Regex(
            "(name|value|type|action)\\s*=\\s*\"([^\"]*)\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FormActionRegex = new Regex(
            "<form\\b[^>]*action\\s*=\\s*\"([^\"]*)\"",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parses the Drive "cannot scan this file for viruses" interstitial: the
        /// page carries a POST form with hidden fields (id, export, confirm, uuid,
        /// at). Only the known field names are extracted; nothing else in the HTML
        /// is interpreted, and no URLs are guessed from the page body.
        /// </summary>
        public static DriveForm ParseDriveInterstitial(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            Match formMatch = FormActionRegex.Match(html);
            if (!formMatch.Success)
            {
                return null;
            }

            DriveForm form = new DriveForm();
            string action = TrimToNull(formMatch.Groups[1].Value);
            if (action == null || !UrlPolicy.IsHttpUrl(action))
            {
                return null;
            }
            form.ActionUrl = action;

            foreach (Match input in InputTagRegex.Matches(html))
            {
                string name = null;
                string value = null;
                string type = null;

                // Scan the tag's attribute pairs (name/value order independent).
                foreach (Match attribute in AttributeRegex.Matches(input.Value))
                {
                    string key = attribute.Groups[1].Value.ToLowerInvariant();
                    string attrValue = attribute.Groups[2].Value;
                    if (key == "name") name = attrValue;
                    else if (key == "value") value = attrValue;
                    else if (key == "type") type = attrValue;
                }

                if (string.IsNullOrEmpty(name) || !string.Equals(type, "hidden", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                form.Fields[name] = value ?? "";
            }

            // The interstitial form must at least identify the file; without an
            // id a POST would be pointless.
            if (!form.Fields.ContainsKey("id") || form.Fields.Count < 2)
            {
                return null;
            }
            return form;
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
