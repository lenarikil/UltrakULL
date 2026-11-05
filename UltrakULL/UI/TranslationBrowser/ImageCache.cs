using System;
using System.Collections;
using System.Collections.Generic;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.Networking;

namespace UltrakULL.UI.TranslationBrowser
{
    /// <summary>
    /// Loads remote catalog images (logos, gallery) through UnityWebRequestTexture,
    /// keeps an LRU sprite cache (bounded, textures destroyed on eviction) and
    /// limits the number of parallel downloads. Only URLs on the shared image
    /// host allowlist (UrlPolicy) are ever requested; anything else reports null
    /// immediately. All callbacks are invoked synchronously when the sprite is
    /// already cached; downloaded results are dispatched from the coroutine (main
    /// thread).
    /// </summary>
    public static class ImageCache
    {
        private class CoroutineHost : MonoBehaviour { }

        private const int TimeoutSeconds = 10;
        private const int MaxCacheEntries = 64;
        private const int MaxConcurrentDownloads = 4;

        private sealed class PendingRequest
        {
            public string Url;
            public readonly List<Action<Sprite>> Callbacks = new List<Action<Sprite>>();
        }

        // url -> sprite, plus access order for LRU eviction (front = oldest).
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly LinkedList<string> CacheOrder = new LinkedList<string>();

        private static readonly Queue<PendingRequest> Waiting = new Queue<PendingRequest>();
        private static readonly Dictionary<string, PendingRequest> InFlight = new Dictionary<string, PendingRequest>();
        private static int _activeDownloads;

        private static CoroutineHost _host;

        /// <summary>
        /// Fetches (or returns from cache) a sprite for <paramref name="url"/>.
        /// On failure (or null/empty/non-allowlisted url) the callback receives null.
        /// </summary>
        public static void GetOrLoad(string url, Action<Sprite> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (!UrlPolicy.IsAllowedImageUrl(url))
            {
                // Missing, non-https or off-allowlist URL: never requested.
                callback(null);
                return;
            }

            string key = url.Trim();

            Sprite cached;
            if (Cache.TryGetValue(key, out cached))
            {
                TouchCacheEntry(key);
                callback(cached);
                return;
            }

            PendingRequest inFlight;
            if (InFlight.TryGetValue(key, out inFlight))
            {
                inFlight.Callbacks.Add(callback);
                return;
            }

            PendingRequest request = new PendingRequest { Url = key };
            request.Callbacks.Add(callback);

            if (_activeDownloads >= MaxConcurrentDownloads)
            {
                Waiting.Enqueue(request);
                return;
            }

            StartDownload(request);
        }

        public static void Forget(string url)
        {
            if (url != null)
            {
                Cache.Remove(url.Trim());
            }
        }

        private static void StartDownload(PendingRequest request)
        {
            EnsureHost();
            InFlight[request.Url] = request;
            _activeDownloads++;
            _host.StartCoroutine(LoadCoroutine(request));
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

        /// <summary>Pumps the waiting queue after a download slot freed up.</summary>
        private static void PumpQueue()
        {
            while (_activeDownloads < MaxConcurrentDownloads && Waiting.Count > 0)
            {
                PendingRequest next = Waiting.Dequeue();
                if (InFlight.ContainsKey(next.Url))
                {
                    // A duplicate slipped into the queue; its callbacks are already
                    // covered by the running request.
                    continue;
                }
                StartDownload(next);
            }
        }

        private static void StoreInCache(string url, Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            if (Cache.ContainsKey(url))
            {
                TouchCacheEntry(url);
                return;
            }

            Cache[url] = sprite;
            CacheOrder.AddLast(url);

            while (Cache.Count > MaxCacheEntries)
            {
                string oldest = CacheOrder.First.Value;
                CacheOrder.RemoveFirst();

                Sprite evicted;
                if (Cache.TryGetValue(oldest, out evicted))
                {
                    Cache.Remove(oldest);
                    if (evicted != null)
                    {
                        // Free the GPU memory; an evicted image that is still on
                        // screen simply shows its placeholder on the next request.
                        if (evicted.texture != null)
                        {
                            UnityEngine.Object.Destroy(evicted.texture);
                        }
                        UnityEngine.Object.Destroy(evicted);
                    }
                }
            }
        }

        private static void TouchCacheEntry(string url)
        {
            LinkedListNode<string> node = CacheOrder.Find(url);
            if (node != null)
            {
                CacheOrder.Remove(node);
                CacheOrder.AddLast(url);
            }
        }

        private static IEnumerator LoadCoroutine(PendingRequest request)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(request.Url))
            {
                webRequest.timeout = TimeoutSeconds;
                yield return webRequest.SendWebRequest();

                _activeDownloads--;
                InFlight.Remove(request.Url);

                Sprite sprite = null;
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Logging.Warn("[ImageCache] Failed to load '" + request.Url + "': " + webRequest.error);
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                    if (texture == null)
                    {
                        Logging.Warn("[ImageCache] Empty texture for '" + request.Url + "'.");
                    }
                    else
                    {
                        texture.name = "UllBrowser_" + request.Url;
                        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                    }
                }

                if (sprite != null)
                {
                    StoreInCache(request.Url, sprite);
                }

                foreach (Action<Sprite> callback in request.Callbacks)
                {
                    try
                    {
                        callback(sprite);
                    }
                    catch (Exception e)
                    {
                        Logging.Warn("[ImageCache] Callback failed: " + e.Message);
                    }
                }

                PumpQueue();
            }
        }
    }
}
