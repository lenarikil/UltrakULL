using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UltrakULL.json;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the CreateButtons function from the SpawnMenu class for translating spawner arm menu sections.
    [HarmonyPatch(typeof(SpawnMenu), "CreateButtons")]
    public static class SpawnMenuPatch
    {
        private static readonly Dictionary<TMP_Text, string> sectionNames = new Dictionary<TMP_Text, string>();

        [HarmonyPrefix]
        public static bool CreateButtons_Prefix(SpawnableObject[] list, ref string sectionName, ref SpawnMenu __instance)
        {
            sectionName = TranslateSectionName(sectionName);
            return true;
        }

        [HarmonyPostfix]
        public static void CreateButtons_Postfix(string sectionName, SpawnMenu __instance)
        {
            if (__instance == null)
                return;

            TMP_Text[] texts = __instance.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text == null || text.text != sectionName || sectionNames.ContainsKey(text))
                    continue;

                string original = GetEnglishSectionName(sectionName);
                if (original != null)
                    sectionNames[text] = original;
                return;
            }
        }

        /// <summary>
        /// Rewrites only the headings of an already open spawn menu. RebuildMenu
        /// destroys every button, which is unnecessarily disruptive on a language
        /// change and can visibly stall the sandbox UI.
        /// </summary>
        public static void Reapply()
        {
            List<TMP_Text> dead = new List<TMP_Text>();
            foreach (KeyValuePair<TMP_Text, string> pair in sectionNames)
            {
                if (pair.Key == null)
                {
                    dead.Add(pair.Key);
                    continue;
                }

                // The headings that are not on screen are written as well: they belong
                // to sections that are built once and shown later.
                pair.Key.text = TranslateSectionName(pair.Value);
            }

            for (int i = 0; i < dead.Count; i++)
                sectionNames.Remove(dead[i]);
        }

        private static string TranslateSectionName(string name)
        {
            if (isUsingEnglish())
                return name;

            switch (name)
            {
                case "SANDBOX TOOLS :^)": return LanguageManager.CurrentLanguage.misc.spawner_sandboxTools;
                case "SANDBOX": return LanguageManager.CurrentLanguage.misc.spawner_sandbox;
                case "ENEMIES": return LanguageManager.CurrentLanguage.misc.spawner_enemies;
                case "ITEMS": return LanguageManager.CurrentLanguage.misc.spawner_items;
                case "SPECIAL": return LanguageManager.CurrentLanguage.misc.spawner_special;
                case "UNLOCKABLES": return LanguageManager.CurrentLanguage.misc.spawner_unlockables;
                default: return name;
            }
        }

        private static string GetEnglishSectionName(string currentName)
        {
            string[] originals =
            {
                "SANDBOX TOOLS :^)", "SANDBOX", "ENEMIES", "ITEMS", "SPECIAL", "UNLOCKABLES"
            };

            for (int i = 0; i < originals.Length; i++)
            {
                string original = originals[i];
                if (currentName == original || currentName == TranslateSectionName(original))
                    return original;
            }

            return null;
        }

    }
}
