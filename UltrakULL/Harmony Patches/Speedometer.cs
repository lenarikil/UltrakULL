using HarmonyLib;
using System;
using System.Globalization;
using TMPro;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(Speedometer), "FixedUpdate")]
    public static class SpeedometerPatch
    {
        [HarmonyPrefix]
        public static bool FixedUpdate_Prefix(TimeSince ___lastUpdate, bool ___classicVersion, TextMeshProUGUI ___textMesh, int ___type)
        {
            if(!___classicVersion)
            {
                if (___type == 0)
                {
                    return false;
                }

                // Only compute velocity/label when the display is actually refreshed
                // (~15 Hz) instead of on every physics tick.
                if (___lastUpdate > 0.064f)
                {
                    float num = 0f;
                    string arg = "";
                    switch (___type)
                    {
                        case 1:
                            num = MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(true).magnitude;
                            arg = LanguageManager.CurrentLanguage.misc.classicHud_speed_u;
                            break;
                        case 2:
                            num = Vector3.ProjectOnPlane(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(true), Vector3.up).magnitude;
                            arg = LanguageManager.CurrentLanguage.misc.classicHud_speed_hu;
                            break;
                        case 3:
                            num = Mathf.Abs(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(true).y);
                            arg = LanguageManager.CurrentLanguage.misc.classicHud_speed_vu;
                            break;
                    }

                    ___textMesh.text = string.Format(LanguageManager.CurrentLanguage.misc.classicHud_speed + ": {0:0.00} {1}", num, arg);
                    ___lastUpdate = 0;
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
