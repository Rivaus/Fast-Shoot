using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.quentintran
{
    public class DisableVolumeCollider : MonoBehaviour
    {
        private void Awake()
        {
            foreach(Volume v in GetComponentsInChildren<Volume>())
            {
                if (v.gameObject.TryGetComponent(out Collider collider))
                    collider.enabled = false;
            }
        }
    }
}
