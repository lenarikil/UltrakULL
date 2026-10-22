using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(ControlsOptions), "Rebuild")]
    public class ControlSections
    {
        [HarmonyPostfix]
        public static void controlSectionsPatch_Postfix(ControlSections __instance, List<GameObject> ___rebindUIObjects)
        {
            // Built-in English keeps the game's original control section names.
            if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

            foreach(GameObject section in ___rebindUIObjects)
            {
                if (section.name == "Control Section Template(Clone)")
                {
                    TextMeshProUGUI sectionText = section.TMP();
                    switch (sectionText.text)
                    {
                        case "-- MOVEMENT --":
                        {
                            sectionText.text = "-- " + LanguageManager.CurrentLanguage.options.controls_movement + " --";
                            break;
                        }
                        case "-- WEAPON --":
                        {
                            sectionText.text = "-- " + LanguageManager.CurrentLanguage.options.controls_weapons + " --";
                            break;
                        }
                        case "-- FIST --":
                        {
                            sectionText.text = "-- " + LanguageManager.CurrentLanguage.options.controls_fist + " --";
                            break;
                        }
                        case "-- HUD --":
                        {
                            sectionText.text = "-- " + LanguageManager.CurrentLanguage.options.category_hud + " --";
                            break ;
                        }
                        default:{ break; }
                    }
                }
            }
        }
    }
    

    [HarmonyPatch(typeof(ControlsOptionsKey),"OnEnable")]
    public class ControlBindNames
    {
        public static string getActionName(string originalText)
        {
            switch (originalText)
            {
                case "MOVE": { return LanguageManager.CurrentLanguage.options.controls_move; }
                case "DODGE": { return LanguageManager.CurrentLanguage.options.controls_dodge; }
                case "SLIDE": { return LanguageManager.CurrentLanguage.options.controls_slide; }
                case "JUMP": { return LanguageManager.CurrentLanguage.options.controls_jump; }
                case "PRIMARY FIRE": { return LanguageManager.CurrentLanguage.options.controls_primaryFire; }
                case "SECONDARY FIRE": { return LanguageManager.CurrentLanguage.options.controls_secondaryFire; }
                case "NEXT VARIATION": { return LanguageManager.CurrentLanguage.options.controls_nextVariation; }
                case "PREVIOUS VARIATION": { return LanguageManager.CurrentLanguage.options.controls_previousVariation; }
                case "REVOLVER": { return LanguageManager.CurrentLanguage.options.controls_revolver; }
                case "SHOTGUN": { return LanguageManager.CurrentLanguage.options.controls_shotgun; }
                case "NAILGUN": { return LanguageManager.CurrentLanguage.options.controls_nailgun; }
                case "RAILCANNON": { return LanguageManager.CurrentLanguage.options.controls_railcannon; }
                case "ROCKET LAUNCHER": { return LanguageManager.CurrentLanguage.options.controls_rocketLauncher; }
                case "SPAWNER ARM": { return LanguageManager.CurrentLanguage.options.controls_spawnerArm; }
                case "NEXT WEAPON": { return LanguageManager.CurrentLanguage.options.controls_nextWeapon; }
                case "PREVIOUS WEAPON": { return LanguageManager.CurrentLanguage.options.controls_previousWeapon; }
                case "LAST USED WEAPON": { return LanguageManager.CurrentLanguage.options.controls_lastUsedWeapon; }
                case "VARIATION SLOT 1": { return LanguageManager.CurrentLanguage.options.controls_variationSlot1; }
                case "VARIATION SLOT 2": { return LanguageManager.CurrentLanguage.options.controls_variationSlot2; }
                case "VARIATION SLOT 3": { return LanguageManager.CurrentLanguage.options.controls_variationSlot3; }
                case "PUNCH": { return LanguageManager.CurrentLanguage.options.controls_punch; }
                case "CHANGE FIST": { return LanguageManager.CurrentLanguage.options.controls_changeFist; }
                case "PUNCH (FEEDBACKER)": { return LanguageManager.CurrentLanguage.options.controls_punchFeedbacker; }
                case "PUNCH (KNUCKLEBLASTER)": { return LanguageManager.CurrentLanguage.options.controls_punchKnuckleblaster; }
                case "HOOK": { return LanguageManager.CurrentLanguage.options.controls_whiplash; }
                case "STATS": { return LanguageManager.CurrentLanguage.sandbox.sandbox_shop_stats; }
                default: return originalText;
            }
        }
        
        [HarmonyPostfix]
        public static void controlBindNamesPatch_Postfix(ControlsOptionsKey __instance)
        {
            // Built-in English keeps the game's original control names.
            if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

            __instance.actionText.text = getActionName(__instance.actionText.text);
        }
    }
    [HarmonyPatch(typeof(ControlsOptionsKey), "GenerateTooltip")]
    public static class BoundMultipleTimes
    {
        [HarmonyPostfix]
        public static string GenerateTooltip_Postfix(string __result, InputAction action, InputBinding binding, InputBinding[] conflicts)
        {
            // Built-in English keeps the game's original conflict tooltip.
            if (UltrakULL.CommonFunctions.isUsingEnglish()) return __result;

            string str = action.GetBindingDisplayStringWithoutOverride(binding, InputBinding.DisplayStringOptions.DontIncludeInteractions).ToUpper();
            string str2 = "<color=red>" + str + " " + LanguageManager.CurrentLanguage.options.controls_boundMultiple + ":";
            HashSet<string> hashSet = new HashSet<string>();
            foreach (InputBinding inputBinding in conflicts)
            {
                if (!hashSet.Contains(inputBinding.action))
                {
                    str2 += "<br>";
                    string actiontranslated = ControlBindNames.getActionName(inputBinding.action.ToUpper());
                    str2 = str2 + "- " + actiontranslated;
                    hashSet.Add(inputBinding.action);
                }
            }
            return str2 + "</color>";

        }
    }
}
