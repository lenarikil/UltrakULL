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

            // Neutral placeholder for "no images"
            GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform));
            placeholder.transform.SetParent(root.transform, false);
            UiFactory.StretchToParent(placeholder, 10f);
            Image placeholderIcon = UiFactory.CreateImage(placeholder.transform, "Icon", new Vector2(44f, 44f), StylePalette.TextDim, PrimitiveSprites.Kind.Image);
            placeholderIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
            placeholderIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
            placeholderIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            placeholderIcon.rectTransform.anchoredPosition = Vector2.zero;
            TextMeshProUGUI label = UiFactory.CreateLabel(placeholder.transform, "Label", "No preview images provided", 13f, StylePalette.TextDim, TextAlignmentOptions.Center, true);
            UiFactory.AddLayoutElement(label.gameObject, 0f, 0f, 200f, 20f, 0f, 0f);
            label.rectTransform.anchorMin = new Vector2(0.5f, 0.25f);
            label.rectTransform.anchorMax = new Vector2(0.5f, 0.25f);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchoredPosition = Vector2.zero;
            gallery.Placeholder = placeholder;

            // Previous/next buttons (created hidden; enabled as needed)
            gallery._prevButton = UiFactory.CreateButton(root.transform, "PrevButton", "", gallery.Prev, new Vector2(26f, 40f), new Color(0f, 0f, 0f, 0.55f), StylePalette.BorderBlue, new Color(0.1f, 0.15f, 0.25f, 0.75f));
            RectTransform prevRect = gallery._prevButton.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(0f, 0.5f);
            prevRect.anchorMax = new Vector2(0f, 0.5f);
            prevRect.pivot = new Vector2(0f, 0.5f);
            prevRect.anchoredPosition = new Vector2(6f, 0f);
            Image prevIcon = UiFactory.CreateImage(gallery._prevButton.transform, "Icon", new Vector2(14f, 14f), Color.white, PrimitiveSprites.Kind.ArrowRight);
            prevIcon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            prevIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            prevIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            prevIcon.rectTransform.anchoredPosition = Vector2.zero;
            prevIcon.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

            gallery._nextButton = UiFactory.CreateButton(root.transform, "NextButton", "", gallery.Next, new Vector2(26f, 40f), new Color(0f, 0f, 0f, 0.55f), StylePalette.BorderBlue, new Color(0.1f, 0.15f, 0.25f, 0.75f));
            RectTransform nextRect = gallery._nextButton.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(1f, 0.5f);
            nextRect.anchorMax = new Vector2(1f, 0.5f);
            nextRect.pivot = new Vector2(1f, 0.5f);
            nextRect.anchoredPosition = new Vector2(-6f, 0f);
            Image nextIcon = UiFactory.CreateImage(gallery._nextButton.transform, "Icon", new Vector2(14f, 14f), Color.white, PrimitiveSprites.Kind.ArrowRight);
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

            if (_stage != null)
            {
                _stage.gameObject.SetActive(hasImages);
                _stage.sprite = null;
                _stage.color = Color.white;
                if (hasImages)
                {
                    string url = _urls[Mathf.Clamp(_index, 0, _urls.Count - 1)];
                    ImageCache.GetOrLoad(url, delegate (Sprite sprite)
                    {
                        if (_stage == null)
                        {
                            return;
                        }
                        if (sprite != null)
                        {
                            _stage.sprite = sprite;
                            _stage.color = Color.white;
                        }
                        else
                        {
                            // Failed to load the current image: show a dim neutral box.
                            _stage.sprite = UiFactory.WhiteSprite;
                            _stage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
                        }
                    });
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
    }
}