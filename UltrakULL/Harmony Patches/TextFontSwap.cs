using System;
using System.Collections.Generic;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
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
	}
}
