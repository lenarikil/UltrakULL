using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(LevelNameFinder), "OnEnable")]
    public static class LevelNameFinderTranslation
    {
        /// <summary>
        /// The text the game itself wrote into each level-name text. It is kept, so
        /// the built-in English can be put back on a language switch.
        /// </summary>
        private static readonly Dictionary<LevelNameFinder, string> originalLevelTexts = new Dictionary<LevelNameFinder, string>();

        [HarmonyPostfix]
        public static void OnEnable_Postfix(LevelNameFinder __instance, TMP_Text ___txt2)
        {
            if (isUsingEnglish())
                return;

            if (___txt2 == null)
                return;

            if (GetPath(__instance.transform).Contains("FinishCanvas"))
                return;

            // Remember the game's original text so we can re-apply after a language
            // switch. The component text is already translated after the first pass,
            // so we must capture it before overwriting.
            if (!originalLevelTexts.ContainsKey(__instance))
            {
                originalLevelTexts[__instance] = ___txt2.text;
            }

            ___txt2.text = "<color=red>" +
                           LanguageManager.CurrentLanguage.shop.shop_returningTo +
                           ":</color>\n" +
                           LevelNames.GetLevelName(__instance.otherLevelNumber);
        }

        /// <summary>
        /// Applies the active language to every level-name text that is on screen.
        ///
        /// The game fills each level name only once, when it appears, so a language
        /// switch did not reach them and they kept the previous language until the
        /// screen was closed and opened again.
        /// </summary>
        public static void Reapply()
        {
            LevelNameFinder[] finders = UnityEngine.Object.FindObjectsOfType<LevelNameFinder>(true);
            for (int i = 0; i < finders.Length; i++)
            {
                LevelNameFinder finder = finders[i];
                if (finder == null)
                {
                    continue;
                }

                TMP_Text txt2 = AccessTools.Field(typeof(LevelNameFinder), "txt2")?.GetValue(finder) as TMP_Text;
                if (txt2 == null)
                {
                    continue;
                }

                if (GetPath(finder.transform).Contains("FinishCanvas"))
                {
                    continue;
                }

                string original;
                if (!originalLevelTexts.TryGetValue(finder, out original))
                {
                    original = txt2.text;
                    originalLevelTexts[finder] = original;
                }

                try
                {
                    if (isUsingEnglish())
                    {
                        // The built-in English is the game's own text.
                        txt2.text = original;
                    }
                    else
                    {
                        txt2.text = "<color=red>" +
                                    LanguageManager.CurrentLanguage.shop.shop_returningTo +
                                    ":</color>\n" +
                                    LevelNames.GetLevelName(finder.otherLevelNumber);
                    }
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }
        }
        private static string GetPath(Transform current)
        {
            string path = current.name;

            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }
    }
}
