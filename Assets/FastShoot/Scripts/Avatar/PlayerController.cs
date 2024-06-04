using com.quentintran.gun;
using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
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

        public UMI3DTrackedUser User { get; private set; }

        private UserTrackingFrameDto trackingFrame;

        private Weapon currentWeapon = null;

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
            trackingFrame = User.CurrentTrackingFrame;

            if (User is null || trackingFrame is null)
                return;

            transform.SetPositionAndRotation(trackingFrame.position.Struct(), trackingFrame.rotation.Quaternion());

            if (this.currentWeapon is not null)
            {
                ControllerDto controller = trackingFrame.trackedBones.Find(bone => bone.boneType == this.currentWeapon.Binding.boneType);
                BoneBinding binding = this.currentWeapon.Binding;
                Vector3 pos = controller.position.Struct();
                Quaternion rot = controller.rotation.Quaternion();

                if (controller is not null)
                {
                    this.currentWeapon.transform.SetPositionAndRotation(pos + rot * binding.offsetPosition, rot * binding.offsetRotation);
                }
            }
        }

        public void Equip(Weapon weapon, uint boneType)
        {
            Transaction transaction = new() { reliable = true };

            if (this.currentWeapon is not null)
            {
                transaction.AddIfNotNull(BindingManager.Instance.RemoveBinding(this.currentWeapon.Binding));

                foreach (UMI3DLoadableEntity entity in this.currentWeapon.GetComponentsInChildren<UMI3DLoadableEntity>())
                {
                    transaction.AddIfNotNull(entity.GetDeleteEntity());
                }
            }

            Weapon weaponGo = GameObject.Instantiate(weapon);
            weaponGo.transform.SetParent(this.transform);

            foreach (UMI3DLoadableEntity entity in weaponGo.GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                transaction.AddIfNotNull(entity.GetLoadEntity());
            }

            BoneBinding binding = new(weaponGo.Model.Id(), User.Id(), boneType)
            {
                syncPosition = true,
                syncRotation = true,
                offsetPosition = (User.HasHeadMountedDisplay) ? weaponGo.VRPositionOffset : weaponGo.DesktopPositionOffset,
                offsetRotation = (User.HasHeadMountedDisplay) ? Quaternion.Euler(weaponGo.VRRotationOffset) : Quaternion.Euler(weaponGo.DesktopRotationOffset),
                bindToController = true,
            };
            transaction.AddIfNotNull(BindingManager.Instance.AddBinding(binding));
            weaponGo.Binding = binding;

            transaction.Dispatch();

            this.IsReady = true;
            this.currentWeapon = weaponGo;
        }

        #endregion
    }
}
