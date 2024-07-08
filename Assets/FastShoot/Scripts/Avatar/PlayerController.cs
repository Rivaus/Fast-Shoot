using com.quentintran.connection;
using com.quentintran.gun;
using System.Collections;
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
        public System.Action<PlayerController> OnPlayerDie;

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

        [Space]
        [Header("Health")]
        [SerializeField]
        private UMI3DNode healthFeedback = null;

        [SerializeField]
        private UMI3DNode heathUIContainer = null;

        [SerializeField]
        private UMI3DNode[] damageFeedbacks = null;

        [SerializeField]
        private UMI3DNode damageContainer = null;

        [SerializeField]
        private Vector3 damageContainerOffset = Vector3.zero;

        [SerializeField]
        private float autoHealDelay = 1f;

        private Coroutine autoHealCoroutine;

        public UMI3DTrackedUser User { get; private set; }

        private BoneBinding avatarBinding, damageBinding = null;

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

        private const float MAX_HEALTH = 100;

        private float health;

        private float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, MAX_HEALTH);

                Transaction transaction = new() { reliable = true };
                transaction.AddIfNotNull(healthFeedback.objectScale.SetValue(new Vector3(health / MAX_HEALTH, 1, 0)));

                for (int i = 0; i < damageFeedbacks.Length; i++)
                {
                    float threshold = (i + 1) * (MAX_HEALTH / damageFeedbacks.Length);
                    transaction.AddIfNotNull(damageFeedbacks[i].objectActive.SetValue(this.User, threshold <= MAX_HEALTH - health));
                }

                transaction.Dispatch();

                if (health < MAX_HEALTH)
                {
                    if (autoHealCoroutine is not null)
                        StopCoroutine(autoHealCoroutine);

                    autoHealCoroutine = StartCoroutine(AutoHealCoroutine());
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
            Debug.Assert(heathUIContainer != null);
            Debug.Assert(damageContainer != null);
            Debug.Assert(damageFeedbacks.Length > 0);
        }

        internal List<Operation> Init(UMI3DUser user, string username)
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

            damageBinding = new(damageContainer.Id(), user.Id(), BoneType.Viewpoint)
            {
                offsetPosition = damageContainerOffset,
                offsetRotation = Quaternion.identity,
                syncRotation = true,
                syncPosition = true,
                bindToController = true
            };
            operations.AddRange(BindingManager.Instance.AddBinding(damageBinding));

            this.Health = MAX_HEALTH;

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
            operations.AddRange(BindingManager.Instance.RemoveBinding(damageBinding) ?? new List<Operation>());

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
            this.Health = MAX_HEALTH;
        }

        public void Disable()
        {
            this.weaponController.Disable();
        }

        public void Hit(UMI3DUser shooter, float damage)
        {
            this.Health -= damage;

            if (this.Health == 0)
            {
                string shooterName = UserManager.Instance.GetUserName(shooter.Id());

                PlayerManager.NotificationService.NotifyUser($"Retour en cuisine, { shooterName } te prive de repas.", this.User.Id(), 4f);
                PlayerManager.NotificationService.NotifyUser($"Tu viens de priver { this.Username } de dessert !", shooter.Id(), 4f);

                ScoreManager.Instance.UserDie(this.User);
                ScoreManager.Instance.UserKill(shooter);

                this.Health = MAX_HEALTH;

                this.OnPlayerDie?.Invoke(this);
            }
            else
            {
                DisplayHealthPointsForUser(shooter);
            }
        }

        Dictionary<UMI3DUser, Coroutine> coroutines = new();

        private void DisplayHealthPointsForUser(UMI3DUser shooter)
        {
            if (coroutines.TryGetValue(shooter, out Coroutine c))
            {
                StopCoroutine(c);
            }

            Coroutine display = StartCoroutine(DisplayHealthPointsForUserCoroutine(shooter));

            coroutines[shooter] = display;
        }

        private IEnumerator DisplayHealthPointsForUserCoroutine(UMI3DUser u)
        {
            Transaction transaction = new() { reliable = true };
            transaction.AddIfNotNull(heathUIContainer.objectActive.SetValue(u, true));
            transaction.Dispatch();

            yield return new WaitForSeconds(4f);

            transaction = new() { reliable = true };
            transaction.AddIfNotNull(heathUIContainer.objectActive.SetValue(u, false));
            transaction.Dispatch();
        }

        private IEnumerator AutoHealCoroutine()
        {
            yield return new WaitForSeconds(autoHealDelay);

            Health += 25f;

            this.autoHealCoroutine = null;
        }

        #endregion
    }
}
