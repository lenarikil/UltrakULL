namespace UltrakULL.UI.Notifications
{
    public class InternalToastNotifier : INotifier
    {
        public void Info(string title, string body, float duration = 4f) => ToastService.Instance.Show(ToastType.Info, title, body, duration);
        public void Success(string title, string body, float duration = 4f) => ToastService.Instance.Show(ToastType.Success, title, body, duration);
        public void Warning(string title, string body, float duration = 4f) => ToastService.Instance.Show(ToastType.Warning, title, body, duration);
        public void Error(string title, string body, float duration = 4f) => ToastService.Instance.Show(ToastType.Error, title, body, duration);

        public void Info(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => ToastService.Instance.Show(ToastType.Info, title, body, actionLabel, action, duration);
        public void Success(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => ToastService.Instance.Show(ToastType.Success, title, body, actionLabel, action, duration);
        public void Warning(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => ToastService.Instance.Show(ToastType.Warning, title, body, actionLabel, action, duration);
        public void Error(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => ToastService.Instance.Show(ToastType.Error, title, body, actionLabel, action, duration);
    }
}