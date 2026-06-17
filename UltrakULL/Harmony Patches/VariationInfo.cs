using HarmonyLib;
using TMPro;
using UnityEngine.UI;
using UltrakULL.json;

using static UltrakULL.CommonFunctions;
using UnityEngine;


namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the UpdateMoney method from the VariationInfo class. This is needed to patch the "ALREADY OWNED" string, and will save having to change every single seperate button containing this string in the shop.
    [HarmonyPatch(typeof(VariationInfo))]
    public static class LocalizeVariationOwnership
    {
        [HarmonyPatch(nameof(VariationInfo.UpdateMoney)), HarmonyPostfix]
        public static void UpdateMoney_Postfix(VariationInfo __instance, int ___money, bool ___alreadyOwned, TMP_Text ___buttonText, TMP_Text ___equipText)
        {
            if(isUsingEnglish())
            {
                return;
            }
                if (!___alreadyOwned)
                {
                    if (__instance.cost < 0)
                    {
                        __instance.costText.text = "<color=red>" + LanguageManager.CurrentLanguage.misc.weapons_unavailable + "</color>";
                    }
                    else if (__instance.cost > ___money)
                    {
                        __instance.costText.text = "<color=red>" + MoneyText.DivideMoney(__instance.cost) + " " + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>";
                    }
                    else
                    {
                        __instance.costText.text = "<color=white>" + MoneyText.DivideMoney(__instance.cost) + "</color> <color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>";
                    }
                }
                else
                {
                    __instance.costText.text = LanguageManager.CurrentLanguage.misc.weapons_alreadyBought;
                }
            ___buttonText.text = (___buttonText.text.ToUpper() == "ALREADY OWNED" ? LanguageManager.CurrentLanguage.misc.weapons_alreadyBought : ___buttonText.text);

            if (___buttonText != null && ___buttonText.text.Contains("P</color>"))
            {
                ___buttonText.text = ___buttonText.text.Replace(
                    "P",
                    LanguageManager.CurrentLanguage.shop.shop_moneyCount
                );
            }

            switch (___equipText.text)
            {
                case "Unequipped":
                    ___equipText.text = LanguageManager.CurrentLanguage.shop.shop_unequipped;
                    return;
                case "Equipped":
                    ___equipText.text = LanguageManager.CurrentLanguage.shop.shop_equipped;
                    return;
                case "Alternate":
                    ___equipText.text = LanguageManager.CurrentLanguage.shop.shop_alternate;
                    return;
                default:
                    return;
            }
        }

        [HarmonyPatch(typeof(VariationInfo), "SetEquipStatusText"), HarmonyPostfix]
        public static void SetEquipStatusTextPostFix(ref TMP_Text ___equipText, int ___equipStatus)
        {
            switch (___equipStatus)
            {
                case 0:
                    ___equipText.text = LanguageManager.CurrentLanguage.shop.shop_unequipped;
                    return;
                case 1:
                    ___equipText.text = LanguageManager.CurrentLanguage.shop.shop_equipped;
                    return;
                case 2:
                    ___equipText.text = LanguageManager.CurrentLanguage.shop.shop_alternate;
                    return;
                default:
                    return;
            }
        }
    }
}
