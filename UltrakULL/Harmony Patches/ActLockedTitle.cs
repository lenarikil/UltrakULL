using HarmonyLib;
using TMPro;
using UltrakULL.json;
using UnityEngine;

[HarmonyPatch(typeof(MenuActSelect), "OnEnable")]
public static class ActLockedTitle
{
    [HarmonyPostfix]
    public static void Postfix(MenuActSelect __instance)
    {
        // Built-in English keeps the game's original locked-act titles.
        if (UltrakULL.CommonFunctions.isUsingEnglish()) return;

        string path = GetPath(__instance.transform);

        switch (path)
        {
            case "Canvas/Chapter Select/Chapters/Act I":
                __instance.nameWhenDisabled = LanguageManager.CurrentLanguage.frontend.chapter_act1_lock;
                break;

            case "Canvas/Chapter Select/Chapters/Act II":
                __instance.nameWhenDisabled = LanguageManager.CurrentLanguage.frontend.chapter_act2_lock;
                break;

            case "Canvas/Chapter Select/Chapters/Act III":
                __instance.nameWhenDisabled = LanguageManager.CurrentLanguage.frontend.chapter_act3_lock;
                break;
        }

        TMP_Text text = __instance.transform.GetChild(0).GetComponent<TMP_Text>();

        if (!__instance.GetComponent<UnityEngine.UI.Button>().interactable &&
            !string.IsNullOrEmpty(__instance.nameWhenDisabled))
        {
            text.text = __instance.nameWhenDisabled;
        }
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}