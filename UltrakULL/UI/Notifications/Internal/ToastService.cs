using System;
using System.Collections.Generic;
using UltrakULL.UI.TranslationBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.Notifications
{
    public class ToastService : MonoBehaviour
    {
        private static ToastService _instance;
        public static ToastService Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("UltrakULL_ToastService");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<ToastService>();
                    _instance.Init();
                }
                return _instance;
            }
        }

        private GameObject canvasRoot;
        private GameObject toastContainer;
        private GameObject toastPrefab;
        private bool _prefabUsesNativeArt;

        private void Init()
        {
            // A UI root must be a RectTransform. Creating the canvas on a plain Transform can
            // leave the overlay canvas with no valid rect, so nothing ever renders.
            canvasRoot = new GameObject("UltrakULL_ToastCanvas", typeof(RectTransform));
            canvasRoot.transform.SetParent(transform, false);
            Canvas canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000; // above ULTRAKILL's own UI canvases
            canvasRoot.AddComponent<GraphicRaycaster>();
            CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            TryInitNativeUi();

            // Container: bottom-right stack that grows upward as toasts are added;
            // the newest toast occupies the top slot (sibling 0).
            toastContainer = new GameObject("ToastContainer", typeof(RectTransform));
            toastContainer.transform.SetParent(canvasRoot.transform, false);
            RectTransform tr = toastContainer.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(1f, 0f);
            tr.anchorMax = new Vector2(1f, 0f);
            tr.pivot = new Vector2(1f, 0f);
            tr.anchoredPosition = new Vector2(-24f, 32f);
            tr.sizeDelta = new Vector2(350f, 500f);

            VerticalLayoutGroup vlg = toastContainer.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperRight;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = toastContainer.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            toastPrefab = ToastResources.CreateToastPrefab();
            toastPrefab.transform.SetParent(transform, false);
            toastPrefab.SetActive(false);
            _prefabUsesNativeArt = NativeUi.IsReady;
        }

        private static void TryInitNativeUi()
        {
            try
            {
                NativeUi.EnsureInitialized();
            }
            catch (Exception e)
            {
                Logging.Warn("[Toast] Native UI initialization failed: " + e.Message);
            }
        }

        /// <summary>
        /// Rebuilds the toast prefab, e.g. once the game's native rounded control
        /// sprites become available after the prefab was first built flat. Already
        /// visible toasts keep their old look until they expire.
        /// </summary>
        private void RebuildPrefab()
        {
            if (toastPrefab != null)
            {
                Destroy(toastPrefab);
            }
            toastPrefab = ToastResources.CreateToastPrefab();
            toastPrefab.transform.SetParent(transform, false);
            toastPrefab.SetActive(false);
            _prefabUsesNativeArt = NativeUi.IsReady;
        }

        public void Show(ToastType type, string title, string body, float duration = 4f)
        {
            Show(type, title, body, null, null, duration);
        }

        public void Show(ToastType type, string title, string body, string actionLabel, NotificationAction action, float duration = 6f)
        {
            Logging.Message("[Toast] " + type + ": " + (title ?? "") + " - " + (body ?? ""));
            if (toastContainer == null) Init();

            // The startup toast can appear before Addressables are ready, so the
            // prefab may have been built with flat rectangles. Rebuild it with the
            // game's native rounded art as soon as the sprites become available.
            TryInitNativeUi();
            if (NativeUi.IsReady && !_prefabUsesNativeArt)
            {
                RebuildPrefab();
            }

            // Bounded stack: when the limit is reached the oldest toast (the last
            // sibling at the bottom of the tower) is dismissed instead of the
            // column growing over the whole screen.
            while (toastContainer.transform.childCount >= MaxVisibleToasts)
            {
                UnityEngine.Object.Destroy(toastContainer.transform.GetChild(toastContainer.transform.childCount - 1).gameObject);
            }

            GameObject inst = Instantiate(toastPrefab, toastContainer.transform, false);
            // Sibling 0 is the top slot of the stack: the newest toast appears
            // above the older ones and pushes them down.
            inst.transform.SetSiblingIndex(0);
            ToastView view = inst.GetComponent<ToastView>();
            if (view != null)
                view.Setup(type, title, body, actionLabel, action, duration);
            else
                Destroy(inst);
        }

        /// <summary>Maximum simultaneously visible toasts; the oldest is dismissed beyond that.</summary>
        public const int MaxVisibleToasts = 5;
    }
}
