using System;

namespace UltrakULL.json
{
    /// <summary>
    /// Shared URL policy for every untrusted remote URL the translation browser
    /// touches (catalog images, downloads). Deliberately free of Unity/BepInEx
    /// dependencies so it can be unit tested and used from both the pure catalog
    /// parser and the runtime image cache.
    /// </summary>
    public static class UrlPolicy
    {
        /// <summary>
        /// Hosts allowed for catalog images (logos, gallery). The catalog is
        /// machine-generated, but the URLs inside it are still untrusted input:
        /// restricting them to these hosts stops a tampered catalog snapshot from
        /// making the game fetch images from arbitrary servers.
        /// </summary>
        public static readonly string[] ImageHostAllowlist =
        {
            "cdn.discordapp.com",
            "media.discordapp.net",
            "ultrakull-catalog.pages.dev"
        };

        /// <summary>True when the host (case-insensitive) may serve catalog images.</summary>
        public static bool IsAllowedImageHost(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            foreach (string allowed in ImageHostAllowlist)
            {
                if (string.Equals(host, allowed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// True for an absolute https URL whose host is in the image allowlist.
        /// Everything else (http, other schemes, relative junk) is rejected so the
        /// UI falls back to its placeholder instead of loading it.
        /// </summary>
        public static bool IsAllowedImageUrl(string url)
        {
            string trimmed = TrimToNull(url);
            if (trimmed == null)
            {
                return false;
            }

            Uri uri;
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out uri))
            {
                return false;
            }
            return string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)
                && IsAllowedImageHost(uri.Host);
        }

        /// <summary>
        /// Yandex Disk web hosts (yadi.sk and the regional disk/360 domains),
        /// moved here from DownloadUrlResolver so all URL policy lives in one
        /// place. Matched exactly, without subdomains.
        /// </summary>
        public static readonly string[] YandexDiskHosts =
        {
            "yadi.sk",
            "disk.yandex.ru",
            "disk.yandex.com",
            "disk.yandex.by",
            "disk.yandex.kz",
            "disk.yandex.uz",
            "disk.yandex.com.tr",
            "360.yandex.ru",
            "360.yandex.com"
        };

        /// <summary>True when the host is one of the Yandex Disk web hosts (exact match).</summary>
        public static bool IsYandexDiskHost(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            foreach (string yandex in YandexDiskHosts)
            {
                if (string.Equals(host, yandex, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Every host the translation browser treats as a file storage, in one
        /// list: hosts the download chain can automate (Dropbox, Yandex, GitHub)
        /// plus popular browser-only storages (Mega, MediaFire, OneDrive). Google
        /// Drive is intentionally NOT supported anymore. When a link cannot be turned into a direct file download,
        /// these hosts may still be handed to the external browser; anything
        /// else (video pages etc.) is rejected as an unsupported source.
        /// </summary>
        public static readonly string[] StorageHostAllowlist =
        {
            // Automatable: DownloadUrlResolver turns their page links into
            // direct downloads.
            "dropbox.com",
            "dl.dropboxusercontent.com",
            "github.com",
            "raw.githubusercontent.com",

            // Browser-only: no documented direct-download path; the mod opens
            // these in the external browser instead.
            "mega.nz",
            "mega.co.nz",
            "mediafire.com",
            "onedrive.live.com",
            "1drv.ms",

            // Yandex Disk web hosts (see YandexDiskHosts).
            "yadi.sk",
            "disk.yandex.ru",
            "disk.yandex.com",
            "disk.yandex.by",
            "disk.yandex.kz",
            "disk.yandex.uz",
            "disk.yandex.com.tr",
            "360.yandex.ru",
            "360.yandex.com"
        };

        /// <summary>
        /// True when the host is a known file storage: an exact match or a
        /// subdomain of an allowlist entry ("www.dropbox.com" matches
        /// "dropbox.com"; "notdropbox.com" does not).
        /// </summary>
        public static bool IsKnownStorageHost(string host)
        {
            string trimmed = TrimToNull(host);
            if (trimmed == null)
            {
                return false;
            }

            string lower = trimmed.ToLowerInvariant();
            foreach (string allowed in StorageHostAllowlist)
            {
                if (string.Equals(lower, allowed, StringComparison.OrdinalIgnoreCase) ||
                    lower.EndsWith("." + allowed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// True for absolute http/https URLs. Used for the download link: unlike
        /// images, arbitrary hosts are allowed (packs live on Drive/Dropbox/etc.),
        /// but non-http schemes (file:, javascript:, ...) are always rejected.
        /// </summary>
        public static bool IsHttpUrl(string url)
        {
            string trimmed = TrimToNull(url);
            if (trimmed == null)
            {
                return false;
            }

            Uri uri;
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out uri))
            {
                return false;
            }
            return string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
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
