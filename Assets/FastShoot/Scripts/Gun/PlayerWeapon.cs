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

namespace com.quentintran.gun
{
    public class PlayerWeapon : MonoBehaviour
    {
        private Weapon weapon;

        [SerializeField]
        private UMI3DInteractable interactable = null;

        [SerializeField]
        private UMI3DEvent shootEvent = null;

        [SerializeField]
        private UMI3DAudioPlayer audioSource = null;

        private UMI3DTrackedUser user;

        private BoneBinding binding = null;

        private void Start()
        {
            this.shootEvent.onTrigger.AddListener(Shoot);
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

            Debug.Log("BINDING " + binding.bindToController);

            transaction.AddIfNotNull(BindingManager.Instance.AddBinding(binding));
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

            if (user.HasHeadMountedDisplay)
            {
                Debug.Log("TODO");
            }

            transaction.AddIfNotNull(this.interactable.GetProjectTool(false, new HashSet<UMI3DUser> { this.user }));

            transaction.Dispatch();
        }

        public void Disable()
        {

        }

        private void Shoot(InteractionEventContent content)
        {
            if (this.weapon is null)
                return;

            if (user.HasHeadMountedDisplay)
            {
                Debug.Log("TODO");
            }
            else
            {
                Debug.Log("SHOOOT");
                Debug.DrawRay(controller.position.Struct(), controller.rotation.Quaternion() * Vector3.forward * this.weapon.Range, Color.red, .5f);
            }
        }

        internal IEnumerable<Operation> GetDelete()
        {
            List<Operation> operations = new();

            if (this.binding is not null)
                operations.AddRange(BindingManager.Instance.RemoveBinding(this.binding));

            return operations;
        }
    }

}
