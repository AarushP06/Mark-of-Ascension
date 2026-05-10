using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    public class SimpleEnemyContactDamage : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float damageCooldown = 1f;

        private float nextDamageTime;
        private SimpleEnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<SimpleEnemyHealth>();
        }

        public void Configure(int damageAmount, float cooldownSeconds)
        {
            contactDamage = Mathf.Max(1, damageAmount);
            damageCooldown = Mathf.Max(0.1f, cooldownSeconds);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            AttemptDamage(collision.collider);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            AttemptDamage(other);
        }

        private void AttemptDamage(Collider2D other)
        {
            if (enemyHealth != null && enemyHealth.IsDead)
            {
                return;
            }

            if (Time.time < nextDamageTime || other == null || !other.CompareTag(playerTag))
            {
                return;
            }

            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                return;
            }

            nextDamageTime = Time.time + damageCooldown;
            playerHealth.TakeDamage(contactDamage);
        }
    }
}
