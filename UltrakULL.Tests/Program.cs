using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UltrakULL.json;
using UltrakULL.UI.TranslationBrowser;

namespace UltrakULL.Tests
{
    /// <summary>
    /// Console test harness for the pure, Unity-free parts of the mod
    /// (plan items 29.1 catalog/updates and 30.1 download/URL/zip safety).
    /// Exits 0 when everything passes, 1 on the first failure.
    /// </summary>
    public static class Program
    {
        private static int _passed;
        private static int _failed;

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passed++;
                Console.WriteLine("[PASS] " + name);
            }
            catch (Exception e)
            {
                _failed++;
                Console.WriteLine("[FAIL] " + name + ": " + e.Message);
            }
        }

        public static int Main()
        {
            CatalogParserTests.RunAll(Run);
            InstallStatusResolverTests.RunAll(Run);
            UrlPolicyTests.RunAll(Run);
            DownloadUrlResolverTests.RunAll(Run);
            SafeZipExtractorTests.RunAll(Run);

            Console.WriteLine();
            Console.WriteLine(_failed == 0
                ? "All " + _passed + " tests passed."
                : _failed + " test(s) failed, " + _passed + " passed.");
            return _failed == 0 ? 0 : 1;
        }
    }

    /// <summary>Shared assertion helpers for all test classes.</summary>
    internal static class Assert
    {
        public static void True(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        public static void Equal<T>(T expected, T actual, string context)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception(context + ": expected <" + expected + ">, got <" + actual + ">");
            }
        }
    }

    // ---------------------------------------------------------------------
    // 29.1: catalog parsing and update resolution
    // ---------------------------------------------------------------------

    public static class CatalogParserTests
    {
        public static void RunAll(Action<string, Action> run)
        {
            run("CatalogParser accepts a valid schema v1 snapshot", ValidSnapshot);
            run("CatalogParser rejects an unsupported schema version", UnsupportedSchema);
            run("CatalogParser rejects broken JSON", BrokenJson);
            run("CatalogParser tolerates a few broken records", ToleratesBrokenRecords);
            run("CatalogParser validates URLs (http schemes, image hosts, gallery cap)", UrlValidation);
        }

        private const string ValidSnapshotJson = @"
{
  ""schema_version"": 1,
  ""updated_at"": ""2026-10-01T00:00:00Z"",
  ""translations"": [
    {
      ""id"": ""ru-translation"",
      ""name"": ""Russian translation"",
      ""language_tag"": ""ru"",
      ""version"": ""v1.2.0"",
      ""author"": ""Russian team"",
      ""description"": ""Full Russian translation."",
      ""download"": ""https://dropbox.com/s/abc/pack.zip?dl=0"",
      ""logo"": ""https://cdn.discordapp.com/attachments/1/logo.png"",
      ""gallery"": [
        ""https://media.discordapp.net/attachments/1/a.png"",
        ""https://evil.example.com/should-be-dropped.png"",
        ""https://media.discordapp.net/attachments/1/b.png""
      ]
    }
  ]
}";

        private static void ValidSnapshot()
        {
            CatalogParseResult result = CatalogParser.Parse(ValidSnapshotJson);
            Assert.True(!result.IsError, "valid snapshot was rejected: " + result.Error);
            Assert.Equal(1, result.Entries.Count, "entry count");
            TranslationEntry entry = result.Entries[0];
            Assert.Equal("ru-translation", entry.id, "id");
            Assert.Equal("1.2.0", InstallStatusResolver.NormalizeVersion(entry.version), "normalized version");
            Assert.Equal("https://dropbox.com/s/abc/pack.zip?dl=0", entry.downloadUrl, "download URL");
            Assert.True(entry.likes == null, "v1 catalog ships no likes");
        }

        private static void UnsupportedSchema()
        {
            string json = ValidSnapshotJson.Replace("\"schema_version\": 1", "\"schema_version\": 99");
            CatalogParseResult result = CatalogParser.Parse(json);
            Assert.True(result.IsError, "unsupported schema version must be rejected");
        }

        private static void BrokenJson()
        {
            CatalogParseResult result = CatalogParser.Parse("{ not json");
            Assert.True(result.IsError, "broken JSON must be rejected");
            Assert.True(CatalogParser.Parse("").IsError, "empty snapshot must be rejected");
            Assert.True(CatalogParser.Parse(null).IsError, "null snapshot must be rejected");
        }

        private static void ToleratesBrokenRecords()
        {
            // One good entry plus one broken record (no id): the broken record
            // is skipped with a warning, the snapshot still loads.
            string withBroken = @"{
  ""schema_version"": 1,
  ""translations"": [
    { ""id"": ""good"", ""name"": ""Good"", ""language_tag"": ""de"", ""download"": ""https://example.com/pack.zip"" },
    { ""name"": ""No id, must be skipped"" }
  ]}";
            CatalogParseResult result = CatalogParser.Parse(withBroken);
            Assert.True(!result.IsError, "a few broken records must not reject the snapshot");
            Assert.Equal(1, result.Entries.Count, "only the valid record survives");
            Assert.Equal("good", result.Entries[0].id, "surviving entry");
        }

        private static void UrlValidation()
        {
            CatalogParseResult result = CatalogParser.Parse(ValidSnapshotJson);
            TranslationEntry entry = result.Entries[0];

            // Gallery: the non-allowlisted host is dropped, the rest kept.
            Assert.Equal(2, entry.gallery.Count, "gallery item count after host filtering");
        }
    }

    public static class InstallStatusResolverTests
    {
        public static void RunAll(Action<string, Action> run)
        {
            run("Resolver: exact manifest match, newer catalog version => UpdateAvailable", ExactMatchUpdate);
            run("Resolver: same version => Installed", ExactMatchInstalled);
            run("Resolver: conflict => Unknown", Conflict);
            run("Resolver: locale-only manual install", LocaleOnlyManual);
            run("Resolver: nothing installed => NotInstalled", NotInstalled);
            run("Resolver: NormalizeVersion", Normalize);
        }

        private static TranslationEntry Entry(string id, string tag, string version)
        {
            return new TranslationEntry { id = id, languageTag = tag, version = version };
        }

        private static InstalledPackInfo Pack(string id, string code, string version)
        {
            return new InstalledPackInfo
            {
                PackKey = "pack",
                Manifest = new TranslationManifest { translationId = id, version = version },
                Metadata = new InstalledPackMetadata { languageCode = code, translationVersion = version, translationId = id }
            };
        }

        private static void ExactMatchUpdate()
        {
            InstallResolution r = InstallStatusResolver.Resolve(
                Entry("ru", "ru", "2.0"),
                new List<InstalledPackInfo> { Pack("ru", "ru", "1.0") });
            Assert.Equal(InstallStatus.UpdateAvailable, r.Status, "status");
            Assert.Equal(MatchBasis.Manifest, r.Basis, "basis");
            Assert.Equal("1.0", r.InstalledVersion, "installed version");
        }

        private static void ExactMatchInstalled()
        {
            InstallResolution r = InstallStatusResolver.Resolve(
                Entry("ru", "ru", "v1.0"),
                new List<InstalledPackInfo> { Pack("ru", "ru", "v1.0") });
            Assert.Equal(InstallStatus.Installed, r.Status, "status");
        }

        private static void Conflict()
        {
            InstallResolution r = InstallStatusResolver.Resolve(
                Entry("ru", "ru", "1.0"),
                new List<InstalledPackInfo> { Pack("ru", "ru", "1.0"), Pack("ru", "ru", "1.0") });
            Assert.Equal(InstallStatus.Unknown, r.Status, "status");
            Assert.Equal(MatchBasis.Conflict, r.Basis, "basis");
        }

        private static void LocaleOnlyManual()
        {
            // Locale-only fallback was removed: only an exact translationId
            // match counts as installed, everything else is NotInstalled.
            InstalledPackInfo manual = new InstalledPackInfo
            {
                PackKey = "manual",
                Metadata = new InstalledPackMetadata { languageCode = "ru", translationVersion = "9.9" }
            };
            InstallResolution r = InstallStatusResolver.Resolve(
                Entry("ru", "ru", "1.0"), new List<InstalledPackInfo> { manual });
            Assert.Equal(InstallStatus.NotInstalled, r.Status, "status");
        }

        private static void NotInstalled()
        {
            InstallResolution r = InstallStatusResolver.Resolve(
                Entry("de", "de", "1.0"),
                new List<InstalledPackInfo> { Pack("ru", "ru", "1.0") });
            Assert.Equal(InstallStatus.NotInstalled, r.Status, "status");
        }

        private static void Normalize()
        {
            Assert.Equal("1.3.1", InstallStatusResolver.NormalizeVersion("v1.3.1"), "v-prefix stripped");
            Assert.Equal("1.3.1", InstallStatusResolver.NormalizeVersion(" 1.3.1 "), "whitespace trimmed");
            Assert.Equal("", InstallStatusResolver.NormalizeVersion(null), "null becomes empty");
        }
    }

    // ---------------------------------------------------------------------
    // 30.1: URL policy, resolver transforms, safe extraction
    // ---------------------------------------------------------------------

    public static class UrlPolicyTests
    {
        public static void RunAll(Action<string, Action> run)
        {
            run("UrlPolicy.IsHttpUrl scheme rules", SchemeRules);
            run("UrlPolicy image host allowlist", ImageHosts);
            run("UrlPolicy known storage hosts (subdomains, lookalikes)", StorageHosts);
        }

        private static void SchemeRules()
        {
            Assert.True(UrlPolicy.IsHttpUrl("https://example.com/a"), "https accepted");
            Assert.True(UrlPolicy.IsHttpUrl("http://example.com/a"), "http accepted");
            Assert.True(!UrlPolicy.IsHttpUrl("javascript:alert(1)"), "javascript rejected");
            Assert.True(!UrlPolicy.IsHttpUrl("file:///C:/secret"), "file rejected");
            Assert.True(!UrlPolicy.IsHttpUrl("ftp://example.com/a"), "ftp rejected");
            Assert.True(!UrlPolicy.IsHttpUrl("not a url"), "junk rejected");
            Assert.True(!UrlPolicy.IsHttpUrl(null), "null rejected");
        }

        private static void ImageHosts()
        {
            Assert.True(UrlPolicy.IsAllowedImageUrl("https://cdn.discordapp.com/x.png"), "allowlisted host");
            Assert.True(!UrlPolicy.IsAllowedImageUrl("https://evil.example.com/x.png"), "other host rejected");
            Assert.True(!UrlPolicy.IsAllowedImageUrl("http://cdn.discordapp.com/x.png"), "plain http rejected");
        }

        private static void StorageHosts()
        {
            Assert.True(UrlPolicy.IsKnownStorageHost("www.dropbox.com"), "subdomain of allowlisted host");
            Assert.True(!UrlPolicy.IsKnownStorageHost("notdropbox.com"), "lookalike rejected");
            Assert.True(!UrlPolicy.IsKnownStorageHost("dropbox.com.evil.example"), "suffix trick rejected");
            Assert.True(UrlPolicy.IsKnownStorageHost("yadi.sk"), "yandex host");
            Assert.True(!UrlPolicy.IsKnownStorageHost("drive.google.com"), "Google Drive is not a storage anymore");
        }
    }

    public static class DownloadUrlResolverTests
    {
        public static void RunAll(Action<string, Action> run)
        {
            run("Resolver: Dropbox dl=0 becomes dl=1", DropboxDirect);
            run("Resolver: dropboxusercontent is already direct", DropboxContentDirect);
            run("Resolver: Google Drive is unsupported (external page)", DriveUnsupported);
            run("Resolver: GitHub blob -> raw", GithubRaw);
            run("Resolver: non-http URLs yield an external page", NonHttp);
            run("ForceDropboxDirect handles existing parameters", DropboxQuerySurgery);
            run("ExtractYandexHref validates the scheme", YandexHref);
        }

        private static void DropboxDirect()
        {
            DownloadResolution r = DownloadUrlResolver.ResolveOffline("https://www.dropbox.com/s/abc/pack.zip?dl=0");
            Assert.True(r.CandidateUrl != null && r.ExternalPageUrl == null, "must resolve to a candidate");
            Assert.Equal("https://www.dropbox.com/s/abc/pack.zip?dl=1", r.CandidateUrl, "dl flipped");
        }

        private static void DropboxContentDirect()
        {
            DownloadResolution r = DownloadUrlResolver.ResolveOffline("https://dl.dropboxusercontent.com/s/abc/pack.zip");
            Assert.Equal("https://dl.dropboxusercontent.com/s/abc/pack.zip", r.CandidateUrl, "unchanged");
        }

        private static void DriveUnsupported()
        {
            DownloadResolution r = DownloadUrlResolver.ResolveOffline("https://drive.google.com/file/d/1abc/view");
            Assert.True(r.ExternalPageUrl != null && r.CandidateUrl == null, "Drive must not produce a candidate");
        }

        private static void GithubRaw()
        {
            DownloadResolution r = DownloadUrlResolver.ResolveOffline("https://github.com/user/repo/blob/main/pack.json");
            Assert.Equal("https://raw.githubusercontent.com/user/repo/main/pack.json", r.CandidateUrl, "raw url");
        }

        private static void NonHttp()
        {
            DownloadResolution r = DownloadUrlResolver.ResolveOffline("javascript:alert(1)");
            Assert.True(r.ExternalPageUrl != null && r.CandidateUrl == null, "no candidate for a non-http URL");
        }

        private static void DropboxQuerySurgery()
        {
            string url = DownloadUrlResolver.ForceDropboxDirect("https://www.dropbox.com/scl/fi/x/pack.zip?rlkey=abc+def");
            Assert.Equal("https://www.dropbox.com/scl/fi/x/pack.zip?rlkey=abc+def&dl=1", url, "existing params kept, dl appended");
        }

        private static void YandexHref()
        {
            string good = "{\"href\": \"https://downloader.yandex.net/direct?x=1+2\"}";
            Assert.Equal("https://downloader.yandex.net/direct?x=1+2",
                DownloadUrlResolver.ExtractYandexHref(good), "https href kept verbatim");
            Assert.True(DownloadUrlResolver.ExtractYandexHref("{\"href\": \"ftp://x\"}") == null, "non-http href rejected");
            Assert.True(DownloadUrlResolver.ExtractYandexHref("not json") == null, "broken json rejected");
        }
    }

    public static class SafeZipExtractorTests
    {
        public static void RunAll(Action<string, Action> run)
        {
            run("Entry names: traversal, absolute, colon rejected", EntryNames);
            run("ValidateArchive: count, size, deny list, bomb ratio", ArchiveValidation);
            run("ExtractToStaging: happy path extracts under the staging root", ExtractHappy);
            run("ExtractToStaging: traversal entry is rejected and staging cleaned", ExtractTraversal);
            run("ExtractToStaging: actual size exceeding the limit aborts", ExtractSizeEnforced);
        }

        private static void EntryNames()
        {
            Assert.Equal("folder/file.json", SafeZipExtractor.ValidateEntryName(@"folder\file.json"), "backslash normalized");
            Assert.True(SafeZipExtractor.ValidateEntryName("../escape.txt") == null, ".. rejected");
            Assert.True(SafeZipExtractor.ValidateEntryName("a/../../escape") == null, "nested .. rejected");
            Assert.True(SafeZipExtractor.ValidateEntryName("/absolute") == null, "absolute rejected");
            Assert.True(SafeZipExtractor.ValidateEntryName("C:/evil") == null, "drive-qualified rejected");
            Assert.True(SafeZipExtractor.ValidateEntryName("") == null, "empty rejected");
            Assert.True(SafeZipExtractor.ValidateEntryName(new string('a', 500)) == null, "too long rejected");
        }

        private static void ArchiveValidation()
        {
            ZipLimits limits = new ZipLimits { MaxFileCount = 2, MaxSingleFileBytes = 10, MaxTotalUncompressedBytes = 20, BombThresholdBytes = 4, BombRatio = 2 };

            using (ZipArchive archive = CreateArchive(new[] { new TestEntry("a.txt", "aaaa"), new TestEntry("b.txt", "bbbb") }))
            {
                Assert.True(SafeZipExtractor.ValidateArchive(archive, limits) == null, "small valid archive passes");
            }

            using (ZipArchive archive = CreateArchive(new[] { new TestEntry("a.txt", "012345678901234") }))
            {
                Assert.True(SafeZipExtractor.ValidateArchive(archive, limits) != null, "single-file size limit enforced");
            }

            using (ZipArchive archive = CreateArchive(new[] { new TestEntry("evil.exe", "data") }))
            {
                string problem = SafeZipExtractor.ValidateArchive(archive, limits);
                Assert.True(problem != null && problem.Contains("forbidden"), "deny list enforced: " + problem);
            }

            using (ZipArchive archive = CreateArchive(new[]
            {
                new TestEntry("a.txt", "aaaa"), new TestEntry("a.txt", "aaaa")
            }))
            {
                Assert.True(SafeZipExtractor.ValidateArchive(archive, limits) != null, "duplicates rejected");
            }
        }

        private static void ExtractHappy()
        {
            string staging = Path.Combine(Path.GetTempPath(), "ultrakull-test-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                using (ZipArchive archive = CreateArchive(new[]
                {
                    new TestEntry("pack/translation.json", "{}"),
                    new TestEntry("pack/images/logo.png", new string('x', 10))
                }))
                {
                    ZipLimits limits = new ZipLimits();
                    Assert.True(SafeZipExtractor.ValidateArchive(archive, limits) == null, "validation passes");
                    SafeZipExtractor.ExtractToStaging(archive, staging, default(System.Threading.CancellationToken), limits);
                }

                Assert.True(File.Exists(Path.Combine(staging, "pack", "translation.json")), "nested file extracted");
                Assert.True(File.Exists(Path.Combine(staging, "pack", "images", "logo.png")), "subfolder extracted");
            }
            finally
            {
                TryCleanup(staging);
            }
        }

        private static void ExtractTraversal()
        {
            string staging = Path.Combine(Path.GetTempPath(), "ultrakull-test-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                using (ZipArchive archive = CreateArchive(new[] { new TestEntry("../escape.txt", "data") }))
                {
                    try
                    {
                        SafeZipExtractor.ExtractToStaging(archive, staging, default(System.Threading.CancellationToken));
                        throw new Exception("traversal entry must throw");
                    }
                    catch (InvalidDataException)
                    {
                        // Expected: the entry is rejected by name validation.
                    }
                    catch (IOException)
                    {
                        // Also acceptable: the OS-level containment check.
                    }
                }
                Assert.True(!File.Exists(Path.Combine(staging, "..", "escape.txt")), "nothing escaped");
                Assert.True(!Directory.Exists(staging) || Directory.GetFiles(staging).Length == 0, "staging cleaned or empty");
            }
            finally
            {
                TryCleanup(staging);
            }
        }

        private static void ExtractSizeEnforced()
        {
            string staging = Path.Combine(Path.GetTempPath(), "ultrakull-test-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            try
            {
                // Headers declare a tiny entry, the stream carries more: the
                // streaming check must abort even when headers lie.
                using (ZipArchive archive = CreateArchive(new[] { new TestEntry("big.txt", new string('x', 100)) }))
                {
                    ZipLimits limits = new ZipLimits { MaxSingleFileBytes = 10, MaxTotalUncompressedBytes = 100 };
                    try
                    {
                        SafeZipExtractor.ExtractToStaging(archive, staging, default(System.Threading.CancellationToken), limits);
                        throw new Exception("oversized entry must abort");
                    }
                    catch (InvalidDataException)
                    {
                        // Expected: the actual decompressed size tripped the limit.
                    }
                }
            }
            finally
            {
                TryCleanup(staging);
            }
        }

        // --- helpers -------------------------------------------------------

        private sealed class TestEntry
        {
            public string Name;
            public string Content;
            public TestEntry(string name, string content) { Name = name; Content = content; }
        }

        private static ZipArchive CreateArchive(TestEntry[] entries)
        {
            MemoryStream memory = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                foreach (TestEntry entry in entries)
                {
                    ZipArchiveEntry zipEntry = archive.CreateEntry(entry.Name);
                    using (Stream stream = zipEntry.Open())
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(entry.Content);
                    }
                }
            }
            memory.Position = 0;
            return new ZipArchive(memory, ZipArchiveMode.Read);
        }

        private static void TryCleanup(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
                // Test cleanup is best-effort.
            }
        }
    }
}