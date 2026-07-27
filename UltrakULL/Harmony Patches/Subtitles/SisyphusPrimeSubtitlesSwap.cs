using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.Harmony_Patches.Subtitles
{
	[HarmonyPatch(typeof(SisyphusPrime))]
	public class SisyphusPrimeSubtitlesSwap
	{
		private const int LdstrInstructionOffset = 3;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SisyphusPrime), "Enrage")]
		private static IEnumerable<CodeInstruction> SisyphusPrime_Update(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_sisyphusPrime_phaseChange", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SisyphusPrime), "Taunt")]
		private static IEnumerable<CodeInstruction> SisyphusPrime_Taunt(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_sisyphusPrime_attack1", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SisyphusPrime), "Clap")]
		private static IEnumerable<CodeInstruction> SisyphusPrime_Clap(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_sisyphusPrime_attack2", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SisyphusPrime), "StompCombo")]
		private static IEnumerable<CodeInstruction> SisyphusPrime_StompCombo(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_sisyphusPrime_attack3", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SisyphusPrime), "UppercutCombo")]
		private static IEnumerable<CodeInstruction> SisyphusPrime_UppercutCombo(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_sisyphusPrime_attack4", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SisyphusPrime), "ExplodeAttack")]
		private static IEnumerable<CodeInstruction> SisyphusPrime_ExplodeAttack(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_sisyphusPrime_attack5", list);
			return list;
		}

		private static void TraverseCodeAndReplaceSubtitles(string subtitlesKey, List<CodeInstruction> instructions)
		{
			for (int i = 0; i < instructions.Count; i++)
			{
				if (DisplaySubtitleCall(instructions[i]))
				{
					ReplaceLdstr(i - 3, subtitlesKey, instructions);
					break;
				}
			}
		}

		private static bool DisplaySubtitleCall(CodeInstruction instruction)
		{
			if (instruction.opcode == OpCodes.Callvirt)
			{
				return CodeInstructionExtensions.OperandIs(instruction, (MemberInfo)AccessTools.Method(typeof(SubtitleController), "DisplaySubtitle", new Type[3]
				{
					typeof(string),
					typeof(AudioSource),
					typeof(bool)
				}, (Type[])null));
			}
			return false;
		}

		private static void ReplaceLdstr(int offset, string subtitlesKey, List<CodeInstruction> instructions)
		{
			// Preserve branch labels from the original instruction before removing it.
			List<Label> originalLabels = instructions[offset].labels;
			instructions.RemoveAt(offset);

			CodeInstruction ldstr = new CodeInstruction(OpCodes.Ldstr, subtitlesKey);
			// Transfer labels to the new Ldstr so any branch targeting this position still works.
			ldstr.labels.AddRange(originalLabels);

			instructions.InsertRange(offset, new List<CodeInstruction>
			{
				ldstr,
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SubtitlesHelper), nameof(SubtitlesHelper.GetText), new[] { typeof(string) }))
			});
		}
	}
}