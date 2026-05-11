using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private string defaultSpawnName = "PlayerSpawn";
        [SerializeField] private string fallbackSpawnName = "PlayerSpawnPoint";
        [SerializeField] private bool respawnOnDeath = true;
        [SerializeField] private float respawnDelay = 0.75f;
        [SerializeField] private bool hideSpriteWhileDead = true;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button backToMainMenuButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameOverPanelName = "GameOverPanel";
        [SerializeField] private string backToMainMenuButtonName = "BackToMainMenuButton";

        private Rigidbody2D body;
        private SpriteRenderer[] spriteRenderers;
        private Collider2D[] colliders;
        private Coroutine respawnRoutine;
        private int maxHealthBonus;

        public int MaxHealth => maxHealth + maxHealthBonus;
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
            CurrentHealth = MaxHealth;
            RefreshSceneUiReferences();
            SceneManager.sceneLoaded += OnSceneLoaded;
            NotifyHealthChanged();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (backToMainMenuButton != null)
            {
                backToMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            NotifyHealthChanged();
            Debug.Log($"[PlayerHealth] {gameObject.name} took {damage} damage. Health: {CurrentHealth}/{MaxHealth}");

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

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            NotifyHealthChanged();
            Debug.Log($"[PlayerHealth] {gameObject.name} healed {amount}. Health: {CurrentHealth}/{MaxHealth}");
        }

        public void RestoreFullHealth()
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
            NotifyHealthChanged();
        }

        public void AddMaxHealthBonus(int amount, bool restoreToFullHealth = false)
        {
            if (amount <= 0)
            {
                return;
            }

            maxHealthBonus += amount;
            CurrentHealth = restoreToFullHealth
                ? MaxHealth
                : Mathf.Min(CurrentHealth + amount, MaxHealth);
            NotifyHealthChanged();
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            var shouldRespawn = ShouldRespawnOnDeath();
            Debug.Log(shouldRespawn
                ? $"[PlayerHealth] {gameObject.name} died. Respawning at the current scene spawn."
                : $"[PlayerHealth] {gameObject.name} died. Game over."
            );
            Died?.Invoke();

            SetDeadState(true);

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            if (!shouldRespawn)
            {
                ShowGameOver();
                return;
            }

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
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
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneUiReferences();
        }

        private void ShowGameOver()
        {
            EnsureEventSystemExists();

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            Time.timeScale = 0f;
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            PersistentPlayer.DestroyPersistentInstance();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private bool ShouldRespawnOnDeath()
        {
            return respawnOnDeath && gameOverPanel == null;
        }

        private void RefreshSceneUiReferences()
        {
            if (backToMainMenuButton != null)
            {
                backToMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }

            var gameOverRoot = GameObject.Find("GameOverUI");
            if (gameOverRoot != null)
            {
                if (!string.IsNullOrWhiteSpace(gameOverPanelName))
                {
                    var panelTransform = gameOverRoot.transform.Find(gameOverPanelName);
                    gameOverPanel = panelTransform != null ? panelTransform.gameObject : null;
                }

                if (!string.IsNullOrWhiteSpace(backToMainMenuButtonName))
                {
                    var buttonTransform = gameOverRoot.transform.Find($"{gameOverPanelName}/{backToMainMenuButtonName}");
                    backToMainMenuButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
                }
            }
            else
            {
                gameOverPanel = null;
                backToMainMenuButton = null;
            }

            if (backToMainMenuButton != null)
            {
                backToMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
                backToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private static void EnsureEventSystemExists()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
