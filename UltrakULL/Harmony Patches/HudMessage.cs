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
        /// <summary>
        /// The original game-provided pieces of the HUD message that is currently shown.
        /// Kept so a language switch can re-translate the visible message immediately
        /// instead of waiting for the next <see cref="SendHudMessage"/> call.
        /// </summary>
        private static string _lastOriginalMessage;
        private static string _lastOriginalMessage2;
        private static string _lastOriginalInput;
        private static TMP_Text _hudTextComponent;

        [HarmonyPrefix]
        public static bool SendHudMessage_Prefix(ref string newmessage,ref string newinput,ref string newmessage2, int delay = 0, bool silent = false)
        {
            // Remember the original game-provided pieces before they are translated.
            if (!string.IsNullOrEmpty(newmessage))
            {
                _lastOriginalMessage = newmessage;
            }
            _lastOriginalMessage2 = newmessage2 ?? string.Empty;
            _lastOriginalInput = newinput ?? string.Empty;

            // Find and cache the text component that displays the HUD message.
            if (_hudTextComponent == null)
            {
                _hudTextComponent = FindHudTextComponent();
            }

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

        /// <summary>
        /// Re-applies the active language to the HUD message that is currently on
        /// screen. Called on every language switch so the visible message changes
        /// immediately instead of keeping the previous language.
        /// </summary>
        public static void Reapply()
        {
            if (string.IsNullOrEmpty(_lastOriginalMessage))
            {
                return;
            }

            // Re-locate the component each time: it may have been created after the
            // last Prefix ran, or its GameObject may have been disabled.
            _hudTextComponent = FindHudTextComponent();
            if (_hudTextComponent == null)
            {
                ClearCache();
                return;
            }

            if (!_hudTextComponent.gameObject.activeInHierarchy)
            {
                // The message is not on screen right now.
                ClearCache();
                return;
            }

            // If there is no actual message text on screen, the cached message is
            // stale (left over from a previous level or an already faded message).
            // Do not resurrect it — clear the cache instead.
            if (string.IsNullOrEmpty(_hudTextComponent.text))
            {
                ClearCache();
                return;
            }

            try
            {
                // Rebuild with the newly active language. Built-in English is handled
                // by the same path: CurrentLanguage is English, so GetMessage yields
                // the full English text (key included).
                string translated = StringsParent.GetMessage(_lastOriginalMessage, _lastOriginalMessage2, _lastOriginalInput);
                if (translated == _lastOriginalMessage)
                {
                    translated = HUDMessages.GetHUDToolTip(_lastOriginalMessage);
                }
                _hudTextComponent.text = translated;
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private static void ClearCache()
        {
            _lastOriginalMessage = null;
            _lastOriginalMessage2 = null;
            _lastOriginalInput = null;
            _hudTextComponent = null;
        }

        private static TMP_Text FindHudTextComponent()
        {
            try
            {
                var receiver = MonoSingleton<HudMessageReceiver>.Instance;
                if (receiver == null)
                {
                    return null;
                }

                var texts = receiver.GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    var text = texts[i];
                    if (text != null && text.gameObject.activeInHierarchy)
                    {
                        return text;
                    }
                }
            }
            catch
            {
                /* ignore */
            }
            return null;
        }
    }

    [HarmonyPatch(typeof(HudMessageReceiver),"SendHudMessage2")]
    public static class SendHudMessage2Patch
    {
        /// <summary>
        /// The original game-provided format and inputs for the HUD2 message
        /// currently shown. Kept so a language switch can re-translate it
        /// immediately instead of waiting for the next <see cref="SendHudMessage2"/>
        /// call.
        /// </summary>
        private static string _lastOriginalFormat;
        private static string[] _lastOriginalInputs;

        [HarmonyPrefix]
        public static bool SendHudMessage2_Prefix(ref string format, ref string[] newinputs, int delay, bool silent, ref bool inputBeenProcessed, bool automaticTimer)
        {
            if (!string.IsNullOrEmpty(format))
            {
                _lastOriginalFormat = format;
                _lastOriginalInputs = newinputs;
            }

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

        /// <summary>
        /// Re-applies the active language to the HUD2 message that is currently on
        /// screen. Called on every language switch so the visible message changes
        /// immediately instead of keeping the previous language.
        /// </summary>
        public static void Reapply()
        {
            if (string.IsNullOrEmpty(_lastOriginalFormat))
            {
                return;
            }

            // Re-locate the component: do not write a stale format into an empty
            // text element (e.g. after a level change or a faded message).
            var receiver = MonoSingleton<HudMessageReceiver>.Instance;
            if (receiver == null)
            {
                ClearCache2();
                return;
            }
            var texts = receiver.GetComponentsInChildren<TMP_Text>(true);
            TMP_Text activeText = null;
            for (int i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    activeText = text;
                    break;
                }
            }
            if (activeText == null || string.IsNullOrEmpty(activeText.text))
            {
                ClearCache2();
                return;
            }

            try
            {
                // Rebuild with the newly active language. Built-in English is handled
                // by the same path: CurrentLanguage is English, so GetMessage yields
                // the full English text.
                string inputs = _lastOriginalInputs != null ? string.Join(" ", _lastOriginalInputs) : string.Empty;
                string translated = StringsParent.GetMessage(_lastOriginalFormat, string.Empty, inputs);
                if (translated == _lastOriginalFormat)
                {
                    translated = HUDMessages.GetHUDToolTip(_lastOriginalFormat);
                }
                ApplyHud2Format(translated, null);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        private static void ClearCache2()
        {
            _lastOriginalFormat = null;
            _lastOriginalInputs = null;
        }

        private static void ApplyHud2Format(string format, string[] inputs)
        {
            var receiver = MonoSingleton<HudMessageReceiver>.Instance;
            if (receiver == null)
            {
                return;
            }

            var texts = receiver.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    text.text = format;
                    return;
                }
            }
        }
    }
}
