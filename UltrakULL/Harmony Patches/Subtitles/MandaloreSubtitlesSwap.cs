using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UltrakULL.json;
using static System.Reflection.Emit.OpCodes;
using static HarmonyLib.AccessTools;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Reflection;

namespace UltrakULL.Harmony_Patches.Subtitles
{
    /**
     * Mandalore is a tricky one since we can't just use Harmony prefixes here.
     * So we're going to have to replace IL instructions of hardcoded strings with our own.
     */
    [HarmonyPatch(typeof(Mandalore))]
    public static class MandaloreSubtitlesSwap
    {
        private const string MandaloreColor = "FFC49E";
        private const string OwlColor = "9EE6FF";
        private const string WhiteColor = "FFFFFF";

        [HarmonyPatch("Start"), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MandaloreStartTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                bool isLdstr = (codes[i].opcode == OpCodes.Ldstr);
                if (isLdstr)
                {
                    if (((string)codes[i].operand).Contains("You cannot"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_taunt3", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("I'm gonna"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_taunt2", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("Why"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_taunt5", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("fucking poison"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_taunt1", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("What"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_intro2", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Hold still"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_taunt4", MandaloreColor);
                    }
                }
            }
            return codes;
        }
        [HarmonyPatch("Update"), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> MandaloreUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                bool isLdstr = (codes[i].opcode == OpCodes.Ldstr);
                if (isLdstr)
                {
                    if (((string)codes[i].operand).Contains("now we lost"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_defeated", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("Full auto"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_attack1", WhiteColor);
                    } 
                    else if (((string)codes[i].operand).Contains("Fuller auto"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_attack2", WhiteColor);
                    }
                    else if (((string)codes[i].operand).Contains("Use the salt"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_phaseChangeThird1", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("I'm reaching"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_phaseChangeThird2", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Feel my maximum speed"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_phaseChangeSecond1", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Slow down"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_phaseChangeSecond2", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("I increase my speed"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_phaseChangeFirst1", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Just fucking"))
                    {
                        ApplySubtitle(codes[i], "subtitles_mandalore_phaseChangeFirst2", OwlColor);
                    }
                }
            }
            return codes;
        }

        /// <summary>
        /// Replaces the subtitle string only when the current language provides a value for it.
        /// In built-in English (or a translation missing the key) the game's original string is
        /// kept; assigning null here would make Harmony's IL writer fail with "ldstr NULL".
        /// </summary>
        private static void ApplySubtitle(CodeInstruction instruction, string fieldName, string colorHex)
        {
            string replacement = GetSubtitleWithColor(fieldName, colorHex);
            if (!string.IsNullOrEmpty(replacement))
                instruction.operand = replacement;
        }

        /// <summary>
        /// Gets a subtitle string from the current language with color tags.
        /// This is called at runtime (not at IL patch time), so it always uses the current language.
        /// </summary>
        private static string GetSubtitleWithColor(string fieldName, string colorHex)
        {
            return SubtitlesHelper.GetTextWithColor(fieldName, colorHex);
        }
    }
}