using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownCharacterController : MonoBehaviour
    {
        public float speed;

        public Vector2 CurrentMoveInput { get; private set; }
        public Vector2 CurrentFacing { get; private set; } = Vector2.down;

        private Animator animator;
        private Rigidbody2D body;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            var input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            CurrentMoveInput = Vector2.ClampMagnitude(input, 1f);

            if (CurrentMoveInput.sqrMagnitude > 0.0001f)
            {
                CurrentFacing = CurrentMoveInput.normalized;
            }

            if (animator != null)
            {
                animator.SetBool("IsMoving", CurrentMoveInput.sqrMagnitude > 0.0001f);
                animator.SetInteger("Direction", GetDirectionIndex(CurrentFacing));
            }
        }

        private void FixedUpdate()
        {
            body.linearVelocity = CurrentMoveInput * speed;
        }

        private static int GetDirectionIndex(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return direction.x < 0f ? 3 : 2;
            }

            return direction.y > 0f ? 1 : 0;
        }
    }
}
