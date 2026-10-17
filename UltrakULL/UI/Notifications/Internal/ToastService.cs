using System.Collections.Generic;
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

            // Container: bottom-right stack that grows upward as toasts are added.
            toastContainer = new GameObject("ToastContainer", typeof(RectTransform));
            toastContainer.transform.SetParent(canvasRoot.transform, false);
            RectTransform tr = toastContainer.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(1f, 0f);
            tr.anchorMax = new Vector2(1f, 0f);
            tr.pivot = new Vector2(1f, 0f);
            tr.anchoredPosition = new Vector2(-20f, 20f);
            tr.sizeDelta = new Vector2(350f, 500f);

            VerticalLayoutGroup vlg = toastContainer.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.LowerRight;
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
        }

        public void Show(ToastType type, string title, string body, float duration = 4f)
        {
            Show(type, title, body, null, null, duration);
        }

        public void Show(ToastType type, string title, string body, string actionLabel, NotificationAction action, float duration = 6f)
        {
            Logging.Message("[Toast] " + type + ": " + (title ?? "") + " - " + (body ?? ""));
            if (toastContainer == null) Init();
            GameObject inst = Instantiate(toastPrefab, toastContainer.transform, false);
            ToastView view = inst.GetComponent<ToastView>();
            if (view != null)
                view.Setup(type, title, body, actionLabel, action, duration);
            else
                Destroy(inst);
        }
    }
}