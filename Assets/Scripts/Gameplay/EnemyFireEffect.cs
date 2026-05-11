using System.Collections;
using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    public class EnemyFireEffect : MonoBehaviour
    {
        [SerializeField] private int fireDamagePerTick = 1;
        [SerializeField] private float fireTickInterval = 0.45f;
        [SerializeField] private int fireTickCount = 4;

        private SimpleEnemyHealth enemyHealth;
        private Coroutine fireRoutine;

        private void Awake()
        {
            enemyHealth = GetComponent<SimpleEnemyHealth>();
        }

        public void ApplyFire(int damagePerTick, float tickInterval, int tickCount)
        {
            fireDamagePerTick = Mathf.Max(1, damagePerTick);
            fireTickInterval = Mathf.Max(0.1f, tickInterval);
            fireTickCount = Mathf.Max(1, tickCount);

            if (fireRoutine != null)
            {
                StopCoroutine(fireRoutine);
            }

            fireRoutine = StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine()
        {
            for (var i = 0; i < fireTickCount; i++)
            {
                yield return new WaitForSeconds(fireTickInterval);

                if (enemyHealth == null || enemyHealth.IsDead)
                {
                    fireRoutine = null;
                    yield break;
                }

                enemyHealth.TakeDamage(fireDamagePerTick);
            }

            fireRoutine = null;
        }
    }
}
