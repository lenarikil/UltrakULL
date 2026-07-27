using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UltrakULL.json;
using static System.Reflection.Emit.OpCodes;
using static HarmonyLib.AccessTools;
using static UltrakULL.CommonFunctions;
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
        private const string MandaloreColour = "<color=#FFC49E>";
        private const string OwlColour = "<color=#9EE6FF>";
        private const string WhiteColour = "<color=#FFFFFF>";

        private const int ReplacementInstructionsLength = 5;

        private static readonly Dictionary<string, (string, string)> MandaloreBattleDialogs =
            new Dictionary<string, (string, string)>
            {
                { "now we lost", ("subtitles_mandalore_defeated", OwlColor) },
                { "Full auto", ("subtitles_mandalore_attack1", WhiteColor) },
                { "Fuller auto", ("subtitles_mandalore_attack2", WhiteColor) },
                { "Use the salt", ("subtitles_mandalore_phaseChangeThird1", OwlColor) },
                { "I'm reaching", ("subtitles_mandalore_phaseChangeThird2", MandaloreColor) },
                { "Feel my maximum speed", ("subtitles_mandalore_phaseChangeSecond1", MandaloreColor) },
                { "Slow down", ("subtitles_mandalore_phaseChangeSecond2", OwlColor) },
                { "I increase my speed", ("subtitles_mandalore_phaseChangeFirst1", OwlColor) },
                { "Just fucking", ("subtitles_mandalore_phaseChangeFirst2", OwlColor) }
            };

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
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_taunt3", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("I'm gonna"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_taunt2", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("Why"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_taunt5", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("fucking poison"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_taunt1", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("What"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_intro2", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Hold still"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_taunt4", MandaloreColor);
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
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_defeated", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("Full auto"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_attack1", WhiteColor);
                    } 
                    else if (((string)codes[i].operand).Contains("Fuller auto"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_attack2", WhiteColor);
                    }
                    else if (((string)codes[i].operand).Contains("Use the salt"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_phaseChangeThird1", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("I'm reaching"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_phaseChangeThird2", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Feel my maximum speed"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_phaseChangeSecond1", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Slow down"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_phaseChangeSecond2", OwlColor);
                    }
                    else if (((string)codes[i].operand).Contains("I increase my speed"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_phaseChangeFirst1", MandaloreColor);
                    }
                    else if (((string)codes[i].operand).Contains("Just fucking"))
                    {
                        codes[i].operand = GetSubtitleWithColor("subtitles_mandalore_phaseChangeFirst2", OwlColor);
                    }
                }
            }
            return codes;
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