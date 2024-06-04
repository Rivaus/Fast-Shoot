using System;
using umi3d.edk;

namespace com.quentintran.notification
{
    public class NotificationService : INotificationService
    {
        public event Action<ulong, string, float> OnMessageEmitted;

        void INotificationService.NotifyUsers(string message, float timeSeconds)
        {
            foreach (UMI3DUser user in UMI3DServer.Instance.UserSetWhenHasJoined())
            {
                (this as INotificationService).NotifyUser(message, user.Id(), timeSeconds);
            }
        }

        void INotificationService.NotifyUser(string message, ulong userId, float timeSeconds)
        {
            this.OnMessageEmitted?.Invoke(userId, message, timeSeconds);
        }
    }
}