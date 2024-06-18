using umi3d.edk.binding;
using umi3d.edk;
using umi3d.edk.interaction;
using umi3d.edk.userCapture.binding;
using UnityEngine;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;
using umi3d.edk.userCapture.tracking;
using System.Collections.Generic;
using static umi3d.edk.interaction.AbstractInteraction;
using System;
using UnityEngine.UIElements;
using com.quentintran.player;
using com.quentintran.server.shoot;

namespace com.quentintran.gun
{
    public class PlayerWeapon : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Current weapon
        /// </summary>
        private Weapon weapon;

        /// <summary>
        /// Interactable to shoot.
        /// </summary>
        [SerializeField]
        private UMI3DInteractable interactable = null;

        /// <summary>
        /// Alternative interactable to shoot.
        /// </summary>
        [SerializeField]
        private UMI3DInteractable alternativeInteractable = null;

        [SerializeField]
        private UMI3DEvent shootEvent, alternativeShootEvent = null;

        /// <summary>
        /// Audio source for shoot sound effect.
        /// </summary>
        [SerializeField]
        private UMI3DAudioPlayer audioSource = null;

        /// <summary>
        /// Audio source for error sound effect.
        /// </summary>
        [SerializeField]
        private UMI3DAudioPlayer errorAudioSource = null;

        /// <summary>
        /// Current user who has the weapon
        /// </summary>
        private UMI3DTrackedUser user;

        private BoneBinding binding = null;

        /// <summary>
        /// Last time user shoot.
        /// </summary>
        float lastTimeShoot = 0f;

        #endregion

        #region Methods

        private void Start()
        {
            Debug.Assert(alternativeInteractable != null);
            Debug.Assert(interactable != null);
            Debug.Assert(alternativeShootEvent != null);
            Debug.Assert(shootEvent != null);
            Debug.Assert(audioSource != null);
            Debug.Assert(errorAudioSource != null);

            this.shootEvent.onTrigger.AddListener(Shoot);
            this.alternativeShootEvent.onTrigger.AddListener(Shoot);
        }

        public void EquipWeapon(UMI3DTrackedUser user, Weapon weaponTemplate, uint boneType)
        {
            this.user = user;

            Transaction transaction = new() { reliable = true };

            if (this.binding is not null)
            {
                transaction.AddIfNotNull(BindingManager.Instance.RemoveBinding(this.binding));

                foreach (UMI3DLoadableEntity entity in this.weapon.GetComponentsInChildren<UMI3DLoadableEntity>())
                {
                    transaction.AddIfNotNull(entity.GetDeleteEntity());
                }

                GameObject.Destroy(this.weapon.gameObject);
            }

            Weapon weaponGo = GameObject.Instantiate(weaponTemplate);
            weaponGo.transform.SetParent(this.transform);

            foreach (UMI3DLoadableEntity entity in weaponGo.GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                transaction.AddIfNotNull(entity.GetLoadEntity());
            }

            binding = new(weaponGo.Model.Id(), this.user.Id(), boneType)
            {
                syncPosition = true,
                syncRotation = true,
                offsetPosition = (user.HasHeadMountedDisplay) ? weaponGo.VRPositionOffset : weaponGo.DesktopPositionOffset,
                offsetRotation = (user.HasHeadMountedDisplay) ? Quaternion.Euler(weaponGo.VRRotationOffset) : Quaternion.Euler(weaponGo.DesktopRotationOffset),
                bindToController = true,
            };

            transaction.AddIfNotNull(BindingManager.Instance.AddBinding(binding));
            transaction.AddIfNotNull(this.audioSource.ObjectAudioResource.SetValue(weaponGo.ShootSound));
            transaction.Dispatch();

            this.weapon = weaponGo;
        }

        UserTrackingFrameDto trackingFrame;
        ControllerDto controller;
        Vector3 pos;
        Quaternion rot;

        public void UpdateTransform()
        {
            if (this.weapon is null)
                return;

            trackingFrame = user.CurrentTrackingFrame;

            controller = trackingFrame.trackedBones.Find(bone => bone.boneType == this.binding.boneType);
            pos = controller.position.Struct();
            rot = controller.rotation.Quaternion();

            if (controller is not null)
            {
                weapon.transform.SetPositionAndRotation(pos + rot * binding.offsetPosition, rot * binding.offsetRotation);
            }
        }

        public void Enable()
        {
            if (this.weapon is null)
            {
                Debug.LogError("Impossible to enable weapon because no one is equipped");
                return;
            }

            Transaction transaction = new() { reliable = true };

            transaction.AddIfNotNull(this.interactable.GetProjectTool(false, new HashSet<UMI3DUser> { this.user }));
            if (user.HasHeadMountedDisplay)
                transaction.AddIfNotNull(this.alternativeInteractable.GetProjectTool(false, new HashSet<UMI3DUser> { this.user }));

            transaction.Dispatch();
        }

        public void Disable()
        {
            Transaction transaction = new() { reliable = true };

            transaction.AddIfNotNull(this.interactable.GetReleaseTool(new HashSet<UMI3DUser> { this.user }));
            if (user.HasHeadMountedDisplay)
                transaction.AddIfNotNull(this.alternativeInteractable.GetReleaseTool(new HashSet<UMI3DUser> { this.user }));

            transaction.Dispatch();
        }

        private void Shoot(InteractionEventContent content)
        {
            if (this.weapon is null)
                return;

            if(Time.time < lastTimeShoot + this.weapon.FireRate)
            {
                Transaction t = new() { reliable = true };
                t.AddIfNotNull(this.errorAudioSource.objectPlaying.SetValue(false));
                t.AddIfNotNull(this.errorAudioSource.objectPlaying.SetValue(true));
                t.Dispatch();

                return;
            }

            lastTimeShoot = Time.time;

            Ray ray;
            Quaternion boneRot = content.boneRotation.Quaternion();

            if (user.HasHeadMountedDisplay)
            {
                if (binding.boneType == content.boneType)
                    weapon.transform.SetPositionAndRotation(content.bonePosition.Struct() + boneRot * binding.offsetPosition, boneRot * binding.offsetRotation);

                ray = new Ray(this.weapon.AimTransform.position, this.weapon.AimTransform.right);
            }
            else
                ray = new Ray(content.bonePosition.Struct() + boneRot * (Vector3.forward * .3f), boneRot * Vector3.forward);

            Debug.DrawRay(ray.origin, ray.direction * this.weapon.Range, Color.red, .5f);

            if (Physics.Raycast(ray, out RaycastHit hit, this.weapon.Range))
            {
                if (hit.transform.TryGetComponent(out PlayerController otherPlayer) && otherPlayer.User != this.user)
                {
                    if (otherPlayer.User != this.user)
                    {
                        otherPlayer.Hit(user, this.weapon.Damage);
                        Debug.Log("Player hit " + otherPlayer.Username);
                    }
                }
                else
                {
                    DecalManager.Instance.DisplayBulletDecal(hit.point, hit.normal);
                }
            }

            Transaction transaction = new() { reliable = true };
            transaction.AddIfNotNull(this.errorAudioSource.objectPlaying.SetValue(false));
            transaction.AddIfNotNull(this.audioSource.objectPlaying.SetValue(false));
            transaction.AddIfNotNull(this.audioSource.objectPlaying.SetValue(true));
            transaction.Dispatch();
        }

        internal IEnumerable<Operation> GetDelete()
        {
            List<Operation> operations = new();

            if (this.binding is not null)
                operations.AddRange(BindingManager.Instance.RemoveBinding(this.binding));

            return operations;
        }

        #endregion
    }

}
