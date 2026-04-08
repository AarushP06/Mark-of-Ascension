using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class ExitPortalPlaceholder : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool isUnlocked;

        private Collider2D portalCollider;
        private SpriteRenderer portalRenderer;

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalRenderer = GetComponent<SpriteRenderer>();
            ApplyVisualState();
        }

        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (portalCollider != null)
            {
                portalCollider.enabled = isUnlocked;
                portalCollider.isTrigger = true;
            }

            if (portalRenderer != null)
            {
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
