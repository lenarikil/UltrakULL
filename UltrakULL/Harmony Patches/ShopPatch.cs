using HarmonyLib;
using Sandbox;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(ShopZone), "TurnOn")]
    public class ShopPatch
    {
        [HarmonyPostfix]
        public static void shopPatch(ShopZone __instance, ref Canvas ___shopCanvas)
        {
            if (___shopCanvas == null)
            {
                return;
            }

            {
                //5-S shop (SecretLevels.Patch5S and FishingPatch.cs)
                if (__instance.gameObject.name == "Fishing Enc Terminal")
                {
                    return;
                }

                //Cybergrind Shop (CyberGrind.cs)
                if (__instance.gameObject.name == "Cybergrind Shop")
                {
                    return;
                }

                //Sandbox shop (Don't do anything cause that's Sandbox.cs do)
                if (__instance.gameObject.name == "Sandbox Shop")
                {
                    return;
                }
                if (__instance.gameObject.name == "Garry Shop")
                {
                    return;
                }

                //Secret testaments (Don't do anything here since it's taken care of SecretLevels.cs)
                if ((__instance.gameObject.name == "Testament Shop") || ((__instance.gameObject.name == "Testament Shop (1)")) && GetCurrentSceneName().Contains("-S"))
                {
                    return;
                }

            //Prime testaments
            if (__instance.gameObject.name == "Shop Prime")
            {
                Logging.Warn("Prime end testament, getting text");
                TextMeshProUGUI primeEndText = ___shopCanvas.gameObject.FindPath("Background", "Main Window", "Scroll View", "Viewport", "Text").TMP();
                PrimeSanctumStrings pss = new PrimeSanctumStrings();

                // The testament is written once, when the terminal is turned on, so
                // it is registered as a live group to follow the language.
                LiveText.ApplyGroup(___shopCanvas.gameObject, delegate { primeEndText.text = pss.GetSecretText(); });
                return;
            }

                TextMeshProUGUI origTip = ___shopCanvas.gameObject.FindPath("Background", "Main Panel", "Tip of the Day", "Panel", "Text Inset", "TipText").TMP();
                GameObject shopObject = ___shopCanvas.gameObject;

                // The game fills the shop when the terminal is turned on and never touches
                // it again, so the shop kept the language that was active before a switch.
                // It is registered as a live group: a language switch puts the game's own
                // text back and writes the new language on top of it, so the player does
                // not have to leave the shop and enter it again.
                LiveText.ApplyGroup(shopObject, delegate { Shop.PatchShopRefactor(shopObject); });
            }
        }

    }
    [HarmonyPatch(typeof(GunColorTypeGetter))]
    public static class GunColorTypeGetterPatch
    {
        [HarmonyPatch("Awake"), HarmonyPostfix]
        public static void AwakePatch(Button ___altButton, bool ___altVersion)
        {
            ___altButton.GetComponentInChildren<TMP_Text>().SetText(___altVersion ? LanguageManager.CurrentLanguage.shop.shop_colorsStandard : LanguageManager.CurrentLanguage.shop.shop_colorsAlternate, true);
        }
        [HarmonyPatch(nameof(GunColorTypeGetter.ToggleAlternate)), HarmonyPostfix]
        public static void ToggleAlternatePostfix(Button ___altButton, bool ___altVersion)
        {
            ___altButton.GetComponentInChildren<TMP_Text>().SetText(___altVersion ? LanguageManager.CurrentLanguage.shop.shop_colorsStandard : LanguageManager.CurrentLanguage.shop.shop_colorsAlternate, true);
        }

        /// <summary>
        /// Applies the active language to the button that switches between the
        /// standard and the alternate colors.
        ///
        /// The game writes the name of the button only when the colors are switched,
        /// so a language switch did not reach it and it kept the previous language.
        /// </summary>
        public static void Reapply()
        {
            GunColorTypeGetter[] getters = UnityEngine.Object.FindObjectsOfType<GunColorTypeGetter>(true);
            for (int i = 0; i < getters.Length; i++)
            {
                GunColorTypeGetter getter = getters[i];
                if (getter == null || getter.altButton == null)
                {
                    continue;
                }

                TMP_Text buttonText = getter.altButton.GetComponentInChildren<TMP_Text>();
                if (buttonText == null)
                {
                    continue;
                }

                if (isUsingEnglish())
                {
                    // The built-in English is the game's own text.
                    buttonText.SetText(getter.altVersion ? "Standard" : "Alternate", true);
                    continue;
                }

                buttonText.SetText(getter.altVersion ? LanguageManager.CurrentLanguage.shop.shop_colorsStandard : LanguageManager.CurrentLanguage.shop.shop_colorsAlternate, true);
            }
        }
    }
}