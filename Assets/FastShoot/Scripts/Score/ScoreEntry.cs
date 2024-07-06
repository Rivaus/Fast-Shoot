using com.quentintran.connection;
using umi3d.edk;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace com.quentintran.score
{
    public class ScoreEntry : MonoBehaviour
    {
        public const float HEIGHT = 7,WIDTH = 170;

        public UMI3DNode node = null;

        [SerializeField]
        private UIText userNameText, nbOfKillText, nbOfDeathText, scoreText;

        [SerializeField]
        private Text userNameLabel, nbOfKillLabel, nbOfDeathLabel, scoreLabel;

        private UMI3DUser user;

        private int nbOfKill = 0;

        public int NbKill
        {
            get => nbOfKill;
            set => SetProperty(ref nbOfKill, value, nbOfKillText, nbOfKillLabel);
        }

        private int nbOfDeath = 0;

        public int NbDeath
        {
            get => nbOfDeath;
            set => SetProperty(ref nbOfDeath, value, nbOfDeathText, nbOfDeathLabel);
        }

        private int score = 0;

        public int Score
        {
            get => nbOfDeath;
            set => SetProperty(ref score, value, scoreText, scoreLabel);
        }

        private void SetProperty(ref int property, int newValue, UIText text, Text label)
        {
            if (newValue == property)
                return;

            property = newValue;

            Transaction t = new() { reliable = true };
            t.AddIfNotNull(text.Text.SetValue(newValue.ToString()));
            label.text = newValue.ToString();
            t.Dispatch();
        }

        public void Init(UMI3DUser user)
        {
            this.user = user;

            string userName = UserManager.Instance.GetUserName(user.Id());

            this.userNameText.Text.SetValue(userName);
            this.userNameLabel.text = userName;

            this.nbOfKillText.Text.SetValue("0");
            this.nbOfDeathText.Text.SetValue("0");
            this.scoreText.Text.SetValue("0");
        }
    }
}

