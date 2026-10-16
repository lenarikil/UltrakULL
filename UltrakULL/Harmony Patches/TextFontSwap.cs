using System;
using System.Collections.Generic;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BepInEx;
using UltrakULL;

namespace UltrakULL.Harmony_Patches
{
	public class TextFontSwap
	{
		[HarmonyPatch(typeof(Text), "OnEnable")]
		public static class TextFontSwapper
		{
			private static HashSet<IntPtr> objectsFixed = new HashSet<IntPtr>();

			public static void ClearCache()
			{
				objectsFixed.Clear();
			}

			[HarmonyPostfix]
			public static void SwapFont(ref Text __instance, IntPtr ___m_CachedPtr)
			{
				if (objectsFixed.Count > 0 && objectsFixed.Contains(___m_CachedPtr))
				{
					return;
				}
				if (LanguageManager.IsRightToLeft)
				{
					TextAnchor alignment = __instance.alignment;
					switch ((int)alignment)
					{
					case 0:
						__instance.alignment = (TextAnchor)2;
						break;
					case 3:
						__instance.alignment = (TextAnchor)5;
						break;
					case 6:
						__instance.alignment = (TextAnchor)8;
						break;
					case 2:
						__instance.alignment = (TextAnchor)0;
						break;
					case 5:
						__instance.alignment = (TextAnchor)3;
						break;
					case 8:
						__instance.alignment = (TextAnchor)6;
						break;
					}
					__instance.alignByGeometry = true;
				}

				// Log the original font before any replacement
				FontLogger.LogFont(__instance.font);

				if (Core.GlobalFontReady)
				{
					if (CommonFunctions.GetCurrentSceneName() == "CreditsMuseum2")
					{
						if (__instance.font.fontNames[0] == "GFS Garaldus")
						{
							__instance.font = Core.MuseumFont;
						}
						else
						{
							__instance.font = Core.GlobalFont;
						}
					}
					else
					{
						__instance.font = Core.GlobalFont;
					}
				}
				objectsFixed.Add(___m_CachedPtr);
			}
		}

		private static class FontLogger
		{
			private static HashSet<string> loggedFonts = new HashSet<string>();
			private static readonly object lockObject = new object();
			private static string logFilePath = null;

			private static string GetLogFilePath()
			{
				if (logFilePath == null)
				{
					logFilePath = Path.Combine(Paths.ConfigPath, "ultrakull", "fonts.txt");
				}
				return logFilePath;
			}

			public static void LogFont(Font font)
			{
				if (font == null)
					return;

				string fontName = font.name;
				if (string.IsNullOrEmpty(fontName))
					return;

				lock (lockObject)
				{
					if (loggedFonts.Contains(fontName))
						return;

					loggedFonts.Add(fontName);
					string path = GetLogFilePath();
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(path));
						File.AppendAllText(path, $"{fontName}\n");
						Logging.Debug($"Logged font: {fontName}");
					}
					catch (Exception e)
					{
						Logging.Error($"Failed to log font {fontName}: {e.Message}");
					}
				}
			}
		}
	}
}
