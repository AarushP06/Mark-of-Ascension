using System.Collections;
using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    public class EnemyPoisonEffect : MonoBehaviour
    {
        [SerializeField] private int poisonDamagePerTick = 1;
        [SerializeField] private float poisonTickInterval = 0.6f;
        [SerializeField] private int poisonTickCount = 3;

        private SimpleEnemyHealth enemyHealth;
        private Coroutine poisonRoutine;

        private void Awake()
        {
            enemyHealth = GetComponent<SimpleEnemyHealth>();
        }

        public void ApplyPoison(int damagePerTick, float tickInterval, int tickCount)
        {
            poisonDamagePerTick = Mathf.Max(1, damagePerTick);
            poisonTickInterval = Mathf.Max(0.1f, tickInterval);
            poisonTickCount = Mathf.Max(1, tickCount);

            if (poisonRoutine != null)
            {
                StopCoroutine(poisonRoutine);
            }

            poisonRoutine = StartCoroutine(PoisonRoutine());
        }

        private IEnumerator PoisonRoutine()
        {
            for (var i = 0; i < poisonTickCount; i++)
            {
                yield return new WaitForSeconds(poisonTickInterval);

                if (enemyHealth == null || enemyHealth.IsDead)
                {
                    poisonRoutine = null;
                    yield break;
                }

                enemyHealth.TakeDamage(poisonDamagePerTick);
            }

            poisonRoutine = null;
        }
    }
}
