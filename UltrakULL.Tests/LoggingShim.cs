using System;

namespace UltrakULL
{
    /// <summary>
    /// Test-local stand-in for the mod's Logging facade: the compiled-in
    /// DownloadUrlResolver references it for warnings; tests only exercise the
    /// pure, non-logging paths, so the shim just records nothing.
    /// </summary>
    public static class Logging
    {
        public static void Message(string message) { }
        public static void Info(string message) { }
        public static void Warn(string message) { }
        public static void Error(string message) { }
    }
}