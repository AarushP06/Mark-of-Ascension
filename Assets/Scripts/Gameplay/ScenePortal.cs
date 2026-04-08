using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class ScenePortal : MonoBehaviour
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private string targetSpawnName;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool portalEnabled = true;

        private bool isLoading;

        private void Reset()
        {
            var trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        public void SetPortalEnabled(bool enabledState)
        {
            portalEnabled = enabledState;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isLoading || !portalEnabled || !other.CompareTag(playerTag) || string.IsNullOrWhiteSpace(targetSceneName))
            {
                return;
            }

            isLoading = true;
            SceneTransitionState.NextSpawnName = targetSpawnName;
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
