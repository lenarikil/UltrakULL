using System;
using System.Collections.Generic;
using BepInEx;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>How the browser lays itself out inside its host container.</summary>
    public enum BrowserLayoutMode
    {
        /// <summary>Three side-by-side panels on large screens.</summary>
        Full,

        /// <summary>Single-column stack: list -> details with a back button (fallback Options page, ~600x800).</summary>
        Compact
    }

    /// <summary>
    /// Online translation browser embedded in the options menu.
    ///
    /// Full mode:
    ///   left ~27% — search, sort, list, refresh + count
    ///   center ~54% — details
    ///   right ~19% — legend
    ///
    /// Compact mode (fallback Options page):
    ///   toolbar (search/sort/refresh/legend) + main area that switches between
    ///   the card list and the details view, footer with the translation count.
    ///
    /// Selection never downloads; only the download button does.
    /// </summary>
    public sealed class BrowserView
    {
        public GameObject Root;
        public BrowserState State = new BrowserState();

        private readonly BrowserLayoutMode _mode;
        private readonly bool _showClose = true;

        private GameObject _onlineTab;
        private GameObject _installedTab;
        private GameObject _settingsTab;
        private GameObject _onlinePage;
        private GameObject _installedPage;
        private GameObject _settingsPage;
        private GameObject _onlinePane;

        private GameObject _listContent;
        private UiFactory.ScrollViewLayout _listScroll;
        private GameObject _installedListContent;
        private readonly List<TranslationCardView> _cards = new List<TranslationCardView>();
        private readonly List<TranslationCardView> _installedCards = new List<TranslationCardView>();
        private string _installedSelectedId;
        private TranslationDetailsView _installedDetails;
        private TranslationDetailsView _details;
        private GameObject _downloadBar;
        private TextMeshProUGUI _countLabel;
        private TextMeshProUGUI _sortButtonLabel;
        private TextMeshProUGUI _statusLabel;
        private UnityEngine.Events.UnityAction _onClose;

        // Compact-mode containers
        private GameObject _compactListArea;
        private GameObject _compactDetailsArea;
        private GameObject _compactLegendArea;

        // ---------------------------------------------------------------------
        // Construction
        // ---------------------------------------------------------------------

        public static BrowserView Create(Transform parent, string name, UnityEngine.Events.UnityAction onClose, BrowserLayoutMode mode = BrowserLayoutMode.Full, bool showClose = true)
        {
            BrowserView browser = new BrowserView(mode, showClose);
            browser._onClose = onClose;

            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiFactory.StretchToParent(root);
            browser.Root = root;

            browser.CreateHeader(root.transform);
            browser.CreateBody(root.transform);
            UiFactory.ForceLayoutHeights(root.transform);

            browser.State.ListChanged += browser.OnListChanged;
            browser.State.SelectionChanged += browser.OnSelectionChanged;
            browser.State.RefreshRequested += delegate { browser.StartLoad(true); };
            browser.State.DownloadRequested += delegate (TranslationEntry entry)
            {
                Canvas canvas = browser.Root != null ? browser.Root.GetComponentInParent<Canvas>() : null;
                DownloadFlow.ConfirmDownload(canvas, entry);
            };

            return browser;
        }

        private BrowserView(BrowserLayoutMode mode, bool showClose)
        {
            _mode = mode;
            _showClose = showClose;
        }

        public void StartLoad(bool force = false)
        {
            SetStatus("Loading translations...");
            LoadAsync(force);
        }

        private async void LoadAsync(bool force)
        {
            bool ok = await CatalogService.LoadAsync(force);
            if (!ok)
            {
                SetStatus(string.IsNullOrEmpty(CatalogService.LastError)
                    ? "Failed to load the translation list."
                    : "Failed to load: " + CatalogService.LastError);
            }
            State.Recompute();
            State.SelectFirst();
        }

        // ---------------------------------------------------------------------
        // Header
        // ---------------------------------------------------------------------

        private void CreateHeader(Transform parent)
        {
            if (_mode == BrowserLayoutMode.Compact)
            {
                CreateCompactHeader(parent);
            }
            else
            {
                CreateFullHeader(parent);
            }
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

            TextMeshProUGUI title = UiFactory.CreateLabel(titleColumn.transform, "Title", "Online translation browser", 16f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(title);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 20f, 230f, 20f, 0f, 0f);
            TextMeshProUGUI subtitle = UiFactory.CreateLabel(titleColumn.transform, "Subtitle", "Preview a translation before downloading it.", 11f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(subtitle);
            UiFactory.AddLayoutElement(subtitle.gameObject, 0f, 16f, 230f, 16f, 0f, 0f);

            // Tab spacer
            GameObject tabSpacer = new GameObject("TabSpacer", typeof(RectTransform));
            tabSpacer.transform.SetParent(header.transform, false);
            UiFactory.AddLayoutElement(tabSpacer, 12f, 0f, 12f, 0f, 0f, 0f);

            // Tabs: Online / Installed / Settings
            _onlineTab = CreateTab(header.transform, "TabOnline", "Online", true, delegate { SwitchTab(0); });
            _installedTab = CreateTab(header.transform, "TabInstalled", "Installed", false, delegate { SwitchTab(1); });
            _settingsTab = CreateTab(header.transform, "TabSettings", "Settings", false, delegate { SwitchTab(2); });

            // Push the close button to the right
            GameObject headerSpacer = new GameObject("HeaderSpacer", typeof(RectTransform));
            headerSpacer.transform.SetParent(header.transform, false);
            UiFactory.AddLayoutElement(headerSpacer, 0f, 0f, 0f, 0f, 1f, 0f);

            if (_showClose)
            {
                CreateCloseButton(header.transform, 30f);
            }
        }

        private void CreateCompactHeader(Transform parent)
        {
            GameObject header = UiFactory.CreatePanel(parent, "Header", new Vector2(0f, 40f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 40f);

            HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 8f;
            headerLayout.padding = new RectOffset(8, 8, 0, 0);
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            GameObject logoMark = UiFactory.CreatePanel(header.transform, "LogoMark", new Vector2(26f, 26f), StylePalette.AccentRedDark, StylePalette.AccentRed, 1f);
            UiFactory.AddLayoutElement(logoMark, 26f, 26f, 26f, 26f, 0f, 0f);
            TextMeshProUGUI logoText = UiFactory.CreateLabel(logoMark.transform, "U", "U", 14f, Color.white, TextAlignmentOptions.Center, false);
            UiFactory.StretchToParent(logoText);

            TextMeshProUGUI title = UiFactory.CreateLabel(header.transform, "Title", "Online translations", 15f, StylePalette.TextPrimary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.SingleLine(title);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 34f, 160f, 34f, 1f, 0f);

            GameObject headerSpacer = new GameObject("HeaderSpacer", typeof(RectTransform));
            headerSpacer.transform.SetParent(header.transform, false);
            UiFactory.AddLayoutElement(headerSpacer, 0f, 0f, 0f, 0f, 1f, 0f);

            if (_showClose)
            {
                CreateCloseButton(header.transform, 26f);
            }
        }

        private void CreateCloseButton(Transform parent, float size)
        {
            GameObject closeButton = UiFactory.CreateButton(parent, "CloseButton", "",
                delegate { if (_onClose != null) _onClose(); },
                new Vector2(size, size),
                StylePalette.PanelAlt,
                StylePalette.AccentRed,
                StylePalette.AccentRedDark);
            UiFactory.AddLayoutElement(closeButton, size, size, size, size, 0f, 0f);
            Image closeIcon = UiFactory.CreateImage(closeButton.transform, "Icon", new Vector2(size * 0.45f, size * 0.45f), StylePalette.AccentRed, PrimitiveSprites.Kind.Close);
            RectTransform closeIconRect = closeIcon.GetComponent<RectTransform>();
            closeIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            closeIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            closeIconRect.pivot = new Vector2(0.5f, 0.5f);
            closeIconRect.anchoredPosition = Vector2.zero;
            closeIconRect.sizeDelta = new Vector2(size * 0.45f, size * 0.45f);
            TooltipService.Attach(closeButton, "Close the translation browser");
        }

        private GameObject CreateTab(Transform parent, string name, string text, bool active, Action onClick)
        {
            Color bg = active ? new Color(0.45f, 0.08f, 0.08f, 1f) : StylePalette.PanelAlt;
            Color border = active ? StylePalette.AccentRed : StylePalette.BorderBlue;
            Color textColor = active ? Color.white : StylePalette.TextSecondary;

            GameObject tab = UiFactory.CreateButton(parent, name, text, onClick, new Vector2(100f, 30f), bg, border, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(tab, 100f, 30f, 100f, 30f, 0f, 0f);
            TextMeshProUGUI label = tab.transform.Find("Label").GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
                label.color = textColor;
                label.alignment = TextAlignmentOptions.Center;
            }
            return tab;
        }

        private void SwitchTab(int index)
        {
            if (_mode != BrowserLayoutMode.Full)
            {
                return;
            }

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

        private void SetTabStyle(GameObject tab, bool active)
        {
            if (tab == null)
            {
                return;
            }
            UiFactory.SetBorderColor(tab, active ? StylePalette.AccentRed : StylePalette.BorderBlue);
            Image bg = tab.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = active ? new Color(0.45f, 0.08f, 0.08f, 1f) : StylePalette.PanelAlt;
            }
            TextMeshProUGUI label = tab.transform.Find("Label").GetComponent<TextMeshProUGUI>();
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
            if (_mode == BrowserLayoutMode.Compact)
            {
                CreateCompactBody(parent);
            }
            else
            {
                CreateFullBody(parent);
            }
        }

        private void CreateFullBody(Transform parent)
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
            // The action bar is a child of the details root so the details layout group
            // reserves a fixed row for it and the scroll area takes the space above it
            // (instead of the scroll area covering an overlapping sibling button).
            _downloadBar = _details.CreateDownloadBar(_details.Root.transform);

            GameObject legendPane = LegendPanel.Create(page, "LegendPane", 0f);
            UiFactory.AddLayoutElement(legendPane, 200f, 0f, 240f, 0f, 0.55f, 1f);
        }

        // ---- Installed tab: same layout as Online, but "Use" instead of download ----
        private void BuildInstalledTab(Transform page)
        {
            AddThreeColumnLayout(page.gameObject);

            GameObject listPane = UiFactory.CreatePanel(page, "InstalledListPane", Vector2.zero, new Color(0f, 0f, 0f, 0f), null);
            UiFactory.AddLayoutElement(listPane, 300f, 0f, 340f, 0f, 0.72f, 1f);
            CreateInstalledListPane(listPane.transform);

            GameObject centerPane = UiFactory.CreatePanel(page, "InstalledCenterPane", Vector2.zero, new Color(0f, 0f, 0f, 0f), null);
            UiFactory.AddLayoutElement(centerPane, 0f, 0f, 500f, 100f, 3f, 1f);

            _installedDetails = TranslationDetailsView.Create(centerPane.transform, State);
            _installedDetails.CreateActionBar(_installedDetails.Root.transform, "Use", PrimitiveSprites.Kind.Check, UseSelectedInstalled);

            // Same three-column layout as the Online tab, legend included.
            GameObject legendPane = LegendPanel.Create(page, "InstalledLegendPane", 0f);
            UiFactory.AddLayoutElement(legendPane, 200f, 0f, 240f, 0f, 0.55f, 1f);
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

            TextMeshProUGUI title = UiFactory.CreateLabel(page.transform, "Title", "Settings", 20f, StylePalette.TextPrimary, TextAlignmentOptions.Center, false);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 30f, 0f, 30f, 1f, 0f);

            AddNativeToggle(page.transform, "activeDubbing", "Active dubbing (replace dialogue with dubbed audio)",
                ReadConfigBool("activeDubbing"), delegate (bool v) { WriteConfigBool("activeDubbing", v); });
            AddNativeToggle(page.transform, "extraDubbing", "Extra dubbing (optional voiced lines)",
                ReadConfigBool("extraDubbing"), delegate (bool v) { WriteConfigBool("extraDubbing", v); });
            AddNativeToggle(page.transform, "bookAudioDubbing", "Book audio dubbing (books and terminals)",
                ReadConfigBool("bookAudioDubbing"), delegate (bool v) { WriteConfigBool("bookAudioDubbing", v); });

            GameObject openFolder = UiFactory.CreateNativeButton(page.transform, "OpenFolder", "Open translations folder",
                delegate { Application.OpenURL(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "ultrakull")); });
            UiFactory.AddLayoutElement(openFolder, 0f, 44f, 0f, 44f, 1f, 0f);

            UiFactory.ForceLayoutHeights(page.transform);
        }

        private void CreateCompactBody(Transform parent)
        {
            GameObject body = new GameObject("Body", typeof(RectTransform));
            body.transform.SetParent(parent, false);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.offsetMin = new Vector2(0f, 0f);
            bodyRect.offsetMax = new Vector2(0f, -42f); // below the compact header

            VerticalLayoutGroup bodyLayout = body.AddComponent<VerticalLayoutGroup>();
            bodyLayout.spacing = 6f;
            bodyLayout.padding = new RectOffset(6, 6, 6, 6);
            bodyLayout.childAlignment = TextAnchor.UpperCenter;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = false;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            CreateCompactToolbar(body.transform);

            // Main area: switches between list, details and legend.
            GameObject mainArea = new GameObject("MainArea", typeof(RectTransform));
            mainArea.transform.SetParent(body.transform, false);
            UiFactory.AddLayoutElement(mainArea, 0f, 100f, 10f, 150f, 1f, 1f);

            // List area
            _compactListArea = new GameObject("ListArea", typeof(RectTransform));
            _compactListArea.transform.SetParent(mainArea.transform, false);
            UiFactory.StretchToParent(_compactListArea);
            CreateCompactListArea(_compactListArea.transform);

            // Details area (hidden until a card is selected)
            _compactDetailsArea = new GameObject("DetailsArea", typeof(RectTransform));
            _compactDetailsArea.transform.SetParent(mainArea.transform, false);
            UiFactory.StretchToParent(_compactDetailsArea);
            _compactDetailsArea.SetActive(false);
            CreateCompactDetailsArea(_compactDetailsArea.transform);

            // Legend area (hidden; toggled from the toolbar)
            _compactLegendArea = new GameObject("LegendArea", typeof(RectTransform));
            _compactLegendArea.transform.SetParent(mainArea.transform, false);
            UiFactory.StretchToParent(_compactLegendArea);
            _compactLegendArea.SetActive(false);
            CreateCompactLegendArea(_compactLegendArea.transform);

            // Footer: count
            GameObject footer = UiFactory.CreatePanel(body.transform, "Footer", new Vector2(0f, 30f), StylePalette.Panel, StylePalette.BorderDim, 1f);
            UiFactory.AddLayoutElement(footer, 0f, 30f, 0f, 30f, 1f, 0f);
            HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();
            footerLayout.padding = new RectOffset(8, 8, 0, 0);
            footerLayout.childAlignment = TextAnchor.MiddleLeft;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childForceExpandHeight = true;
            _countLabel = UiFactory.CreateLabel(footer.transform, "Count", "0 translations", 12f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.AddLayoutElement(_countLabel.gameObject, 90f, 28f, 120f, 28f, 1f, 0f);
        }

        private void CreateCompactToolbar(Transform parent)
        {
            GameObject toolbar = UiFactory.CreatePanel(parent, "Toolbar", new Vector2(0f, 36f), StylePalette.PanelAlt, StylePalette.BorderDim, 1f);
            UiFactory.AddLayoutElement(toolbar, 0f, 36f, 0f, 36f, 1f, 0f);

            HorizontalLayoutGroup toolbarLayout = toolbar.AddComponent<HorizontalLayoutGroup>();
            toolbarLayout.spacing = 6f;
            toolbarLayout.padding = new RectOffset(6, 6, 0, 0);
            toolbarLayout.childAlignment = TextAnchor.MiddleLeft;
            toolbarLayout.childControlWidth = true;
            toolbarLayout.childControlHeight = true;
            toolbarLayout.childForceExpandWidth = false;
            toolbarLayout.childForceExpandHeight = true;

            TMP_InputField search = UiFactory.CreateInputField(toolbar.transform, "SearchField", new Vector2(0f, 32f), "Search language, team or author", "");
            UiFactory.AddLayoutElement(search.gameObject, 0f, 32f, 0f, 32f, 1f, 0f);
            search.onValueChanged.AddListener(delegate (string value) { State.SetSearch(value); });

            GameObject sortButton = UiFactory.CreateNativeButton(toolbar.transform, "SortButton", BuildSortLabel(), State.CycleSortMode);
            UiFactory.AddLayoutElement(sortButton, 96f, 30f, 96f, 30f, 0f, 0f);
            _sortButtonLabel = sortButton.GetComponentInChildren<TextMeshProUGUI>();

            GameObject refreshButton = UiFactory.CreateNativeButton(toolbar.transform, "RefreshButton", "", State.RequestRefresh);
            UiFactory.AddLayoutElement(refreshButton, 34f, 30f, 34f, 30f, 0f, 0f);
            Image refreshIcon = UiFactory.CreateImage(refreshButton.transform, "Icon", new Vector2(16f, 16f), Color.white, PrimitiveSprites.Kind.Refresh);
            RectTransform refreshIconRect = refreshIcon.GetComponent<RectTransform>();
            refreshIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            refreshIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            refreshIconRect.pivot = new Vector2(0.5f, 0.5f);
            refreshIconRect.anchoredPosition = Vector2.zero;
            refreshIconRect.sizeDelta = new Vector2(16f, 16f);
            TooltipService.Attach(refreshButton, "Reload the translation list");

            GameObject legendButton = UiFactory.CreateNativeButton(toolbar.transform, "LegendButton", "?", ToggleCompactLegend);
            UiFactory.AddLayoutElement(legendButton, 34f, 30f, 34f, 30f, 0f, 0f);
            TooltipService.Attach(legendButton, "Show the interface legend");
        }

        private void CreateCompactListArea(Transform parent)
        {
            // Scrollable list fills the area; scrollbar sits in an overlay layer.
            _listScroll = UiFactory.CreateScrollView(parent, "ListScroll", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            RectTransform listRect = _listScroll.Root;
            listRect.anchorMin = new Vector2(0f, 0f);
            listRect.anchorMax = new Vector2(1f, 1f);
            listRect.offsetMin = new Vector2(0f, 0f);
            listRect.offsetMax = new Vector2(0f, 0f);
            UiFactory.AddLayoutElement(_listScroll.Root.gameObject, 0f, 100f, 10f, 150f, 1f, 1f);

            _listContent = _listScroll.Content.gameObject;
            _listContent.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 16, 0, 0);

            UiFactory.CreateScrollbar(parent, "ListScrollbar", _listScroll.ScrollRect);
            RectTransform sbRect = parent.Find("ListScrollbar").GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-3f, 0f);
            sbRect.sizeDelta = new Vector2(10f, 0f);

            _statusLabel = UiFactory.CreateLabel(parent, "StatusLabel", "", 12f, StylePalette.TextSecondary, TextAlignmentOptions.Left, true);
            RectTransform statusRect = _statusLabel.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(1f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 2f);
            statusRect.sizeDelta = new Vector2(0f, 20f);
            UiFactory.AddLayoutElement(_statusLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
        }

        private void CreateCompactDetailsArea(Transform parent)
        {
            // Back button at the top-left (native style).
            GameObject backButton = UiFactory.CreateNativeButton(parent, "BackButton", "< Back", delegate
            {
                State.SetSelected(null);
            });
            RectTransform backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = new Vector2(4f, -4f);
            backRect.sizeDelta = new Vector2(110f, 28f);

            // Details view (stretched between the back row and the download bar).
            _details = TranslationDetailsView.Create(parent, State);
            RectTransform detailsRect = _details.Root.GetComponent<RectTransform>();
            detailsRect.anchorMin = new Vector2(0f, 0f);
            detailsRect.anchorMax = new Vector2(1f, 1f);
            detailsRect.pivot = new Vector2(0.5f, 0.5f);
            detailsRect.offsetMin = new Vector2(0f, 56f);
            detailsRect.offsetMax = new Vector2(0f, -34f);

            _downloadBar = _details.CreateDownloadBar(parent);
        }

        private void CreateCompactLegendArea(Transform parent)
        {
            GameObject legendPane = LegendPanel.Create(parent, "Legend", 0f);
            RectTransform legendRect = legendPane.GetComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0f, 0f);
            legendRect.anchorMax = new Vector2(1f, 1f);
            legendRect.pivot = new Vector2(0.5f, 0.5f);
            legendRect.anchoredPosition = Vector2.zero;
            legendRect.sizeDelta = Vector2.zero;

            GameObject closeButton = UiFactory.CreateButton(legendPane.transform, "CloseLegend", "", delegate
            {
                ToggleCompactLegend(false);
            }, new Vector2(28f, 28f), StylePalette.PanelAlt, StylePalette.AccentRed, StylePalette.AccentRedDark);
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-4f, -4f);
            closeRect.sizeDelta = new Vector2(28f, 28f);
        }

        private void ToggleCompactLegend()
        {
            ToggleCompactLegend(_compactLegendArea == null || !_compactLegendArea.activeSelf);
        }

        private void ToggleCompactLegend(bool show)
        {
            if (_compactLegendArea == null)
            {
                return;
            }
            _compactLegendArea.SetActive(show);
            if (show && _compactListArea != null)
            {
                _compactListArea.SetActive(false);
                _compactDetailsArea.SetActive(false);
            }
        }

        // ---------------------------------------------------------------------
        // Left pane (Full mode): search, sort, list, footer
        // ---------------------------------------------------------------------

        private void CreateLeftPane(Transform parent)
        {
            // Layout area: search, sort, status, list, footer (in flow).
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

            // Dedicated layer for the list scrollbar, so it is not part of the flow.
            GameObject scrollLayer = new GameObject("ScrollLayer", typeof(RectTransform));
            scrollLayer.transform.SetParent(parent, false);
            UiFactory.StretchToParent(scrollLayer);

            // Search
            TMP_InputField search = UiFactory.CreateInputField(layoutArea.transform, "SearchField", new Vector2(0f, 34f), "Search language, team or author", "");
            UiFactory.AddLayoutElement(search.gameObject, 0f, 34f, 0f, 34f, 1f, 0f);
            search.onValueChanged.AddListener(delegate (string value) { State.SetSearch(value); });

            // Sort cycle button
            GameObject sortButton = UiFactory.CreateButton(layoutArea.transform, "SortButton", BuildSortLabel(), State.CycleSortMode, new Vector2(0f, 30f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(sortButton, 0f, 30f, 0f, 30f, 1f, 0f);
            _sortButtonLabel = sortButton.transform.Find("Label").GetComponent<TextMeshProUGUI>();

            // Status line
            _statusLabel = UiFactory.CreateLabel(layoutArea.transform, "StatusLabel", "", 12f, StylePalette.TextSecondary, TextAlignmentOptions.Left, true);
            UiFactory.AddLayoutElement(_statusLabel.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            // Scrollable list (fills the remaining height)
            _listScroll = UiFactory.CreateScrollView(layoutArea.transform, "ListScroll", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            UiFactory.AddLayoutElement(_listScroll.Root.gameObject, 0f, 100f, 10f, 150f, 1f, 1f);

            _listContent = _listScroll.Content.gameObject;
            _listContent.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 16, 0, 0);

            // Scrollbar lives on the overlay layer (right edge), not in the flow.
            UiFactory.CreateScrollbar(scrollLayer.transform, "ListScrollbar", _listScroll.ScrollRect);
            RectTransform sbRect = scrollLayer.transform.Find("ListScrollbar").GetComponent<RectTransform>();
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
            footerLayout.childForceExpandHeight = true;

            _countLabel = UiFactory.CreateLabel(footer.transform, "Count", "0 translations", 12f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.AddLayoutElement(_countLabel.gameObject, 90f, 28f, 120f, 28f, 1f, 0f);

            GameObject footerSpacer = new GameObject("FooterSpacer", typeof(RectTransform));
            footerSpacer.transform.SetParent(footer.transform, false);
            UiFactory.AddLayoutElement(footerSpacer, 0f, 0f, 0f, 0f, 1f, 0f);

            GameObject refreshButton = UiFactory.CreateButton(footer.transform, "RefreshButton", "Refresh", State.RequestRefresh, new Vector2(90f, 26f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(refreshButton, 90f, 26f, 90f, 26f, 0f, 0f);
            TooltipService.Attach(refreshButton, "Reload the translation list");
        }

        private string BuildSortLabel()
        {
            string label = "Sort: ";
            switch (State.SortMode)
            {
                case CatalogSortMode.Language: label += "Language"; break;
                case CatalogSortMode.Team: label += "Team"; break;
                default: label += "Popularity"; break;
            }
            return label;
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = text ?? "";
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
                _countLabel.text = CatalogService.Count + " translations";
            }
            if (_sortButtonLabel != null)
            {
                _sortButtonLabel.text = BuildSortLabel();
            }
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

            IReadOnlyList<TranslationEntry> visible = State.VisibleEntries;
            if (visible.Count == 0)
            {
                SetStatus(CatalogService.IsLoaded ? "No translations match your search." : "The list is empty.");
                return;
            }

            SetStatus("");
            foreach (TranslationEntry entry in visible)
            {
                TranslationCardView card = TranslationCardView.Create(_listContent.transform, entry, delegate (TranslationCardView c)
                {
                    State.SetSelected(c.Entry.id);
                });
                if (card != null)
                {
                    _cards.Add(card);
                }
            }
            UiFactory.ForceLayoutHeights(_listContent.transform);
            RefreshCardHighlights();
        }

        /// <summary>Re-reads installed languages into the Installed tab (after downloads).</summary>
        public void RefreshInstalledList()
        {
            BuildInstalledList();
        }

        private void OnSelectionChanged()
        {
            if (_mode == BrowserLayoutMode.Compact)
            {
                bool hasSelection = State.SelectedEntry != null;
                if (_compactListArea != null)
                {
                    _compactListArea.SetActive(!hasSelection);
                }
                if (_compactDetailsArea != null)
                {
                    _compactDetailsArea.SetActive(hasSelection);
                }
                if (_compactLegendArea != null)
                {
                    _compactLegendArea.SetActive(false);
                }
            }

            if (_details != null)
            {
                _details.SetEntry(State.SelectedEntry);
            }
            RefreshCardHighlights();
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
                bool selected = card.Entry != null && string.Equals(card.Entry.id, selectedId, StringComparison.OrdinalIgnoreCase);
                card.SetSelected(selected);
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

            // Overlay layer for the scrollbar, so it is not part of the flow.
            GameObject scrollLayer = new GameObject("ScrollLayer", typeof(RectTransform));
            scrollLayer.transform.SetParent(parent, false);
            UiFactory.StretchToParent(scrollLayer);

            TextMeshProUGUI title = UiFactory.CreateLabel(layoutArea.transform, "Title", "Installed translations", 16f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(title.gameObject, 0f, 24f, 0f, 24f, 1f, 0f);

            UiFactory.ScrollViewLayout scroll = UiFactory.CreateScrollView(layoutArea.transform, "InstalledList", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            UiFactory.AddLayoutElement(scroll.Root.gameObject, 0f, 100f, 10f, 150f, 1f, 1f);

            _installedListContent = scroll.Content.gameObject;
            scroll.Content.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 16, 0, 0);

            UiFactory.CreateScrollbar(scrollLayer.transform, "InstalledScrollbar", scroll.ScrollRect);
            RectTransform sbRect = scrollLayer.transform.Find("InstalledScrollbar").GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-3f, 0f);
            sbRect.sizeDelta = new Vector2(10f, 0f);

            BuildInstalledList();
        }

        /// <summary>Rebuilds the Installed tab from the languages loaded locally.</summary>
        private void BuildInstalledList()
        {
            if (_installedListContent == null)
            {
                return;
            }

            for (int i = _installedCards.Count - 1; i >= 0; i--)
            {
                if (_installedCards[i] != null && _installedCards[i].Root != null)
                {
                    _installedCards[i].Root.transform.SetParent(null, false);
                }
                _installedCards[i].Destroy();
            }
            _installedCards.Clear();

            // Clean up any non-card children (e.g. the previous empty-state label).
            for (int i = _installedListContent.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(_installedListContent.transform.GetChild(i).gameObject);
            }

            if (LanguageManager.allLanguages == null || LanguageManager.allLanguages.Count == 0)
            {
                TextMeshProUGUI empty = UiFactory.CreateLabel(_installedListContent.transform, "Empty", "No installed translations.", 13f, StylePalette.TextDim, TextAlignmentOptions.Center, true);
                UiFactory.AddLayoutElement(empty.gameObject, 0f, 24f, 0f, 24f, 1f, 0f);
                _installedSelectedId = null;
                if (_installedDetails != null)
                {
                    _installedDetails.SetEntry(null);
                }
                return;
            }

            foreach (KeyValuePair<string, JsonFormat> pair in LanguageManager.allLanguages)
            {
                string languageName = pair.Key;
                TranslationEntry entry = BuildInstalledEntry(languageName, pair.Value);
                string capturedId = entry.id;

                TranslationCardView card = TranslationCardView.Create(_installedListContent.transform, entry,
                    delegate { SelectInstalled(capturedId); }, true);
                if (card != null)
                {
                    card.SetSelected(string.Equals(capturedId, _installedSelectedId, StringComparison.OrdinalIgnoreCase));
                    _installedCards.Add(card);
                }
            }

            UiFactory.ForceLayoutHeights(_installedListContent.transform);
        }

        /// <summary>
        /// Maps a locally installed language to a card/detail entry. Installed packs carry
        /// only the legacy v1 metadata, so capabilities default to Unknown except fonts,
        /// which can be inferred from the font file names in the metadata.
        /// </summary>
        private static TranslationEntry BuildInstalledEntry(string languageName, JsonFormat lang)
        {
            Metadata md = lang != null ? lang.metadata : null;

            string display = md != null && !string.IsNullOrEmpty(md.langDisplayName) ? md.langDisplayName
                : (md != null && !string.IsNullOrEmpty(md.langName) ? md.langName : languageName);

            TranslationFeatures features = new TranslationFeatures();
            if (md != null && md.fonts != null && !string.IsNullOrEmpty(md.fonts.MainFont))
            {
                features.fonts = FeatureStatus.Full;
            }

            return new TranslationEntry
            {
                id = languageName,
                languageTag = languageName,
                languageName = display,
                author = md != null ? md.langAuthor : null,
                version = md != null ? md.langVersion : null,
                features = features
            };
        }

        private void SelectInstalled(string languageName)
        {
            _installedSelectedId = languageName;
            foreach (TranslationCardView card in _installedCards)
            {
                if (card == null || card.Root == null)
                {
                    continue;
                }
                bool selected = card.Entry != null && string.Equals(card.Entry.id, languageName, StringComparison.OrdinalIgnoreCase);
                card.SetSelected(selected);
            }

            if (_installedDetails != null)
            {
                _installedDetails.SetEntry(FindInstalledEntry(languageName));
            }
        }

        private TranslationEntry FindInstalledEntry(string languageName)
        {
            if (LanguageManager.allLanguages == null || string.IsNullOrEmpty(languageName))
            {
                return null;
            }
            JsonFormat lang;
            if (LanguageManager.allLanguages.TryGetValue(languageName, out lang))
            {
                return BuildInstalledEntry(languageName, lang);
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

            TooltipService.Attach(row, label);
        }

        private static bool ReadConfigBool(string key)
        {
            return Convert.ToBoolean(LanguageManager.configFile.Bind("General", key, "False").Value);
        }

        private static void WriteConfigBool(string key, bool value)
        {
            LanguageManager.configFile.Bind("General", key, "False").Value = value.ToString();
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
    }
}