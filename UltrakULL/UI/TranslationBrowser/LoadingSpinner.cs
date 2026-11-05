using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Sweep loader (item 10): a hollow ring whose radial fill grows from
    /// nothing to a full circle and then unwinds back, alternating the sweep
    /// direction every half-cycle - the look of the CSS "l20" clip-path
    /// spinner. Runs on unscaled time so it keeps animating while the game is
    /// paused (installs pause gameplay). Attach to an Image whose sprite is a
    /// ring and whose type is Filled/Radial360 (see <see cref="Create"/>).
    /// </summary>
    public sealed class LoadingSpinner : MonoBehaviour
    {
        private const float HalfCycleSeconds = 0.8f;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (_image != null)
            {
                _image.fillAmount = 0f;
            }
        }

        private void Update()
        {
            if (_image == null)
            {
                return;
            }

            // Phase 0..1: grow clockwise; phase 1..2: unwind counterclockwise.
            float phase = (Time.unscaledTime / HalfCycleSeconds) % 2f;
            if (phase < 1f)
            {
                _image.fillClockwise = true;
                _image.fillAmount = phase;
            }
            else
            {
                _image.fillClockwise = false;
                _image.fillAmount = 2f - phase;
            }
        }

        /// <summary>
        /// Creates a ring sprite image set up for the radial sweep and drives it
        /// with this component. Returns the image (hidden until activated).
        /// </summary>
        public static Image Create(Transform parent, string name, Vector2 size, Color color)
        {
            Image image = UiFactory.CreateImage(parent, name, size, color, PrimitiveSprites.Kind.Ring);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = 0f;
            image.gameObject.AddComponent<LoadingSpinner>();
            return image;
        }
    }
}
