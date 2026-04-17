using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private string defaultSpawnName = "PlayerSpawn";
        [SerializeField] private string fallbackSpawnName = "PlayerSpawnPoint";

        private Rigidbody2D body;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            Debug.Log($"[PlayerHealth] {gameObject.name} took {damage} damage. Health: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            Debug.Log($"[PlayerHealth] {gameObject.name} healed {amount}. Health: {CurrentHealth}/{maxHealth}");
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        private void Die()
        {
            IsDead = true;
            Debug.Log($"[PlayerHealth] {gameObject.name} died. Respawning at the current scene spawn.");
            RespawnAtSceneSpawn();
        }

        private void RespawnAtSceneSpawn()
        {
            var spawn = FindSpawnPoint();

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            if (spawn != null)
            {
                transform.position = spawn.position;
            }

            RestoreFullHealth();
        }

        private Transform FindSpawnPoint()
        {
            if (!string.IsNullOrWhiteSpace(SceneTransitionState.NextSpawnName))
            {
                var transitionSpawn = GameObject.Find(SceneTransitionState.NextSpawnName);
                if (transitionSpawn != null)
                {
                    return transitionSpawn.transform;
                }
            }

            var defaultSpawn = GameObject.Find(defaultSpawnName);
            if (defaultSpawn != null)
            {
                return defaultSpawn.transform;
            }

            var fallbackSpawn = GameObject.Find(fallbackSpawnName);
            if (fallbackSpawn != null)
            {
                return fallbackSpawn.transform;
            }

            return null;
        }
    }
}
