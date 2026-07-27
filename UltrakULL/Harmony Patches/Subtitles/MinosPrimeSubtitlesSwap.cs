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
	[HarmonyPatch(typeof(MinosPrime))]
	public class MinosPrimeSubtitlesSwap
	{
		private const int LdstrInstructionOffset = 3;

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "Update")]
		private static IEnumerable<CodeInstruction> MinosPrime_Update(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_phaseChange", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "Combo")]
		private static IEnumerable<CodeInstruction> MinosPrime_Combo(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_attack1", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "Boxing")]
		private static IEnumerable<CodeInstruction> MinosPrime_Boxing(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_attack2", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "RiderKick")]
		private static IEnumerable<CodeInstruction> MinosPrime_RiderKick(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_attack3", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "DropAttack")]
		private static IEnumerable<CodeInstruction> MinosPrime_DropAttack(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_attack4", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "Dropkick")]
		private static IEnumerable<CodeInstruction> MinosPrime_Dropkick(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_attack5", list);
			return list;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MinosPrime), "Enrage")]
		private static IEnumerable<CodeInstruction> MinosPrime_Enrage(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList();
			TraverseCodeAndReplaceSubtitles("subtitles_minosPrime_phaseChange", list);
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