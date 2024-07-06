using inetum.unityUtils;
using System;
using umi3d.edk;

public class ScoreManager : SingleBehaviour<ScoreManager>
{
    public event Action<UMI3DUser> OnUserDie, OnUserKill;
    public event Action<UMI3DUser, float> OnUserScore;

    public void UserScore(UMI3DUser user, float score)
    {
        OnUserScore?.Invoke(user, score);
    }

    public void UserDie(UMI3DUser user)
    {
        this.OnUserDie?.Invoke(user);
    }

    public void UserKill(UMI3DUser user)
    {
        this.OnUserKill?.Invoke(user);
    }
}