using System.IO;
using UltrakULL.json;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Pure policy for choosing and sanitizing language pack folder names during
    /// an install. Extracted from PackInstaller so it can be compiled into the
    /// test project; PackInstaller performs the actual file moves.
    /// </summary>
    public static class PackFolderPolicy
    {
        /// <summary>
        /// Picks the installation folder name: when the archive carried its own
        /// top-level folder name, that name is preferred so the installed pack
        /// keeps the layout the authors shipped; otherwise the sanitized locale
        /// is used. On a collision with a folder that does not belong to this
        /// translation, the locale/id (or name/id) suffix keeps installs from
        /// overwriting each other. Folders of manual installs are never chosen
        /// as targets.
        /// </summary>
        public static string ResolveTargetFolder(TranslationEntry entry, string packsRoot, string archiveFolderName = null)
        {
            string baseName = !string.IsNullOrWhiteSpace(archiveFolderName)
                ? archiveFolderName
                : (entry != null ? entry.languageTag : null);

            string tag = SanitizeFolderName(baseName);
            if (tag != null)
            {
                string dir = Path.Combine(packsRoot, tag);
                if (!Directory.Exists(dir))
                {
                    return tag;
                }
                if (FolderBelongsToTranslation(dir, entry != null ? entry.id : null))
                {
                    return tag;
                }
            }

            string idPart = entry != null && !string.IsNullOrEmpty(entry.id)
                ? entry.id.Substring(0, System.Math.Min(8, entry.id.Length))
                : "pack";
            // Locale/name + id prefix; entries without a usable base name get a
            // plain "pack-<id>" name.
            string fallbackBase = tag != null ? tag : (entry != null ? entry.languageTag : null);
            string fallback = fallbackBase != null
                ? SanitizeFolderName(fallbackBase + "_" + idPart)
                : null;
            return fallback ?? ("pack-" + idPart);
        }

        /// <summary>
        /// True when the folder is a browser install of this exact translation
        /// (manifest or metadata translationId). A folder without identity data
        /// is assumed to be a manual install and never matches.
        /// </summary>
        public static bool FolderBelongsToTranslation(string folder, string translationId)
        {
            if (string.IsNullOrEmpty(translationId))
            {
                return false;
            }

            try
            {
                string manifestPath = Path.Combine(folder, TranslationManifest.FileName);
                if (File.Exists(manifestPath))
                {
                    TranslationManifest manifest = TranslationManifest.Load(manifestPath);
                    if (manifest == null)
                    {
                        return false;
                    }
                    return string.Equals(manifest.translationId, translationId, System.StringComparison.Ordinal);
                }

                InstalledPackMetadata metadata = InstalledPackRegistry.ReadMetadata(folder);
                return metadata != null && string.Equals(metadata.translationId, translationId, System.StringComparison.Ordinal);
            }
            catch (System.Exception)
            {
                // A broken folder is treated as "not ours": installation continues
                // into a fresh folder instead of overwriting it.
                return false;
            }
        }

        /// <summary>
        /// Makes a string safe as a pack folder name: invalid filename characters
        /// become underscores, length is capped, and reserved prefixes (_, ., -,
        /// spaces) get a "pack-" prefix so the folder is always scannable.
        /// </summary>
        public static string SanitizeFolderName(string name)
        {
            string result = name;
            if (string.IsNullOrWhiteSpace(result))
            {
                return null;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(c, '_');
            }
            result = result.Trim();
            if (result.Length > 60)
            {
                result = result.Substring(0, 60).Trim();
            }
            if (result.Length == 0)
            {
                return null;
            }

            char first = result[0];
            if (first == '_' || first == '.' || first == '-' || first == ' ')
            {
                result = "pack-" + result;
            }
            return result;
        }
    }
}
