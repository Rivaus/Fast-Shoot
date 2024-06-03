using umi3d.edk;

namespace com.quentintran.connection
{
    internal interface IUserManagerService
    {
        event System.Action<UMI3DUser> OnUserJoin, OnUserLeave;

        UMI3DUser? GetUser(ulong userId);

        bool IsUserExisting(ulong userId);

        void RegisterUser(UMI3DUser user);

        string GetUserName(ulong userId);
    }
}

