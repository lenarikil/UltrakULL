using HarmonyLib;
using UnityEngine;
using TMPro;
using UltrakULL.json;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(MoneyText), "DivideMoney")]
    public static class MoneyTextPatch
    {
        [HarmonyPostfix]
        public static void DivideMoneyPatch(int dosh, ref string __result)
        {
            // Built-in English keeps the game's original money formatting.
            if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

            if (dosh > 1000000000)
            {
                __result = LanguageManager.CurrentLanguage.shop.shop_lotsOfMoney;
            }
        }
    }
    [HarmonyPatch(typeof(MoneyText), "UpdateMoney")]
    public static class UpdateMoneyPatch
    {
        // The counter the game wrote last. Kept so a language switch can rewrite
        // it right away instead of waiting for the next money change.
        private static MoneyText lastMoneyText;

        [HarmonyPostfix]
        public static void UpdateMoneyPostfix(MoneyText __instance)
        {
            lastMoneyText = __instance;

            // Built-in English keeps the game's original money formatting.
            if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

            TextMeshProUGUI tmtext = __instance.GetComponent<TextMeshProUGUI>();
            tmtext.text = MoneyText.DivideMoney(GameProgressSaver.GetMoney()) + " <color=#FF4343>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>";
        }

        /// <summary>
        /// Rewrites the money counter in the active language.
        ///
        /// The game writes its own text in UpdateMoney and the postfix above
        /// replaces it with the translation, so letting the game update the
        /// counter once more is enough to put the new language on screen. The
        /// game's own English text is restored the same way.
        /// </summary>
        internal static void Reapply()
        {
            MoneyText instance = lastMoneyText;

            if (instance == null)
            {
                MoneyText[] found = UnityEngine.Object.FindObjectsOfType<MoneyText>(true);
                if (found.Length > 0)
                    instance = found[0];
            }

            if (instance == null)
                return;

            instance.UpdateMoney();
        }
    }

}
