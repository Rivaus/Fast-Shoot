using System;

namespace com.quentintran.notification
{
    public interface INotificationService
    {
        event Action<ulong, string, float> OnMessageEmitted;

        void NotifyUser(string message, ulong userId, float timeSeconds);

        void NotifyUsers(string message, float timeSeconds);
    }
}

