using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;

namespace com.quentintran.score
{
    public class ScoreScreen : MonoBehaviour
    {
        [SerializeField]
        private ScoreEntry scoreTemplate;

        [SerializeField]
        private UIRect scoreContainer;

        private Dictionary<UMI3DUser, ScoreEntry> scores = new();

        private void Awake()
        {
            Debug.Assert(scoreTemplate != null);   
            Debug.Assert(scoreContainer != null);   
        }

        private void OnEnable()
        {
            UMI3DCollaborationServer.Instance.OnUserActive.AddListener(OnUserActive);
            UMI3DCollaborationServer.Instance.OnUserLeave.AddListener(OnUserLeave);

            ScoreManager.Instance.OnUserKill += OnUserKill;
            ScoreManager.Instance.OnUserDie += OnUserDie;
        }

        private void OnDisable()
        {
            UMI3DCollaborationServer.Instance.OnUserActive.RemoveListener(OnUserActive);
            UMI3DCollaborationServer.Instance.OnUserLeave.RemoveListener(OnUserLeave);

            ScoreManager.Instance.OnUserKill -= OnUserKill;
            ScoreManager.Instance.OnUserDie -= OnUserDie;
        }

        private IEnumerable<Operation> GetUpdateLayout()
        {
            List<Operation> operations = new List<Operation>();
            IEnumerable<ScoreEntry> scoreEntries = this.scores.Values.OrderByDescending(e => e.NbKill);

            int i = 0;

            foreach (ScoreEntry scoreEntry in scoreEntries)
            {
                operations.Add(scoreEntry.node.objectPosition.SetValue(new Vector3( - ScoreEntry.WIDTH / 2f, - i * ScoreEntry.HEIGHT, 0)));
                i++;
            }

            return operations;
        }

        private void OnUserActive(UMI3DUser user)
        {
            ScoreEntry entry = GameObject.Instantiate(scoreTemplate);
            scores[user] = entry;
            entry.Init(user);
            entry.gameObject.transform.SetParent(scoreContainer.transform, false);

            Transaction t = new() { reliable = true };

            foreach (UMI3DLoadableEntity loadable in entry.GetComponentsInChildren<UMI3DLoadableEntity>())
            {
                t.AddIfNotNull(loadable.GetLoadEntity());
            }

            t.AddIfNotNull(GetUpdateLayout());

            t.Dispatch();
        }

        private void OnUserLeave(UMI3DUser user)
        {

        }

        private void OnUserKill(UMI3DUser user)
        {
            if (scores.TryGetValue(user, out ScoreEntry entry))
            {
                entry.NbKill++;

                Transaction t = new() { reliable = true };
                t.AddIfNotNull(GetUpdateLayout());
                t.Dispatch();
            }
            else
            {
                Debug.LogError("ScoreScreen.OnUserKill : user not found");
            }
        }

        private void OnUserDie(UMI3DUser user)
        {
            if (scores.TryGetValue(user, out ScoreEntry entry))
            {
                entry.NbDeath++;

                Transaction t = new() { reliable = true };
                t.AddIfNotNull(GetUpdateLayout());
                t.Dispatch();
            }
            else
            {
                Debug.LogError("ScoreScreen.OnUserKill : user not found");
            }
        }
    }
}
