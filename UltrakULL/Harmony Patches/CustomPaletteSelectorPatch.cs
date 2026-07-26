using HarmonyLib;
using System.Collections.Generic;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(CustomPaletteSelector), "BuildMenu")]
    public static class CustomPaletteSelectorPatch
    {
        private static readonly Dictionary<string, System.Func<Option, string>> BuiltInPalettes =
            new Dictionary<string, System.Func<Option, string>>
            {
                { "Gamebot Color", options => options.graphics_paletteGamebotColor },
                { "Noir", options => options.graphics_paletteNoir },
                { "Pink and Purple", options => options.graphics_palettePinkAndPurple },
                { "Rustic", options => options.graphics_paletteRustic },
                { "Shake", options => options.graphics_paletteShake },
                { "Sin Shitty", options => options.graphics_paletteSinShitty }
            };

        [HarmonyPostfix]
        private static void Postfix(Transform ___container)
        {
            if (LanguageManager.CurrentLanguage == null || ___container == null)
            {
                return;
            }

            for (int i = 1; i < ___container.childCount; i++)
            {
                Text label = ___container.GetChild(i).GetComponentInChildren<Text>();
                if (label == null || !BuiltInPalettes.TryGetValue(label.text, out System.Func<Option, string> getTranslation))
                {
                    continue;
                }

                string translation = getTranslation(LanguageManager.CurrentLanguage.options);
                if (!string.IsNullOrWhiteSpace(translation))
                {
                    label.text = translation;
                }
            }
        }
    }
}
