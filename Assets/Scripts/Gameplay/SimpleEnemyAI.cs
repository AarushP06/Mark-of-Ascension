using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleEnemyAI : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float stopDistance = 0.85f;

        private Rigidbody2D body;
        private Transform playerTarget;
        private SimpleEnemyHealth enemyHealth;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            enemyHealth = GetComponent<SimpleEnemyHealth>();
        }

        public void Configure(float speed, float range, float minimumDistance)
        {
            moveSpeed = speed;
            detectionRange = range;
            stopDistance = minimumDistance;
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                var playerObject = GameObject.FindGameObjectWithTag(playerTag);
                if (playerObject != null)
                {
                    playerTarget = playerObject.transform;
                }
            }
        }

        private void FixedUpdate()
        {
            if (body == null || playerTarget == null || (enemyHealth != null && enemyHealth.IsDead))
            {
                StopMoving();
                return;
            }

            var toPlayer = (Vector2)(playerTarget.position - transform.position);
            var distance = toPlayer.magnitude;

            if (distance > detectionRange || distance <= stopDistance)
            {
                StopMoving();
                return;
            }

            body.linearVelocity = toPlayer.normalized * moveSpeed;
        }

        private void StopMoving()
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }
    }
}
