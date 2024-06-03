using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using umi3d.common;
using umi3d.edk.collaboration;
using UnityEngine;
using umi3d.edk.interaction;

namespace com.quentintran.connection
{
    /// <summary>
    /// Identifier that requires to fill a form to connect.
    /// </summary>
    [CreateAssetMenu(fileName = "ConnectionIdentifier", menuName = "UMI3D/Connection Identifier")]
    internal class ConnectionIdentifier : PinIdentifierApi
    {
        UMI3DForm form = null;

        internal void Init(UMI3DForm form)
        {
            this.form = form;
        }

        /// <inheritdoc/>
        public override ConnectionFormDto GetParameterDtosFor(UMI3DCollaborationUser user)
        {
            return form.ToConnectionFormDto(user);
        }

        /// <inheritdoc/>
        public override StatusType UpdateIdentity(UMI3DCollaborationUser user, UserConnectionAnswerDto identity)
        {
            StatusType res;

            if (UserManager.Instance.IsUserExisting(user.Id()))
            {
                if (user.status == StatusType.ACTIVE)
                {
                    Debug.LogError("User already connected ! ");
                    res = StatusType.ACTIVE;
                }
                else if (string.IsNullOrEmpty(UserManager.Instance.GetUserName(user.Id())))
                {
                    if (!UserManager.Instance.AreUserConnectionDataValid(identity.parameters))
                    {
                        res = StatusType.CREATED;
                    }
                    else
                    {
                        UserManager.Instance.UpdateUserData(user.Id(), identity.parameters);

                        res = base.UpdateIdentity(user, identity);
                    }
                }
                else
                {
                    res = base.UpdateIdentity(user, identity);
                }
            }
            else
            {
                UserManager.Instance.RegisterUser(user);
                res = StatusType.CREATED;
            }

            return res;
        }
    }
}