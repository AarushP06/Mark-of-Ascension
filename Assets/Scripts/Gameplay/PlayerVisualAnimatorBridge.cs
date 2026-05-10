using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    /// <summary>
    /// Drives a child visual Animator from the existing player Rigidbody2D so
    /// imported character packs can be used without replacing the working
    /// movement, persistence, or collision setup on the player root.
    /// </summary>
    public class PlayerVisualAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator targetAnimator;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Rigidbody2D movementSource;
        [SerializeField] private TopDownCharacterController controllerSource;
        [SerializeField] private string idleStateName = "idle";
        [SerializeField] private string walkStateName = "walk";
        [SerializeField] private string attackStateName = "attack";
        [SerializeField] private float attackStateDuration = 0.2f;
        [SerializeField] private string moveXParameter = "movementX";
        [SerializeField] private string moveYParameter = "movementY";
        [SerializeField] private float movementThreshold = 0.01f;
        [SerializeField] private float horizontalFlipThreshold = 0.1f;

        private Vector2 lastFacing = Vector2.down;
        private bool hasMoveXParameter;
        private bool hasMoveYParameter;
        private int idleStateHash;
        private int walkStateHash;
        private int attackStateHash;
        private float actionStateEndTime;

        private void Awake()
        {
            if (targetAnimator == null)
            {
                targetAnimator = GetComponent<Animator>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            if (movementSource == null)
            {
                movementSource = GetComponentInParent<Rigidbody2D>();
            }

            if (controllerSource == null)
            {
                controllerSource = GetComponentInParent<TopDownCharacterController>();
            }

            idleStateHash = Animator.StringToHash(idleStateName);
            walkStateHash = Animator.StringToHash(walkStateName);
            attackStateHash = Animator.StringToHash(attackStateName);
            CacheAnimatorParameters();
        }

        private void LateUpdate()
        {
            if (targetAnimator == null || movementSource == null)
            {
                return;
            }

            var animationVector = movementSource.linearVelocity;
            var isMoving = animationVector.sqrMagnitude > movementThreshold * movementThreshold;

            if (controllerSource != null)
            {
                animationVector = controllerSource.CurrentMoveInput;
                isMoving = controllerSource.CurrentMoveInput.sqrMagnitude > movementThreshold * movementThreshold;
            }

            if (isMoving)
            {
                lastFacing = animationVector.normalized;
            }
            else if (controllerSource != null && controllerSource.CurrentFacing.sqrMagnitude > 0.0001f)
            {
                lastFacing = controllerSource.CurrentFacing.normalized;
            }

            if (hasMoveXParameter)
            {
                targetAnimator.SetFloat(moveXParameter, lastFacing.x);
            }

            if (hasMoveYParameter)
            {
                targetAnimator.SetFloat(moveYParameter, lastFacing.y);
            }

            if (targetRenderer != null)
            {
                if (lastFacing.x < -horizontalFlipThreshold)
                {
                    targetRenderer.flipX = true;
                }
                else if (lastFacing.x > horizontalFlipThreshold)
                {
                    targetRenderer.flipX = false;
                }
            }

            if (Time.time < actionStateEndTime)
            {
                return;
            }

            var desiredState = isMoving ? walkStateHash : idleStateHash;
            var stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.shortNameHash.Equals(desiredState))
            {
                targetAnimator.Play(desiredState, 0, 0f);
            }
        }

        public void PlayAttackAnimation()
        {
            if (targetAnimator == null || string.IsNullOrWhiteSpace(attackStateName))
            {
                return;
            }

            actionStateEndTime = Time.time + attackStateDuration;
            targetAnimator.Play(attackStateHash, 0, 0f);
        }

        private void CacheAnimatorParameters()
        {
            if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
            {
                return;
            }

            foreach (var parameter in targetAnimator.parameters)
            {
                if (parameter.name == moveXParameter)
                {
                    hasMoveXParameter = true;
                }
                else if (parameter.name == moveYParameter)
                {
                    hasMoveYParameter = true;
                }
            }
        }
    }
}
