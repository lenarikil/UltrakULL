using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Central details panel of the selected translation. Shows the header (logo,
    /// flag, translation name, locale, authors, version, likes), the link to the
    /// original Discord post, preview gallery, the four capability rows, the
    /// authors block, the description (scrollable, height-limited) and the action
    /// area. The action area shows the status-specific action button (Download /
    /// Update / Use) and switches into a progress bar with a cancel button while
    /// an install is running. Selecting a card never triggers a download; only
    /// the action button does.
    ///
    /// All text coming from the (untrusted) catalog is rendered with TMP rich
    /// text disabled so markup in names/descriptions can never inject formatting.
    /// </summary>
    public sealed class TranslationDetailsView
    {
        private const int MainAuthorsVisible = 3;
        private const float DescriptionMaxHeight = 240f;

        public GameObject Root;
        public BrowserState State;

        private TranslationEntry _entry;
        private InstallResolution _resolution;
        private GalleryView _gallery;
        private List<GameObject> _extraAuthorRows = new List<GameObject>();
        private GameObject _showAllButton;

        // Action area: either the status button or the download progress bar.
        // The progress bar belongs to one entry at a time (_installingEntryId);
        // it is only shown while that entry is the displayed one, so parallel
        // installs never block the details of other translations (item 2).
        private GameObject _actionArea;
        private GameObject _actionBar;
        /// <summary>The action/download button GameObject, for tour targeting.</summary>
        public GameObject DownloadButton { get { return _actionBar; } }
        private GameObject _progressBar;
        private Image _progressFill;
        private TextMeshProUGUI _progressLabel;
        private GameObject _cancelButton;
        private Action _onCancelDownload;
        private string _installingEntryId;
        // Indeterminate pulse shown while the total size is unknown (content is
        // streamed without Content-Length), so the bar never sticks at zero (5.4).
        private ProgressPulse _progressPulse;

        public static TranslationDetailsView Create(Transform parent, BrowserState state)
        {
            TranslationDetailsView view = new TranslationDetailsView();
            view.State = state;

            GameObject root = new GameObject("DetailsView", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
            rootLayout.spacing = 8f;
            rootLayout.childAlignment = TextAnchor.UpperCenter;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = false;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            UiFactory.StretchToParent(root);
            view.Root = root;

            // Scrollable body
            UiFactory.ScrollViewLayout scroll = UiFactory.CreateScrollView(root.transform, "ScrollArea", Vector2.zero, new Color(0f, 0f, 0f, 0f));
            RectTransform scrollRect = scroll.Root;
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            // Flexible height: the root layout group (childControlHeight = true) sizes the
            // scroll area to the space left over by the fixed action bar below it.
            UiFactory.AddLayoutElement(scroll.Root.gameObject, 0f, 0f, 10f, 300f, 1f, 1f);

            // Native scrollbar on the right edge of the preview (item 12): the
            // mouse wheel works through the ScrollRect itself, the scrollbar
            // adds the drag handle in the game's own style.
            UiFactory.CreateScrollbar(scroll.Root, "DetailsScrollbar", scroll.ScrollRect);
            Transform sbTransform = scroll.Root.Find("DetailsScrollbar");
            RectTransform detailsSbRect = sbTransform != null ? sbTransform.GetComponent<RectTransform>() : null;
            if (detailsSbRect != null)
            {
                detailsSbRect.anchorMin = new Vector2(1f, 0f);
                detailsSbRect.anchorMax = new Vector2(1f, 1f);
                detailsSbRect.pivot = new Vector2(1f, 0.5f);
                detailsSbRect.anchoredPosition = new Vector2(-3f, 0f);
                detailsSbRect.sizeDelta = new Vector2(10f, 0f);
            }

            // Content layout
            VerticalLayoutGroup contentLayout = scroll.Content.GetComponent<VerticalLayoutGroup>();
            if (contentLayout != null)
            {
                contentLayout.spacing = 8f;
                contentLayout.padding = new RectOffset(2, 12, 2, 2);
            }

            view._gallery = null;
            view.Root = root;

            return view;
        }

        /// <summary>
        /// Rebuilds the body for the given translation entry. The install
        /// resolution drives the action button (download / update / use); null
        /// means "no status" (Installed tab uses its own Use button).
        /// </summary>
        public void SetEntry(TranslationEntry entry, InstallResolution resolution = null)
        {
            _entry = entry;
            _resolution = resolution;

            if (_entry == null)
            {
                // Nothing to show: clear existing content and bail out.
                Transform nullContent = Root.transform.Find("ScrollArea/Viewport/Content");
                if (nullContent != null)
                {
                    for (int i = nullContent.childCount - 1; i >= 0; i--)
                    {
                        Transform child = nullContent.GetChild(i);
                        child.SetParent(null, false);
                        UnityEngine.Object.Destroy(child.gameObject);
                    }
                }
                _extraAuthorRows.Clear();
                _showAllButton = null;
                return;
            }

            // Wipe previous content. Detach first: Destroy is deferred until the end
            // of the frame, and new children are created immediately below.
            Transform content = Root.transform.Find("ScrollArea/Viewport/Content");
            if (content == null)
            {
                return;
            }
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Transform child = content.GetChild(i);
                child.SetParent(null, false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
            _extraAuthorRows.Clear();
            _showAllButton = null;

            if (entry == null)
            {
                CreateEmptyState(content);
                RefreshActionArea();
                return;
            }

            // The built-in English is the original game text, not a user
            // translation: it gets a dedicated preview without the manifest
            // sections (no authors, version, features, gallery or likes).
            if (entry.isBuiltinEnglish)
            {
                CreateBuiltinEnglishPreview(content);
                UiFactory.ForceLayoutHeights(content);
                RefreshActionArea();
                return;
            }

            CreateHeader(content);
            CreateGallery(content);
            CreateFeatures(content);
            CreateAuthors(content);
            CreateDescription(content);

            UiFactory.ForceLayoutHeights(content);
            RefreshActionArea();
        }

        // ---------------------------------------------------------------------
        // Sections
        // ---------------------------------------------------------------------

        private void CreateEmptyState(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "EmptyPanel", 110f);
            TextMeshProUGUI label = UiFactory.CreateLabel(panel.transform, "Message", BrowserText.DetailsEmpty, 15f, StylePalette.TextSecondary, TextAlignmentOptions.Center, true);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.offsetMin = new Vector2(20f, 20f);
            label.rectTransform.offsetMax = new Vector2(-20f, -20f);
        }

        /// <summary>
        /// Dedicated preview of the built-in English (Default English): a short
        /// explanation that this is the original game text. No translation
        /// metadata is shown: the in-memory manifest carries no real authors,
        /// version or features, and fake values must not be displayed (10.2).
        /// </summary>
        private void CreateBuiltinEnglishPreview(Transform content)
        {
            // Header: flag/logo and the name with the locale, nothing else.
            GameObject panel = CreateSectionPanel(content, "BuiltinEnglishPanel", 110f);
            VerticalLayoutGroup panelLayout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(8, 8, 8, 8);
            panelLayout.spacing = 8f;
            panelLayout.childAlignment = TextAnchor.UpperCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            GameObject headerRow = new GameObject("HeaderRow", typeof(RectTransform));
            headerRow.transform.SetParent(panel.transform, false);
            HorizontalLayoutGroup rowLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(headerRow, 0f, 64f, 0f, 64f, 1f, 0f);

            UiFactory.LogoSlot logo = UiFactory.CreateLogoSlot(headerRow.transform, "Logo", 64f, _entry.logoUrl, _entry.flagUrl, _entry.languageTag, FlagProvider.GetSprite(_entry.languageTag));
            UiFactory.AddLayoutElement(logo.Root, 64f, 64f, 64f, 64f, 0f, 0f);

            GameObject info = new GameObject("Info", typeof(RectTransform));
            info.transform.SetParent(headerRow.transform, false);
            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 2f;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(info, 10f, 0f, 100f, 64f, 1f, 1f);

            string displayName = !string.IsNullOrWhiteSpace(_entry.name)
                ? _entry.name
                : (!string.IsNullOrWhiteSpace(_entry.id) ? _entry.id : "English");
            string languageName = LanguageDisplay.Name(_entry.languageTag);
            // Name in a masked marquee viewport so long names scroll instead of clipping.
            GameObject nameArea = new GameObject("NameArea", typeof(RectTransform), typeof(RectMask2D));
            nameArea.transform.SetParent(info.transform, false);
            RectTransform nameAreaRect = nameArea.GetComponent<RectTransform>();
            nameAreaRect.anchorMin = new Vector2(0f, 0f);
            nameAreaRect.anchorMax = new Vector2(1f, 1f);
            nameAreaRect.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.AddLayoutElement(nameArea, 0f, 26f, 10f, 26f, 1f, 0f);
            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(nameArea.transform, "NameLabel",
                Cap(displayName, 200) + (string.IsNullOrEmpty(languageName) ? "" : "  [" + languageName + "]"),
                20f, StylePalette.TextPrimary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.SingleLine(nameLabel);
            nameLabel.overflowMode = TextOverflowModes.Overflow;
            nameLabel.enableWordWrapping = false;
            nameLabel.richText = false;
            RectTransform nameLabelRect = nameLabel.GetComponent<RectTransform>();
            nameLabelRect.anchorMin = new Vector2(0f, 0f);
            nameLabelRect.anchorMax = new Vector2(0f, 1f);
            nameLabelRect.pivot = new Vector2(0f, 0.5f);
            nameLabelRect.anchoredPosition = Vector2.zero;
            nameLabelRect.sizeDelta = Vector2.zero;
            nameArea.AddComponent<MarqueeLabel>().Label = nameLabel;

            // The explanation line, mildly witty by request.
            TextMeshProUGUI noteLabel = UiFactory.CreateLabel(info.transform, "NoteLabel",
                BrowserText.BuiltinEnglishDescription,
                14f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            noteLabel.richText = false;
            UiFactory.AddLayoutElement(noteLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
        }

        private void CreateHeader(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "HeaderPanel", 92f);
            VerticalLayoutGroup panelLayout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(8, 8, 8, 8);
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            GameObject headerRow = new GameObject("HeaderRow", typeof(RectTransform));
            headerRow.transform.SetParent(panel.transform, false);
            HorizontalLayoutGroup rowLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            // Fixed row height: force-expanding the children vertically is what
            // stretched the square logo slot (64x76) and the Discord button
            // (34x76) into rectangles (items 8 and 20).
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(headerRow, 0f, 76f, 0f, 76f, 1f, 0f);

            UiFactory.LogoSlot logo = UiFactory.CreateLogoSlot(headerRow.transform, "Logo", 64f, _entry.logoUrl, _entry.flagUrl, _entry.languageTag, FlagProvider.GetSprite(_entry.languageTag));
            UiFactory.AddLayoutElement(logo.Root, 64f, 64f, 64f, 64f, 0f, 0f);

            GameObject info = new GameObject("Info", typeof(RectTransform));
            info.transform.SetParent(headerRow.transform, false);
            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 2f;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(info, 10f, 0f, 100f, 76f, 1f, 1f);

            // Main line: translation name + locale code.
            string displayName = !string.IsNullOrWhiteSpace(_entry.name)
                ? _entry.name
                : (!string.IsNullOrWhiteSpace(_entry.id) ? _entry.id : "?");
            string languageName = LanguageDisplay.Name(_entry.languageTag);
            // Name in a masked marquee viewport so long names scroll instead of clipping.
            GameObject nameArea = new GameObject("NameArea", typeof(RectTransform), typeof(RectMask2D));
            nameArea.transform.SetParent(info.transform, false);
            RectTransform nameAreaRect = nameArea.GetComponent<RectTransform>();
            nameAreaRect.anchorMin = new Vector2(0f, 0f);
            nameAreaRect.anchorMax = new Vector2(1f, 1f);
            nameAreaRect.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.AddLayoutElement(nameArea, 0f, 26f, 10f, 26f, 1f, 0f);
            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(nameArea.transform, "NameLabel",
                Cap(displayName, 200) + (string.IsNullOrEmpty(languageName) ? "" : "  [" + languageName + "]"),
                20f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(nameLabel);
            nameLabel.overflowMode = TextOverflowModes.Overflow;
            nameLabel.enableWordWrapping = false;
            nameLabel.richText = false;
            RectTransform nameLabelRect = nameLabel.GetComponent<RectTransform>();
            nameLabelRect.anchorMin = new Vector2(0f, 0f);
            nameLabelRect.anchorMax = new Vector2(0f, 1f);
            nameLabelRect.pivot = new Vector2(0f, 0.5f);
            nameLabelRect.anchoredPosition = Vector2.zero;
            nameLabelRect.sizeDelta = Vector2.zero;
            nameArea.AddComponent<MarqueeLabel>().Label = nameLabel;

            // Authors line.
            string authorText = !string.IsNullOrWhiteSpace(_entry.author) ? _entry.author : (_entry.teamName ?? "");
            TextMeshProUGUI authorLabel = UiFactory.CreateLabel(info.transform, "AuthorLabel",
                string.IsNullOrEmpty(authorText) ? BrowserText.NoAuthors : Cap(authorText, 200),
                15f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.SingleLine(authorLabel);
            authorLabel.richText = false;
            UiFactory.AddLayoutElement(authorLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);

            // Version line: on an available update it shows "installed -> available".
            TextMeshProUGUI versionLabel = UiFactory.CreateLabel(info.transform, "VersionLabel", BuildVersionText(), 13f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(versionLabel);
            versionLabel.richText = false;
            UiFactory.AddLayoutElement(versionLabel.gameObject, 0f, 18f, 0f, 18f, 1f, 0f);

            // Likes block, pushed to the right
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(headerRow.transform, false);
            UiFactory.AddLayoutElement(spacer, 0f, 0f, 0f, 0f, 1f, 0f);

            GameObject likesBox = new GameObject("LikesBox", typeof(RectTransform));
            likesBox.transform.SetParent(headerRow.transform, false);
            HorizontalLayoutGroup likesLayout = likesBox.AddComponent<HorizontalLayoutGroup>();
            likesLayout.spacing = 6f;
            likesLayout.childAlignment = TextAnchor.MiddleLeft;
            likesLayout.childControlWidth = true;
            likesLayout.childControlHeight = false;
            likesLayout.childForceExpandWidth = false;
            likesLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(likesBox, 60f, 30f, 60f, 30f, 0f, 0f);

            Image likesIcon = UiFactory.CreateImage(likesBox.transform, "Icon", new Vector2(20f, 20f), StylePalette.TextSecondary, PrimitiveSprites.Kind.Heart);
            UiFactory.AddLayoutElement(likesIcon.gameObject, 20f, 20f, 20f, 20f, 0f, 0f);

            TextMeshProUGUI likeCountLabel = UiFactory.CreateLabel(likesBox.transform, "Count",
                _entry.likes.HasValue ? _entry.likes.Value.ToString() : "—",
                17f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.AddLayoutElement(likeCountLabel.gameObject, 30f, 20f, 30f, 20f, 0f, 0f);

            // Link to the original Discord publication thread (only when the
            // catalog provided valid coordinates).
            if (UrlPolicy.IsHttpUrl(_entry.discordUrl))
            {
                string discordUrl = _entry.discordUrl;
                GameObject discordButton = UiFactory.CreateButton(headerRow.transform, "DiscordButton", "",
                    delegate { Application.OpenURL(discordUrl); },
                    new Vector2(34f, 34f),
                    StylePalette.PanelAlt,
                    StylePalette.BorderBlue,
                    StylePalette.PanelHover);
                UiFactory.AddLayoutElement(discordButton, 34f, 34f, 34f, 34f, 0f, 0f);
                Image discordIcon = UiFactory.CreateImage(discordButton.transform, "Icon", new Vector2(18f, 18f), Color.white, PrimitiveSprites.Kind.Discord);
                RectTransform discordIconRect = discordIcon.GetComponent<RectTransform>();
                discordIconRect.anchorMin = new Vector2(0.5f, 0.5f);
                discordIconRect.anchorMax = new Vector2(0.5f, 0.5f);
                discordIconRect.pivot = new Vector2(0.5f, 0.5f);
                discordIconRect.anchoredPosition = Vector2.zero;
                discordIconRect.sizeDelta = new Vector2(18f, 18f);
                TooltipService.Attach(discordButton, BrowserText.ActionOpenSource);
            }
        }

        private string BuildVersionText()
        {
            string version = string.IsNullOrWhiteSpace(_entry.version) ? null : "v" + InstallStatusResolver.NormalizeVersion(_entry.version);
            bool updateAvailable = _resolution != null && _resolution.Status == InstallStatus.UpdateAvailable;

            if (updateAvailable)
            {
                // The resolver only reports an update when both versions are known.
                string installed = _resolution.InstalledVersion;
                return BrowserText.VersionUpdateFromTo(
                    string.IsNullOrEmpty(installed) ? "?" : InstallStatusResolver.NormalizeVersion(installed),
                    InstallStatusResolver.NormalizeVersion(_entry.version));
            }

            return version ?? "—";
        }

        private void CreateGallery(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "GalleryPanel", 210f);

            _gallery = GalleryView.Create(panel.transform, "Gallery", new Vector2(0f, 210f));
            RectTransform galleryRect = _gallery.Root.GetComponent<RectTransform>();
            galleryRect.anchorMin = Vector2.zero;
            galleryRect.anchorMax = Vector2.one;
            galleryRect.offsetMin = Vector2.zero;
            galleryRect.offsetMax = Vector2.zero;
            UiFactory.AddLayoutElement(_gallery.Root, 0f, 210f, 0f, 210f, 1f, 0f);

            _gallery.SetUrls(_entry.gallery);
        }

        private void CreateFeatures(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "FeaturesPanel", 0f);
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TranslationFeatures features = _entry.features ?? new TranslationFeatures();
            CreateFeatureRow(panel.transform, "RowDubbing", TranslationFeatureType.Dubbing, features.dubbing,
                BrowserText.FeatureDescVoiceOver);
            CreateFeatureRow(panel.transform, "RowExtraDubbing", TranslationFeatureType.ExtraDubbing, features.extraDubbing,
                BrowserText.FeatureDescAdditionalVoiceOver);
            CreateFeatureRow(panel.transform, "RowTextures", TranslationFeatureType.Textures, features.textures,
                BrowserText.FeatureDescReplacementTextures);
            CreateFeatureRow(panel.transform, "RowFonts", TranslationFeatureType.Fonts, features.fonts,
                BrowserText.FeatureDescCustomFonts);
        }

        private void CreateFeatureRow(Transform parent, string name, TranslationFeatureType type, FeatureStatus status, string description)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(row, 0f, 42f, 0f, 42f, 1f, 0f);

            FeatureIndicatorView indicator = FeatureIndicator.Create(row.transform, "Indicator", type, status, 30f, true);
            UiFactory.AddLayoutElement(indicator.Root, 30f, 30f, 30f, 30f, 0f, 0f);

            GameObject info = new GameObject("Info", typeof(RectTransform));
            info.transform.SetParent(row.transform, false);
            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 0f;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(info, 10f, 0f, 100f, 42f, 1f, 1f);

            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(info.transform, "Name", BrowserText.FeatureName(type), 15f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(nameLabel);
            UiFactory.AddLayoutElement(nameLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
            TextMeshProUGUI descLabel = UiFactory.CreateLabel(info.transform, "Description", description, 12f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, true);
            descLabel.richText = false;
            UiFactory.AddLayoutElement(descLabel.gameObject, 0f, 18f, 0f, 18f, 1f, 0f);

            TextMeshProUGUI statusLabel = UiFactory.CreateLabel(row.transform, "Status", StylePalette.FeatureLabel(status), 13f, StylePalette.FeatureColor(status), TextAlignmentOptions.MidlineRight, false);
            UiFactory.AddLayoutElement(statusLabel.gameObject, 130f, 20f, 130f, 20f, 0f, 0f);
        }

        private void CreateAuthors(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "AuthorsPanel", 0f);
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI section = UiFactory.CreateLabel(panel.transform, "SectionLabel", BrowserText.AuthorsSection, 17f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(section.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            // The catalog ships authors as one plain string.
            if (_entry.authors == null || _entry.authors.Count == 0)
            {
                string authorText = !string.IsNullOrWhiteSpace(_entry.author) ? _entry.author : (_entry.teamName ?? "");
                TextMeshProUGUI authorLabel = UiFactory.CreateLabel(panel.transform, "Author",
                    string.IsNullOrEmpty(authorText) ? BrowserText.NoAuthors : authorText,
                    13f, StylePalette.TextSecondary, TextAlignmentOptions.Left, true);
                authorLabel.richText = false;
                UiFactory.AddLayoutElement(authorLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
                return;
            }

            // Legacy structured author list (locally synthesized entries).
            _extraAuthorRows.Clear();
            for (int i = 0; i < _entry.authors.Count; i++)
            {
                GameObject row = CreateAuthorRow(panel.transform, _entry.authors[i]);
                if (i >= MainAuthorsVisible)
                {
                    row.SetActive(false);
                    _extraAuthorRows.Add(row);
                }
            }

            if (_extraAuthorRows.Count > 0)
            {
                _showAllButton = UiFactory.CreateButton(panel.transform, "ShowAll",
                    BrowserText.AuthorsShowAll(_entry.authors.Count),
                    ShowAllAuthors,
                    new Vector2(0f, 30f),
                    StylePalette.PanelAlt,
                    StylePalette.BorderBlue,
                    StylePalette.PanelHover);
                UiFactory.AddLayoutElement(_showAllButton, 0f, 30f, 0f, 30f, 1f, 0f);
            }
        }

        private GameObject CreateAuthorRow(Transform parent, TranslationAuthor author)
        {
            GameObject row = new GameObject("AuthorRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(row, 0f, 36f, 0f, 36f, 1f, 0f);

            Image avatar = UiFactory.CreateImage(row.transform, "Avatar", new Vector2(28f, 28f), StylePalette.TextDim, PrimitiveSprites.Kind.Circle);
            UiFactory.AddLayoutElement(avatar.gameObject, 28f, 28f, 28f, 28f, 0f, 0f);
            if (author != null && !string.IsNullOrWhiteSpace(author.avatarUrl))
            {
                string url = author.avatarUrl;
                ImageCache.GetOrLoad(url, delegate (Sprite sprite)
                {
                    if (avatar != null && sprite != null)
                    {
                        avatar.sprite = sprite;
                        avatar.color = Color.white;
                    }
                });
            }

            GameObject info = new GameObject("Info", typeof(RectTransform));
            info.transform.SetParent(row.transform, false);
            VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 0f;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(info, 10f, 0f, 100f, 36f, 1f, 1f);

            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(info.transform, "Name", author != null ? (author.name ?? "?") : "?", 15f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(nameLabel);
            nameLabel.richText = false;
            UiFactory.AddLayoutElement(nameLabel.gameObject, 0f, 18f, 0f, 18f, 1f, 0f);
            TextMeshProUGUI roleLabel = UiFactory.CreateLabel(info.transform, "Role", author != null ? (author.role ?? "") : "", 12f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(roleLabel);
            roleLabel.richText = false;
            UiFactory.AddLayoutElement(roleLabel.gameObject, 0f, 16f, 0f, 16f, 1f, 0f);

            if (author != null && !string.IsNullOrWhiteSpace(author.link))
            {
                string link = author.link;
                GameObject linkButton = UiFactory.CreateButton(row.transform, "Link",
                    "Link",
                    delegate { Application.OpenURL(link); },
                    new Vector2(56f, 24f),
                    StylePalette.PanelAlt,
                    StylePalette.BorderBlue,
                    StylePalette.PanelHover);
                UiFactory.AddLayoutElement(linkButton, 56f, 24f, 56f, 24f, 0f, 0f);
            }

            return row;
        }

        private void ShowAllAuthors()
        {
            foreach (GameObject row in _extraAuthorRows)
            {
                if (row != null)
                {
                    row.SetActive(true);
                }
            }
            if (_showAllButton != null)
            {
                _showAllButton.SetActive(false);
            }
        }

        private void CreateDescription(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "DescriptionPanel", 0f);
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI section = UiFactory.CreateLabel(panel.transform, "SectionLabel", BrowserText.DescriptionSection, 17f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(section.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            if (string.IsNullOrWhiteSpace(_entry.readme))
            {
                TextMeshProUGUI empty = UiFactory.CreateLabel(panel.transform, "Empty", BrowserText.NoDescription, 13f, StylePalette.TextDim, TextAlignmentOptions.Left, true);
                empty.richText = false;
                UiFactory.AddLayoutElement(empty.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
                return;
            }

            UiFactory.ScrollViewLayout scroll = UiFactory.CreateScrollView(panel.transform, "DescriptionScroll", new Vector2(0f, DescriptionMaxHeight), StylePalette.Panel);
            RectTransform scrollRect = scroll.Root;
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 0f);
            scrollRect.pivot = new Vector2(0.5f, 0f);
            scrollRect.anchoredPosition = Vector2.zero;
            scrollRect.sizeDelta = new Vector2(0f, DescriptionMaxHeight);
            UiFactory.AddLayoutElement(scroll.Root.gameObject, 0f, DescriptionMaxHeight, 0f, DescriptionMaxHeight, 1f, 0f);

            scroll.Content.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            scroll.Content.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            // Catalog descriptions may contain markdown links [text](url). Convert
            // them to TMP rich text <link=url>text</link> so they are clickable
            // inside the scrollable description. Other markup (**bold*, *italic*)
            // is also converted. Unsafe characters are escaped first.
            string richDescription = MarkdownToRichText(_entry.readme);
            TextMeshProUGUI descriptionText = UiFactory.CreateLabel(scroll.Content, "Text", richDescription, 13f, StylePalette.TextPrimary, TextAlignmentOptions.TopLeft, true);
            descriptionText.richText = true;
            // No preferred/flexible height here: a LayoutElement with those set
            // (priority 1) overrides TMP's natural preferred height, capping the
            // text at the viewport size so the ScrollRect has nothing to scroll.
            // With TMP driving the height, the ContentSizeFitter grows the
            // content past the viewport and the description becomes scrollable.
            UiFactory.AddLayoutElement(descriptionText.gameObject, 0f, 0f, 10f);

            RectTransform sbRect = UiFactory.CreateScrollbar(panel.transform, "DescriptionScrollbar", scroll.ScrollRect).GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-2f, 0f);
            sbRect.sizeDelta = new Vector2(10f, 0f);

            // Clickable links in the description with confirmation (5.17).
            string link = ExtractFirstUrl(_entry.readme);
            if (!string.IsNullOrEmpty(link))
            {
                Canvas canvas = Root.GetComponentInParent<Canvas>();
                GameObject linkRow = new GameObject("LinkRow", typeof(RectTransform));
                linkRow.transform.SetParent(panel.transform, false);
                HorizontalLayoutGroup linkLayout = linkRow.AddComponent<HorizontalLayoutGroup>();
                linkLayout.spacing = 6f;
                linkLayout.childAlignment = TextAnchor.MiddleLeft;
                linkLayout.childControlWidth = false;
                linkLayout.childControlHeight = true;
                linkLayout.childForceExpandWidth = false;
                linkLayout.childForceExpandHeight = false;
                UiFactory.AddLayoutElement(linkRow, 0f, 30f, 0f, 30f, 1f, 0f);

                GameObject linkButton = UiFactory.CreateButton(linkRow.transform, "OpenLink",
                    "",
                    delegate
                    {
                        if (canvas != null)
                        {
                            ConfirmDialog.Show(canvas, BrowserText.ConfirmOpenLinkTitle(),
                                BrowserText.ConfirmOpenLinkBody(link),
                                delegate { Application.OpenURL(link); });
                        }
                        else
                        {
                            Application.OpenURL(link);
                        }
                    },
                    new Vector2(0f, 30f),
                    StylePalette.PanelAlt,
                    StylePalette.BorderBlue,
                    StylePalette.PanelHover);
                UiFactory.AddLayoutElement(linkButton, 0f, 30f, 200f, 30f, 1f, 0f);
                var linkLabelTransform = linkButton.transform.Find("Label");
                TextMeshProUGUI linkLabel = linkLabelTransform != null ? linkLabelTransform.GetComponent<TextMeshProUGUI>() : null;
                if (linkLabel != null)
                {
                    linkLabel.text = BrowserText.ActionOpenLink + " " + Cap(link, 60);
                }
            }

            // Social Links block: one button per hyperlink from the catalog's
            // Social Links field. Button text is the link's display text; the
            // URL itself is never shown. Clicking asks for confirmation, then
            // opens the URL (same Ask Window flow as other external links).
            var socialLinks = _entry.socialLinks;
            if (socialLinks != null && socialLinks.Count > 0)
            {
                TextMeshProUGUI socialSection = UiFactory.CreateLabel(panel.transform, "SocialSectionLabel", BrowserText.SocialLinksSection, 15f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
                UiFactory.AddLayoutElement(socialSection.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

                Canvas canvas = Root.GetComponentInParent<Canvas>();
                foreach (var kvp in socialLinks)
                {
                    string display = kvp.Key;
                    string url = kvp.Value;
                    if (string.IsNullOrEmpty(display) || string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    GameObject socialRow = new GameObject("SocialLinkRow", typeof(RectTransform));
                    socialRow.transform.SetParent(panel.transform, false);
                    HorizontalLayoutGroup socialLayout = socialRow.AddComponent<HorizontalLayoutGroup>();
                    socialLayout.spacing = 6f;
                    socialLayout.childAlignment = TextAnchor.MiddleLeft;
                    socialLayout.childControlWidth = false;
                    socialLayout.childControlHeight = true;
                    socialLayout.childForceExpandWidth = false;
                    socialLayout.childForceExpandHeight = false;
                    UiFactory.AddLayoutElement(socialRow, 0f, 30f, 0f, 30f, 1f, 0f);

                    GameObject socialButton = UiFactory.CreateButton(socialRow.transform, "SocialLink",
                        "",
                        delegate
                        {
                            if (canvas != null)
                            {
                                ConfirmDialog.Show(canvas, BrowserText.ConfirmOpenLinkTitle(),
                                    BrowserText.ConfirmOpenLinkBody(url),
                                    delegate { Application.OpenURL(url); });
                            }
                            else
                            {
                                Application.OpenURL(url);
                            }
                        },
                        new Vector2(0f, 30f),
                        StylePalette.PanelAlt,
                        StylePalette.BorderBlue,
                        StylePalette.PanelHover);
                    UiFactory.AddLayoutElement(socialButton, 0f, 30f, 200f, 30f, 1f, 0f);
                    var socialLabelTransform = socialButton.transform.Find("Label");
                    TextMeshProUGUI socialLabel = socialLabelTransform != null ? socialLabelTransform.GetComponent<TextMeshProUGUI>() : null;
                    if (socialLabel != null)
                    {
                        socialLabel.text = Cap(display, 60);
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // Action area (status button <-> download progress)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Creates the pinned action area under the scroll area: hosts either the
        /// status-specific action button or the download progress bar. Call
        /// <see cref="RefreshActionArea"/> after <see cref="SetEntry"/> to fill it.
        /// </summary>
        public GameObject CreateActionArea(Transform parent)
        {
            GameObject area = new GameObject("ActionArea", typeof(RectTransform));
            area.transform.SetParent(parent, false);
            RectTransform areaRect = area.GetComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0f, 0f);
            areaRect.anchorMax = new Vector2(1f, 0f);
            areaRect.pivot = new Vector2(0.5f, 0f);
            areaRect.anchoredPosition = Vector2.zero;
            areaRect.sizeDelta = new Vector2(0f, 54f);
            UiFactory.AddLayoutElement(area, 0f, 54f, 0f, 54f, 1f, 0f);

            _actionArea = area;
            CreateProgressBar(area.transform);
            _actionBar = null;
            return area;
        }

        /// <summary>Builds the action button for the current entry and resolution.</summary>
        public void RefreshActionArea()
        {
            if (_actionArea == null)
            {
                return;
            }

            if (_actionBar != null)
            {
                UnityEngine.Object.Destroy(_actionBar);
                _actionBar = null;
            }

            if (_entry == null)
            {
                SetProgressVisible(false);
                return;
            }

            string label;
            PrimitiveSprites.Kind icon;
            Action onClick;

            if (_resolution != null && _resolution.Status == InstallStatus.UpdateAvailable)
            {
                label = BrowserText.ActionUpdate;
                icon = PrimitiveSprites.Kind.Download;
                onClick = RequestDownload;
            }
            else if (_resolution != null && _resolution.Status == InstallStatus.Installed && _resolution.Pack != null)
            {
                // The pack is installed and can always be re-downloaded /
                // replaced from the catalog. The in-use state stays visible
                // through the green card outline and the Installed tab (item 22).
                label = BrowserText.ActionReinstall;
                icon = PrimitiveSprites.Kind.Download;
                onClick = RequestDownload;
            }
            else
            {
                // Archive already cached but not installed → offer install without
                // re-downloading.
                bool cached = _entry != null && !string.IsNullOrEmpty(_entry.id)
                    && PackInstaller.GetCachedArchive(_entry.id) != null;
                label = cached ? BrowserText.ActionReadyToInstall : BrowserText.ActionDownload;
                icon = PrimitiveSprites.Kind.Download;
                onClick = RequestDownload;
            }

            _actionBar = CreateActionButton(_actionArea.transform, label, icon, onClick);
            _actionBar.SetActive(!IsProgressVisible);
        }

        /// <summary>
        /// Overrides the action button's label without touching its behavior.
        /// Used by the product tour (e.g. the "Ready to install" step shows
        /// the button as "READY TO INSTALL" instead of "Download").
        /// </summary>
        public void SetActionLabel(string text)
        {
            if (_actionBar == null)
            {
                return;
            }
            TextMeshProUGUI label = _actionBar.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
            }
        }

        private void RequestDownload()
        {
            if (_entry != null && State != null)
            {
                State.RequestDownload(_entry);
            }
        }

        /// <summary>
        /// Accent action button used for the Installed tab's fixed "Use" action.
        /// Independent of the status-driven action area.
        /// </summary>
        public GameObject CreateActionBar(Transform parent, string label, PrimitiveSprites.Kind icon, System.Action onClick)
        {
            return CreateActionButton(parent, label, icon, onClick);
        }

        private GameObject CreateActionButton(Transform parent, string label, PrimitiveSprites.Kind icon, System.Action onClick)
        {
            GameObject buttonObj = UiFactory.CreateButton(parent, "ActionBar", label,
                delegate
                {
                    if (onClick != null)
                    {
                        onClick();
                    }
                },
                new Vector2(0f, 54f),
                StylePalette.ActionButtonBg,
                StylePalette.AccentRed,
                StylePalette.ActionButtonHover);

            RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0f, 0f);
            btnRect.anchorMax = new Vector2(1f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = new Vector2(0f, 54f);
            UiFactory.AddLayoutElement(buttonObj, 0f, 54f, 0f, 54f, 1f, 0f);

            Image iconImage = UiFactory.CreateImage(buttonObj.transform, "Icon", new Vector2(24f, 24f), StylePalette.AccentRed, icon);
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(-110f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);

            return buttonObj;
        }

        private void CreateProgressBar(Transform parent)
        {
            GameObject bar = UiFactory.CreatePanel(parent, "DownloadProgress", new Vector2(0f, 54f), StylePalette.Panel, StylePalette.AccentRed, 2f);
            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = Vector2.zero;
            barRect.sizeDelta = new Vector2(0f, 54f);
            UiFactory.AddLayoutElement(bar, 0f, 54f, 0f, 54f, 1f, 0f);
            _progressBar = bar;

            // Clip everything on the bar (fill, pulse, label, cancel button)
            // to the bar's rectangle so no percentage - including 100% and the
            // indeterminate pulse - can render outside the frame (item 6).
            // RectMask2D clips by the rect and, unlike Mask, does not depend
            // on the mask graphic's alpha.
            RectMask2D barMask = bar.AddComponent<RectMask2D>();
            barMask.padding = Vector4.zero;

            // Fill image: left-anchored, width driven by the progress percentage.
            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(bar.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0f, 0f);
            Image fillImage = fill.AddComponent<Image>();
            // The bar panel renders the native small rounded art, so the fill
            // uses the same rounded sprite and stays inside the frame's
            // corners; flat white sprite when Addressables art is unavailable.
            if (NativeUi.IsReady && NativeUi.FillSmall != null)
            {
                NativeUi.ApplySliced(fillImage, NativeUi.FillSmall, StylePalette.ProgressFill);
            }
            else
            {
                fillImage.sprite = UiFactory.WhiteSprite;
                fillImage.color = StylePalette.ProgressFill;
            }
            fillImage.raycastTarget = false;
            // Keep the frame visible: CreatePanel adds the NativeBorder child
            // (or the four fallback strips) before the fill, and a later
            // sibling renders on top, so the fill would cover the frame while
            // downloading. Render the fill below the border instead so the
            // frame is drawn over the fill at every percentage (item 6).
            fill.transform.SetAsFirstSibling();
            _progressFill = fillImage;

            _progressLabel = UiFactory.CreateLabel(bar.transform, "Label", "", 15f, StylePalette.TextPrimary, TextAlignmentOptions.Center, false);
            _progressLabel.richText = false;
            UiFactory.StretchToParent(_progressLabel);

            _cancelButton = UiFactory.CreateButton(bar.transform, "CancelButton", "",
                delegate
                {
                    Action cancel = _onCancelDownload;
                    if (cancel != null)
                    {
                        cancel();
                    }
                },
                new Vector2(90f, 40f),
                StylePalette.PanelAlt,
                StylePalette.AccentRed,
                StylePalette.AccentRedDark);
            RectTransform cancelRect = _cancelButton.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(1f, 0.5f);
            cancelRect.anchorMax = new Vector2(1f, 0.5f);
            cancelRect.pivot = new Vector2(1f, 0.5f);
            cancelRect.anchoredPosition = new Vector2(-6f, 0f);
            cancelRect.sizeDelta = new Vector2(90f, 40f);

            TextMeshProUGUI cancelLabel = UiFactory.CreateLabel(_cancelButton.transform, "Label", BrowserText.ActionCancel, 13f, StylePalette.TextPrimary, TextAlignmentOptions.Center, false);
            UiFactory.StretchToParent(cancelLabel);

            bar.SetActive(false);
        }

        /// <summary>True while the progress bar replaces the action button.</summary>
        public bool IsProgressVisible
        {
            get { return _progressBar != null && _progressBar.activeSelf; }
        }

        /// <summary>
        /// Switches the action area into download-progress mode for
        /// <paramref name="entryId"/>. The bar is only visible while that entry
        /// is the one currently displayed; otherwise the action button stays
        /// available (items 1 and 2).
        /// </summary>
        public void ShowDownloadProgress(string entryId, InstallProgress progress)
        {
            if (_progressBar == null)
            {
                return;
            }

            _installingEntryId = entryId;

            bool isCurrent = !string.IsNullOrEmpty(entryId)
                && _entry != null
                && string.Equals(_entry.id, entryId, StringComparison.Ordinal);

            if (isCurrent)
            {
                UpdateProgressContents(progress);
            }
            SetProgressVisible(isCurrent);
        }

        /// <summary>
        /// Leaves download-progress mode. With an <paramref name="entryId"/>, only
        /// clears the bar when it belongs to that entry.
        /// </summary>
        public void HideDownloadProgress(string entryId = null)
        {
            if (string.IsNullOrEmpty(entryId) ||
                string.Equals(_installingEntryId, entryId, StringComparison.Ordinal))
            {
                _installingEntryId = null;
                SetProgressVisible(false);
                _onCancelDownload = null;
                StopProgressPulse();
            }
        }

        private void UpdateProgressContents(InstallProgress progress)
        {
            long done = progress != null ? progress.BytesRead : 0;
            long? total = progress != null ? progress.TotalBytes : null;

            // Stage label (resolving / downloading / extracting / installing)
            // followed by the percent while the total size is known (item 16).
            string stageLabel = progress != null ? BrowserText.InstallStageLabel(progress.Stage) : "";
            string percent = BrowserText.DownloadProgress(done, total);
            _progressLabel.text = percent.Length == 0 ? stageLabel : stageLabel + " " + percent;

            RectTransform fillRect = _progressFill.rectTransform;
            RectTransform barRect = _progressBar.GetComponent<RectTransform>();
            if (!total.HasValue || total.Value <= 0)
            {
                // Total size unknown: pulse the fill so the bar shows activity
                // instead of sticking at zero (5.4).
                EnsureProgressPulse();
                _progressPulse.Target = fillRect;
                _progressPulse.BarWidth = barRect.rect.width;
                _progressPulse.Enabled = true;
                fillRect.sizeDelta = new Vector2(0f, 0f);
            }
            else
            {
                StopProgressPulse();
                float fraction = Mathf.Clamp01((float)done / total.Value);
                fillRect.sizeDelta = new Vector2(barRect.rect.width * fraction, 0f);
            }
        }

        private void EnsureProgressPulse()
        {
            if (_progressPulse == null)
            {
                _progressPulse = _progressFill.gameObject.AddComponent<ProgressPulse>();
            }
        }

        private void StopProgressPulse()
        {
            if (_progressPulse != null)
            {
                _progressPulse.Enabled = false;
                _progressPulse.Target = null;
            }
        }

        /// <summary>Registers the handler invoked by the progress bar's cancel button.</summary>
        public void SetCancelHandler(Action onCancel)
        {
            _onCancelDownload = onCancel;
        }

        private void SetProgressVisible(bool visible)
        {
            if (_progressBar != null)
            {
                _progressBar.SetActive(visible);
            }
            if (_actionBar != null)
            {
                _actionBar.SetActive(!visible);
            }
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private GameObject CreateSectionPanel(Transform parent, string name, float preferredHeight)
        {
            GameObject panel = UiFactory.CreatePanel(parent, name, new Vector2(0f, preferredHeight), StylePalette.Panel, StylePalette.BorderDim, 1f);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;

            // Fixed-height panels declare their height through a LayoutElement; dynamic
            // panels (preferredHeight <= 0) get no LayoutElement so the parent layout
            // group reads the preferred height of the panel's own VerticalLayoutGroup.
            // Mixing a ContentSizeFitter with a LayoutElement here made the parent
            // group and the fitter fight over the height, collapsing/overlapping panels.
            if (preferredHeight > 0f)
            {
                UiFactory.AddLayoutElement(panel, 0f, preferredHeight, 0f, preferredHeight, 0f, 0f);
            }
            return panel;
        }

        private static string Cap(string value, int max)
        {
            if (value == null || value.Length <= max)
            {
                return value;
            }
            return value.Substring(0, max);
        }

        private static readonly Regex UrlRegex = new Regex(@"https?://\S+", RegexOptions.Compiled);

        private static string ExtractFirstUrl(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            Match match = UrlRegex.Match(text);
            if (!match.Success)
            {
                return null;
            }
            string url = match.Value;
            // Strip trailing punctuation that is not part of the URL.
            while (url.Length > 0 && ".,;:)!>\"'".IndexOf(url[url.Length - 1]) >= 0)
            {
                url = url.Substring(0, url.Length - 1);
            }
            return url;
        }

        /// <summary>
        /// Converts a subset of markdown to TMP rich text:
        /// <list type="bullet">
        ///   <item><description>[text](url) → &lt;link=url&gt;text&lt;/link&gt;</description></item>
        ///   <item><description>**text** → &lt;b&gt;text&lt;/b&gt;</description></item>
        ///   <item><description>*text* → &lt;i&gt;text&lt;/i&gt;</description></item>
        /// </list>
        /// Unsafe characters (&lt;, &gt;, &amp;) are escaped first so untrusted
        /// catalog text cannot inject arbitrary TMP tags.
        /// </summary>
        private static string MarkdownToRichText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            // Escape XML-unsafe chars first, then rebuild safe markup.
            var sb = new System.Text.StringBuilder(text);
            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            string escaped = sb.ToString();

            // [text](url) -> <link=url>text</link>
            escaped = _markdownLinkRegex.Replace(escaped, "<link=\"$2\">$1</link>");

            // **text** -> <b>text</b> (greedy, non-overlapping)
            escaped = _markdownBoldRegex.Replace(escaped, "<b>$1</b>");

            // *text* -> <i>text</i> (but not ** which is already handled)
            escaped = _markdownItalicRegex.Replace(escaped, "<i>$1</i>");

            return escaped;
        }

        private static readonly Regex _markdownLinkRegex = new Regex(
            @"\[([^\]]*?)\]\((https?://[^\s)]+)\)",
            RegexOptions.Compiled);

        private static readonly Regex _markdownBoldRegex = new Regex(
            @"\*\*(.+?)\*\*",
            RegexOptions.Compiled);

        private static readonly Regex _markdownItalicRegex = new Regex(
            @"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)",
            RegexOptions.Compiled);

        /// <summary>Pulses a fill rect back and forth while the total size is unknown.</summary>
        private sealed class ProgressPulse : MonoBehaviour
        {
            public RectTransform Target;
            public float BarWidth;
            public bool Enabled;

            private void Update()
            {
                if (!Enabled || Target == null || BarWidth <= 0f)
                {
                    return;
                }
                float wave = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
                Target.sizeDelta = new Vector2(BarWidth * wave * 0.7f, 0f);
            }
        }
    }
}
