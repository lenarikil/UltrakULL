using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Preview gallery of a translation. Handles 0 images (neutral placeholder),
    /// exactly 1 image (no navigation controls) and N images (arrow buttons + a
    /// position indicator). Only previews; never downloads anything.
    /// </summary>
    public sealed class GalleryView
    {
        public GameObject Root;

        private List<string> _urls = new List<string>();
        private int _index;
        private Image _stage;
        private Image _spinner;
        private GameObject Placeholder;
        private GameObject _prevButton;
        private GameObject _nextButton;
        private TextMeshProUGUI _counter;

        public static GalleryView Create(Transform parent, string name, Vector2 size)
        {
            GalleryView gallery = new GalleryView();
            GameObject root = UiFactory.CreatePanel(parent, name, size, StylePalette.PanelAlt, StylePalette.BorderDim, 1f);
            gallery.Root = root;

            // Stage image
            Image stage = UiFactory.CreateImage(root.transform, "Stage", Vector2.zero, Color.white);
            UiFactory.StretchToParent(stage, 10f);
            stage.preserveAspect = true;
            stage.raycastTarget = false;
            gallery._stage = stage;

            // Sweep loader shown while a preview image is pending (item 10).
            Image spinner = LoadingSpinner.Create(root.transform, "Spinner", new Vector2(28f, 28f), StylePalette.TextDim);
            spinner.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            spinner.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            spinner.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            spinner.rectTransform.anchoredPosition = Vector2.zero;
            spinner.rectTransform.sizeDelta = new Vector2(28f, 28f);
            spinner.gameObject.SetActive(false);
            gallery._spinner = spinner;

            // Neutral placeholder for "no images"
            GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform));
            placeholder.transform.SetParent(root.transform, false);
            UiFactory.StretchToParent(placeholder, 10f);
            Image placeholderIcon = UiFactory.CreateImage(placeholder.transform, "Icon", new Vector2(44f, 44f), StylePalette.TextDim, PrimitiveSprites.Kind.Image);
            placeholderIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
            placeholderIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
            placeholderIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            placeholderIcon.rectTransform.anchoredPosition = Vector2.zero;
            TextMeshProUGUI label = UiFactory.CreateLabel(placeholder.transform, "Label", BrowserText.GalleryEmpty, 13f, StylePalette.TextDim, TextAlignmentOptions.Center, true);
            label.richText = false;
            UiFactory.AddLayoutElement(label.gameObject, 0f, 0f, 200f, 20f, 0f, 0f);
            label.rectTransform.anchorMin = new Vector2(0.5f, 0.25f);
            label.rectTransform.anchorMax = new Vector2(0.5f, 0.25f);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchoredPosition = Vector2.zero;
            gallery.Placeholder = placeholder;

            // Previous/next buttons (created hidden; enabled as needed)
            gallery._prevButton = UiFactory.CreateButton(root.transform, "PrevButton", "", gallery.Prev, new Vector2(40f, 40f), StylePalette.GalleryNavBg, StylePalette.BorderBlue, StylePalette.GalleryNavHover);
            RectTransform prevRect = gallery._prevButton.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(0f, 0.5f);
            prevRect.anchorMax = new Vector2(0f, 0.5f);
            prevRect.pivot = new Vector2(0f, 0.5f);
            prevRect.anchoredPosition = new Vector2(6f, 0f);
            Image prevIcon = UiFactory.CreateImage(gallery._prevButton.transform, "Icon", new Vector2(16f, 16f), Color.white, PrimitiveSprites.Kind.ArrowRight);
            prevIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            prevIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            prevIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            prevIcon.rectTransform.anchoredPosition = Vector2.zero;
            prevIcon.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

            gallery._nextButton = UiFactory.CreateButton(root.transform, "NextButton", "", gallery.Next, new Vector2(40f, 40f), StylePalette.GalleryNavBg, StylePalette.BorderBlue, StylePalette.GalleryNavHover);
            RectTransform nextRect = gallery._nextButton.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(1f, 0.5f);
            nextRect.anchorMax = new Vector2(1f, 0.5f);
            nextRect.pivot = new Vector2(1f, 0.5f);
            nextRect.anchoredPosition = new Vector2(-6f, 0f);
            Image nextIcon = UiFactory.CreateImage(gallery._nextButton.transform, "Icon", new Vector2(16f, 16f), Color.white, PrimitiveSprites.Kind.ArrowRight);
            nextIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            nextIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            nextIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            nextIcon.rectTransform.anchoredPosition = Vector2.zero;

            // Position counter
            TextMeshProUGUI counter = UiFactory.CreateLabel(root.transform, "Counter", "", 12f, StylePalette.TextSecondary, TextAlignmentOptions.Center, false);
            RectTransform counterRect = counter.GetComponent<RectTransform>();
            counterRect.anchorMin = new Vector2(0.5f, 0f);
            counterRect.anchorMax = new Vector2(0.5f, 0f);
            counterRect.pivot = new Vector2(0.5f, 0f);
            counterRect.anchoredPosition = new Vector2(0f, -22f);
            counterRect.sizeDelta = new Vector2(120f, 16f);
            gallery._counter = counter;

            gallery.SetUrls(null);
            return gallery;
        }

        public void SetUrls(List<string> urls)
        {
            _urls = urls ?? new List<string>();
            _index = 0;
            Refresh();
        }

        public void Next()
        {
            if (_urls.Count <= 1)
            {
                return;
            }
            _index = (_index + 1) % _urls.Count;
            Refresh();
        }

        public void Prev()
        {
            if (_urls.Count <= 1)
            {
                return;
            }
            _index = (_index - 1 + _urls.Count) % _urls.Count;
            Refresh();
        }

        private void Refresh()
        {
            bool hasImages = _urls != null && _urls.Count > 0;

            if (Placeholder != null)
            {
                Placeholder.SetActive(!hasImages);
            }

            if (_spinner != null)
            {
                _spinner.gameObject.SetActive(hasImages);
            }

            if (_stage != null)
            {
                _stage.gameObject.SetActive(hasImages);
                if (hasImages)
                {
                    // Item 11: do not show any placeholder while loading - only the spinner.
                    _stage.sprite = null;
                    _stage.color = new Color(1f, 1f, 1f, 0f);
                    _spinner.gameObject.SetActive(true);
                    string url = _urls[Mathf.Clamp(_index, 0, _urls.Count - 1)];

                    // Local file paths (installed packs, from the pack manifest)
                    // load straight from disk; remote URLs go through ImageCache
                    // (which rejects anything that is not http(s), item 15).
                    bool isLocal = !url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase)
                        && !url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase);
                    if (isLocal)
                    {
                        _spinner.gameObject.SetActive(false);
                        Sprite localSprite = UiFactory.LoadLocalSprite(url);
                        if (localSprite != null)
                        {
                            _stage.sprite = localSprite;
                            _stage.color = Color.white;
                        }
                        else
                        {
                            ShowFailureBox();
                        }
                    }
                    else
                    {
                        ImageCache.GetOrLoad(url, delegate (Sprite sprite)
                        {
                            if (_stage == null || _spinner == null)
                            {
                                return;
                            }
                            _spinner.gameObject.SetActive(false);
                            if (sprite != null)
                            {
                                _stage.sprite = sprite;
                                _stage.color = Color.white;
                            }
                            else
                            {
                                ShowFailureBox();
                            }
                        });
                    }
                }
                else
                {
                    _stage.sprite = null;
                    _spinner.gameObject.SetActive(false);
                }
            }

            if (_prevButton != null)
            {
                _prevButton.SetActive(hasImages && _urls.Count > 1);
            }
            if (_nextButton != null)
            {
                _nextButton.SetActive(hasImages && _urls.Count > 1);
            }
            if (_counter != null)
            {
                if (hasImages)
                {
                    _counter.text = _urls.Count > 1
                        ? (_index + 1) + " / " + _urls.Count
                        : "1 / 1";
                }
                else
                {
                    _counter.text = "";
                }
            }
        }

        /// <summary>Failed to load the current image: show a dim neutral box.</summary>
        private void ShowFailureBox()
        {
            if (_stage == null)
            {
                return;
            }
            _stage.sprite = UiFactory.WhiteSprite;
            _stage.color = StylePalette.GalleryFailure;
        }
    }
}