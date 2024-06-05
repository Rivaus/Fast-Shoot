using umi3d.edk;
using umi3d.edk.userCapture.binding;
using UnityEngine;

namespace com.quentintran.gun
{
    public class Weapon : MonoBehaviour
    {
        [field: SerializeField]
        public UMI3DModel Model { get; private set; }

        [field:SerializeField]
        public float FireRate { get; private set; } = .2f;

        [field: SerializeField]
        public float Damage { get; private set; } = 35f;

        [field: SerializeField]
        public Transform AimTransform { get; private set; } = null;

        [field: SerializeField]
        public float Range { get; private set; } = 30f;

        [field: SerializeField]
        public Vector3 DesktopPositionOffset { get; private set; }

        [field: SerializeField]
        public Vector3 DesktopRotationOffset { get; private set; }

        [field: SerializeField]
        public Vector3 VRPositionOffset { get; private set; }

        [field: SerializeField]
        public Vector3 VRRotationOffset { get; private set; }
    }
}
