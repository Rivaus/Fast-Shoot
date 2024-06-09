using com.quentintran.gun;
using System.Collections.Generic;
using System.Transactions;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking;
using umi3d.edk;
using umi3d.edk.binding;
using umi3d.edk.userCapture.binding;
using umi3d.edk.userCapture.tracking;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
        private Vector3 avatarPositionOffset = Vector3.zero;

        [SerializeField]
        private PlayerWeapon weaponController = null;

        public UMI3DTrackedUser User { get; private set; }

        private BoneBinding avatarBinding = null;

        private UserTrackingFrameDto trackingFrame;

        public bool IsReady { get; private set; } = false;

        public string Username { get; private set; }

        private bool isWalking = false;

        public bool IsWalking
        {
            get => this.isWalking;

            set
            {
                if (this.isWalking != value)
                {
                    Transaction transaction = new() { reliable = value };
                    transaction.AddIfNotNull(this.stepAudioSource.objectPlaying.SetValue(value));
                    transaction.Dispatch();

                    this.isWalking = value;
                }
            }
        }

        #endregion

        #region Fields

        private void Awake()
        {
            Debug.Assert(node != null);
            Debug.Assert(avatar != null);
            Debug.Assert(nameTag != null);
            Debug.Assert(stepAudioSource != null);
            Debug.Assert(weaponController != null);
        }

        internal List<Operation> Init(UMI3DUser user, string username, Vector3 spawnPosition)
        {
            List<Operation> operations = new List<Operation>();

            this.User = user as UMI3DTrackedUser;
            this.Username = username;

            nameTag.Text.SetValue(username);
            avatar.objectActive.SetValue(user, false);
            stepAudioSource.ObjectVolume.SetValue(user, .5f);

            foreach (UMI3DLoadableEntity entity in GetComponentsInChildren<UMI3DLoadableEntity>())
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

            this.IsWalking = trackingFrame.grounded && trackingFrame.speed.Struct().magnitude > 0.01f;

            transform.SetPositionAndRotation(trackingFrame.position.Struct(), trackingFrame.rotation.Quaternion());

            this.weaponController.UpdateTransform();
        }

        public void Equip(Weapon weaponTemplate, uint boneType)
        {
            weaponController.EquipWeapon(this.User, weaponTemplate, boneType);

            this.IsReady = true;
        }

        public List<Operation> GetDelete()
        {
            List<Operation> operations = new();

            operations.AddRange(this.weaponController.GetDelete());
            operations.AddRange(BindingManager.Instance.RemoveBinding(avatarBinding) ?? new List<Operation>());

            foreach (UMI3DLoadableEntity entity in GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                operations.Add(entity.GetDeleteEntity());
            }

            GameObject.Destroy(this.gameObject);

            return operations;
        }

        public void EnableForParty()
        {
            this.weaponController.Enable();
        }

        public void Disable()
        {
            this.weaponController.Disable();
        }

        #endregion
    }
}
