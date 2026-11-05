using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>Progress report of a running install (throttled by the installer).</summary>
    public sealed class InstallProgress
    {
        /// <summary>"resolving" | "downloading" | "extracting" | "installing".</summary>
        public string Stage;
        public long BytesRead;
        public long? TotalBytes;
    }

    /// <summary>
    /// Size/count limits a downloaded archive must respect. Defaults follow the
    /// project plan; tests inject smaller values to exercise the limit logic with
    /// tiny synthetic archives.
    /// </summary>
    public sealed class ZipLimits
    {
        public long MaxTotalUncompressedBytes = 4L * 1024 * 1024 * 1024; // 4 GB
        public int MaxFileCount = 20000;
        public long MaxSingleFileBytes = 2L * 1024 * 1024 * 1024;        // 2 GB

        // Zip-bomb heuristic: only applied when the total is already large, so a
        // 1 MB text file that happens to compress to 1 KB never trips it.
        public long BombThresholdBytes = 512L * 1024 * 1024;
        public int BombRatio = 100;
    }

    /// <summary>
    /// Structural validation and sandboxed extraction of downloaded language pack
    /// archives. This is NOT an antivirus: it enforces path safety (ZipSlip),
    /// executable deny-listing and size/count limits, and never executes or
    /// interprets any extracted content.
    ///
    /// Pure System.IO + System.IO.Compression only (no Unity, no BepInEx, no
    /// logging), so it can be compiled into the test project and safely run on a
    /// background thread.
    /// </summary>
    public static class SafeZipExtractor
    {
        public const int MaxEntryNameLength = 240;

        /// <summary>
        /// File extensions never extracted. A translation pack is data (json,
        /// images, fonts, audio); executables and scripts have no business in one.
        /// Lowercase, with the leading dot.
        /// </summary>
        public static readonly HashSet<string> DenyList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".dll", ".exe", ".bat", ".cmd", ".ps1", ".sh", ".jar", ".com", ".scr",
            ".vbs", ".msi", ".dylib", ".so", ".cpl", ".msp", ".sys", ".hta", ".wsf"
        };

        /// <summary>
        /// Validates a single entry name structurally. Returns the normalized name
        /// (forward slashes, no traversal) or null when the name is unsafe:
        /// empty, too long, absolute, drive-qualified, parent-relative (".."),
        /// or rooted. Does NOT check the deny list (that is archive-level policy).
        /// </summary>
        public static string ValidateEntryName(string entryName)
        {
            if (string.IsNullOrEmpty(entryName))
            {
                return null;
            }

            // Zip names use '/'; tolerate (and normalize) Windows-style '\' too.
            string normalized = entryName.Replace('\\', '/');

            if (normalized.Length > MaxEntryNameLength)
            {
                return null;
            }

            // Absolute paths and drive specifications.
            if (normalized.StartsWith("/", StringComparison.Ordinal) ||
                normalized.Contains(":") ||
                normalized.StartsWith("//", StringComparison.Ordinal))
            {
                return null;
            }

            // No ".." path segments anywhere (ZipSlip).
            string[] segments = normalized.Split('/');
            foreach (string segment in segments)
            {
                if (string.Equals(segment, "..", StringComparison.Ordinal))
                {
                    return null;
                }
            }

            return normalized;
        }

        /// <summary>
        /// Validates the whole archive from its central directory (headers only,
        /// nothing is decompressed): entry names, deny list, duplicates, per-file
        /// and total size limits, file count, and the zip-bomb ratio heuristic.
        /// Returns null when the archive is acceptable, or a human-readable
        /// description of the first problem found.
        /// </summary>
        public static string ValidateArchive(ZipArchive archive, ZipLimits limits = null)
        {
            if (archive == null)
            {
                return "archive is null";
            }
            if (limits == null)
            {
                limits = new ZipLimits();
            }

            if (archive.Entries.Count > limits.MaxFileCount)
            {
                return "archive contains more than " + limits.MaxFileCount + " files";
            }

            HashSet<string> seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            long totalUncompressed = 0;
            long totalCompressed = 0;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string name = ValidateEntryName(entry.FullName);
                if (name == null)
                {
                    return "unsafe entry name: '" + entry.FullName + "'";
                }
                if (IsDenied(name))
                {
                    return "entry '" + name + "' has a forbidden file type";
                }
                if (!seenNames.Add(name))
                {
                    return "duplicate entry name: '" + name + "'";
                }
                if (entry.Length > limits.MaxSingleFileBytes)
                {
                    return "entry '" + name + "' exceeds the single-file size limit";
                }

                totalUncompressed += entry.Length;
                totalCompressed += entry.CompressedLength;

                if (totalUncompressed > limits.MaxTotalUncompressedBytes)
                {
                    return "archive exceeds the total uncompressed size limit";
                }
            }

            if (totalUncompressed > limits.BombThresholdBytes && totalCompressed > 0 &&
                totalUncompressed / totalCompressed > limits.BombRatio)
            {
                return "archive looks like a zip bomb (compression ratio " +
                    (totalUncompressed / totalCompressed) + ":1)";
            }

            return null;
        }

        /// <summary>
        /// Extracts the archive into <paramref name="stagingDir"/>, re-checking the
        /// ACTUAL decompressed sizes while streaming (headers can lie): a file that
        /// grows past the limits aborts the extraction. On any failure or
        /// cancellation the staging directory is deleted and the exception
        /// propagates. Safe to run on a background thread.
        /// </summary>
        public static void ExtractToStaging(
            ZipArchive archive,
            string stagingDir,
            CancellationToken cancellationToken,
            ZipLimits limits = null,
            IProgress<InstallProgress> progress = null)
        {
            if (archive == null)
            {
                throw new ArgumentNullException("archive");
            }
            if (string.IsNullOrEmpty(stagingDir))
            {
                throw new ArgumentException("stagingDir is empty", "stagingDir");
            }
            if (limits == null)
            {
                limits = new ZipLimits();
            }

            string rootFullPath = Path.GetFullPath(stagingDir);
            if (!rootFullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                rootFullPath += Path.DirectorySeparatorChar;
            }

            const int ProgressReportChunk = 4194304; // 4 MB

            long totalBytes = 0;
            long totalUncompressed = 0;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!entry.FullName.EndsWith("/", StringComparison.Ordinal))
                {
                    totalUncompressed += entry.Length;
                }
            }
            long lastReported = 0;

            // Pre-create all directories upfront to avoid per-entry
            // filesystem round-trips during extraction.
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string name = ValidateEntryName(entry.FullName);
                if (name == null || name.EndsWith("/", StringComparison.Ordinal))
                {
                    continue;
                }
                string destination = Path.Combine(stagingDir, name.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
            }

            byte[] buffer = new byte[1048576]; // 1 MB

            try
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string name = ValidateEntryName(entry.FullName);
                    if (name == null)
                    {
                        throw new InvalidDataException("Unsafe entry name: '" + entry.FullName + "'");
                    }
                    if (IsDenied(name))
                    {
                        throw new InvalidDataException("Entry '" + name + "' has a forbidden file type");
                    }

                    if (name.EndsWith("/", StringComparison.Ordinal))
                    {
                        // Directory entry: create it and move on.
                        Directory.CreateDirectory(Path.Combine(stagingDir, name));
                        continue;
                    }

                    string destination = Path.Combine(stagingDir, name.Replace('/', Path.DirectorySeparatorChar));
                    string destinationFull = Path.GetFullPath(destination);

                    // Containment check: the destination must live inside the
                    // staging root (defense in depth on top of the ".." checks).
                    if (!destinationFull.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException("Entry '" + name + "' escapes the extraction directory");
                    }

                    long fileBytes = 0;
                    using (Stream input = entry.Open())
                    using (FileStream output = new FileStream(destinationFull, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, FileOptions.SequentialScan))
                    {
                        int read;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            fileBytes += read;
                            totalBytes += read;
                            if (fileBytes > limits.MaxSingleFileBytes)
                            {
                                throw new InvalidDataException("Entry '" + name + "' exceeded its declared size");
                            }
                            if (totalBytes > limits.MaxTotalUncompressedBytes)
                            {
                                throw new InvalidDataException("Archive exceeded the total size limit while extracting");
                            }
                            output.Write(buffer, 0, read);

                            if (totalBytes - lastReported >= ProgressReportChunk || totalBytes >= totalUncompressed)
                            {
                                lastReported = totalBytes;
                                ReportProgress(progress, "extracting", totalBytes, totalUncompressed > 0 ? totalUncompressed : (long?)null);
                            }
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    if (Directory.Exists(stagingDir))
                    {
                        Directory.Delete(stagingDir, true);
                    }
                }
                catch
                {
                    // Best-effort cleanup of a partial extraction; the failure that
                    // triggered it is the one worth reporting.
                }
                throw;
            }
        }

        private static void ReportProgress(IProgress<InstallProgress> progress, string stage, long bytesRead, long? totalBytes)
        {
            if (progress != null)
            {
                progress.Report(new InstallProgress { Stage = stage, BytesRead = bytesRead, TotalBytes = totalBytes });
            }
        }

        /// <summary>True when the entry's extension is on the deny list.</summary>
        public static bool IsDenied(string normalizedEntryName)
        {
            if (string.IsNullOrEmpty(normalizedEntryName))
            {
                return false;
            }
            if (normalizedEntryName.EndsWith("/", StringComparison.Ordinal))
            {
                return false;
            }

            int lastSlash = normalizedEntryName.LastIndexOf('/');
            string fileName = lastSlash >= 0 ? normalizedEntryName.Substring(lastSlash + 1) : normalizedEntryName;
            int dot = fileName.LastIndexOf('.');
            if (dot < 0)
            {
                return false;
            }
            return DenyList.Contains(fileName.Substring(dot));
        }
    }
}
