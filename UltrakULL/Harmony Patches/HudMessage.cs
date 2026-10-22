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

        /// <summary>
        /// Applies the active language to the skip hint of a cutscene.
        ///
        /// The hint is filled once, when the cutscene starts, so a language switch did
        /// not reach it and it kept the previous language until the cutscene ended.
        /// </summary>
        public static void Reapply()
        {
            CutsceneSkipText skipText = MonoSingleton<CutsceneSkipText>.Instance;
            if (skipText == null)
            {
                return;
            }

            TMP_Text text = skipText.GetComponent<TMP_Text>();
            if (text == null || !text.enabled)
            {
                // The hint is not on screen right now, it is filled when it is shown.
                return;
            }

            try
            {
                text.text = LanguageManager.CurrentLanguage.misc.pressToSkip;
            }
            catch (Exception e)
            {
                HandleError(e);
            }
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
