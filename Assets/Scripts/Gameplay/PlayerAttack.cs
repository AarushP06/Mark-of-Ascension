using Cainos.PixelArtTopDown_Basic;
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
        private float nextAttackTime;

        private void Awake()
        {
            controller = GetComponent<TopDownCharacterController>();

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

            var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            foreach (var hit in hits)
            {
                hit.gameObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
                Debug.Log($"[PlayerAttack] Hit {hit.name} for {attackDamage} damage.");
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
                facing = Vector2.down;
            }

            attackPoint.localPosition = facing.normalized * attackPointDistance;
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
