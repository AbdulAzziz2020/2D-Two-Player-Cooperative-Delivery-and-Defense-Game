using UnityEngine;

namespace Game
{
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private static readonly int HashX = Animator.StringToHash("X");
        private static readonly int HashY = Animator.StringToHash("Y");
        private static readonly int HashMove = Animator.StringToHash("Move");
        private static readonly int HashAttack = Animator.StringToHash("Attack");
        private static readonly int HashIsCarrying = Animator.StringToHash("IsPickup");

        private Vector2 lastDirection = Vector2.down;

        private void Reset()
        {
            animator = GetComponent<Animator>();
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0f)
            {
                SetMove(false);
                return;
            }

            direction = Get4Direction(direction);

            lastDirection = direction;

            animator.SetFloat(HashX, direction.x);
            animator.SetFloat(HashY, direction.y);
            SetMove(true);
        }

        public void SetMove(bool isMoving)
        {
            animator.SetFloat(HashMove, isMoving ? 1f : 0f);
        }

        public void SetCarrying(bool value)
        {
            animator.SetBool(HashIsCarrying, value);
        }

        public void Attack()
        {
            animator.SetTrigger(HashAttack);
        }

        private static Vector2 Get4Direction(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return new Vector2(Mathf.Sign(direction.x), 0f);

            return new Vector2(0f, Mathf.Sign(direction.y));
        }
    }
}