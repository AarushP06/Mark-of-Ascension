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

        private TopDownCharacterController controller;
        private PlayerVisualAnimatorBridge visualAnimatorBridge;
        private float nextAttackTime;
        private Vector2 lastAttackDirection = Vector2.down;

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
            nextAttackTime = Time.time + attackCooldown;

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

                targetObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
                Debug.Log($"[PlayerAttack] Hit {targetObject.name} for {attackDamage} damage.");
            }
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
