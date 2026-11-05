using System;
using System.IO;
using UltrakULL.json;

namespace UltrakULL.json
{
    /// <summary>
    /// Creates the mandatory ultrakull_manifest.json for language packs that
    /// do not have one yet (manual installs made before the manifest became
    /// mandatory). Runs from LanguageManager whenever a pack is loaded; the
    /// check is a cheap File.Exists, so repeated scans stay free.
    ///
    /// Fault-tolerant by design: any failure (read-only folder, locked file,
    /// broken metadata) only logs a warning - the game must never fail to
    /// start because a manifest could not be written. Readers fall back to
    /// the pack's translation.json metadata in that case.
    /// </summary>
    public static class ManifestMigrator
    {
        /// <summary>
        /// Writes the manifest for <paramref name="packFolder"/> when it does
        /// not exist yet, deriving the identity from the pack's metadata.
        /// </summary>
        public static void MigrateIfNeeded(string packFolder, Metadata metadata)
        {
            if (string.IsNullOrEmpty(packFolder))
            {
                return;
            }

            try
            {
                string manifestFile = Path.Combine(packFolder, TranslationManifest.FileName);
                if (File.Exists(manifestFile))
                {
                    return;
                }

                string folderName = Path.GetFileName(packFolder);
                string languageCode = TrimToNull(metadata != null ? metadata.languageCode : null);
                TranslationManifest manifest = new TranslationManifest
                {
                    name = PickName(metadata, folderName, languageCode),
                    languageCode = languageCode,
                    authors = TrimToNull(metadata != null ? metadata.langAuthors : null)
                        ?? TrimToNull(metadata != null ? metadata.credits : null),
                    version = TrimToNull(metadata != null ? metadata.translationVersion : null),
                    source = "manual",
                    installedAt = DateTime.UtcNow.ToString("o")
                };

                TranslationManifest.Save(manifestFile, manifest);
                Logging.Info("[ManifestMigrator] Created the manifest for the manual pack '" + folderName + "'.");
            }
            catch (Exception e)
            {
                Logging.Warn("[ManifestMigrator] Could not create the manifest for '" + packFolder + "': " + e.Message);
            }
        }

        /// <summary>
        /// In-memory manifest of the built-in English pseudo-pack (it has no
        /// folder on disk, so nothing is ever written).
        /// </summary>
        public static TranslationManifest CreateBuiltinManifest(string name, string languageCode, string version)
        {
            return new TranslationManifest
            {
                name = name,
                languageCode = languageCode,
                version = version,
                source = "builtin"
            };
        }

        private static string PickName(Metadata metadata, string folderName, string languageCode)
        {
            string name = TrimToNull(metadata != null ? metadata.languageName : null);
            if (name == null ||
                string.Equals(name, languageCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, folderName, StringComparison.OrdinalIgnoreCase))
            {
                return folderName;
            }
            return name;
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
