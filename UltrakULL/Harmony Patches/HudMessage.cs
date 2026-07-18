using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UltrakULL.json;
using UltrakULL;

using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(CutsceneSkipText),"Show")]
    public static class CutsceneSkipTextPatch
    {
        [HarmonyPostfix]
        public static void CutsceneSkipText_Patch(CutsceneSkipText __instance, ref TMP_Text ___txt)
        {
            Console.WriteLine(___txt.text);
            //Need to disable the TextOverride component. Slightly hacky but we can't access TextOverride directly.
            Component[] test = __instance.GetComponents(typeof(Component));
            Behaviour bhvr = (Behaviour)test[3];
            bhvr.enabled = false;
            ___txt.text = LanguageManager.CurrentLanguage.misc.pressToSkip;
        }
    }

    [HarmonyPatch(typeof(HudMessageReceiver),"SendHudMessage")]
    public static class SendHudMessagePatch
    {
        [HarmonyPrefix]
        public static bool SendHudMessage_Prefix(ref string newmessage,ref string newinput,ref string newmessage2, int delay = 0, bool silent = false)
        {
            if (!CommonFunctions.isUsingEnglish() && !string.IsNullOrEmpty(newmessage))
            {
                string translated = StringsParent.GetMessage(newmessage, newmessage2 ?? string.Empty, newinput ?? string.Empty);
                if ((newmessage2 == null || newinput == null) && translated == newmessage)
                {
                    newmessage = HUDMessages.GetHUDToolTip(newmessage);
                }
                else
                {
                    newmessage = translated;
                }

                newmessage2 = string.Empty;
                newinput = string.Empty;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HudMessageReceiver),"SendHudMessage2")]
    public static class SendHudMessage2Patch
    {
        [HarmonyPrefix]
        public static bool SendHudMessage2_Prefix(ref string format, ref string[] newinputs, int delay, bool silent, ref bool inputBeenProcessed, bool automaticTimer)
        {
            if (!isUsingEnglish())
            {
                // Локализуем каждый input, если они есть
                if (newinputs != null)
                {
                    for (int i = 0; i < newinputs.Length; i++)
                    {
                        newinputs[i] = GetLocalizedInput(newinputs[i]);
                    }
                }

                // Если это сообщение о свободном падении на уровне 8-4, заменяем format на переведённый вариант
                if (format.Contains("WARNING:") && format.Contains("free fall"))
                {
                    string translated = Act3Strings.Level84(format, "", newinputs);
                    if (translated != "Unimplemented string")
                    {
                        format = translated;
                        newinputs = null; // чтобы оригинальный метод не пытался форматировать снова
                        inputBeenProcessed = true;
                    }
                }
            }
            return true;
        }
    }
}
