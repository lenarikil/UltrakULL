using System;
using HarmonyLib;
using TMPro;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using static UltrakULL.CommonFunctions;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
    //@Override
    //Overrides ScanBook from the ScanningStuff class, for the "scanning" panel and book translations.
    [HarmonyPatch(typeof(ScanningStuff), "ScanBook")]
    public static class LocalizeScanningText
    {
        [HarmonyPrefix]
        public static bool ScanBook_MyPatch(ref string text, bool noScan, int instanceId, ScanningStuff __instance)
        {
            if(isUsingEnglish())
            {
                return true;
            }

            string originalText = text;
            string sceneName = GetCurrentSceneName();
            string bookId = IdentifyBookId(originalText, sceneName);

            if (!string.IsNullOrEmpty(bookId))
            {
                bool bookAudioEnabled = Convert.ToBoolean(LanguageManager.configFile.Bind("General", "bookAudioDubbing", "False").Value);
                if (bookAudioEnabled)
                {
                    bool isReversed = IsReadingScannedTextReversed();
                    string audioBookId = isReversed ? bookId + "_reversed" : bookId;
                    Logging.Message("[BookAudio] Book opened: " + sceneName + "/" + audioBookId);
                    BookAudioPlayer.Play(sceneName, audioBookId);
                }
            }

            GameObject canvas = GetInactiveRootObject("Canvas");

            TextMeshProUGUI scanningText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(canvas, "ScanningStuff"), "ScanningPanel"), "Text"));
            scanningText.text = LanguageManager.CurrentLanguage.books.books_scanning;
            text = Books.GetBookText(text);
            return true;
        }

        public static bool IsReadingScannedTextReversed()
        {
            GameObject canvas = GetInactiveRootObject("Canvas");
            if (canvas == null) return false;

            GameObject readingScanned = GetGameObjectChild(
                GetGameObjectChild(canvas, "ScanningStuff"),
                "ReadingScanned");
            GameObject panel = GetGameObjectChild(readingScanned, "Panel");
            GameObject scrollRect = GetGameObjectChild(panel, "Scroll Rect");
            GameObject viewport = GetGameObjectChild(scrollRect, "Viewport");
            GameObject textObj = GetGameObjectChild(viewport, "Text");
            if (textObj == null) return false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            return textRect != null && textRect.localScale.x < 0f;
        }

        private static string IdentifyBookId(string originalText, string sceneName)
        {
            switch (sceneName)
            {
                case "CreditsMuseum2":
                    return IdentifyMuseumBookId(originalText);

                case "Level 1-4":
                    return "limboFourth";

                case "Level 2-2":
                    return "lustSecond";

                case "Level 4-2":
                    return "greedSecond";

                case "Level 4-3":
                    return "greedThird";

                case "Level 5-2":
                    return "wrathSecond";

                case "Level 5-S":
                    if (originalText.Contains("DAY 529"))
                        return "fishingDiary";
                    return "fishingBottle";

                case "Level 6-1":
                    return "heresyFirst";

                case "Level 7-1":
                    if (originalText.Contains("The unending halls of"))
                        return "violenceFirst";
                    return "violenceFirstSlate";

                case "Level 7-2":
                    if (originalText.Contains("> < < > < < > > < > > < >"))
                        return "violenceSecond";
                    return "violenceSecondAmbush";

                case "Level 7-4":
                    return "violenceFourth";

                case "Level 7-S":
                    return "violenceSecret";

                case "Level 8-2":
                    if (originalText.Contains("Layer 8: Fraud has become"))
                        return "fraudSecond_1";
                    if (originalText.Contains("EARTHMOVER MENTIONS DETECTED"))
                        return "fraudSecond_2";
                    return "fraudSecond_3";

                case "Level 8-3":
                    if (originalText.Contains("ANOMALY DETECTED"))
                        return "fraudThird_1";
                    return "fraudThird_2";

                default:
                    return null;
            }
        }

        private static string IdentifyMuseumBookId(string originalText)
        {
            if (originalText.Contains("HAKITA"))
                return "hakita";
            if (originalText.Contains("FRANCIS XIE"))
                return "francisXie";
            if (originalText.Contains("JERICHO_RUS"))
                return "jerichoRus";
            if (originalText.Contains("BIGROCKBMP"))
                return "bigRockBMP";
            if (originalText.Contains("MAXIMILIAN OVESSON"))
                return "maximilianOvesson";
            if (originalText.Contains("RHIANNON MITCHELL"))
                return "rhiannonMitchell";
            if (originalText.Contains("VICTORIA HOLLAND"))
                return "victoriaHolland";
            if (originalText.Contains("TONI STIGELL"))
                return "toniStigell";
            if (originalText.Contains("FLYINGDOG"))
                return "flyingDog";
            if (originalText.Contains("SAMUEL JAMES BRYAN"))
                return "samuelJamesBryan";
            if (originalText.Contains("CAMERON MARTIN"))
                return "qaTeam";
            if (originalText.Contains("PITR"))
                return "pitr";
            if (originalText.Contains("HECKTECK"))
                return "heckteck";
            if (originalText.Contains("HAZELUFF"))
                return "hazeluff";
            if (originalText.Contains("CHIZHOV"))
                return "chizhov";
            if (originalText.Contains("LUCAS VARNEY"))
                return "lucasVarney";
            if (originalText.Contains("BEN MOIR"))
                return "benMoir";
            if (originalText.Contains("MEGANEKO"))
                return "meganeko";
            if (originalText.Contains("KEYGEN CHURCH"))
                return "keygenChurch";
            if (originalText.Contains("HEALTH"))
                return "health";
            if (originalText.Contains("KING GIZZARD"))
                return "kingGizzard";
            if (originalText.Contains("QUETZAL TIRADO"))
                return "quetzalTirado";
            if (originalText.Contains("SALAD"))
                return "salad";
            if (originalText.Contains("JACOB H.H.R."))
                return "jacobHHR";
            if (originalText.Contains("VVIZARD"))
                return "vvizard";
            if (originalText.Contains("ADDITIONAL MUSIC CREDITS"))
                return "additionalMusic";
            if (originalText.Contains("COMMUNITY CYBER GRIND"))
                return "communityCyberGrind";
            if (originalText.Contains("STEPHAN WEYTE"))
                return "stephanWeyte";
            if (originalText.Contains("LENVAL BROWN"))
                return "lenvalBrown";
            return "museum";
        }
    }
}
