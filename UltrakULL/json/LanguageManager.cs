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
using UnityEngine.SceneManagement;
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
        #endregion

        public static void InitializeManager()
        {
            LoadLanguages();

            configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ultrakull", "lastLang.cfg"), true);

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
                CurrentLanguage = allLanguages["en-GB"];
                SetCurrentLanguage("en-GB");
            }

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

            string[] files = Directory.GetFiles(path, "*.json");
            string[] subdirectories = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                Logging.Info($"Trying to load \"{file}\"");
                if (TryLoad(file, out JsonFormat lang) && !allLanguages.ContainsKey(lang.metadata.langName) && lang.metadata.langName != "te-mp")
                {
                    allLanguages.Add(lang.metadata.langName, lang);
                    allLanguagesDisplayNames.Add(lang.metadata.langDisplayName, lang);
                }
            }

        }

        public static void LoadLanguages()
        {
            Logging.Message("Loading language files stored locally on disk...");

            allLanguages = new Dictionary<string, JsonFormat>();

            LoadLanguagesInDirectory(Path.Combine(Paths.ConfigPath, "ultrakull"));
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
            return JsonConvert.DeserializeObject<JsonFormat>(json);
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

                AudioSwapper.SpeechFolder = Path.Combine(Paths.ConfigPath, "ultrakull", "audio", CurrentLanguage.metadata.langName) + Path.DirectorySeparatorChar;
                SubtitledAudioSourcesReplacer.SpeechFolder = AudioSwapper.SpeechFolder;
                AudioSwapper.ClearCacheForLanguageChange();
                AudioPreloadManager.OnLanguageChanged();

                MainPatch.Instance.onSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
                AudioPreloadManager.PreloadCurrentScene();
                DumpLastLanguage();

                //Patch some leftover components that aren't caught in the main change wave...
                CommonFunctions.RefreshLocalizedInputs();
                InjectLanguageButton.updateLanguageButtonText();
                LoadingTextPatch.updateLoadingText();

                if (GetCurrentSceneName() != "Main Menu")
                {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>Language changes will not fully take effect until the current mission is quit or restarted.</color>");
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
