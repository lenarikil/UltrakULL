using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;

// The class name clashes with the namespace of this patch, so it is aliased.
using JsonSubtitles = UltrakULL.json.Subtitles;

namespace UltrakULL.Harmony_Patches.Subtitles
{
	[HarmonyPatch(typeof(GabrielVoice), "Start")]
	public static class GabrielSubtitlesSwap
	{
		private const BindingFlags VoiceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		private static readonly FieldInfo TauntsField = typeof(GabrielVoice).GetField("taunts", VoiceFlags);
		private static readonly FieldInfo TauntsSecondPhaseField = typeof(GabrielVoice).GetField("tauntsSecondPhase", VoiceFlags);
		private static readonly FieldInfo PhaseChangeField = typeof(GabrielVoice).GetField("phaseChangeSubtitle", VoiceFlags);

		// The keys follow the order of the game's own taunt arrays, so index i of a
		// taunts array always belongs to the key at index i here.
		private static readonly string[] FirstEncounterTauntKeys =
		{
			"subtitles_gabriel_taunt2",
			"subtitles_gabriel_taunt3",
			"subtitles_gabriel_taunt8",
			"subtitles_gabriel_taunt6",
			"subtitles_gabriel_taunt9",
			"subtitles_gabriel_taunt4",
			"subtitles_gabriel_taunt5",
			"subtitles_gabriel_taunt7",
			"subtitles_gabriel_taunt1",
			"subtitles_gabriel_taunt12",
			"subtitles_gabriel_taunt10",
			"subtitles_gabriel_taunt11"
		};

		private static readonly string[] SecondEncounterTauntKeys =
		{
			"subtitles_gabrielSecondTaunt6",
			"subtitles_gabrielSecondTaunt5",
			"subtitles_gabrielSecondTaunt8",
			"subtitles_gabrielSecondTaunt4",
			"subtitles_gabrielSecondTaunt9",
			"subtitles_gabrielSecondTaunt7",
			"subtitles_gabrielSecondTaunt1"
		};

		private static readonly string[] SecondEncounterPhaseTwoTauntKeys =
		{
			"subtitles_gabrielSecondTaunt11",
			"subtitles_gabrielSecondTaunt12",
			"subtitles_gabrielSecondTaunt13",
			"subtitles_gabrielSecondTaunt3",
			"subtitles_gabrielSecondTaunt2",
			"subtitles_gabrielSecondTaunt10",
			"subtitles_gabrielSecondTaunt14"
		};

		private const string FirstEncounterPhaseChangeKey = "subtitles_gabriel_phaseChange";
		private const string SecondEncounterPhaseChangeKey = "subtitles_gabrielSecondPhaseChange";

		private sealed class VoiceOriginals
		{
			public GabrielVoice Voice;
			public bool Second;
			public string[] Taunts;
			public string[] TauntsSecondPhase;
			public string PhaseChangeSubtitle;
		}

		private static readonly object SaveLock = new object();
		private static readonly Dictionary<int, VoiceOriginals> SavedOriginals = new Dictionary<int, VoiceOriginals>();

		/// <summary>
		/// Re-applies the active language to every GabrielVoice already present in
		/// the scene and to the taunt subtitles that are on screen. New instances
		/// are handled by the Start postfix, so this only covers the ones that
		/// spawned before the switch. Switching to the built-in English restores
		/// the game's own strings instead.
		/// </summary>
		public static void Reapply()
		{
			PruneDeadOriginals();

			bool english = UltrakULL.CommonFunctions.isUsingEnglish();

			GabrielVoice[] voices = UnityEngine.Object.FindObjectsOfType<GabrielVoice>(true);
			foreach (GabrielVoice voice in voices)
			{
				if (voice == null) continue;
				try
				{
					if (english)
					{
						RestoreOriginals(voice);
					}
					else
					{
						SaveOriginals(voice);
						ApplyLanguage(voice);
					}
				}
				catch (Exception e)
				{
					UltrakULL.Logging.Warn("GabrielSubtitlesSwap.Reapply failed: " + e.Message);
				}
			}

			RegisterTauntTexts();
			RefreshOnScreenSubtitles();
		}

		[HarmonyPostfix]
		public static void GabrielVoice_Start(GabrielVoice __instance)
		{
			// The game's own strings are kept before anything is overwritten, so
			// switching back to the built-in English restores them exactly.
			SaveOriginals(__instance);

			// The texts are registered before the first taunt can be shown, so a
			// subtitle that is still on screen during a later language switch can
			// be found by its text.
			RegisterTauntTexts();

			// Built-in English keeps the game's original Gabriel subtitles.
			if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

			ApplyLanguage(__instance);

			SubtitledAudioSourcesReplacer.ReplaceSubsAndAudio();
		}

