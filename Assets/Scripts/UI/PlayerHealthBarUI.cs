using MarkOfAscension.Gameplay;
using UnityEngine;
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
                healthSlider = GetComponentInChildren<Slider>();
            }
        }

        private void Update()
        {
            if (healthSlider == null)
            {
                return;
            }

            if (playerHealth == null)
            {
                FindPlayerHealth();
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

        private void FindPlayerHealth()
        {
            var playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject == null)
            {
                return;
            }

            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }
    }
}
