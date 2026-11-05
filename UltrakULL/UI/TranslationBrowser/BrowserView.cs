using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Online translation browser embedded in the options menu.
    ///
    /// Layout:
    ///   left ~27% — search, sort, language filter, status, list, count + refresh
    ///   center ~73% — details
    ///
    /// Selection never downloads; only the download button does.
    /// </summary>
    public sealed class BrowserView
    {
        public GameObject Root;
        public BrowserState State = new BrowserState();

        private GameObject _onlinePane;

        /// <summary>Tab bar: Online / Installed / Settings.</summary>
        private GameObject _onlineTab;
        private GameObject _installedTab;
        private GameObject _settingsTab;
        private GameObject _onlinePage;
        private GameObject _installedPage;
        private GameObject _settingsPage;

        private GameObject _installedListPane;

        private GameObject _listContent;
        private UiFactory.ScrollViewLayout _listScroll;
        private GameObject _installedListContent;
        private readonly List<TranslationCardView> _cards = new List<TranslationCardView>();
        private readonly List<TranslationCardView> _installedCards = new List<TranslationCardView>();
        private string _installedSelectedId;
        private TranslationDetailsView _installedDetails;
        private TranslationDetailsView _details;
        private TextMeshProUGUI _installedActionBarLabel;
        private TextMeshProUGUI _countLabel;
        private TextMeshProUGUI _sortButtonLabel;
        private TextMeshProUGUI _filterButtonLabel;
        private TextMeshProUGUI _statusLabel;
        private UnityEngine.Events.UnityAction _onClose;

        // Snapshot of the local packs; rescanned on refresh/install, cached while
        // the user types so a keystroke never triggers a directory scan.
        private List<InstalledPackInfo> _installedPacks = new List<InstalledPackInfo>();

        // Product tour: the Guide button starts it after a confirmation, the
        // first open offers it through a welcome dialog (item 13).
        private ProductTour _tour;
        private GameObject _guideToggle;

        // First online card: used by the product tour to spotlight capability icons.
        private TranslationCardView _firstOnlineCard;

        // Product tour override for the Installed tab: dummy entries that
        // replace the real locally-installed list during the tour.
        private List<TranslationEntry> _installedEntriesOverride;

        // Tracks which tab the tour is currently on, so Prepare actions don't
        // switch tabs when the user navigates back/forward between steps.
        private int? _tourCurrentTab;

        // Selected entry id before the tour started, to restore after the tour.
        private string _savedSelectedId;

        // Selected installed-pack id before the tour started, to restore after.
        private string _savedInstalledSelectedId;

        // Product tour fake data: real data is saved here and replaced with
        // dummy entries during the tour so the tour always shows consistent
        // example data regardless of what the user actually has installed.
        private List<TranslationEntry> _savedCatalogEntries;
        private List<InstalledPackInfo> _savedInstalledPacks;

        // ---------------------------------------------------------------------
        // Construction
        // ---------------------------------------------------------------------

        public static BrowserView Create(Transform parent, string name, UnityEngine.Events.UnityAction onClose)
        {
            BrowserView browser = new BrowserView();
            browser._onClose = onClose;

            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiFactory.StretchToParent(root);
            browser.Root = root;
            // 5.8: Escape closes the browser.
            var escapeCloser = root.AddComponent<EscapeCloser>();
            escapeCloser.onClose = browser._onClose;
            escapeCloser.SetBrowser(browser);

            browser.CreateHeader(root.transform);
            browser.CreateBody(root.transform);
            UiFactory.ForceLayoutHeights(root.transform);

            browser.State.ListChanged += browser.OnListChanged;
            browser.State.SelectionChanged += browser.OnSelectionChanged;
            browser.State.RefreshRequested += delegate { browser.StartLoad(true); };
            browser.State.InstallingChanged += browser.OnInstallingChanged;
            browser.State.DownloadRequested += delegate (TranslationEntry entry)
            {
                Canvas canvas = browser.Root != null ? browser.Root.GetComponentInParent<Canvas>() : null;
                DownloadFlow.ConfirmDownload(canvas, entry);
            };

            browser.RefreshInstalledPacks();

            return browser;
        }

        public void StartLoad(bool force = false)
        {
            SetStatus(BrowserText.StatusLoading);
            LoadAsync(force);
        }

        private async void LoadAsync(bool force)
        {
            bool ok = await CatalogService.LoadAsync(force);
            if (!ok && !CatalogService.LoadedFromCache)
            {
                SetStatus(string.IsNullOrEmpty(CatalogService.LastError)
                    ? BrowserText.StatusNotLoaded
                    : BrowserText.StatusError(CatalogService.LastError));
            }
            State.Recompute();
            State.SelectFirst();
        }

        // ---------------------------------------------------------------------
        // Local pack snapshot
        // ---------------------------------------------------------------------

        /// <summary>Root folder that holds every language pack.</summary>
        private static string PacksRootFolder
        {
            get { return Path.Combine(Paths.ConfigPath, "ultrakull"); }
        }

        /// <summary>
        /// Rescans the language pack folders on disk. Called on browser creation,
        /// on refresh and after installs; the cached snapshot feeds the install
        /// status badges of the online cards and the Installed tab.
        /// </summary>
        public void RefreshInstalledPacks()
        {
            try
            {
                _installedPacks = InstalledPackRegistry.Scan(PacksRootFolder);
            }
            catch (Exception e)
            {
                Logging.Warn("[Browser] Failed to scan installed packs: " + e.Message);
                _installedPacks = new List<InstalledPackInfo>();
            }
        }

        // ---------------------------------------------------------------------
        // Header
        // ---------------------------------------------------------------------

        private void CreateHeader(Transform parent)
        {
            CreateFullHeader(parent);
        }

        private void CreateFullHeader(Transform parent)
        {
            GameObject header = UiFactory.CreatePanel(parent, "Header", new Vector2(0f, 46f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 46f);

            HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 12f;
            headerLayout.padding = new RectOffset(10, 10, 0, 0);
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            // Control widths so the title column keeps its reserved width and the spacer
            // pushes the close button to the right; otherwise the title column collapses
            // and its text overlaps the tabs.
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            // Logo mark + title
            GameObject logoMark = UiFactory.CreatePanel(header.transform, "LogoMark", new Vector2(30f, 30f), StylePalette.AccentRedDark, StylePalette.AccentRed, 1f);
            UiFactory.AddLayoutElement(logoMark, 30f, 30f, 30f, 30f, 0f, 0f);
            TextMeshProUGUI logoText = UiFactory.CreateLabel(logoMark.transform, "U", "U", 16f, Color.white, TextAlignmentOptions.Center, false);
            UiFactory.StretchToParent(logoText);
            TooltipService.Attach(logoText, "ULTRAKILL");

            GameObject titleColumn = new GameObject("TitleColumn", typeof(RectTransform));
            titleColumn.transform.SetParent(header.transform, false);
            VerticalLayoutGroup titleLayout = titleColumn.AddComponent<VerticalLayoutGroup>();
            titleLayout.spacing = 0f;
            titleLayout.childAlignment = TextAnchor.MiddleLeft;
            titleLayout.childControlWidth = true;
            titleLayout.childControlHeight = false;
            titleLayout.childForceExpandWidth = false;
            titleLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(titleColumn, 0f, 38f, 230f, 38f, 0f, 0f);

            TextMeshProUGUI title = UiFactory.CreateLabel(titleColumn.transform, "Title", BrowserText.Title, 16f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(title);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 20f, 230f, 20f, 0f, 0f);
            TextMeshProUGUI subtitle = UiFactory.CreateLabel(titleColumn.transform, "Subtitle", BrowserText.Subtitle, 11f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(subtitle);
            UiFactory.AddLayoutElement(subtitle.gameObject, 0f, 16f, 230f, 16f, 0f, 0f);

            // Tab spacer
            GameObject tabSpacer = new GameObject("TabSpacer", typeof(RectTransform));
            tabSpacer.transform.SetParent(header.transform, false);
            UiFactory.AddLayoutElement(tabSpacer, 12f, 0f, 12f, 0f, 0f, 0f);

            // Tabs: Online / Installed / Settings
            _onlineTab = CreateTab(header.transform, "TabOnline", BrowserText.TabOnline, true, delegate { SwitchTab(0); });
            _installedTab = CreateTab(header.transform, "TabInstalled", BrowserText.TabInstalled, false, delegate { SwitchTab(1); });
            _settingsTab = CreateTab(header.transform, "TabSettings", BrowserText.TabSettings, false, delegate { SwitchTab(2); });

            // Guide button: starts the product tour after a confirmation
            // (item 13). Sized to fit the "Guide" label with padding.
            _guideToggle = UiFactory.CreateButton(header.transform, "GuideToggle", BrowserText.ToggleGuide, ConfirmRestartTour, new Vector2(86f, 32f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(_guideToggle, 86f, 32f, 86f, 32f, 0f, 0f);
            TextMeshProUGUI guideLabel = null;
            var guideLabelTransform = _guideToggle.transform.Find("Label");
            if (guideLabelTransform != null)
            {
                guideLabel = guideLabelTransform.GetComponent<TextMeshProUGUI>();
            }
            if (guideLabel != null)
            {
                // Guard for localized labels: shrink the font instead of clipping.
                guideLabel.enableAutoSizing = true;
                guideLabel.fontSizeMin = 10f;
                guideLabel.fontSizeMax = 15f;
            }
            TooltipService.Attach(_guideToggle, BrowserText.TooltipTour);

            // Push the tabs away from the title
            GameObject headerSpacer = new GameObject("HeaderSpacer", typeof(RectTransform));
            headerSpacer.transform.SetParent(header.transform, false);
            UiFactory.AddLayoutElement(headerSpacer, 0f, 0f, 0f, 0f, 1f, 0f);
        }

        private void SwitchTab(int index)
        {
            SetTabStyle(_onlineTab, index == 0);
            SetTabStyle(_installedTab, index == 1);
            SetTabStyle(_settingsTab, index == 2);

            // Exactly one tab root is active at a time. The old layout kept the details
            // and legend panels permanently visible, which left the tab pages overlapping.
            if (_onlinePage != null)
            {
                _onlinePage.SetActive(index == 0);
            }
            if (_installedPage != null)
            {
                _installedPage.SetActive(index == 1);
            }
            if (_settingsPage != null)
            {
                _settingsPage.SetActive(index == 2);
            }

            if (index == 1)
            {
                BuildInstalledList();
            }
        }

        private GameObject CreateTab(Transform parent, string name, string text, bool active, Action onClick)
        {
            Color bg = active ? StylePalette.TabActiveBg : StylePalette.PanelAlt;
            Color border = active ? StylePalette.AccentRed : StylePalette.BorderBlue;
            Color textColor = active ? Color.white : StylePalette.TextSecondary;

            GameObject tab = UiFactory.CreateButton(parent, name, text, onClick, new Vector2(100f, 30f), bg, border, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(tab, 100f, 30f, 100f, 30f, 0f, 0f);
            // Guard: CreateButton may not build a child named "Label"; Find returns
            // null then, and calling GetComponent on null crashes.
            Transform labelTransform = tab.transform.Find("Label");
            TextMeshProUGUI label = labelTransform != null ? labelTransform.GetComponent<TextMeshProUGUI>() : null;
            if (label != null)
            {
                label.text = text;
                label.color = textColor;
                label.alignment = TextAlignmentOptions.Center;
            }
            return tab;
        }

        private void SetTabStyle(GameObject tab, bool active)
        {
            if (tab == null)
                return;

            UiFactory.SetBorderColor(tab, active ? StylePalette.AccentRed : StylePalette.BorderBlue);
            Image bg = tab.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = active ? StylePalette.TabActiveBg : StylePalette.PanelAlt;
            }
            // Guard: the tab label child might be missing; Find returns null then
            // and calling GetComponent on null crashes.
            Transform labelTransform = tab.transform.Find("Label");
            TextMeshProUGUI label = labelTransform != null ? labelTransform.GetComponent<TextMeshProUGUI>() : null;
            if (label != null)
            {
                label.color = active ? Color.white : StylePalette.TextSecondary;
            }
        }

        // ---------------------------------------------------------------------
        // Body
        // ---------------------------------------------------------------------

        private void CreateBody(Transform parent)
        {
            GameObject body = new GameObject("Body", typeof(RectTransform));
            body.transform.SetParent(parent, false);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.offsetMin = new Vector2(0f, 0f);
            bodyRect.offsetMax = new Vector2(0f, -48f);

            // The body only hosts the three tab pages; each page stretches to fill it and
            // carries its own layout. Exactly one page is active at a time.
            _onlinePage = CreateTabPage(body.transform, "OnlinePage");
            _installedPage = CreateTabPage(body.transform, "InstalledPage");
            _settingsPage = CreateTabPage(body.transform, "SettingsPage");

            BuildOnlineTab(_onlinePage.transform);
            BuildInstalledTab(_installedPage.transform);
            BuildSettingsTab(_settingsPage.transform);

            _installedPage.SetActive(false);
            _settingsPage.SetActive(false);
        }

        private static GameObject CreateTabPage(Transform parent, string name)
        {
            GameObject page = new GameObject(name, typeof(RectTransform));
            page.transform.SetParent(parent, false);
            UiFactory.StretchToParent(page);
            return page;
        }

        private static HorizontalLayoutGroup AddThreeColumnLayout(GameObject page)
        {
            HorizontalLayoutGroup layout = page.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return layout;
        }

        // ---- Online tab: list (27%) | details (54%) | legend (19%) ----
        private void BuildOnlineTab(Transform page)
        {
            AddThreeColumnLayout(page.gameObject);

            _onlinePane = UiFactory.CreatePanel(page, "OnlinePane", Vector2.zero, new Color(0f, 0f, 0f, 0f), null);
            UiFactory.AddLayoutElement(_onlinePane, 300f, 0f, 340f, 0f, 0.72f, 1f);
            CreateLeftPane(_onlinePane.transform);

            GameObject centerPane = UiFactory.CreatePanel(page, "CenterPane", Vector2.zero, new Color(0f, 0f, 0f, 0f), null);
            UiFactory.AddLayoutElement(centerPane, 0f, 0f, 500f, 100f, 3f, 1f);

            _details = TranslationDetailsView.Create(centerPane.transform, State);
            // The action area is a child of the details root so the details layout
            // group reserves a fixed row for it and the scroll area takes the space
            // above it (instead of the scroll area covering an overlapping sibling
            // button). It hosts the status button or the download progress bar.
            _details.CreateActionArea(_details.Root.transform);
        }

        // ---- Installed tab: same layout as Online, but "Use" instead of download ----
        private void BuildInstalledTab(Transform page)
        {
            AddThreeColumnLayout(page.gameObject);

            _installedListPane = UiFactory.CreatePanel(page, "InstalledListPane", Vector2.zero, new Color(0f, 0f, 0f, 0f), null);
            UiFactory.AddLayoutElement(_installedListPane, 300f, 0f, 340f, 0f, 0.72f, 1f);
            CreateInstalledListPane(_installedListPane.transform);

            GameObject centerPane = UiFactory.CreatePanel(page, "InstalledCenterPane", Vector2.zero, new Color(0f, 0f, 0f, 0f), null);
            UiFactory.AddLayoutElement(centerPane, 0f, 0f, 500f, 100f, 3f, 1f);

            _installedDetails = TranslationDetailsView.Create(centerPane.transform, State);
            GameObject installedActionBar = _installedDetails.CreateActionBar(_installedDetails.Root.transform, BrowserText.ActionUse, PrimitiveSprites.Kind.Check, UseSelectedInstalled);
            _installedActionBarLabel = installedActionBar != null
                ? (installedActionBar.transform.Find("Label") != null ? installedActionBar.transform.Find("Label").GetComponent<TextMeshProUGUI>() : null)
                : null;
        }

        private void BuildSettingsTab(Transform page)
        {
            VerticalLayoutGroup layout = page.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = UiFactory.CreateLabel(page.transform, "Title", BrowserText.SettingsTitle, 20f, StylePalette.TextPrimary, TextAlignmentOptions.Center, false);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 30f, 0f, 30f, 1f, 0f);

            AddNativeToggle(page.transform, "activeDubbing", "Active dubbing (replace dialogue with dubbed audio)",
                ReadConfigBool("activeDubbing", false), delegate (bool v) { WriteConfigBool("activeDubbing", v); });
            AddNativeToggle(page.transform, "extraDubbing", "Extra dubbing (optional voiced lines)",
                ReadConfigBool("extraDubbing", false), delegate (bool v) { WriteConfigBool("extraDubbing", v); });
            AddNativeToggle(page.transform, "bookAudioDubbing", "Book audio dubbing (books and terminals)",
                ReadConfigBool("bookAudioDubbing", false), delegate (bool v) { WriteConfigBool("bookAudioDubbing", v); });
            AddNativeToggle(page.transform, "replaceTextures", "Replace textures (swap game textures with translated ones)",
                ReadConfigBool("replaceTextures", true), delegate (bool v)
                {
                    WriteConfigBool("replaceTextures", v);
                    // Live toggle: apply or remove the replacements of the
                    // current scene right away (item 12).
                    UltrakULL.Harmony_Patches.TexturePatcher.OnReplaceTexturesSettingChanged();
                });

            GameObject openFolder = UiFactory.CreateNativeButton(page.transform, "OpenFolder", BrowserText.SettingsOpenFolder,
                delegate { Application.OpenURL(PacksRootFolder); });
            UiFactory.AddLayoutElement(openFolder, 0f, 44f, 0f, 44f, 1f, 0f);

            UiFactory.ForceLayoutHeights(page.transform);
        }

        /// <summary>
        /// Guide button handler: asks for a confirmation first, so the tour is
        /// never restarted by a stray click; declining changes nothing (13.6).
        /// </summary>
        private void ConfirmRestartTour()
        {
            if (_tour != null)
            {
                // The tour is already running; a second click does nothing.
                return;
            }

            Canvas canvas = Root != null ? Root.GetComponentInParent<Canvas>() : null;
            if (canvas == null)
            {
                return;
            }

            ConfirmDialog.Show(canvas, BrowserText.TourRestartTitle, BrowserText.TourRestartBody,
                StartProductTour,
                BrowserText.TourWelcomeStart, BrowserText.TourWelcomeNotNow);
        }

        /// <summary>
        /// Welcome dialog of the first browser open: offers the tour once per
        /// installation; "Start" runs it, "Not now" only stores that the offer
        /// was seen (13.5). Called when the language page is shown, not on
        /// construction, so the dialog never appears before the mod page is open.
        /// </summary>
        public void ShowWelcomeOnce()
        {
            try
            {
                if (ReadConfigBool("tourWelcomeShown", false))
                {
                    return;
                }

                Canvas canvas = Root != null ? Root.GetComponentInParent<Canvas>() : null;
                if (canvas == null)
                {
                    return;
                }

                WriteConfigBool("tourWelcomeShown", true);
                ConfirmDialog.Show(canvas, BrowserText.TourWelcomeTitle, BrowserText.TourWelcomeBody,
                    StartProductTour,
                    BrowserText.TourWelcomeStart, BrowserText.TourWelcomeNotNow);
            }
            catch (Exception e)
            {
                Logging.Warn("[BrowserView] Failed to show the tour welcome: " + e.Message);
            }
        }

        // ---------------------------------------------------------------------
        // Product tour (item 13)
        // ---------------------------------------------------------------------

        // ---------------------------------------------------------------------
        // Product tour fake data
        // ---------------------------------------------------------------------

        /// <summary>Creates dummy translation entries for the product tour so
        /// the tour always shows consistent example data.</summary>
        private static List<TranslationEntry> CreateFakeCatalogEntries()
        {
            var entries = new List<TranslationEntry>();

            entries.Add(new TranslationEntry
            {
                id = "demo-official-russian",
                name = "ULTRAKILL: Official Russian Localization",
                languageTag = "ru-RU",
                author = "UltrakULL Localization Group",
                teamName = "UltrakULL Localization Group",
                version = "1.3.4",
                readme = "Официальная русская локализация ULTRAKILL с адаптированными игровыми терминами, интерфейсом и авторским подходом к переводу. Включает визуальные улучшения и подборку шрифтов.",
                discordUrl = "https://discord.gg/example",
                likes = 42,
                logoUrl = "https://picsum.photos/seed/fake-russian-logo/128/128",
                flagUrl = "https://picsum.photos/seed/fake-russian-flag/64/32",
                features = new TranslationFeatures
                {
                    dubbing = FeatureStatus.Full,
                    extraDubbing = FeatureStatus.Partial,
                    textures = FeatureStatus.Full,
                    fonts = FeatureStatus.Full,
                },
                socialLinks = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Steam Workshop", "https://steamcommunity.com/sharedfiles/filedetails/?id=3606721877"),
                    new KeyValuePair<string, string>("YouTube", "https://www.youtube.com/watch?v=dQw4w9WgXcQ"),
                    new KeyValuePair<string, string>("GitHub", "https://github.com/example/ultrakull-russian"),
                },
            });

            entries.Add(new TranslationEntry
            {
                id = "demo-community-goblin",
                name = "ULTRAKILL: Community Goblin Translation",
                languageTag = "ru-RU",
                author = "Community Goblin Team",
                teamName = "Community Goblin Team",
                version = "2.1.0",
                readme = "Полный перевод игровых текстов на русский язык с авторским стилем оформления и улучшенной типографикой. Для взрослой аудитории.",
                likes = 17,
                logoUrl = "https://picsum.photos/seed/fake-goblin-logo/128/128",
                flagUrl = "https://picsum.photos/seed/fake-goblin-flag/64/32",
                features = new TranslationFeatures
                {
                    dubbing = FeatureStatus.Partial,
                    extraDubbing = FeatureStatus.None,
                    textures = FeatureStatus.None,
                    fonts = FeatureStatus.Full,
                },
                socialLinks = null,
            });

            entries.Add(new TranslationEntry
            {
                id = "demo-enhanced-english",
                name = "ULTRAKILL: Enhanced English Localization",
                languageTag = "en-GB",
                author = "Community Localizers",
                teamName = "Community Localizers",
                version = "0.9.2",
                readme = "An enhanced English localization with improved terminology, corrected typos, and expanded font support for special characters.",
                likes = 83,
                logoUrl = "https://picsum.photos/seed/fake-english-logo/128/128",
                flagUrl = "https://picsum.photos/seed/fake-english-flag/64/32",
                features = new TranslationFeatures
                {
                    dubbing = FeatureStatus.Full,
                    extraDubbing = FeatureStatus.Full,
                    textures = FeatureStatus.Full,
                    fonts = FeatureStatus.Partial,
                },
                socialLinks = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Discord", "https://discord.gg/example2"),
                },
            });

            return entries;
        }

        /// <summary>Creates dummy installed packs for the product tour.</summary>
        private static List<InstalledPackInfo> CreateFakeInstalledPacks()
        {
            var packs = new List<InstalledPackInfo>();

            var rickrollManifest = new TranslationManifest
            {
                translationId = "demo-official-russian",
                name = "ULTRAKILL: Official Russian Localization",
                languageCode = "ru-RU",
                authors = "UltrakULL Localization Group",
                version = "1.3.4",
                source = "catalog",
                likes = 42,
                featuresJson = TranslationManifest.FeaturesToJson(new TranslationFeatures
                {
                    dubbing = FeatureStatus.Full,
                    extraDubbing = FeatureStatus.Partial,
                    textures = FeatureStatus.Full,
                    fonts = FeatureStatus.Full,
                }),
                socialLinksJson = TranslationManifest.SocialLinksToJson(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Steam Workshop", "https://steamcommunity.com/sharedfiles/filedetails/?id=3606721877"),
                    new KeyValuePair<string, string>("YouTube", "https://www.youtube.com/watch?v=dQw4w9WgXcQ"),
                    new KeyValuePair<string, string>("GitHub", "https://github.com/example/ultrakull-russian"),
                }),
            };

            packs.Add(new InstalledPackInfo
            {
                Folder = "demo-official-russian",
                PackKey = "demo-official-russian",
                Manifest = rickrollManifest,
                Metadata = new InstalledPackMetadata
                {
                    languageCode = "ru-RU",
                    languageName = "Русский",
                    translationVersion = "1.3.4",
                    translationId = "demo-official-russian",
                    authors = "UltrakULL Localization Group",
                },
            });

            var goblinManifest = new TranslationManifest
            {
                translationId = "demo-community-goblin",
                name = "ULTRAKILL: Community Goblin Translation",
                languageCode = "ru-RU",
                authors = "Community Goblin Team",
                // Deliberately older than the catalog entry (2.1.0) so the
                // resolver reports UpdateAvailable for this translation -
                // the tour's Updates step spotlights that red badge.
                version = "2.0.0",
                source = "catalog",
                likes = 17,
                featuresJson = TranslationManifest.FeaturesToJson(new TranslationFeatures
                {
                    dubbing = FeatureStatus.Partial,
                    extraDubbing = FeatureStatus.None,
                    textures = FeatureStatus.None,
                    fonts = FeatureStatus.Full,
                }),
                socialLinksJson = null,
            };

            packs.Add(new InstalledPackInfo
            {
                Folder = "demo-community-goblin",
                PackKey = "demo-community-goblin",
                Manifest = goblinManifest,
                Metadata = new InstalledPackMetadata
                {
                    languageCode = "ru-RU",
                    languageName = "Русский (гоблинский)",
                    translationVersion = "2.0.0",
                    translationId = "demo-community-goblin",
                    authors = "Community Goblin Team",
                },
            });

            var englishManifest = new TranslationManifest
            {
                translationId = "demo-enhanced-english",
                name = "ULTRAKILL: Enhanced English Localization",
                languageCode = "en-GB",
                authors = "Community Localizers",
                version = "0.9.2",
                source = "catalog",
                likes = 83,
                featuresJson = TranslationManifest.FeaturesToJson(new TranslationFeatures
                {
                    dubbing = FeatureStatus.Full,
                    extraDubbing = FeatureStatus.Full,
                    textures = FeatureStatus.Full,
                    fonts = FeatureStatus.Partial,
                }),
                socialLinksJson = TranslationManifest.SocialLinksToJson(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Discord", "https://discord.gg/example2"),
                }),
            };

            packs.Add(new InstalledPackInfo
            {
                Folder = "demo-enhanced-english",
                PackKey = "demo-enhanced-english",
                Manifest = englishManifest,
                Metadata = new InstalledPackMetadata
                {
                    languageCode = "en-GB",
                    languageName = "English",
                    translationVersion = "0.9.2",
                    translationId = "demo-enhanced-english",
                    authors = "Community Localizers",
                },
            });

            return packs;
        }

        /// <summary>
        /// Dummy entries for the Installed tab during the product tour, built
        /// from the fake installed packs so both views show the same data.
        /// </summary>
        private static List<TranslationEntry> CreateFakeInstalledEntries(List<InstalledPackInfo> packs)
        {
            var entries = new List<TranslationEntry>();
            if (packs == null)
            {
                return entries;
            }

            foreach (InstalledPackInfo pack in packs)
            {
                TranslationManifest manifest = pack != null ? pack.Manifest : null;
                if (manifest == null)
                {
                    continue;
                }
                entries.Add(new TranslationEntry
                {
                    id = string.IsNullOrEmpty(manifest.translationId) ? pack.PackKey : manifest.translationId,
                    name = string.IsNullOrEmpty(manifest.name) ? pack.PackKey : manifest.name,
                    languageTag = manifest.languageCode,
                    author = manifest.authors,
                    teamName = manifest.authors,
                    version = manifest.version,
                    readme = pack.Metadata != null ? pack.Metadata.languageName : null,
                    likes = manifest.likes,
                    features = TranslationManifest.FeaturesFromJson(manifest.featuresJson),
                });
            }
            return entries;
        }

        /// <summary>Sets the Installed-tab dummy entries for the product tour.</summary>
        public void SetInstalledEntriesOverride(List<TranslationEntry> entries)
        {
            _installedEntriesOverride = entries;
        }

        /// <summary>Restores the real Installed-tab list after a tour override.</summary>
        public void ClearInstalledEntriesOverride()
        {
            _installedEntriesOverride = null;
        }

        // ---------------------------------------------------------------------
        // Product tour
        // ---------------------------------------------------------------------

        /// <summary>
        /// Runs the product tour. Real catalog and installed data are saved
        /// and replaced with dummy entries during the tour so the tour always
        /// shows consistent example data. Finishing or skipping restores the
        /// tab the user was on and touches nothing else (13.7).
        /// </summary>
        /// <summary>
        /// Switches the browser tab for a tour step. Skips the switch when the
        /// tour is already on that tab, so back/forward navigation between
        /// steps never re-triggers tab side effects.
        /// </summary>
        private void TourEnsureTab(int tab)
        {
            if (_tourCurrentTab != tab)
            {
                SwitchTab(tab);
                _tourCurrentTab = tab;
            }
        }

        private void StartProductTour()
        {
            if (_tour != null || Root == null)
            {
                return;
            }

            // Save real data and replace with dummy entries for the tour.
            _savedCatalogEntries = new List<TranslationEntry>(State.VisibleEntries);
            _savedInstalledPacks = new List<InstalledPackInfo>(_installedPacks);
            _savedSelectedId = State.SelectedId;

            int originalTab = _installedPage != null && _installedPage.activeSelf ? 1 : 0;
            _tourCurrentTab = originalTab;

            List<ProductTour.TourStep> steps = new List<ProductTour.TourStep>
            {
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepCatalogTitle,
                    Body = BrowserText.TourStepCatalogBody,
                    Prepare = delegate
                    {
                        TourEnsureTab(0);
                        var fakeEntries = CreateFakeCatalogEntries();
                        State.SetVisibleEntriesOverride(fakeEntries);
                        RebuildList();
                        // Select the first fake entry so the details preview shows
                        // tour data, not the real translation that was open before.
                        if (fakeEntries.Count > 0)
                        {
                            State.SetSelected(fakeEntries[0].id);
                        }
                    },
                    Target = delegate { return _onlinePane; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepIconsTitle,
                    Body = BrowserText.TourStepIconsBody,
                    Prepare = delegate
                    {
                        TourEnsureTab(0);
                        State.SetVisibleEntriesOverride(CreateFakeCatalogEntries());
                        RebuildList();
                    },
                    Target = delegate
                    {
                        // Spotlight all four capability icons with ONE frame:
                        // the whole feature row, not each indicator separately.
                        if (_firstOnlineCard != null && _firstOnlineCard.FeatureRow != null)
                        {
                            return _firstOnlineCard.FeatureRow;
                        }
                        if (_firstOnlineCard != null && _firstOnlineCard.Root != null)
                        {
                            return _firstOnlineCard.Root;
                        }
                        return _onlinePane;
                    }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepDetailsTitle,
                    Body = BrowserText.TourStepDetailsBody,
                    Prepare = delegate
                    {
                        TourEnsureTab(0);
                    },
                    Target = delegate { return _details != null ? _details.Root : null; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepDownloadTitle,
                    Body = BrowserText.TourStepDownloadBody,
                    Prepare = delegate
                    {
                        TourEnsureTab(0);
                        // Returning from the "Ready to install" step must restore
                        // the real action label (e.g. "Download"): rebuild the
                        // action button from the current entry and resolution.
                        if (_details != null)
                        {
                            _details.RefreshActionArea();
                        }
                    },
                    Target = delegate { return _details != null ? _details.DownloadButton : null; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepReadyTitle,
                    Body = BrowserText.TourStepReadyBody,
                    Prepare = delegate
                    {
                        TourEnsureTab(0);
                        // The step is about installing/re-installing, so the
                        // details action button must not read "Download".
                        if (_details != null)
                        {
                            _details.SetActionLabel(BrowserText.ActionReadyToInstall);
                        }
                    },
                    Target = delegate { return _details != null ? _details.DownloadButton : null; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepInstalledTitle,
                    Body = BrowserText.TourStepInstalledBody,
                    Prepare = delegate
                    {
                        // Only switch tabs when entering this step from a
                        // different page; navigating back should not re-switch.
                        TourEnsureTab(1);
                        _installedPacks = CreateFakeInstalledPacks();
                        // The Installed tab shows its own dummy entries: the real
                        // list reads the disk via LanguageManager and must not be
                        // touched, so it is overridden for the tour.
                        _savedInstalledSelectedId = _installedSelectedId;
                        _installedSelectedId = null;
                        SetInstalledEntriesOverride(CreateFakeInstalledEntries(_installedPacks));
                        BuildInstalledList();
                    },
                    Target = delegate { return _installedListPane; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepSwitchTitle,
                    Body = BrowserText.TourStepSwitchBody,
                    Prepare = delegate { TourEnsureTab(1); },
                    Target = delegate { return _installedDetails != null ? _installedDetails.Root : null; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepUpdatesTitle,
                    Body = BrowserText.TourStepUpdatesBody,
                    Prepare = delegate
                    {
                        // The update badge lives on the ONLINE cards (resolved
                        // against the installed packs), so show the catalog:
                        // the fake installed goblin pack is one version behind
                        // its fake catalog entry and its card shows the red
                        // "Update" badge.
                        TourEnsureTab(0);
                        _installedPacks = CreateFakeInstalledPacks();
                        RebuildList();
                    },
                    Target = delegate
                    {
                        // Spotlight the "Update available" badge of whichever
                        // card carries it (the outdated fake pack).
                        foreach (TranslationCardView card in _cards)
                        {
                            if (card != null && card.Resolution != null &&
                                card.Resolution.Status == InstallStatus.UpdateAvailable &&
                                card.StatusBadge != null)
                            {
                                return card.StatusBadge;
                            }
                        }
                        if (_firstOnlineCard != null && _firstOnlineCard.Root != null)
                        {
                            return _firstOnlineCard.Root;
                        }
                        return _onlinePane;
                    }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepSettingsTitle,
                    Body = BrowserText.TourStepSettingsBody,
                    Prepare = delegate { TourEnsureTab(2); },
                    Target = delegate { return _settingsPage != null ? _settingsPage : _settingsTab; }
                },
                new ProductTour.TourStep
                {
                    Title = BrowserText.TourStepRepeatTitle,
                    Body = BrowserText.TourStepRepeatBody,
                    Prepare = delegate
                    {
                        TourEnsureTab(0);
                    },
                    Target = delegate { return _guideToggle; }
                }
            };

            _tour = ProductTour.Create(Root.transform, steps, delegate
            {
                // Restore real data after the tour.
                State.ClearVisibleEntriesOverride();
                ClearInstalledEntriesOverride();
                _installedPacks = _savedInstalledPacks ?? new List<InstalledPackInfo>();
                _firstOnlineCard = null;
                _tourCurrentTab = null;

                // Recompute the visible list from the real catalog. This also
                // fires ListChanged, which rebuilds the online cards; without
                // it the list would keep showing the fake tour entries.
                State.Recompute();

                // Restore the Installed tab from the real local languages and
                // the real installed selection.
                _installedSelectedId = _savedInstalledSelectedId;
                _savedInstalledSelectedId = null;
                BuildInstalledList();

                // Restore the selection that was active before the tour. This
                // must run after Recompute: the selected id was a fake id during
                // the tour, so Recompute clears it first.
                string savedId = _savedSelectedId;
                _savedSelectedId = null;
                _savedCatalogEntries = null;
                _savedInstalledPacks = null;
                State.SetSelected(savedId);

                _tour = null;
                SwitchTab(originalTab);
            });
        }

        // ---------------------------------------------------------------------
        // Left pane: search, sort, filter, status, list, footer
        // ---------------------------------------------------------------------

        private void CreateLeftPane(Transform parent)
        {
            // Layout area: search, sort, filter, status, list, footer (in flow).
            GameObject layoutArea = new GameObject("LayoutArea", typeof(RectTransform));
            layoutArea.transform.SetParent(parent, false);
            UiFactory.StretchToParent(layoutArea);
            VerticalLayoutGroup layout = layoutArea.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Search
            TMP_InputField search = UiFactory.CreateInputField(layoutArea.transform, "SearchField", new Vector2(0f, 34f), BrowserText.SearchPlaceholder, "");
            UiFactory.AddLayoutElement(search.gameObject, 0f, 34f, 0f, 34f, 1f, 0f);
            search.onValueChanged.AddListener(delegate (string value) { State.SetSearch(value); });

            // Sort cycle + language filter cycle
            GameObject sortRow = new GameObject("SortRow", typeof(RectTransform));
            sortRow.transform.SetParent(layoutArea.transform, false);
            HorizontalLayoutGroup sortRowLayout = sortRow.AddComponent<HorizontalLayoutGroup>();
            sortRowLayout.spacing = 6f;
            sortRowLayout.childAlignment = TextAnchor.MiddleLeft;
            sortRowLayout.childControlWidth = true;
            sortRowLayout.childControlHeight = true;
            sortRowLayout.childForceExpandWidth = true;
            sortRowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(sortRow, 0f, 30f, 0f, 30f, 1f, 0f);

            GameObject sortButton = UiFactory.CreateButton(sortRow.transform, "SortButton", BuildSortLabel(), State.CycleSortMode, new Vector2(0f, 30f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(sortButton, 0f, 30f, 0f, 30f, 1f, 0f);
            var sortLabelTransform = sortButton.transform.Find("Label");
            _sortButtonLabel = sortLabelTransform != null ? sortLabelTransform.GetComponent<TextMeshProUGUI>() : null;

            GameObject filterButton = UiFactory.CreateButton(sortRow.transform, "LanguageFilterButton", BuildFilterLabel(), CycleLanguageFilter, new Vector2(0f, 30f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(filterButton, 0f, 30f, 0f, 30f, 1f, 0f);
            var filterLabelTransform = filterButton.transform.Find("Label");
            _filterButtonLabel = filterLabelTransform != null ? filterLabelTransform.GetComponent<TextMeshProUGUI>() : null;
            TooltipService.Attach(filterButton, BuildFilterTooltip());

            // Status line
            _statusLabel = UiFactory.CreateLabel(layoutArea.transform, "StatusLabel", "", 12f, StylePalette.TextSecondary, TextAlignmentOptions.Left, true);
            // The row is a fixed 22px: wrap + ellipsis truncates long error lines
            // visibly instead of silently clipping them mid-letter.
            _statusLabel.overflowMode = TextOverflowModes.Ellipsis;
            UiFactory.AddLayoutElement(_statusLabel.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            // Scrollable list (fills the remaining height)
            _listScroll = UiFactory.CreateScrollView(layoutArea.transform, "ListScroll", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            UiFactory.AddLayoutElement(_listScroll.Root.gameObject, 0f, 100f, 10f, 150f, 1f, 1f);

            _listContent = _listScroll.Content.gameObject;
            _listContent.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 16, 0, 0);

            // Scrollbar is attached to the list root so it covers only the list area
            // (a pane-wide overlay intersected the search field and footer).
            UiFactory.CreateScrollbar(_listScroll.Root, "ListScrollbar", _listScroll.ScrollRect);
            RectTransform sbRect = _listScroll.Root.Find("ListScrollbar").GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-3f, 0f);
            sbRect.sizeDelta = new Vector2(10f, 0f);

            // Footer: count + refresh
            GameObject footer = UiFactory.CreatePanel(layoutArea.transform, "Footer", new Vector2(0f, 34f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            UiFactory.AddLayoutElement(footer, 0f, 34f, 0f, 34f, 1f, 0f);

            HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 8f;
            footerLayout.padding = new RectOffset(8, 6, 0, 0);
            footerLayout.childAlignment = TextAnchor.MiddleLeft;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            // Fixed-height row: force-expanding the children vertically is what
            // stretched the square refresh button into a rectangle (item 20).
            footerLayout.childForceExpandHeight = false;

            _countLabel = UiFactory.CreateLabel(footer.transform, "Count", BrowserText.Count(0), 12f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.AddLayoutElement(_countLabel.gameObject, 90f, 28f, 120f, 28f, 1f, 0f);

            GameObject footerSpacer = new GameObject("FooterSpacer", typeof(RectTransform));
            footerSpacer.transform.SetParent(footer.transform, false);
            UiFactory.AddLayoutElement(footerSpacer, 0f, 0f, 0f, 0f, 1f, 0f);

            GameObject refreshButton = UiFactory.CreateButton(footer.transform, "RefreshButton", "", State.RequestRefresh, new Vector2(26f, 26f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(refreshButton, 26f, 26f, 26f, 26f, 0f, 0f);
            Image refreshIcon = UiFactory.CreateImage(refreshButton.transform, "Icon", new Vector2(16f, 16f), Color.white, PrimitiveSprites.Kind.Refresh);
            RectTransform refreshIconRect = refreshIcon.GetComponent<RectTransform>();
            refreshIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            refreshIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            refreshIconRect.pivot = new Vector2(0.5f, 0.5f);
            refreshIconRect.anchoredPosition = Vector2.zero;
            refreshIconRect.sizeDelta = new Vector2(16f, 16f);
            TooltipService.Attach(refreshButton, BrowserText.TooltipRefresh);
        }

        private string BuildSortLabel()
        {
            string label = BrowserText.SortPrefix;
            switch (State.SortMode)
            {
                case CatalogSortMode.Language: label += BrowserText.SortLanguage; break;
                case CatalogSortMode.Updated: label += BrowserText.SortUpdated; break;
                default: label += BrowserText.SortPopularity; break;
            }
            return label;
        }

        private string BuildFilterLabel()
        {
            return BrowserText.FilterLanguagePrefix +
                (string.IsNullOrEmpty(State.FilterLanguage) ? BrowserText.FilterLanguageAll : State.FilterLanguage);
        }

        private string BuildFilterTooltip()
        {
            return BuildFilterLabel() + " (" + BrowserText.FilterLanguageAll.ToLowerInvariant() + " / " + string.Join(", ", AvailableLanguages()) + ")";
        }

        /// <summary>Unique locale codes present in the loaded catalog, sorted.</summary>
        private List<string> AvailableLanguages()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> result = new List<string>();
            foreach (TranslationEntry entry in CatalogService.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.languageTag))
                {
                    continue;
                }
                if (seen.Add(entry.languageTag))
                {
                    result.Add(entry.languageTag);
                }
            }
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private void CycleLanguageFilter()
        {
            List<string> languages = AvailableLanguages();
            if (languages.Count == 0)
            {
                State.SetFilterLanguage(null);
                return;
            }

            string current = State.FilterLanguage;
            if (string.IsNullOrEmpty(current))
            {
                State.SetFilterLanguage(languages[0]);
                return;
            }

            int index = languages.FindIndex(l => string.Equals(l, current, StringComparison.OrdinalIgnoreCase));
            State.SetFilterLanguage(index < 0 || index + 1 >= languages.Count ? null : languages[index + 1]);
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = text ?? "";
                _statusLabel.richText = false;
            }
        }

        /// <summary>
        /// Composes the status line: load state first (loading / error / not
        /// loaded), then list-empty states (empty catalog / no match), then the
        /// freshness info (offline cache / updated).
        /// </summary>
        private void UpdateStatusLabel()
        {
            if (CatalogService.IsLoading)
            {
                SetStatus(BrowserText.StatusLoading);
                return;
            }

            if (!CatalogService.IsLoaded)
            {
                SetStatus(string.IsNullOrEmpty(CatalogService.LastError)
                    ? BrowserText.StatusNotLoaded
                    : BrowserText.StatusError(CatalogService.LastError));
                return;
            }

            if (State.VisibleEntries.Count == 0)
            {
                SetStatus(CatalogService.Count == 0 ? BrowserText.StatusEmptyCatalog : BrowserText.StatusNoMatch);
                return;
            }

            if (CatalogService.LoadedFromCache)
            {
                SetStatus(BrowserText.StatusOffline(RelativeSnapshotTime()));
            }
            else
            {
                string relative = RelativeSnapshotTime();
                SetStatus(relative.Length == 0 ? "" : BrowserText.StatusUpdated(relative));
            }
        }

        private static string RelativeSnapshotTime()
        {
            DateTime? time = CatalogService.DocumentUpdatedAt ?? CatalogService.CacheFileTimeUtc;
            return time.HasValue ? BrowserText.RelativeTime(time.Value) : "";
        }

        // ---------------------------------------------------------------------
        // Install status + download progress plumbing
        // ---------------------------------------------------------------------

        /// <summary>
        /// Resolves a catalog entry against the cached local pack snapshot.
        /// Called by the details view (action bar), the cards and DownloadFlow.
        /// </summary>
        public InstallResolution ResolveEntry(TranslationEntry entry)
        {
            return InstallStatusResolver.Resolve(entry, _installedPacks);
        }

        /// <summary>Called by DownloadFlow when an install starts for this entry.</summary>
        public void OnDownloadStarted(TranslationEntry entry)
        {
            if (entry == null)
            {
                return;
            }
            RouteProgressToDetails(entry.id, new InstallProgress { Stage = "resolving", BytesRead = 0, TotalBytes = null });
        }

        /// <summary>Called by DownloadFlow for every throttled progress report.</summary>
        public void OnDownloadProgress(string entryId, InstallProgress progress)
        {
            RouteProgressToDetails(entryId, progress);
        }

        /// <summary>
        /// Shows the progress of <paramref name="entryId"/> in the details area,
        /// but only while that entry is the selected one - installing one
        /// translation must never block the details of another (item 2).
        /// </summary>
        private void RouteProgressToDetails(string entryId, InstallProgress progress)
        {
            if (_details == null || string.IsNullOrEmpty(entryId))
            {
                return;
            }

            TranslationEntry selected = State != null ? State.SelectedEntry : null;
            if (selected == null || !string.Equals(selected.id, entryId, StringComparison.Ordinal))
            {
                return;
            }

            _details.SetCancelHandler(delegate { DownloadFlow.CancelEntry(entryId); });
            _details.ShowDownloadProgress(entryId, progress);
        }

        /// <summary>Called by DownloadFlow when the install finished (any status).</summary>
        public void OnDownloadFinished(string entryId, InstallResult result)
        {
            if (_details != null)
            {
                _details.HideDownloadProgress(entryId);
            }
            RefreshInstalledPacks();
            State.Recompute();
            if (_details != null)
            {
                _details.SetEntry(State.SelectedEntry, ResolveEntry(State.SelectedEntry));
            }
        }

        // ---------------------------------------------------------------------
        // State -> view
        // ---------------------------------------------------------------------

        private void OnListChanged()
        {
            RebuildList();
            if (_countLabel != null)
            {
                _countLabel.text = BrowserText.Count(State.VisibleEntries.Count);
            }
            if (_sortButtonLabel != null)
            {
                _sortButtonLabel.text = BuildSortLabel();
            }
            if (_filterButtonLabel != null)
            {
                _filterButtonLabel.text = BuildFilterLabel();
            }
            UpdateStatusLabel();
        }

        private void RebuildList()
        {
            if (_listContent == null)
            {
                return;
            }

            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                if (_cards[i] != null && _cards[i].Root != null)
                {
                    _cards[i].Root.transform.SetParent(null, false);
                }
                _cards[i].Destroy();
            }
            _cards.Clear();

            // The old cards are destroyed above; reset the tour's card handle
            // so it never points at a destroyed card (which would make the
            // feature-indicator target resolve to a zero-size rect).
            _firstOnlineCard = null;

            IReadOnlyList<TranslationEntry> visible = State.VisibleEntries;
            if (visible.Count == 0)
            {
                UpdateStatusLabel();
                return;
            }

            foreach (TranslationEntry entry in visible)
            {
                TranslationCardView card = TranslationCardView.Create(_listContent.transform, entry, delegate (TranslationCardView c)
                {
                    State.SetSelected(c.Entry.id);
                }, false, _installedPacks);
                if (card != null)
                {
                    _cards.Add(card);
                    if (_firstOnlineCard == null)
                    {
                        _firstOnlineCard = card;
                    }
                }
            }
            UiFactory.ForceLayoutHeights(_listContent.transform);
            RefreshCardHighlights();
        }

        /// <summary>Re-reads installed languages into the Installed tab (after downloads).</summary>
        public void RefreshInstalledList()
        {
            RefreshInstalledPacks();
            BuildInstalledList();
            // Online card badges were computed from the previous pack snapshot.
            State.Recompute();
        }

        private void OnSelectionChanged()
        {
            if (_details != null)
            {
                _details.SetEntry(State.SelectedEntry, ResolveEntry(State.SelectedEntry));

                // Follow the install state of the newly selected entry: show its
                // progress bar while it is being installed, hide it otherwise
                // (per-entry progress, item 2).
                InstallProgress installing = State.SelectedEntry != null
                    ? State.GetInstallingProgress(State.SelectedEntry.id)
                    : null;
                if (installing != null)
                {
                    RouteProgressToDetails(State.SelectedEntry.id, installing);
                }
                else
                {
                    _details.HideDownloadProgress();
                }
            }
            RefreshCardHighlights();
        }

        private void OnInstallingChanged()
        {
            foreach (TranslationCardView card in _cards)
            {
                if (card == null || card.Root == null || card.Entry == null)
                {
                    continue;
                }
                InstallProgress progress = State.GetInstallingProgress(card.Entry.id);
                if (progress != null)
                {
                    card.SetInstallingProgress(progress);
                }
                else
                {
                    card.ClearInstallingProgress();
                }
            }
        }

        private void RefreshCardHighlights()
        {
            string selectedId = State.SelectedId;
            foreach (TranslationCardView card in _cards)
            {
                if (card == null || card.Root == null)
                {
                    continue;
                }
                bool selected = card.Entry != null && string.Equals(card.Entry.id, selectedId, StringComparison.Ordinal);
                card.SetSelected(selected);
                // Entries whose resolution did not identify a concrete pack get no
                // marker: there is nothing reliable to compare against.
                string packKey = card.Resolution != null && card.Resolution.Pack != null
                    ? card.Resolution.Pack.PackKey
                    : null;
                card.SetUsed(ActiveLanguageState.IsPackInUse(packKey));
            }
        }

        /// <summary>
        /// Refreshes everything that shows the "in use" state after a language
        /// switch: the online card outlines, the details action button and the
        /// Installed tab's fixed action bar.
        /// </summary>
        private void RefreshUsedMarkers()
        {
            RefreshCardHighlights();
            if (_details != null)
            {
                _details.RefreshActionArea();
            }
            RefreshInstalledActionBar();
        }

        /// <summary>Sets the Installed tab's fixed action bar to Use/Used.</summary>
        private void RefreshInstalledActionBar()
        {
            if (_installedActionBarLabel != null)
            {
                _installedActionBarLabel.text = ActiveLanguageState.IsPackInUse(_installedSelectedId)
                    ? BrowserText.ActionUsed
                    : BrowserText.ActionUse;
            }
        }

        // ---------------------------------------------------------------------
        // Installed tab (Full mode)
        // ---------------------------------------------------------------------

        private void CreateInstalledListPane(Transform parent)
        {
            // Flow area: title + list, mirroring the Online tab's left pane.
            GameObject layoutArea = new GameObject("LayoutArea", typeof(RectTransform));
            layoutArea.transform.SetParent(parent, false);
            UiFactory.StretchToParent(layoutArea);
            VerticalLayoutGroup layout = layoutArea.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = UiFactory.CreateLabel(layoutArea.transform, "Title", BrowserText.InstalledTitle, 16f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 24f, 0f, 24f, 1f, 0f);

            UiFactory.ScrollViewLayout scroll = UiFactory.CreateScrollView(layoutArea.transform, "InstalledList", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            UiFactory.AddLayoutElement(scroll.Root.gameObject, 0f, 100f, 10f, 150f, 1f, 1f);

            _installedListContent = scroll.Content.gameObject;
            VerticalLayoutGroup installedListLayout = scroll.Content.GetComponent<VerticalLayoutGroup>();
            if (installedListLayout != null)
            {
                installedListLayout.padding = new RectOffset(0, 16, 0, 0);
            }

            UiFactory.CreateScrollbar(scroll.Root, "InstalledScrollbar", scroll.ScrollRect);
            Transform installedSbTransform = scroll.Root.Find("InstalledScrollbar");
            RectTransform sbRect = installedSbTransform != null ? installedSbTransform.GetComponent<RectTransform>() : null;
            if (sbRect != null)
            {
                sbRect.anchorMin = new Vector2(1f, 0f);
                sbRect.anchorMax = new Vector2(1f, 1f);
                sbRect.pivot = new Vector2(1f, 0.5f);
                sbRect.anchoredPosition = new Vector2(-3f, 0f);
                sbRect.sizeDelta = new Vector2(10f, 0f);
            }

            // Footer: rescan the language folder so packs that were dropped into it
            // while the game is running appear without a restart.
            GameObject footer = UiFactory.CreatePanel(layoutArea.transform, "Footer", new Vector2(0f, 34f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            UiFactory.AddLayoutElement(footer, 0f, 34f, 0f, 34f, 1f, 0f);

            HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 8f;
            footerLayout.padding = new RectOffset(8, 6, 0, 0);
            footerLayout.childAlignment = TextAnchor.MiddleLeft;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            // Fixed-height row keeps the refresh button square (item 20).
            footerLayout.childForceExpandHeight = false;

            GameObject footerSpacer = new GameObject("FooterSpacer", typeof(RectTransform));
            footerSpacer.transform.SetParent(footer.transform, false);
            UiFactory.AddLayoutElement(footerSpacer, 0f, 0f, 0f, 0f, 1f, 0f);

            GameObject refreshButton = UiFactory.CreateButton(footer.transform, "RefreshButton", "",
                RefreshInstalledLanguages, new Vector2(26f, 26f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(refreshButton, 26f, 26f, 26f, 26f, 0f, 0f);
            Image refreshIcon = UiFactory.CreateImage(refreshButton.transform, "Icon", new Vector2(16f, 16f), Color.white, PrimitiveSprites.Kind.Refresh);
            RectTransform refreshIconRect = refreshIcon.GetComponent<RectTransform>();
            refreshIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            refreshIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            refreshIconRect.pivot = new Vector2(0.5f, 0.5f);
            refreshIconRect.anchoredPosition = Vector2.zero;
            refreshIconRect.sizeDelta = new Vector2(16f, 16f);
            TooltipService.Attach(refreshButton, BrowserText.InstalledRescan);

            BuildInstalledList();
        }

        /// <summary>
        /// Rereads the language folders on disk and rebuilds the lists from them, so a
        /// translation that was added or removed while the game is running shows up
        /// without a restart.
        /// </summary>
        private void RefreshInstalledLanguages()
        {
            LanguageManager.LoadLanguages();
            RefreshInstalledPacks();
            BuildInstalledList();

            // The selection may point at a pack that no longer exists on disk; the
            // details view follows the selection.
            if (!string.IsNullOrEmpty(_installedSelectedId) && FindInstalledEntry(_installedSelectedId) == null)
            {
                _installedSelectedId = null;
                if (_installedDetails != null)
                {
                    _installedDetails.SetEntry(null);
                }
            }

            // The online cards compute their installed badges when they are built, so
            // the list is rebuilt to follow the new state of the disk.
            State.Recompute();
        }

        /// <summary>Rebuilds the Installed tab from the languages loaded locally.</summary>
        private void BuildInstalledList()
        {
            if (_installedListContent == null)
            {
                return;
            }

            try
            {
                for (int i = _installedCards.Count - 1; i >= 0; i--)
            {
                if (_installedCards[i] != null)
                {
                    if (_installedCards[i].Root != null)
                    {
                        _installedCards[i].Root.transform.SetParent(null, false);
                    }
                    _installedCards[i].Destroy();
                }
            }
            _installedCards.Clear();

            // Clean up any non-card children (e.g. the previous empty-state label).
            for (int i = _installedListContent.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(_installedListContent.transform.GetChild(i).gameObject);
            }

            // Product tour override: show the dummy installed entries instead of
            // the real local languages so the tour has consistent example data.
            if (_installedEntriesOverride != null)
            {
                foreach (TranslationEntry entry in _installedEntriesOverride)
                {
                    TranslationEntry captured = entry;
                    TranslationCardView card = TranslationCardView.Create(_installedListContent.transform, entry,
                        delegate { SelectInstalled(captured.id); }, true);
                    if (card != null)
                    {
                        card.SetSelected(string.Equals(captured.id, _installedSelectedId, StringComparison.OrdinalIgnoreCase));
                        _installedCards.Add(card);
                    }
                }

                // Keep a selection so the installed details view shows tour data.
                if (_installedCards.Count > 0 && string.IsNullOrEmpty(_installedSelectedId))
                {
                    SelectInstalled(_installedCards[0].Entry.id);
                }

                UiFactory.ForceLayoutHeights(_installedListContent.transform);
                RefreshInstalledActionBar();
                return;
            }

            if (LanguageManager.allLanguages == null || LanguageManager.allLanguages.Count == 0)
            {
                TextMeshProUGUI empty = UiFactory.CreateLabel(_installedListContent.transform, "Empty", BrowserText.InstalledEmpty, 13f, StylePalette.TextDim, TextAlignmentOptions.Center, true);
                UiFactory.AddLayoutElement(empty.gameObject, 0f, 24f, 0f, 24f, 1f, 0f);
                _installedSelectedId = null;
                if (_installedDetails != null)
                {
                    _installedDetails.SetEntry(null);
                }
                RefreshInstalledActionBar();
                return;
            }

            // Manifest data by pack key, so the display name can prefer the name the
            // catalog install recorded over the pack's own metadata.
            Dictionary<string, InstalledPackInfo> packsByKey = new Dictionary<string, InstalledPackInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (InstalledPackInfo pack in _installedPacks)
            {
                if (pack != null && !string.IsNullOrEmpty(pack.PackKey))
                {
                    packsByKey[pack.PackKey] = pack;
                }
            }

            foreach (KeyValuePair<string, JsonFormat> pair in LanguageManager.allLanguages)
            {
                InstalledPackInfo info;
                packsByKey.TryGetValue(pair.Key, out info);
                if (info == null && pair.Value != null && pair.Value.RootFolder == null)
                {
                    // The built-in English has no folder on disk; its identity
                    // comes from the in-memory manifest (source "builtin").
                    info = new InstalledPackInfo { PackKey = pair.Key, Manifest = LanguageManager.BuiltinEnglishManifest };
                }
                TranslationEntry entry = BuildInstalledEntry(pair.Key, pair.Value, info);
                string capturedId = entry.id;

                TranslationCardView card = TranslationCardView.Create(_installedListContent.transform, entry,
                    delegate { SelectInstalled(capturedId); }, true);
                if (card != null)
                {
                    card.SetSelected(string.Equals(capturedId, _installedSelectedId, StringComparison.OrdinalIgnoreCase));
                    card.SetUsed(ActiveLanguageState.IsPackInUse(capturedId));
                    _installedCards.Add(card);
                }
            }

            UiFactory.ForceLayoutHeights(_installedListContent.transform);
            RefreshInstalledActionBar();
            }
            catch (Exception e)
            {
                Logging.Warn("[BrowserView] BuildInstalledList failed: " + e.Message);
            }
        }

        /// <summary>
        /// Maps a locally installed language to a card/detail entry. Identity and
        /// display data (name, language code, authors, version, description,
        /// Discord link) come from the install manifest first; the pack's own
        /// metadata is only a fallback for a broken or missing manifest. The
        /// folder name is the last resort for the display name.
        /// </summary>
        private static TranslationEntry BuildInstalledEntry(string packKey, JsonFormat lang, InstalledPackInfo info)
        {
            Metadata md = lang != null ? lang.metadata : null;
            TranslationManifest manifest = info != null ? info.Manifest : null;

            // The built-in English has no folder on disk: it is the original
            // game text, not a user translation, so no translation metadata
            // (version, authors, features) may be shown for it (item 10.2).
            bool isBuiltinEnglish = lang != null && lang.RootFolder == null;

            string languageCode = manifest != null ? TrimToNull(manifest.languageCode) : null;
            if (languageCode == null && md != null)
            {
                languageCode = TrimToNull(md.languageCode);
            }

            // Manifest-first display name; a name that is empty or just the
            // language code counts as missing (broken metadata).
            string display = isBuiltinEnglish ? null : ReadManifestName(manifest);
            if (isBuiltinEnglish && md != null)
            {
                // The canonical built-in English title ("English (DEFAULT)")
                // lives in the metadata; the in-memory manifest only carries
                // a short "English".
                display = TrimToNull(md.languageName);
            }
            if (display == null && md != null)
            {
                display = TrimToNull(md.languageName);
                if (display != null && languageCode != null &&
                    string.Equals(display, languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    display = null;
                }
            }
            if (display == null)
            {
                display = packKey;
            }

            int? likes = manifest?.likes;
            // Prefer the live like count from the online catalog so the number
            // stays up to date instead of being frozen at install time.
            if (!isBuiltinEnglish && manifest != null && !string.IsNullOrEmpty(manifest.translationId))
            {
                var online = CatalogService.FindById(manifest.translationId);
                if (online != null)
                {
                    likes = online.likes;
                }
            }

            TranslationFeatures features = null;
            List<KeyValuePair<string, string>> manifestSocialLinks = null;
            if (!isBuiltinEnglish)
            {
                features = new TranslationFeatures();
                if (md != null && md.fonts != null && !string.IsNullOrEmpty(md.fonts.MainFont))
                {
                    features.fonts = FeatureStatus.Full;
                }
                if (manifest != null)
                {
                    var manifestFeatures = TranslationManifest.FeaturesFromJson(manifest.featuresJson);
                    if (manifestFeatures != null)
                    {
                        features = manifestFeatures;
                    }
                    manifestSocialLinks = TranslationManifest.SocialLinksFromJson(manifest.socialLinksJson);
                }
            }

            // Author line: manifest first, then the dedicated authors key, then
            // the credits block (a free-form fallback for packs that only fill
            // that field). The built-in English shows no authors.
            string author = isBuiltinEnglish ? null : (manifest != null ? TrimToNull(manifest.authors) : null);
            if (author == null && !isBuiltinEnglish && md != null)
            {
                author = !string.IsNullOrEmpty(md.langAuthors) ? md.langAuthors : md.credits;
            }

            // The built-in English has no translation version: the template's
            // number is the mod data version, not a translation release.
            string version = isBuiltinEnglish ? null : (manifest != null ? TrimToNull(manifest.version) : null);
            if (version == null && !isBuiltinEnglish && md != null)
            {
                version = TrimToNull(md.translationVersion);
            }

            // Local image paths from the install manifest, resolved against the pack
            // folder so the Installed tab can show them without re-downloading.
            string logoUrl = null;
            string flagUrl = null;
            List<string> gallery = null;
            if (info != null && manifest != null && !string.IsNullOrEmpty(info.Folder))
            {
                if (!string.IsNullOrEmpty(manifest.logoPath))
                {
                    logoUrl = Path.Combine(info.Folder, manifest.logoPath);
                }
                if (!string.IsNullOrEmpty(manifest.flagPath))
                {
                    flagUrl = Path.Combine(info.Folder, manifest.flagPath);
                }
                if (manifest.galleryPaths != null && manifest.galleryPaths.Count > 0)
                {
                    gallery = new List<string>();
                    foreach (string p in manifest.galleryPaths)
                    {
                        gallery.Add(Path.Combine(info.Folder, p));
                    }
                }
            }

            return new TranslationEntry
            {
                id = packKey,
                name = display,
                languageTag = languageCode,
                author = author,
                version = version,
                features = features,
                likes = likes,
                socialLinks = manifestSocialLinks,
                readme = manifest != null ? TrimToNull(manifest.description) : null,
                discordUrl = manifest != null ? TrimToNull(manifest.discordUrl) : null,
                logoUrl = logoUrl,
                flagUrl = flagUrl,
                gallery = gallery,
                isBuiltinEnglish = isBuiltinEnglish
            };
        }

        /// <summary>
        /// Manifest display name, treating empty and language-code-equal names
        /// as missing so broken metadata never shows a code as the title.
        /// </summary>
        private static string ReadManifestName(TranslationManifest manifest)
        {
            if (manifest == null)
            {
                return null;
            }
            string name = TrimToNull(manifest.name);
            if (name == null ||
                string.Equals(name, manifest.languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return name;
        }

        private static string TrimToNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return value.Trim();
        }

        private void SelectInstalled(string packKey)
        {
            _installedSelectedId = packKey;
            foreach (TranslationCardView card in _installedCards)
            {
                if (card == null || card.Root == null)
                {
                    continue;
                }
                bool selected = card.Entry != null && string.Equals(card.Entry.id, packKey, StringComparison.OrdinalIgnoreCase);
                card.SetSelected(selected);
                card.SetUsed(ActiveLanguageState.IsPackInUse(card.Entry != null ? card.Entry.id : null));
            }

            if (_installedDetails != null)
            {
                _installedDetails.SetEntry(_installedEntriesOverride != null
                    ? FindOverrideInstalledEntry(packKey)
                    : FindInstalledEntry(packKey));
            }
            RefreshInstalledActionBar();
        }

        /// <summary>Finds an entry in the product tour's Installed-tab override.</summary>
        private TranslationEntry FindOverrideInstalledEntry(string packKey)
        {
            if (_installedEntriesOverride == null || string.IsNullOrEmpty(packKey))
            {
                return null;
            }
            foreach (TranslationEntry entry in _installedEntriesOverride)
            {
                if (entry != null && string.Equals(entry.id, packKey, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            return null;
        }

        private TranslationEntry FindInstalledEntry(string packKey)
        {
            if (LanguageManager.allLanguages == null || string.IsNullOrEmpty(packKey))
            {
                return null;
            }

            foreach (KeyValuePair<string, JsonFormat> pair in LanguageManager.allLanguages)
            {
                if (string.Equals(pair.Key, packKey, StringComparison.OrdinalIgnoreCase))
                {
                    InstalledPackInfo info = null;
                    foreach (InstalledPackInfo candidate in _installedPacks)
                    {
                        if (candidate != null && string.Equals(candidate.PackKey, packKey, StringComparison.OrdinalIgnoreCase))
                        {
                            info = candidate;
                            break;
                        }
                    }
                    if (info == null && pair.Value != null && pair.Value.RootFolder == null)
                    {
                        info = new InstalledPackInfo { PackKey = packKey, Manifest = LanguageManager.BuiltinEnglishManifest };
                    }
                    return BuildInstalledEntry(pair.Key, pair.Value, info);
                }
            }
            return null;
        }

        private void UseSelectedInstalled()
        {
            if (string.IsNullOrEmpty(_installedSelectedId))
            {
                return;
            }
            LanguageManager.SetCurrentLanguage(_installedSelectedId);
            BuildInstalledList();
            SelectInstalled(_installedSelectedId);
            // The switch also changes the online tab's "in use" markers and its
            // details action button; refresh them together with the lists above.
            RefreshUsedMarkers();
        }

        private static void AddNativeToggle(Transform parent, string name, string label, bool initial, Action<bool> onChange)
        {
            if (GameUiAssets.ToggleTemplate == null)
            {
                CreateNativeToggleRow(parent, name, label, initial, onChange);
                return;
            }

            GameObject toggleObj = UnityEngine.Object.Instantiate(GameUiAssets.ToggleTemplate, parent);
            toggleObj.name = name;
            toggleObj.SetActive(true);

            // Strip any tooltip behavior the game's toggle prefab carries: the
            // settings rows must not show the game's hover popups (item 24).
            StripGameTooltips(toggleObj.transform);

            TextMeshProUGUI text = toggleObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = label;
            }

            Toggle toggle = toggleObj.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = initial;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(delegate (bool v) { onChange(v); });
            }

            UiFactory.AddLayoutElement(toggleObj, 0f, 40f, 0f, 40f, 1f, 0f);
        }

        /// <summary>
        /// Removes every component with a tooltip-like name from a hierarchy
        /// cloned out of the game's UI (the settings rows must not show the
        /// game's own hover popups, item 24).
        /// </summary>
        private static void StripGameTooltips(Transform root)
        {
            try
            {
                Behaviour[] behaviours = root.GetComponentsInChildren<Behaviour>(true);
                foreach (Behaviour behaviour in behaviours)
                {
                    if (behaviour != null &&
                        behaviour.GetType().Name.IndexOf("Tooltip", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        UnityEngine.Object.Destroy(behaviour);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[Browser] Could not strip the game tooltips: " + e.Message);
            }
        }

        /// <summary>
        /// Builds a settings toggle row from scratch in the ULTRAKILL style (rounded
        /// panel + a check box on the right) using the game's native sprites when
        /// available. Used when the game's own toggle prefab could not be extracted.
        /// </summary>
        private static void CreateNativeToggleRow(Transform parent, string name, string label, bool initial, Action<bool> onChange)
        {
            bool state = initial;

            GameObject row = UiFactory.CreatePanel(parent, name, new Vector2(0f, 44f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            UiFactory.AddLayoutElement(row, 0f, 44f, 0f, 44f, 1f, 0f);

            TextMeshProUGUI text = UiFactory.CreateLabel(row.transform, "Label", label, 15f, StylePalette.TextPrimary, TextAlignmentOptions.MidlineLeft, true);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.offsetMin = new Vector2(12f, 0f);
            textRect.offsetMax = new Vector2(-52f, 0f);

            GameObject box = UiFactory.CreatePanel(row.transform, "Box", new Vector2(30f, 30f), StylePalette.PanelAlt, StylePalette.BorderBlue, 1f);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(1f, 0.5f);
            boxRect.anchorMax = new Vector2(1f, 0.5f);
            boxRect.pivot = new Vector2(1f, 0.5f);
            boxRect.anchoredPosition = new Vector2(-10f, 0f);
            boxRect.sizeDelta = new Vector2(30f, 30f);

            Image check = UiFactory.CreateImage(box.transform, "Check", new Vector2(20f, 20f), StylePalette.AccentCyan);
            if (NativeUi.Check != null)
            {
                check.sprite = NativeUi.Check;
            }
            check.preserveAspect = true;
            RectTransform checkRect = check.rectTransform;
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.pivot = new Vector2(0.5f, 0.5f);
            checkRect.anchoredPosition = Vector2.zero;
            checkRect.sizeDelta = new Vector2(20f, 20f);
            check.enabled = state;

            Image background = row.GetComponent<Image>();
            background.raycastTarget = true;

            Button button = row.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = UiFactory.ButtonTint;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(delegate
            {
                state = !state;
                check.enabled = state;
                if (onChange != null)
                {
                    onChange(state);
                }
            });
        }

        private static bool ReadConfigBool(string key, bool defaultValue)
        {
            return Convert.ToBoolean(LanguageManager.configFile.Bind("General", key, defaultValue ? "True" : "False").Value);
        }

        private static void WriteConfigBool(string key, bool value)
        {
            LanguageManager.configFile.Bind("General", key, "True").Value = value.ToString();
        }

        /// <summary>True while a product tour is running.</summary>
        public bool IsTourActive { get { return _tour != null; } }

        /// <summary>Skips the running product tour (e.g. on Escape).</summary>
        public void SkipTour()
        {
            if (_tour != null)
            {
                _tour.Skip();
            }
        }

        // ---------------------------------------------------------------------
        // Teardown
        // ---------------------------------------------------------------------

        public void Destroy()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
            }
        }

        /// <summary>Closes the browser on Escape key (5.8 convenience). During a product tour, Escape skips the tour instead.</summary>
        private sealed class EscapeCloser : MonoBehaviour
        {
            public UnityEngine.Events.UnityAction onClose;
            private BrowserView _browser;
            public void SetBrowser(BrowserView browser) { _browser = browser; }
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_browser != null && _browser.IsTourActive)
                    {
                        _browser.SkipTour();
                        return;
                    }
                    if (onClose != null) onClose.Invoke();
                }
            }
        }
    }
}
