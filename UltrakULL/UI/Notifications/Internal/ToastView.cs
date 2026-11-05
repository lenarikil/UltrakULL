using System.Collections;
using TMPro;
using UltrakULL.UI.TranslationBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.UI.Notifications
{
    public class ToastView : MonoBehaviour
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI bodyText;
        public Button actionButton;
        public TextMeshProUGUI actionButtonText;
        public CanvasGroup canvasGroup;
        public Image backgroundImage;

        // Set by ToastResources when the prefab is built.
        public Image accentImage;
        public Image iconImage;
        public Image progressFill;
        public GameObject panelRoot;

        private Coroutine hideCoroutine;
        private NotificationAction currentAction;
        private float _duration = 4f;
        private float _remaining;

        public void Setup(ToastType type, string title, string body, string actionLabel = null, NotificationAction action = null, float duration = 4f)
        {
            // Pick up the current language font (it can change at runtime).
            TMP_FontAsset font = ToastResources.GetTmpFont();
            if (font != null)
            {
                if (titleText != null) { titleText.font = font; Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(titleText); }
                if (bodyText != null) { bodyText.font = font; Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(bodyText); }
                if (actionButtonText != null) { actionButtonText.font = font; Harmony_Patches.TextMeshProFontSwap.MarkAsModUI(actionButtonText); }
            }

            if (titleText != null) titleText.text = title ?? "";
            if (bodyText != null) bodyText.text = body ?? "";

            currentAction = action;
            if (actionButton != null)
            {
                if (!string.IsNullOrEmpty(actionLabel) && action != null)
                {
                    actionButton.gameObject.SetActive(true);
                    if (actionButtonText != null) actionButtonText.text = actionLabel;
                    actionButton.onClick.RemoveAllListeners();
                    actionButton.onClick.AddListener(() =>
                    {
                        try { currentAction?.Invoke(); }
                        catch (System.Exception e) { Logging.Warn("Toast action failed: " + e.Message); }
                        HideImmediate();
                    });
                }
                else
                {
                    actionButton.gameObject.SetActive(false);
                }
            }

            SetColors(type);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            gameObject.SetActive(true);

            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(FadeInAndHide(duration));
        }

        private void SetColors(ToastType type)
        {
            Color color = ToastResources.TypeColor(type);

            if (backgroundImage != null)
            {
                backgroundImage.color = ToastResources.ToastBackground;
            }
            if (panelRoot != null)
            {
                UiFactory.SetBorderColor(panelRoot, color);
            }
            if (accentImage != null)
            {
                accentImage.color = color;
            }
            if (iconImage != null)
            {
                iconImage.color = color;
                iconImage.sprite = PrimitiveSprites.Get(ToastResources.TypeIcon(type));
            }
            if (progressFill != null)
            {
                progressFill.color = color;
            }
        }

        private IEnumerator FadeInAndHide(float duration)
        {
            _duration = Mathf.Max(0.1f, duration);
            _remaining = _duration;
            UpdateProgressFill();

            yield return Fade(0f, 1f, 0.2f);

            // Unscaled countdown (menus pause the game, toasts must still
            // dismiss) with the reverse progress strip following the remaining
            // time.
            while (_remaining > 0f)
            {
                _remaining -= Time.unscaledDeltaTime;
                if (_remaining < 0f)
                {
                    _remaining = 0f;
                }
                UpdateProgressFill();
                yield return null;
            }

            yield return Fade(1f, 0f, 0.2f);
            Destroy(gameObject);
        }

        private void UpdateProgressFill()
        {
            if (progressFill != null)
            {
                float fraction = _duration > 0f ? Mathf.Clamp01(_remaining / _duration) : 0f;
                progressFill.rectTransform.anchorMax = new Vector2(fraction, 1f);
            }
        }

        private IEnumerator Fade(float from, float to, float time)
        {
            if (canvasGroup == null) yield break;
            float t = 0f;
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        private void HideImmediate()
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            Destroy(gameObject);
        }
    }
}
