using UnityEngine;
using UnityEngine.Events;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Hosts the translation browser. The only presentation is
    /// <see cref="Embed"/>, which builds the browser directly inside the
    /// game's OptionsMenu "Language Page" so the left navigation rail stays
    /// visible and the browser uses the game's own Canvas and EventSystem
    /// (which is what makes the buttons clickable).
    /// </summary>
    public static class BrowserHost
    {
        private static BrowserView _view;
        private static GameObject _embeddedRoot;

        /// <summary>The browser instance currently hosted, when any.</summary>
        public static BrowserView CurrentView
        {
            get { return _view; }
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

            _view = BrowserView.Create(parent, "TranslationBrowser", onClose);
            _embeddedRoot = _view.Root;
            _view.StartLoad();
            return _view;
        }

        public static void DestroyEmbedded()
        {
            if (_embeddedRoot != null)
            {
                _view = null;
                Object.Destroy(_embeddedRoot);
                _embeddedRoot = null;
            }
        }

        /// <summary>Re-reads the local language list (e.g. right after a download).</summary>
        public static void RefreshInstalled()
        {
            if (_view != null && _embeddedRoot != null)
            {
                _view.RefreshInstalledList();
            }
        }
    }
}
