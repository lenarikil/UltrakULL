using UltrakULL.Harmony_Patches;
using UltrakULL.json;
using UltrakULL.UI.Notifications;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Download entry point shared by both browser versions. Selection alone never
    /// downloads: the accent download bar triggers a confirmation dialog first, and
    /// only a confirmed dialog hands the entry over to the existing installer
    /// (InjectLanguageButton.downloadLanguageFile) which also drives the toast
    /// notifications. Refresh requests reload the catalog.
    /// </summary>
    public static class DownloadFlow
    {
        public static void Hook(BrowserState state, Canvas canvas)
        {
            if (state == null)
            {
                return;
            }

            state.RefreshRequested += delegate
            {
                // Reload is triggered by the browser view itself via StartLoad;
                // keep this hook for external callers such as Thorn buttons.
            };

            state.DownloadRequested += delegate (TranslationEntry entry)
            {
                ConfirmDownload(canvas, entry);
            };
        }

        public static void ConfirmDownload(Canvas canvas, TranslationEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            string url = CatalogService.GetDownloadUrl(entry);
            if (string.IsNullOrEmpty(url))
            {
                UltrakULL.json.Notifications notif = LanguageManager.CurrentLanguage != null ? LanguageManager.CurrentLanguage.notifications : null;
                NotificationRouter.Current.Error("UltrakULL",
                    NotificationText.Localize(notif?.download_error, NotificationText.DownloadError), 5f);
                return;
            }

            string title = "Download translation";
            string message = "Download \"" + (entry.languageName ?? entry.id) + "\"?\n\n" +
                             (string.IsNullOrEmpty(entry.teamName) ? "" : "Team: " + entry.teamName + "\n") +
                             "No files are downloaded until you confirm.";

            ConfirmDialog.Show(canvas, title, message, delegate
            {
                PerformDownload(entry, url);
            });
        }

        private static void PerformDownload(TranslationEntry entry, string url)
        {
            string tag = entry.languageTag;
            string name = entry.languageName ?? (entry.id ?? "translation");
            if (string.IsNullOrEmpty(tag))
            {
                NotificationRouter.Current.Error("UltrakULL",
                    NotificationText.Localize(null, NotificationText.DownloadError), 5f);
                return;
            }

            // The Options menu patch class owns the existing installer.
            InjectLanguageButton.downloadLanguageFile(tag, name, url);
        }
    }
}