		private static bool IsSecondEncounter(GabrielVoice voice)
		{
			return voice.gameObject.name.Contains("2nd");
		}

		private static void ApplyLanguage(GabrielVoice voice)
		{
			JsonFormat current = LanguageManager.CurrentLanguage;
			JsonSubtitles subtitles = current != null ? current.subtitles : null;
			if (subtitles == null) return;

			bool second = IsSecondEncounter(voice);

			if (second)
			{
				WriteTaunts(TauntsField, voice, GetTauntValues(subtitles, SecondEncounterTauntKeys));
				WriteTaunts(TauntsSecondPhaseField, voice, GetTauntValues(subtitles, SecondEncounterPhaseTwoTauntKeys));
				if (PhaseChangeField != null)
					PhaseChangeField.SetValue(voice, subtitles.subtitles_gabrielSecondPhaseChange);
			}
			else
			{
				WriteTaunts(TauntsField, voice, GetTauntValues(subtitles, FirstEncounterTauntKeys));
				if (PhaseChangeField != null)
					PhaseChangeField.SetValue(voice, subtitles.subtitles_gabriel_phaseChange);
			}
		}

		private static string[] GetTauntValues(JsonSubtitles subtitles, string[] keys)
		{
			string[] values = new string[keys.Length];
			for (int i = 0; i < keys.Length; i++)
				values[i] = subtitles.GetField(keys[i]);
			return values;
		}

		private static void WriteTaunts(FieldInfo field, GabrielVoice voice, string[] values)
		{
			if (field == null || values == null) return;

			string[] arr = field.GetValue(voice) as string[];
			if (arr == null) return;

			int count = Math.Min(arr.Length, values.Length);
			for (int i = 0; i < count; i++)
				arr[i] = values[i];
		}

		private static void SaveOriginals(GabrielVoice voice)
		{
			if (voice == null) return;
			int id = voice.GetInstanceID();

			lock (SaveLock)
			{
				// Saved only once: after the first localization the arrays hold
				// translated strings, which must never become the "originals".
				if (SavedOriginals.ContainsKey(id)) return;

				VoiceOriginals originals = new VoiceOriginals();
				originals.Voice = voice;
				originals.Second = IsSecondEncounter(voice);

				string[] taunts = TauntsField != null ? TauntsField.GetValue(voice) as string[] : null;
				if (taunts != null)
					originals.Taunts = (string[])taunts.Clone();

				if (originals.Second && TauntsSecondPhaseField != null)
				{
					string[] phaseTwo = TauntsSecondPhaseField.GetValue(voice) as string[];
					if (phaseTwo != null)
						originals.TauntsSecondPhase = (string[])phaseTwo.Clone();
				}

				if (PhaseChangeField != null)
					originals.PhaseChangeSubtitle = PhaseChangeField.GetValue(voice) as string;

				SavedOriginals[id] = originals;
			}
		}

		private static void RestoreOriginals(GabrielVoice voice)
		{
			if (voice == null) return;
			int id = voice.GetInstanceID();

			lock (SaveLock)
			{
				VoiceOriginals originals;
				if (!SavedOriginals.TryGetValue(id, out originals)) return;

				CopyInto(TauntsField, voice, originals.Taunts);
				CopyInto(TauntsSecondPhaseField, voice, originals.TauntsSecondPhase);
				if (PhaseChangeField != null && originals.PhaseChangeSubtitle != null)
					PhaseChangeField.SetValue(voice, originals.PhaseChangeSubtitle);
			}
		}

		private static void CopyInto(FieldInfo field, GabrielVoice voice, string[] values)
		{
			if (field == null || values == null) return;

			string[] arr = field.GetValue(voice) as string[];
			if (arr == null || arr.Length != values.Length) return;

			Array.Copy(values, arr, values.Length);
		}

		private static void PruneDeadOriginals()
		{
			lock (SaveLock)
			{
				if (SavedOriginals.Count == 0) return;

				List<int> dead = null;
				foreach (KeyValuePair<int, VoiceOriginals> pair in SavedOriginals)
				{
					if (pair.Value == null || pair.Value.Voice == null)
					{
						if (dead == null) dead = new List<int>();
						dead.Add(pair.Key);
					}
				}

				if (dead == null) return;
				foreach (int id in dead)
					SavedOriginals.Remove(id);
			}
		}

