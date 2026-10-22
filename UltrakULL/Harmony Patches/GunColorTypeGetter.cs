using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides OnEnable from the GunColorTypeGetter class. Used for the Soul Orb checker.
    [HarmonyPatch(typeof(GunColorTypeGetter), "OnEnable")]
    public static class LocalizeGunColorTypeShop
    {
        [HarmonyPostfix]
        public static void OnEnablePostFix_MyPatch(GunColorTypeGetter __instance, TMP_Text[] ___templateTexts)
        {
            if(isUsingEnglish())
            {
                return;
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
