using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ULTRAKILL;
using System;
using UltrakULL.json;
using UltrakULL.audio;
using static UltrakULL.CommonFunctions;
using UltrakULL;
using SettingsMenu.Components;
using UnityEngine.SceneManagement;

namespace UltrakULL.Harmony_Patches
{
    public static class MainMenuPatches
    {
        [HarmonyPatch(typeof(SettingsMenu.Components.SettingsPageBuilder))]
        public static class AudioPatch
        {
            [HarmonyPatch("BuildPage"), HarmonyPostfix]
            public static void OptionsSetSelectedPostfix(SettingsPageBuilder __instance)
            {
                if (__instance.name != "Audio" || __instance.transform.parent == null) return;

                Logging.Info("[AudioDubSlider] Processing Audio settings page");

                if (__instance.transform.GetChild(0).transform.name == "Container")
                {
                    try
                    {
                        GameObject AudioPage = __instance.gameObject;
                        AudioPage.SetActive(true);

                        GameObject AudioPageContainer = GetGameObjectChild(AudioPage, "Container");
                        GameObject originalSlider = GetGameObjectChild(AudioPageContainer, "Subtitles");

                        // ---- Dialogue Dub ----
                        GameObject dubSlider = GameObject.Instantiate(originalSlider, AudioPageContainer.transform);
                        dubSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(300f, -225f);
                        dubSlider.name = "Dialogue Dub";

                        GameObject oldToggleObj = GetGameObjectChild(dubSlider, "Toggle(Clone)");
                        Toggle oldToggle = oldToggleObj.GetComponent<Toggle>();

                        GameObject newToggleObj = GameObject.Instantiate(oldToggleObj, oldToggleObj.transform.parent);
                        newToggleObj.name = "DubToggle";
                        newToggleObj.SetActive(true);

                        Toggle dubToggle = newToggleObj.GetComponent<Toggle>();

                        GameObject oldCheckmark = GetGameObjectChild(GetGameObjectChild(oldToggleObj, "Background"), "Checkmark");
                        GameObject oldFakeCheck = GetGameObjectChild(GetGameObjectChild(oldToggleObj, "Background"), "FakeCheck");
                        oldCheckmark.SetActive(false);
                        oldFakeCheck.SetActive(false);

                        GameObject background = GetGameObjectChild(newToggleObj, "Background");
                        GameObject checkmark = GetGameObjectChild(background, "Checkmark");

                        dubToggle.targetGraphic = background.GetComponent<Image>();
                        dubToggle.graphic = checkmark.GetComponent<Image>();

                        bool isDubbingActive = Convert.ToBoolean(LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value);
                        dubToggle.isOn = isDubbingActive;

                        dubToggle.transition = Selectable.Transition.ColorTint;

                        GameObject.Destroy(GetGameObjectChild(dubSlider, "Reset Button Variant(Clone)"));

                        try
                        {
                            GetTextMeshProUGUI(GetGameObjectChild(dubSlider, "Text")).text = LanguageManager.CurrentLanguage.options.audio_dubbing;
                        }
                        catch (Exception e)
                        {
                            Logging.Error("[AudioDubSlider] Failed to set localized text, using fallback. Error: " + e);
                            GetTextMeshProUGUI(GetGameObjectChild(dubSlider, "Text")).text = "DUBBED AUDIO";
                        }
                        Logging.Info("[AudioDubSlider] Successful creation of the AudioDubSlider");

                        // ---- Extra Option ----
                        GameObject extraSlider = GameObject.Instantiate(originalSlider, AudioPageContainer.transform);
                        extraSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(650f, -225f);
                        extraSlider.name = "ExtraOption";

                        GameObject extraOldToggleObj = GetGameObjectChild(extraSlider, "Toggle(Clone)");
                        Toggle extraOldToggle = extraOldToggleObj.GetComponent<Toggle>();

                        GameObject extraNewToggleObj = GameObject.Instantiate(extraOldToggleObj, extraOldToggleObj.transform.parent);
                        extraNewToggleObj.name = "ExtraToggle";
                        extraNewToggleObj.SetActive(true);

                        Toggle extraToggle = extraNewToggleObj.GetComponent<Toggle>();

                        GameObject extraOldCheckmark = GetGameObjectChild(GetGameObjectChild(extraOldToggleObj, "Background"), "Checkmark");
                        GameObject extraOldFakeCheck = GetGameObjectChild(GetGameObjectChild(extraOldToggleObj, "Background"), "FakeCheck");
                        extraOldCheckmark.SetActive(false);
                        extraOldFakeCheck.SetActive(false);

                        GameObject extraBackground = GetGameObjectChild(extraNewToggleObj, "Background");
                        GameObject extraCheckmark = GetGameObjectChild(extraBackground, "Checkmark");

                        extraToggle.targetGraphic = extraBackground.GetComponent<Image>();
                        extraToggle.graphic = extraCheckmark.GetComponent<Image>();

                        bool isExtraActive = Convert.ToBoolean(LanguageManager.configFile.Bind("General", "extraDubbing", "False").Value);
                        extraToggle.isOn = isExtraActive;

                        extraToggle.transition = Selectable.Transition.ColorTint;

                        GameObject.Destroy(GetGameObjectChild(extraSlider, "Reset Button Variant(Clone)"));

                        extraToggle.onValueChanged.AddListener(delegate
                        {
                            bool newValue = extraToggle.isOn;
                            LanguageManager.configFile.Bind("General", "extraDubbing", "False").Value = newValue.ToString();
                            Logging.Info("[AudioDubSlider] EXTRA DUBBING toggled: " + newValue);
                        });

                        string extraLabel = LanguageManager.CurrentLanguage?.options?.audio_dubbing_extra;
                        if (!String.IsNullOrWhiteSpace(extraLabel))
                        { GetTextMeshProUGUI(GetGameObjectChild(extraSlider, "Text")).text = extraLabel; }
                        else { GetTextMeshProUGUI(GetGameObjectChild(extraSlider, "Text")).text = "EXTRA AUDIO DUBBING"; }

                        Action<bool> updateExtraInteractable = (bool active) =>
                        {
                            extraToggle.interactable = active;
                            CanvasGroup cg = extraSlider.GetComponent<CanvasGroup>();
                            if (cg == null) cg = extraSlider.AddComponent<CanvasGroup>();
                            cg.alpha = active ? 1f : 0.5f;
                        };

                        updateExtraInteractable(dubToggle.isOn);

                        Logging.Info("[AudioDubSlider] Successful creation of the ExtraOption slider");

                        // ---- Book Audio Dubbing ----
                        GameObject bookSlider = GameObject.Instantiate(originalSlider, AudioPageContainer.transform);
                        bookSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(650f, -280f);
                        bookSlider.name = "BookAudioDubbing";

                        GameObject bookOldToggleObj = GetGameObjectChild(bookSlider, "Toggle(Clone)");
                        Toggle bookOldToggle = bookOldToggleObj.GetComponent<Toggle>();

                        GameObject bookNewToggleObj = GameObject.Instantiate(bookOldToggleObj, bookOldToggleObj.transform.parent);
                        bookNewToggleObj.name = "BookAudioToggle";
                        bookNewToggleObj.SetActive(true);

                        Toggle bookToggle = bookNewToggleObj.GetComponent<Toggle>();

                        GameObject bookOldCheckmark = GetGameObjectChild(GetGameObjectChild(bookOldToggleObj, "Background"), "Checkmark");
                        GameObject bookOldFakeCheck = GetGameObjectChild(GetGameObjectChild(bookOldToggleObj, "Background"), "FakeCheck");
                        bookOldCheckmark.SetActive(false);
                        bookOldFakeCheck.SetActive(false);

                        GameObject bookBackground = GetGameObjectChild(bookNewToggleObj, "Background");
                        GameObject bookCheckmark = GetGameObjectChild(bookBackground, "Checkmark");

                        bookToggle.targetGraphic = bookBackground.GetComponent<Image>();
                        bookToggle.graphic = bookCheckmark.GetComponent<Image>();

                        bool isBookAudioActive = Convert.ToBoolean(LanguageManager.configFile.Bind("General", "bookAudioDubbing", "False").Value);
                        bookToggle.isOn = isBookAudioActive;

                        bookToggle.transition = Selectable.Transition.ColorTint;

                        GameObject.Destroy(GetGameObjectChild(bookSlider, "Reset Button Variant(Clone)"));

                        bookToggle.onValueChanged.AddListener(delegate
                        {
                            bool newValue = bookToggle.isOn;
                            LanguageManager.configFile.Bind("General", "bookAudioDubbing", "False").Value = newValue.ToString();
                            Logging.Info("[AudioDubSlider] BOOK AUDIO DUBBING toggled: " + newValue);
                        });

                        string bookLabel = LanguageManager.CurrentLanguage?.options?.audio_bookAudioDubbing;
                        if (!String.IsNullOrWhiteSpace(bookLabel))
                        { GetTextMeshProUGUI(GetGameObjectChild(bookSlider, "Text")).text = bookLabel; }
                        else { GetTextMeshProUGUI(GetGameObjectChild(bookSlider, "Text")).text = "BOOK AUDIO DUBBING"; }

                        Action<bool> updateBookInteractable = (bool active) =>
                        {
                            bookToggle.interactable = active;
                            CanvasGroup cg = bookSlider.GetComponent<CanvasGroup>();
                            if (cg == null) cg = bookSlider.AddComponent<CanvasGroup>();
                            cg.alpha = active ? 1f : 0.5f;
                        };

                        updateBookInteractable(dubToggle.isOn);

                        Logging.Info("[AudioDubSlider] Successful creation of the BookAudioDubbing slider");

                        dubToggle.onValueChanged.AddListener(delegate (bool newValue)
                        {
                            LanguageManager.configFile.Bind("General", "activeDubbing", "False").Value = newValue.ToString();
                            Logging.Info("[AudioDubSlider] Dialogue Dub toggled: " + newValue);
                            updateExtraInteractable(newValue);
                            updateBookInteractable(newValue);

                            if (SceneManager.GetActiveScene().name != "Main Menu")
                            {
                                AudioPreloadManager.EnsureCurrentScenePreloaded(delegate
                                {
                                    UltrakULL.Harmony_Patches.AudioSwaps.GabrielAudioSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.GabrielSecondAudioSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.MinosPrimeAudioSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.SisyphusPrimeAudioSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.MandaloreAudioSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.PowerAudioSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.PowerIntroSwap.RebindExistingInstances();
                                    UltrakULL.Harmony_Patches.AudioSwaps.BigJohninatorAudioSwap.RebindExistingInstances();
                                });
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Logging.Error("[AudioDubSlider] Error in SetActive: " + e);
                    }
                }
            }
        }
    }
}
