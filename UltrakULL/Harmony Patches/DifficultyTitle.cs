using HarmonyLib;
using UnityEngine.UI;
using UltrakULL.json;
using System;
using TMPro;
using static UltrakULL.CommonFunctions;
using UnityEngine;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides Check from the vanilla game. Used for persistant difficulty strings across all scenes.
    [HarmonyPatch(typeof(DifficultyTitle), "Check")]
    public static class LocalizeGameProgressCheck
    {
        /// <summary>
        /// Builds the difficulty title text for the given difficulty index and the
        /// active language.
        /// </summary>
        private static string GetDifficultyText(int @int, bool lines)
        {
            string text = "";
            if (lines)
            {
                text += "-- ";
            }

            if (isUsingEnglish())
            {
                switch (@int)
                {
                    case 0:
                        text += "HARMLESS";
                        break;
                    case 1:
                        text += "LENIENT";
                        break;
                    case 2:
                        text += "STANDARD";
                        break;
                    case 3:
                        text += "VIOLENT";
                        break;
                    case 4:
                        text += "BRUTAL";
                        break;
                    case 5:
                        text += "ULTRAKILL MUST DIE";
                        break;
                }
            }
            else
            {
                switch (@int)
                {
                    case 0:
                        text += LanguageManager.CurrentLanguage.frontend.difficulty_harmless;
                        break;
                    case 1:
                        text += LanguageManager.CurrentLanguage.frontend.difficulty_lenient;
                        break;
                    case 2:
                        text += LanguageManager.CurrentLanguage.frontend.difficulty_standard;
                        break;
                    case 3:
                        text += LanguageManager.CurrentLanguage.frontend.difficulty_violent;
                        break;
                    case 4:
                        text += LanguageManager.CurrentLanguage.frontend.difficulty_brutal;
                        break;
                    case 5:
                        text += LanguageManager.CurrentLanguage.frontend.difficulty_umd;
                        break;
                }
            }

            if (lines)
            {
                text += " --";
            }

            return text;
        }

        /// <summary>
        /// Applies the active language to every difficulty title that is on screen.
        ///
        /// The difficulty title is written only when the difficulty changes, so a
        /// language switch did not reach it and it kept the previous language until
        /// the difficulty was changed again.
        /// </summary>
        public static void Reapply()
        {
            try
            {
                int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 0);
                string text = GetDifficultyText(@int, true);

                DifficultyTitle[] titles = UnityEngine.Object.FindObjectsOfType<DifficultyTitle>(true);
                for (int i = 0; i < titles.Length; i++)
                {
                    DifficultyTitle title = titles[i];
                    if (title == null)
                    {
                        continue;
                    }

                    TMP_Text txt2 = title.GetComponent<TMP_Text>();
                    if (txt2 != null)
                    {
                        txt2.text = text;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to re-apply difficulty titles.");
                Logging.Error(e.ToString());
            }
        }

        [HarmonyPrefix]
        public static bool Check_MyPatch(DifficultyTitle __instance, ref TMP_Text ___txt2)
        {
            try
            {
                int @int = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 0);
                string text = GetDifficultyText(@int, __instance.lines);

                if (!___txt2)
                {
                    ___txt2 = __instance.GetComponent<TMP_Text>();
                }

                if (___txt2)
                {
                    ___txt2.text = text;
                    return false;
                }

                return false;
            }
            catch (Exception e)
            {
                Logging.Error("Failed to get difficulty strings.");
                Logging.Error(e.ToString());
                return true;
            }
        }
    }
}
