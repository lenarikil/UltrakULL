using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using static UltrakULL.CommonFunctions;
using UltrakULL.json;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(CheatsManager))]
    public static class CheatsManagerPatch
    {
        /// <summary>
        /// The name of the game for every header of the cheat menu. The headers are
        /// not cheat items, so the game does not refresh them and the mod has to
        /// translate them on its own after a language switch.
        /// </summary>
        private static readonly Dictionary<CheatMenuItem, string> categoryNames = new Dictionary<CheatMenuItem, string>();

        [HarmonyPatch("StartRebind"), HarmonyPostfix]
        public static void StartRebind_Postfix(Dictionary<ICheat, CheatMenuItem> ___menuItems, ref ICheat cheat) 
        {
            // Built-in English keeps the game's own text.
            if(isUsingEnglish())
            {
                return;
            }

            ___menuItems[cheat].bindButtonText.text = LanguageManager.CurrentLanguage.cheats.cheats_pressAnyKey;//Press any key
        }

        [HarmonyPatch("RebuildMenu"), HarmonyPostfix]
        public static void RebuildMenu_Postfix()
        {
            if(isUsingEnglish())
            {
                return;
            }
            
            GameObject canvas = GetInactiveRootObject("Canvas");

            GameObject cheatMenu = canvas.FindPath("Cheat Menu", "Cheats Manager", "Scroll View", "Viewport");

            CheatMenuItem[] cheatList = cheatMenu.GetComponentsInChildren<CheatMenuItem>();
            foreach (CheatMenuItem category in cheatList)
            {
                string name = category.longName.text;
                string translated = GetCategoryName(name);
                if (translated == null)
                {
                    // Not a category. The cheat items are translated by the patch
                    // that takes over the state update of the game.
                    continue;
                }

                // The name of the game is remembered, so the category can be
                // translated again after a language switch, without building the
                // menu again.
                categoryNames[category] = name;
                category.longName.text = translated;
            }
        }

        /// <summary>
        /// The name of a cheat category in the active language, or null when the given
        /// name is not a category.
        /// </summary>
        private static string GetCategoryName(string name)
        {
            switch (name)
            {
                case "META": { return LanguageManager.CurrentLanguage.cheats.cheats_categoryMeta; }
                case "SANDBOX": { return LanguageManager.CurrentLanguage.cheats.cheats_categorySandbox; }
                case "GENERAL": { return LanguageManager.CurrentLanguage.cheats.cheats_categoryGeneral; }
                case "MOVEMENT": { return LanguageManager.CurrentLanguage.cheats.cheats_categoryMovement; }
                case "WEAPONS": { return LanguageManager.CurrentLanguage.cheats.cheats_categoryWeapons; }
                case "ENEMIES": { return LanguageManager.CurrentLanguage.cheats.cheats_categoryEnemies; }
                case "VISUAL": { return LanguageManager.CurrentLanguage.cheats.cheats_categoryVisual; }
                case "SPECIAL": { return LanguageManager.CurrentLanguage.cheats.cheats_categorySpecial; }
                default: { return null; }
            }
        }

        /// <summary>
        /// Translates the headers of the cheat menu again.
        ///
        /// The headers are not cheat items, so the refresh of the cheat states does
        /// not reach them. The name of the game is remembered for every header, so
        /// the header can be translated again after a language switch.
        /// </summary>
        public static void ReapplyCategories()
        {
            if (categoryNames.Count == 0)
                return;

            List<CheatMenuItem> dead = new List<CheatMenuItem>();
            foreach (KeyValuePair<CheatMenuItem, string> pair in categoryNames)
            {
                CheatMenuItem category = pair.Key;
                if (category == null)
                {
                    // The menu was built again, the old item is gone.
                    dead.Add(category);
                    continue;
                }

                string text;
                if (isUsingEnglish())
                {
                    // The built-in English is the game's own name.
                    text = pair.Value;
                }
                else
                {
                    text = GetCategoryName(pair.Value);
                }

                if (string.IsNullOrEmpty(text) || text == category.longName.text)
                    continue;

                category.longName.text = text;
            }

            for (int i = 0; i < dead.Count; i++)
            {
                categoryNames.Remove(dead[i]);
            }
        }


        //@Override
        //Overrides the *private* UpdateCheatState function from the CheatsManager class for translating the cheat menu.
        [HarmonyPatch("UpdateCheatState", new Type[] { typeof(CheatMenuItem), typeof(ICheat) }), HarmonyPrefix]
        public static bool UpdateCheatState_MyPatch(CheatMenuItem item, ICheat cheat, CheatsManager __instance, Color ___enabledColor, Color ___disabledColor)
        {
            // Built-in English keeps the game's own cheat menu, the original method
            // does the same work with the game's own strings.
            if (isUsingEnglish())
            {
                return true;
            }

            try
            {
                item.longName.text = Cheats.GetCheatName(cheat.Identifier);
                item.stateBackground.color = (cheat.IsActive ? ___enabledColor : ___disabledColor);
                
                string cheatDisabledStatus = Cheats.GetCheatStatus(cheat.ButtonDisabledOverride);
                string cheatEnabledStatus = Cheats.GetCheatStatus(cheat.ButtonEnabledOverride);
                
                item.stateText.text = (cheat.IsActive ? (cheatEnabledStatus ?? LanguageManager.CurrentLanguage.cheats.cheats_activated) : (cheatDisabledStatus ?? LanguageManager.CurrentLanguage.cheats.cheats_deactivated)); //Cheat status
                item.bindButtonBack.gameObject.SetActive(false);
                
                string text = MonoSingleton<CheatBinds>.Instance.ResolveCheatKey(cheat.Identifier);
                if (string.IsNullOrEmpty(text))
                {
                    item.bindButtonText.text = LanguageManager.CurrentLanguage.cheats.cheats_pressToBind; //Press to bind
                }
                else
                {
                    item.bindButtonText.text = text.ToUpper();
                }
                GameObject parentResetButton = item.resetBindButton.gameObject;
                TextMeshProUGUI parentResetText = parentResetButton.Child("Text").TMP();
                parentResetText.text = LanguageManager.CurrentLanguage.cheats.cheats_delete;
                __instance.RenderCheatsInfo();
                return false;
            }
            catch (Exception e)
            {
                HandleError(e);
                return true;
            }
        }

        //@Override
        //Overrides the RenderCheatsInfo function from the CheatsManager class for displaying the active cheats on the HUD.
        [HarmonyPatch("RenderCheatsInfo"), HarmonyPrefix]
        public static bool RenderCheatsInfo_MyPatch(CheatsManager __instance, Dictionary<string, List<ICheat>> ___allRegisteredCheats)
        {
            // Built-in English keeps the game's own cheat list.
            if (isUsingEnglish())
            {
                return true;
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (MonoSingleton<SandboxNavmesh>.Instance && MonoSingleton<SandboxNavmesh>.Instance.isDirty)
            {
                stringBuilder.AppendLine("<color=red>" + LanguageManager.CurrentLanguage.cheats.cheats_navmeshOutdated1 + "\n<size=12>" + LanguageManager.CurrentLanguage.cheats.cheats_navmeshOutdated2 + "</size></color>\n");
            }
            if (__instance.GetCheatState("ultrakill.spawner-arm"))
            {
                stringBuilder.AppendLine("<color=#C2D7FF>" + LanguageManager.CurrentLanguage.cheats.cheats_spawnerArmSlot + "</color>\n");
            }
            foreach (KeyValuePair<string, List<ICheat>> keyValuePair in ___allRegisteredCheats)
            {
                foreach (ICheat cheat2 in from cheat in keyValuePair.Value
                                          where cheat.IsActive
                                          select cheat)
                {
                    string text = MonoSingleton<CheatBinds>.Instance.ResolveCheatKey(cheat2.Identifier);
                    if (!string.IsNullOrEmpty(text))
                    {
                        stringBuilder.Append("[<color=orange>" + text.ToUpper() + "</color>] ");
                    }
                    else
                    {
                        stringBuilder.Append("[ ] ");
                    }
                    stringBuilder.Append("<color=white>" + Cheats.GetCheatName(cheat2.Identifier) + "</color>\n");
                }
            }
            MonoSingleton<CheatsController>.Instance.cheatsInfo.text = stringBuilder.ToString();
            return false;
        }
    }
}
