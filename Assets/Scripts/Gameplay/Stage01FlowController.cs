using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarkOfAscension.Gameplay
{
    public class Stage01FlowController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GameObject bossGate;
        [SerializeField] private ExitPortalPlaceholder exitPortal;
        [SerializeField] private Transform bossSpawn;
        [SerializeField] private GameObject bossPrefab;

        [Header("Stage Enemies")]
        [SerializeField] private bool bossDefeated;
        [SerializeField] private int smallEnemyHealth = 3;
        [SerializeField] private float smallEnemySpeed = 1.75f;
        [SerializeField] private int smallEnemyDamage = 1;

        [Header("Boss")]
        [SerializeField] private string bossName = "Stage01_SlimeBoss";
        [SerializeField] private int bossHealth = 12;
        [SerializeField] private float bossSpeed = 1.15f;
        [SerializeField] private int bossDamage = 2;
        [SerializeField] private float bossScaleMultiplier = 2.35f;
        [SerializeField] private Vector3 unlockedPortalPosition = new Vector3(0f, 19f, 0f);
        [SerializeField] private string[] bossPathBlockerNames =
        {
            "BossGatePlaceholder",
            "Boss_BottomLeft",
            "Boss_BottomRight",
            "Corridor_Left",
            "Corridor_Right",
            "Entry_TopLeft",
            "Entry_TopRight"
        };

        private readonly List<SimpleEnemyHealth> trackedSmallEnemies = new();
        private SimpleEnemyHealth currentBoss;
        private bool bossSpawned;

        private void Start()
        {
            SetupInitialEnemies();
            ApplyState();
        }

        [ContextMenu("Simulate Boss Defeat")]
        public void SimulateBossDefeat()
        {
            bossDefeated = true;
            ApplyState();
            Debug.Log("[Stage01FlowController] Boss defeat simulated.");
        }

        public void ApplyState()
        {
            var shouldLockBossArea = !bossSpawned && trackedSmallEnemies.Count > 0;
            SetBossPathBlocked(shouldLockBossArea);

            if (exitPortal != null)
            {
                exitPortal.SetUnlocked(bossDefeated);
            }
        }

        private void SetupInitialEnemies()
        {
            trackedSmallEnemies.Clear();

            var enemies = FindObjectsByType<SimpleEnemyHealth>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                ConfigureEnemy(enemy, false, smallEnemyHealth, smallEnemySpeed, smallEnemyDamage, 5.5f, 0.8f, 1f);
                enemy.Died -= OnSmallEnemyDied;
                enemy.Died += OnSmallEnemyDied;
                trackedSmallEnemies.Add(enemy);
            }
        }

        private void OnSmallEnemyDied(SimpleEnemyHealth defeatedEnemy)
        {
            defeatedEnemy.Died -= OnSmallEnemyDied;
            trackedSmallEnemies.Remove(defeatedEnemy);

            if (trackedSmallEnemies.Count == 0 && !bossSpawned && !bossDefeated)
            {
                SetBossPathBlocked(false);
                SpawnBoss(defeatedEnemy != null ? defeatedEnemy.gameObject : null);
            }

            ApplyState();
        }

        private void SpawnBoss(GameObject fallbackSource)
        {
            bossSpawned = true;

            if (bossSpawn == null)
            {
                Debug.LogWarning("[Stage01FlowController] BossSpawn is missing.");
                ApplyState();
                return;
            }

            var bossObject = CreateBossObject(fallbackSource);
            if (bossObject == null)
            {
                Debug.LogWarning("[Stage01FlowController] No valid boss source was found.");
                ApplyState();
                return;
            }
            bossObject.name = bossName;
            bossObject.transform.localScale *= bossScaleMultiplier;

            currentBoss = bossObject.GetComponent<SimpleEnemyHealth>();
            if (currentBoss == null)
            {
                currentBoss = bossObject.AddComponent<SimpleEnemyHealth>();
            }

            ConfigureEnemy(currentBoss, true, bossHealth, bossSpeed, bossDamage, 7f, 1.2f, 0.8f);
            currentBoss.Died -= OnBossDied;
            currentBoss.Died += OnBossDied;

            Debug.Log("[Stage01FlowController] Small enemies defeated. Slime boss spawned.");
            ApplyState();
        }

        private GameObject CreateBossObject(GameObject fallbackSource)
        {
            if (bossPrefab != null)
            {
                try
                {
                    var spawnedObject = Instantiate((Object)bossPrefab, bossSpawn.position, Quaternion.identity) as GameObject;
                    if (spawnedObject != null)
                    {
                        return spawnedObject;
                    }
                }
                catch
                {
                    Debug.LogWarning("[Stage01FlowController] Boss prefab reference was invalid. Falling back to a large slime copy.");
                }
            }

            if (fallbackSource != null)
            {
                return Instantiate(fallbackSource, bossSpawn.position, Quaternion.identity);
            }

            return null;
        }

        private void OnBossDied(SimpleEnemyHealth defeatedBoss)
        {
            defeatedBoss.Died -= OnBossDied;
            bossDefeated = true;
            GrantStageReward();
            PositionExitPortal();
            Debug.Log("[Stage01FlowController] Boss defeated. Exit portal unlocked.");
            ApplyState();
        }

        private void GrantStageReward()
        {
            var player = PersistentPlayer.Instance != null
                ? PersistentPlayer.Instance.gameObject
                : GameObject.FindGameObjectWithTag("Player");
            var progression = player != null ? player.GetComponent<PlayerProgression>() : null;
            progression?.GrantRewardForStage(SceneManager.GetActiveScene().name);
        }

        private void ConfigureEnemy(
            SimpleEnemyHealth enemy,
            bool isBoss,
            int health,
            float moveSpeed,
            int damage,
            float detectionRange,
            float stopDistance,
            float damageCooldown)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.Configure(health, isBoss);
            SetEnemyLayer(enemy.gameObject);

            var body = enemy.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = enemy.gameObject.AddComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.linearDamping = 8f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = enemy.GetComponent<Collider2D>();
            if (collider == null)
            {
                var boxCollider = enemy.gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = isBoss ? new Vector2(1.2f, 0.9f) : new Vector2(0.7f, 0.45f);
                boxCollider.offset = isBoss ? new Vector2(0f, -0.1f) : new Vector2(0f, -0.05f);
            }

            var ai = enemy.GetComponent<SimpleEnemyAI>();
            if (ai == null)
            {
                ai = enemy.gameObject.AddComponent<SimpleEnemyAI>();
            }

            ai.Configure(moveSpeed, detectionRange, stopDistance);

            var contactDamage = enemy.GetComponent<SimpleEnemyContactDamage>();
            if (contactDamage == null)
            {
                contactDamage = enemy.gameObject.AddComponent<SimpleEnemyContactDamage>();
            }

            contactDamage.Configure(damage, damageCooldown);
        }

        private static void SetEnemyLayer(GameObject target)
        {
            var enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer < 0 || target == null)
            {
                return;
            }

            foreach (var childTransform in target.GetComponentsInChildren<Transform>(true))
            {
                childTransform.gameObject.layer = enemyLayer;
            }
        }

        private void SetBossPathBlocked(bool blocked)
        {
            if (bossGate != null)
            {
                bossGate.SetActive(blocked);
            }

            foreach (var blockerName in bossPathBlockerNames)
            {
                if (string.IsNullOrWhiteSpace(blockerName))
                {
                    continue;
                }

                var blockerObject = GameObject.Find(blockerName);
                if (blockerObject != null)
                {
                    blockerObject.SetActive(blocked);
                }
            }
        }

        private void PositionExitPortal()
        {
            if (exitPortal == null)
            {
                return;
            }

            exitPortal.transform.position = unlockedPortalPosition;
        }
    }
}
