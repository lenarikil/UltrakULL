using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;

namespace UltrakULL.Harmony_Patches.AudioSwaps
{
	[HarmonyPatch(typeof(SisyphusPrime), "Start")]
	public class SisyphusPrimeAudioSwap
	{
		private static readonly Dictionary<string, AudioClip[]> _savedArrays = new Dictionary<string, AudioClip[]>();
		private static readonly Dictionary<string, AudioClip> _savedSingles = new Dictionary<string, AudioClip>();
		private static readonly object _lock = new object();

		private static readonly string[] ArrayFieldNames = {
			"clapVoice", "explosionVoice", "hurtVoice",
			"stompComboVoice", "tauntVoice", "uppercutComboVoice"
		};

		private static void SaveOriginals(SisyphusPrime instance)
		{
			if (instance == null) return;
			int id = instance.GetInstanceID();

			foreach (string fieldName in ArrayFieldNames)
			{
				var field = typeof(SisyphusPrime).GetField(fieldName);
				if (field == null) continue;
				AudioClip[] clips = field.GetValue(instance) as AudioClip[];
				if (clips == null) continue;
				string key = $"{id}_{fieldName}";
				if (!_savedArrays.ContainsKey(key))
				{
					_savedArrays[key] = (AudioClip[])clips.Clone();
					Logging.Info($"[SisyphusPrimeAudioSwap] Saved {clips.Length} original clips for {fieldName} (instance {id})");
				}
			}

			string singleKey = $"{id}_phaseChangeVoice";
			if (!_savedSingles.ContainsKey(singleKey) && instance.phaseChangeVoice != null)
			{
				_savedSingles[singleKey] = instance.phaseChangeVoice;
				Logging.Info($"[SisyphusPrimeAudioSwap] Saved original phaseChangeVoice '{instance.phaseChangeVoice.name}' (instance {id})");
			}
		}

		private static void RestoreOriginals(SisyphusPrime instance)
		{
			if (instance == null) return;
			int id = instance.GetInstanceID();

			lock (_lock)
			{
				foreach (string fieldName in ArrayFieldNames)
				{
					string key = $"{id}_{fieldName}";
					if (_savedArrays.TryGetValue(key, out AudioClip[] saved))
					{
						var field = typeof(SisyphusPrime).GetField(fieldName);
						if (field == null) continue;
						AudioClip[] current = field.GetValue(instance) as AudioClip[];
						if (current != null && current.Length == saved.Length)
						{
							Array.Copy(saved, current, saved.Length);
							Logging.Info($"[SisyphusPrimeAudioSwap] Restored {saved.Length} original clips for {fieldName} (instance {id})");
						}
					}
				}

				string singleKey = $"{id}_phaseChangeVoice";
				if (_savedSingles.TryGetValue(singleKey, out AudioClip savedClip))
				{
					instance.phaseChangeVoice = savedClip;
					Logging.Info($"[SisyphusPrimeAudioSwap] Restored phaseChangeVoice '{savedClip.name}' (instance {id})");
				}

				_savedArrays.Clear();
				_savedSingles.Clear();
			}
		}

		[HarmonyPostfix]
		public static void SisyphusPrimeAudioSwapPatch(ref SisyphusPrime __instance)
		{
			SaveOriginals(__instance);

			if (LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value == "False" || CommonFunctions.isUsingEnglish())
			{
				return;
			}
			SisyphusPrime instance = __instance;
			AudioPreloadManager.EnsureCurrentScenePreloaded(delegate { ApplyAudioSwap(instance); });
		}

		/// <summary>
		/// Re-applies audio swaps to all existing SisyphusPrime instances in the scene.
		/// Called when the language is changed without reloading the scene.
		/// </summary>
		public static void RebindExistingInstances()
		{
			SisyphusPrime[] instances = UnityEngine.Object.FindObjectsOfType<SisyphusPrime>(true);

			if (LanguageManager.configFile.Bind<string>("General", "activeDubbing", "False", (ConfigDescription)null).Value == "False" || CommonFunctions.isUsingEnglish())
			{
				foreach (var instance in instances)
				{
					if (instance == null) continue;
					RestoreOriginals(instance);
				}
				return;
			}

			foreach (var instance in instances)
			{
				if (instance == null) continue;
				SaveOriginals(instance);
				AudioPreloadManager.EnsureCurrentScenePreloaded(delegate { ApplyAudioSwap(instance); });
			}
		}

		private static void ApplyAudioSwap(SisyphusPrime __instance)
		{
			if (__instance == null)
			{
				return;
			}

			AudioSwapper.LogAudioSourceDiagnostics(__instance.GetComponent<AudioSource>(), "SisyphusPrime");
			string text = AudioSwapper.SpeechFolder + "sisyphusPrime\\";
			AudioClip[] begoneAttacks = __instance.clapVoice;
			for (int i = 0; i < begoneAttacks.Length; i++)
			{
				int ix = i;
				string audioFilePath = text + "sisyphusBegone" + (ix + 1);
				AudioSwapper.SwapClipInArrayAsync(begoneAttacks, ix, audioFilePath);
			}
			AudioClip[] thisWillHurtAttack = __instance.explosionVoice;
			for (int num = 0; num < thisWillHurtAttack.Length; num++)
			{
				int ix2 = num;
				string audioFilePath2 = text + "sisyphusThisWillHurt";
				AudioSwapper.SwapClipInArrayAsync(thisWillHurtAttack, ix2, audioFilePath2);
			}
			AudioClip[] grunt = __instance.hurtVoice;
			for (int num2 = 0; num2 < grunt.Length; num2++)
			{
				int ix3 = num2;
				string audioFilePath3 = text + "sisyphusGrunt";
				AudioSwapper.SwapClipInArrayAsync(grunt, ix3, audioFilePath3);
			}
			AudioClip[] stompAttacks = __instance.stompComboVoice;
			for (int num3 = 0; num3 < stompAttacks.Length; num3++)
			{
				int ix4 = num3;
				string audioFilePath4 = text + "sisyphusYouCantEscape" + (ix4 + 1);
				AudioSwapper.SwapClipInArrayAsync(stompAttacks, ix4, audioFilePath4);
			}
			AudioClip[] taunts = __instance.tauntVoice;
			for (int num4 = 0; num4 < taunts.Length; num4++)
			{
				int ix5 = num4;
				string audioFilePath5 = text + "sisyphusNiceTry" + (ix5 + 1);
				AudioSwapper.SwapClipInArrayAsync(taunts, ix5, audioFilePath5);
			}
			AudioClip[] uppercutAttacks = __instance.uppercutComboVoice;
			for (int num5 = 0; num5 < uppercutAttacks.Length; num5++)
			{
				int ix6 = num5;
				string audioFilePath6 = text + "sisyphusDestroy" + (ix6 + 1);
				AudioSwapper.SwapClipInArrayAsync(uppercutAttacks, ix6, audioFilePath6);
			}
			SisyphusPrime inst = __instance;
			AudioClip phaseChangeVoice = inst.phaseChangeVoice;
			string audioFilePath7 = text + "sisyphusYesThatsIt";
			AudioSwapper.SwapClipWithFileAsync(phaseChangeVoice, audioFilePath7, delegate(AudioClip clip)
			{
				try
				{
					inst.phaseChangeVoice = clip;
				}
				catch
				{
				}
			});
		}
	}
}
