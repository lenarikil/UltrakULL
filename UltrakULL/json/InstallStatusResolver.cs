using System;
using System.Collections.Generic;

namespace UltrakULL.json
{
    /// <summary>
    /// Installation state of a catalog entry as shown on its card and details
    /// panel. The honesty rules:
    ///  - <see cref="Installed"/> and <see cref="UpdateAvailable"/> are assigned
    ///    ONLY on an exact identity match (manifest or metadata translationId);
    ///  - <see cref="NotInstalled"/> is shown only when no pack of the entry's
    ///    locale exists at all, so a manually installed language of the same
    ///    locale is never reported as "not installed";
    ///  - everything uncertain is <see cref="LanguageInstalledManual"/> or
    ///    <see cref="Unknown"/>.
    /// </summary>
    public enum InstallStatus
    {
        NotInstalled,
        Installed,
        UpdateAvailable,
        LanguageInstalledManual,
        Unknown
    }

    /// <summary>How the status was determined; drives the UI tooltip text.</summary>
    public enum MatchBasis
    {
        None,
        /// <summary>Exact match via ultrakull_manifest.json.</summary>
        Manifest,
        /// <summary>Exact match via metadata.translationId inside translation.json.</summary>
        MetadataTranslationId,
        /// <summary>Same locale and strong fuzzy similarity (version + authors/name).</summary>
        LikelyManual,
        /// <summary>Same locale, but the pack could not be identified.</summary>
        LocaleOnly,
        /// <summary>Contradicting data (several packs claim the same translationId).</summary>
        Conflict
    }

    /// <summary>Outcome of resolving one catalog entry against the local packs.</summary>
    public class InstallResolution
    {
        public InstallStatus Status = InstallStatus.Unknown;
        public MatchBasis Basis = MatchBasis.None;

        /// <summary>The locally installed pack this status refers to, when one was found.</summary>
        public InstalledPackInfo Pack;

        /// <summary>True for LikelyManual: "probably this very translation, installed by hand".</summary>
        public bool LikelyThis;

        /// <summary>Installed version string, when known (for update tooltips).</summary>
        public string InstalledVersion;
    }

    /// <summary>
    /// Pure decision logic that maps (catalog entry, local packs) to an install
    /// status. No I/O and no Unity/BepInEx dependencies so it can be unit tested.
    /// </summary>
    public static class InstallStatusResolver
    {
        public static InstallResolution Resolve(TranslationEntry entry, List<InstalledPackInfo> packs)
        {
            InstallResolution resolution = new InstallResolution();

            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || packs == null)
            {
                resolution.Status = InstallStatus.Unknown;
                return resolution;
            }

            // --- Exact identity matches (manifest first, then metadata). ---
            List<InstalledPackInfo> exact = new List<InstalledPackInfo>();
            foreach (InstalledPackInfo pack in packs)
            {
                if (pack == null)
                {
                    continue;
                }
                if (MatchesByTranslationId(pack, entry.id))
                {
                    exact.Add(pack);
                }
            }

            if (exact.Count > 1)
            {
                // Several folders claim to be this exact translation: refuse to guess.
                resolution.Status = InstallStatus.Unknown;
                resolution.Basis = MatchBasis.Conflict;
                return resolution;
            }

            if (exact.Count == 1)
            {
                InstalledPackInfo pack = exact[0];
                resolution.Pack = pack;

                bool byManifest = pack.Manifest != null
                    && string.Equals(pack.Manifest.translationId, entry.id, StringComparison.Ordinal);
                resolution.Basis = byManifest ? MatchBasis.Manifest : MatchBasis.MetadataTranslationId;
                resolution.InstalledVersion = byManifest
                    ? TrimToNull(pack.Manifest.version)
                    : (pack.Metadata != null ? TrimToNull(pack.Metadata.translationVersion) : null);

                string installedVersion = NormalizeVersion(resolution.InstalledVersion);
                string catalogVersion = NormalizeVersion(entry.version);
                bool versionsDiffer = installedVersion.Length > 0 && catalogVersion.Length > 0
                    && !string.Equals(installedVersion, catalogVersion, StringComparison.OrdinalIgnoreCase);

                resolution.Status = versionsDiffer ? InstallStatus.UpdateAvailable : InstallStatus.Installed;
                return resolution;
            }

            // --- No exact match: nothing of this translation is installed. ---
            resolution.Status = InstallStatus.NotInstalled;
            resolution.Basis = MatchBasis.None;
            return resolution;
        }

        /// <summary>Normalizes a version for comparison ("v1.3.1" -> "1.3.1").</summary>
        public static string NormalizeVersion(string version)
        {
            string v = TrimToNull(version) ?? "";
            if (v.Length > 1 && (v[0] == 'v' || v[0] == 'V'))
            {
                v = v.Substring(1).Trim();
            }
            return v;
        }

        private static bool MatchesByTranslationId(InstalledPackInfo pack, string id)
        {
            if (pack.Manifest != null && string.Equals(pack.Manifest.translationId, id, StringComparison.Ordinal))
            {
                return true;
            }
            return pack.Metadata != null && string.Equals(pack.Metadata.translationId, id, StringComparison.Ordinal);
        }

        /// <summary>
        /// Strong fuzzy match for manual installations: same version AND either an
        /// author overlap (the catalog author line appears in the pack's
        /// langAuthors/credits) or clearly similar names.
        /// </summary>
        private static bool IsLikelySameTranslation(TranslationEntry entry, InstalledPackInfo pack)
        {
            InstalledPackMetadata meta = pack.Metadata;
            if (meta == null)
            {
                return false;
            }

            string installedVersion = NormalizeVersion(meta.translationVersion);
            string catalogVersion = NormalizeVersion(entry.version);
            if (installedVersion.Length == 0 || catalogVersion.Length == 0)
            {
                return false;
            }
            if (!string.Equals(installedVersion, catalogVersion, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string catalogAuthors = NormalizeText(entry.author);
            string packAuthors = NormalizeText(meta.authors);
            if (catalogAuthors.Length > 0 && packAuthors.Length > 0 && packAuthors.Contains(catalogAuthors))
            {
                return true;
            }

            return NamesSimilar(entry.name, meta.languageName);
        }

        private static bool NamesSimilar(string a, string b)
        {
            string left = NormalizeText(a);
            string right = NormalizeText(b);
            if (left.Length == 0 || right.Length == 0)
            {
                return false;
            }
            return left == right || left.Contains(right) || right.Contains(left);
        }

        private static string NormalizeText(string value)
        {
            return (value ?? "").Trim().ToLowerInvariant();
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
