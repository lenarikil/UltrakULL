using System;
using HarmonyLib;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the NameAppear function from LevelNamePopup. Used for showing layer and level names at the start of a level.
    [HarmonyPatch(typeof(LevelNamePopup), "NameAppear")]
    public static class LocalizeLevelPopup
    {
        /// <summary>
        /// The names the game itself reveals last. They are remembered when the reveal
        /// starts, so a language switch can translate them again. A custom reveal
        /// shows the names of the game without a translation, so it clears them.
        /// </summary>
        private static string gameRevealLayer;
        private static string gameRevealName;

        [HarmonyPrefix]
        public static bool NameAppear_MyPatch(LevelNamePopup __instance, ref string ___layerString, ref string ___nameString)
        {
            // The strings still hold the names of the game here, they are the baseline
            // every language works from. They are remembered even for the built-in
            // English, so a switch away from it can translate the reveal that is
            // already typing.
            gameRevealLayer = ___layerString;
            gameRevealName = ___nameString;

            if(isUsingEnglish())
            {
                return true;
            }
            try
            {
                if (TitleManager.GetName(___nameString) == null)
                {
                    Logging.Warn("There's no translated level name here!");
                    Logging.Warn("Layer Name is:" + ___layerString);
                    Logging.Warn("Level Name is:" + ___nameString);
                    return true;
                }
                ___layerString = TitleManager.GetLayer(___layerString);
                ___nameString = TitleManager.GetName(___nameString);
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to Patch Level Name Popup!");
                Logging.Warn(e.ToString());
            }
            return true;
        }

        [HarmonyPatch(typeof(LevelNamePopup), "CustomNameAppear"), HarmonyPostfix]
        public static void CustomNameAppear_Postfix()
        {
            // A custom reveal is not translated, so there is nothing to switch.
            gameRevealLayer = null;
            gameRevealName = null;
        }

        /// <summary>
        /// Applies the active language to the title reveal of the level.
        ///
        /// The reveal types the layer and the level name letter by letter, so a
        /// language switch did not reach it and it kept typing the previous language
        /// until the reveal was over. The strings of the reveal are swapped for the new
        /// language, and the reveal keeps typing from the letter it is on, so it does
        /// not start over. The part that is already fully typed is swapped as a whole.
        /// </summary>
        public static void Reapply()
        {
            LevelNamePopup popup = MonoSingleton<LevelNamePopup>.Instance;
            if (popup == null || gameRevealLayer == null || gameRevealName == null)
            {
                // No reveal of the game is on screen, or it is a custom one.
                return;
            }

            if (popup.layerText == null || popup.nameText == null)
            {
                return;
            }

            if (popup.layerText.text.Length == 0 && popup.nameText.text.Length == 0)
            {
                // The reveal has not started typing yet, it is translated when it does.
                return;
            }

            try
            {
                string newLayer;
                string newName;
                if (isUsingEnglish())
                {
                    // The built-in English is the game's own text.
                    newLayer = gameRevealLayer;
                    newName = gameRevealName;
                }
                else
                {
                    newLayer = TitleManager.GetLayer(gameRevealLayer);
                    newName = TitleManager.GetName(gameRevealName);
                    if (string.IsNullOrEmpty(newLayer) || string.IsNullOrEmpty(newName))
                    {
                        // The level has no translation, the reveal keeps what it has.
                        return;
                    }
                }

                string oldLayer = ReflectionUtils.GetPrivate<LevelNamePopup, string>(popup, typeof(LevelNamePopup), "layerString");
                string oldName = ReflectionUtils.GetPrivate<LevelNamePopup, string>(popup, typeof(LevelNamePopup), "nameString");

                ReflectionUtils.SetPrivate(popup, typeof(LevelNamePopup), "layerString", newLayer);
                ReflectionUtils.SetPrivate(popup, typeof(LevelNamePopup), "nameString", newName);

                // The name that is being typed right now continues from its letter on
                // its own. The one that is fully typed already is swapped here.
                if (popup.layerText.text == oldLayer)
                {
                    popup.layerText.text = newLayer;
                }

                if (popup.nameText.text == oldName)
                {
                    popup.nameText.text = newName;
                }
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to apply the active language to the level title reveal!");
                Logging.Warn(e.ToString());
            }
        }
    }
}
