using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Loads remote images (logos, flags, gallery) through UnityWebRequestTexture,
    /// keeps a per-URL Sprite cache and dispatches results on the main thread via a
    /// persistent coroutine host. All callbacks are invoked synchronously if the
    /// sprite is already cached (or when the url is missing, in which case a null
    /// sprite is reported immediately).
    /// </summary>
    public static class ImageCache
    {
        private class CoroutineHost : MonoBehaviour { }

        private const int TimeoutSeconds = 10;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static CoroutineHost _host;

        /// <summary>
        /// Fetches (or returns from cache) a sprite for <paramref name="url"/>.
        /// On failure (or null/empty url) the callback receives null.
        /// </summary>
        public static void GetOrLoad(string url, Action<Sprite> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                callback(null);
                return;
            }

            Sprite cached;
            if (Cache.TryGetValue(url, out cached))
            {
                callback(cached);
                return;
            }

            EnsureHost();
            _host.StartCoroutine(LoadCoroutine(url, callback));
        }

        public static void Forget(string url)
        {
            if (url != null)
            {
                Cache.Remove(url);
            }
        }

        private static void EnsureHost()
        {
            if (_host != null)
            {
                return;
            }

            GameObject go = new GameObject("UltrakULL_BrowserImageCache");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<CoroutineHost>();
        }

        private static IEnumerator LoadCoroutine(string url, Action<Sprite> callback)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                request.timeout = TimeoutSeconds;
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Logging.Warn("[ImageCache] Failed to load '" + url + "': " + request.error);
                    callback(null);
                    yield break;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture == null)
                {
                    Logging.Warn("[ImageCache] Empty texture for '" + url + "'.");
                    callback(null);
                    yield break;
                }

                texture.name = "UllBrowser_" + url;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                Cache[url] = sprite;
                callback(sprite);
            }
        }
    }
}