using TMPro;
using UnityEngine;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Ping-pong scrolling ("marquee") for a single-line TMP label inside a
    /// RectMask2D viewport. When the text is wider than the viewport, the label
    /// scrolls back and forth between both ends with a short pause at each; when
    /// it fits, it is centered manually. Attached next to the label's viewport
    /// (the label must be a direct child of the masked rect).
    ///
    /// The browser lives in the Options menu where the game is paused, so the
    /// animation runs on unscaled time.
    /// </summary>
    public class MarqueeLabel : MonoBehaviour
    {
        /// <summary>The scrolled label. Its parent rect carries the RectMask2D.</summary>
        public TextMeshProUGUI Label;

        /// <summary>Scroll speed in pixels per second.</summary>
        public float ScrollSpeed = 28f;

        /// <summary>Pause at each end of the scroll, in seconds.</summary>
        public float EndPause = 0.8f;

        private RectTransform _viewport;
        private string _measuredText;
        private float _textWidth;
        private float _pauseRemaining;
        private float _direction = -1f;

        private void Update()
        {
            if (Label == null)
            {
                return;
            }

            if (_viewport == null)
            {
                _viewport = Label.transform.parent as RectTransform;
            }
            if (_viewport == null)
            {
                return;
            }

            float viewportWidth = _viewport.rect.width;
            if (viewportWidth <= 0f)
            {
                // Not laid out yet; try again next frame.
                return;
            }

            // Measure lazily and only when the text changes: preferredWidth forces
            // a text mesh rebuild and must not run every frame.
            string text = Label.text;
            if (!string.Equals(text, _measuredText, System.StringComparison.Ordinal))
            {
                _measuredText = text;
                _textWidth = Label.preferredWidth;
                _direction = -1f;
                _pauseRemaining = EndPause;
                SetX(0f);
            }

            float overflow = _textWidth - viewportWidth;
            if (overflow <= 0f)
            {
                // Fits: keep the text pinned to the left edge so titles read
                // left-aligned, matching the label's own text alignment.
                SetX(0f);
                return;
            }

            if (_pauseRemaining > 0f)
            {
                _pauseRemaining -= Time.unscaledDeltaTime;
                return;
            }

            float x = Label.rectTransform.anchoredPosition.x + _direction * ScrollSpeed * Time.unscaledDeltaTime;
            if (x <= -overflow)
            {
                x = -overflow;
                _direction = 1f;
                _pauseRemaining = EndPause;
            }
            else if (x >= 0f)
            {
                x = 0f;
                _direction = -1f;
                _pauseRemaining = EndPause;
            }
            SetX(x);
        }

        private void SetX(float x)
        {
            if (Label == null)
            {
                return;
            }
            Vector2 position = Label.rectTransform.anchoredPosition;
            if (position.x != x)
            {
                Label.rectTransform.anchoredPosition = new Vector2(x, position.y);
            }
        }
    }
}
