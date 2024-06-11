using com.quentintran.connection;
using com.quentintran.notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.edk;
using umi3d.edk.interaction;
using UnityEngine;
using static umi3d.edk.interaction.AbstractInteraction;

namespace com.quentintran.player
{
    public class PlayerManager : MonoBehaviour
    {
        #region Fields

        private static PlayerManager instance;

        public static INotificationService NotificationService => instance.notificationService;

        [SerializeField]
        UserManager userManager = null;

        IUserManagerService userManagerService = null;
        INotificationService notificationService = null;

        [SerializeField]
        UMI3DScene playerScene = null;

        [SerializeField]
        Transform spawnPosition;

        [SerializeField]
        Transform[] spawns;

        [SerializeField]
        UMI3DNode spawnPoint;

        [SerializeField]
        UMI3DEvent spawnPointEvent;

        [Space]
        [SerializeField]
        PlayerController playerTemplate = null;
        [SerializeField]
        PlayerNotification playerNotificationTemplate = null;

        Dictionary<ulong, PlayerController> playerControllers = new();
        Dictionary<ulong, PlayerNotification> playerNotifications = new();

        private bool isStarted = false;

        #endregion

        #region Fields

        public PlayerController? GetPlayer(ulong userId)
        {
            playerControllers.TryGetValue(userId, out PlayerController playerController);

            return playerController;
        }

        private void Awake()
        {
            Debug.Assert(userManager != null);
            Debug.Assert(playerScene != null);
            Debug.Assert(spawnPosition != null);
            Debug.Assert(playerTemplate != null);
            Debug.Assert(playerNotificationTemplate != null);
            Debug.Assert(spawns.Length > 0);
            Debug.Assert(spawnPoint != null);
            Debug.Assert(spawnPointEvent != null);

            userManagerService = userManager;
            notificationService = new NotificationService();

            userManagerService.OnUserJoin += OnUserJoin;
            userManagerService.OnUserLeave += OnUserLeave;

            instance = this;
        }

        private void Start()
        {
            UMI3DEnvironment.objectStartPosition.SetValue(spawnPosition.position);
            UMI3DEnvironment.objectStartOrientation.SetValue(spawnPosition.rotation);

            spawnPoint.objectActive.SetValue(false);
            spawnPointEvent.onTrigger.AddListener(SpawnPlayer);
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
            controller.OnPlayerDie += OnPlayerDie;

            playerControllers.Add(user.Id(), controller);
            transaction.AddIfNotNull(controller.Init(user, username));

            PlayerNotification notification = GameObject.Instantiate(playerNotificationTemplate);
            notification.gameObject.name = "PlayerNotification--" + username;
            notification.transform.SetParent(playerScene.transform, false);
            notification.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            playerNotifications.Add(user.Id(), notification);
            transaction.AddIfNotNull(notification.Init(user, notificationService));

            if (isStarted)
                transaction.AddIfNotNull(spawnPoint.objectActive.SetValue(user, true));

            transaction.Dispatch();
        }

        private void OnPlayerDie(PlayerController playerDead)
        {
            playerDead.Disable();

            Transaction transaction = new() { reliable = true };
            transaction.AddIfNotNull(new TeleportRequest(spawnPosition.position, spawnPosition.rotation) { users = new HashSet<UMI3DUser>() { playerDead.User } });
            transaction.AddIfNotNull(spawnPoint.objectActive.SetValue(playerDead.User, true));

            transaction.Dispatch();
        }

        private void OnUserLeave(UMI3DUser user)
        {
            if (playerControllers.TryGetValue(user.Id(), out PlayerController controller))
            {
                Transaction transaction = new() { reliable = true };

                transaction.AddIfNotNull(controller.GetDelete());

                transaction.Dispatch();

                playerControllers.Remove(user.Id());
            }
            else
            {
                Debug.LogError("Impossible to delete player");
            }
        }

        [ContextMenu("Fake Notification")]
        private void SendNotificationTest()
        {
            notificationService.NotifyUsers("Hello !", 6);
        }

        [ContextMenu("Start Party")]
        public async void StartParty()
        {
            bool ready = true;

            foreach (PlayerController player in playerControllers.Values)
            {
                if (!player.IsReady)
                {
                    Debug.Log($"Cannot start game, {player.Username} is not ready");
                    ready = false;
                }
            }

            if (playerControllers.Count > spawns.Length)
            {
                Debug.LogError("Impossible to start game, not enough spawns");
                return;
            }

            if (ready)
            {
                this.notificationService.NotifyUsers("La repas va bientôt commencer !", 1f);
                await Task.Delay(1000);

                for (int i = 0; i < 5; i++)
                {
                    this.notificationService.NotifyUsers((5 - i).ToString(), 1f);
                    await Task.Delay(1000);
                }

                Transaction transaction = new () { reliable = true };

                IEnumerable<PlayerController> players = playerControllers.Values;
                PlayerController player;

                for (int i = 0; i < players.Count(); i++)
                {
                    player = players.ElementAt(i);
                    player.EnableForParty();
                    TeleportRequest tp = new(spawns[i].position, spawns[i].rotation) { users = new HashSet<UMI3DUser>() { player.User } };
                    transaction.AddIfNotNull(tp);
                }

                transaction.Dispatch();

                isStarted = true;
            }
        }

        private void SpawnPlayer(InteractionEventContent content)
        {
            Transaction transaction = new() { reliable = true };

            if (playerControllers.TryGetValue(content.user.Id(), out PlayerController player))
            {
                Transform spawn = spawns[UnityEngine.Random.Range(0, spawns.Length - 1)];
                TeleportRequest tp = new(spawn.position, spawn.rotation) { users = new HashSet<UMI3DUser>() { player.User } };
                transaction.AddIfNotNull(tp);
                player.EnableForParty();

                transaction.AddIfNotNull(spawnPoint.objectActive.SetValue(player.User, false));
            }
            else
                Debug.LogError("Internal error");


            transaction.Dispatch();
        }

        #endregion
    }
}

