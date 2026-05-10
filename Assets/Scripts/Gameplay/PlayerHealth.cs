using System;
using System.Collections;
using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private string defaultSpawnName = "PlayerSpawn";
        [SerializeField] private string fallbackSpawnName = "PlayerSpawnPoint";
        [SerializeField] private float respawnDelay = 0.75f;
        [SerializeField] private bool hideSpriteWhileDead = true;

        private Rigidbody2D body;
        private SpriteRenderer[] spriteRenderers;
        private Collider2D[] colliders;
        private Coroutine respawnRoutine;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        public event Action<int, int> HealthChanged;
        public event Action Died;
        public event Action Respawned;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            colliders = GetComponents<Collider2D>();
            CurrentHealth = maxHealth;
            NotifyHealthChanged();
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            NotifyHealthChanged();
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
            NotifyHealthChanged();
            Debug.Log($"[PlayerHealth] {gameObject.name} healed {amount}. Health: {CurrentHealth}/{maxHealth}");
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = maxHealth;
            IsDead = false;
            NotifyHealthChanged();
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            Debug.Log($"[PlayerHealth] {gameObject.name} died. Respawning at the current scene spawn.");
            Died?.Invoke();

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            SetDeadState(true);

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            if (respawnDelay > 0f)
            {
                yield return new WaitForSeconds(respawnDelay);
            }

            RespawnAtSceneSpawn();
            respawnRoutine = null;
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
            SetDeadState(false);

            if (body != null)
            {
                body.simulated = true;
            }

            Respawned?.Invoke();
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

        private void SetDeadState(bool isDead)
        {
            foreach (var collider2D in colliders)
            {
                if (collider2D != null)
                {
                    collider2D.enabled = !isDead;
                }
            }

            if (!hideSpriteWhileDead)
            {
                return;
            }

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !isDead;
                }
            }
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
