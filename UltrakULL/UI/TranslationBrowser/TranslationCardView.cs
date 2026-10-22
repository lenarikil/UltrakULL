using System;
using TMPro;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// One card of the translations list: logo + language flag, language name with
    /// code, team/author, version, the four capability indicators, like count and a
    /// "selectable" arrow. Each translation is its own card, even when several
    /// translations target the same language.
    /// </summary>
    public sealed class TranslationCardView
    {
        public const float CardHeight = 132f;

        public GameObject Root;
        public TranslationEntry Entry;
        public Image BackgroundImage;
        public UiFactory.LogoSlot LogoSlot;

        private Image _arrowIcon;
        private TextMeshProUGUI _likesLabel;
        private bool _installedList;

        public static TranslationCardView Create(
            Transform parent,
            TranslationEntry entry,
            Action<TranslationCardView> onClick,
            bool installedList = false)
        {
            if (entry == null)
            {
                return null;
            }

            TranslationCardView card = new TranslationCardView();
            card.Entry = entry;
            card._installedList = installedList;

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

            card.LogoSlot = UiFactory.CreateLogoSlot(headerRow.transform, "LogoSlot", 56f, entry.logoUrl, entry.flagUrl, entry.languageTag);
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

            string languageCode = string.IsNullOrWhiteSpace(entry.languageTag) ? "" : entry.languageTag.ToUpperInvariant();
            TextMeshProUGUI languageLabel = UiFactory.CreateLabel(infoColumn.transform, "LanguageLabel",
                (entry.languageName ?? "?") + (string.IsNullOrEmpty(languageCode) ? "" : " (" + languageCode + ")"),
                17f, StylePalette.TextPrimary, TextAlignmentOptions.BottomLeft, false);
            UiFactory.SingleLine(languageLabel);
            UiFactory.AddLayoutElement(languageLabel.gameObject, 0f, 22f, 0f, 22f, 1f, 0f);

            string teamText = !string.IsNullOrWhiteSpace(entry.teamName) ? entry.teamName : (entry.author ?? "");
            TextMeshProUGUI teamLabel = UiFactory.CreateLabel(infoColumn.transform, "TeamLabel",
                string.IsNullOrEmpty(teamText) ? "—" : teamText,
                14f, StylePalette.TextSecondary, TextAlignmentOptions.MidlineLeft, false);
            UiFactory.SingleLine(teamLabel);
            UiFactory.AddLayoutElement(teamLabel.gameObject, 0f, 18f, 0f, 18f, 1f, 0f);

            string versionText = string.IsNullOrWhiteSpace(entry.version) ? "" : "v" + entry.version;
            TextMeshProUGUI versionLabel = UiFactory.CreateLabel(infoColumn.transform, "VersionLabel",
                string.IsNullOrEmpty(versionText) ? "—" : versionText,
                13f, StylePalette.TextDim, TextAlignmentOptions.TopLeft, false);
            UiFactory.SingleLine(versionLabel);
            UiFactory.AddLayoutElement(versionLabel.gameObject, 0f, 16f, 0f, 16f, 1f, 0f);

            // ---- Row 2: capabilities + likes + arrow ----
            GameObject metaRow = new GameObject("MetaRow", typeof(RectTransform));
            metaRow.transform.SetParent(root.transform, false);
            HorizontalLayoutGroup metaLayout = metaRow.AddComponent<HorizontalLayoutGroup>();
            metaLayout.spacing = 8f;
            metaLayout.childAlignment = TextAnchor.MiddleLeft;
            metaLayout.childControlWidth = true;
            metaLayout.childControlHeight = true;
            metaLayout.childForceExpandWidth = false;
            metaLayout.childForceExpandHeight = true;
            UiFactory.AddLayoutElement(metaRow, 0f, 26f, 0f, 26f, 1f, 0f);

            card.CreateFeatureIndicators(metaRow.transform);

            // "Installed" badge: distinguishes locally installed packs from packs that
            // are only available online (can be downloaded).
            card.CreateInstalledBadge(metaRow.transform);

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

            // Flexible spacer pushes the arrow to the right edge.
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(metaRow.transform, false);
            UiFactory.AddLayoutElement(spacer, 0f, 0f, 0f, 0f, 1f, 0f);

            card._arrowIcon = UiFactory.CreateImage(metaRow.transform, "Arrow", new Vector2(18f, 18f), StylePalette.TextSecondary, PrimitiveSprites.Kind.ArrowRight);
            UiFactory.AddLayoutElement(card._arrowIcon.gameObject, 18f, 18f, 18f, 18f, 0f, 0f);

            UiFactory.ForceLayoutHeights(card.Root.transform);
            return card;
        }

        public void SetSelected(bool selected)
        {
            if (Root == null)
            {
                return;
            }

            if (BackgroundImage != null)
            {
                BackgroundImage.color = selected ? StylePalette.PanelHover : StylePalette.Panel;
            }
            UiFactory.SetBorderColor(Root, selected ? StylePalette.AccentRed : StylePalette.BorderDim);
            if (_arrowIcon != null)
            {
                _arrowIcon.color = selected ? StylePalette.AccentRed : StylePalette.TextSecondary;
            }
        }

        public void Destroy()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
            }
        }

        private void CreateInstalledBadge(Transform parent)
        {
            // In the Installed tab every card is already a local download, so the badge
            // only distinguishes up-to-date from outdated. In the Online tab it reflects
            // whether the specific catalog entry is installed (id match, built-in
            // English excluded).
            bool installed = _installedList || CatalogService.IsInstalled(Entry);
            bool outdated = installed && CatalogService.IsOutdated(Entry);

            // Three states: not installed (Online), installed up to date (Installed),
            // installed but a newer catalog version exists (Update).
            string label = !installed ? "Online" : (outdated ? "Update" : "Installed");
            Color fillColor = installed ? StylePalette.AccentRedDark : StylePalette.PanelAlt;
            Color borderColor = !installed ? StylePalette.BorderBlue : (outdated ? StylePalette.AccentCyan : StylePalette.AccentRed);
            Color textColor = !installed ? StylePalette.TextSecondary : Color.white;

            GameObject badge = UiFactory.CreatePanel(
                parent,
                "InstalledBadge",
                new Vector2(76f, 20f),
                fillColor,
                borderColor,
                1f);
            UiFactory.AddLayoutElement(badge, 76f, 20f, 76f, 20f, 0f, 0f);

            TextMeshProUGUI text = UiFactory.CreateLabel(
                badge.transform,
                "Text",
                label,
                11f,
                textColor,
                TextAlignmentOptions.Center,
                false);
            UiFactory.StretchToParent(text);

            string tooltip;
            if (!installed)
            {
                tooltip = "This translation is available online and can be downloaded.";
            }
            else if (outdated)
            {
                tooltip = "Installed, but a newer version is available in the catalog.";
            }
            else
            {
                tooltip = "This translation is installed locally.";
            }
            TooltipService.Attach(badge, tooltip);
        }

        private void CreateFeatureIndicators(Transform parent)
        {
            const float iconSize = 19f;
            if (Entry.features == null)
            {
                return;
            }

            CreateIndicator(parent, "IndicatorDubbing", TranslationFeatureType.Dubbing, Entry.features.dubbing, iconSize);
            CreateIndicator(parent, "IndicatorExtraDubbing", TranslationFeatureType.ExtraDubbing, Entry.features.extraDubbing, iconSize);
            CreateIndicator(parent, "IndicatorTextures", TranslationFeatureType.Textures, Entry.features.textures, iconSize);
            CreateIndicator(parent, "IndicatorFonts", TranslationFeatureType.Fonts, Entry.features.fonts, iconSize);
        }

        private void CreateIndicator(Transform parent, string name, TranslationFeatureType type, UltrakULL.json.FeatureStatus status, float size)
        {
            FeatureIndicatorView indicator = FeatureIndicator.Create(parent, name, type, status, size, true);
            if (indicator != null && indicator.Root != null)
            {
                UiFactory.AddLayoutElement(indicator.Root, size, size, size, size, 0f, 0f);
            }
        }
    }
}