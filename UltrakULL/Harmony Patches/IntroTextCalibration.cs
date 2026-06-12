using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UltrakULL.json;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch]
    public static class LocalizeIntroTextCalibration
    {
        static MethodBase TargetMethod()
        {
            var nested = typeof(IntroText).GetNestedTypes(BindingFlags.NonPublic);
            var stateMachine = nested.FirstOrDefault(t => t.Name.Contains("TextAppear"));
            if (stateMachine == null)
            {
                Logging.Error("[IntroTextCalibration] Could not find TextAppear state machine type");
                return null;
            }
            return AccessTools.Method(stateMachine, "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldstr)
                {
                    var str = (string)code.operand;
                    if (str == "<color=red>ERROR</color>")
                    {
                        yield return new CodeInstruction(OpCodes.Call,
                            typeof(LocalizeIntroTextCalibration).GetMethod("GetCalibrationError",
                                BindingFlags.Static | BindingFlags.NonPublic));
                        continue;
                    }
                    if (str == "<color=green>OK</color>")
                    {
                        yield return new CodeInstruction(OpCodes.Call,
                            typeof(LocalizeIntroTextCalibration).GetMethod("GetCalibrationOk",
                                BindingFlags.Static | BindingFlags.NonPublic));
                        continue;
                    }
                }
                yield return code;
            }
        }

        private static string GetCalibrationError()
        {
            if (isUsingEnglish())
            {
                return "<color=red>ERROR</color>";
            }
            string translated = LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationError;
            if (string.IsNullOrEmpty(translated))
            {
                return "<color=red>ERROR</color>";
            }
            return "<color=red>" + translated + "</color>";
        }

        private static string GetCalibrationOk()
        {
            if (isUsingEnglish())
            {
                return "<color=green>OK</color>";
            }
            string translated = LanguageManager.CurrentLanguage.tutorial.tutorial_calibrationOk;
            if (string.IsNullOrEmpty(translated))
            {
                return "<color=green>OK</color>";
            }
            return "<color=green>" + translated + "</color>";
        }
    }
}
