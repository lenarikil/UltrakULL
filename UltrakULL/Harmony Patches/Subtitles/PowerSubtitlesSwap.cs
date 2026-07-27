using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.Harmony_Patches.Subtitles
{
	[HarmonyPatch(typeof(PowerVoiceController))]
	public static class PowerSubtitlesSwap
	{
		private static readonly HashSet<string> CurrentlyHandledPowerClips = new HashSet<string>();

		public static void RegisterPowerClipAsHandled(string clipName)
		{
			if (!string.IsNullOrEmpty(clipName))
			{
				lock (CurrentlyHandledPowerClips)
				{
					CurrentlyHandledPowerClips.Add(clipName);
				}
			}
		}

		public static void UnregisterPowerClip(string clipName)
		{
			if (!string.IsNullOrEmpty(clipName))
			{
				lock (CurrentlyHandledPowerClips)
				{
					CurrentlyHandledPowerClips.Remove(clipName);
				}
			}
		}

		/// <summary>
		/// Clears all handled clip flags. Should be called when switching languages
		/// to prevent stale flags from suppressing original subtitles.
		/// </summary>
		public static void ClearHandledClips()
		{
			lock (CurrentlyHandledPowerClips)
			{
				CurrentlyHandledPowerClips.Clear();
			}
			Logging.Info("[PowerSubtitlesSwap] Cleared all handled clip flags.");
		}

		private static bool IsPowerClipBeingHandled(string clipName)
		{
			if (string.IsNullOrEmpty(clipName))
			{
				return false;
			}
			lock (CurrentlyHandledPowerClips)
			{
				return CurrentlyHandledPowerClips.Contains(clipName);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Intro")]
		private static bool Intro_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("intro", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastIntro");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if ((UnityEngine.Object)val != (UnityEngine.Object)null && IsPowerClipBeingHandled(((UnityEngine.Object)val).name))
						{
							Logging.Info($"[PowerSubtitlesSwap] Intro clip '{val.name}' is being handled, suppressing subtitle.");
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Intro_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Enrage")]
		private static bool Enrage_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("enrage", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastEnrage");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Enrage_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Taunt")]
		private static bool Taunt_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("taunt", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastTaunt");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Taunt_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "CheapShot")]
		private static bool CheapShot_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("cheapShot", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastCheapShot");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in CheapShot_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Rapier")]
		private static bool Rapier_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("rapier", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastRapier");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Rapier_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Greatsword")]
		private static bool Greatsword_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("greatsword", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastGreatsword");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Greatsword_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Spear")]
		private static bool Spear_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("spear", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastSpear");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Spear_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "SpearThrow")]
		private static bool SpearThrow_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("spearThrow", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastSpearThrow");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in SpearThrow_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "Glaive")]
		private static bool Glaive_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("glaive", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastGlaive");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in Glaive_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PowerVoiceController), "GlaiveThrow")]
		private static bool GlaiveThrow_Prefix(PowerVoiceController __instance)
		{
			try
			{
				FieldInfo field = typeof(PowerVoiceController).GetField("glaiveThrow", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field.GetValue(__instance) is AudioClip[] array && array.Length != 0)
				{
					int? index = GetIndex(__instance, "lastGlaiveThrow");
					if (index.HasValue && index.Value >= 0 && index.Value < array.Length)
					{
						AudioClip val = array[index.Value];
						if (val != null && IsPowerClipBeingHandled(val.name))
						{
							return false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Error in GlaiveThrow_Prefix: " + ex.Message);
			}
			return true;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Intro")]
		private static IEnumerable<CodeInstruction> Intro_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceSubtitleInMethod(instructions, "power_intro");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Enrage")]
		private static IEnumerable<CodeInstruction> Enrage_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceSubtitleInMethod(instructions, "power_enrage");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Taunt")]
		private static IEnumerable<CodeInstruction> Taunt_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceSubtitleInMethod(instructions, "power_taunt");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "CheapShot")]
		private static IEnumerable<CodeInstruction> CheapShot_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceSubtitleInMethod(instructions, "power_cheapShot");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Rapier")]
		private static IEnumerable<CodeInstruction> Rapier_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceFixedSubtitle(instructions, "power_rapier");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Greatsword")]
		private static IEnumerable<CodeInstruction> Greatsword_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceFixedSubtitle(instructions, "power_greatsword");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Spear")]
		private static IEnumerable<CodeInstruction> Spear_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceFixedSubtitle(instructions, "power_spear");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "SpearThrow")]
		private static IEnumerable<CodeInstruction> SpearThrow_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceFixedSubtitle(instructions, "power_spearThrow");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "Glaive")]
		private static IEnumerable<CodeInstruction> Glaive_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceFixedSubtitle(instructions, "power_glaive");
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PowerVoiceController), "GlaiveThrow")]
		private static IEnumerable<CodeInstruction> GlaiveThrow_Transpile(IEnumerable<CodeInstruction> instructions)
		{
			return ReplaceFixedSubtitle(instructions, "power_glaiveThrow");
		}

		/// <summary>
		/// Replaces hardcoded subtitle strings in methods that have variant-based subtitles (Intro, Enrage, Taunt, CheapShot).
		/// Uses SubtitlesHelper.GetText() to fetch the current language's subtitle at runtime.
		/// Always replaces the Ldstr with a call to SubtitlesHelper.GetText, without checking
		/// the current language value at IL patch time. This ensures the replacement works
		/// even if LanguageManager hasn't been initialized yet.
		/// Explicitly transfers branch labels from the replaced instruction to the new Ldstr.
		/// </summary>
		private static IEnumerable<CodeInstruction> ReplaceSubtitleInMethod(IEnumerable<CodeInstruction> instructions, string baseKey)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].opcode == OpCodes.Ldstr && list[i].operand is string text)
				{
					int variantIndexFromOriginal = GetVariantIndexFromOriginal(text);
					string subtitleKey = $"subtitles_{baseKey}_{variantIndexFromOriginal}";

					// Preserve branch labels from the original instruction before removing it.
					List<Label> originalLabels = list[i].labels;
					list.RemoveAt(i);

					CodeInstruction ldstr = new CodeInstruction(OpCodes.Ldstr, subtitleKey);
					// Transfer labels to the new Ldstr so any branch targeting this position still works.
					ldstr.labels.AddRange(originalLabels);

					list.InsertRange(i, new List<CodeInstruction>
					{
						ldstr,
						new CodeInstruction(OpCodes.Call,
							AccessTools.Method(typeof(SubtitlesHelper), nameof(SubtitlesHelper.GetText), new[] { typeof(string) }))
					});
					i++; // Skip the newly inserted Call instruction
				}
			}

			return list;
		}

		/// <summary>
		/// Replaces hardcoded subtitle strings in methods that have fixed subtitles (Rapier, Greatsword, Spear, etc.).
		/// Uses SubtitlesHelper.GetText() to fetch the current language's subtitle at runtime.
		/// Always replaces the Ldstr without checking the current language value at IL patch time.
		/// Explicitly transfers branch labels from the replaced instruction to the new Ldstr.
		/// </summary>
		private static IEnumerable<CodeInstruction> ReplaceFixedSubtitle(IEnumerable<CodeInstruction> instructions, string baseKey)
		{
			List<CodeInstruction> list = new List<CodeInstruction>(instructions);

			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i].opcode == OpCodes.Ldstr) || !(list[i].operand is string text))
				{
					continue;
				}

				// For spearThrow and glaiveThrow, only replace the specific matching strings,
				// leave other Ldstr instructions in those methods untouched.
				if (baseKey == "power_spearThrow" && text != "Over here!")
					continue;
				if (baseKey == "power_glaiveThrow" && text != "Take THIS!")
					continue;

				string subtitleKey = "subtitles_" + baseKey;

				// Preserve branch labels from the original instruction before removing it.
				List<Label> originalLabels = list[i].labels;
				list.RemoveAt(i);

				CodeInstruction ldstr = new CodeInstruction(OpCodes.Ldstr, subtitleKey);
				// Transfer labels to the new Ldstr so any branch targeting this position still works.
				ldstr.labels.AddRange(originalLabels);

				list.InsertRange(i, new List<CodeInstruction>
				{
					ldstr,
					new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(SubtitlesHelper), nameof(SubtitlesHelper.GetText), new[] { typeof(string) }))
				});
				i++; // Skip the newly inserted Call instruction
			}

			return list;
		}

		private static int GetVariantIndexFromOriginal(string originalText)
		{
			if (string.IsNullOrEmpty(originalText))
				return 0;

			switch (originalText)
			{
				case "Be afraid, machine.":
					return 0;
				case "Here shall be your grave.":
					return 1;
				case "It is over, machine!":
					return 2;
				case "Surrender or perish!":
					return 3;
				case "Lay down and die!":
					return 4;
				case "Bastard!":
					return 0;
				case "You piece of SHIT!":
					return 1;
				case "Just DIE already!":
					return 2;
				case "Why won't you die!?":
					return 3;
				case "God DAMN it!":
					return 4;
				case "This lowly thing could never have bested him!":
					return 0;
				case "An inconvenience at best.":
					return 1;
				case "This is a waste of my time!":
					return 2;
				case "Just another worthless object.":
					return 3;
				case "PAY ATTENTION!":
					return 0;
				case "Wait your TURN!":
					return 1;
				case "WRONG TARGET!":
					return 2;
				default:
					Logging.Warn("[PowerTranspiler] GetVariantIndexFromOriginal: UNKNOWN string '" + originalText + "'");
					return 0;
			}
		}

		private static bool IsDisplaySubtitleCall(CodeInstruction instruction)
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

		private static string GetSubtitlesFieldSafe(string key)
		{
			try
			{
				var a = LanguageManager.CurrentLanguage?.subtitles;
				if (a == null)
				{
					Logging.Warn("[PowerSubtitlesSwap] Key is null in CurrentLanguage. Using fallback.");
					return null;
				}
				FieldInfo field = a.GetType().GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null)
				{
					Logging.Warn("[PowerSubtitlesSwap] Field '" + key + "' not found in Subtitles. Using fallback.");
					return null;
				}
				string text = field.GetValue(a) as string;

				return text;
			}
			catch (Exception ex)
			{
				Logging.Error("[PowerSubtitlesSwap] Exception in GetSubtitlesFieldSafe for key '" + key + "': " + ex.Message);
				return null;
			}
		}

		private static int? GetIndex(PowerVoiceController instance, string fieldName)
		{
			try
			{
				return typeof(PowerVoiceController).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(instance) as int?;
			}
			catch (Exception ex)
			{
				Logging.Warn("[PowerSubtitlesSwap] Failed to get index for '" + fieldName + "': " + ex.Message);
				return null;
			}
		}
	}
}