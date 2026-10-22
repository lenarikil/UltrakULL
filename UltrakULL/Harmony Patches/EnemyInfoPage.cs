using HarmonyLib;
using UnityEngine.UI;
using UltrakULL.json;
using System;
using TMPro;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the DisplayInfo method from the EnemyInfoPage class. This is to allow swapping out of monster bios in the shop.
    [HarmonyPatch(typeof(EnemyInfoPage), "DisplayInfo", new Type[] { typeof(SpawnableObject) })]
    public static class LocalizeEnemyInfo
    {
        // The bio that is currently open. The game renders it only once, when an
        // enemy is picked, so it is remembered here to be able to write the new
        // language into it after a switch.
        private static SpawnableObject openSource;
        private static TMP_Text openTitle;
        private static TMP_Text openContent;
        private static TMP_Text openEntryTitle;

        /// <summary>The title the game wrote, before the mod took the component over.</summary>
        private static string openGameTitle;
        private static string openGameContent;
        private static string openGameEntryTitle;

        [HarmonyPostfix]
        public static void DisplayInfo_Postfix(SpawnableObject source, EnemyInfoPage __instance, TMP_Text ___enemyPageTitle, TMP_Text ___enemyPageContent, TMP_Text ___enemyEntryTitle)
        {
            // The bio is remembered even when the built-in English is active, so a
            // switch to another language can translate the bio that is open.
            openSource = source;
            openTitle = ___enemyPageTitle;
            openContent = ___enemyPageContent;
            openEntryTitle = ___enemyEntryTitle;
            openGameTitle = ___enemyPageTitle.text;
            openGameContent = ___enemyPageContent != null ? ___enemyPageContent.text : null;
            openGameEntryTitle = ___enemyEntryTitle != null ? ___enemyEntryTitle.text : null;

            if (isUsingEnglish())
            {
                return;
            }

            ApplyTranslations(source, ___enemyPageTitle, ___enemyPageContent, ___enemyEntryTitle, openGameTitle);
        }

        /// <summary>
        /// Writes the bio of the given enemy in the active language.
        /// </summary>
        private static void ApplyTranslations(SpawnableObject source, TMP_Text enemyPageTitle, TMP_Text enemyPageContent, TMP_Text enemyEntryTitle, string gameTitle)
        {
            if (source == null || enemyPageTitle == null)
            {
                return;
            }
            string enemyName;
            if (gameTitle == "SWORDSMACHINE" || gameTitle == "INSURRECTIONIST")
            {
                source.objectName += " DEFAULT";
                enemyName = EnemyBios.GetName(source.objectName);
                if (source.objectName == "SWORDSMACHINE DEFAULT") { source.objectName = "SWORDSMACHINE"; }
                if (source.objectName == "INSURRECTIONIST DEFAULT") { source.objectName = "INSURRECTIONIST"; }
            }
            else { enemyName = EnemyBios.GetName(source.objectName); }

            Logging.Warn("Enemy Name in SHOP: " + enemyName);
            string enemyType = EnemyBios.GetType(source.type);
            string enemyDescription = EnemyBios.GetDescription(source.objectName);
            string enemyStrategy = EnemyBios.GetStrategy(source.objectName);
            

            enemyPageTitle.text = enemyName;
            if (enemyEntryTitle != null)
            {
                enemyEntryTitle.text = enemyName;
            }
            string text = "</s><color=#FF4343>" + LanguageManager.CurrentLanguage.enemyBios.enemyBios_type + "</color> " + enemyType + "\n\n<color=#FF4343>" + LanguageManager.CurrentLanguage.enemyBios.enemyBios_data + "</color>\n";
            if (MonoSingleton<BestiaryData>.Instance.GetEnemy(source.enemyType) > 1)
            {
                text += enemyDescription;
            }
            else
            {
                text += "???";
            }
            text = text + "\n\n</s><color=#FF4343>" + LanguageManager.CurrentLanguage.enemyBios.enemyBios_strategy + "</color>\n" + enemyStrategy;
            enemyPageContent.text = text;
        }

        /// <summary>
        /// Renders the bio that is currently open again.
        ///
        /// The game fills the bio only once, when an enemy is picked, so a language
        /// switch did not reach it and it kept the previous language until the bio
        /// was closed and opened again. Only the text is written again, the model the
        /// game shows next to the bio is left alone, so the bio does not flicker.
        /// </summary>
        public static void Reapply()
        {
            if (openSource == null || openTitle == null)
            {
                // No bio was opened yet, so there is nothing to translate.
                return;
            }

            if (!openTitle.gameObject.activeInHierarchy && (openEntryTitle == null || !openEntryTitle.gameObject.activeInHierarchy))
            {
                // The bio is not on screen, so there is nothing to translate.
                return;
            }

            try
            {
                if (isUsingEnglish())
                {
                    // The bio is populated after the shop's live group has captured
                    // its baseline, so that group cannot restore it. Put back the
                    // values saved by DisplayInfo instead of calling DisplayInfo
                    // again: the latter destroys and recreates the preview model.
                    openTitle.text = openGameTitle;
                    if (openContent != null)
                    {
                        openContent.text = openGameContent;
                    }
                    if (openEntryTitle != null)
                    {
                        openEntryTitle.text = openGameEntryTitle;
                    }
                    return;
                }

                ApplyTranslations(openSource, openTitle, openContent, openEntryTitle, openGameTitle);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
    }
}
