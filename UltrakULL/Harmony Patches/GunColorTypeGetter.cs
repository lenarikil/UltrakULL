using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides OnEnable from the GunColorTypeGetter class. Used for the Soul Orb checker.
    [HarmonyPatch(typeof(GunColorTypeGetter), "OnEnable")]
    public static class LocalizeGunColorTypeShop
    {
        /// <summary>
        /// The texts the game itself wrote into the soul-orb template rows. It is kept,
        /// so the built-in English can be put back on a language switch.
        /// </summary>
        private static readonly Dictionary<GunColorTypeGetter, string[]> originalSoulOrbTexts = new Dictionary<GunColorTypeGetter, string[]>();

        [HarmonyPostfix]
        public static void OnEnablePostFix_MyPatch(GunColorTypeGetter __instance, TMP_Text[] ___templateTexts)
        {
            if(isUsingEnglish())
            {
                return;
            }
            
            // Remember the game's original texts so we can re-apply after a language
            // switch. The component text is already translated after the first pass,
            // so we must capture it before overwriting.
            if (!originalSoulOrbTexts.ContainsKey(__instance) && ___templateTexts != null)
            {
                string[] originals = new string[___templateTexts.Length];
                for (int k = 0; k < ___templateTexts.Length; k++)
                {
                    originals[k] = ___templateTexts[k] != null ? ___templateTexts[k].text : null;
                }
                originalSoulOrbTexts[__instance] = originals;
            }

            for (int i = 1; i < 5; i++)
            {
                bool flag = GameProgressSaver.GetTotalSecretsFound() >= GunColorController.requiredSecrets[i];
                if (!flag)
                {
                    ___templateTexts[i].text = string.Concat(new object[]
                    {
                        LanguageManager.CurrentLanguage.shop.shop_soulOrbs + ": ",
                        GameProgressSaver.GetTotalSecretsFound(),
                        " / ",
                        GunColorController.requiredSecrets[i]
                    });
                }
            }
        }

        /// <summary>
        /// Applies the active language to the soul-orb texts on screen.
        ///
        /// The texts are written only when the shop opens, so a language switch did
        /// not reach them and they kept the previous language until the shop was
        /// closed and opened again.
        /// </summary>
        public static void Reapply()
        {
            GunColorTypeGetter[] getters = UnityEngine.Object.FindObjectsOfType<GunColorTypeGetter>(true);
            for (int i = 0; i < getters.Length; i++)
            {
                GunColorTypeGetter getter = getters[i];
                if (getter == null)
                {
                    continue;
                }

                TMP_Text[] templateTexts = AccessTools.Field(typeof(GunColorTypeGetter), "templateTexts")?.GetValue(getter) as TMP_Text[];
                if (templateTexts == null)
                {
                    continue;
                }

                string[] originals = null;
                if (!originalSoulOrbTexts.TryGetValue(getter, out originals) || originals == null)
                {
                    originals = new string[templateTexts.Length];
                    for (int k = 0; k < templateTexts.Length; k++)
                    {
                        originals[k] = templateTexts[k] != null ? templateTexts[k].text : null;
                    }
                    originalSoulOrbTexts[getter] = originals;
                }

                try
                {
                    for (int j = 1; j < 5; j++)
                    {
                        if (j >= templateTexts.Length || templateTexts[j] == null)
                        {
                            continue;
                        }

                        bool flag = GameProgressSaver.GetTotalSecretsFound() >= GunColorController.requiredSecrets[j];
                        if (!flag)
                        {
                            if (isUsingEnglish())
                            {
                                // The built-in English is the game's own text.
                                if (originals.Length > j && originals[j] != null)
                                    templateTexts[j].text = originals[j];
                            }
                            else
                            {
                                templateTexts[j].text = string.Concat(new object[]
                                {
                                    LanguageManager.CurrentLanguage.shop.shop_soulOrbs + ": ",
                                    GameProgressSaver.GetTotalSecretsFound(),
                                    " / ",
                                    GunColorController.requiredSecrets[j]
                                });
                            }
                        }
                        else if (isUsingEnglish() && originals.Length > j && originals[j] != null)
                        {
                            // Enough secrets found: the game cleared this row, restore the
                            // game's own text.
                            templateTexts[j].text = originals[j];
                        }
                    }
                }
                catch (Exception e)
                {
                    UltrakULL.CommonFunctions.HandleError(e);
                }
            }
        }
    }
    [HarmonyPatch(typeof(GunColorLock), "OnEnable")]
    public class GunColorLockPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr && (string)code.operand == "<color=#FF4343>P</color>")
                {
                    // Keep the vanilla money label when the language has none (built-in English).
                    string money = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.shop != null
                        ? LanguageManager.CurrentLanguage.shop.shop_moneyCount : null;
                    if (!string.IsNullOrEmpty(money))
                        code.operand = $"<color=#FF4343>" + money + "</color>";
                }

                if (code.opcode == OpCodes.Ldstr && (string)code.operand == "<color=red>1,000,000 P</color>")
                {
                    string money = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.shop != null
                        ? LanguageManager.CurrentLanguage.shop.shop_moneyCount : null;
                    if (!string.IsNullOrEmpty(money))
                        code.operand = $"1,000,000<color=#FF4343> " + money + "</color>";
                }

                yield return code;
            }
        }
    }
}