		/// <summary>
		/// Remembers the text of every taunt line with its key, so a taunt that is
		/// on screen can be rewritten in another language. The text on screen
		/// belongs to the language that was active when the taunt was shown, so
		/// the texts of every loaded pack are registered, not just the active one.
		/// </summary>
		private static void RegisterTauntTexts()
		{
			try
			{
				foreach (KeyValuePair<string, JsonFormat> pair in LanguageManager.allLanguages)
				{
					JsonSubtitles packSubtitles = pair.Value != null ? pair.Value.subtitles : null;
					if (packSubtitles == null) continue;

					RegisterPackTexts(packSubtitles);
				}

				// The active language is registered last, so its (for RTL languages
				// fixed) strings win when a pack shares a raw string with it.
				JsonFormat current = LanguageManager.CurrentLanguage;
				if (current != null && current.subtitles != null)
					RegisterPackTexts(current.subtitles);

				// The game's own strings of the voices that are in the scene are
				// registered as well, so a taunt shown in the built-in English is
				// found even when the en-GB pack does not repeat the game's strings
				// exactly.
				RegisterSavedOriginalTexts();
			}
			catch (Exception e)
			{
				UltrakULL.Logging.Warn("GabrielSubtitlesSwap.RegisterTauntTexts failed: " + e.Message);
			}
		}

		private static void RegisterPackTexts(JsonSubtitles subtitles)
		{
			RegisterArrayTexts(subtitles, FirstEncounterTauntKeys);
			RegisterArrayTexts(subtitles, SecondEncounterTauntKeys);
			RegisterArrayTexts(subtitles, SecondEncounterPhaseTwoTauntKeys);
			UltrakULL.LiveSubtitles.RegisterText(FirstEncounterPhaseChangeKey, subtitles.GetField(FirstEncounterPhaseChangeKey));
			UltrakULL.LiveSubtitles.RegisterText(SecondEncounterPhaseChangeKey, subtitles.GetField(SecondEncounterPhaseChangeKey));
		}

		private static void RegisterArrayTexts(JsonSubtitles subtitles, string[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
				UltrakULL.LiveSubtitles.RegisterText(keys[i], subtitles.GetField(keys[i]));
		}

		private static void RegisterSavedOriginalTexts()
		{
			lock (SaveLock)
			{
				foreach (KeyValuePair<int, VoiceOriginals> pair in SavedOriginals)
				{
					VoiceOriginals originals = pair.Value;
					if (originals == null) continue;

					string[] tauntKeys = originals.Second ? SecondEncounterTauntKeys : FirstEncounterTauntKeys;
					RegisterOriginalArray(originals.Taunts, tauntKeys);
					RegisterOriginalArray(originals.TauntsSecondPhase, SecondEncounterPhaseTwoTauntKeys);

					if (!string.IsNullOrEmpty(originals.PhaseChangeSubtitle))
					{
						UltrakULL.LiveSubtitles.RegisterText(
							originals.Second ? SecondEncounterPhaseChangeKey : FirstEncounterPhaseChangeKey,
							originals.PhaseChangeSubtitle);
					}
				}
			}
		}

		private static void RegisterOriginalArray(string[] values, string[] keys)
		{
			if (values == null) return;

			int count = Math.Min(values.Length, keys.Length);
			for (int i = 0; i < count; i++)
				UltrakULL.LiveSubtitles.RegisterText(keys[i], values[i]);
		}

		/// <summary>
		/// Links the subtitles that are on screen to their keys. A taunt that is
		/// still on screen was linked when it appeared, but the texts of packs that
		/// are no longer active can also be on screen, and those were registered
		/// only after the switch. Feeding every on-screen subtitle through
		/// OnDisplayed again links the ones that match, so the reapply below
		/// rewrites them in the active language.
		/// </summary>
		private static void RefreshOnScreenSubtitles()
		{
			try
			{
				Subtitle[] subtitles = UnityEngine.Object.FindObjectsOfType<Subtitle>(true);
				foreach (Subtitle sub in subtitles)
				{
					if (sub == null) continue;

					TMPro.TMP_Text tmp = sub.GetComponentInChildren<TMPro.TMP_Text>(true);
					if (tmp == null) continue;

					string current = tmp.text;
					if (string.IsNullOrEmpty(current)) continue;

					try
					{
						UltrakULL.LiveSubtitles.OnDisplayed(sub, current);
					}
					catch { }
				}

				UltrakULL.LiveSubtitles.Reapply();
			}
			catch (Exception e)
			{
				UltrakULL.Logging.Warn("GabrielSubtitlesSwap.RefreshOnScreenSubtitles failed: " + e.Message);
			}
		}
	}
}
