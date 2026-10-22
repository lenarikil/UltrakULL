using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using HarmonyLib;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UltrakULL.Harmony_Patches;
using UltrakULL.json;
using UltrakULL.UI.Notifications;
using static UltrakULL.CommonFunctions;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace UltrakULL
{
    public static class Core
    {
        public static Font VcrFont;

        public static volatile bool updateAvailable;
        public static volatile bool updateFailed;
        public static volatile string latestOnlineVersion;
        public static volatile bool updateCheckCompleted;
        
        public static bool GlobalFontReady;
        public static bool TMPFontReady;
        
        public static Font GlobalFont;
        public static Font MuseumFont;
        public static TMP_FontAsset GlobalFontTMP;
        public static TMP_FontAsset MuseumFontTMP;
        public static TMP_FontAsset CJKFontTMP;
        public static TMP_FontAsset JaFontTMP;
        public static TMP_FontAsset ArabicFontTMP;
        public static TMP_FontAsset HebrewFontTMP;
        public static Sprite[] CustomRankImages;

        // The mod bundle's own overlay material. Its shader is the bundle's copy
        // of "TextMeshPro/Distance Field Overlay" and, unlike the game's copy,
        // it includes the compiled UNDERLAY variant (no game material uses
        // overlay + underlay, so the game's copy has that variant stripped).
        // Overlay material presets with shadows must be cloned from this one.
        public static Material GlobalFontTMPOverlayMat;

        // Custom fonts loaded from external files
        public static TMP_FontAsset CustomMainFontTMP;
        public static TMP_FontAsset CustomMuseumFontTMP;
        public static TMP_FontAsset CustomTerminalFontTMP;
        public static TMP_FontAsset CustomSecretTerminalFontTMP;

        private static bool startupNotifShown = false;

        public static Sprite ArabicUltrakillLogo;

		public static bool wasLanguageReset = false;
        
        private static readonly HttpClient Client = new HttpClient();
        
        //Encapsulation function to patch all of the front end.
        public static void PatchFrontEnd(GameObject frontEnd)
        {
            // Built-in English leaves the front end untouched (vanilla text).
            // The language menu is injected separately and remains available.
            if (isUsingEnglish()) return;

            MainMenu.Patch(frontEnd);
            Options options = new Options(ref frontEnd);
        }

        public static async Task CheckForUpdates()
        {
            string rssUrl = "https://github.com/ClearwaterUK/UltrakULL/releases.atom";
            // Increase timeout to 10 seconds for better reliability
            Client.Timeout = TimeSpan.FromSeconds(10);
            // Add User-Agent header to avoid being blocked by GitHub
            if (!Client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                Client.DefaultRequestHeaders.Add("User-Agent", "UltrakULL-Update-Checker/1.0");
            }

            try
            {
                string rssContent = await Client.GetStringAsync(rssUrl);
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(rssContent);

                // Create namespace manager for Atom
                var nsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom");

                // Get latest release entry using namespace
                var latest = doc.SelectSingleNode("//atom:entry[1]", nsManager);
                if (latest == null)
                    throw new Exception("No releases found in RSS feed");

                string title = latest.SelectSingleNode("atom:title", nsManager)?.InnerText ?? "";
                string updated = latest.SelectSingleNode("atom:updated", nsManager)?.InnerText ?? "";

                // Parse version from title (usually the tag)
                string versionString = title.TrimStart('v', 'V');
                // Remove any suffix after hyphen (e.g., "-beta.2") or plus (e.g., "+build")
                int hyphenIndex = versionString.IndexOf('-');
                if (hyphenIndex >= 0)
                    versionString = versionString.Substring(0, hyphenIndex);
                int plusIndex = versionString.IndexOf('+');
                if (plusIndex >= 0)
                    versionString = versionString.Substring(0, plusIndex);
                // Ensure version string is valid for Version class
                Logging.Message("Latest version from RSS (cleaned): " + versionString);
                Logging.Message("Current local version: " + MainPatch.GetVersion());

                Version onlineVersion = new Version(versionString);
                // Clean local version similarly
                string localVersionString = MainPatch.GetVersion();
                localVersionString = localVersionString.TrimStart('v', 'V');
                hyphenIndex = localVersionString.IndexOf('-');
                if (hyphenIndex >= 0)
                    localVersionString = localVersionString.Substring(0, hyphenIndex);
                plusIndex = localVersionString.IndexOf('+');
                if (plusIndex >= 0)
                    localVersionString = localVersionString.Substring(0, plusIndex);
                Version localVersion = new Version(localVersionString);

                // Simple version compare - update available if online version is newer
                updateAvailable = localVersion.CompareTo(onlineVersion) < 0;
                latestOnlineVersion = onlineVersion.ToString();

                if (updateAvailable)
                    Logging.Warn("UPDATE AVAILABLE!");
                else
                    Logging.Message("No newer version detected. Assuming current version is up to date.");

                updateFailed = false;
            }
            catch (TaskCanceledException)
            {
                Logging.Error("Update check timed out after 10 seconds.");
                updateAvailable = false;
                updateFailed = true;
            }
            catch (HttpRequestException hre)
            {
                Logging.Error("Network error while checking for updates: " + hre.Message);
                updateAvailable = false;
                updateFailed = true;
            }
            catch (Exception e)
            {
                Logging.Error("Unable to check for updates via RSS feed.");
                Logging.Error(e.ToString());
                updateAvailable = false;
                updateFailed = true;
            }

            updateCheckCompleted = true;
        }

        // Called every frame from MainPatch.Update; shows the update toast once the
        // background check has finished (it can complete after the main menu has loaded).
        public static void TryShowPendingNotifications()
        {
            ShowStartupNotificationIfNeeded();
        }


        //Patches all text strings in the pause menu.
        public static void PatchPauseMenu(ref GameObject canvasObj)
        {
            try
            {
                GameObject pauseMenu = canvasObj.Child("PauseMenu");

                //Title
                TextMeshProUGUI pauseText = pauseMenu.Child("Text").TMP();
                pauseText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_title;

                //Resume
                TextMeshProUGUI continueText = pauseMenu.FindPath("Resume", "Text").TMP();
                continueText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_resume;

                //Checkpoint
                TextMeshProUGUI checkpointText = pauseMenu.FindPath("Restart Checkpoint", "Text").TMP();
                checkpointText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_respawn;
                //SKIP button 
                if (GetCurrentSceneName().Contains("Intermission"))
                {
                    TextMeshProUGUI skipText = pauseMenu.FindPath("Restart Checkpoint (1)", "Text").TMP();
                    skipText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_skip;
                }
                //Restart mission
                TextMeshProUGUI restartText = pauseMenu.FindPath("Restart Mission", "Text").TMP();
                restartText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restart;

                //Options
                TextMeshProUGUI optionsText = pauseMenu.FindPath("Options", "Text").TMP();
                optionsText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_options;

                //Quit
                TextMeshProUGUI quitText = pauseMenu.FindPath("Quit Mission", "Text").TMP();
                quitText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quit;

                //Quit+Restart windows
                GameObject pauseDialogs = canvasObj.Child("PauseMenuDialogs");

                //Quit
                GameObject quitDialog = pauseDialogs.FindPath("Quit Confirm", "Panel");
                TextMeshProUGUI quitDialogText = quitDialog.Child("Text (2)").TMP();
                quitDialogText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quitConfirm;

                TextMeshProUGUI quitDialogTooltip = quitDialog.Child("Text (1)").TMP();
                quitDialogTooltip.text = LanguageManager.CurrentLanguage.pauseMenu.pause_disableWindow;

                TextMeshProUGUI quitDialogYes = quitDialog.FindPath("Confirm", "Text").TMP();
                quitDialogYes.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quitConfirmYes;

                TextMeshProUGUI quitDialogNo = quitDialog.FindPath("Cancel", "Text").TMP();
                quitDialogNo.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quitConfirmNo;

                //Restart
                GameObject restartDialog = pauseDialogs.FindPath("Restart Confirm", "Panel");

                TextMeshProUGUI restartDialogText = restartDialog.Child("Text").TMP();
                restartDialogText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restartConfirm;

                TextMeshProUGUI restartDialogTooltip = restartDialog.Child("Text (1)").TMP();
                restartDialogTooltip.text = LanguageManager.CurrentLanguage.pauseMenu.pause_disableWindow;

                TextMeshProUGUI restartDialogYes = restartDialog.FindPath("Confirm", "Text").TMP();
                restartDialogYes.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restartConfirmYes;

                TextMeshProUGUI restartDialogNo = restartDialog.FindPath("Cancel", "Text").TMP();
                restartDialogNo.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restartConfirmNo;
            }
            catch (Exception e)
            {
                Logging.Error("Failed to patch pause menu.");
                Logging.Error(e.ToString());
            }
        }
        
        private static string FindFontFile(string directory, string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                Logging.Message($"FindFontFile: baseName is null or empty");
                return null;
            }

            // Если имя уже содержит расширение, проверим как есть
            string fullPath = Path.Combine(directory, baseName);
            Logging.Message($"FindFontFile: checking exact path '{fullPath}'");
            if (File.Exists(fullPath))
            {
                Logging.Message($"FindFontFile: found exact file '{fullPath}'");
                return fullPath;
            }

            // Попробуем добавить распространённые расширения шрифтов
            string[] extensions = { ".ttf", ".otf", ".ttc", ".woff", ".woff2" };
            foreach (var ext in extensions)
            {
                string path = Path.Combine(directory, baseName + ext);
                Logging.Message($"FindFontFile: checking path '{path}'");
                if (File.Exists(path))
                {
                    Logging.Message($"FindFontFile: found file with extension '{ext}' at '{path}'");
                    return path;
                }
            }

            // Не найдено
            Logging.Message($"FindFontFile: no file found for '{baseName}' in directory '{directory}'");
            return null;
        }

        /// <summary>
        /// Creates a TMP_FontAsset from a font file path.
        /// </summary>
        /// <param name="fontPath">Full path to the font file (TTF, OTF, etc.)</param>
        /// <param name="samplingPointSize">Sampling point size for the font atlas (default 90)</param>
        /// <param name="padding">Padding between glyphs in the atlas (default 9)</param>
        /// <param name="renderMode">Glyph render mode (default SDFAA)</param>
        /// <param name="atlasWidth">Width of the atlas texture (default 1024)</param>
        /// <param name="atlasHeight">Height of the atlas texture (default 1024)</param>
        /// <param name="atlasPopulationMode">Atlas population mode (default Dynamic)</param>
        /// <returns>TMP_FontAsset if successful, null otherwise</returns>
        private static TMP_FontAsset CreateTMPFontFromFile(string fontPath,
            int samplingPointSize = 90,
            int padding = 9,
            GlyphRenderMode renderMode = GlyphRenderMode.SDFAA,
            int atlasWidth = 1024,
            int atlasHeight = 1024,
            AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.Dynamic)
        {
            if (string.IsNullOrEmpty(fontPath) || !File.Exists(fontPath))
            {
                Logging.Error($"CreateTMPFontFromFile: font file not found or path empty: {fontPath}");
                return null;
            }

            try
            {
                // Load Unity Font from file
                Font unityFont = new Font(fontPath);
                if (unityFont == null)
                {
                    Logging.Error($"CreateTMPFontFromFile: failed to create Unity Font from {fontPath}");
                    return null;
                }

                Logging.Message($"CreateTMPFontFromFile: creating TMP font asset from {Path.GetFileName(fontPath)}");
                TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(
                    unityFont,
                    samplingPointSize,
                    padding,
                    renderMode,
                    atlasWidth,
                    atlasHeight,
                    atlasPopulationMode
                );

                // If failed, try without parameters (simpler method)
                if (tmpFont == null)
                {
                    Logging.Warn($"CreateTMPFontFromFile: first attempt failed, trying without parameters...");
                    tmpFont = TMP_FontAsset.CreateFontAsset(unityFont);
                }

                if (tmpFont == null)
                {
                    Logging.Error($"CreateTMPFontFromFile: TMP_FontAsset.CreateFontAsset returned null for {fontPath}");
                    return null;
                }

                // CreateFontAsset leaves the name empty. The file name makes the
                // font recognizable in logs and by the font swap, whose
                // classification reads font names (a terminal font file named
                // after Tahoma keeps terminals on the terminal font).
                tmpFont.name = Path.GetFileNameWithoutExtension(fontPath);

                ApplyStrikeoutMetrics(tmpFont, fontPath, samplingPointSize);

                Logging.Message($"CreateTMPFontFromFile: successfully created TMP font asset '{tmpFont.name}'");
                return tmpFont;
            }
            catch (Exception e)
            {
                Logging.Error($"CreateTMPFontFromFile: exception while processing {fontPath}: {e.Message}");
                Logging.Error(e.ToString());
                return null;
            }
        }

        private static void ApplyStrikeoutMetrics(TMP_FontAsset tmpFont, string fontPath, int samplingPointSize = 90)
        {
            OpenTypeStrikeoutMetrics metrics;
            string reason;
            if (!OpenTypeStrikeoutMetricsReader.TryRead(fontPath, out metrics, out reason))
            {
                Logging.Warn($"CreateTMPFontFromFile: keeping TMP strikeout metrics for {Path.GetFileName(fontPath)} because {reason}.");
                return;
            }

            var faceInfo = tmpFont.faceInfo;
            float effectivePointSize = faceInfo.pointSize > 0f ? faceInfo.pointSize : samplingPointSize;
            float scale = effectivePointSize / metrics.UnitsPerEm;

            // Calculate the center of the strikethrough line using the font's capHeight metric.
            // capHeight is the height of uppercase letters, which provides a good compromise
            // between ALL-CAPS text and mixed-case text:
            //   - For ALL-CAPS: line at 50% of capHeight is near the center of uppercase letters
            //   - For mixed case: line at 50% of capHeight is slightly above center of lowercase,
            //     but much better than using ascender (which would be too high for lowercase)
            // Fallback chain: capHeight -> xHeight -> ascender
            float referenceHeight;
            if (metrics.CapHeight > 0)
                referenceHeight = metrics.CapHeight;
            else if (metrics.XHeight > 0)
                referenceHeight = metrics.XHeight;
            else
                referenceHeight = metrics.Ascender;

            float centerOffset = (referenceHeight * 0.5f) * scale;
            float thickness = metrics.Thickness * scale;

            faceInfo.strikethroughOffset = centerOffset;
            faceInfo.strikethroughThickness = thickness;
            tmpFont.faceInfo = faceInfo;

            Logging.Message($"CreateTMPFontFromFile: applied strikeout metrics for {Path.GetFileName(fontPath)} " +
                $"(raw: pos={metrics.Position}, thick={metrics.Thickness}, upem={metrics.UnitsPerEm}, " +
                $"ascender={metrics.Ascender}, xHeight={metrics.XHeight}, capHeight={metrics.CapHeight}, " +
                $"facePointSize={faceInfo.pointSize}, samplingPointSize={samplingPointSize}, " +
                $"effectivePointSize={effectivePointSize}, " +
                $"offset={faceInfo.strikethroughOffset:F3}, thickness={faceInfo.strikethroughThickness:F3}).");
        }

        public static void LoadCustomFonts()
        {
            // Reset custom font fields before loading
            CustomMainFontTMP = null;
            CustomMuseumFontTMP = null;
            CustomTerminalFontTMP = null;
            CustomSecretTerminalFontTMP = null;

            if (LanguageManager.CurrentLanguage?.metadata?.fonts == null)
            {
                Logging.Message("No custom fonts defined in language metadata.");
                return;
            }

            var fonts = LanguageManager.CurrentLanguage.metadata.fonts;

            // Debug log font fields
            Logging.Message($"Custom font fields - MainFont: '{fonts.MainFont}', MuseumFont: '{fonts.MuseumFont}', TerminalFont: '{fonts.TerminalFont}', SecretTerminalFont: '{fonts.SecretTerminalFont}'");

            // Check if any font field is non-empty
            if (string.IsNullOrEmpty(fonts.MainFont) &&
                string.IsNullOrEmpty(fonts.MuseumFont) &&
                string.IsNullOrEmpty(fonts.TerminalFont) &&
                string.IsNullOrEmpty(fonts.SecretTerminalFont))
            {
                Logging.Message("All custom font fields are empty, skipping custom font loading.");
                return;
            }

            string fontsPath = LanguageManager.GetLanguageAssetFolder("fonts");

            Logging.Message($"Custom fonts directory path: {fontsPath}");
            if (!Directory.Exists(fontsPath))
            {
                Logging.Message($"Custom fonts directory not found: {fontsPath}");
                return;
            }

            Logging.Message($"Loading custom fonts from: {fontsPath}");

            CustomMainFontTMP = TryLoadCustomFont("MainFont", fonts.MainFont, fontsPath);
            CustomMuseumFontTMP = TryLoadCustomFont("MuseumFont", fonts.MuseumFont, fontsPath);
            CustomTerminalFontTMP = TryLoadCustomFont("TerminalFont", fonts.TerminalFont, fontsPath);
            CustomSecretTerminalFontTMP = TryLoadCustomFont("SecretTerminalFont", fonts.SecretTerminalFont, fontsPath);
        }

        private static TMP_FontAsset TryLoadCustomFont(string fontKind, string fileName, string fontsPath)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            string fontPath = FindFontFile(fontsPath, fileName);
            if (fontPath == null)
            {
                Logging.Warn($"Custom {fontKind} file not found: {fileName} (searched with extensions .ttf, .otf, .ttc, .woff, .woff2)");
                return null;
            }

            TMP_FontAsset tmpFont = CreateTMPFontFromFile(fontPath);
            if (tmpFont == null)
            {
                Logging.Error($"CreateTMPFontFromFile returned null for {fontKind}");
                return null;
            }

            Logging.Message($"Loaded custom {fontKind} TMP: {fileName} (from {Path.GetFileName(fontPath)})");
            return tmpFont;
        }

        /// <summary>
        /// Reloads custom fonts based on the current language.
        /// Call this after changing language.
        /// </summary>
        public static void ReloadCustomFonts()
        {
            LoadCustomFonts();
        }

        public static void LoadFonts()
        {
            Logging.Message("Loading font resource bundle...");
            //Will load from the same directory that the dll is in.
            AssetBundle fontBundle = AssetBundle.LoadFromFile(Path.Combine(MainPatch.ModFolder,"ullfont.resource"));

            AssetBundle extraFontBundle = AssetBundle.LoadFromFile(Path.Combine(MainPatch.ModFolder, "arabfonts","arabfonts"));

            if (extraFontBundle == null)
            {
                Logging.Error("Failed to load Arabic / Hebrew fonts. :( (No extra AssetBundle found!)");
            }
            else
            {
                Logging.Message("Extra Fonts Asset Bundle has been loaded...");

                TMP_FontAsset arabicFontAsset = extraFontBundle.LoadAsset<TMP_FontAsset>("segoeui SDF Arabic");
				TMP_FontAsset hebrewFontAsset = extraFontBundle.LoadAsset<TMP_FontAsset>("segoeui SDF Hebrew");
				Sprite arabicLogo = extraFontBundle.LoadAsset<Sprite>("2023_improved_logo.png");

                Sprite rankD = extraFontBundle.LoadAsset<Sprite>("RankD.png");
                Sprite rankC = extraFontBundle.LoadAsset<Sprite>("RankC.png");
                Sprite rankB = extraFontBundle.LoadAsset<Sprite>("RankB.png");
                Sprite rankA = extraFontBundle.LoadAsset<Sprite>("RankA.png");
                Sprite rankS = extraFontBundle.LoadAsset<Sprite>("RankS.png");
                Sprite rankSS = extraFontBundle.LoadAsset<Sprite>("RankSS.png");
                Sprite rankSSS = extraFontBundle.LoadAsset<Sprite>("RankSSS.png");
                Sprite rankU = extraFontBundle.LoadAsset<Sprite>("RankU.png");

                CustomRankImages = new Sprite[8];
				CustomRankImages[0] = rankD;
				CustomRankImages[1] = rankC;
				CustomRankImages[2] = rankB;
				CustomRankImages[3] = rankA;
				CustomRankImages[4] = rankS;
				CustomRankImages[5] = rankSS;
				CustomRankImages[6] = rankSSS;
				CustomRankImages[7] = rankU;

				if (arabicFontAsset == null)
                {
                    Logging.Warn("There is no Arabic font in this AssetBundle!?");
                }
                else
                {
                    Logging.Message("Arabic Font has been loaded.");
                    ArabicFontTMP = arabicFontAsset;
                }

                if (arabicLogo == null)
                {
					Logging.Warn("There is no Arabic logo in this AssetBundle!?");
				}
                else
                {
                    ArabicUltrakillLogo = arabicLogo;
                }

				if (hebrewFontAsset == null)
				{
					Logging.Warn("There is no Hebrew font in this AssetBundle!?");
				}
				else
				{
					Logging.Message("Hebrew Font has been loaded.");
					HebrewFontTMP = hebrewFontAsset;
				}
			}

			if (fontBundle == null)
            {
                Logging.Error("FAILED TO LOAD");
            }
            else
            {
                Logging.Message("Font bundle loaded.");
                Logging.Message("Loading fonts from bundle...");
                
                Font font1 = fontBundle.LoadAsset<Font>("VCR_OSD_MONO_EXTENDED");
                Font font2 = fontBundle.LoadAsset<Font>("EBGaramond-Regular");
                TMP_FontAsset font1TMP = fontBundle.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
                TMP_FontAsset font2TMP = fontBundle.LoadAsset<TMP_FontAsset>("EBGaramond-Regular_TMP");

                TMP_FontAsset cjkFontTMP = fontBundle.LoadAsset<TMP_FontAsset>("NotoSans-CJK_TMP");
                TMP_FontAsset jafontTMP = fontBundle.LoadAsset<TMP_FontAsset>("JF-Dot-jiskan16s-2000_TMP");
                if (font1 && font2)
                {
                    Logging.Warn("Normal fonts loaded.");
                    GlobalFont = font1;
                    MuseumFont = font2;
                    GlobalFontReady = true;
                }
                else
                {
                    Logging.Error("FAILED TO LOAD NORMAL FONTS");
                    GlobalFontReady = false;
                }
                if(font1TMP && font2TMP && cjkFontTMP && jafontTMP)
                {
                    Logging.Warn("Normal TMP fonts loaded.");
                    GlobalFontTMP = font1TMP;
                    MuseumFontTMP = font2TMP;
                    CJKFontTMP = cjkFontTMP;
                    JaFontTMP = jafontTMP;

                    TMPFontReady = true;
                }
                else
                {
                    Logging.Error("FAILED TO LOAD TMP FONTS");
                    TMPFontReady = false;
                }

                Material overlayMat = fontBundle.LoadAsset<Material>("VCR_OSD_MONO_EXTENDED_TMP_Overlay_Material");
                if (overlayMat != null)
                {
                    GlobalFontTMPOverlayMat = overlayMat;
                }
                else
                {
                    Logging.Warn("Overlay material not found in the font bundle; overlay text shadows may not render.");
                }
                
                // Load custom fonts after standard fonts and materials are ready
                Logging.Message("Loading custom fonts...");
                LoadCustomFonts();
            }
        }
        
        // Shows a single startup notification per session: mod loaded, current language,
        // update status. The update check runs on a background task, so this is polled
        // from MainPatch.Update until both the language and the check are ready.
        private static void ShowStartupNotificationIfNeeded()
        {
            if (startupNotifShown)
                return;
            if (!updateCheckCompleted)
                return;
            // Notiffy/toast rendering is reliable only once the menu UI exists;
            // the update check often completes during Bootstrap/Intro.
            if (GetCurrentSceneName() != "Main Menu")
                return;
            if (LanguageManager.CurrentLanguage == null)
                return;

            startupNotifShown = true;

            string version = MainPatch.GetVersion();
            string lang = LanguageManager.CurrentLanguage.metadata.langName;
            Notifications notif = LanguageManager.CurrentLanguage.notifications;

            string title = NotificationText.Localize(notif?.startup_title, NotificationText.StartupTitle,
                "version", version);
            string langLine = NotificationText.Localize(notif?.startup_languageLine, NotificationText.StartupLanguageLine,
                "lang", lang);

            if (updateAvailable)
            {
                string body = langLine + "\n" + NotificationText.Localize(notif?.startup_updateAvailable,
                    NotificationText.StartupUpdateAvailable, "latestVersion", latestOnlineVersion ?? "?");
                NotificationRouter.Current.Info(
                    title,
                    body,
                    NotificationText.Localize(notif?.startup_viewUpdate, NotificationText.StartupViewUpdate),
                    () => Application.OpenURL("https://github.com/ClearwaterUK/UltrakULL/releases/latest"),
                    8f);
            }
            else if (updateFailed)
            {
                string body = langLine + "\n" + NotificationText.Localize(notif?.startup_updateCheckFailed,
                    NotificationText.StartupUpdateCheckFailed);
                NotificationRouter.Current.Warning(title, body, 6f);
            }
            else
            {
                string body = langLine + "\n" + NotificationText.Localize(notif?.startup_upToDate,
                    NotificationText.StartupUpToDate);
                NotificationRouter.Current.Success(title, body, 5f);
            }
        }

        public static void HandleSceneSwitch(Scene scene,ref GameObject canvas)
        {

            //Logging.Message("Switching scenes...");
            string levelName = GetCurrentSceneName();
            if(levelName == "Intro" || levelName == "Bootstrap")
            {
                //Don't do anything if we're still booting up the game.
                //Logging.Warn("In intro, not hooking yet");
                return;
            }

            // Remember the game's own text before the first localization pass of this
            // scene, so a later language switch can put the original strings back.
            OriginalTextSnapshot.Capture();

            // The tracked originals of the previous scene belong to objects that are
            // gone by now.
            LiveText.PruneDead();
            LocalizeBossBar.PruneDead();
            LocalizedDropdowns.PruneDead();
            LiveSubtitles.PruneDead();
            UltrakULL.audio.AudioSwapper.PruneDeadSourceOriginals();

            ApplyLanguageToScene(levelName);
        }

        /// <summary>
        /// Applies the active language to the scene that is currently loaded.
        ///
        /// The game's own text is the baseline: it is restored first and the active
        /// language is written on top of it. Switching to the built-in English
        /// therefore brings back the game's own strings, and switching to another
        /// language never leaves strings of the previous language behind.
        ///
        /// Used by the language switch so the new language is visible right away,
        /// without reloading the scene.
        /// </summary>
        public static void ApplyLanguageToCurrentScene()
        {
            string levelName = GetCurrentSceneName();
            if(levelName == "Intro" || levelName == "Bootstrap")
                return;

            OriginalTextSnapshot.Restore();

            // The boss bar is only updated by the running game, so the restored
            // names have to be translated again right away.
            LocalizeBossBar.Reapply();

            ApplyLanguageToScene(levelName);

            // The screens that the game fills when the player opens them (the shop
            // terminal, ...) are not part of the localization pass. They are
            // registered as live groups, so they follow the new language as well
            // without the player having to close and open them again.
            LiveText.Reapply();

            // The options of the dropdowns are not text components, so the live
            // groups cannot put the game's own options back for the built-in
            // English.
            LocalizedDropdowns.Reapply();

            // The money counter is written by the game itself, so it needs its own
            // refresh to follow the new language.
            UpdateMoneyPatch.Reapply();

            // The price and the ownership text of the shop items are written by the
            // game when the shop is opened, so they need their own refresh.
            LocalizeVariationOwnership.Reapply();

            // The button that switches between the standard and the alternate colors
            // is only written by the game when the colors are switched.
            GunColorTypeGetterPatch.Reapply();

            // The game builds the cheat menu only once, when it starts. It has to be
            // rebuilt, so the menu and the cheat list on the HUD follow the new
            // language as well.
            Cheats.ReapplyCheatMenu();

            // The enemy bio lives inside the shop, so it is rendered again after the
            // live groups have put the game's own text back.
            LocalizeEnemyInfo.Reapply();

            // The screens the game fills when the player opens them follow the new
            // language as well, so nothing has to be closed and opened again.
            LocalizeScanningText.Reapply();
            SelectFish.Reapply();
            FishingHUDPatch.Reapply();
            LocalizeSaveSlots.Reapply();
            LocalizeLevelStatNames.Reapply();

            // The subtitles that are on screen keep their line, only their language
            // changes.
            LiveSubtitles.Reapply();

            // The skip hint of a cutscene and the title reveal of the level follow
            // the new language as well, without starting over.
            CutsceneSkipTextPatch.Reapply();
            LocalizeLevelPopup.Reapply();
            SandboxWorldOptions.Reapply();
            SpawnMenuPatch.Reapply();
            LocalizeFinalRankInfo.Reapply();

            // Fonts are normally chosen from TMP's OnEnable hook. Existing UI does
            // not run that hook during a language switch, so refresh only the active
            // game's TMP components after every live text reapplication.
            TextMeshProFontSwap.ReapplyActiveSceneFonts();

            // Textures belong to the language as well, so they are rebuilt and applied
            // to the scene that is currently loaded.
            TexturePatcher.ReapplyForLanguageChange();
        }

        /// <summary>
        /// Runs the localization of the active language on the current scene.
        /// </summary>
        private static void ApplyLanguageToScene(string levelName)
        {
            //Each scene (level) has an object called Canvas. Most game objects are there.
            GameObject canvasObj = GetInactiveRootObject("Canvas");
            if (!canvasObj)
            {
                Logging.Fatal("UNABLE TO FIND CANVAS IN CURRENT SCENE");
                return;
            }
            else
            {
                TextMeshProFontSwap.ClearFontSwapCache();
                TextFontSwap.TextFontSwapper.ClearCache();

                // Remember what the text components show right now. Everything the
                // localization pass below changes is a translation of the mod, the
                // rest belongs to the game and has to survive a language switch.
                OriginalTextSnapshot.CapturePassBaseline();

                switch (levelName)
                {
                    case "Intro": { break; }
                    case "Main Menu":
                        {
                            if (Core.wasLanguageReset)
                            {
                                Core.wasLanguageReset = false;
                                NotificationRouter.Current.Warning("UltrakULL", "Current language file could not be loaded. Switched to English.", 5f);
                            }

                            PatchFrontEnd(canvasObj);

                            break;
                        }

                    default:
                        {
                            if (isUsingEnglish())
                            {
                                Logging.Warn("Current language is English, not patching.");
                                break;
                            }

                            Logging.Message("Regular scene");
                            Logging.Message("Attempting to patch base elements");
                            try { PatchPauseMenu(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { Cheats.PatchCheatConsentPanel(ref canvasObj); ; } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { Sandbox.PatchAlterMenu(); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { HUDMessages.PatchDeathScreen(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { LevelStatWindow.PatchStats(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { HUDMessages.PatchMisc(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { Options options = new Options(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }

                            Logging.Message("Base elements patched");
                        }


                        if (levelName.Contains("Tutorial"))
                        {
                            Logging.Message("Tutorial");
                        }
                        else if (AngryLevel.IsAngryCustomLevel() == true)
                        {
                            Logging.Message("Angry Custom Level");
                            AngryLevel.PatchAngry();
                        }
                        else if (levelName.Contains("-S"))
                        {
                            Logging.Message("Secret");
                            SecretLevels secretLevels = new SecretLevels(ref canvasObj);
                        }
                        if (levelName.Contains("0-") & !levelName.Contains("-E"))
                        {
                            Logging.Message("Prelude");
                            Prelude preludePatchClass = new Prelude(ref canvasObj);
                        }
                        else if ((levelName.Contains("1-") & !levelName.Contains("-E")) || (levelName.Contains("2-") & !levelName.Contains("-E")) || (levelName.Contains("3-") & !levelName.Contains("-E")))
                        {
                            Logging.Message("Act 1");
                            Act1.PatchAct1(ref canvasObj);
                        }
                        else if ((levelName.Contains("4-") & !levelName.Contains("-E")) || (levelName.Contains("5-") & !levelName.Contains("-E")) || (levelName.Contains("6-") & !levelName.Contains("-E")))
                        {
                            Logging.Message("Act 2");
                            Act2.PatchAct2(ref canvasObj);
                        }
                        else if ((levelName.Contains("7-") & !levelName.Contains("-E")) || (levelName.Contains("8-") & !levelName.Contains("-E")) || (levelName.Contains("9-") & !levelName.Contains("-E")))
                        {
                            Logging.Message("Act 3");
                            if (LanguageManager.CurrentLanguage.act3 != null)
                            {
                                Act3.PatchAct3(ref canvasObj);
                            }
                            else
                            {
                                Logging.Warn("Category is not found in the language file!");
                            }
                        }
                        else if (levelName.Contains("P-"))
                        {
                            Logging.Message("Prime");
                            PrimeSanctum primeSanctumClass = new PrimeSanctum();
                        }
                        else if (levelName.Contains("-E"))
                        {
                            Logging.Message("Encore");
                            if (LanguageManager.CurrentLanguage.encore != null)
                            {
                                Encore.PatchEncore(ref canvasObj);
                            }

                        }
                        else if (levelName == "uk_construct")
                        {
                            Logging.Message("Sandbox");
                            Sandbox sandbox = new Sandbox(ref canvasObj);
                        }
                        else if (levelName == "Endless")
                        {
                            Logging.Message("CyberGrind");
                            CyberGrind.PatchCg();
                        }
                        else if (levelName.Contains("Intermission") || levelName.Contains("EarlyAccessEnd"))
                        {
                            Logging.Message("Intermission");
                            Intermission intermission = new Intermission(ref canvasObj);
                        }
                        else if (levelName == "CreditsMuseum2")
                        {
                            Logging.Message("DevMuseum");
                            DevMuseum devMuseum = new DevMuseum();
                        }
                        break;
                }

                // The pass has written the active language into the components it
                // covers. They are the only ones a later language switch takes the
                // translation off again.
                OriginalTextSnapshot.MarkTranslated();
            }
        }

        public static async void ApplyPostInitFixes(GameObject canvasObj)
        {
            await Task.Delay(250); // Fix warning about async without await
            /*if (GetCurrentSceneName() == "Main Menu")
            {
                //Open Language Folder button in Options->Language
                TextMeshProUGUI openLangFolderText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(canvasObj,"OptionsMenu"), "Language Page"),"Scroll Rect (1)"),"Contents"),"OpenLangFolder"),"Slot Text")); 
                openLangFolderText.text = "<color=#03fc07>Open language folder</color>";
                
            }*/
        }
    }
}
