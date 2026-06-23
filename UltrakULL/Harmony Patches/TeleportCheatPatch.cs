using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(TeleportCheat))]
    public static class TeleportCheatPatch
    {
        private static HashSet<TeleportCheat> _processed = new HashSet<TeleportCheat>();

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void UpdatePrefix(TeleportCheat __instance)
        {
            if (!_processed.Contains(__instance))
            {
                _processed.Add(__instance);
                AutoPopulateKeys(__instance);
            }

            ApplyTranslations(__instance);
        }

        private static void ApplyTranslations(TeleportCheat __instance)
        {
            string sceneName = GetCurrentSceneName();
            var buttonTemplate = AccessTools.Field(typeof(TeleportCheat), "buttonTemplate").GetValue(__instance) as GameObject;
            if (buttonTemplate == null) return;

            Transform parent = buttonTemplate.transform.parent;
            if (parent == null) return;

            var teleportLevels = LanguageManager.CurrentLanguage.misc.teleportLevels;
            if (teleportLevels == null) return;
            if (!teleportLevels.TryGetValue(sceneName, out var levelTeleports)) return;

            foreach (Transform child in parent)
            {
                if (!child.gameObject.activeSelf) continue;
                var tmp = child.GetComponentInChildren<TMP_Text>();
                if (tmp == null || string.IsNullOrEmpty(tmp.text)) continue;

                if (levelTeleports.TryGetValue(tmp.text, out string translation) && !string.IsNullOrEmpty(translation))
                {
                    tmp.text = translation;
                }
            }
        }

        private static void AutoPopulateKeys(TeleportCheat __instance)
        {
            try
            {
                string sceneName = GetCurrentSceneName();
                var buttonTemplate = AccessTools.Field(typeof(TeleportCheat), "buttonTemplate").GetValue(__instance) as GameObject;
                if (buttonTemplate == null) return;

                Transform parent = buttonTemplate.transform.parent;
                if (parent == null) return;

                string langFilePath = Path.Combine(Paths.ConfigPath, "ultrakull", LanguageManager.CurrentLanguage.metadata.langName + ".json");
                if (!File.Exists(langFilePath)) return;

                string json = File.ReadAllText(langFilePath);
                var root = JObject.Parse(json);
                var misc = root["misc"] as JObject;
                if (misc == null) return;

                var levels = misc["teleportLevels"] as JObject;
                if (levels == null)
                {
                    levels = new JObject();
                    misc["teleportLevels"] = levels;
                }

                var sceneObj = levels[sceneName] as JObject;
                if (sceneObj == null)
                {
                    sceneObj = new JObject();
                    levels[sceneName] = sceneObj;
                }

                bool changed = false;
                foreach (Transform child in parent)
                {
                    if (!child.gameObject.activeSelf) continue;
                    var tmp = child.GetComponentInChildren<TMP_Text>();
                    if (tmp == null || string.IsNullOrEmpty(tmp.text)) continue;

                    if (sceneObj[tmp.text] == null)
                    {
                        sceneObj[tmp.text] = "";
                        changed = true;
                    }
                }

                if (changed)
                {
                    File.WriteAllText(langFilePath, root.ToString(Formatting.Indented));

                    var dict = LanguageManager.CurrentLanguage.misc.teleportLevels;
                    if (dict == null)
                    {
                        dict = new Dictionary<string, Dictionary<string, string>>();
                        LanguageManager.CurrentLanguage.misc.teleportLevels = dict;
                    }
                    if (!dict.ContainsKey(sceneName))
                    {
                        dict[sceneName] = new Dictionary<string, string>();
                    }
                    foreach (Transform child in parent)
                    {
                        if (!child.gameObject.activeSelf) continue;
                        var tmp = child.GetComponentInChildren<TMP_Text>();
                        if (tmp == null || string.IsNullOrEmpty(tmp.text)) continue;
                        if (!dict[sceneName].ContainsKey(tmp.text))
                        {
                            dict[sceneName][tmp.text] = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warn($"[TeleportCheatPatch] Auto-populate failed: {ex.Message}");
            }
        }
    }
}
