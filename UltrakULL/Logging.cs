
using BepInEx.Logging;

namespace UltrakULL
{
    public static class Logging 
    {
        public static ManualLogSource UllLogger = Logger.CreateLogSource("ULL LOGGING");

        // High-frequency callers should check this before formatting per-item
        // messages. Toggle via "verboseLogging" in BepInEx/config/ultrakull/Settings.cfg.
        public static bool Verbose;

        public static void Debug(string text)
        {
            if (Verbose)
                UllLogger.LogDebug(text);
        }
        
        public static void Message(string text)
        {
            UllLogger.LogMessage(text);
        }
        
        public static void Warn(string text)
        {
            UllLogger.LogWarning(text);
        }
        
        public static void Error(string text)
        {
            UllLogger.LogError(text);
        }
        
        public static void Fatal(string text)
        {
            UllLogger.LogFatal(text);
        }
        
        public static void Info(string text)
        {
            UllLogger.LogInfo(text);
        }

        
    }
}