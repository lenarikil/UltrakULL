using HarmonyLib;
using UltrakULL.json;
using UnityEngine;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the RicoshotPointsCheck from the Coin class. Used for translating additional style strings that come from ricochets.
    [HarmonyPatch(typeof(Coin), "RicoshotPointsCheck")]
    public static class LocalizeTranslateStyleStrings
    {
        [HarmonyPrefix]
        public static bool RicoshotPointsCheck_MyPatch(Coin __instance, GameObject ___altBeam, bool ___wasShotByEnemy, StyleHUD ___shud, EnemyIdentifier ___eid)
        {
            if(isUsingEnglish())
            {
                return true;
            }
            
            string text = "";
            int num = 50;
            RevolverBeam revolverBeam;
            if (___altBeam != null && ___altBeam.TryGetComponent<RevolverBeam>(out revolverBeam) && revolverBeam.ultraRicocheter)
            {
                string ultraText = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.style != null
                    ? LanguageManager.CurrentLanguage.style.style_ricoshotUltra
                    : null;
                text = "<color=orange>" + LocalizeStyleHud.TagStyleName(
                    LocalizeStyleHud.RicoshotUltraModifierId,
                    string.IsNullOrEmpty(ultraText) ? "ULTRA" : ultraText) + "</color>";
                num += 50;
            }
            if (___wasShotByEnemy)
            {
                string counterText = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.style != null
                    ? LanguageManager.CurrentLanguage.style.style_ricoshotCounter
                    : null;
                text += "<color=red>" + LocalizeStyleHud.TagStyleName(
                    LocalizeStyleHud.RicoshotCounterModifierId,
                    string.IsNullOrEmpty(counterText) ? "COUNTER" : counterText) + "</color>";
                num += 50;
            }
            if (__instance.ricochets > 1)
            {
                num += __instance.ricochets * 15;
            }
            StyleHUD styleHUD = ___shud;
            int points = num;
            string pointID = "ultrakill.ricoshot";
            string prefix = text;
            styleHUD.AddPoints(points, pointID, __instance.sourceWeapon, ___eid, __instance.ricochets, prefix, "");

            return false;
        }
    }
}
