using com.quentintran.notification;
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.edk;
using umi3d.edk.binding;
using umi3d.edk.collaboration;
using umi3d.edk.userCapture.binding;
using UnityEngine;

namespace com.quentintran.player
{

    public class PlayerNotification : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private UIText text;

        [SerializeField]
        private UMI3DNode node;

        [SerializeField]
        private Vector3 uiPositionOffset = Vector3.zero;

        private INotificationService notificationService;

        private UMI3DUser user;

        private Queue<(string message, float time)> notifications = new();

        #endregion

        #region Methods

        private void Awake()
        {
            Debug.Assert(text != null);
            Debug.Assert(node != null);
            this.text.Text.SetValue("");

            StartCoroutine(DisplayNotifications());
        }

        public List<Operation> Init(UMI3DUser user, INotificationService notificationService)
        {
            this.user = user;
            this.notificationService = notificationService;
            this.notificationService.OnMessageEmitted += MessageEmitted;

            List<Operation> operations = new List<Operation>();

            foreach (UMI3DLoadableEntity entity in GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                operations.Add(entity.GetLoadEntity());
            }

            BoneBinding binding = new(node.Id(), user.Id(), BoneType.Viewpoint)
            {
                offsetPosition = uiPositionOffset,
                offsetRotation = Quaternion.identity,
                syncRotation = true,
                syncPosition = true,
                bindToController = (user as UMI3DCollaborationUser).HasHeadMountedDisplay,
            };
            operations.AddRange(BindingManager.Instance.AddBinding(binding));

            return operations;
        }

        private void MessageEmitted(ulong userId, string message, float time)
        {
            if (userId != this.user.Id())
                return;

            notifications.Enqueue((message, time));
        }

        private IEnumerator DisplayNotifications()
        {
            WaitForSeconds wait = new(1 / 5f);

            while (true)
            {
                yield return null;

                if (notifications.Count > 0)
                {
                    (string message, float time) = notifications.Dequeue();

                    Transaction transaction = new() { reliable = true };
                    transaction.AddIfNotNull(text.Text.SetValue(message));
                    transaction.Dispatch();

                    yield return new WaitForSeconds(time);

                    transaction = new() { reliable = true };
                    transaction.AddIfNotNull(text.Text.SetValue(string.Empty));
                    transaction.Dispatch();
                }
            }
        }

        private void OnDestroy()
        {
            this.notificationService.OnMessageEmitted += MessageEmitted;
        }

        #endregion
    }
}