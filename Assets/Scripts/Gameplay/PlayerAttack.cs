using Cainos.PixelArtTopDown_Basic;
using System.Collections.Generic;
using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private KeyCode attackKey = KeyCode.B;
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRange = 0.7f;
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float attackCooldown = 0.35f;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private float attackPointDistance = 0.55f;
        [SerializeField] private bool poisonUnlocked;
        [SerializeField] private int poisonDamagePerTick = 1;
        [SerializeField] private float poisonTickInterval = 0.6f;
        [SerializeField] private int poisonTickCount = 3;
        [SerializeField] private bool fireUnlocked;
        [SerializeField] private int fireDamagePerTick = 1;
        [SerializeField] private float fireTickInterval = 0.45f;
        [SerializeField] private int fireTickCount = 4;

        private TopDownCharacterController controller;
        private PlayerVisualAnimatorBridge visualAnimatorBridge;
        private float nextAttackTime;
        private Vector2 lastAttackDirection = Vector2.down;
        private int attackDamageBonus;
        private float attackCooldownReduction;

        public Vector2 LastAttackDirection => lastAttackDirection;

        private void Awake()
        {
            controller = GetComponent<TopDownCharacterController>();
            visualAnimatorBridge = GetComponentInChildren<PlayerVisualAnimatorBridge>();

            if (attackPoint == null)
            {
                var existingPoint = transform.Find("AttackPoint");
                if (existingPoint != null)
                {
                    attackPoint = existingPoint;
                }
            }
        }

        private void Update()
        {
            UpdateAttackPointPosition();

            if (Time.time < nextAttackTime)
            {
                return;
            }

            if (Input.GetKeyDown(attackKey))
            {
                Attack();
            }
        }

        private void Attack()
        {
            nextAttackTime = Time.time + CurrentAttackCooldown;
            GameAudio.PlayPlayerAttack();

            if (attackPoint == null)
            {
                Debug.LogWarning("[PlayerAttack] AttackPoint is missing on the player.");
                return;
            }

            visualAnimatorBridge?.PlayAttackAnimation();

            var hits = enemyLayers.value == 0
                ? Physics2D.OverlapCircleAll(attackPoint.position, attackRange)
                : Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
            var damagedObjects = new HashSet<GameObject>();

            foreach (var hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                var targetObject = hit.attachedRigidbody != null
                    ? hit.attachedRigidbody.gameObject
                    : hit.transform.root.gameObject;

                if (!damagedObjects.Add(targetObject))
                {
                    continue;
                }

                targetObject.SendMessage("TakeDamage", CurrentAttackDamage, SendMessageOptions.DontRequireReceiver);
                ApplyPoison(targetObject);
                ApplyFire(targetObject);
                Debug.Log($"[PlayerAttack] Hit {targetObject.name} for {CurrentAttackDamage} damage.");
            }

        }

        public void UnlockPoisonDamage()
        {
            poisonUnlocked = true;
        }

        public void AddAttackDamageBonus(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            attackDamageBonus += amount;
        }

        public void UnlockFireDamage()
        {
            fireUnlocked = true;
        }

        public void ReduceAttackCooldown(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            attackCooldownReduction += amount;
        }

        private int CurrentAttackDamage => attackDamage + attackDamageBonus;

        private float CurrentAttackCooldown => Mathf.Max(0.1f, attackCooldown - attackCooldownReduction);

        private void ApplyPoison(GameObject targetObject)
        {
            if (!poisonUnlocked || targetObject == null)
            {
                return;
            }

            var enemyHealth = targetObject.GetComponent<SimpleEnemyHealth>();
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                return;
            }

            var poisonEffect = targetObject.GetComponent<EnemyPoisonEffect>();
            if (poisonEffect == null)
            {
                poisonEffect = targetObject.AddComponent<EnemyPoisonEffect>();
            }

            poisonEffect.ApplyPoison(poisonDamagePerTick, poisonTickInterval, poisonTickCount);
        }

        private void ApplyFire(GameObject targetObject)
        {
            if (!fireUnlocked || targetObject == null)
            {
                return;
            }

            var enemyHealth = targetObject.GetComponent<SimpleEnemyHealth>();
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                return;
            }

            var fireEffect = targetObject.GetComponent<EnemyFireEffect>();
            if (fireEffect == null)
            {
                fireEffect = targetObject.AddComponent<EnemyFireEffect>();
            }

            fireEffect.ApplyFire(fireDamagePerTick, fireTickInterval, fireTickCount);
        }

        private void UpdateAttackPointPosition()
        {
            if (attackPoint == null)
            {
                return;
            }

            var facing = controller != null ? controller.CurrentFacing : Vector2.down;
            if (facing.sqrMagnitude < 0.0001f)
            {
                facing = lastAttackDirection;
            }

            lastAttackDirection = facing.normalized;
            attackPoint.localPosition = lastAttackDirection * attackPointDistance;
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
