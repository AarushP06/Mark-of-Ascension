using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class ExitPortalPlaceholder : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool isUnlocked;
        [SerializeField] private bool hideWhileLocked = true;

        private Collider2D portalCollider;
        private SpriteRenderer[] portalRenderers;
        private ScenePortal scenePortal;
        private bool wasUnlocked;

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            scenePortal = GetComponent<ScenePortal>();
            wasUnlocked = isUnlocked;
            ApplyVisualState();
        }

        public void SetUnlocked(bool unlocked)
        {
            if (!wasUnlocked && unlocked)
            {
                GameAudio.PlayPortalUnlock();
            }

            isUnlocked = unlocked;
            wasUnlocked = unlocked;
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (portalCollider != null)
            {
                portalCollider.enabled = isUnlocked;
                portalCollider.isTrigger = true;
            }

            if (scenePortal != null)
            {
                scenePortal.SetPortalEnabled(isUnlocked);
            }

            if (portalRenderers == null)
            {
                return;
            }

            foreach (var portalRenderer in portalRenderers)
            {
                if (portalRenderer == null)
                {
                    continue;
                }

                portalRenderer.enabled = isUnlocked || !hideWhileLocked;
                portalRenderer.color = isUnlocked
                    ? new Color(0.65f, 0.2f, 1f, 0.9f)
                    : new Color(0.35f, 0.2f, 0.45f, 0.45f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isUnlocked || !other.CompareTag(playerTag))
            {
                return;
            }

            Debug.Log("[ExitPortalPlaceholder] Portal entered. Hook your next scene load here later."
            );
        }
    }
}
