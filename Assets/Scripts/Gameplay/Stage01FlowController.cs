using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    public class Stage01FlowController : MonoBehaviour
    {
        [SerializeField] private GameObject bossGate;
        [SerializeField] private ExitPortalPlaceholder exitPortal;
        [SerializeField] private bool bossDefeated;

        private void Start()
        {
            ApplyState();
        }

        [ContextMenu("Simulate Boss Defeat")]
        public void SimulateBossDefeat()
        {
            bossDefeated = true;
            ApplyState();
            Debug.Log("[Stage01FlowController] Boss defeated placeholder triggered."
            );
        }

        public void ApplyState()
        {
            if (bossGate != null)
            {
                bossGate.SetActive(!bossDefeated);
            }

            if (exitPortal != null)
            {
                exitPortal.SetUnlocked(bossDefeated);
            }
        }
    }
}
