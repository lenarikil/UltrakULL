using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UltrakULL.json;
using UltrakULL.UI.Notifications;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Download entry point shared by both browser versions. Selection alone never
    /// downloads: the action button triggers a confirmation dialog first, and only
    /// a confirmed dialog runs the PackInstaller. While the install runs, the
    /// details view's action area turns into a progress bar with a cancel button;
    /// the outcome is reported through localized toasts. Entries whose link cannot
    /// be resolved to a direct file are opened in the external browser.
    /// </summary>
    public static class DownloadFlow
    {
        private static readonly Dictionary<string, CancellationTokenSource> _activeInstalls =
            new Dictionary<string, CancellationTokenSource>();

        public static bool IsInstalling
        {
            get { return _activeInstalls.Count > 0; }
        }

        public static bool IsInstallingEntry(string entryId)
        {
            return _activeInstalls.ContainsKey(entryId);
        }

        public static void ConfirmDownload(Canvas canvas, TranslationEntry entry)
        {
            if (entry == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(entry.id))
            {
                return;
            }
            if (IsInstallingEntry(entry.id))
            {
                // This entry is already being installed.
                return;
            }

            // The catalog entry carries the download URL; entries without one
            // cannot be downloaded through the browser.
            if (string.IsNullOrEmpty(entry.downloadUrl))
            {
                NotificationRouter.Current.Error("UltrakULL", BrowserText.DownloadFailed("no download URL"), 5f);
                return;
            }

            string name = !string.IsNullOrWhiteSpace(entry.name) ? entry.name : (entry.id ?? "translation");

            // Update vs fresh download: the confirmation text warns about the
            // overwrite only when an update is actually detected.
            BrowserView view = BrowserHost.CurrentView;
            InstallResolution resolution = view != null ? view.ResolveEntry(entry) : null;
            bool isUpdate = resolution != null && resolution.Status == InstallStatus.UpdateAvailable;

            // Overwrite of the pack currently in use: confirm, pause gameplay, and
            // temporarily switch to English so the replaced pack is never read mid-install.
            bool isOverwriteActive = resolution != null && resolution.Pack != null &&
                (resolution.Status == InstallStatus.UpdateAvailable || resolution.Status == InstallStatus.Installed) &&
                ActiveLanguageState.IsPackInUse(resolution.Pack.PackKey);

            string title = isOverwriteActive
                ? BrowserText.ConfirmOverwriteTitle()
                : BrowserText.ConfirmDownloadTitle();
            string body;
            if (isOverwriteActive)
            {
                body = BrowserText.ConfirmOverwriteBody(name);
            }
            else if (isUpdate)
            {
                body = BrowserText.ConfirmUpdateBody(name, entry.version);
            }
            else
            {
                body = BrowserText.ConfirmDownloadBody(name, entry.author);
            }

            ConfirmDialog.Show(canvas, title, body, delegate
            {
                RunInstall(entry, isOverwriteActive);
            });
        }

        private static async void RunInstall(TranslationEntry entry, bool isOverwriteActive)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id))
            {
                return;
            }
            if (IsInstallingEntry(entry.id))
            {
                return;
            }

            BrowserView view = BrowserHost.CurrentView;
            string name = !string.IsNullOrWhiteSpace(entry.name) ? entry.name : (entry.id ?? "translation");

            var cts = new CancellationTokenSource();
            _activeInstalls[entry.id] = cts;
            CancellationToken ct = cts.Token;

            view?.OnDownloadStarted(entry);

            NotificationRouter.Current.Info("UltrakULL", BrowserText.DownloadStarted(name), 3f);

            // When the pack being overwritten is the active language: pause gameplay
            // (if we are in a gameplay scene and not already paused), switch to English
            // so the replaced files are never read mid-install, then restore afterwards.
            string originalPackKey = null;
            bool paused = false;
            float previousTimeScale = 1f;
            if (isOverwriteActive)
            {
                originalPackKey = LanguageManager.CurrentLanguage != null
                    ? LanguageManager.CurrentLanguage.PackKey
                    : null;
                if (IsGameplayScene() && !IsPaused())
                {
                    previousTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                    paused = true;
                    NotificationRouter.Current.Info("UltrakULL", BrowserText.PauseGame, 3f);
                }
                if (originalPackKey != null &&
                    !string.Equals(originalPackKey, "en-GB", StringComparison.OrdinalIgnoreCase))
                {
                    LanguageManager.SetCurrentLanguage("en-GB");
                }
            }

            try
            {
                // Progress<T> posts to the Unity main thread (captured context),
                // which drives the per-entry progress bars.
                IProgress<InstallProgress> progress = view != null
                    ? new Progress<InstallProgress>(p =>
                    {
                        view.OnDownloadProgress(entry.id, p);
                        view.State.SetInstalling(entry.id, p);
                    })
                    : null;

                InstallResult result = await PackInstaller.InstallAsync(entry, progress, ct);
                HandleResult(view, entry, name, result);
            }
            finally
            {
                CancellationTokenSource finished = null;
                if (_activeInstalls.ContainsKey(entry.id))
                {
                    finished = _activeInstalls[entry.id];
                    _activeInstalls.Remove(entry.id);
                }
                if (finished != null)
                {
                    finished.Dispose();
                }
                view?.State.ClearInstalling(entry.id);
                if (paused)
                {
                    Time.timeScale = previousTimeScale;
                    NotificationRouter.Current.Info("UltrakULL", BrowserText.ResumeGame, 3f);
                }
                if (originalPackKey != null && LanguageManager.CurrentLanguage != null &&
                    !string.Equals(LanguageManager.CurrentLanguage.PackKey, originalPackKey, StringComparison.OrdinalIgnoreCase))
                {
                    LanguageManager.SetCurrentLanguage(originalPackKey);
                }
            }
        }

        private static void HandleResult(BrowserView view, TranslationEntry entry, string name, InstallResult result)
        {
            view?.OnDownloadFinished(entry != null ? entry.id : null, result);

            switch (result.Status)
            {
                case PackInstallStatus.Success:
                    NotificationRouter.Current.Success("UltrakULL",
                        result.ReplacedActivePack
                            ? BrowserText.DownloadUpdatedPack(name) + "\n" + BrowserText.DownloadActivePackWarning
                            : BrowserText.DownloadSuccess(name),
                        result.ReplacedActivePack ? 8f : 6f);
                    break;

                case PackInstallStatus.Cancelled:
                    NotificationRouter.Current.Info("UltrakULL", BrowserText.DownloadCancelled, 3f);
                    break;

                case PackInstallStatus.ExternalPage:
                    // The link could not be turned into a direct file download:
                    // open the page in the external browser, nothing is installed.
                    NotificationRouter.Current.Warning("UltrakULL", BrowserText.DownloadExternalPage, 5f);
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        Application.OpenURL(result.Message);
                    }
                    break;

                case PackInstallStatus.Unsupported:
                    // The link does not point at a downloadable file and its host
                    // is not a known storage (e.g. a video page): reject it with
                    // an error, without opening anything.
                    NotificationRouter.Current.Error("UltrakULL", BrowserText.DownloadUnsupportedHost, 6f);
                    break;

                default:
                    NotificationRouter.Current.Error("UltrakULL",
                        string.IsNullOrEmpty(result.Message) ? BrowserText.DownloadFailed("unknown error") : BrowserText.DownloadFailed(result.Message),
                        6f);
                    break;
            }
        }

        /// <summary>Cancels the running install for <paramref name="entryId"/>, if any.</summary>
        public static void CancelEntry(string entryId)
        {
            CancellationTokenSource cts;
            if (_activeInstalls.TryGetValue(entryId, out cts))
            {
                try
                {
                    cts.Cancel();
                }
                catch (System.ObjectDisposedException)
                {
                    // Already finished between the check and the cancel.
                }
            }
        }

        /// <summary>Cancels all running installs.</summary>
        public static void CancelAll()
        {
            foreach (var kv in _activeInstalls)
            {
                try { kv.Value.Cancel(); } catch (System.ObjectDisposedException) { }
            }
        }

        private static bool IsGameplayScene()
        {
            try
            {
                Scene scene = SceneManager.GetActiveScene();
                string name = scene.name;
                return !string.Equals(name, "Bootstrap", StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(name, "Intro", StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(name, "Main Menu", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPaused()
        {
            try
            {
                var instance = MonoSingleton<OptionsManager>.Instance;
                return instance != null && instance.paused;
            }
            catch
            {
                return Time.timeScale <= 0f;
            }
        }
    }
}
