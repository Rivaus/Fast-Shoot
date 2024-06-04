using com.quentintran.connection;
using com.quentintran.notification;
using System;
using System.Collections.Generic;
using umi3d.edk;
using UnityEngine;

namespace com.quentintran.player
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField]
        UserManager userManager = null;

        IUserManagerService userManagerService = null;
        INotificationService notificationService = null;

        [SerializeField]
        UMI3DScene playerScene = null;

        [SerializeField]
        Transform spawnPosition;

        [Space]
        [SerializeField]
        PlayerController playerTemplate = null;
        [SerializeField]
        PlayerNotification playerNotificationTemplate = null;

        Dictionary<ulong, PlayerController> playerControllers = new();
        Dictionary<ulong, PlayerNotification> playerNotifications = new();

        private void Awake()
        {
            Debug.Assert(userManager != null);
            Debug.Assert(playerScene != null);
            Debug.Assert(spawnPosition != null);
            Debug.Assert(playerTemplate != null);
            Debug.Assert(playerNotificationTemplate != null);

            userManagerService = userManager;
            notificationService = new NotificationService();

            userManagerService.OnUserJoin += OnUserJoin;
            userManagerService.OnUserLeave += OnUserLeave;
        }

        private void Update()
        {
            foreach (PlayerController controller in playerControllers.Values)
                controller.UpdateTransform();
        }

        private void OnUserJoin(UMI3DUser user)
        {
            if (playerControllers.ContainsKey(user.Id()))
                throw new NotImplementedException();

            Transaction transaction = new() { reliable = true };

            string username = userManagerService.GetUserName(user.Id());

            PlayerController controller = GameObject.Instantiate(playerTemplate);
            controller.gameObject.name = "Player--" + username;
            controller.transform.SetParent(playerScene.transform, false);
            controller.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            playerControllers.Add(user.Id(), controller);
            transaction.AddIfNotNull(controller.Init(user, username, spawnPosition.position));


            PlayerNotification notification = GameObject.Instantiate(playerNotificationTemplate);
            notification.gameObject.name = "PlayerNotification--" + username;
            notification.transform.SetParent(playerScene.transform, false);
            notification.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            playerNotifications.Add(user.Id(), notification);
            transaction.AddIfNotNull(notification.Init(user, notificationService));

            transaction.Dispatch();
        }

        private void OnUserLeave(UMI3DUser user)
        {

        }

        [ContextMenu("Fake Notification")]
        private void SendNotificationTest()
        {
            notificationService.NotifyUsers("Hello !", 6);
        }
    }
}

