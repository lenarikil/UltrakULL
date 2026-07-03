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
            if (isUsingEnglish())
            {
                return;
            }

            if (___shopCanvas != null)
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
                    TextMeshProUGUI primeEndText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject, "Background"), "Main Window"),
                        "Scroll View"), "Viewport"), "Text"));
                    PrimeSanctumStrings pss = new PrimeSanctumStrings();
                    primeEndText.text = pss.GetSecretText();
                    return;
                }

                TextMeshProUGUI origTip = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject, "Background"), "Main Panel"), "Tip of the Day"), "Panel"), "Text Inset"), "TipText"));
                GameObject shopObject = ___shopCanvas.gameObject;

                //Redirect for the 5-3 end shop.
                Shop.PatchShopRefactor(ref shopObject);
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
    }
}