using HarmonyLib;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;


namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(SceneHelper),"OnSceneLoaded")]
    public class LoadingTextPatch
    {
        public static TextMeshProUGUI loadingText;

        /// <summary>
        /// The text the game itself puts on the loading blocker. It is remembered
        /// before the mod writes its translation, so the built-in English can be put
        /// back on a language switch.
        /// </summary>
        private static string originalText;

        /// <summary>
        /// Writes the active language into the loading blocker.
        ///
        /// The game enables the blocker before it loads the scene, so the text has to
        /// be translated before the blocker becomes visible. Writing it after the
        /// scene was loaded was too late, the player only ever saw the game's own
        /// English while the scene was loading.
        /// </summary>
        public static void Translate(GameObject loadingBlocker)
        {
            if (loadingBlocker == null)
                return;

            if (loadingText == null)
            {
                loadingText = loadingBlocker.FindPath("Panel", "Text").TMP();
                if (loadingText == null)
                    return;

                // The blocker still shows the game's own text at this point.
                originalText = loadingText.text;
            }

            ApplyLanguage(loadingText);
        }

        /// <summary>
        /// Writes the active language into the loading blocker again. Used after a
        /// language switch, so a blocker that is on screen shows the new language.
        /// </summary>
        public static void updateLoadingText()
        {
            ApplyLanguage(loadingText);
        }

        private static void ApplyLanguage(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            if (isUsingEnglish())
            {
                // The built-in English is the game's own text.
                if (!string.IsNullOrEmpty(originalText))
                {
                    text.text = originalText;
                }
                return;
            }

            text.text = LanguageManager.CurrentLanguage.misc.loading;
        }

        [HarmonyPostfix]
        public static void LoadingTextPatch_Postfix(Scene scene, LoadSceneMode mode, ref SceneHelper __instance, ref GameObject ___loadingBlocker)
        {
            Translate(___loadingBlocker);
        }
    }

    /// <summary>
    /// The game enables the loading blocker before it loads the scene, so the text of
    /// the blocker is translated before it becomes visible.
    /// </summary>
    [HarmonyPatch(typeof(SceneHelper), "LoadSceneCoroutine")]
    public class LoadingBlockerTextPatch
    {
        [HarmonyPrefix]
        public static void LoadSceneCoroutine_Prefix(ref GameObject ___loadingBlocker)
        {
            LoadingTextPatch.Translate(___loadingBlocker);
        }
    }
}
