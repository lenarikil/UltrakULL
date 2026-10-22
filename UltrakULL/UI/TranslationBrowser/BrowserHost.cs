using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Hosts the translation browser. The primary (fallback) presentation is
    /// <see cref="Embed"/>, which builds the browser directly inside the game's
    /// OptionsMenu "Language Page" so the left navigation rail stays visible and
    /// the browser uses the game's own Canvas and EventSystem (which is what makes
    /// the buttons clickable). <see cref="Open"/> keeps the older standalone
    /// full-screen overlay as a fallback when no in-menu container is available.
    /// </summary>
    public static class BrowserHost
    {
        private static GameObject _canvasGo;
        private static BrowserView _view;
        private static GameObject _viewRoot;
        private static GameObject _embeddedRoot;

        public static bool IsOpen
        {
            get { return _canvasGo != null && _canvasGo.activeSelf; }
        }

        /// <summary>
        /// Builds the browser inside <paramref name="parent"/> (an OptionsMenu page).
        /// Any previously embedded instance is destroyed first, so this can be called
        /// again whenever the options menu is rebuilt on a scene change.
        /// </summary>
        public static BrowserView Embed(Transform parent, UnityAction onClose)
        {
            NativeUi.EnsureInitialized();
            DestroyEmbedded();

            _view = BrowserView.Create(parent, "TranslationBrowser", onClose, BrowserLayoutMode.Full, false);
            _embeddedRoot = _view.Root;
            _viewRoot = _embeddedRoot;
            _view.StartLoad();
            return _view;
        }

        public static void DestroyEmbedded()
        {
            if (_embeddedRoot != null)
            {
                if (_viewRoot == _embeddedRoot)
                {
                    _view = null;
                    _viewRoot = null;
                }
                Object.Destroy(_embeddedRoot);
                _embeddedRoot = null;
            }
        }

        public static void Open()
        {
            NativeUi.EnsureInitialized();

            if (_canvasGo != null)
            {
                if (!_canvasGo.activeSelf)
                {
                    _canvasGo.SetActive(true);
                }
                return;
            }

            _canvasGo = new GameObject("UltrakULL_TranslationBrowser", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(_canvasGo);

            Canvas canvas = _canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Above the game's options menu and any mod UI.
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = _canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = _canvasGo.AddComponent<Image>();
            background.sprite = UiFactory.WhiteSprite;
            background.color = StylePalette.Background;
            background.raycastTarget = true;

            _view = BrowserView.Create(_canvasGo.transform, "TranslationBrowser", Close, BrowserLayoutMode.Full);
            _viewRoot = _canvasGo;
            _view.StartLoad();
        }

        public static void Close()
        {
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
            _viewRoot = null;
            if (_canvasGo != null)
            {
                Object.Destroy(_canvasGo);
                _canvasGo = null;
            }
        }

        /// <summary>Re-reads the local language list (e.g. right after a download).</summary>
        public static void RefreshInstalled()
        {
            if (_view != null && _viewRoot != null)
            {
                _view.RefreshInstalledList();
            }
        }
    }
}
