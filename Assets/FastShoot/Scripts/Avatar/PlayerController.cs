using com.quentintran.gun;
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
        #region Fields

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

        [SerializeField]
        private PlayerWeapon currentWeapon = null;

        public UMI3DTrackedUser User { get; private set; }

        private BoneBinding avatarBinding = null;

        private UserTrackingFrameDto trackingFrame;

        public bool IsReady { get; private set; } = false;

        public string Username { get; private set; }

        #endregion

        #region Fields

        private void Awake()
        {
            Debug.Assert(node != null);
            Debug.Assert(avatar != null);
            Debug.Assert(nameTag != null);
            Debug.Assert(stepAudioSource != null);
            Debug.Assert(currentWeapon != null);
        }

        internal List<Operation> Init(UMI3DUser user, string username, Vector3 spawnPosition)
        {
            List<Operation> operations = new List<Operation>();

            this.User = user as UMI3DTrackedUser;
            this.Username = username;

            nameTag.Text.SetValue(username);
            avatar.objectActive.SetValue(user, false);

            foreach(UMI3DLoadableEntity entity in GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                operations.Add(entity.GetLoadEntity());
            }

            avatarBinding = new(node.Id(), user.Id(), BoneType.Hips)
            {
                offsetPosition = avatarPositionOffset,
                offsetRotation = Quaternion.identity,
                syncRotation = true,
                syncPosition = true
            };
            operations.AddRange(BindingManager.Instance.AddBinding(avatarBinding));

            TeleportRequest tp = new (spawnPosition, Quaternion.identity) { users = new HashSet<UMI3DUser>() { user } };
            operations.Add(tp);

            return operations;
        }

        internal void UpdateTransform()
        {
            trackingFrame = User.CurrentTrackingFrame;

            if (User is null || trackingFrame is null)
                return;

            transform.SetPositionAndRotation(trackingFrame.position.Struct(), trackingFrame.rotation.Quaternion());

            this.currentWeapon.UpdateTransform();
        }

        public void Equip(Weapon weaponTemplate, uint boneType)
        {
            currentWeapon.EquipWeapon(this.User, weaponTemplate, boneType);
            currentWeapon.Enable();

            this.IsReady = true;
        }

        public List<Operation> GetDelete()
        {
            List<Operation> operations = new();

            operations.AddRange(this.currentWeapon.GetDelete());
            operations.AddRange(BindingManager.Instance.RemoveBinding(avatarBinding) ?? new List<Operation>());

            foreach (UMI3DLoadableEntity entity in GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                operations.Add(entity.GetDeleteEntity());
            }

            GameObject.Destroy(this.gameObject);

            return operations;
        }

        #endregion
    }
}
