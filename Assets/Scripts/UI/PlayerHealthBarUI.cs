using MarkOfAscension.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkOfAscension.UI
{
    public class PlayerHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private string playerTag = "Player";

        private PlayerHealth playerHealth;

        private void Awake()
        {
            if (healthSlider == null)
            {
                healthSlider = GetComponentInChildren<Slider>(true);
            }

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.wholeNumbers = true;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryBindToPlayerHealth();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnbindFromPlayerHealth();
        }

        private void Update()
        {
            if (playerHealth == null)
            {
                TryBindToPlayerHealth();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryBindToPlayerHealth();
        }

        private void TryBindToPlayerHealth()
        {
            if (healthSlider == null)
            {
                return;
            }

            var playerObject = GameObject.FindGameObjectWithTag(playerTag);
            var nextPlayerHealth = playerObject != null ? playerObject.GetComponent<PlayerHealth>() : null;

            if (nextPlayerHealth == playerHealth)
            {
                RefreshSlider();
                return;
            }

            UnbindFromPlayerHealth();
            playerHealth = nextPlayerHealth;

            if (playerHealth == null)
            {
                healthSlider.gameObject.SetActive(false);
                return;
            }

            playerHealth.HealthChanged += OnHealthChanged;
            playerHealth.Respawned += OnPlayerRespawned;
            RefreshSlider();
        }

        private void UnbindFromPlayerHealth()
        {
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.HealthChanged -= OnHealthChanged;
            playerHealth.Respawned -= OnPlayerRespawned;
            playerHealth = null;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (healthSlider == null)
            {
                return;
            }

            healthSlider.gameObject.SetActive(true);
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        private void OnPlayerRespawned()
        {
            RefreshSlider();
        }

        private void RefreshSlider()
        {
            if (healthSlider == null)
            {
                return;
            }

            if (playerHealth == null)
            {
                healthSlider.gameObject.SetActive(false);
                return;
            }

            healthSlider.gameObject.SetActive(true);
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }
}
