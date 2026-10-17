using System;

namespace UltrakULL.UI.Notifications
{
    // Wraps a notifier so a notification failure (Unity object destroyed, reflection change in
    // Notiffy, missing font, etc.) never breaks the caller's flow. Failures are logged instead
    // of swallowed silently.
    internal class SafeNotifier : INotifier
    {
        private readonly INotifier _inner;

        public SafeNotifier(INotifier inner)
        {
            _inner = inner ?? throw new ArgumentNullException("inner");
        }

        private void Guard(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to show notification: " + e.Message);
                Logging.Warn(e.ToString());
            }
        }

        public void Info(string title, string body, float duration = 4f) => Guard(() => _inner.Info(title, body, duration));
        public void Success(string title, string body, float duration = 4f) => Guard(() => _inner.Success(title, body, duration));
        public void Warning(string title, string body, float duration = 4f) => Guard(() => _inner.Warning(title, body, duration));
        public void Error(string title, string body, float duration = 4f) => Guard(() => _inner.Error(title, body, duration));

        public void Info(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => Guard(() => _inner.Info(title, body, actionLabel, action, duration));
        public void Success(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => Guard(() => _inner.Success(title, body, actionLabel, action, duration));
        public void Warning(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => Guard(() => _inner.Warning(title, body, actionLabel, action, duration));
        public void Error(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => Guard(() => _inner.Error(title, body, actionLabel, action, duration));
    }
}