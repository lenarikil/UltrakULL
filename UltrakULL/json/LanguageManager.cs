using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ArabicSupportUnity;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Newtonsoft.Json;
using UltrakULL.audio;
using UltrakULL.Harmony_Patches;
using UltrakULL.Harmony_Patches.AudioSwaps;
using UltrakULL.Harmony_Patches.Subtitles;
using UltrakULL.UI.Notifications;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.json
{
    public static class LanguageManager
    {
        public static Dictionary<string, JsonFormat> allLanguages = new Dictionary<string, JsonFormat>();
        private static Dictionary<string, JsonFormat> allLanguagesDisplayNames = new Dictionary<string, JsonFormat>();
        public static JsonFormat CurrentLanguage { get; private set; }
        private static ManualLogSource jsonLogger = Logger.CreateLogSource("LanguageManager");
        public static ConfigFile configFile;

        #region Helper Properties
        public static bool IsRightToLeft { get => CurrentLanguage.metadata.langRTL; }
        public static bool UsingHinduNumbers { get => CurrentLanguage.metadata.langHinduNumbers; }
        public static float rankHeight { get => CurrentLanguage.metadata.rankHeight; }
        #endregion

        #region Asset Folders
        /// <summary>
        /// Returns the folder that holds a given resource kind ("audio", "textures", "fonts")
        /// for the active language. Language packs keep these folders inside their own RootFolder.
        /// When the active language has no folder on disk (built-in English) a non-existent path
        /// is returned so lookups simply find nothing.
        /// </summary>
        public static string GetLanguageAssetFolder(string kind)
        {
            JsonFormat lang = CurrentLanguage;
            if (lang != null && !string.IsNullOrEmpty(lang.RootFolder))
                return Path.Combine(lang.RootFolder, kind);

            string langName = (lang != null && lang.metadata != null) ? lang.metadata.langName : "en-GB";
            return Path.Combine(Paths.ConfigPath, "ultrakull", kind, langName);
        }

        /// <summary>
        /// Recomputes the asset folders that depend on the active language. Must be called
        /// after CurrentLanguage changes, both at startup and on language switches.
        /// </summary>
        public static void RefreshAssetFolders()
        {
            string audioFolder = GetLanguageAssetFolder("audio");
            AudioSwapper.SpeechFolder = audioFolder + Path.DirectorySeparatorChar;
            SubtitledAudioSourcesReplacer.SpeechFolder = audioFolder + Path.DirectorySeparatorChar;
        }
        #endregion

        public static void InitializeManager()
        {
            LoadLanguages();

            string configDir = Path.Combine(Paths.ConfigPath, "ultrakull");
            string configPath = Path.Combine(configDir, "Settings.cfg");
            string legacyConfigPath = Path.Combine(configDir, "lastLang.cfg");

            // Migrate the previous config name so existing settings survive the rename.
            string activeConfigPath = configPath;
            try
            {
                if (!File.Exists(configPath) && File.Exists(legacyConfigPath))
                {
                    File.Move(legacyConfigPath, configPath);
                    jsonLogger.Log(LogLevel.Info, "Migrated lastLang.cfg to Settings.cfg");
                }
            }
            catch (Exception ex)
            {
                // If the rename fails, keep using the old file so settings are never lost.
                jsonLogger.Log(LogLevel.Warning, "Failed to migrate lastLang.cfg to Settings.cfg: " + ex.Message);
                if (File.Exists(legacyConfigPath))
                    activeConfigPath = legacyConfigPath;
            }

            configFile = new ConfigFile(activeConfigPath, true);

            UltrakULLConfig.Register(configFile);

            string value = configFile.Bind("General", "LastLanguage", "en-GB").Value;
            string dubValue = configFile.Bind("General", "activeDubbing", "False").Value;

            if (allLanguages.ContainsKey(value))
            {
                jsonLogger.Log(LogLevel.Message, "Setting language to " + value);
                CurrentLanguage = CloneLanguage(allLanguages[value]);
                if (IsRightToLeft)
                {
                    Logging.Message("Language is set as RTL - applying fix!");
                    CurrentLanguage = ApplyRtl(CurrentLanguage);
                }
            }
            else
            {
                jsonLogger.Log(LogLevel.Message, "Previous lang file is missing from disk: " + value);
                Logging.Warn("Setting language back to en-GB to avoid problems");
                Core.wasLanguageReset = true;
                CurrentLanguage = CloneLanguage(allLanguages["en-GB"]);
                SetCurrentLanguage("en-GB");
            }

            // Asset folders (audio, textures, fonts) depend on the active language.
            RefreshAssetFolders();

            LoadSubtitledSourcesConfig();

            RefreshLocalizedInputs();
        }

        public static void DumpLastLanguage()
        {
            configFile.Bind("General", "LastLanguage", "en-GB").Value = CurrentLanguage.metadata.langName; // Thank you copilot
        }

        public static void LoadLanguagesInDirectory(string path)
        {
            Logging.Info($"Loading all language files in \"{path}\"");

            if (!Directory.Exists(path))
                return;

            // One folder = one language pack. The folder is identified by the
            // translation.json file at its root; the folder path becomes its RootFolder.
            foreach (string directory in Directory.GetDirectories(path))
            {
                string dirName = Path.GetFileName(directory);
                if (dirName.StartsWith("_") || dirName.StartsWith("."))
                    continue;

                string translationFile = Path.Combine(directory, "translation.json");
                if (!File.Exists(translationFile))
                    continue;

                Logging.Info($"Trying to load \"{translationFile}\"");
                if (TryLoad(translationFile, out JsonFormat lang) && lang != null)
                {
                    lang.RootFolder = directory;
                    if (!allLanguages.ContainsKey(lang.metadata.langName) && lang.metadata.langName != "te-mp")
                    {
                        allLanguages.Add(lang.metadata.langName, lang);
                        if (!allLanguagesDisplayNames.ContainsKey(lang.metadata.langDisplayName))
                            allLanguagesDisplayNames.Add(lang.metadata.langDisplayName, lang);
                    }
                }
            }
        }

        public static void LoadLanguages()
        {
            Logging.Message("Loading language files stored locally on disk...");

            allLanguages = new Dictionary<string, JsonFormat>();

            LoadLanguagesInDirectory(Path.Combine(Paths.ConfigPath, "ultrakull"));

            // English is built into the mod and means "no translation": it plays the game's
            // own text. It is always available so a missing or broken language pack can never
            // leave the game without a valid language. A user-installed en-GB pack takes
            // priority over the built-in one.
            if (!allLanguages.ContainsKey("en-GB"))
            {
                JsonFormat english = LoadBuiltinEnglish();
                allLanguages.Add("en-GB", english);
                if (!allLanguagesDisplayNames.ContainsKey(english.metadata.langDisplayName))
                    allLanguagesDisplayNames.Add(english.metadata.langDisplayName, english);
            }
        }

        /// <summary>
        /// Loads the built-in English template from the embedded resource.
        /// Falls back to file (config/ultrakull/en-GB.json) then to synthetic English.
        /// </summary>
        private static JsonFormat LoadBuiltinEnglish()
        {
            // 1. Try embedded resource (primary, works even without config folder)
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "UltrakULL.json.templates.en-GB.json";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string json = reader.ReadToEnd();
                            JsonFormat english = JsonConvert.DeserializeObject<JsonFormat>(json);
                            if (english != null)
                            {
                                english.RootFolder = null;
                                EnsureNestedObjects(english, 0);
                                Logging.Message("Loaded built-in English template from embedded resource");
                                return english;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to load built-in English from embedded resource: " + ex.Message);
            }

            // 2. Fallback: file in config folder (copied by CopyJSON target)
            string templatePath = Path.Combine(Paths.ConfigPath, "ultrakull", "en-GB.json");
            if (File.Exists(templatePath))
            {
                try
                {
                    string json = File.ReadAllText(templatePath);
                    JsonFormat english = JsonConvert.DeserializeObject<JsonFormat>(json);
                    if (english != null)
                    {
                        english.RootFolder = null;
                        EnsureNestedObjects(english, 0);
                        Logging.Message("Loaded built-in English template from " + templatePath);
                        return english;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to load built-in English template from file: " + ex.Message);
                }
            }

            // 3. Last resort: synthetic English
            Logging.Warn("Falling back to synthetic English");
            return CreateSyntheticEnglish();
        }

        /// <summary>
        /// Builds the built-in "English" (no translation) language entry. All nested objects
        /// are instantiated so translation code that reads them never hits a NullReferenceException;
        /// they contain no text, so English mode leaves the game's original strings untouched.
        /// </summary>
        private static JsonFormat CreateSyntheticEnglish()
        {
            JsonFormat english = new JsonFormat();
            EnsureNestedObjects(english, 0);

            english.metadata.langName = "en-GB";
            english.metadata.langDisplayName = "English";
            english.metadata.langVersion = "1.0";
            english.RootFolder = null;

            // Minimum strings used by the built-in language menu. These are the only labels
            // the menu needs from "English" when no custom English pack is installed.
            english.options.language_title = "Language";
            english.options.language_languages = "Languages";
            english.options.language_openLanguageFolder = "Open Language Folder";

            return english;
        }

        /// <summary>
        /// Recursively instantiates null reference-type fields of a freshly created language object.
        /// Strings and collections (arrays, lists, dictionaries) are left untouched.
        /// </summary>
        private static void EnsureNestedObjects(object target, int depth)
        {
            if (target == null || depth > 8)
                return;

            Type type = target.GetType();
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Type fieldType = field.FieldType;
                if (!fieldType.IsClass || fieldType == typeof(string))
                    continue;
                if (typeof(IEnumerable).IsAssignableFrom(fieldType))
                    continue;

                object value = field.GetValue(target);
                if (value == null)
                {
                    try
                    {
                        value = Activator.CreateInstance(fieldType);
                        field.SetValue(target, value);
                    }
                    catch (Exception ex)
                    {
                        jsonLogger.Log(LogLevel.Warning, $"Could not create {fieldType.Name} for built-in English: {ex.Message}");
                        continue;
                    }
                }

                EnsureNestedObjects(value, depth + 1);
            }
        }

        private static void LoadSubtitledSourcesConfig()
        {
            var config = Encoding.UTF8.GetString(Resources.SubtitledSources);
            SubtitledAudioSourcesReplacer.Config = JsonConvert.DeserializeObject<SubtitledSourcesConfig>(config);
        }


        /*
         Now, when reading the file, if missing keys are found, the mod will not give an error.
        Instead, it will add them to the file.
        Thanks to GitHub Copilot.
        Unfortunately, the logs are not working well at this moment... I'm sorry T_T
        */
        private static bool TryLoad<T>(string pathName, out T file)
        {
            file = default;
            try
            {
                string jsonFile = File.ReadAllText(pathName);
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = (sender, args) =>
                    {
                        // Log the error and mark it as handled
                        jsonLogger.Log(LogLevel.Warning, $"Missing member: {args.ErrorContext.Member}");
                        args.ErrorContext.Handled = true;
                    }
                };
                file = JsonConvert.DeserializeObject<T>(jsonFile, settings);
                if (file != null)
                {
                    T reference = Activator.CreateInstance<T>();
                    string logFileName = $"{Path.GetFileNameWithoutExtension(pathName)}_MISSING_KEYS.log";
                    jsonLogger.Log(LogLevel.Info, $"Starting to add missing keys for {pathName}");
                    AddMissingKeys(reference, file, null, logFileName);
                    string updatedJson = JsonConvert.SerializeObject(file, Formatting.Indented);
                    File.WriteAllText(pathName, updatedJson);
                    jsonLogger.Log(LogLevel.Info, $"Finished adding missing keys for {pathName}");
                }

                return true;
            }
            catch (Exception e)
            {
                jsonLogger.Log(LogLevel.Error, "Failed to load language file " + pathName + ": " + e.Message);
                return false;
            }
        }

        private static void AddMissingKeys(object reference, object target, string parentKey, string logFileName)
        {
            if (reference == null || target == null)
                return;

            Type type = reference.GetType();
            if (type != target.GetType())
                return;

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object refValue = field.GetValue(reference);
                object targetValue = field.GetValue(target);

                if (refValue == null)
                {
                    if (targetValue == null)
                    {
                        if (field.FieldType.IsClass && field.FieldType != typeof(string))
                        {
                            refValue = Activator.CreateInstance(field.FieldType);
                        }
                        field.SetValue(target, refValue);
                        string logMessage = $"Added missing key: {field.Name}";
                        jsonLogger.Log(LogLevel.Info, logMessage);
                        LogToFile(logFileName, logMessage);
                    }
                }
                else if (refValue is IDictionary refDict && targetValue is IDictionary targetDict)
                {
                    foreach (var key in refDict.Keys)
                    {
                        if (!targetDict.Contains(key))
                        {
                            targetDict[key] = refDict[key];
                            string logMessage = $"Added missing key: {key} in dictionary: {field.Name}";
                            jsonLogger.Log(LogLevel.Info, logMessage);
                            LogToFile(logFileName, logMessage);
                        }
                        else
                        {
                            AddMissingKeys(refDict[key], targetDict[key], field.Name, logFileName);
                        }
                    }
                }
                else if (refValue is IList refList && targetValue is IList targetList)
                {
                    for (int i = 0; i < refList.Count; i++)
                    {
                        if (i < targetList.Count)
                        {
                            AddMissingKeys(refList[i], targetList[i], field.Name, logFileName);
                        }
                        else
                        {
                            targetList.Add(refList[i]);
                            string logMessage = $"Added missing item in list: {field.Name}";
                            jsonLogger.Log(LogLevel.Info, logMessage);
                            LogToFile(logFileName, logMessage);
                        }
                    }
                }
                else
                {
                    AddMissingKeys(refValue, targetValue, field.Name, logFileName);
                }
            }
        }

        private static void LogToFile(string logFileName, string message)
        {
            try
            {
                string logDirectory = Path.Combine(Paths.ConfigPath, "ultrakull");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFilePath = Path.Combine(logDirectory, logFileName);
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception e)
            {
                jsonLogger.Log(LogLevel.Error, $"Failed to write log to file {logFileName}: {e.Message}");
            }
        }




        private static JsonFormat CloneLanguage(JsonFormat original)
        {
            string json = JsonConvert.SerializeObject(original);
            JsonFormat clone = JsonConvert.DeserializeObject<JsonFormat>(json);

            // RootFolder is [JsonIgnore], so it is not carried through serialization.
            if (clone != null)
                clone.RootFolder = original.RootFolder;

            return clone;
        }

        private static JsonFormat ApplyRtl(JsonFormat language)
        {
            if (language == null)
            {
                return language;
            }

            List<object> translationComponents = new List<object>
            {
                language.frontend,
                language.tutorial,
                language.prelude,
                language.act1,
                language.act2,
                language.act3,
                language.cyberGrind,
                language.encore,
                language.primeSanctum,
                language.secretLevels,
                language.intermission,
                language.ranks,
                language.pauseMenu,
                language.options,
                language.levelNames,
                language.levelChallenges,
                language.enemyNames,
                language.enemyBios,
                language.shop,
                language.levelTips,
                language.books,
                language.visualnovel,
                language.subtitles,
                language.style,
                language.cheats,
                language.misc,
                language.devMuseum
            };

            ProcessTranslationComponents(translationComponents);
            return language;
        }

        private static void ProcessTranslationComponents(List<object> components)
        {
            foreach (object component in components)
            {
                if (component == null) continue;

                try
                {
                    ProcessComponent(component);
                }
                catch (Exception ex)
                {
                    jsonLogger.LogError($"Ошибка при обработке компонента {component.GetType().Name}: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        jsonLogger.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                    }
                }
            }
        }

        private static void ProcessComponent(object component)
        {
            Type type = component.GetType();
            
            // Обрабатываем поля
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ProcessField(component, field);
            }

            // Обрабатываем свойства
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    ProcessProperty(component, prop);
                }
            }
        }

        private static void ProcessField(object component, FieldInfo field)
        {
            object value = field.GetValue(component);
            if (value == null) return;

            if (value is string stringValue)
            {
                // Обработка строкового значения
                string translated = ArabicFixer.Fix(stringValue);
                if (translated != stringValue)
                {
                    field.SetValue(component, translated);
                }
            }
            else if (value is IEnumerable enumerable && !(value is string))
            {
                // Рекурсивная обработка коллекций
                foreach (object item in enumerable)
                {
                    if (item != null)
                    {
                        ProcessComponent(item);
                    }
                }
            }
            else if (value.GetType().IsClass)
            {
                // Рекурсивная обработка объектов
                ProcessComponent(value);
            }
        }

        private static void ProcessProperty(object component, PropertyInfo prop)
        {
            try
            {
                object value = prop.GetValue(component);
                if (value == null) return;

                if (value is string stringValue)
                {
                    string translated = ArabicFixer.Fix(stringValue);
                    if (translated != stringValue)
                    {
                        prop.SetValue(component, translated);
                    }
                }
                else if (value is IEnumerable enumerable && !(value is string))
                {
                    foreach (object item in enumerable)
                    {
                        if (item != null)
                        {
                            ProcessComponent(item);
                        }
                    }
                }
                else if (value.GetType().IsClass)
                {
                    ProcessComponent(value);
                }
            }
            catch (Exception ex)
            {
                jsonLogger.LogWarning($"Не удалось обработать свойство {prop.Name}: {ex.Message}");
            }
        }

        public static void SetCurrentLanguage(string langName)
        {
            if (CurrentLanguage != null && CurrentLanguage.metadata.langName == langName)
            {
                Logging.Warn("Tried to switch language to " + langName + " but it was already set as that!");
                return;
            }
            if (allLanguages.ContainsKey(langName))
            {
                Logging.Message("Setting language to " + langName);

                CurrentLanguage = CloneLanguage(allLanguages[langName]);
                if (IsRightToLeft)
                {
                    Logging.Message("Language is an RTL - applying fix!");
                    CurrentLanguage = ApplyRtl(CurrentLanguage);
                }

                // Reload custom fonts for the new language
                Core.ReloadCustomFonts();
                TextMeshProFontSwap.ClearFontSwapCache();

                RefreshAssetFolders();
                AudioSwapper.ClearCacheForLanguageChange();
                AudioPreloadManager.OnLanguageChanged();

                // Clear PowerSubtitlesSwap handled clips to prevent stale flags
                // from suppressing original subtitles after language change
                PowerSubtitlesSwap.ClearHandledClips();

                // Apply the new language to the scene that is currently loaded. The
                // game's own text is restored first, so switching to the built-in
                // English brings back the game's own strings and switching to another
                // language never leaves strings of the previous language behind.
                Core.ApplyLanguageToCurrentScene();

                AudioPreloadManager.PreloadCurrentScene();

                // Re-apply subtitles from SubtitledSourcesConfig to all SubtitledAudioSource objects
                SubtitledAudioSourcesReplacer.ReplaceSubsAndAudio();

                // Re-apply audio swaps to all existing boss instances. This runs
                // after the scene preload, so the rebind also hands the original
                // clips of the boss voices to the swap registry - which is why the
                // playing lines are switched last: the registry has to know the
                // lines before they can be replaced in the middle of playing.
                AudioPreloadManager.EnsureCurrentScenePreloaded(delegate
                {
                    UltrakULL.Harmony_Patches.AudioSwaps.GabrielAudioSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.GabrielSecondAudioSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.MinosPrimeAudioSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.SisyphusPrimeAudioSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.MandaloreAudioSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.PowerAudioSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.PowerIntroSwap.RebindExistingInstances();
                    UltrakULL.Harmony_Patches.AudioSwaps.BigJohninatorAudioSwap.RebindExistingInstances();

                    // The voice lines that are in the middle of playing (boss
                    // taunts, hurt sounds, attack calls) switch to the new language
                    // at the same playback time. Sources that hold no line pick up
                    // the new language with their next line on their own.
                    AudioSwapper.ReapplyPlayingVoices();
                });

                DumpLastLanguage();

                //Patch some leftover components that aren't caught in the main change wave...
                CommonFunctions.RefreshLocalizedInputs();
                InjectLanguageButton.updateLanguageButtonText();
                LoadingTextPatch.updateLoadingText();

                string sceneName = GetCurrentSceneName();
                // Skip Bootstrap/Intro: SetCurrentLanguage can run once during startup
                // when the configured language file is broken (en-GB fallback).
                if (sceneName != "Bootstrap" && sceneName != "Intro")
                {
                    // Use the NEW language's strings (CurrentLanguage is already switched here);
                    // empty keys fall back to English.
                    Notifications notif = CurrentLanguage.notifications;
                    string title = NotificationText.Localize(notif?.languageChanged_title,
                        NotificationText.LanguageChangedTitle, "lang", langName);

                    if (sceneName == "Main Menu")
                    {
                        // The menu re-patches everything, so the new language is fully applied.
                        string body = NotificationText.Localize(notif?.languageChanged_menuBody,
                            NotificationText.LanguageChangedMenuBody);
                        NotificationRouter.Current.Success(title, body, 5f);
                    }
                    else
                    {
                        string body = NotificationText.Localize(notif?.languageChanged_levelBody,
                            NotificationText.LanguageChangedLevelBody);
                        NotificationRouter.Current.Warning(title, body, 6f);
                    }
                }
            }
            else
                Logging.Warn("No language found with name " + langName);
        }

        /// <summary>
        /// Clears any caches used by the language manager.
        /// </summary>
        public static void ClearCaches()
        {
            // Currently no caches to clear, but method is kept for compatibility.
        }
    }
}
