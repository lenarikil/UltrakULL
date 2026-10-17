using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UltrakULL.UI.Notifications
{
    // Bridges INotifier to Notiffy via reflection, so UltrakULL never hard-references the Notiffy DLL.
    // When Notiffy is installed, all mod notifications must go through it (per project decision).
    public class NotiffyAdapter : INotifier
    {
        private const string ActionId = "ultrakull_action";
        // Safety cap so a missed NotificationDeleted event can never grow the map unbounded.
        private const int MaxPendingActions = 16;

        private readonly MethodInfo _notifySendMethod;
        private readonly ParameterInfo[] _sendParameters;
        private readonly Dictionary<uint, NotificationAction> _pendingActions = new Dictionary<uint, NotificationAction>();
        private readonly object _pendingLock = new object();

        public NotiffyAdapter()
        {
            Type notificationSystemType = FindNotificationSystemType();
            if (notificationSystemType == null)
                throw new Exception("Notiffy.API.NotificationSystem not found");

            _notifySendMethod = notificationSystemType.GetMethod("NotifySend", BindingFlags.Public | BindingFlags.Static);
            if (_notifySendMethod == null)
                throw new Exception("Notiffy.API.NotificationSystem.NotifySend not found");
            _sendParameters = _notifySendMethod.GetParameters();

            // Subscribe exactly once: resubscribing per send would double-invoke actions,
            // and unsubscribing after the first action would break later notifications.
            EventInfo actionInvoked = notificationSystemType.GetEvent("ActionInvoked", BindingFlags.Public | BindingFlags.Static);
            EventInfo notificationDeleted = notificationSystemType.GetEvent("NotificationDeleted", BindingFlags.Public | BindingFlags.Static);
            if (actionInvoked != null && notificationDeleted != null)
            {
                try
                {
                    // CreateDelegate requires an exact signature match with the event's own handler type.
                    Delegate actionHandler = Delegate.CreateDelegate(actionInvoked.EventHandlerType, this, "HandleActionInvoked");
                    Delegate deletedHandler = Delegate.CreateDelegate(notificationDeleted.EventHandlerType, this, "HandleNotificationDeleted");
                    actionInvoked.AddEventHandler(null, actionHandler);
                    notificationDeleted.AddEventHandler(null, deletedHandler);
                }
                catch (Exception e)
                {
                    Logging.Warn("Notiffy action events use unexpected signatures, notification actions will be unavailable: " + e.Message);
                }
            }
        }

        private static Type FindNotificationSystemType()
        {
            Type type = Type.GetType("Notiffy.API.NotificationSystem, Notiffy");
            if (type != null)
                return type;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = asm.GetType("Notiffy.API.NotificationSystem");
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Some dynamic assemblies can throw on GetType; ignore them.
                }
            }
            return null;
        }

        // Event signatures from the Notiffy wiki: (uint id, string actionIdentifier) and (uint id).
        private void HandleActionInvoked(uint id, string actionIdentifier)
        {
            if (!string.Equals(actionIdentifier, ActionId, StringComparison.Ordinal))
                return;

            NotificationAction action;
            lock (_pendingLock)
            {
                if (!_pendingActions.TryGetValue(id, out action))
                    return;
                _pendingActions.Remove(id);
            }

            try { if (action != null) action(); }
            catch (Exception e) { Logging.Warn("Notiffy notification action failed: " + e.Message); }
        }

        private void HandleNotificationDeleted(uint id)
        {
            lock (_pendingLock)
            {
                _pendingActions.Remove(id);
            }
        }

        // Builds Invoke() arguments from the actual method parameters instead of assuming
        // a fixed argument order, so reordering/new optional parameters in Notiffy don't break us.
        private object[] BuildArgs(string title, string body, Dictionary<string, string> actions, float duration)
        {
            object[] args = new object[_sendParameters.Length];
            int stringIndex = 0;

            for (int i = 0; i < _sendParameters.Length; i++)
            {
                ParameterInfo p = _sendParameters[i];
                string name = (p.Name ?? string.Empty).ToLowerInvariant();
                Type t = p.ParameterType;

                if (t == typeof(string))
                {
                    if (name == "summary" || name == "title")
                        args[i] = title ?? string.Empty;
                    else if (name == "body" || name == "text" || name == "message")
                        args[i] = body ?? string.Empty;
                    else if (name == "appname")
                        args[i] = "UltrakULL"; // shown as the sender in the notification panel
                    else if (stringIndex == 0 && string.IsNullOrEmpty(name))
                        args[i] = title ?? string.Empty;
                    else if (stringIndex == 1 && string.IsNullOrEmpty(name))
                        args[i] = body ?? string.Empty;
                    else if (p.HasDefaultValue && p.DefaultValue is string)
                        args[i] = p.DefaultValue; // iconFilePath="", category=null and similar
                    else
                        // Never pass null: Notiffy checks "iconFilePath != \"\"", so null is
                        // treated as a real path and LoadNewSprite(null) throws.
                        args[i] = string.Empty;
                    stringIndex++;
                }
                else if (t == typeof(Sprite))
                    args[i] = null;
                else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    args[i] = actions;
                else if (name == "timeout" || name == "expiretimeout" || name == "expiretime")
                {
                    // Notiffy follows the notify-send convention: timeout in milliseconds, -1 = never expire.
                    double ms = Math.Round(duration * 1000.0);
                    if (t == typeof(int)) args[i] = (int)ms;
                    else if (t == typeof(long)) args[i] = (long)ms;
                    else if (t == typeof(uint)) args[i] = (uint)ms;
                    else if (t == typeof(float)) args[i] = (float)ms;
                    else if (t == typeof(double)) args[i] = ms;
                    else if (p.HasDefaultValue) args[i] = p.DefaultValue;
                    else args[i] = null;
                }
                else if (p.HasDefaultValue)
                    args[i] = p.DefaultValue;
                else if (t.IsValueType)
                    args[i] = Activator.CreateInstance(t);
                else
                    args[i] = null;
            }
            return args;
        }

        private void SendCore(string title, string body, string actionLabel, NotificationAction action, float duration)
        {
            Dictionary<string, string> actions = null;
            if (action != null)
                actions = new Dictionary<string, string> { { ActionId, actionLabel ?? "OK" } };

            try
            {
                object result = _notifySendMethod.Invoke(null, BuildArgs(title, body, actions, duration));
                Logging.Message("[Notiffy] Sent: " + title);

                if (action == null)
                    return;

                uint id = 0;
                if (result is uint u) id = u;
                else if (result != null && uint.TryParse(result.ToString(), out uint parsed)) id = parsed;
                if (id == 0)
                    return;

                lock (_pendingLock)
                {
                    if (_pendingActions.Count >= MaxPendingActions)
                        _pendingActions.Clear();
                    _pendingActions[id] = action;
                }
            }
            catch (Exception e)
            {
                Logging.Warn("Notiffy NotifySend failed: " + e.Message);
            }
        }

        public void Info(string title, string body, float duration = 4f) => SendCore(title, body, null, null, duration);
        public void Success(string title, string body, float duration = 4f) => SendCore(title, body, null, null, duration);
        public void Warning(string title, string body, float duration = 4f) => SendCore(title, body, null, null, duration);
        public void Error(string title, string body, float duration = 4f) => SendCore(title, body, null, null, duration);

        public void Info(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => SendCore(title, body, actionLabel, action, duration);
        public void Success(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => SendCore(title, body, actionLabel, action, duration);
        public void Warning(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => SendCore(title, body, actionLabel, action, duration);
        public void Error(string title, string body, string actionLabel, NotificationAction action, float duration = 6f) => SendCore(title, body, actionLabel, action, duration);
    }
}