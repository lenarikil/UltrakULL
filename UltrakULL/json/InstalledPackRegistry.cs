using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace UltrakULL.json
{
    /// <summary>
    /// Read-only metadata snapshot of one installed pack's translation.json.
    /// Only the fields the browser needs for status resolution are extracted.
    /// </summary>
    public class InstalledPackMetadata
    {
        public string languageCode;
        public string languageName;
        public string translationVersion;
        public string translationId;
        public string authors;
    }

    /// <summary>
    /// One folder under config/ultrakull as seen by the browser. A pack is a
    /// folder with a translation.json; a manifest-only folder (translation.json
    /// deleted by hand) is reported with TranslationFileMissing instead of being
    /// hidden, so the UI can explain the state instead of silently ignoring it.
    /// </summary>
    public class InstalledPackInfo
    {
        /// <summary>Absolute folder path.</summary>
        public string Folder;

        /// <summary>Folder name; the dictionary key LanguageManager uses for this pack.</summary>
        public string PackKey;

        /// <summary>Parsed ultrakull_manifest.json, or null when missing/broken.</summary>
        public TranslationManifest Manifest;

        /// <summary>True when a manifest file exists but could not be parsed.</summary>
        public bool ManifestBroken;

        /// <summary>Metadata snapshot, or null when translation.json is missing or unparseable.</summary>
        public InstalledPackMetadata Metadata;

        /// <summary>True when a manifest exists but translation.json is gone.</summary>
        public bool TranslationFileMissing;
    }

    /// <summary>
    /// Read-only scan of the language pack folders for the translation browser.
    /// LanguageManager stays the writer/owner of pack data; this registry never
    /// deletes, moves or rewrites anything, it only inspects. Pure System.IO +
    /// Newtonsoft so it can be compiled into the test project.
    /// </summary>
    public static class InstalledPackRegistry
    {
        public static List<InstalledPackInfo> Scan(string rootDirectory)
        {
            List<InstalledPackInfo> packs = new List<InstalledPackInfo>();
            if (string.IsNullOrEmpty(rootDirectory) || !Directory.Exists(rootDirectory))
            {
                return packs;
            }

            foreach (string directory in Directory.GetDirectories(rootDirectory))
            {
                string dirName = Path.GetFileName(directory);
                // Same skip rule as LanguageManager: "_" and "." prefixes are not
                // language packs (_template, ".bak" leftovers, hidden folders...).
                if (dirName.StartsWith("_") || dirName.StartsWith("."))
                {
                    continue;
                }

                string translationFile = Path.Combine(directory, "translation.json");
                string manifestFile = Path.Combine(directory, TranslationManifest.FileName);
                bool translationExists = File.Exists(translationFile);
                bool manifestExists = File.Exists(manifestFile);

                if (!translationExists && !manifestExists)
                {
                    continue;
                }

                InstalledPackInfo info = new InstalledPackInfo();
                info.Folder = directory;
                info.PackKey = dirName;
                info.TranslationFileMissing = !translationExists;

                if (manifestExists)
                {
                    try
                    {
                        info.Manifest = TranslationManifest.Load(manifestFile);
                        info.ManifestBroken = info.Manifest == null;
                    }
                    catch
                    {
                        info.Manifest = null;
                        info.ManifestBroken = true;
                    }
                }

                if (translationExists)
                {
                    info.Metadata = ReadMetadata(directory);
                }

                packs.Add(info);
            }

            return packs;
        }

        /// <summary>
        /// Extracts only the metadata object of a pack folder's translation.json.
        /// Full deserialization through LanguageManager.TryLoad is intentionally
        /// avoided: it performs missing-key synchronization and rewrites the file,
        /// which must never happen from a read-only status/identity check.
        /// </summary>
        public static InstalledPackMetadata ReadMetadata(string packFolder)
        {
            string translationFile = Path.Combine(packFolder, "translation.json");
            try
            {
                JObject root = JObject.Parse(File.ReadAllText(translationFile));
                JToken token = root["metadata"];
                if (token == null || token.Type != JTokenType.Object)
                {
                    return null;
                }

                Metadata metadata = token.ToObject<Metadata>();
                if (metadata == null)
                {
                    return null;
                }

                return new InstalledPackMetadata
                {
                    languageCode = TrimToNull(metadata.languageCode),
                    languageName = TrimToNull(metadata.languageName),
                    translationVersion = TrimToNull(metadata.translationVersion),
                    translationId = TrimToNull(metadata.translationId),
                    authors = TrimToNull(metadata.langAuthors) ?? TrimToNull(metadata.credits)
                };
            }
            catch
            {
                // Broken translation.json: reported as "no metadata" (null), which
                // the status resolver treats as a manual/unknown installation.
                return null;
            }
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
