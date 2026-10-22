using HarmonyLib;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// The state of the equipment of every shop item. The game only writes it when
        /// the player changes the equipment, so it is kept to be able to write it
        /// again after a language switch.
        /// </summary>
        private static readonly Dictionary<VariationInfo, int> equipStatus = new Dictionary<VariationInfo, int>();

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
        public static void SetEquipStatusTextPostFix(VariationInfo __instance, ref TMP_Text ___equipText, int ___equipStatus)
        {
            // The state of the equipment is kept, so it can be written again after a
            // language switch.
            equipStatus[__instance] = ___equipStatus;

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

        /// <summary>
        /// Drops the shop items that are gone, so the state of the equipment does not
        /// pile up over the levels.
        /// </summary>
        private static void PruneDead()
        {
            if (equipStatus.Count == 0)
            {
                return;
            }

            List<VariationInfo> dead = new List<VariationInfo>();
            foreach (KeyValuePair<VariationInfo, int> pair in equipStatus)
            {
                if (pair.Key == null)
                {
                    // The shop item died with the scene it belonged to.
                    dead.Add(pair.Key);
                }
            }

            for (int i = 0; i < dead.Count; i++)
            {
                equipStatus.Remove(dead[i]);
            }
        }

        /// <summary>
        /// Applies the active language to the shop items that are on screen.
        ///
        /// The price, the ownership text and the equipment state are written by the
        /// game when the shop is opened, so they kept the language that was active
        /// before a switch. The game writes its own text again and the patch above
        /// translates it, so the shop does not have to be entered again.
        /// </summary>
        public static void Reapply()
        {
            PruneDead();

            VariationInfo[] variations = UnityEngine.Object.FindObjectsOfType<VariationInfo>(true);
            for (int i = 0; i < variations.Length; i++)
            {
                VariationInfo variation = variations[i];
                if (variation == null || !variation.gameObject.activeInHierarchy)
                {
                    // Only the items that are on screen are shown to the player.
                    continue;
                }

                try
                {
                    variation.UpdateMoney();

                    // The state of the equipment is not written by the game when the
                    // money is updated, so it is written here.
                    if (variation.equipText == null)
                    {
                        continue;
                    }

                    int status;
                    if (!equipStatus.TryGetValue(variation, out status))
                    {
                        continue;
                    }

                    if (isUsingEnglish())
                    {
                        // The built-in English is the game's own text.
                        variation.equipText.text = status == 0 ? "Unequipped" : (status == 1 ? "Equipped" : "Alternate");
                        continue;
                    }

                    switch (status)
                    {
                        case 0:
                            variation.equipText.text = LanguageManager.CurrentLanguage.shop.shop_unequipped;
                            break;
                        case 1:
                            variation.equipText.text = LanguageManager.CurrentLanguage.shop.shop_equipped;
                            break;
                        case 2:
                            variation.equipText.text = LanguageManager.CurrentLanguage.shop.shop_alternate;
                            break;
                    }
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }
        }
    }
}
