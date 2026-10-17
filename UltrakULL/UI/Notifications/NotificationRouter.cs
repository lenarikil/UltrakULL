using System;
using System.Reflection;
using BepInEx.Bootstrap;

namespace UltrakULL.UI.Notifications
{
    public static class NotificationRouter
    {
        private static INotifier _notifier;
        public static INotifier Current => _notifier ?? (_notifier = Create());

        private static INotifier Create()
        {
            try
            {
                if (IsNotiffyPresent())
                {
                    Logging.Message("Notification backend: Notiffy");
                    return new SafeNotifier(new NotiffyAdapter());
                }
                Logging.Message("Notification backend: internal toast");
                return new SafeNotifier(new InternalToastNotifier());
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to initialize notification backend, using internal toast notifier: " + e.Message);
                return new SafeNotifier(new InternalToastNotifier());
            }
        }

        private static bool IsNotiffyPresent()
        {
            try
            {
                if (Chainloader.PluginInfos != null)
                {
                    foreach (var plugin in Chainloader.PluginInfos)
                    {
                        string guid = plugin.Key ?? string.Empty;
                        string name = plugin.Value?.Metadata?.GUID ?? string.Empty;
                        string displayName = plugin.Value?.Metadata?.Name ?? string.Empty;
                        if (guid.IndexOf("notiffy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.IndexOf("notiffy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            displayName.IndexOf("notiffy", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                }

                // Fallback: Notiffy is loaded but registered under an unexpected GUID.
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (asm.GetType("Notiffy.API.NotificationSystem") != null)
                            return true;
                    }
                    catch
                    {
                        // Some dynamic assemblies can throw on GetType; they can't contain Notiffy anyway.
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}