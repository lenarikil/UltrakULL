using System.Collections.Generic;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Central details panel of the selected translation. Shows the header (logo,
    /// flag, language, team, version, likes), preview gallery, the four capability
    /// rows, the author list with expandable extras, the README block (scrollable,
    /// height-limited) and the accent download button. Selecting a card never
    /// triggers a download; only the download button does.
    /// </summary>
    public sealed class TranslationDetailsView
    {
        private const int MainAuthorsVisible = 3;
        private const float ReadmeMaxHeight = 240f;

        public GameObject Root;
        public BrowserState State;

        private TranslationEntry _entry;
        private GalleryView _gallery;
        private TextMeshProUGUI _likeCountLabel;
        private List<GameObject> _extraAuthorRows = new List<GameObject>();
        private GameObject _showAllButton;

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

            // Content layout
            scroll.Content.GetComponent<VerticalLayoutGroup>().spacing = 8f;
            scroll.Content.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(2, 2, 2, 2);

            view._gallery = null;
            view.Root = root;

            return view;
        }

        /// <summary>Rebuilds the body for the given translation entry.</summary>
        public void SetEntry(TranslationEntry entry)
        {
            _entry = entry;

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
                return;
            }

            CreateHeader(content);
            CreateGallery(content);
            CreateFeatures(content);
            CreateAuthors(content);
            CreateReadme(content);

            UiFactory.ForceLayoutHeights(content);
        }

        // ---------------------------------------------------------------------
        // Sections
        // ---------------------------------------------------------------------

        private void CreateEmptyState(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "EmptyPanel", 110f);
            TextMeshProUGUI label = UiFactory.CreateLabel(panel.transform, "Message", "Select a translation from the list to preview its details.", 15f, StylePalette.TextSecondary, TextAlignmentOptions.Center, true);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.offsetMin = new Vector2(20f, 20f);
            label.rectTransform.offsetMax = new Vector2(-20f, -20f);
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
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            UiFactory.AddLayoutElement(headerRow, 0f, 76f, 0f, 76f, 1f, 1f);

            UiFactory.LogoSlot logo = UiFactory.CreateLogoSlot(headerRow.transform, "Logo", 64f, _entry.logoUrl, _entry.flagUrl, _entry.languageTag);
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

            string languageCode = string.IsNullOrWhiteSpace(_entry.languageTag) ? "" : _entry.languageTag.ToUpperInvariant();
            TextMeshProUGUI languageLabel = UiFactory.CreateLabel(info.transform, "LanguageLabel",
                (_entry.languageName ?? "?") + (string.IsNullOrEmpty(languageCode) ? "" : " (" + languageCode + ")"),
                20f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(languageLabel);
            UiFactory.AddLayoutElement(languageLabel.gameObject, 0f, 26f, 0f, 26f, 1f, 0f);

            string teamText = !string.IsNullOrWhiteSpace(_entry.teamName) ? _entry.teamName : (_entry.author ?? "");
            TextMeshProUGUI teamLabel = UiFactory.CreateLabel(info.transform, "TeamLabel",
                string.IsNullOrEmpty(teamText) ? "—" : teamText,
                15f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.SingleLine(teamLabel);
            UiFactory.AddLayoutElement(teamLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);

            string versionText = string.IsNullOrWhiteSpace(_entry.version) ? "—" : "v" + _entry.version;
            TextMeshProUGUI versionLabel = UiFactory.CreateLabel(info.transform, "VersionLabel", versionText, 13f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(versionLabel);
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

            _likeCountLabel = UiFactory.CreateLabel(likesBox.transform, "Count",
                _entry.likes.HasValue ? _entry.likes.Value.ToString() : "—",
                17f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.AddLayoutElement(_likeCountLabel.gameObject, 30f, 20f, 30f, 20f, 0f, 0f);
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

            TextMeshProUGUI section = UiFactory.CreateLabel(panel.transform, "SectionLabel", "Capabilities", 17f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(section.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            TranslationFeatures features = _entry.features ?? new TranslationFeatures();
            CreateFeatureRow(panel.transform, "RowDubbing", TranslationFeatureType.Dubbing, features.dubbing,
                "Replaces in-game voice lines with translated audio.");
            CreateFeatureRow(panel.transform, "RowExtraDubbing", TranslationFeatureType.ExtraDubbing, features.extraDubbing,
                "Adds dubbed audio for optional, non-essential voiced lines.");
            CreateFeatureRow(panel.transform, "RowTextures", TranslationFeatureType.Textures, features.textures,
                "Replaces textures and graphical assets in the game.");
            CreateFeatureRow(panel.transform, "RowFonts", TranslationFeatureType.Fonts, features.fonts,
                "Provides custom or adapted fonts for the language.");
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

            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(info.transform, "Name", StylePalette.FeatureDisplayName(type), 15f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(nameLabel);
            UiFactory.AddLayoutElement(nameLabel.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
            TextMeshProUGUI descLabel = UiFactory.CreateLabel(info.transform, "Description", description, 12f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, true);
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

            TextMeshProUGUI section = UiFactory.CreateLabel(panel.transform, "SectionLabel", "About authors", 17f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(section.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            if (_entry.authors == null || _entry.authors.Count == 0)
            {
                TextMeshProUGUI empty = UiFactory.CreateLabel(panel.transform, "Empty", "No authors listed.", 13f, StylePalette.TextDim, TextAlignmentOptions.Left, true);
                UiFactory.AddLayoutElement(empty.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
                return;
            }

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
                    "Show all authors (" + _entry.authors.Count + ")",
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
            if (!string.IsNullOrWhiteSpace(author.avatarUrl))
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

            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(info.transform, "Name", author.name ?? "?", 15f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(nameLabel);
            UiFactory.AddLayoutElement(nameLabel.gameObject, 0f, 18f, 0f, 18f, 1f, 0f);
            TextMeshProUGUI roleLabel = UiFactory.CreateLabel(info.transform, "Role", author.role ?? "", 12f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(roleLabel);
            UiFactory.AddLayoutElement(roleLabel.gameObject, 0f, 16f, 0f, 16f, 1f, 0f);

            if (!string.IsNullOrWhiteSpace(author.link))
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

        private void CreateReadme(Transform content)
        {
            GameObject panel = CreateSectionPanel(content, "ReadmePanel", 0f);
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>() ?? panel.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI section = UiFactory.CreateLabel(panel.transform, "SectionLabel", "README", 17f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.AddLayoutElement(section.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            if (string.IsNullOrWhiteSpace(_entry.readme))
            {
                TextMeshProUGUI empty = UiFactory.CreateLabel(panel.transform, "Empty", "No description provided by the authors.", 13f, StylePalette.TextDim, TextAlignmentOptions.Left, true);
                UiFactory.AddLayoutElement(empty.gameObject, 0f, 20f, 0f, 20f, 1f, 0f);
                return;
            }

            UiFactory.ScrollViewLayout scroll = UiFactory.CreateScrollView(panel.transform, "ReadmeScroll", new Vector2(0f, ReadmeMaxHeight), StylePalette.Panel);
            RectTransform scrollRect = scroll.Root;
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 0f);
            scrollRect.pivot = new Vector2(0.5f, 0f);
            scrollRect.anchoredPosition = Vector2.zero;
            scrollRect.sizeDelta = new Vector2(0f, ReadmeMaxHeight);
            UiFactory.AddLayoutElement(scroll.Root.gameObject, 0f, ReadmeMaxHeight, 0f, ReadmeMaxHeight, 1f, 0f);

            scroll.Content.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            scroll.Content.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;

            TextMeshProUGUI readmeText = UiFactory.CreateLabel(scroll.Content, "Text", _entry.readme, 13f, StylePalette.TextPrimary, TextAlignmentOptions.TopLeft, true);
            readmeText.richText = true;
            UiFactory.AddLayoutElement(readmeText.gameObject, 0f, 0f, 10f, ReadmeMaxHeight, 1f, 1f);

            RectTransform sbRect = UiFactory.CreateScrollbar(panel.transform, "ReadmeScrollbar", scroll.ScrollRect).GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-2f, 0f);
            sbRect.sizeDelta = new Vector2(10f, 0f);
        }

        /// <summary>Accent download bar pinned under the scroll area (Online tab).</summary>
        public GameObject CreateDownloadBar(Transform parent)
        {
            return CreateActionBar(parent, "Proceed to download", PrimitiveSprites.Kind.Download, delegate
            {
                if (_entry != null && State != null)
                {
                    State.RequestDownload(_entry);
                }
            });
        }

        /// <summary>
        /// Accent action bar pinned under the scroll area. Shared by the Online tab
        /// ("Proceed to download") and the Installed tab ("Use").
        /// </summary>
        public GameObject CreateActionBar(Transform parent, string label, PrimitiveSprites.Kind icon, System.Action onClick)
        {
            Color bg = new Color(StylePalette.AccentRedDark.r, StylePalette.AccentRedDark.g, StylePalette.AccentRedDark.b, 0.85f);

            GameObject buttonObj = UiFactory.CreateButton(parent, "ActionBar", label,
                delegate
                {
                    if (onClick != null)
                    {
                        onClick();
                    }
                },
                new Vector2(0f, 54f),
                bg,
                StylePalette.AccentRed,
                new Color(0.45f, 0.12f, 0.12f, 1f));

            RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0f, 0f);
            btnRect.anchorMax = new Vector2(1f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 0f);
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
    }
}