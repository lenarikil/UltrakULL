using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UltrakULL.json;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions.Must;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides the AddPoints method from the StyleHUD class. This is needed to intercept and translate any strings coming into the style meter in-game.

    [HarmonyPatch(typeof(StyleHUD), "GetLocalizedName")]
    public static class LocalizeStyleHud
    {
        internal const string StyleLinkPrefix = "uakstyle:";
        internal const string RicoshotUltraModifierId = "ultrakill.style.ricoshotUltra";
        internal const string RicoshotCounterModifierId = "ultrakill.style.ricoshotCounter";

        internal static string TagStyleName(string id, string text)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith("<link=\"" + StyleLinkPrefix, StringComparison.Ordinal))
                return text;

            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(id))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return "<link=\"" + StyleLinkPrefix + token + "\">" + text + "</link>";
        }

        [HarmonyPrefix]
        public static bool GetLocalizedName_MyPatch(string id, StyleHUD __instance, Dictionary<string, string> ___idNameDict, ref string __result)
        {
            if (isUsingEnglish())
                return true;

            if (___idNameDict.ContainsKey(id))
            {
                string dictResult = StyleBonusStrings.GetStyleBonusDictionary(id);
                if (dictResult == null)
                    return true;
                __result = dictResult;
                return false;
            }

            string translated = StyleBonusStrings.GetTranslatedStyleBonus(id);
            if (translated == null || translated == "")
                return true;

            __result = translated;
            return false;
        }

        [HarmonyPostfix]
        public static void GetLocalizedName_Postfix(string id, ref string __result)
        {
            __result = TagStyleName(id, __result);
        }

        internal static string StripTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return Regex.Replace(s, "<[^>]*>", "");
        }

    }

    /// <summary>
    /// Gives each style name a stable, invisible link tag so the shared style text and
    /// pending queue can be re-translated by style ID instead of ambiguous display text.
    /// </summary>
    [HarmonyPatch(typeof(StyleHUD), "AddPoints")]
    public static class LocalizeAddPoints
    {
        private const string RicoshotUltraEnglish = "ULTRA";
        private const string RicoshotCounterEnglish = "COUNTER";

        private sealed class StyleName
        {
            public string EnglishName;
        }

        private static readonly Dictionary<string, StyleName> knownStyleNames = new Dictionary<string, StyleName>();
        private static readonly Regex styleLinkRegex = new Regex(
            "<link=\"uakstyle:([A-Za-z0-9_-]+)\">(.*?)</link>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ricoshotModifierRegex = new Regex(
            "(?<open><color=[^>]+>)?(?<name>ULTRA|COUNTER)(?<close></color>)?",
            RegexOptions.IgnoreCase);

        [HarmonyPrefix]
        public static void Prefix(ref string prefix)
        {
            if (string.IsNullOrEmpty(prefix) || prefix.Contains("<link=\"" + LocalizeStyleHud.StyleLinkPrefix))
                return;

            prefix = ricoshotModifierRegex.Replace(prefix, delegate(Match match)
            {
                string modifier = match.Groups["name"].Value;
                string id = string.Equals(modifier, RicoshotUltraEnglish, StringComparison.OrdinalIgnoreCase)
                    ? LocalizeStyleHud.RicoshotUltraModifierId
                    : LocalizeStyleHud.RicoshotCounterModifierId;

                return match.Groups["open"].Value +
                       LocalizeStyleHud.TagStyleName(id, modifier) +
                       match.Groups["close"].Value;
            });
        }

        [HarmonyPostfix]
        public static void Postfix(StyleHUD __instance, int points, string pointID)
        {
            if (__instance == null || string.IsNullOrEmpty(pointID))
                return;

            var idNameDict = Traverse.Create(__instance).Field("idNameDict").GetValue<Dictionary<string, string>>();
            string name;
            string englishName = idNameDict != null && idNameDict.TryGetValue(pointID, out name) ? name : pointID;

            knownStyleNames[pointID] = new StyleName { EnglishName = englishName };
        }

        /// <summary>
        /// Rewrites style names in the shared style-info text and in the game's
        /// pending display queue. AddPoints enqueues strings which UpdateItems later
        /// prepends to one TMP component, so individual floating-text tracking cannot
        /// update style names that are already on screen.
        /// </summary>
        public static void Reapply()
        {
            if (knownStyleNames.Count == 0)
                return;

            StyleHUD[] styleHuds = UnityEngine.Object.FindObjectsOfType<StyleHUD>(true);
            for (int i = 0; i < styleHuds.Length; i++)
            {
                StyleHUD styleHud = styleHuds[i];
                if (styleHud == null)
                    continue;

                TMP_Text styleInfo = Traverse.Create(styleHud).Field("styleInfo").GetValue<TMP_Text>();
                if (styleInfo != null)
                    styleInfo.text = ReplaceTaggedStyleNames(styleInfo.text);

                Queue<string> pendingItems = Traverse.Create(styleHud).Field("hudItemsQueue").GetValue<Queue<string>>();
                if (pendingItems != null && pendingItems.Count > 0)
                {
                    string[] items = pendingItems.ToArray();
                    pendingItems.Clear();
                    for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
                        pendingItems.Enqueue(ReplaceTaggedStyleNames(items[itemIndex]));
                }
            }
        }

        private static string ReplaceTaggedStyleNames(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return styleLinkRegex.Replace(text, delegate(Match match)
            {
                string token = match.Groups[1].Value.Replace('-', '+').Replace('_', '/');
                int padding = (4 - token.Length % 4) % 4;
                token = token.PadRight(token.Length + padding, '=');

                string pointID;
                try
                {
                    pointID = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                }
                catch (FormatException e)
                {
                    Logging.Warn("[StyleHUD] Invalid style link identifier: " + e.Message);
                    return match.Value;
                }

                StyleName styleName;
                if (!knownStyleNames.TryGetValue(pointID, out styleName) &&
                    pointID != LocalizeStyleHud.RicoshotUltraModifierId &&
                    pointID != LocalizeStyleHud.RicoshotCounterModifierId)
                    return match.Value;

                string englishName = styleName != null
                    ? styleName.EnglishName
                    : pointID == LocalizeStyleHud.RicoshotUltraModifierId
                        ? RicoshotUltraEnglish
                        : RicoshotCounterEnglish;
                string localizedName = GetCurrentStyleName(pointID, englishName);
                return "<link=\"" + LocalizeStyleHud.StyleLinkPrefix + match.Groups[1].Value + "\">" +
                       (localizedName ?? match.Groups[2].Value) +
                       "</link>";
            });
        }

        private static string GetCurrentStyleName(string pointID, string englishName)
        {
            if (isUsingEnglish())
                return englishName;

            if (pointID == LocalizeStyleHud.RicoshotUltraModifierId)
            {
                string text = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.style != null
                    ? LanguageManager.CurrentLanguage.style.style_ricoshotUltra
                    : null;
                return string.IsNullOrEmpty(text) ? englishName : text;
            }

            if (pointID == LocalizeStyleHud.RicoshotCounterModifierId)
            {
                string text = LanguageManager.CurrentLanguage != null && LanguageManager.CurrentLanguage.style != null
                    ? LanguageManager.CurrentLanguage.style.style_ricoshotCounter
                    : null;
                return string.IsNullOrEmpty(text) ? englishName : text;
            }

            string translation = StyleBonusStrings.GetStyleBonusDictionary(pointID);
            if (!string.IsNullOrEmpty(translation))
                return translation;

            translation = StyleBonusStrings.GetTranslatedStyleBonus(englishName);
            return string.IsNullOrEmpty(translation) ? englishName : translation;
        }
    }

    //@Override
    //Overrides the UpdateFreshnessSlider to localize freshness strings
    [HarmonyPatch(typeof(StyleHUD), "UpdateFreshnessSlider")]
    public static class LocalizeWeaponFreshnessText
    {
        // UpdateFreshnessSlider runs every frame during combat; cache the localized
        // string per freshness state and only write the TMP text when it changes.
        private static readonly string[] freshnessTexts = new string[4];
        private static JsonFormat cachedFreshnessLanguage;

        [HarmonyPrefix]
        public static bool UpdateFreshnessSlider_MyPatch(StyleHUD __instance, GunControl ___gc)
        {
            StyleFreshnessState freshnessState = __instance.GetFreshnessState(___gc.currentWeapon);

            int stateIndex = (int)freshnessState;
            if (stateIndex < 0 || stateIndex >= freshnessTexts.Length)
            {
                __instance.freshnessSliderText.text = StyleBonusStrings.GetWeaponFreshness(freshnessState);
                return false;
            }

            if (!ReferenceEquals(cachedFreshnessLanguage, LanguageManager.CurrentLanguage))
            {
                cachedFreshnessLanguage = LanguageManager.CurrentLanguage;
                for (int i = 0; i < freshnessTexts.Length; i++)
                    freshnessTexts[i] = null;
            }

            string text = freshnessTexts[stateIndex] ?? (freshnessTexts[stateIndex] = StyleBonusStrings.GetWeaponFreshness(freshnessState));

            if (__instance.freshnessSliderText.text != text)
            {
                __instance.freshnessSliderText.text = text;
            }

            return false;
        }

        /// <summary>
        /// Re-applies the active language to the freshness multiplier text immediately.
        ///
        /// Called on every language switch so the visible multiplier changes right away
        /// instead of waiting for the next UpdateFreshnessSlider call (which may not
        /// happen on tutorial levels where no weapon is active).
        /// </summary>
        public static void Reapply()
        {
            // Invalidate the per-language cache so the next UpdateFreshnessSlider
            // recalculates from the new language.
            cachedFreshnessLanguage = null;
            for (int i = 0; i < freshnessTexts.Length; i++)
                freshnessTexts[i] = null;

            try
            {
                var hud = MonoSingleton<StyleHUD>.Instance;
                if (hud == null || hud.freshnessSliderText == null)
                    return;

                StyleFreshnessState state;
                try
                {
                    GunControl gc = Traverse.Create(hud).Field("gc").GetValue<GunControl>();
                    if (gc == null)
                        gc = UnityEngine.Object.FindObjectOfType(typeof(GunControl)) as GunControl;
                    if (gc == null || gc.currentWeapon == null)
                        state = (StyleFreshnessState)0; // Fresh — состояние по умолчанию для tutorial
                    else
                        state = hud.GetFreshnessState(gc.currentWeapon);
                }
                catch
                {
                    state = (StyleFreshnessState)0; // Fresh — состояние по умолчанию при ошибке
                }

                var text = StyleBonusStrings.GetWeaponFreshness(state);
                hud.freshnessSliderText.text = text;
            }
            catch
            {
                // StyleHUD may not be available between scenes; no-op is safe.
            }
        }
    }

    [HarmonyPatch(typeof(StyleHUD), "AscendRank")]
    public static class StyleHUD_AscendRankPatch
    {
        [HarmonyPrefix]
        public static void Prefix(StyleHUD __instance)
        {
            if (!LanguageManager.IsRightToLeft && LanguageManager.CurrentLanguage.metadata.languageCode != "Arabic") return;

            try
			{
				int c = Mathf.Clamp(__instance.rankIndex, 0, 7);
				int i = Mathf.Clamp(__instance.rankIndex + 1, 0, 7);
				if (c - i == 0)
				{
					return; // no change
				}
				__instance.ranks[i].sprite = Core.CustomRankImages[i];
            }
            catch (Exception e)
            {
                Logging.Message($"Exception thrown in StyleHUD_AscendRankPatch: {e.Message}");
            }
        }
    }

	[HarmonyPatch(typeof(StyleHUD), "DescendRank")]
	public static class StyleHUD_DescendRankPatch
	{
		[HarmonyPrefix]
		public static void Prefix(StyleHUD __instance)
		{
			if (!LanguageManager.IsRightToLeft && LanguageManager.CurrentLanguage.metadata.languageCode != "Arabic") return;

			try
			{
                int c = Mathf.Clamp(__instance.rankIndex, 0, 7);
                int i = Mathf.Clamp(__instance.rankIndex - 1, 0, 7);
                if (c - i == 0)
                {
                    return; // no change
                }
                __instance.ranks[i].sprite = Core.CustomRankImages[i];
            }
            catch (Exception e)
            {
                // For some fucking reason this is called an obscene amount of times.
                //Logging.Message($"Exception thrown in StyleHUD_DescendRankPatch: {e.Message}");
            }
		}
	}

    [HarmonyPatch(typeof(StyleHUD), "Start")]
    public static class StyleHUD_KoreanPatch
    {
        [HarmonyPostfix]
        public static void Postfix(StyleHUD __instance)
        {
            if (isUsingEnglish()) return;

            string lang = LanguageManager.CurrentLanguage.metadata.languageCode;

            float spacing;
            if (lang == "ko-KR" || lang == "vi-VI")
                spacing = -10f;
            else
                return;

            TMP_Text styleInfo = Traverse.Create(__instance)
                .Field("styleInfo")
                .GetValue<TMP_Text>();

            if (styleInfo != null)
            {
                styleInfo.lineSpacing = spacing;
                Logging.Message($"StyleHUD Patched ({lang})");
            }
        }
    }

    /*
	[HarmonyPatch(typeof(StyleHUD), "Awake")]
	public static class StyleHUD_AwakePatch
	{
		static bool patched = false;
        [HarmonyPrefix]
        public static void Prefix(StyleHUD __instance)
        {
			if (!LanguageManager.IsRightToLeft && LanguageManager.CurrentLanguage.metadata.languageCode != "Arabic") return;

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    __instance.ranks[i].sprite = Core.CustomRankImages[i];
                }
            }
            catch(Exception e)
			{
				Logging.Message($"Exception thrown in StyleHUD_AwakePatch: {e.Message}");
			}
        }
	}
    */
}
