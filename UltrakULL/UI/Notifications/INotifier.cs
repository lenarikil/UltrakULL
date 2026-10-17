using System;

namespace UltrakULL.UI.Notifications
{
    public delegate void NotificationAction();

    public interface INotifier
    {
        void Info(string title, string body, float duration = 4f);
        void Success(string title, string body, float duration = 4f);
        void Warning(string title, string body, float duration = 4f);
        void Error(string title, string body, float duration = 4f);

        void Info(string title, string body, string actionLabel, NotificationAction action, float duration = 6f);
        void Success(string title, string body, string actionLabel, NotificationAction action, float duration = 6f);
        void Warning(string title, string body, string actionLabel, NotificationAction action, float duration = 6f);
        void Error(string title, string body, string actionLabel, NotificationAction action, float duration = 6f);
    }
}