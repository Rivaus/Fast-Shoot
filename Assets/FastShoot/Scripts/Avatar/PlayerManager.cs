using com.quentintran.connection;
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

        [SerializeField]
        UMI3DScene playerScene = null;

        [SerializeField]
        Transform spawnPosition;

        [Space]
        [SerializeField]
        PlayerController playerTemplate = null;

        Dictionary<ulong, PlayerController> playerControllers = new();

        private void Awake()
        {
            Debug.Assert(userManager != null);
            Debug.Assert(playerScene != null);
            Debug.Assert(spawnPosition != null);
            Debug.Assert(playerTemplate != null);

            userManagerService = userManager;

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

            string username = userManagerService.GetUserName(user.Id());

            PlayerController controller = GameObject.Instantiate(playerTemplate);
            controller.gameObject.name = "Player--" + username;
            controller.transform.SetParent(playerScene.transform, false);
            controller.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            playerControllers.Add(user.Id(), controller);
            controller.Init(user, username, spawnPosition.position);
        }

        private void OnUserLeave(UMI3DUser user)
        {

        }
    }
}

