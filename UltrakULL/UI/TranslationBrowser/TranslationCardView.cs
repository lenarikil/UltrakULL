using System;
using System.Collections.Generic;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// One card of the translations list: logo + language flag, translation name
    /// with locale code, authors, version, the four capability indicators, like
    /// count, an install-status badge and a "selectable" arrow. Each translation
    /// is its own card, even when several translations target the same language.
    ///
    /// All text coming from the (untrusted) catalog is rendered with TMP rich
    /// text disabled so markup in names/descriptions can never inject formatting.
    /// </summary>
    public sealed class TranslationCardView
    {
        public const float CardHeight = 120f;

        public GameObject Root;
        public TranslationEntry Entry;
        public Image BackgroundImage;
        public UiFactory.LogoSlot LogoSlot;
        public InstallResolution Resolution;

        private Image _arrowIcon;
        private TextMeshProUGUI _likesLabel;
        private TextMeshProUGUI _progressLabel;
        private GameObject _progressArea;
        private TextMeshProUGUI _loadErrorLabel;
        private GameObject _firstFeatureIndicator;
        private GameObject _statusBadge;
        private GameObject _featureRow;
        private bool _installedList;
        private List<InstalledPackInfo> _packs;
        private bool _selected;
        private bool _used;

        /// <summary>Root of the first feature indicator (dubbing), for tour targeting.</summary>
        public GameObject FirstFeatureIndicator { get { return _firstFeatureIndicator; } }

        /// <summary>Install-status badge root, for tour targeting (the Updates step).</summary>
        public GameObject StatusBadge { get { return _statusBadge; } }

        /// <summary>
        /// Row that hosts all four capability indicators together, so the tour
        /// can spotlight them with a single highlight instead of one by one.
        /// </summary>
        public GameObject FeatureRow { get { return _featureRow; } }

        public static TranslationCardView Create(
            Transform parent,
            TranslationEntry entry,
            Action<TranslationCardView> onClick,
            bool installedList = false,
            List<InstalledPackInfo> packs = null)
        {
            if (entry == null)
            {
                return null;
            }

            TranslationCardView card = new TranslationCardView();
            card.Entry = entry;
            card._installedList = installedList;
            card._packs = packs;

            GameObject root = UiFactory.CreatePanel(parent, "Card_" + (entry.id ?? "unknown"), new Vector2(0f, CardHeight), StylePalette.Panel, StylePalette.BorderDim, 1f);
            card.Root = root;

            UiFactory.AddLayoutElement(root, -1f, CardHeight, -1f, CardHeight, 1f, 0f);

            card.BackgroundImage = root.GetComponent<Image>();
            card.BackgroundImage.raycastTarget = true;

            Button button = root.AddComponent<Button>();
            button.targetGraphic = card.BackgroundImage;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { if (onClick != null) onClick(card); });

            // Vertical stack of the two rows
            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // ---- Row 1: logo + text ----
            GameObject headerRow = new GameObject("HeaderRow", typeof(RectTransform));
            headerRow.transform.SetParent(root.transform, false);
            HorizontalLayoutGroup headerLayout = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10f;
            headerLayout.childAlignment = TextAnchor.UpperLeft;
            // Control widths so the info column (flexible width) fills the space next to
            // the logo; with childControlWidth = false the column stays at its default
            // width and text wraps into a narrow, overflowing strip.
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = false;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;

            card.LogoSlot = UiFactory.CreateLogoSlot(headerRow.transform, "LogoSlot", 56f, entry.logoUrl, entry.flagUrl, entry.languageTag, FlagProvider.GetSprite(entry.languageTag),
                delegate (bool remoteOk)
                {
                    card.SetLoadError(!remoteOk);
                });
            UiFactory.AddLayoutElement(card.LogoSlot.Root, 56f, 56f, 56f, 56f, 0f, 0f);

            GameObject infoColumn = new GameObject("InfoColumn", typeof(RectTransform));
            infoColumn.transform.SetParent(headerRow.transform, false);
            VerticalLayoutGroup infoLayout = infoColumn.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 2f;
            infoLayout.childAlignment = TextAnchor.UpperLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(infoColumn, 0f, 0f, 10f, 56f, 1f, 1f);
            // Main line: the translation's own name (not the language name).
            string displayName = !string.IsNullOrWhiteSpace(entry.name)
                ? entry.name
                : (!string.IsNullOrWhiteSpace(entry.id) ? entry.id : "?");

            // Wrap the name in a masked viewport so a name that does not fit
            // scrolls back and forth (MarqueeLabel) instead of being clipped.
            GameObject nameArea = new GameObject("NameArea", typeof(RectTransform), typeof(RectMask2D));
            nameArea.transform.SetParent(infoColumn.transform, false);
            RectTransform nameAreaRect = nameArea.GetComponent<RectTransform>();
            nameAreaRect.anchorMin = new Vector2(0f, 0f);
            nameAreaRect.anchorMax = new Vector2(1f, 1f);
            nameAreaRect.pivot = new Vector2(0.5f, 0.5f);
            // The LayoutElement must sit on the viewport (not on the label):
            // ForceLayoutHeights turns on child height control in every layout
            // group of the card, and a child without a LayoutElement collapses
            // to zero height - the name line disappears and the locale code
            // line below reads as the card's title.
            UiFactory.AddLayoutElement(nameArea, 0f, 22f, 10f, 22f, 1f, 0f);

            TextMeshProUGUI nameLabel = UiFactory.CreateLabel(nameArea.transform, "NameLabel",
                Cap(displayName, 200), 17f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
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

            MarqueeLabel nameMarquee = nameArea.AddComponent<MarqueeLabel>();
            nameMarquee.Label = nameLabel;
            nameLabel.richText = false;

            // Second line: the target language's own name ("Русский"), or the
            // short uppercase code when the tag is not a known language.
            string languageText = LanguageDisplay.Name(entry.languageTag);
            TextMeshProUGUI languageLabel = UiFactory.CreateLabel(infoColumn.transform, "LanguageLabel",
                string.IsNullOrEmpty(languageText) ? "—" : languageText,
                14f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.SingleLine(languageLabel);
            languageLabel.richText = false;
            UiFactory.AddLayoutElement(languageLabel.gameObject, 0f, 18f, 0f, 18f, 1f, 0f);

            // Third line: authors, then version below. The built-in English is
            // the original game text, not a translation, so neither authors nor
            // a version may be shown for it (item 10.2).
            if (!entry.isBuiltinEnglish)
            {
                string authorText = Cap(!string.IsNullOrWhiteSpace(entry.author) ? entry.author : (entry.teamName ?? ""), 120);
                TextMeshProUGUI authorLabel = UiFactory.CreateLabel(infoColumn.transform, "AuthorLabel",
                    string.IsNullOrEmpty(authorText) ? "—" : authorText,
                    13f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, false);
                UiFactory.SingleLine(authorLabel);
                authorLabel.richText = false;
                UiFactory.AddLayoutElement(authorLabel.gameObject, 0f, 16f, 0f, 16f, 1f, 0f);

                string versionText = string.IsNullOrWhiteSpace(entry.version) ? "" : InstallStatusResolver.NormalizeVersion(entry.version);
                TextMeshProUGUI versionLabel = UiFactory.CreateLabel(infoColumn.transform, "VersionLabel",
                    string.IsNullOrEmpty(versionText) ? "—" : versionText,
                    13f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
                UiFactory.SingleLine(versionLabel);
                versionLabel.richText = false;
                UiFactory.AddLayoutElement(versionLabel.gameObject, 0f, 16f, 0f, 16f, 1f, 0f);
            }

            // ---- Row 2: capabilities + status + likes + arrow ----
            GameObject metaRow = new GameObject("MetaRow", typeof(RectTransform));
            metaRow.transform.SetParent(root.transform, false);
            HorizontalLayoutGroup metaLayout = metaRow.AddComponent<HorizontalLayoutGroup>();
            metaLayout.spacing = 12f;
            metaLayout.childAlignment = TextAnchor.MiddleLeft;
            metaLayout.childControlWidth = true;
            metaLayout.childControlHeight = true;
            metaLayout.childForceExpandWidth = false;
            metaLayout.childForceExpandHeight = true;
            UiFactory.AddLayoutElement(metaRow, 0f, 26f, 0f, 26f, 1f, 0f);

            card.CreateFeatureIndicators(metaRow.transform);
            card.CreateStatusBadge(metaRow.transform);

            // Likes come from the catalog record; the built-in English has no
            // record and no likes to show (items 10.2 and 14.1).
            if (!entry.isBuiltinEnglish)
            {
                GameObject likesContainer = new GameObject("Likes", typeof(RectTransform));
                likesContainer.transform.SetParent(metaRow.transform, false);
                HorizontalLayoutGroup likesLayout = likesContainer.AddComponent<HorizontalLayoutGroup>();
                likesLayout.spacing = 4f;
                likesLayout.childAlignment = TextAnchor.MiddleLeft;
                likesLayout.childControlWidth = true;
                likesLayout.childControlHeight = false;
                likesLayout.childForceExpandWidth = false;
                likesLayout.childForceExpandHeight = false;
                UiFactory.AddLayoutElement(likesContainer, 26f, 26f, 26f, 26f, 0f, 0f);

                Image likesIcon = UiFactory.CreateImage(likesContainer.transform, "LikesIcon", new Vector2(15f, 15f), StylePalette.TextSecondary, PrimitiveSprites.Kind.Heart);
                UiFactory.AddLayoutElement(likesIcon.gameObject, 15f, 15f, 15f, 15f, 0f, 0f);

                string likesText = entry.likes.HasValue ? entry.likes.Value.ToString() : "—";
                TextMeshProUGUI likesLabel = UiFactory.CreateLabel(likesContainer.transform, "LikesLabel", likesText, 13f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
                UiFactory.AddLayoutElement(likesLabel.gameObject, 20f, 15f, 20f, 15f, 0f, 0f);
                card._likesLabel = likesLabel;
            }

            // Compact per-entry progress (shown only while this entry is being installed).
            GameObject progressArea = new GameObject("ProgressArea", typeof(RectTransform), typeof(RectMask2D));
            progressArea.transform.SetParent(metaRow.transform, false);
            RectTransform progressAreaRect = progressArea.GetComponent<RectTransform>();
            progressAreaRect.anchorMin = new Vector2(0f, 0f);
            progressAreaRect.anchorMax = new Vector2(1f, 1f);
            progressAreaRect.pivot = new Vector2(0f, 0.5f);
            progressAreaRect.anchoredPosition = Vector2.zero;
            progressAreaRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI progressLabel = UiFactory.CreateLabel(progressArea.transform, "ProgressLabel", "", 11f, StylePalette.TextDim, TextAlignmentOptions.MidlineLeft, false);
            progressLabel.richText = false;
            // Ellipsis (not Overflow): the label lives inside the card's
            // RectMask2D and must never render past the card edge (item 10).
            progressLabel.overflowMode = TextOverflowModes.Ellipsis;
            progressLabel.enableWordWrapping = false;
            RectTransform progressLabelRect = progressLabel.GetComponent<RectTransform>();
            progressLabelRect.anchorMin = new Vector2(0f, 0f);
            progressLabelRect.anchorMax = new Vector2(0f, 1f);
            progressLabelRect.pivot = new Vector2(0f, 0.5f);
            progressLabelRect.anchoredPosition = Vector2.zero;
            progressLabelRect.sizeDelta = Vector2.zero;
            progressLabel.gameObject.SetActive(false);
            card._progressLabel = progressLabel;
            card._progressArea = progressArea;
            UiFactory.AddLayoutElement(progressArea, 0f, 15f, 0f, 15f, 1f, 0f);

            // Flexible spacer pushes the arrow to the right edge.
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(metaRow.transform, false);
            UiFactory.AddLayoutElement(spacer, 0f, 0f, 0f, 0f, 1f, 0f);

            card._arrowIcon = UiFactory.CreateImage(metaRow.transform, "Arrow", new Vector2(18f, 18f), StylePalette.TextSecondary, PrimitiveSprites.Kind.ArrowRight);
            UiFactory.AddLayoutElement(card._arrowIcon.gameObject, 18f, 18f, 18f, 18f, 0f, 0f);

            // Warning label shown when a remote logo image fails to download.
            GameObject loadErrorRow = new GameObject("LoadErrorRow", typeof(RectTransform));
            loadErrorRow.transform.SetParent(card.Root.transform, false);
            HorizontalLayoutGroup loadErrorLayout = loadErrorRow.AddComponent<HorizontalLayoutGroup>();
            loadErrorLayout.spacing = 6f;
            loadErrorLayout.childAlignment = TextAnchor.MiddleLeft;
            loadErrorLayout.childControlWidth = true;
            loadErrorLayout.childControlHeight = false;
            loadErrorLayout.childForceExpandWidth = true;
            loadErrorLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(loadErrorRow, 0f, 18f, 0f, 18f, 1f, 0f);

            Image errorIcon = UiFactory.CreateImage(loadErrorRow.transform, "ErrorIcon", new Vector2(14f, 14f), StylePalette.AccentRed, PrimitiveSprites.Kind.Warning);
            UiFactory.AddLayoutElement(errorIcon.gameObject, 14f, 14f, 14f, 14f, 0f, 0f);

            TextMeshProUGUI errorLabel = UiFactory.CreateLabel(loadErrorRow.transform, "ErrorLabel",
                BrowserText.ImageLoadFailed, 12f, StylePalette.TextDim, TextAlignmentOptions.MidlineLeft, false);
            errorLabel.richText = false;
            UiFactory.AddLayoutElement(errorLabel.gameObject, 0f, 15f, 0f, 15f, 1f, 0f);

            card._loadErrorLabel = errorLabel;
            card._loadErrorLabel.transform.parent.gameObject.SetActive(false);

            UiFactory.ForceLayoutHeights(card.Root.transform);
            return card;
        }

        /// <summary>Shows or hides the image-download-failed warning under the card.</summary>
        public void SetLoadError(bool show)
        {
            // The error text is intentionally never shown on the card (item F3):
            // a failed remote logo silently falls back to the flag/diamond instead
            // of cluttering the card with "The image failed to load."
            if (_loadErrorLabel != null)
            {
                _loadErrorLabel.transform.parent.gameObject.SetActive(false);
            }
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyCardState();
        }

        /// <summary>
        /// Marks the card of the pack that is currently in use. The in-use card
        /// keeps its green outline even while selected; selection stays visible
        /// through the red arrow and the hover background.
        /// </summary>
        public void SetUsed(bool used)
        {
            _used = used;
            ApplyCardState();
        }

        private void ApplyCardState()
        {
            if (Root == null)
            {
                return;
            }

            if (BackgroundImage != null)
            {
                BackgroundImage.color = _selected ? StylePalette.PanelHover : StylePalette.Panel;
            }

            Color border = _used
                ? StylePalette.AccentGreen
                : (_selected ? StylePalette.AccentRed : StylePalette.BorderDim);
            UiFactory.SetBorderColor(Root, border);
            if (_arrowIcon != null)
            {
                _arrowIcon.color = _selected ? StylePalette.AccentRed : StylePalette.TextSecondary;
            }
        }

        /// <summary>Shows compact per-entry download/install progress (5.5, 5.6).</summary>
        public void SetInstallingProgress(InstallProgress progress)
        {
            if (_progressLabel == null)
            {
                return;
            }
            if (progress == null)
            {
                ClearInstallingProgress();
                return;
            }

            // Localized stage label plus the percent while the total is known;
            // the stage label alone otherwise (items 1 and 16).
            string stageLabel = BrowserText.InstallStageLabel(progress.Stage);
            string percent = BrowserText.DownloadProgress(progress.BytesRead, progress.TotalBytes);
            _progressLabel.text = percent.Length == 0 ? stageLabel : stageLabel + " " + percent;
            _progressLabel.gameObject.SetActive(true);
        }

        public void ClearInstallingProgress()
        {
            if (_progressLabel != null)
            {
                _progressLabel.text = "";
                _progressLabel.gameObject.SetActive(false);
            }
        }

        public void Destroy()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
            }
        }

        /// <summary>
        /// Install-status badge. On the Online tab the status is resolved against
        /// the local packs (manifest / metadata identity first, locale fallback
        /// second); on the Installed tab every card is by definition installed.
        /// </summary>
        private void CreateStatusBadge(Transform parent)
        {
            if (_installedList)
            {
                Resolution = new InstallResolution { Status = InstallStatus.Installed, Basis = MatchBasis.Manifest };
            }
            else
            {
                Resolution = InstallStatusResolver.Resolve(Entry, _packs);
            }

            InstallStatus status = Resolution.Status;

            string label = BrowserText.InstallStatusLabel(status);
            Color fillColor;
            Color borderColor = StylePalette.StatusColor(status);
            Color textColor;

            switch (status)
            {
                case InstallStatus.Installed:
                case InstallStatus.UpdateAvailable:
                    fillColor = StylePalette.AccentRedDark;
                    textColor = Color.white;
                    break;
                case InstallStatus.LanguageInstalledManual:
                    fillColor = StylePalette.PanelAlt;
                    textColor = StylePalette.TextSecondary;
                    break;
                case InstallStatus.NotInstalled:
                    fillColor = StylePalette.PanelAlt;
                    textColor = StylePalette.TextSecondary;
                    break;
                default:
                    fillColor = StylePalette.PanelAlt;
                    textColor = StylePalette.TextDim;
                    break;
            }

            GameObject badge = UiFactory.CreatePanel(
                parent,
                "InstalledBadge",
                new Vector2(100f, 24f),
                fillColor,
                borderColor,
                1f);
            UiFactory.AddLayoutElement(badge, 100f, 24f, 100f, 24f, 0f, 0f);
            _statusBadge = badge;

            // The label sits in its own masked text area: a status that does not
            // fit the fixed badge scrolls back and forth (MarqueeLabel) instead
            // of being ellipsized into an unreadable fragment.
            GameObject textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(badge.transform, false);
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.pivot = new Vector2(0.5f, 0.5f);
            textAreaRect.offsetMin = new Vector2(5f, 0f);
            textAreaRect.offsetMax = new Vector2(-5f, 0f);

            TextMeshProUGUI text = UiFactory.CreateLabel(
                textArea.transform,
                "Text",
                label,
                11f,
                textColor,
                TextAlignmentOptions.MidlineLeft,
                false);
            // Overflow (not Ellipsis) so the full text renders past the rect edge
            // and the marquee can slide it; the RectMask2D does the clipping.
            text.overflowMode = TextOverflowModes.Overflow;
            text.enableWordWrapping = false;
            text.richText = false;
            // Left-anchored rect: the single line starts at the rect's left edge,
            // so MarqueeLabel shifts it purely through anchoredPosition.x.
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            MarqueeLabel marquee = badge.AddComponent<MarqueeLabel>();
            marquee.Label = text;

            TooltipService.Attach(badge, BuildStatusTooltip(status));
        }

        private string BuildStatusTooltip(InstallStatus status)
        {
            string basis = BrowserText.BasisLabel(Resolution.Basis);
            string line = BrowserText.LegendStatusLabel(status);
            if (!string.IsNullOrEmpty(basis))
            {
                line += "\n(" + basis + ")";
            }

            if (status == InstallStatus.UpdateAvailable &&
                !string.IsNullOrEmpty(Resolution.InstalledVersion))
            {
                line += "\n" + BrowserText.VersionUpdateFromTo(Resolution.InstalledVersion, Entry.version);
            }
            return line;
        }

        private void CreateFeatureIndicators(Transform parent)
        {
            const float iconSize = 19f;
            if (Entry.features == null)
            {
                return;
            }

            // All four indicators share one tight sub-row so the product tour
            // can highlight the whole group with a single frame.
            GameObject row = new GameObject("FeatureRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 4f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            _featureRow = row;

            CreateIndicator(row.transform, "IndicatorDubbing", TranslationFeatureType.Dubbing, Entry.features.dubbing, iconSize);
            CreateIndicator(row.transform, "IndicatorExtraDubbing", TranslationFeatureType.ExtraDubbing, Entry.features.extraDubbing, iconSize);
            CreateIndicator(row.transform, "IndicatorTextures", TranslationFeatureType.Textures, Entry.features.textures, iconSize);
            CreateIndicator(row.transform, "IndicatorFonts", TranslationFeatureType.Fonts, Entry.features.fonts, iconSize);

            float rowWidth = iconSize * 4f + rowLayout.spacing * 3f;
            UiFactory.AddLayoutElement(row, rowWidth, iconSize, rowWidth, iconSize, 0f, 0f);
        }

        private void CreateIndicator(Transform parent, string name, TranslationFeatureType type, UltrakULL.json.FeatureStatus status, float size)
        {
            FeatureIndicatorView indicator = FeatureIndicator.Create(parent, name, type, status, size, true);
            if (indicator != null && indicator.Root != null)
            {
                if (_firstFeatureIndicator == null)
                {
                    _firstFeatureIndicator = indicator.Root;
                }
                UiFactory.AddLayoutElement(indicator.Root, size, size, size, size, 0f, 0f);
            }
        }

        private static string Cap(string value, int max)
        {
            if (value == null || value.Length <= max)
            {
                return value;
            }
            return value.Substring(0, max);
        }
    }
}
