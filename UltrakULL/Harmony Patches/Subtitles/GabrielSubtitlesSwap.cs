using System.Collections.Generic;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.Harmony_Patches.Subtitles
{
	[HarmonyPatch(typeof(GabrielVoice), "Start")]
	public static class GabrielSubtitlesSwap
	{
		// Taunt order is built at runtime rather than in static initializers so the values
		// always reflect the language that is active when Gabriel actually spawns.
		private static List<string> BuildFirstEncounterTauntsOrder()
		{
			var subtitles = LanguageManager.CurrentLanguage.subtitles;
			return new List<string>
			{
				subtitles.subtitles_gabriel_taunt2,
				subtitles.subtitles_gabriel_taunt3,
				subtitles.subtitles_gabriel_taunt8,
				subtitles.subtitles_gabriel_taunt6,
				subtitles.subtitles_gabriel_taunt9,
				subtitles.subtitles_gabriel_taunt4,
				subtitles.subtitles_gabriel_taunt5,
				subtitles.subtitles_gabriel_taunt7,
				subtitles.subtitles_gabriel_taunt1,
				subtitles.subtitles_gabriel_taunt12,
				subtitles.subtitles_gabriel_taunt10,
				subtitles.subtitles_gabriel_taunt11
			};
		}

		private static List<string> BuildSecondEncounterTauntsOrder()
		{
			var subtitles = LanguageManager.CurrentLanguage.subtitles;
			return new List<string>
			{
				subtitles.subtitles_gabrielSecondTaunt6,
				subtitles.subtitles_gabrielSecondTaunt5,
				subtitles.subtitles_gabrielSecondTaunt8,
				subtitles.subtitles_gabrielSecondTaunt4,
				subtitles.subtitles_gabrielSecondTaunt9,
				subtitles.subtitles_gabrielSecondTaunt7,
				subtitles.subtitles_gabrielSecondTaunt1
			};
		}

		private static List<string> BuildSecondEncounterPhaseTwoTauntsOrder()
		{
			var subtitles = LanguageManager.CurrentLanguage.subtitles;
			return new List<string>
			{
				subtitles.subtitles_gabrielSecondTaunt11,
				subtitles.subtitles_gabrielSecondTaunt12,
				subtitles.subtitles_gabrielSecondTaunt13,
				subtitles.subtitles_gabrielSecondTaunt3,
				subtitles.subtitles_gabrielSecondTaunt2,
				subtitles.subtitles_gabrielSecondTaunt10,
				subtitles.subtitles_gabrielSecondTaunt14
			};
		}

		[HarmonyPostfix]
		public static void GabrielVoice_Start(ref GabrielVoice __instance, ref string[] ___taunts, ref string[] ___tauntsSecondPhase)
		{
			// Built-in English keeps the game's original Gabriel subtitles.
			if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

			if (((Object)((Component)__instance).gameObject).name.Contains("2nd"))
			{
				var secondEncounterTauntsOrder = BuildSecondEncounterTauntsOrder();
				var secondEncounterPhaseTwoTauntsOrder = BuildSecondEncounterPhaseTwoTauntsOrder();
				__instance.phaseChangeSubtitle = LanguageManager.CurrentLanguage.subtitles.subtitles_gabrielSecondPhaseChange;
				for (int i = 0; i < secondEncounterTauntsOrder.Count; i++)
				{
					___taunts[i] = secondEncounterTauntsOrder[i];
				}
				for (int j = 0; j < secondEncounterPhaseTwoTauntsOrder.Count; j++)
				{
					___tauntsSecondPhase[j] = secondEncounterPhaseTwoTauntsOrder[j];
				}
				SubtitledAudioSourcesReplacer.ReplaceSubsAndAudio();
			}
			else
			{
				var firstEncounterTauntsOrder = BuildFirstEncounterTauntsOrder();
				__instance.phaseChangeSubtitle = LanguageManager.CurrentLanguage.subtitles.subtitles_gabriel_phaseChange;
				for (int k = 0; k < firstEncounterTauntsOrder.Count; k++)
				{
					___taunts[k] = firstEncounterTauntsOrder[k];
				}
				SubtitledAudioSourcesReplacer.ReplaceSubsAndAudio();
			}
		}
	}
}
