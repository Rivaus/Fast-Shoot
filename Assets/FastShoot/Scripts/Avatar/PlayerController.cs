using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking;
using umi3d.edk;
using umi3d.edk.binding;
using umi3d.edk.userCapture.binding;
using umi3d.edk.userCapture.tracking;
using UnityEngine;

namespace com.quentintran.player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        UIText nameTag = null;

        [SerializeField]
        UMI3DNode node = null;

        [SerializeField]
        UMI3DModel avatar = null;

        [SerializeField]
        UMI3DAudioPlayer stepAudioSource = null;

        [SerializeField]
        private Vector3 avatarPositionOffset = Vector3.zero, uiPositionOffset = Vector3.zero;

        private UMI3DTrackedUser user;

        private UserTrackingFrameDto trackingFrame;

        private void Awake()
        {
            Debug.Assert(node != null);
            Debug.Assert(avatar != null);
            Debug.Assert(nameTag != null);
            Debug.Assert(stepAudioSource != null);
        }

        internal List<Operation> Init(UMI3DUser user, string username, Vector3 spawnPosition)
        {
            List<Operation> operations = new List<Operation>();

            this.user = user as UMI3DTrackedUser;

            nameTag.Text.SetValue(username);
            avatar.objectActive.SetValue(user, false);

            foreach(UMI3DLoadableEntity entity in GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                operations.Add(entity.GetLoadEntity());
            }

            BoneBinding binding = new(node.Id(), user.Id(), BoneType.Hips)
            {
                offsetPosition = avatarPositionOffset,
                offsetRotation = Quaternion.identity,
                syncRotation = true,
                syncPosition = true
            };
            operations.AddRange(BindingManager.Instance.AddBinding(binding));

            TeleportRequest tp = new (spawnPosition, Quaternion.identity) { users = new HashSet<UMI3DUser>() { user } };
            operations.Add(tp);

            return operations;
        }

        internal void UpdateTransform()
        {
            trackingFrame = user.CurrentTrackingFrame;

            if (user is null || trackingFrame is null)
                return;

            transform.SetPositionAndRotation(trackingFrame.position.Struct(), trackingFrame.rotation.Quaternion());
        }
    }
}
