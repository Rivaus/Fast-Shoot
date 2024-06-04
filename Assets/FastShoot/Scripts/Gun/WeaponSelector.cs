using com.quentintran.player;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.interaction;
using UnityEngine;

namespace com.quentintran.gun
{

    public class WeaponSelector : MonoBehaviour
    {
        [SerializeField]
        private Weapon weapon;

        [SerializeField]
        private UMI3DEvent ev;

        [SerializeField]
        private PlayerManager playerManager;

        private void Awake()
        {
            Debug.Assert(weapon != null);
            Debug.Assert(ev != null);
            Debug.Assert(playerManager != null);

            ev.onTrigger.AddListener(content =>
            {
                playerManager.GetPlayer(content.user.Id())?.Equip(weapon, content.boneType);
            });
        }
    }
}
