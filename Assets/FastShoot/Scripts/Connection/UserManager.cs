using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.interaction;
using umi3d.edk;
using umi3d.edk.interaction;
using UnityEngine;

namespace com.quentintran.connection
{
    internal class UserManager : SingleBehaviour<UserManager>, IUserManagerService
    {
        [SerializeField]
        private ConnectionIdentifier identifer = null;

        private Dictionary<ulong, UMI3DUser> users = new();
        private Dictionary<ulong, string> userNames = new();

        public event Action<UMI3DUser> OnUserJoin, OnUserLeave;

        protected override void Awake()
        {
            base.Awake();

            identifer?.Init(GenerateForm());

            UMI3DServer.Instance.OnUserLeave.AddListener(OnUserLeaveListener);
            UMI3DServer.Instance.OnUserActive.AddListener(user =>
            {
                try
                {
                    OnUserJoin?.Invoke(user);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            });
        }

        private UMI3DForm GenerateForm()
        {
            var form = gameObject.AddComponent<UMI3DForm>();
            form.Display.name = "Connexion au serveur Fast Shoot";

            var pseudoField = gameObject.AddComponent<StringParameter>();
            pseudoField.Display.name = "Pseudo qui te donne l'air cool";
            pseudoField.value = string.Empty;

            form.Fields = new List<AbstractParameter>() { pseudoField };

            return form;
        }

        public bool IsUserExisting(ulong id)
        {
            return users.ContainsKey(id);
        }

        public UMI3DUser GetUser(ulong id)
        {
            if (IsUserExisting(id))
                return users[id];
            else return null;
        }

        public void RegisterUser(UMI3DUser user)
        {
            if (user == null)
                return;

            if (!IsUserExisting(user.Id()))
            {
                users.Add(user.Id(), user);
            }
            else
            {
                Debug.LogError("[UserManager.RegisterUser] Impossible to register user !");
            }
        }

        internal bool AreUserConnectionDataValid(FormAnswerDto dto)
        {
            if (dto.answers.Count == 0) return false;

            if (dto.answers[0] == null || dto.answers[0].parameter as string == null) return false;

            string pseudo = (dto.answers[0].parameter as string).Trim();

            if (string.IsNullOrEmpty(pseudo))
            {
                Debug.LogError("[UserManager.RegisterUser] Empty pseudo !");
            }
            else if (userNames.Values.Contains(pseudo))
            {
                Debug.LogError("[UserManager.RegisterUser] Empty already taken !");
            }
            else
            {
                return true;
            }

            return false;
        }

        internal void UpdateUserData(ulong id, FormAnswerDto dto)
        {
            string pseudo = (dto.answers[0].parameter as string).Trim();

            userNames[id] = pseudo;

            Debug.Log($"[UserManager.UpdateUserData] User enters { pseudo} with id { id }." );
        }

        private void OnUserLeaveListener(UMI3DUser user)
        {
            if (users.ContainsKey(user.Id()))
            {
                try
                {
                    OnUserLeave?.Invoke(user);
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }

                users.Remove(user.Id());
                userNames.Remove(user.Id());
            }
        }

        public string GetUserName(ulong id)
        {
            return userNames.ContainsKey(id) ? userNames[id] : string.Empty;
        }
    }
}