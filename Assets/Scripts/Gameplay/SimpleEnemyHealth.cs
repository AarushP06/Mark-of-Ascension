using UnityEngine;
using System;

namespace MarkOfAscension.Gameplay
{
    public class SimpleEnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private bool isBoss;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsBoss => isBoss;
        public bool IsDead { get; private set; }
        public event Action<SimpleEnemyHealth> Died;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void Configure(int healthAmount, bool boss)
        {
            maxHealth = Mathf.Max(1, healthAmount);
            isBoss = boss;
            CurrentHealth = maxHealth;
            IsDead = false;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || damage <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            Debug.Log($"[SimpleEnemyHealth] {gameObject.name} took {damage} damage. Health: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
