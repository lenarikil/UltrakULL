using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Animated product tour over the translation browser (item 13). A scrim
    /// dims everything except a hole around the current step's target, the
    /// card explains the feature and holds the tour's own controls; every
    /// scrim part is a raycast target, so the interface below stays blocked
    /// until the tour is finished or skipped.
    ///
    /// The seven steps cover the online catalog, the preview, the download,
    /// the installed list, the language switch, the update notifications and
    /// the repeat option. Finishing or skipping restores the tab the user was
    /// on; nothing else is touched.
    /// </summary>
    public sealed class ProductTour : MonoBehaviour
    {
        /// <summary>One tour step: what to spotlight and what to say.</summary>
        public sealed class TourStep
        {
            public string Title;
            public string Body;

            /// <summary>Target to spotlight. Null shows a centered card without a hole.</summary>
            public Func<GameObject> Target;

            /// <summary>Runs before the step appears (e.g. switches the browser tab).</summary>
            public Action Prepare;
        }

        private const float CardWidth = 360f;
        /// <summary>Minimum distance between the card and the screen edge.</summary>
        private const float ScreenMargin = 10f;
        private const float CardSlideDistance = 24f;
        private const float CardFadeSeconds = 0.18f;

        private List<TourStep> _steps;
        private Action _onFinished;

        private RectTransform _overlayRect;
        private Image _dimTop;
        private Image _dimBottom;
        private Image _dimLeft;
        private Image _dimRight;
        private Image _holeFrame;

        private RectTransform _cardRect;
        private CanvasGroup _cardGroup;
        private TextMeshProUGUI _cardTitle;
        private TextMeshProUGUI _cardBody;
        private TextMeshProUGUI _cardCounter;
        private GameObject _backButton;
        private GameObject _nextButton;
        private TextMeshProUGUI _nextLabel;

        // Pulse border: a second frame that expands outward from the hole and
        // fades out, drawing attention to the spotlighted target.
        private Image _pulseFrame;
        private Coroutine _pulseAnimation;

        private int _index;
        private Coroutine _animation;
        private bool _finished;

        public static ProductTour Create(Transform browserRoot, List<TourStep> steps, Action onFinished)
        {
            GameObject overlay = new GameObject("ProductTour", typeof(RectTransform));
            overlay.transform.SetParent(browserRoot, false);

            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            ProductTour tour = overlay.AddComponent<ProductTour>();
            tour._steps = steps != null ? steps : new List<TourStep>();
            tour._onFinished = onFinished;
            tour.Build();
            tour.ShowStep(0);
            return tour;
        }

        private void Build()
        {
            _overlayRect = (RectTransform)transform;

            _dimTop = CreateDim("DimTop");
            _dimBottom = CreateDim("DimBottom");
            _dimLeft = CreateDim("DimLeft");
            _dimRight = CreateDim("DimRight");

            // The frame both highlights the hole and blocks interaction with
            // the spotlighted element: its image raycasts over the whole rect
            // even though the center is transparent.
            GameObject frame = new GameObject("HoleFrame", typeof(RectTransform));
            frame.transform.SetParent(transform, false);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.localScale = Vector3.one;
            _holeFrame = frame.AddComponent<Image>();
            if (NativeUi.IsReady)
            {
                NativeUi.ApplySliced(_holeFrame, NativeUi.BorderSmall, StylePalette.AccentRed);
            }
            else
            {
                _holeFrame.sprite = null;
                _holeFrame.color = new Color(0f, 0f, 0f, 0f);
                UiFactory.AddBorder(frame.transform, StylePalette.AccentRed, 2f);
            }
            _holeFrame.raycastTarget = true;

            // Pulse frame: starts at the hole size, expands outward and fades
            // out to draw attention to the spotlighted target.
            GameObject pulseFrameGo = new GameObject("PulseFrame", typeof(RectTransform));
            pulseFrameGo.transform.SetParent(transform, false);
            RectTransform pulseRect = pulseFrameGo.GetComponent<RectTransform>();
            pulseRect.localScale = Vector3.one;
            _pulseFrame = pulseFrameGo.AddComponent<Image>();
            if (NativeUi.IsReady)
            {
                NativeUi.ApplySliced(_pulseFrame, NativeUi.BorderSmall, StylePalette.AccentRed);
            }
            else
            {
                _pulseFrame.sprite = null;
                _pulseFrame.color = new Color(0f, 0f, 0f, 0f);
                UiFactory.AddBorder(pulseFrameGo.transform, StylePalette.AccentRed, 2f);
            }
            _pulseFrame.raycastTarget = false;
            _pulseFrame.gameObject.SetActive(false);

            BuildCard();
        }

        private Image CreateDim(string name)
        {
            GameObject dim = new GameObject(name, typeof(RectTransform));
            dim.transform.SetParent(transform, false);
            RectTransform rect = dim.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.localScale = Vector3.one;
            Image image = dim.AddComponent<Image>();
            image.sprite = UiFactory.WhiteSprite;
            image.color = StylePalette.Scrim;
            // Every dim part blocks the interface under it.
            image.raycastTarget = true;
            return image;
        }

        private void BuildCard()
        {
            GameObject card = UiFactory.CreatePanel(transform, "Card", new Vector2(CardWidth, 200f), StylePalette.Panel, StylePalette.AccentCyan, 2f);
            _cardRect = card.GetComponent<RectTransform>();
            _cardRect.anchorMin = Vector2.zero;
            _cardRect.anchorMax = Vector2.zero;
            _cardRect.pivot = new Vector2(0f, 0f);
            _cardRect.localScale = Vector3.one;
            _cardGroup = card.AddComponent<CanvasGroup>();

            VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            // Right padding must match the left one: with 0 the button row
            // stretches flush to the card's right border and the Next button
            // visually crosses the frame.
            layout.padding = new RectOffset(14, 14, 10, 10);
            layout.childAlignment = TextAnchor.UpperLeft;
            // The layout group must control child heights: with childControlHeight
            // left false, every label keeps its default 100x100 sizeDelta and
            // the content overflows the card frame (buttons far below the
            // description, everything drifting off the card).
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _cardTitle = UiFactory.CreateLabel(card.transform, "Title", "", 17f, StylePalette.TextPrimary, TextAlignmentOptions.Left, false);
            UiFactory.SingleLine(_cardTitle);
            UiFactory.AddLayoutElement(_cardTitle.gameObject, -1f, 22f, -1f, 22f, 0f, 0f);

            _cardBody = UiFactory.CreateLabel(card.transform, "Body", "", 13f, StylePalette.TextSecondary, TextAlignmentOptions.TopLeft, true);
            _cardBody.richText = false;
            _cardBody.enableWordWrapping = true;
            // No explicit height: the layout group sizes the body to its
            // wrapped preferred height, so the counter and buttons sit right
            // under the description.
            UiFactory.AddLayoutElement(_cardBody.gameObject, -1f, -1f, -1f, -1f, 1f, 0f);

            _cardCounter = UiFactory.CreateLabel(card.transform, "Counter", "", 12f, StylePalette.TextDim, TextAlignmentOptions.Left, false);
            UiFactory.SingleLine(_cardCounter);
            UiFactory.AddLayoutElement(_cardCounter.gameObject, -1f, 16f, -1f, 16f, 0f, 0f);

            GameObject buttonRow = new GameObject("ButtonRow", typeof(RectTransform));
            buttonRow.transform.SetParent(card.transform, false);
            HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 6f;
            rowLayout.childAlignment = TextAnchor.MiddleRight;
            // Layout-controlled widths: the layout group clamps every child
            // into the available row width, so the buttons (Next included)
            // can never cross the card's right border.
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            UiFactory.AddLayoutElement(buttonRow, 0f, 32f, 0f, 32f, 1f, 0f);

            _backButton = UiFactory.CreateButton(buttonRow.transform, "Back", BrowserText.TourBack, GoBack,
                new Vector2(80f, 32f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(_backButton, 80f, 32f, 80f, 32f, 0f, 0f);

            GameObject skipButton = UiFactory.CreateButton(buttonRow.transform, "Skip", BrowserText.TourSkip, Finish,
                new Vector2(80f, 32f), StylePalette.PanelAlt, StylePalette.BorderBlue, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(skipButton, 80f, 32f, 80f, 32f, 0f, 0f);

            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(buttonRow.transform, false);
            UiFactory.AddLayoutElement(spacer, 0f, 0f, 0f, 0f, 1f, 0f);

            _nextButton = UiFactory.CreateButton(buttonRow.transform, "Next", BrowserText.TourNext, GoNext,
                new Vector2(110f, 32f), StylePalette.PanelAlt, StylePalette.AccentRed, StylePalette.PanelHover);
            UiFactory.AddLayoutElement(_nextButton, 110f, 32f, 110f, 32f, 0f, 0f);
            _nextLabel = _nextButton.transform.Find("Label") != null
                ? _nextButton.transform.Find("Label").GetComponent<TextMeshProUGUI>()
                : null;
        }

        // ---------------------------------------------------------------------
        // Step flow
        // ---------------------------------------------------------------------

        private void ShowStep(int index)
        {
            if (_steps == null || _steps.Count == 0)
            {
                Finish();
                return;
            }

            _index = Mathf.Clamp(index, 0, _steps.Count - 1);
            TourStep step = _steps[_index];

            if (step.Prepare != null)
            {
                try
                {
                    step.Prepare();
                }
                catch (Exception e)
                {
                    Logging.Warn("[ProductTour] Step prepare failed: " + e.Message);
                }
            }

            StopAnimation();
            StartCoroutine(StepRoutine(step));
        }

        private IEnumerator StepRoutine(TourStep step)
        {
            // Let the Prepare actions (tab switches, layout) settle before the
            // target rect is measured.
            yield return null;
            yield return null;

            GameObject target = null;
            try
            {
                if (step.Target != null)
                {
                    target = step.Target();
                }
            }
            catch (Exception e)
            {
                Logging.Warn("[ProductTour] Step target failed: " + e.Message);
            }

            Rect hole = target != null ? GetTargetLocalRect(target) : new Rect(0f, 0f, 0f, 0f);
            PositionScrim(hole);
            StartPulseBorder(hole);

            // Card content.
            _cardTitle.text = step.Title ?? "";
            _cardBody.text = step.Body ?? "";
            _cardCounter.text = (_index + 1) + " / " + _steps.Count;

            bool last = _index == _steps.Count - 1;
            if (_nextLabel != null)
            {
                _nextLabel.text = last ? BrowserText.TourFinish : BrowserText.TourNext;
            }
            _backButton.SetActive(_index > 0);

            // Card size: the layout group controls child heights, so measure the
            // body's wrapped height at the card's width and size the card to
            // exactly fit the content (title + body + counter + buttons).
            float overlayWidth = _overlayRect.rect.width;
            float overlayHeight = _overlayRect.rect.height;

            float cardWidth = Mathf.Clamp(CardWidth, 240f, Mathf.Max(240f, overlayWidth - ScreenMargin * 2f));

            _cardRect.sizeDelta = new Vector2(cardWidth, 160f);
            Canvas.ForceUpdateCanvases();

            // Padding top(10) + title(22) + spacing(6) + body + spacing(6) +
            // counter(16) + spacing(6) + button row(32) + padding bottom(10).
            const float cardChrome = 108f;
            float bodyHeight = Mathf.Max(36f, _cardBody.preferredHeight);
            float cardHeight = bodyHeight + cardChrome;

            // Never let the card grow past the visible area: if the description
            // is too long, clamp the card and truncate the body with an
            // ellipsis so the buttons always stay inside the screen.
            float maxCardHeight = Mathf.Max(160f, overlayHeight - ScreenMargin * 2f);
            if (cardHeight > maxCardHeight)
            {
                cardHeight = maxCardHeight;
                UiFactory.AddLayoutElement(_cardBody.gameObject, -1f, -1f, -1f, Mathf.Max(24f, cardHeight - cardChrome), 1f, 0f);
                _cardBody.overflowMode = TextOverflowModes.Ellipsis;
                Canvas.ForceUpdateCanvases();
            }
            else
            {
                UiFactory.AddLayoutElement(_cardBody.gameObject, -1f, -1f, -1f, -1f, 1f, 0f);
                _cardBody.overflowMode = TextOverflowModes.Overflow;
            }

            _cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);

            Vector2 cardPosition = FindCardPosition(hole, cardWidth, cardHeight);

            // The card slides in from slightly below its final position.
            Vector2 animatedFrom = cardPosition + new Vector2(0f, -CardSlideDistance);
            _cardGroup.alpha = 0f;
            _animation = StartCoroutine(AnimateCardIn(animatedFrom, cardPosition));
        }

        private void PositionScrim(Rect hole)
        {
            float width = _overlayRect.rect.width;
            float height = _overlayRect.rect.height;

            hole.xMin = Mathf.Clamp(hole.xMin, 0f, width);
            hole.xMax = Mathf.Clamp(hole.xMax, 0f, width);
            hole.yMin = Mathf.Clamp(hole.yMin, 0f, height);
            hole.yMax = Mathf.Clamp(hole.yMax, 0f, height);

            bool hasHole = hole.width > 1f && hole.height > 1f;

            PlaceRect(_dimTop.GetComponent<RectTransform>(), 0f, hole.yMax, width, Mathf.Max(0f, height - hole.yMax));
            PlaceRect(_dimBottom.GetComponent<RectTransform>(), 0f, 0f, width, hole.yMin);
            PlaceRect(_dimLeft.GetComponent<RectTransform>(), 0f, hole.yMin, hole.xMin, hole.height);
            PlaceRect(_dimRight.GetComponent<RectTransform>(), hole.xMax, hole.yMin, Mathf.Max(0f, width - hole.xMax), hole.height);

            _holeFrame.gameObject.SetActive(hasHole);
            if (hasHole)
            {
                PlaceRect(_holeFrame.GetComponent<RectTransform>(), hole.xMin - 2f, hole.yMin - 2f, hole.width + 4f, hole.height + 4f);
            }
        }

        private void PlaceRect(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(Mathf.Max(0f, w), Mathf.Max(0f, h));
        }

        /// <summary>
        /// Starts the pulsing border animation around the spotlighted hole:
        /// a copy of the frame expands outward from the center of the original
        /// border while fading out, then repeats continuously.
        /// </summary>
        private void StartPulseBorder(Rect hole)
        {
            StopPulseBorder();
            if (hole.width <= 1f || hole.height <= 1f)
            {
                return;
            }

            _pulseFrame.gameObject.SetActive(true);
            // Center the pulse frame exactly on the hole center so scale
            // expansion radiates outward from the center, not a corner.
            float cx = (hole.xMin + hole.xMax) * 0.5f;
            float cy = (hole.yMin + hole.yMax) * 0.5f;
            PlaceRect(_pulseFrame.GetComponent<RectTransform>(),
                cx - 2f, cy - 2f, hole.width + 4f, hole.height + 4f);
            RectTransform pulseRect = _pulseFrame.GetComponent<RectTransform>();
            pulseRect.pivot = new Vector2(0.5f, 0.5f);
            _pulseFrame.color = new Color(1f, 1f, 1f, 0.85f);
            _pulseFrame.transform.localScale = Vector3.one;
            _pulseAnimation = StartCoroutine(AnimatePulseBorder());
        }

        private void StopPulseBorder()
        {
            if (_pulseAnimation != null)
            {
                StopCoroutine(_pulseAnimation);
                _pulseAnimation = null;
            }
            _pulseFrame.gameObject.SetActive(false);
        }

        private IEnumerator AnimatePulseBorder()
        {
            const float duration = 1.0f;
            while (true)
            {
                float t = 0f;
                Vector3 startScale = Vector3.one;
                Vector3 endScale = new Vector3(1.12f, 1.12f, 1f);
                Color startColor = new Color(1f, 1f, 1f, 0.85f);
                Color endColor = new Color(1f, 1f, 1f, 0f);

                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = t / duration;
                    float smooth = Mathf.SmoothStep(0f, 1f, k);
                    _pulseFrame.transform.localScale = Vector3.Lerp(startScale, endScale, smooth);
                    _pulseFrame.color = Color.Lerp(startColor, endColor, smooth);
                    yield return null;
                }

                _pulseFrame.transform.localScale = endScale;
                _pulseFrame.color = endColor;
            }
        }

        /// <summary>
        /// The target's rect in the overlay's local space. The overlay's pivot
        /// is the bottom-left corner, so the local coordinates directly work
        /// as bottom-left based placement values.
        /// </summary>
        private Rect GetTargetLocalRect(GameObject target)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            Vector3 min = transform.InverseTransformPoint(corners[0]);
            Vector3 max = transform.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>
        /// Places the card below the hole, above it when there is no room
        /// below, and always clamps it inside the browser so no hint ever
        /// leaves the screen.
        /// </summary>
        private Vector2 FindCardPosition(Rect hole, float cardWidth, float cardHeight)
        {
            float width = _overlayRect.rect.width;
            float height = _overlayRect.rect.height;
            float margin = ScreenMargin;

            float x;
            if (hole.width > 1f && hole.xMin + hole.width * 0.5f < width * 0.5f)
            {
                // Target sits on the left: the card goes to its right.
                x = hole.xMax + margin;
            }
            else if (hole.width > 1f)
            {
                // Target sits on the right: the card goes to its left.
                x = hole.xMin - cardWidth - margin;
            }
            else
            {
                x = (width - cardWidth) * 0.5f;
            }

            float y = -1f;
            if (hole.height > 1f && hole.yMax + margin + cardHeight <= height)
            {
                y = hole.yMax + margin;
            }
            else if (hole.height > 1f && hole.yMin - margin - cardHeight >= 0f)
            {
                y = hole.yMin - margin - cardHeight;
            }

            if (y < 0f)
            {
                y = (height - cardHeight) * 0.5f;
            }

            // Clamp inside the visible area; the card may be narrower than
            // CardWidth on small screens, so clamp against the real width.
            float maxX = Mathf.Max(margin, width - cardWidth - margin);
            float maxY = Mathf.Max(margin, height - cardHeight - margin);
            x = Mathf.Clamp(x, margin, maxX);
            y = Mathf.Clamp(y, margin, maxY);
            return new Vector2(x, y);
        }

        private IEnumerator AnimateCardIn(Vector2 from, Vector2 to)
        {
            float t = 0f;
            while (t < CardFadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / CardFadeSeconds);
                _cardRect.anchoredPosition = Vector2.Lerp(from, to, k);
                _cardGroup.alpha = k;
                yield return null;
            }

            _cardRect.anchoredPosition = to;
            _cardGroup.alpha = 1f;
            _animation = null;
        }

        private void StopAnimation()
        {
            if (_animation != null)
            {
                StopCoroutine(_animation);
                _animation = null;
            }
            StopPulseBorder();
        }

        private void GoNext()
        {
            if (_index < _steps.Count - 1)
            {
                ShowStep(_index + 1);
            }
            else
            {
                Finish();
            }
        }

        private void GoBack()
        {
            if (_index > 0)
            {
                ShowStep(_index - 1);
            }
        }

        /// <summary>
        /// Ends the tour: finishing and skipping take the same path, the
        /// caller restores whatever the tour changed (13.7).
        /// </summary>
        private void Finish()
        {
            if (_finished)
            {
                return;
            }
            _finished = true;

            StopAllCoroutines();
            StopAnimation();

            Action onFinished = _onFinished;
            UnityEngine.Object.Destroy(gameObject);
            if (onFinished != null)
            {
                onFinished();
            }
        }

        /// <summary>Skips the tour: same as finishing, callable from outside.</summary>
        public void Skip()
        {
            Finish();
        }
    }
}
