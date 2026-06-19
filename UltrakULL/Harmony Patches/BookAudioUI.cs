using HarmonyLib;
using System;
using TMPro;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(ScanningStuff), "ScanBook")]
    public static class BookAudioUI
    {
        [HarmonyPostfix]
        public static void ScanBook_Postfix()
        {
            if (isUsingEnglish()) return;

            GameObject canvas = GetInactiveRootObject("Canvas");
            if (canvas == null) return;

            GameObject panel = GetGameObjectChild(
                GetGameObjectChild(
                    GetGameObjectChild(canvas, "ScanningStuff"),
                    "ReadingScanned"),
                "Panel");
            if (panel == null) return;

            BookUIController controller = panel.GetComponent<BookUIController>();
            if (controller == null)
                controller = panel.AddComponent<BookUIController>();

            controller.EnsureUI(panel);
        }
    }

    public class PlayPauseGraphic : Graphic
    {
        public enum State { Playing, Paused, Stopped, ClipEnded }

        private State _state = State.Stopped;

        public State CurrentState
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    SetAllDirty();
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();

            if (_state == State.Playing)
                DrawPlayTriangle(vh, r);
            else
                DrawPauseBars(vh, r);
        }

        private static void DrawPlayTriangle(VertexHelper vh, Rect r)
        {
            float margin = Mathf.Min(r.width, r.height) * 0.18f;
            float cx = r.center.x;
            float cy = r.center.y;
            float hw = r.width * 0.5f - margin;
            float hh = r.height * 0.5f - margin;

            int i = vh.currentVertCount;
            vh.AddVert(new Vector2(cx - hw * 0.5f, cy - hh), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(cx - hw * 0.5f, cy + hh), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(cx + hw * 0.6f, cy), Color.white, Vector2.zero);
            vh.AddTriangle(i, i + 2, i + 1);
        }

        private static void DrawPauseBars(VertexHelper vh, Rect r)
        {
            float margin = Mathf.Min(r.width, r.height) * 0.2f;
            float gap = Mathf.Min(r.width, r.height) * 0.12f;
            float availW = r.width - 2f * margin - gap;
            float barW = availW * 0.5f;

            float yBot = r.yMin + margin;
            float yTop = r.yMax - margin;

            float leftBarX1 = r.xMin + margin;
            float leftBarX2 = leftBarX1 + barW;
            float rightBarX1 = r.xMax - margin - barW;
            float rightBarX2 = r.xMax - margin;

            int i = vh.currentVertCount;
            vh.AddVert(new Vector2(leftBarX1, yBot), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(leftBarX2, yBot), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(leftBarX2, yTop), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(leftBarX1, yTop), Color.white, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);

            vh.AddVert(new Vector2(rightBarX1, yBot), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(rightBarX2, yBot), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(rightBarX2, yTop), Color.white, Vector2.zero);
            vh.AddVert(new Vector2(rightBarX1, yTop), Color.white, Vector2.zero);
            int j = i + 4;
            vh.AddTriangle(j, j + 1, j + 2);
            vh.AddTriangle(j, j + 2, j + 3);
        }
    }

    public class BevelBorderGraphic : Graphic
    {
        public float bevel = 2f;
        public float borderWidth = 2f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            Color c = color;

            float b = Mathf.Max(0.1f, bevel);
            float bw = Mathf.Max(0.1f, borderWidth);
            float sqrt2 = 1.41421356f;
            float d = bw * (sqrt2 - 1f);

            float xMin = r.xMin, xMax = r.xMax;
            float yMin = r.yMin, yMax = r.yMax;

            Vector2[] outer = new Vector2[8];
            outer[0] = new Vector2(xMin + b, yMax);
            outer[1] = new Vector2(xMax - b, yMax);
            outer[2] = new Vector2(xMax, yMax - b);
            outer[3] = new Vector2(xMax, yMin + b);
            outer[4] = new Vector2(xMax - b, yMin);
            outer[5] = new Vector2(xMin + b, yMin);
            outer[6] = new Vector2(xMin, yMin + b);
            outer[7] = new Vector2(xMin, yMax - b);

            Vector2[] inner = new Vector2[8];
            inner[0] = new Vector2(xMin + b + d, yMax - bw);
            inner[1] = new Vector2(xMax - b - d, yMax - bw);
            inner[2] = new Vector2(xMax - bw, yMax - b - d);
            inner[3] = new Vector2(xMax - bw, yMin + b + d);
            inner[4] = new Vector2(xMax - b - d, yMin + bw);
            inner[5] = new Vector2(xMin + b + d, yMin + bw);
            inner[6] = new Vector2(xMin + bw, yMin + b + d);
            inner[7] = new Vector2(xMin + bw, yMax - b - d);

            for (int i = 0; i < 8; i++)
            {
                int ni = (i + 1) % 8;
                int idx = vh.currentVertCount;

                vh.AddVert(outer[i], c, Vector2.zero);
                vh.AddVert(outer[ni], c, Vector2.zero);
                vh.AddVert(inner[ni], c, Vector2.zero);
                vh.AddVert(inner[i], c, Vector2.zero);

                vh.AddTriangle(idx, idx + 1, idx + 2);
                vh.AddTriangle(idx, idx + 2, idx + 3);
            }
        }
    }

    public class BookUIController : MonoBehaviour
    {
        private GameObject controlsRoot;
        private PlayPauseGraphic playIcon;
        private GameObject progressFill;
        private TextMeshProUGUI timeTextWhite;
        private TextMeshProUGUI timeTextBlack;
        private TextMeshProUGUI errorText;
        private RectTransform progressInteriorRect;
        private bool uiCreated = false;

        private RectTransform scrollRectTransform;
        private Vector2 savedScrollOffsetMin;
        private Vector2 savedScrollOffsetMax;
        private RectTransform scrollbarTransform;
        private Vector2 savedScrollbarOffsetMin;
        private Vector2 savedScrollbarOffsetMax;
        private Vector2 savedScrollbarAnchorMin;
        private Vector2 savedScrollbarAnchorMax;
        private GameObject spacer;

        private bool _wasActuallyPlaying = false;
        private bool _wasGamePaused = false;

        private const float safety = 9f;
        private const float ctrlHeight = 42f;
        private const float panelMargin = 10f;
        private const float iconContentSize = 40f;
        private const float borderWidth = 2f;
        private const float iconOuterSize = iconContentSize + 2f * borderWidth;
        private const float bevel = 5f;

        public void EnsureUI(GameObject panel)
        {
            if (uiCreated && controlsRoot != null)
            {
                controlsRoot.SetActive(true);
                return;
            }

            ScrollRect scrollRect = panel.GetComponentInChildren<ScrollRect>(true);
            if (scrollRect == null) return;

            RectTransform scrollRectTransformRef = scrollRect.GetComponent<RectTransform>();

            controlsRoot = new GameObject("BookAudio_Controls", typeof(RectTransform));
            controlsRoot.transform.SetParent(panel.transform, false);
            controlsRoot.transform.SetAsLastSibling();

            RectTransform rootRect = controlsRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(scrollRectTransformRef.anchorMin.x, 0);
            rootRect.anchorMax = new Vector2(scrollRectTransformRef.anchorMax.x, 0);
            rootRect.pivot = new Vector2(0.5f, 0);
            rootRect.offsetMin = new Vector2(scrollRectTransformRef.offsetMin.x, panelMargin);
            rootRect.offsetMax = new Vector2(scrollRectTransformRef.offsetMax.x, ctrlHeight + panelMargin);

            BevelBorderGraphic outerBorder = controlsRoot.AddComponent<BevelBorderGraphic>();
            outerBorder.bevel = bevel;
            outerBorder.borderWidth = borderWidth;
            outerBorder.color = Color.white;
            outerBorder.raycastTarget = false;

            GameObject iconObj = new GameObject("BookAudio_Icon", typeof(RectTransform));
            iconObj.transform.SetParent(controlsRoot.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            float iconSize = Mathf.Min(iconOuterSize, ctrlHeight - 2f * safety);
            iconRect.anchorMin = new Vector2(0, 0);
            iconRect.anchorMax = new Vector2(0, 1);
            iconRect.offsetMin = new Vector2(safety, safety);
            iconRect.offsetMax = new Vector2(safety + iconSize, -safety);

            BevelBorderGraphic iconBorder = iconObj.AddComponent<BevelBorderGraphic>();
            iconBorder.bevel = bevel;
            iconBorder.borderWidth = borderWidth;
            iconBorder.color = Color.white;
            iconBorder.raycastTarget = false;

            GameObject iconBg = new GameObject("BookAudio_IconBg", typeof(RectTransform), typeof(Image));
            iconBg.transform.SetParent(iconObj.transform, false);
            RectTransform iconBgRect = iconBg.GetComponent<RectTransform>();
            iconBgRect.anchorMin = Vector2.zero;
            iconBgRect.anchorMax = Vector2.one;
            iconBgRect.offsetMin = new Vector2(borderWidth, borderWidth);
            iconBgRect.offsetMax = new Vector2(-borderWidth, -borderWidth);
            Image iconBgImg = iconBg.GetComponent<Image>();
            iconBgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
            iconBgImg.raycastTarget = false;

            GameObject iconContent = new GameObject("BookAudio_IconContent", typeof(RectTransform));
            iconContent.transform.SetParent(iconBg.transform, false);
            RectTransform iconContentRect = iconContent.GetComponent<RectTransform>();
            iconContentRect.anchorMin = Vector2.zero;
            iconContentRect.anchorMax = Vector2.one;
            iconContentRect.sizeDelta = Vector2.zero;

            playIcon = iconContent.AddComponent<PlayPauseGraphic>();
            playIcon.raycastTarget = false;
            playIcon.CurrentState = PlayPauseGraphic.State.Stopped;

            float iconRightEdge = safety + iconSize + safety;

            GameObject progressFrame = new GameObject("BookAudio_ProgressFrame", typeof(RectTransform));
            progressFrame.transform.SetParent(controlsRoot.transform, false);
            RectTransform progressFrameRect = progressFrame.GetComponent<RectTransform>();
            progressFrameRect.anchorMin = new Vector2(0, 0);
            progressFrameRect.anchorMax = new Vector2(1, 1);
            progressFrameRect.offsetMin = new Vector2(iconRightEdge, safety);
            progressFrameRect.offsetMax = new Vector2(-safety, -safety);

            BevelBorderGraphic progressBorder = progressFrame.AddComponent<BevelBorderGraphic>();
            progressBorder.bevel = bevel;
            progressBorder.borderWidth = borderWidth;
            progressBorder.color = Color.white;
            progressBorder.raycastTarget = false;

            GameObject progressInterior = new GameObject("BookAudio_ProgressInterior", typeof(RectTransform), typeof(Image));
            progressInterior.transform.SetParent(progressFrame.transform, false);
            progressInteriorRect = progressInterior.GetComponent<RectTransform>();
            progressInteriorRect.anchorMin = Vector2.zero;
            progressInteriorRect.anchorMax = Vector2.one;
            progressInteriorRect.offsetMin = new Vector2(borderWidth, borderWidth);
            progressInteriorRect.offsetMax = new Vector2(-borderWidth, -borderWidth);
            Image progressInteriorBg = progressInterior.GetComponent<Image>();
            progressInteriorBg.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
            progressInteriorBg.raycastTarget = false;

            progressFill = new GameObject("BookAudio_ProgressFill", typeof(RectTransform), typeof(Image));
            progressFill.transform.SetParent(progressInterior.transform, false);
            RectTransform fillRect = progressFill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = progressFill.GetComponent<Image>();
            fillImage.color = Color.white;
            fillImage.raycastTarget = false;

            Mask fillMask = progressFill.AddComponent<Mask>();
            fillMask.showMaskGraphic = true;

            GameObject timeWhiteObj = new GameObject("BookAudio_TimeTextWhite", typeof(RectTransform));
            timeWhiteObj.SetActive(false);
            timeTextWhite = timeWhiteObj.AddComponent<TextMeshProUGUI>();
            timeWhiteObj.transform.SetParent(progressInterior.transform, false);
            timeWhiteObj.transform.SetAsFirstSibling();
            RectTransform timeWhiteRect = timeWhiteObj.GetComponent<RectTransform>();
            timeWhiteRect.anchorMin = Vector2.zero;
            timeWhiteRect.anchorMax = Vector2.one;
            timeWhiteRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI sourceFont = panel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (sourceFont != null && sourceFont.font != null)
                timeTextWhite.font = sourceFont.font;

            timeTextWhite.fontSize = 16f;
            timeTextWhite.alignment = TextAlignmentOptions.Center;
            timeTextWhite.color = Color.white;
            timeTextWhite.text = "00:00/00:00";

            timeWhiteObj.SetActive(true);

            GameObject timeBlackObj = new GameObject("BookAudio_TimeTextBlack", typeof(RectTransform));
            timeBlackObj.SetActive(false);
            timeTextBlack = timeBlackObj.AddComponent<TextMeshProUGUI>();
            timeBlackObj.transform.SetParent(progressFill.transform, false);
            RectTransform timeBlackRect = timeBlackObj.GetComponent<RectTransform>();
            timeBlackRect.anchorMin = Vector2.zero;
            timeBlackRect.anchorMax = Vector2.one;
            timeBlackRect.sizeDelta = Vector2.zero;

            if (sourceFont != null && sourceFont.font != null)
                timeTextBlack.font = sourceFont.font;

            timeTextBlack.fontSize = 16f;
            timeTextBlack.alignment = TextAlignmentOptions.Center;
            timeTextBlack.color = Color.black;
            timeTextBlack.text = "00:00/00:00";

            timeBlackObj.SetActive(true);

            GameObject errorObj = new GameObject("BookAudio_ErrorText", typeof(RectTransform));
            errorObj.SetActive(false);
            errorText = errorObj.AddComponent<TextMeshProUGUI>();
            errorObj.transform.SetParent(progressInterior.transform, false);
            RectTransform errorRect = errorObj.GetComponent<RectTransform>();
            errorRect.anchorMin = Vector2.zero;
            errorRect.anchorMax = Vector2.one;
            errorRect.sizeDelta = Vector2.zero;

            if (sourceFont != null && sourceFont.font != null)
                errorText.font = sourceFont.font;

            errorText.fontSize = 14f;
            errorText.alignment = TextAlignmentOptions.Center;
            errorText.color = new Color(1f, 0.3f, 0.3f);
            string errorKey = LanguageManager.CurrentLanguage?.books?.books_audioError;
            errorText.text = string.IsNullOrEmpty(errorKey)
                ? "ERROR: IMPOSSIBLE TO BUILD A SPEECH PATTERN"
                : errorKey;

            errorObj.SetActive(true);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
            float controlsHeight = rootRect.rect.height;
            float bottomOffset = controlsHeight + panelMargin;

            if (scrollRect != null)
            {
                scrollRectTransform = scrollRect.GetComponent<RectTransform>();
                savedScrollOffsetMin = scrollRectTransform.offsetMin;
                savedScrollOffsetMax = scrollRectTransform.offsetMax;
                scrollRectTransform.offsetMin = new Vector2(scrollRectTransform.offsetMin.x,
                    savedScrollOffsetMin.y + bottomOffset);

                if (scrollRect.content != null)
                {
                    spacer = new GameObject("BookAudio_Spacer", typeof(RectTransform));
                    spacer.transform.SetParent(scrollRect.content, false);
                    spacer.transform.SetAsLastSibling();
                    RectTransform spacerRect = spacer.GetComponent<RectTransform>();
                    spacerRect.anchorMin = new Vector2(0, 0);
                    spacerRect.anchorMax = new Vector2(1, 0);
                    spacerRect.pivot = new Vector2(0.5f, 0);
                    spacerRect.anchoredPosition = Vector2.zero;
                    spacerRect.sizeDelta = new Vector2(0, bottomOffset);
                }
            }

            Scrollbar scrollbarComp = panel.GetComponentInChildren<Scrollbar>(true);
            if (scrollbarComp != null)
            {
                scrollbarTransform = scrollbarComp.GetComponent<RectTransform>();
                savedScrollbarAnchorMin = scrollbarTransform.anchorMin;
                savedScrollbarAnchorMax = scrollbarTransform.anchorMax;
                savedScrollbarOffsetMin = scrollbarTransform.offsetMin;
                savedScrollbarOffsetMax = scrollbarTransform.offsetMax;

                RectTransform viewport = scrollRect.viewport;
                if (viewport != null)
                {
                    scrollbarTransform.anchorMin = new Vector2(
                        scrollbarTransform.anchorMin.x, viewport.anchorMin.y);
                    scrollbarTransform.anchorMax = new Vector2(
                        scrollbarTransform.anchorMax.x, viewport.anchorMax.y);
                    scrollbarTransform.offsetMin = new Vector2(
                        scrollbarTransform.offsetMin.x, viewport.offsetMin.y);
                    scrollbarTransform.offsetMax = new Vector2(
                        scrollbarTransform.offsetMax.x, viewport.offsetMax.y);
                }
            }

            uiCreated = true;

            BookAudioPlayer.OnPlay += OnPlaybackEvent;
            BookAudioPlayer.OnStop += OnPlaybackEvent;
            BookAudioPlayer.OnPause += OnPlaybackEvent;
            BookAudioPlayer.OnResume += OnPlaybackEvent;
            BookAudioPlayer.OnError += OnPlaybackEvent;

            UpdateUI();
        }

        private void OnDisable()
        {
            if (uiCreated)
            {
                BookAudioPlayer.Stop();
            }
        }

        private void OnDestroy()
        {
            BookAudioPlayer.OnPlay -= OnPlaybackEvent;
            BookAudioPlayer.OnStop -= OnPlaybackEvent;
            BookAudioPlayer.OnPause -= OnPlaybackEvent;
            BookAudioPlayer.OnResume -= OnPlaybackEvent;
            BookAudioPlayer.OnError -= OnPlaybackEvent;

            if (scrollRectTransform != null)
            {
                scrollRectTransform.offsetMin = savedScrollOffsetMin;
                scrollRectTransform.offsetMax = savedScrollOffsetMax;
            }
            if (scrollbarTransform != null)
            {
                scrollbarTransform.anchorMin = savedScrollbarAnchorMin;
                scrollbarTransform.anchorMax = savedScrollbarAnchorMax;
                scrollbarTransform.offsetMin = savedScrollbarOffsetMin;
                scrollbarTransform.offsetMax = savedScrollbarOffsetMax;
            }
        }

        private void OnPlaybackEvent()
        {
            if (gameObject.activeInHierarchy)
                UpdateUI();
        }

        private void Update()
        {
            if (!uiCreated || !gameObject.activeInHierarchy) return;

            bool gamePaused = MonoSingleton<OptionsManager>.Instance != null &&
                              MonoSingleton<OptionsManager>.Instance.paused;

            if (gamePaused != _wasGamePaused)
            {
                if (gamePaused)
                    BookAudioPlayer.PauseForGamePause();
                else
                {
                    BookAudioPlayer.ResumeForGamePause();
                    _wasActuallyPlaying = BookAudioPlayer.RawIsPlaying;
                }
                _wasGamePaused = gamePaused;
                UpdateUI();
            }

            if (gamePaused)
                return;

            HandleInput();

            bool rawPlaying = BookAudioPlayer.RawIsPlaying;

            if (_wasActuallyPlaying && !rawPlaying && !BookAudioPlayer.IsPaused && !BookAudioPlayer.IsGamePaused && BookAudioPlayer.HasClip)
            {
                BookAudioPlayer.NotifyClipEnded();
            }

            _wasActuallyPlaying = rawPlaying;

            UpdateUI();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                BookAudioPlayer.TogglePause();
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                BookAudioPlayer.Stop();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                BookAudioPlayer.Seek(-5f);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                BookAudioPlayer.Seek(5f);
            else if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                BookAudioPlayer.Volume += 0.1f;
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                BookAudioPlayer.Volume -= 0.1f;
        }

        private void UpdateUI()
        {
            bool hasError = BookAudioPlayer.HasError;

            if (errorText != null)
                errorText.gameObject.SetActive(hasError);
            if (timeTextWhite != null)
                timeTextWhite.gameObject.SetActive(!hasError);
            if (timeTextBlack != null)
                timeTextBlack.gameObject.SetActive(!hasError);

            if (hasError && progressFill != null)
                progressFill.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);

            UpdateStatusIcon();
            if (!hasError)
                UpdateProgress();
        }

        private void UpdateStatusIcon()
        {
            if (playIcon == null) return;

            if (BookAudioPlayer.IsPlaying)
                playIcon.CurrentState = PlayPauseGraphic.State.Playing;
            else if (BookAudioPlayer.ClipEnded)
                playIcon.CurrentState = PlayPauseGraphic.State.ClipEnded;
            else if (BookAudioPlayer.IsPaused)
                playIcon.CurrentState = PlayPauseGraphic.State.Paused;
            else
                playIcon.CurrentState = PlayPauseGraphic.State.Stopped;
        }

        private void UpdateProgress()
        {
            float total = BookAudioPlayer.TotalTime;
            float current = BookAudioPlayer.CurrentTime;

            if (progressFill != null)
            {
                float fill = total > 0f ? current / total : 0f;
                progressFill.GetComponent<RectTransform>().anchorMax = new Vector2(fill, 1);

                if (timeTextBlack != null && progressInteriorRect != null)
                {
                    float safeFill = Mathf.Max(fill, 0.001f);
                    float extend = (1f - safeFill) / (2f * safeFill);
                    timeTextBlack.rectTransform.anchorMin = new Vector2(-extend, 0);
                    timeTextBlack.rectTransform.anchorMax = new Vector2(1f + extend, 1);

                    float interiorW = progressInteriorRect.rect.width;
                    float fillW = fill * interiorW;
                    timeTextBlack.rectTransform.anchoredPosition =
                        new Vector2((interiorW - fillW) * 0.5f, 0);
                }
            }

            string timeStr = FormatTime(current) + "/" + FormatTime(total);
            if (timeTextWhite != null)
                timeTextWhite.text = timeStr;
            if (timeTextBlack != null)
                timeTextBlack.text = timeStr;
        }

        private static string FormatTime(float seconds)
        {
            int totalSec = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            int min = totalSec / 60;
            int sec = totalSec % 60;
            return min.ToString("D2") + ":" + sec.ToString("D2");
        }
    }
}
