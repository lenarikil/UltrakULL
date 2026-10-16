using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using UltrakULL.Harmony_Patches;
using UltrakULL.json;

[HarmonyPatch(typeof(IntroViolenceScreen))]
public static class IntroViolenceScreenPatch
{
    private static GameObject textObject1;
    private static GameObject textObject2;

    private static readonly FieldInfo FadeAmountField = AccessTools.Field(typeof(IntroViolenceScreen), "fadeAmount");
    private static readonly FieldInfo TargetAlphaField = AccessTools.Field(typeof(IntroViolenceScreen), "targetAlpha");
    private static readonly FieldInfo FadeField = AccessTools.Field(typeof(IntroViolenceScreen), "fade");
    private static readonly FieldInfo RedField = AccessTools.Field(typeof(IntroViolenceScreen), "red");

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void StartPatch(IntroViolenceScreen __instance)
    {
        Transform canvasTransform = __instance.transform.parent;
        if (canvasTransform == null) return;

        Transform image1 = canvasTransform.Find("Image");
        Transform image2 = canvasTransform.Find("ImageRed");

        if (image1 != null)
        {
            ReplaceImage(image1, "violence1.png"); 
            textObject1 = AddTextToImage(image1, LanguageManager.CurrentLanguage.misc.violenceScreenText1, 0);
        }

        if (image2 != null)
        {
            ReplaceImage(image2, "violence2.png");
            textObject2 = AddTextToImage(image2, LanguageManager.CurrentLanguage.misc.violenceScreenText2, 0);
        }
    }

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdatePatch(IntroViolenceScreen __instance)
    {
        if (textObject1 == null || textObject2 == null) return;
        if (FadeAmountField == null || TargetAlphaField == null || FadeField == null || RedField == null) return;

        float fadeAmount = (float)FadeAmountField.GetValue(__instance);
        float targetAlpha = (float)TargetAlphaField.GetValue(__instance);
        bool fade = (bool)FadeField.GetValue(__instance);
        Color redColor = ((Image)RedField.GetValue(__instance)).color;

               if (fade && targetAlpha == 1f)
        {
            UpdateTextAlpha(textObject1, fadeAmount);
        }

        if (redColor.a > 0)
        {
            UpdateTextAlpha(textObject1, 0); 
            UpdateTextAlpha(textObject2, fadeAmount);
        }
    }

    private static GameObject AddTextToImage(Transform imageTransform, string text, float startAlpha)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(imageTransform, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 10;
        tmp.fontSizeMax = 100;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.color = new Color(1, 1, 1, startAlpha);
        tmp.enableWordWrapping = false;
        TextMeshProFontSwap.SwapTMPFont(ref tmp);
        //tmp.autoSizeTextContainer = true;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        float maxWidth = Mathf.Min(Screen.width * 0.9f, 800);
        float maxHeight = Mathf.Min(Screen.height * 0.45f, 300);

        rectTransform.sizeDelta = new Vector2(maxWidth, maxHeight);

        return textObj;
    }


    private static void UpdateTextAlpha(GameObject textObj, float alpha)
    {
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
    }
    private static void ReplaceImage(Transform imageTransform, string ImageName)
    {
        Image img = imageTransform.GetComponent<Image>();
        if (img == null) return;

        string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\IntroViolenceScreen";
        string imagePath = Path.Combine(modPath, ImageName);

        Sprite newSprite = LoadPNG(imagePath);
        if (newSprite != null)
        {
            img.sprite = newSprite;
        }
    }

    private static Sprite LoadPNG(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }
}
