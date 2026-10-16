using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UltrakULL.json;
using static UltrakULL.CommonFunctions;
using System.Linq;
using UnityEngine.EventSystems;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(OptionsMenuToManager), "Start")]
    public static class InjectLanguageButton
    {
        public static TextMeshProUGUI languageButtonText;
        public static TextMeshProUGUI languagePageTitleText;
        private static readonly HttpClient Client = new HttpClient();
        
        private static bool hasAlreadyFetchedLanguages = false;
        private static List<GameObject> languageButtons = new List<GameObject>();
        
        private static GameObject langBrowserPage;
        private static GameObject langLocalPage;
        private static GameObject redownloadConfirmPanel;
        private static GameObject referenceButtonTemplate;

        private static void EnsureReferenceButtonTemplate()
        {
            if (referenceButtonTemplate != null)
                return;

            var optionsParent = GetInactiveRootObject("Canvas").Child("OptionsMenu").transform;
            var navigationRail = optionsParent.Find("Navigation Rail").gameObject;
            var buttonPrefab = navigationRail.Child("Back");

            referenceButtonTemplate = GameObject.Instantiate(buttonPrefab);
            referenceButtonTemplate.name = "ReferenceButtonTemplate";
            referenceButtonTemplate.SetActive(false);
        }
        /// <summary>
        /// Refactored utility method for creating consistent TMP buttons based on a reference button.
        /// </summary>
        public static class ButtonUtils
        {
            private static readonly ColorBlock defaultColorBlock = new ColorBlock
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(0.5094f, 0.5094f, 0.5094f, 1f),
                pressedColor = new Color(1f, 0f, 0f, 1f),
                selectedColor = new Color(0.5094f, 0.5094f, 0.5094f, 1f),
                disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            public static GameObject CreateTMPButton(
                Transform parent,
                string name,
                string labelText,
                Action onClick,
                Color? buttonColor = null,
                Vector2? size = null,
                bool richText = true,
                bool changeSize = true,
                bool addHighlightSupport = false)
            {
                EnsureReferenceButtonTemplate();
                GameObject buttonObj = GameObject.Instantiate(referenceButtonTemplate, parent);
                if (buttonObj.GetComponent<HudOpenEffect>() == null)
                {
                    buttonObj.AddComponent<HudOpenEffect>();
                }
                buttonObj.name = name;

                // Reset position/rotation/scale
                RectTransform rect = buttonObj.GetComponent<RectTransform>();
                rect.localPosition = Vector3.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;

                if (buttonColor.HasValue)
                {
                    Image img = buttonObj.GetComponent<Image>();
                    if (img != null)
                        img.color = buttonColor.Value;
                }

                Button button = buttonObj.GetComponent<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                if (onClick != null)
                    button.onClick.AddListener(() => onClick());

                button.interactable = true;
                button.transition = Selectable.Transition.ColorTint;
                button.colors = defaultColorBlock;
                button.navigation = new Navigation { mode = Navigation.Mode.None };

                TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = labelText;
                    text.richText = richText;
                    text.alignment = TextAlignmentOptions.Center;
                    text.font = Core.GlobalFontTMP;

                    if (changeSize)
                    {
                        text.enableAutoSizing = true;
                        text.fontSizeMin = 10f;
                        text.fontSizeMax = 36f;
                    }
                }

                if (changeSize && size.HasValue)
                {
                    rect.sizeDelta = size.Value;
                }

                buttonObj.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                if (addHighlightSupport)
                {
                    var buttonImage = buttonObj.GetComponent<Image>();
                    var highlightParent = buttonObj.GetComponentInParent<ButtonHighlightParent>();
                    if (highlightParent != null && buttonImage != null)
                    {
                        // Add click reaction to this button
                        button.onClick.AddListener(() => highlightParent.ChangeButton(buttonImage));
                    }
                }

                return buttonObj;
            }
        }


        public static bool langFileLocallyExists(string languageTag)
        {
            string expectedFileLocation = Path.Combine(BepInEx.Paths.ConfigPath, "ultrakull", languageTag + ".json");
            return File.Exists(expectedFileLocation);
        }

        public static void updateLanguageButtonText()
        {
            languageButtonText.text = LanguageManager.CurrentLanguage.options.language_languages;
            languagePageTitleText.text = "--" + LanguageManager.CurrentLanguage.options.language_languages + "--";
        }
        
        public static void warnBeforeDownload(LanguageInfo lInfo)
        {
            GameObject difficultySelectMenu = GetInactiveRootObject("Canvas").Child("Difficulty Select (1)");

            GameObject panelToUse = GetInactiveRootObject("Canvas").FindPath("OptionsMenu", "Assist Options", "Panel");
            
            if(redownloadConfirmPanel == null)
            {
                redownloadConfirmPanel = GameObject.Instantiate(panelToUse,difficultySelectMenu.transform.parent);
            }

            redownloadConfirmPanel.name = "ConfirmDownloadPanel";
            
            Text confirmDownloadText = redownloadConfirmPanel.FindPath("Panel", "Text (2)").Text();
            
            confirmDownloadText.fontSize = 22;
            confirmDownloadText.text =
            "This language has already been downloaded. <color=#34e1eb>Redownload?</color>\n\n" 
                +"<color=orange>The current file's contents will be overwritten.</color>";

            Text confirmDownloadTextConfirm = redownloadConfirmPanel.FindPath("Panel", "Text (1)").Text();
            confirmDownloadTextConfirm.text = "";
            
            //Destroy the original buttons and replace them with new ones (at least until I can figure out how to change the listeners of the original buttons)
            GameObject origYes = redownloadConfirmPanel.FindPath("Panel", "Yes");
            GameObject origNo = redownloadConfirmPanel.FindPath("Panel", "No");
            origYes.SetActive(false);
            origNo.SetActive(false);
            
            //Make new buttons here
            GameObject DownloadYes = CreateButton("YES","DownloadYes");
            DownloadYes.name = "DownloadYes";
            DownloadYes.transform.position = new Vector3(1150, 300, 0);
            DownloadYes.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(300f, 50f);
            DownloadYes.GetComponentInChildren<Text>().text = "YES";
            DownloadYes.GetComponentInChildren<Text>().font = Core.GlobalFont;
            DownloadYes.transform.SetParent(redownloadConfirmPanel.transform);
            
            GameObject DownloadNo = CreateButton("NO","DownloadNo");
            DownloadNo.name = "DownloadNo";
            DownloadNo.transform.position = new Vector3(750, 300, 0);
            DownloadNo.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(300f, 50f);
            DownloadNo.GetComponentInChildren<Text>().text = "NO";
            DownloadNo.GetComponentInChildren<Text>().font = Core.GlobalFont;
            DownloadNo.transform.SetParent(redownloadConfirmPanel.transform);
            
            DownloadYes.GetComponentInChildren<Button>().onClick.AddListener(delegate { redownloadConfirmPanel.SetActive(false); downloadLanguageFile(lInfo.languageTag,lInfo.languageFullName); });
            DownloadNo.GetComponentInChildren<Button>().onClick.AddListener(delegate { redownloadConfirmPanel.SetActive(false); });
            
            redownloadConfirmPanel.SetActive(true);
        }

        public async static Task getOnlineLanguages(Transform parent, GameObject languagePage)
        {
            string masterLanguageUrl = "https://clearwateruk.github.io/mods/ultrakULL/languagesMaster.json";
            Transform navigationRail = GameObject.Find("Navigation Rail").transform;
            Transform optionsParent = GetInactiveRootObject("Canvas").Child("OptionsMenu").transform;
            Transform pagesParent = GameObject.Find("Pages").transform;
            Transform referencePage = pagesParent.transform.Find("General");
            Scrollbar referenceScrollbar = referencePage.GetComponentsInChildren<Scrollbar>().FirstOrDefault();


            if (langBrowserPage != null)
            {
                UnityEngine.Object.Destroy(langBrowserPage);
                langBrowserPage = null;
            }
            if (langBrowserPage == null)
            {
                langBrowserPage = new GameObject("LanguageBrowserPage", typeof(RectTransform), typeof(CanvasRenderer));
                langBrowserPage.transform.SetParent(parent, false);
                RectTransform pageRect = langBrowserPage.GetComponent<RectTransform>();
                pageRect.sizeDelta = new Vector2(600, 800);        

                // Title
                GameObject titleObject = GameObject.Instantiate(navigationRail.Find("Text (7)").gameObject, langBrowserPage.transform);
                //This causes NRE in the fontswap. idk why but upper one works without it so
                //GameObject titleObject = new GameObject("Title", typeof(TextMeshProUGUI));
                titleObject.transform.SetParent(langBrowserPage.transform, false);
                TextMeshProUGUI langBrowserTitle = titleObject.GetComponent<TextMeshProUGUI>();
                langBrowserTitle.text = "--LANGUAGE BROWSER--";
                langBrowserTitle.alignment = TextAlignmentOptions.Center;
                langBrowserTitle.fontSize = 24;
                langBrowserTitle.font = Core.GlobalFontTMP;
                TextMeshProFontSwap.SwapTMPFont(ref langBrowserTitle);

                RectTransform titleRect = langBrowserTitle.rectTransform;
                titleRect.anchorMin = new Vector2(0.5f, 1);
                titleRect.anchorMax = new Vector2(0.5f, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.anchoredPosition = new Vector2(0, -50);
                titleRect.sizeDelta = new Vector2(400, 50);

                // ScrollView
                GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
                scrollView.transform.SetParent(langBrowserPage.transform, false);
                RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
                scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
                scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                scrollRect.anchoredPosition = new Vector2(0, 0);
                scrollRect.sizeDelta = new Vector2(550, 600);
                scrollView.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
                scrollView.GetComponent<Mask>().showMaskGraphic = false;

                // ScrollRect settings to limit side-to-side scrolling
                ScrollRect scrollRectComponent = scrollView.GetComponent<ScrollRect>();
                scrollRectComponent.horizontal = false;
                scrollRectComponent.vertical = true;
                scrollRectComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

                // Adding a scrollbar
                GameObject scrollbar = GameObject.Instantiate(referenceScrollbar.gameObject, langBrowserPage.transform);
                scrollbar.transform.SetParent(langBrowserPage.transform, false);
                RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();

                Scrollbar scrollbarComponent = scrollbar.GetComponent<Scrollbar>();
                scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;
                scrollRectComponent.verticalScrollbar = scrollbarComponent;

                // Content Container
                GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                content.transform.SetParent(scrollView.transform, false);
                RectTransform contentRect = content.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.anchoredPosition = Vector2.zero;
                contentRect.sizeDelta = new Vector2(0, 0);

                VerticalLayoutGroup vGroup = content.GetComponent<VerticalLayoutGroup>();
                vGroup.spacing = 10;
                vGroup.childAlignment = TextAnchor.UpperCenter;
                vGroup.childForceExpandWidth = true;
                vGroup.childForceExpandHeight = false;
                vGroup.childControlWidth = true;
                vGroup.childControlHeight = true;

                ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                scrollView.GetComponent<ScrollRect>().content = contentRect;

                try
                {
                    Logging.Warn("Obtaining online languages from UltrakULL repo...");
                    string responseJsonRaw = await Client.GetStringAsync(masterLanguageUrl);
                    MasterLanguages responseJson = JsonConvert.DeserializeObject<MasterLanguages>(responseJsonRaw);

                    foreach (LanguageInfo langInfo in responseJson.availableLanguages)
                    {
                        GameObject langButtonObj = ButtonUtils.CreateTMPButton(content.transform, langInfo.languageTag + " " + langInfo.languageFullName, langInfo.languageFullName, () => downloadLanguageFile(langInfo.languageTag, langInfo.languageFullName));
                    }
                }
                catch (Exception e)
                {
                    Logging.Error("Error loading languages: " + e);
                }

                // The "Return" button
                GameObject backButtonObj = ButtonUtils.CreateTMPButton(content.transform, "Return from LanguageBrowserPage", "Return", () =>
                {
                    foreach (Transform page in pagesParent)
                    {
                        page.gameObject.SetActive(false);
                    }
                    langBrowserPage.SetActive(false);
                    languagePage.SetActive(true);
                }, Color.red);


                Logging.Message("Setting up navigation buttons to hide language browser page...");
                foreach (Transform child in navigationRail)
                {
                    if (child.name != "Saves")
                    {
                        Button navButton = child.GetComponent<Button>();
                        if (navButton != null)
                        {
                            navButton.onClick.AddListener(() =>
                            {
                                if (langBrowserPage.activeSelf)
                                {
                                    Logging.Message("Hiding Language Browser Page as another button was clicked: " + child.name);
                                    langBrowserPage.SetActive(false);
                                }
                            });
                        }
                    }
                }
            }
        }

        public static void downloadLanguageFile(string languageTag, string languageName)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>DOWNLOADING...</color>");
            
            string fileName = languageTag + ".json";

            string languageFileUrl = "https://clearwateruk.github.io/mods/ultrakULL/" + fileName;
            
            string localLanguageFolder = Path.Combine(BepInEx.Paths.ConfigPath, "ultrakull//");
            
            string fullPath = localLanguageFolder + fileName;
            
            Logging.Warn("Downloading to: " + fullPath);
            
            Client.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
            Client.Timeout = TimeSpan.FromSeconds(5);
            
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string messageNotif;
                    bool newLangDownloaded = false;
              
                    //If the file was simply updated, it can be used straightaway.
                    //If a new lang file was downloaded, display a notif to the user to enter a level or reload the menu.
                    if(langFileLocallyExists(languageTag))
                    {
                        messageNotif = "Language file \"" + languageName + "\" has been updated.";
                    }
                    else
                    {
                        messageNotif = "A new language file \"" + languageName + "\" has been downloaded.";
                        newLangDownloaded = true;
                    }

                    webClient.DownloadFile(languageFileUrl, fullPath);
                    string jsonFile = File.ReadAllText(fullPath);
                    JsonFormat file = JsonConvert.DeserializeObject<JsonFormat>(jsonFile);
                    
                    Logging.Info("Lang file saved.");
                       
                    
                    MonoSingleton<HudMessageReceiver>.Instance.ClearMessage();
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=green>" + messageNotif + "</color>");

                    if(newLangDownloaded)
                    {

                        LanguageManager.allLanguages.Add(languageTag, file);
                        
                        
                        Transform optionsParent = GetInactiveRootObject("Canvas").Child("OptionsMenu").transform;
                        GameObject slotRowPrefab = optionsParent.Find("Save Slots").Find("Grid").Find("Slot Row").gameObject;
                        addLocalLanguageToLocalList(ref slotRowPrefab, file.metadata.langName,true);
                    }
                }
            }
            catch(Exception e)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=red>A download error occured, file has not been saved.</color>");
                Logging.Error("Attempted to download from: " + languageFileUrl);
                Logging.Error(e.ToString());
            }

        }
        
        public static void addLocalLanguageToLocalList(ref GameObject slotRowPrefab, string language, bool newlyAdded=false)
        {
            Transform contentParent = langLocalPage.transform.Find("Scroll Rect").Find("Contents");
            GameObject deleteButtonPrefab = slotRowPrefab.transform.Find("Delete Wrapper").gameObject.Child("Delete Button");
            GameObject languageButtonInstance = ButtonUtils.CreateTMPButton(contentParent, language, LanguageManager.allLanguages[language].metadata.langDisplayName, delegate
            {
                contentParent.gameObject.FindPath(LanguageManager.CurrentLanguage.metadata.langName, "Text").TMP().text = LanguageManager.CurrentLanguage.metadata.langDisplayName;
                contentParent.gameObject.FindPath(language, "Text").TMP().text += "\n<size=22>(<color=green>Selected</color>)</size>";
                LanguageManager.SetCurrentLanguage(language);
            });
            //languageButtonInstance.transform.Find("Select Wrapper").gameObject.SetActive(false);
            //languageButtonInstance.transform.Find("Delete Wrapper").gameObject.SetActive(false);
            //languageButtonInstance.transform.Find("State Text").gameObject.SetActive(false);
            if(newlyAdded)
            {
                languageButtonInstance.transform.SetSiblingIndex(2);
            }
            GameObject.Destroy(languageButtonInstance.GetComponent<SlotRowPanel>());

            Transform slotTextTf = languageButtonInstance.transform.Find("Text");
            TextMeshProUGUI slotText = slotTextTf.GetComponent<TextMeshProUGUI>();
            if(LanguageManager.CurrentLanguage.metadata.langName == language) {slotText.text += "\n(<color=green>Selected</color>)";}
            languageButtonInstance.SetActive(true);
        }

        public static bool Prefix(OptionsMenuToManager __instance)
        {
            
            hasAlreadyFetchedLanguages = false;
            languageButtons.Clear();

            if (GetCurrentSceneName() == "Main Menu")
            {
                Logging.Message("In main menu");
            }

            Logging.Message("Adding language option to options menu...");

            Transform optionsParent = __instance.optionsMenu.transform;
            Transform navigationRail = optionsParent.Find("Navigation Rail");
            Transform pagesParent = optionsParent.Find("Pages");
            Transform generalPage = pagesParent.Find("General");
            Transform generalScrollRect = generalPage.Find("Scroll Rect");
            Transform generalContents = generalScrollRect.Find("Contents");
            RectTransform generalScrollRectTransform = generalScrollRect.GetComponent<RectTransform>();
            RectTransform generalContentsTransform = generalContents.GetComponent<RectTransform>();


            Logging.Message("Creating language settings page...");
            langLocalPage = new GameObject("Language Page", typeof(RectTransform), typeof(CanvasRenderer));
            langLocalPage.transform.SetParent(pagesParent, false);
            langLocalPage.SetActive(false);
            RectTransform pageRect = langLocalPage.GetComponent<RectTransform>();
            pageRect.sizeDelta = new Vector2(600, 800);

            // ScrollView
            GameObject scrollView = new GameObject("Scroll Rect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollView.transform.SetParent(langLocalPage.transform, false);
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = generalScrollRectTransform.anchorMin;
            scrollRect.anchorMax = generalScrollRectTransform.anchorMax;
            scrollRect.pivot = generalScrollRectTransform.pivot;
            scrollRect.anchoredPosition = generalScrollRectTransform.anchoredPosition;
            scrollRect.sizeDelta = generalScrollRectTransform.sizeDelta;
            scrollView.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            scrollView.GetComponent<Mask>().showMaskGraphic = false;

            // ScrollRect settings to limit side-to-side scrolling
            ScrollRect scrollRectComponent = scrollView.GetComponent<ScrollRect>();
            scrollRectComponent.horizontal = false; // Disable horizontal scrolling
            scrollRectComponent.vertical = true; // Enable vertical scrolling
            scrollRectComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            // Adding a scrollbar
            Transform referencePage = pagesParent.transform.Find("General");
            Scrollbar referenceScrollbar = referencePage.GetComponentsInChildren<Scrollbar>().FirstOrDefault();
            GameObject scrollbar = GameObject.Instantiate(referenceScrollbar.gameObject, langLocalPage.transform);
            scrollbar.transform.SetParent(langLocalPage.transform, false);
            RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();

            Scrollbar scrollbarComponent = scrollbar.GetComponent<Scrollbar>();
            scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;
            scrollRectComponent.verticalScrollbar = scrollbarComponent;
            scrollRectComponent.scrollSensitivity = 20f;

            // Content Container
            GameObject content = new GameObject("Contents", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(scrollView.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = generalContentsTransform.anchorMin;
            contentRect.anchorMax = generalContentsTransform.anchorMax;
            contentRect.pivot = generalContentsTransform.pivot;
            contentRect.anchoredPosition = generalContentsTransform.anchoredPosition;
            contentRect.sizeDelta = generalContentsTransform.sizeDelta;

            VerticalLayoutGroup vGroup = content.GetComponent<VerticalLayoutGroup>();
            vGroup.spacing = 10;
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandWidth = true;
            vGroup.childForceExpandHeight = false;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollView.GetComponent<ScrollRect>().content = contentRect;

            GameObject titleObject = GameObject.Instantiate(optionsParent.Find("Text").gameObject, content.transform);
            titleObject.name = "Title";
            languagePageTitleText = titleObject.GetComponent<TextMeshProUGUI>();
            languagePageTitleText.font = Core.GlobalFontTMP;
            languagePageTitleText.text = "--" + LanguageManager.CurrentLanguage.options.language_languages + "--";
            languagePageTitleText.alignment = TextAlignmentOptions.Center;
            languagePageTitleText.fontSize = 24;

            RectTransform titleRect = languagePageTitleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(400, 50);

            Logging.Message("Creating language menu button...");
            GameObject languageButton = ButtonUtils.CreateTMPButton(navigationRail, "Language", LanguageManager.CurrentLanguage.options.language_title, () => ShowLanguagePage(), changeSize: false);
            languageButtonText = languageButton.Child("Text").TMP();
            RectTransform sourceRect = navigationRail.gameObject.Child("General").GetComponent<RectTransform>();
            RectTransform targetRect = languageButton.GetComponent<RectTransform>();
            targetRect.sizeDelta = sourceRect.sizeDelta;
            //targetRect.anchorMin = sourceRect.anchorMin;
            //targetRect.anchorMax = sourceRect.anchorMax;
            //targetRect.pivot = sourceRect.pivot;


            languageButton.transform.SetSiblingIndex(7);
            Logging.Message("Adding language selection buttons...");
            foreach (string language in LanguageManager.allLanguages.Keys)
            {
                GameObject langButton = ButtonUtils.CreateTMPButton(content.transform, language, LanguageManager.allLanguages[language].metadata.langDisplayName, delegate
                {
                    SelectLanguage(language);
                    foreach (Transform child in content.transform)
                    {
                        if (child.name != "Title" && child.name != "LangBrowser" && child.name.Contains("-"))
                        {
                            TextMeshProUGUI tC = child.GetComponentInChildren<TextMeshProUGUI>();
                            if (tC != null && LanguageManager.allLanguages.ContainsKey(child.name))
                            {
                                tC.text = LanguageManager.allLanguages[child.name].metadata.langDisplayName;
                                if (LanguageManager.CurrentLanguage.metadata.langName == child.name) { tC.text += "\n<size=22>(<color=green>Selected</color>)</size>"; }
                                else if (tC.text.Contains("<color=green>Selected</color>"))
                                {
                                    tC.text = LanguageManager.allLanguages[child.name].metadata.langDisplayName;
                                }
                            }
                        }
                    }
                });

                TextMeshProUGUI textComponent = langButton.GetComponentInChildren<TextMeshProUGUI>();
                if (LanguageManager.CurrentLanguage.metadata.langName == language) { textComponent.text += "\n<size=22>(<color=green>Selected</color>)</size>"; }
            }

            Logging.Message("Creating Open Language Folder button...");

            GameObject openLangFolder = ButtonUtils.CreateTMPButton(content.transform, "openLangFolder", "<color=#03fc07>" + LanguageManager.CurrentLanguage.options.language_openLanguageFolder + "</color>", () => Application.OpenURL(Path.Combine(BepInEx.Paths.ConfigPath, "ultrakull")));

            void ShowLanguagePage()
            {
                Logging.Message("Opening Language Settings Page...");
                EventSystem.current.SetSelectedGameObject(null);
                foreach (Transform page in pagesParent)
                {
                    if (page != null)
                    {
                        page.gameObject.SetActive(false);
                    }
                }
                if (langLocalPage != null)
                {
                    langLocalPage.SetActive(true);
                    foreach (Transform child in navigationRail)
                    {
                        if (child.TryGetComponent(out Button b))
                        {
                            ColorBlock cb = b.colors;
                            b.colors = cb; // Force update colors (sometimes helps)
                        }
                    }
                    Transform navRail = GetInactiveRootObject("Canvas").Child("OptionsMenu").transform.Find("Navigation Rail");
                    GameObject langBtn = navRail.gameObject.Child("Language");
                    EventSystem.current.SetSelectedGameObject(langBtn);
                }
            }

            void SelectLanguage(string language)
            {
                Logging.Message("Selected language: " + language);
                LanguageManager.SetCurrentLanguage(language);
            }

            Logging.Message("Setting up navigation buttons to hide language page...");
            foreach (Transform child in navigationRail)
            {
                if (child.name != "Language" && child.name != "Saves")
                {
                    Button navButton = child.GetComponent<Button>();
                    if (navButton != null)
                    {
                        navButton.onClick.AddListener(() =>
                        {
                            if (langLocalPage.activeSelf)
                            {
                                Logging.Message("Hiding Language Page as another button was clicked: " + child.name);
                                langLocalPage.SetActive(false);
                            }
                        });
                    }
                }
            }

            Logging.Message("Creating language browser page...");
            langBrowserPage = new GameObject("LanguageBrowser", typeof(RectTransform));
            langBrowserPage.transform.SetParent(pagesParent, false);
            langBrowserPage.SetActive(false);

            VerticalLayoutGroup browserLayout = langBrowserPage.AddComponent<VerticalLayoutGroup>();
            browserLayout.spacing = 10f;
            browserLayout.childAlignment = TextAnchor.UpperCenter;

            Logging.Message("Creating Browse Online Languages button...");
            GameObject browseLangButtonObj = ButtonUtils.CreateTMPButton(content.transform, "LangBrowser", "<color=#03fc07>→Browse langs online←</color>", () =>
            {
                langLocalPage.SetActive(false);
                langBrowserPage.SetActive(true);
                if (langBrowserPage.transform.Find("Title") == null)
                {
                    getOnlineLanguages(pagesParent, langLocalPage);
                }
            });

            browseLangButtonObj.GetComponent<Button>().interactable = false;
            var text = browseLangButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = "<color=#028702>→Browse langs online←</color>";

            Logging.Info("Browse Language button added successfully.");
            return true;
        }

    }
}
