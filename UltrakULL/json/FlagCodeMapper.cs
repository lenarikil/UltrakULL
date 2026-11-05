using System;
using System.Collections.Generic;

namespace UltrakULL.json
{
    /// <summary>
    /// Pure mapping from a language tag (ru-RU, en-GB, GB-ENG, zh, es-419...) to
    /// an ISO 3166-1 flag code (ru, gb, gb-eng...). Extracted from FlagProvider
    /// so it can be compiled into the test project; FlagProvider adds the Unity
    /// sprite layer on top.
    ///
    /// Rules: a region subtag wins when its flag exists (ru-RU -> ru, en-GB ->
    /// gb, GB-ENG -> gb-eng; es-419 has no flag); a 3-letter subtag is treated
    /// as a subdivision code; otherwise the small region-less table applies
    /// (uk -> ua, ja -> jp...); everything else maps to null.
    /// </summary>
    public static class FlagCodeMapper
    {
        /// <summary>
        /// Flag codes available in the embedded set: the 249 officially assigned
        /// ISO 3166-1 alpha-2 codes plus Kosovo (xk) and the UK constituent
        /// flags (gb-eng/gb-nir/gb-sct/gb-wls). Mirrors tools/fetch-flags.ps1.
        /// </summary>
        public static readonly HashSet<string> EmbeddedFlagCodes = BuildEmbeddedFlagCodes();

        /// <summary>Minimal mapping for region-less language codes: language -> flag.</summary>
        public static readonly Dictionary<string, string> RegionLessMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", "gb" },
            { "ru", "ru" },
            { "uk", "ua" },
            { "ja", "jp" },
            { "zh", "cn" },
            { "ko", "kr" },
            { "pt", "pt" },
            { "de", "de" },
            { "fr", "fr" },
            { "es", "es" },
            { "it", "it" },
            { "pl", "pl" },
            { "tr", "tr" },
            { "ar", "sa" },
            { "hi", "in" },
            { "nl", "nl" },
            { "sv", "se" },
            { "cs", "cz" },
            { "el", "gr" },
            { "he", "il" },
            { "fa", "ir" },
            { "vi", "vn" },
            { "th", "th" },
            { "id", "id" },
            { "hu", "hu" },
            { "ro", "ro" }
        };

        /// <summary>
        /// Maps a language tag to an available flag code, or null when no flag
        /// exists for it (the UI then shows a language-code badge instead).
        /// </summary>
        public static string MapLanguageToFlagCode(string languageTag, ISet<string> availableFlagCodes)
        {
            string tag = languageTag != null ? languageTag.Trim() : null;
            if (string.IsNullOrEmpty(tag) || availableFlagCodes == null)
            {
                return null;
            }

            string[] parts = tag.Split('-');
            string primary = parts[0].ToLowerInvariant();

            if (parts.Length == 1)
            {
                // Region-less code: only the explicit table applies (e.g. uk -> ua).
                string mapped;
                return RegionLessMap.TryGetValue(primary, out mapped) && availableFlagCodes.Contains(mapped)
                    ? mapped
                    : null;
            }

            // A 3-letter second subtag is a subdivision (GB-ENG -> gb-eng).
            if (parts.Length == 2 && parts[1].Length == 3)
            {
                string combined = primary + "-" + parts[1].ToLowerInvariant();
                return availableFlagCodes.Contains(combined) ? combined : null;
            }

            // Region subtag: ru-RU -> ru, en-GB -> gb, zh-TW -> tw...
            string region = parts[parts.Length - 1].ToLowerInvariant();
            if (region.Length == 2 && availableFlagCodes.Contains(region))
            {
                return region;
            }

            // No flag for the region (es-419): fall back to the region-less table
            // for the primary language before giving up.
            string regionless;
            return RegionLessMap.TryGetValue(primary, out regionless) && availableFlagCodes.Contains(regionless)
                ? regionless
                : null;
        }

        private static HashSet<string> BuildEmbeddedFlagCodes()
        {
            string[] codes =
            {
                "ad","ae","af","ag","ai","al","am","ao","ar","as","at","au","aw","ax","az",
                "ba","bb","bd","be","bf","bg","bh","bi","bj","bl","bm","bn","bo","bq","br","bs","bt","bv","bw","by","bz",
                "ca","cc","cd","cf","cg","ch","ci","ck","cl","cm","cn","co","cr","cu","cv","cw","cx","cy","cz",
                "de","dj","dk","dm","do","dz",
                "ec","ee","eg","eh","er","es","et",
                "fi","fj","fk","fm","fo","fr",
                "ga","gb","gd","ge","gf","gg","gh","gi","gl","gm","gn","gp","gq","gr","gs","gt","gu","gw","gy",
                "hk","hm","hn","hr","ht","hu",
                "id","ie","il","im","in","io","iq","ir","is","it",
                "je","jm","jo","jp",
                "ke","kg","kh","ki","km","kn","kp","kr","kw","ky","kz",
                "la","lb","lc","li","lk","lr","ls","lt","lu","lv","ly",
                "ma","mc","md","me","mf","mg","mh","mk","ml","mm","mn","mo","mp","mq","mr","ms","mt","mu","mv","mw","mx","my","mz",
                "na","nc","ne","nf","ng","ni","nl","no","np","nr","nu","nz",
                "om",
                "pa","pe","pf","pg","ph","pk","pl","pm","pn","pr","ps","pt","pw","py",
                "qa",
                "re","ro","rs","ru","rw",
                "sa","sb","sc","sd","se","sg","sh","si","sj","sk","sl","sm","sn","so","sr","ss","st","sv","sx","sy","sz",
                "tc","td","tf","tg","th","tj","tk","tl","tm","tn","to","tr","tt","tv","tw","tz",
                "ua","ug","um","us","uy","uz",
                "va","vc","ve","vg","vi","vn","vu",
                "wf","ws",
                "ye","yt",
                "za","zm","zw",
                // Kosovo (user-assigned) and UK constituent flags.
                "xk","gb-eng","gb-nir","gb-sct","gb-wls"
            };

            return new HashSet<string>(codes, StringComparer.OrdinalIgnoreCase);
        }
    }
}
