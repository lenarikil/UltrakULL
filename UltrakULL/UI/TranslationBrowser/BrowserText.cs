using System;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Localizable strings of the translation browser. English fallbacks live
    /// here as constants; language files override them through the "browser"
    /// section (see BrowserStrings), which the missing-key sync fills into
    /// installed packs automatically. Pattern mirrors NotificationText.
    /// </summary>
    public static class BrowserText
    {
        // -------------------------------------------------------------------
        // English defaults
        // -------------------------------------------------------------------

        public const string DefaultTitle = "Translation browser";
        public const string DefaultSubtitle = "Preview and install translations.";
        public const string DefaultTabOnline = "Online";
        public const string DefaultTabInstalled = "Installed";
        public const string DefaultTabSettings = "Settings";
        public const string DefaultSearchPlaceholder = "Search name, language or author";
        public const string DefaultSortPrefix = "Sort: ";
        public const string DefaultSortPopularity = "Popularity";
        public const string DefaultSortUpdated = "Updated";
        public const string DefaultSortLanguage = "Language";
        public const string DefaultFilterLanguagePrefix = "Language: ";
        public const string DefaultFilterLanguageAll = "All";
        public const string DefaultCountLabel = "{count} translations";
        public const string DefaultStatusLoading = "Loading the catalog...";
        public const string DefaultStatusError = "Error: {error}";
        public const string DefaultStatusOffline = "Offline - showing the cached catalog (updated {time})";
        public const string DefaultStatusUpdated = "Catalog updated {time}";
        public const string DefaultStatusEmptyCatalog = "The catalog is empty.";
        public const string DefaultStatusNoMatch = "Nothing matches your search.";
        public const string DefaultStatusNotLoaded = "The catalog is not loaded yet (no network and no saved copy).";
        public const string DefaultUpdatedAgoNow = "just now";
        public const string DefaultUpdatedAgoMinutes = "{count} min ago";
        public const string DefaultUpdatedAgoHours = "{count} h ago";
        public const string DefaultUpdatedAgoDays = "{count} d ago";
        public const string DefaultInstallStatusInstalled = "Installed";
        public const string DefaultInstallStatusUpdate = "Update";
        public const string DefaultInstallStatusManual = "Language installed";
        public const string DefaultInstallStatusNotInstalled = "Not installed";
        public const string DefaultInstallStatusUnknown = "Unknown";
        public const string DefaultBasisManifest = "exact match: install manifest";
        public const string DefaultBasisMetadata = "exact match: pack metadata";
        public const string DefaultBasisLikely = "looks like this translation, installed manually";
        public const string DefaultBasisLocale = "a language of this locale is installed, the pack was not identified";
        public const string DefaultBasisConflict = "several installed packs claim to be this translation";
        public const string DefaultActionDownload = "Download";
        public const string DefaultActionReadyToInstall = "READY TO INSTALL";
        public const string DefaultActionUpdate = "Update";
        public const string DefaultActionUse = "Use";
        public const string DefaultActionUsed = "Used";
        public const string DefaultActionReinstall = "Reinstall";
        public const string DefaultActionConfirm = "Confirm";
        public const string DefaultActionOpenSource = "Open original post";
        public const string DefaultActionCancel = "Cancel";
        public const string DefaultActionOpenLink = "Open link";
        public const string DefaultConfirmOpenLinkTitle = "Open link";
        public const string DefaultConfirmOpenLinkBody = "Open \"{url}\" in your browser?";
        public const string DefaultDownloadProgressOf = "{percent}%";
        public const string DefaultDownloadProgressBytes = "";
        public const string DefaultDownloadStarted = "Downloading \"{name}\"...";
        public const string DefaultDownloadSuccess = "\"{name}\" has been installed. Select it in the Installed tab.";
        public const string DefaultDownloadUpdatedPack = "\"{name}\" has been updated.";
        public const string DefaultDownloadFailed = "Download failed: {error}";
        public const string DefaultDownloadInvalid = "The downloaded file is not a valid translation pack.";
        public const string DefaultDownloadExternalPage = "This translation is hosted on an external page. Opening it in your browser...";
        public const string DefaultDownloadUnsupportedHost = "This link does not point to a downloadable translation pack (unsupported source).";
        public const string DefaultDownloadCancelled = "Download cancelled.";
        public const string DefaultDownloadActivePackWarning = "The active language pack was replaced. Changes apply after switching languages or reloading the level.";
        public const string DefaultConfirmDownloadTitle = "Download translation";
        public const string DefaultConfirmDownloadBody = "Download \"{name}\" by {authors}?\n\nNothing is downloaded until you confirm.";
        public const string DefaultConfirmUpdateBody = "Update \"{name}\" to {version}?\n\nThe installed pack will be replaced.";
        public const string DefaultConfirmOverwriteTitle = "Overwrite installed translation";
        public const string DefaultConfirmOverwriteBody = "Overwrite \"{name}\"?\n\nThe installed pack will be replaced. If this language is in use, the game will switch back to English and restore it after the install.";
        public const string DefaultPauseGame = "Game paused during install";
        public const string DefaultResumeGame = "Game resumed";
        public const string DefaultInstallStageResolving = "Resolving link...";
        public const string DefaultInstallStageDownloading = "Downloading...";
        public const string DefaultInstallStageExtracting = "Extracting...";
        public const string DefaultInstallStageInstalling = "Installing...";
        public const string DefaultGalleryEmpty = "No screenshots provided.";
        public const string DefaultNoDescription = "No description provided.";
        public const string DefaultNoAuthors = "Authors not listed.";
        public const string DefaultVersionInstalled = "Installed: v{version}";
        public const string DefaultVersionAvailable = "Available: v{version}";
        public const string DefaultVersionUpdateFromTo = "v{installed} -> v{available}";
        public const string DefaultFeatureVoiceOver = "Voice-over";
        public const string DefaultFeatureAdditionalVoiceOver = "Additional voice-over";
        public const string DefaultFeatureReplacementTextures = "Replacement textures";
        public const string DefaultFeatureCustomFonts = "Custom fonts";
        public const string DefaultFeatureDescVoiceOver = "Replaces in-game voice lines with translated audio.";
        public const string DefaultFeatureDescAdditionalVoiceOver = "Adds dubbed audio for optional, non-essential voiced lines.";
        public const string DefaultFeatureDescReplacementTextures = "Replaces textures and graphical assets in the game.";
        public const string DefaultFeatureDescCustomFonts = "Provides custom or adapted fonts for the language.";
        // Install-status legend descriptions: kept for the card status tooltips
        // (the legend panel itself was replaced by the product tour).
        public const string DefaultLegendStatusInstalled = "this exact translation is installed.";
        public const string DefaultLegendStatusUpdate = "installed, a newer version is available.";
        public const string DefaultLegendStatusManual = "the language is installed, but not through the browser (or not identified).";
        public const string DefaultLegendStatusNotInstalled = "available online, not installed.";
        public const string DefaultLegendStatusUnknown = "could not be determined.";
        public const string DefaultInstalledTitle = "Installed translations";
        public const string DefaultInstalledEmpty = "No installed translations.";
        public const string DefaultInstalledRescan = "Rescan the translations folder for newly added packs";
        public const string DefaultSettingsTitle = "Settings";
        public const string DefaultSettingsOpenFolder = "Open translations folder";
        public const string DefaultDetailsEmpty = "Select a translation from the list to preview its details.";
        public const string DefaultBuiltinEnglishDescription = "Original game text. No translation patch required.";
        public const string DefaultAuthorsShowAll = "Show all authors ({count})";
        public const string DefaultAuthorsSection = "Authors";
        public const string DefaultDescriptionSection = "Description";
        public const string DefaultSocialLinksSection = "Social links";
        public const string DefaultTooltipRefresh = "Reload the catalog from the network";
        public const string DefaultToggleGuide = "Guide";
        public const string DefaultTooltipTour = "Restart the interactive interface tour";

        // Product tour (item 13): welcome/restart dialogs, card controls and
        // the seven step texts.
        public const string DefaultTourWelcomeTitle = "Welcome to the translation browser";
        public const string DefaultTourWelcomeBody = "A quick interactive tour will show you where to find translations, how to download them and how to switch languages. Takes about a minute.";
        public const string DefaultTourWelcomeStart = "Start";
        public const string DefaultTourWelcomeNotNow = "Not now";
        public const string DefaultTourRestartTitle = "Restart the interface tour?";
        public const string DefaultTourRestartBody = "The tour will walk through the browser interface again, step by step.";
        public const string DefaultTourNext = "Next";
        public const string DefaultTourBack = "Back";
        public const string DefaultTourSkip = "Skip";
        public const string DefaultTourFinish = "Finish";
        public const string DefaultTourStepCatalogTitle = "Online catalog";
        public const string DefaultTourStepCatalogBody = "The community catalog of translations. Search by name, check the summary and pick a language.";
        public const string DefaultTourStepDetailsTitle = "Details";
        public const string DefaultTourStepDetailsBody = "The preview shows the authors, version, capabilities and description. Nothing is downloaded yet: this is only a preview of the catalog entry.";
        public const string DefaultTourStepDownloadTitle = "Download";
        public const string DefaultTourStepDownloadBody = "The download button is the only trigger that starts a download. The progress bar shows the process; a download can be cancelled.";
        public const string DefaultTourStepInstalledTitle = "Installed translations";
        public const string DefaultTourStepInstalledBody = "Everything you have downloaded lives here, including manually installed languages the browser could identify.";
        public const string DefaultTourStepSwitchTitle = "Switch language";
        public const string DefaultTourStepSwitchBody = "Selecting an installed translation and confirming switches the game language. The currently used translation is marked.";
        public const string DefaultTourStepUpdatesTitle = "Updates";
        public const string DefaultTourStepUpdatesBody = "When a newer version of an installed translation appears in the catalog, the browser shows an update badge and can update it.";
        public const string DefaultTourStepIconsTitle = "List icons";
        public const string DefaultTourStepIconsBody = "Each card shows four capability icons (dubbing, voices, textures, fonts), a like count, and an install-status badge. Green means installed, red means an update is available.";
        public const string DefaultTourStepReadyTitle = "Ready to install";
        public const string DefaultTourStepReadyBody = "If the installation fails or you have deleted the translation, click the download button again to download and install it once more. Cached archives are reused — nothing is downloaded twice.";
        public const string DefaultTourStepRepeatTitle = "That's it!";
        public const string DefaultTourStepRepeatBody = "Press Guide at any time to restart this tour. Enjoy the game in your language!";
        public const string DefaultTourStepSettingsTitle = "Settings";
        public const string DefaultTourStepSettingsBody = "The Settings tab holds the mod's own options: live texture and audio replacement toggles, the folder with custom fonts, and the folder with installed translation packs.";
        public const string DefaultImageLoadFailed = "The image failed to load.";

        // -------------------------------------------------------------------
        // Localized accessors
        // -------------------------------------------------------------------

        private static BrowserStrings Strings
        {
            get
            {
                JsonFormat current = LanguageManager.CurrentLanguage;
                return current != null ? current.browser : null;
            }
        }

        public static string Title => Localize(Strings?.browser_title, DefaultTitle);
        public static string Subtitle => Localize(Strings?.browser_subtitle, DefaultSubtitle);
        public static string TabOnline => Localize(Strings?.tab_online, DefaultTabOnline);
        public static string TabInstalled => Localize(Strings?.tab_installed, DefaultTabInstalled);
        public static string TabSettings => Localize(Strings?.tab_settings, DefaultTabSettings);
        public static string SearchPlaceholder => Localize(Strings?.search_placeholder, DefaultSearchPlaceholder);
        public static string SortPrefix => Localize(Strings?.sort_prefix, DefaultSortPrefix);
        public static string SortPopularity => Localize(Strings?.sort_popularity, DefaultSortPopularity);
        public static string SortUpdated => Localize(Strings?.sort_updated, DefaultSortUpdated);
        public static string SortLanguage => Localize(Strings?.sort_language, DefaultSortLanguage);
        public static string FilterLanguagePrefix => Localize(Strings?.filter_language_prefix, DefaultFilterLanguagePrefix);
        public static string FilterLanguageAll => Localize(Strings?.filter_language_all, DefaultFilterLanguageAll);
        public static string StatusLoading => Localize(Strings?.status_loading, DefaultStatusLoading);
        public static string StatusEmptyCatalog => Localize(Strings?.status_empty_catalog, DefaultStatusEmptyCatalog);
        public static string StatusNoMatch => Localize(Strings?.status_no_match, DefaultStatusNoMatch);
        public static string StatusNotLoaded => Localize(Strings?.status_not_loaded, DefaultStatusNotLoaded);
        public static string InstallStatusInstalled => Localize(Strings?.install_status_installed, DefaultInstallStatusInstalled);
        public static string InstallStatusUpdate => Localize(Strings?.install_status_update, DefaultInstallStatusUpdate);
        public static string InstallStatusManual => Localize(Strings?.install_status_manual, DefaultInstallStatusManual);
        public static string InstallStatusNotInstalled => Localize(Strings?.install_status_not_installed, DefaultInstallStatusNotInstalled);
        public static string InstallStatusUnknown => Localize(Strings?.install_status_unknown, DefaultInstallStatusUnknown);
        public static string ActionDownload => Localize(Strings?.action_download, DefaultActionDownload);
        public static string ActionReadyToInstall => Localize(null, DefaultActionReadyToInstall);
        public static string ActionUpdate => Localize(Strings?.action_update, DefaultActionUpdate);
        public static string ActionUse => Localize(Strings?.action_use, DefaultActionUse);
        public static string ActionUsed => Localize(Strings?.action_used, DefaultActionUsed);
        public static string ActionReinstall => Localize(null, DefaultActionReinstall);
        public static string ActionConfirm => Localize(null, DefaultActionConfirm);
        public static string ActionOpenSource => Localize(Strings?.action_open_source, DefaultActionOpenSource);
        public static string ActionOpenLink => Localize(null, DefaultActionOpenLink);
        public static string ConfirmOpenLinkTitle() => Localize(null, DefaultConfirmOpenLinkTitle);
        public static string ConfirmOpenLinkBody(string url)
        {
            // Highlight the link text with the dialog frame color so the URL
            // stands out from the surrounding question text (TMP rich text).
            string hex = ColorUtility.ToHtmlStringRGB(StylePalette.AccentRed);
            string coloredUrl = "<color=#" + hex + ">" + (url ?? "") + "</color>";
            return Localize(null, DefaultConfirmOpenLinkBody, "url", coloredUrl);
        }
        public static string ActionCancel => Localize(Strings?.action_cancel, DefaultActionCancel);
        public static string DownloadInvalid => Localize(Strings?.download_invalid, DefaultDownloadInvalid);
        public static string DownloadCancelled => Localize(Strings?.download_cancelled, DefaultDownloadCancelled);
        public static string DownloadExternalPage => Localize(Strings?.download_external_page, DefaultDownloadExternalPage);
        public static string DownloadUnsupportedHost => Localize(Strings?.download_unsupported_host, DefaultDownloadUnsupportedHost);
        public static string DownloadActivePackWarning => Localize(Strings?.download_active_pack_warning, DefaultDownloadActivePackWarning);
        public static string ConfirmOverwriteTitle() => Localize(null, DefaultConfirmOverwriteTitle);
        public static string ConfirmOverwriteBody(string name) => Localize(null, DefaultConfirmOverwriteBody, "name", name ?? "");
        public static string PauseGame => Localize(null, DefaultPauseGame);
        public static string ResumeGame => Localize(null, DefaultResumeGame);
        public static string InstallStageResolving => Localize(null, DefaultInstallStageResolving);
        public static string InstallStageDownloading => Localize(null, DefaultInstallStageDownloading);
        public static string InstallStageExtracting => Localize(null, DefaultInstallStageExtracting);
        public static string InstallStageInstalling => Localize(null, DefaultInstallStageInstalling);
        public static string GalleryEmpty => Localize(Strings?.gallery_empty, DefaultGalleryEmpty);
        public static string NoDescription => Localize(Strings?.no_description, DefaultNoDescription);
        public static string NoAuthors => Localize(Strings?.no_authors, DefaultNoAuthors);
        public static string FeatureVoiceOver => Localize(Strings?.feature_voice_over, DefaultFeatureVoiceOver);
        public static string FeatureAdditionalVoiceOver => Localize(Strings?.feature_additional_voice_over, DefaultFeatureAdditionalVoiceOver);
        public static string FeatureReplacementTextures => Localize(Strings?.feature_replacement_textures, DefaultFeatureReplacementTextures);
        public static string FeatureCustomFonts => Localize(Strings?.feature_custom_fonts, DefaultFeatureCustomFonts);
        public static string FeatureDescVoiceOver => Localize(Strings?.feature_desc_voice_over, DefaultFeatureDescVoiceOver);
        public static string FeatureDescAdditionalVoiceOver => Localize(Strings?.feature_desc_additional_voice_over, DefaultFeatureDescAdditionalVoiceOver);
        public static string FeatureDescReplacementTextures => Localize(Strings?.feature_desc_replacement_textures, DefaultFeatureDescReplacementTextures);
        public static string FeatureDescCustomFonts => Localize(Strings?.feature_desc_custom_fonts, DefaultFeatureDescCustomFonts);
        public static string InstalledTitle => Localize(Strings?.installed_title, DefaultInstalledTitle);
        public static string InstalledEmpty => Localize(Strings?.installed_empty, DefaultInstalledEmpty);
        public static string InstalledRescan => Localize(Strings?.installed_rescan, DefaultInstalledRescan);
        public static string SettingsTitle => Localize(Strings?.settings_title, DefaultSettingsTitle);
        public static string SettingsOpenFolder => Localize(Strings?.settings_open_folder, DefaultSettingsOpenFolder);
        public static string DetailsEmpty => Localize(Strings?.details_empty, DefaultDetailsEmpty);
        public static string BuiltinEnglishDescription => Localize(Strings?.builtin_english_description, DefaultBuiltinEnglishDescription);
        public static string AuthorsSection => Localize(Strings?.authors_section, DefaultAuthorsSection);
        public static string DescriptionSection => Localize(Strings?.description_section, DefaultDescriptionSection);
        public static string SocialLinksSection => Localize(Strings?.social_links_section, DefaultSocialLinksSection);
        public static string TooltipRefresh => Localize(null, DefaultTooltipRefresh);
        public static string ToggleGuide => Localize(null, DefaultToggleGuide);
        public static string TooltipTour => Localize(null, DefaultTooltipTour);
        public static string TourWelcomeTitle => Localize(Strings?.tour_welcome_title, DefaultTourWelcomeTitle);
        public static string TourWelcomeBody => Localize(Strings?.tour_welcome_body, DefaultTourWelcomeBody);
        public static string TourWelcomeStart => Localize(Strings?.tour_welcome_start, DefaultTourWelcomeStart);
        public static string TourWelcomeNotNow => Localize(Strings?.tour_welcome_not_now, DefaultTourWelcomeNotNow);
        public static string TourRestartTitle => Localize(Strings?.tour_restart_title, DefaultTourRestartTitle);
        public static string TourRestartBody => Localize(Strings?.tour_restart_body, DefaultTourRestartBody);
        public static string TourNext => Localize(Strings?.tour_next, DefaultTourNext);
        public static string TourBack => Localize(Strings?.tour_back, DefaultTourBack);
        public static string TourSkip => Localize(Strings?.tour_skip, DefaultTourSkip);
        public static string TourFinish => Localize(Strings?.tour_finish, DefaultTourFinish);
        public static string TourStepCatalogTitle => Localize(Strings?.tour_step_catalog_title, DefaultTourStepCatalogTitle);
        public static string TourStepCatalogBody => Localize(Strings?.tour_step_catalog_body, DefaultTourStepCatalogBody);
        public static string TourStepDetailsTitle => Localize(Strings?.tour_step_details_title, DefaultTourStepDetailsTitle);
        public static string TourStepDetailsBody => Localize(Strings?.tour_step_details_body, DefaultTourStepDetailsBody);
        public static string TourStepDownloadTitle => Localize(Strings?.tour_step_download_title, DefaultTourStepDownloadTitle);
        public static string TourStepDownloadBody => Localize(Strings?.tour_step_download_body, DefaultTourStepDownloadBody);
        public static string TourStepInstalledTitle => Localize(Strings?.tour_step_installed_title, DefaultTourStepInstalledTitle);
        public static string TourStepInstalledBody => Localize(Strings?.tour_step_installed_body, DefaultTourStepInstalledBody);
        public static string TourStepSwitchTitle => Localize(Strings?.tour_step_switch_title, DefaultTourStepSwitchTitle);
        public static string TourStepSwitchBody => Localize(Strings?.tour_step_switch_body, DefaultTourStepSwitchBody);
        public static string TourStepUpdatesTitle => Localize(Strings?.tour_step_updates_title, DefaultTourStepUpdatesTitle);
        public static string TourStepUpdatesBody => Localize(Strings?.tour_step_updates_body, DefaultTourStepUpdatesBody);
        public static string TourStepIconsTitle => Localize(Strings?.tour_step_icons_title, DefaultTourStepIconsTitle);
        public static string TourStepIconsBody => Localize(Strings?.tour_step_icons_body, DefaultTourStepIconsBody);
        public static string TourStepReadyTitle => Localize(Strings?.tour_step_ready_title, DefaultTourStepReadyTitle);
        public static string TourStepReadyBody => Localize(Strings?.tour_step_ready_body, DefaultTourStepReadyBody);
        public static string TourStepRepeatTitle => Localize(Strings?.tour_step_repeat_title, DefaultTourStepRepeatTitle);
        public static string TourStepRepeatBody => Localize(Strings?.tour_step_repeat_body, DefaultTourStepRepeatBody);
        public static string TourStepSettingsTitle => Localize(Strings?.tour_step_settings_title, DefaultTourStepSettingsTitle);
        public static string TourStepSettingsBody => Localize(Strings?.tour_step_settings_body, DefaultTourStepSettingsBody);
        public static string ImageLoadFailed => Localize(Strings?.image_load_failed, DefaultImageLoadFailed);

        /// <summary>
        /// Resolves a localized browser string: empty/null falls back to the
        /// built-in English text, then {placeholders} are substituted.
        /// Placeholders are passed as key/value pairs: Localize(s, def, "lang", name).
        /// </summary>
        public static string Localize(string localized, string englishDefault, params string[] keysAndValues)
        {
            string result = string.IsNullOrEmpty(localized) ? englishDefault : localized;
            if (keysAndValues != null)
            {
                for (int i = 0; i + 1 < keysAndValues.Length; i += 2)
                {
                    result = result.Replace("{" + keysAndValues[i] + "}", keysAndValues[i + 1] ?? string.Empty);
                }
            }
            return result;
        }

        /// <summary>
        /// Localized "{count} translations" footer label for the given entry count.
        /// </summary>
        public static string Count(int count)
        {
            return Localize(Strings?.count_label, DefaultCountLabel, "count", count.ToString());
        }

        public static string StatusError(string error)
        {
            return Localize(Strings?.status_error, DefaultStatusError, "error", error ?? "");
        }

        public static string StatusOffline(string relativeTime)
        {
            return Localize(Strings?.status_offline, DefaultStatusOffline, "time", relativeTime ?? "");
        }

        public static string StatusUpdated(string relativeTime)
        {
            return Localize(Strings?.status_updated, DefaultStatusUpdated, "time", relativeTime ?? "");
        }

        /// <summary>
        /// Localized label of an install stage key ("resolving", "downloading",
        /// "extracting", "installing"); shared by the details progress bar and
        /// the card progress labels.
        /// </summary>
        public static string InstallStageLabel(string stage)
        {
            switch (stage)
            {
                case "resolving":
                    return InstallStageResolving;
                case "downloading":
                    return InstallStageDownloading;
                case "extracting":
                    return InstallStageExtracting;
                case "installing":
                    return InstallStageInstalling;
                default:
                    return string.IsNullOrEmpty(stage) ? InstallStageDownloading : stage;
            }
        }

        /// <summary>
        /// Localized download progress. A known total yields an integer percent
        /// clamped to 0-100 (item 16); an unknown total yields an empty string
        /// so only the stage label remains. The {done}/{total} placeholders are
        /// still substituted for language packs that localize the old format.
        /// </summary>
        public static string DownloadProgress(long doneBytes, long? totalBytes)
        {
            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                int percent = (int)Math.Round((double)doneBytes / totalBytes.Value * 100.0);
                percent = Math.Max(0, Math.Min(100, percent));
                return Localize(Strings?.download_progress_of, DefaultDownloadProgressOf,
                    "percent", percent.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "done", FormatBytes(doneBytes),
                    "total", FormatBytes(totalBytes.Value));
            }
            return Localize(Strings?.download_progress_bytes, DefaultDownloadProgressBytes, "done", FormatBytes(doneBytes));
        }

        public static string DownloadStarted(string name)
        {
            return Localize(Strings?.download_started, DefaultDownloadStarted, "name", name ?? "");
        }

        public static string DownloadSuccess(string name)
        {
            return Localize(Strings?.download_success, DefaultDownloadSuccess, "name", name ?? "");
        }

        public static string DownloadUpdatedPack(string name)
        {
            return Localize(Strings?.download_updated_pack, DefaultDownloadUpdatedPack, "name", name ?? "");
        }

        public static string DownloadFailed(string error)
        {
            return Localize(Strings?.download_failed, DefaultDownloadFailed, "error", error ?? "");
        }

        public static string ConfirmDownloadTitle()
        {
            return Localize(Strings?.confirm_download_title, DefaultConfirmDownloadTitle);
        }

        public static string ConfirmDownloadBody(string name, string authors)
        {
            return Localize(Strings?.confirm_download_body, DefaultConfirmDownloadBody,
                "name", name ?? "", "authors", authors ?? "");
        }

        public static string ConfirmUpdateBody(string name, string version)
        {
            return Localize(Strings?.confirm_update_body, DefaultConfirmUpdateBody,
                "name", name ?? "", "version", version ?? "");
        }

        public static string VersionInstalled(string version)
        {
            return Localize(Strings?.version_installed, DefaultVersionInstalled, "version", version ?? "");
        }

        public static string VersionAvailable(string version)
        {
            return Localize(Strings?.version_available, DefaultVersionAvailable, "version", version ?? "");
        }

        public static string VersionUpdateFromTo(string installed, string available)
        {
            return Localize(Strings?.version_update_from_to, DefaultVersionUpdateFromTo,
                "installed", installed ?? "", "available", available ?? "");
        }

        public static string AuthorsShowAll(int count)
        {
            return Localize(Strings?.authors_show_all, DefaultAuthorsShowAll, "count", count.ToString());
        }

        /// <summary>
        /// Human-readable localized byte size (progress labels). Uses the invariant
        /// culture so the value never depends on the OS locale.
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            const long kb = 1024;
            const long mb = kb * 1024;
            const long gb = mb * 1024;

            if (bytes >= gb)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.0} GB", (double)bytes / gb);
            }
            if (bytes >= mb)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.0} MB", (double)bytes / mb);
            }
            if (bytes >= kb)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.0} KB", (double)bytes / kb);
            }
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} B", bytes);
        }

        /// <summary>
        /// Coarse localized relative time ("just now", "5 min ago", "3 h ago",
        /// "2 d ago") for catalog/cache freshness labels.
        /// </summary>
        public static string RelativeTime(DateTime utc)
        {
            TimeSpan delta = DateTime.UtcNow - utc;
            if (delta.TotalSeconds < 90)
            {
                return Localize(Strings?.updated_ago_now, DefaultUpdatedAgoNow);
            }
            if (delta.TotalMinutes < 60)
            {
                return Localize(Strings?.updated_ago_minutes, DefaultUpdatedAgoMinutes, "count", ((int)delta.TotalMinutes).ToString());
            }
            if (delta.TotalHours < 24)
            {
                return Localize(Strings?.updated_ago_hours, DefaultUpdatedAgoHours, "count", ((int)delta.TotalHours).ToString());
            }
            return Localize(Strings?.updated_ago_days, DefaultUpdatedAgoDays, "count", ((int)delta.TotalDays).ToString());
        }

        /// <summary>Localized legend row label for a capability status.</summary>
        public static string LegendStatusLabel(InstallStatus status)
        {
            switch (status)
            {
                case InstallStatus.Installed:
                    return Localize(Strings?.legend_status_installed, DefaultLegendStatusInstalled);
                case InstallStatus.UpdateAvailable:
                    return Localize(Strings?.legend_status_update, DefaultLegendStatusUpdate);
                case InstallStatus.LanguageInstalledManual:
                    return Localize(Strings?.legend_status_manual, DefaultLegendStatusManual);
                case InstallStatus.NotInstalled:
                    return Localize(Strings?.legend_status_not_installed, DefaultLegendStatusNotInstalled);
                default:
                    return Localize(Strings?.legend_status_unknown, DefaultLegendStatusUnknown);
            }
        }

        /// <summary>Localized short badge label for an install status.</summary>
        public static string InstallStatusLabel(InstallStatus status)
        {
            switch (status)
            {
                case InstallStatus.Installed:
                    return InstallStatusInstalled;
                case InstallStatus.UpdateAvailable:
                    return InstallStatusUpdate;
                case InstallStatus.LanguageInstalledManual:
                    return InstallStatusManual;
                case InstallStatus.NotInstalled:
                    return InstallStatusNotInstalled;
                default:
                    return InstallStatusUnknown;
            }
        }

        /// <summary>Localized tooltip basis line for an install resolution.</summary>
        public static string BasisLabel(MatchBasis basis)
        {
            switch (basis)
            {
                case MatchBasis.Manifest:
                    return Localize(Strings?.basis_manifest, DefaultBasisManifest);
                case MatchBasis.MetadataTranslationId:
                    return Localize(Strings?.basis_metadata, DefaultBasisMetadata);
                case MatchBasis.LikelyManual:
                    return Localize(Strings?.basis_likely, DefaultBasisLikely);
                case MatchBasis.LocaleOnly:
                    return Localize(Strings?.basis_locale, DefaultBasisLocale);
                case MatchBasis.Conflict:
                    return Localize(Strings?.basis_conflict, DefaultBasisConflict);
                default:
                    return "";
            }
        }

        /// <summary>Localized feature display name shared by cards, details and legend.</summary>
        public static string FeatureName(TranslationFeatureType feature)
        {
            switch (feature)
            {
                case TranslationFeatureType.Dubbing:
                    return FeatureVoiceOver;
                case TranslationFeatureType.ExtraDubbing:
                    return FeatureAdditionalVoiceOver;
                case TranslationFeatureType.Textures:
                    return FeatureReplacementTextures;
                default:
                    return FeatureCustomFonts;
            }
        }
    }
}
