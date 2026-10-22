using System.Collections;
using TMPro;
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

        private Coroutine hideCoroutine;
        private NotificationAction currentAction;

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
            if (backgroundImage == null) return;
            switch (type)
            {
                case ToastType.Success:
                    backgroundImage.color = new Color(0.235f, 0.702f, 0.443f, 0.92f);
                    break;
                case ToastType.Warning:
                    backgroundImage.color = new Color(1f, 0.647f, 0f, 0.92f);
                    break;
                case ToastType.Error:
                    backgroundImage.color = new Color(1f, 0.302f, 0.302f, 0.92f);
                    break;
                case ToastType.Info:
                default:
                    backgroundImage.color = new Color(0.298f, 0.6f, 0.902f, 0.92f);
                    break;
            }
        }

        private IEnumerator FadeInAndHide(float duration)
        {
            yield return Fade(0f, 1f, 0.2f);
            // Unscaled: the game pauses (timeScale = 0) in menus, toasts must still dismiss.
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration - 0.4f));
            yield return Fade(1f, 0f, 0.2f);
            Destroy(gameObject);
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