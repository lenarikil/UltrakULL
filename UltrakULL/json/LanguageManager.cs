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
        public static bool IsRightToLeft { get => CurrentLanguage.metadata.rightToLeft; }
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

            string langName = (lang != null && lang.metadata != null) ? lang.metadata.languageCode : "en-GB";
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

            // LastLanguage stores the pack key (folder name). A plain language code
            // still resolves when only one pack of that locale exists, so old
            // configs keep working.
            JsonFormat storedLanguage;
            if (TryResolveLanguage(value, out storedLanguage))
            {
                jsonLogger.Log(LogLevel.Message, "Setting language to " + value);
                CurrentLanguage = CloneLanguage(storedLanguage);
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
            // Store the pack key (folder name): it is the unique physical identity
            // of the installation, while languageCode no longer is (several packs
            // can share one locale).
            configFile.Bind("General", "LastLanguage", "en-GB").Value =
                !string.IsNullOrEmpty(CurrentLanguage.PackKey) ? CurrentLanguage.PackKey : CurrentLanguage.metadata.languageCode;
        }

        /// <summary>
        /// Resolves a language key to a loaded pack: first an exact dictionary-key
        /// match (pack folder name), then a fallback by languageCode so callers
        /// that only know the locale ("en-GB", "ru-RU") keep working.
        /// </summary>
        private static bool TryResolveLanguage(string key, out JsonFormat language)
        {
            language = null;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (allLanguages.TryGetValue(key, out language))
            {
                return true;
            }

            foreach (KeyValuePair<string, JsonFormat> pair in allLanguages)
            {
                JsonFormat candidate = pair.Value;
                if (candidate != null && candidate.metadata != null &&
                    string.Equals(candidate.metadata.languageCode, key, StringComparison.OrdinalIgnoreCase))
                {
                    language = candidate;
                    return true;
                }
            }
            return false;
        }

        public static void LoadLanguagesInDirectory(string path)
        {
            Logging.Info($"Loading all language files in \"{path}\"");

            if (!Directory.Exists(path))
                return;

            // One folder = one language pack. The dictionary key is the folder
            // name: several packs may translate the same locale (each in its own
            // folder), so the languageCode can no longer be the key.
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
                    lang.PackKey = dirName;

                    // The manifest is mandatory: a pack without one (a manual
                    // install) gets it generated from its own metadata. Failures
                    // only log - readers fall back to the metadata.
                    ManifestMigrator.MigrateIfNeeded(directory, lang.metadata);

                    if (!allLanguages.ContainsKey(dirName) && dirName != "te-mp")
                    {
                        allLanguages.Add(dirName, lang);
                        if (!allLanguagesDisplayNames.ContainsKey(lang.metadata.languageName))
                            allLanguagesDisplayNames.Add(lang.metadata.languageName, lang);
                    }
                }
            }
        }

        public static void LoadLanguages()
        {
            Logging.Message("Loading language files stored locally on disk...");

            // A full rescan (also at runtime, from the browser's refresh): both maps
            // are rebuilt from scratch so packs that were removed stop being listed.
            allLanguages = new Dictionary<string, JsonFormat>();
            allLanguagesDisplayNames = new Dictionary<string, JsonFormat>();
            BuiltinEnglishManifest = null;

            LoadLanguagesInDirectory(Path.Combine(Paths.ConfigPath, "ultrakull"));

            // English is built into the mod and means "no translation": it plays the game's
            // own text. It is always available so a missing or broken language pack can never
            // leave the game without a valid language. A user-installed en-GB pack takes
            // priority over the built-in one.
            if (!HasPackWithLanguageCode("en-GB"))
            {
                JsonFormat english = LoadBuiltinEnglish();
                english.PackKey = "en-GB";
                allLanguages.Add("en-GB", english);
                if (!allLanguagesDisplayNames.ContainsKey(english.metadata.languageName))
                    allLanguagesDisplayNames.Add(english.metadata.languageName, english);

                // The built-in English has no folder on disk, so its mandatory
                // manifest only exists in memory (source "builtin").
                BuiltinEnglishManifest = ManifestMigrator.CreateBuiltinManifest(
                    "English", "en-GB", english.metadata.translationVersion);
            }
        }

        /// <summary>
        /// In-memory manifest of the built-in English pseudo-pack, or null when
        /// a user-installed en-GB pack replaced the built-in one.
        /// </summary>
        public static TranslationManifest BuiltinEnglishManifest;

        /// <summary>True when any loaded pack declares the given languageCode.</summary>
        private static bool HasPackWithLanguageCode(string languageCode)
        {
            foreach (KeyValuePair<string, JsonFormat> pair in allLanguages)
            {
                JsonFormat pack = pair.Value;
                if (pack != null && pack.metadata != null &&
                    string.Equals(pack.metadata.languageCode, languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
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

            english.metadata.languageCode = "en-GB";
            english.metadata.languageName = "English (DEFAULT)";
            english.metadata.translationVersion = "1.0";
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
         Instead, it will add them to the file with the value from the built-in English
         template, so an outdated pack falls back to English text instead of null.
         Thanks to GitHub Copilot.
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
                    // The fill reference is the built-in English template: every key
                    // the pack does not have gets the English value, so an outdated
                    // pack shows English instead of crashing on a null string later.
                    // A fresh template is loaded per pack, so no section instance is
                    // shared between languages.
                    object reference;
                    if (file is JsonFormat)
                    {
                        reference = LoadBuiltinEnglish();
                    }
                    else
                    {
                        reference = Activator.CreateInstance<T>();
                    }

                    int filledKeys = AddMissingKeys(reference, file, null);
                    if (filledKeys > 0)
                    {
                        jsonLogger.Log(LogLevel.Warning, $"Filled {filledKeys} missing key(s) from the English template in {pathName}");
                    }

                    string updatedJson = JsonConvert.SerializeObject(file, Formatting.Indented);
                    File.WriteAllText(pathName, updatedJson);
                }

                return true;
            }
            catch (Exception e)
            {
                jsonLogger.Log(LogLevel.Error, "Failed to load language file " + pathName + ": " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Copies every value the pack does not have from the reference (the built-in
        /// English template) into the loaded language, and reports each key it fills.
        /// Sections the pack does not have are created and filled key by key, so the
        /// pack keeps its own values everywhere else. Returns the number of filled
        /// keys so the caller can log a summary.
        /// </summary>
        private static int AddMissingKeys(object reference, object target, string parentKey)
        {
            if (reference == null || target == null)
                return 0;

            Type type = reference.GetType();
            if (type != target.GetType())
                return 0;

            // Strings, primitives and enums are leaf values, not containers. Their
            // private implementation fields (String.m_stringLength, Int32.m_value,
            // which refers to itself on a boxed value) must never be walked: the
            // recursion would not terminate.
            if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
                return 0;

            int filledKeys = 0;

            // Collections are handled structurally, never through their fields:
            // the private bookkeeping of BCL collections (Dictionary.keys is a lazy
            // KeyCollection without a public constructor, Entry structs, comparers)
            // is not translation data.
            if (reference is IDictionary refDict && target is IDictionary targetDict)
            {
                foreach (var key in refDict.Keys)
                {
                    if (!targetDict.Contains(key))
                    {
                        targetDict[key] = refDict[key];
                        filledKeys++;
                        jsonLogger.Log(LogLevel.Warning, $"Filled missing key '{parentKey}[{key}]' from the English template");
                    }
                    else
                    {
                        filledKeys += AddMissingKeys(refDict[key], targetDict[key], parentKey + "[" + key + "]");
                    }
                }

                return filledKeys;
            }

            if (reference is IList refList && target is IList targetList)
            {
                for (int i = 0; i < refList.Count; i++)
                {
                    if (i < targetList.Count)
                    {
                        filledKeys += AddMissingKeys(refList[i], targetList[i], parentKey);
                    }
                    else
                    {
                        targetList.Add(refList[i]);
                        filledKeys++;
                    }
                }

                return filledKeys;
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object refValue = field.GetValue(reference);
                object targetValue = field.GetValue(target);
                string fullKey = string.IsNullOrEmpty(parentKey) ? field.Name : parentKey + "." + field.Name;

                if (targetValue == null && refValue != null)
                {
                    // The pack does not have the key: the template's value is taken,
                    // so an outdated pack falls back to English instead of a null
                    // string that crashes the patch that reads it.
                    field.SetValue(target, refValue);
                    filledKeys++;
                    jsonLogger.Log(LogLevel.Warning, $"Filled missing key '{fullKey}' from the English template");
                }
                else if (targetValue == null)
                {
                    // Neither side has a value: a section is still created, so the
                    // rest of the mod sees a valid object instead of null.
                    if (field.FieldType.IsClass && field.FieldType != typeof(string))
                    {
                        try
                        {
                            field.SetValue(target, Activator.CreateInstance(field.FieldType));
                        }
                        catch (Exception)
                        {
                            // Not every class can be instantiated blindly; the field
                            // stays null, exactly like it was before the sync.
                        }
                    }
                }
                else
                {
                    filledKeys += AddMissingKeys(refValue, targetValue, fullKey);
                }
            }

            return filledKeys;
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

            // RootFolder and PackKey are [JsonIgnore], so they are not carried
            // through serialization.
            if (clone != null)
            {
                clone.RootFolder = original.RootFolder;
                clone.PackKey = original.PackKey;
            }

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
            // Resolve through the two-step lookup (pack key first, then
            // languageCode) so both folder names and plain locale codes work.
            JsonFormat target;
            if (!TryResolveLanguage(langName, out target))
            {
                Logging.Warn("No language found with name " + langName);
                return;
            }

            if (CurrentLanguage != null && CurrentLanguage.metadata != null &&
                !string.IsNullOrEmpty(target.PackKey) &&
                string.Equals(CurrentLanguage.PackKey, target.PackKey, StringComparison.OrdinalIgnoreCase))
            {
                Logging.Warn("Tried to switch language to " + langName + " but it was already set as that!");
                return;
            }

            Logging.Message("Setting language to " + langName);

            CurrentLanguage = CloneLanguage(target);
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

            // Rewrite the subtitle data and the clips of the subtitled sources
            // right away, without waiting for the scene preload: a line that is
            // in the middle of playing (the Power intro of 8-3) would keep the
            // previous language until the preload finished, and its upcoming
            // subtitle would already be gone. The clip loads stream in on their
            // own while the preload fills the cache for the rest of the scene.
            SubtitledAudioSourcesReplacer.ApplySubsAndAudio();

            // Sources the config entries cannot reach anymore - the waves of 8-3
            // spawn copies of the configured intro objects that carry their own
            // subtitle data - follow through their already localized lines.
            LiveSubtitles.ReapplyPendingLines();

            // The voice lines that are in the middle of playing switch right away
            // at the same playback time. The registry maps the playing clip to
            // its translation path; the clips load without waiting for the scene
            // preload.
            AudioSwapper.ReapplyPlayingVoices();

            // Re-apply audio swaps to all existing boss instances. This runs
            // after the scene preload, so the rebind also hands the original
            // clips of the boss voices to the swap registry - which is why the
            // playing lines are switched again last: when switching away from
            // English, the registry only learns the lines here.
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

                // The second pass reaches the sources that the first one could
                // not resolve yet (see above): sources that already hold the new
                // language are skipped, so this is a no-op for them.
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

                // Show the human-readable translation name (not the code and not
                // the folder name) in the "language changed" toast.
                string displayName = !string.IsNullOrEmpty(CurrentLanguage.metadata.languageName)
                    ? CurrentLanguage.metadata.languageName
                    : (CurrentLanguage.PackKey ?? langName);
                string title = NotificationText.Localize(notif?.languageChanged_title,
                    NotificationText.LanguageChangedTitle, "lang", displayName);

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

        /// <summary>
        /// Clears any caches used by the language manager.
        /// </summary>
        public static void ClearCaches()
        {
            // Currently no caches to clear, but method is kept for compatibility.
        }
    }
}
