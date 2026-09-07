using UnityEngine;

namespace Game
{
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float radius = 1.5f;
        [SerializeField] private float angle = 90f;
        [SerializeField] private ContactFilter2D contactFilter;

        private const int MAX_RESULTS = 16;

        private readonly Collider2D[] results =
            new Collider2D[MAX_RESULTS];

        private readonly Threat[] targets =
            new Threat[MAX_RESULTS];


      

        public void Attack(Vector3 facingDirection)
        {
            int count = FindTargets(facingDirection, targets, targets.Length);
            for (int i = 0; i < count; i++)
            {
                Threat threat = targets[i];

                if (threat == null)
                    continue;
                
                if(!threat.NetworkObject.IsSpawned)
                    continue;
                
                threat.TakeDamage();
                targets[i] = null;
            }
        }

        private int FindTargets(Vector3 facingDirection, Threat[] targets, int maxTargets)
        {
            int count = Physics2D.OverlapCircle(
                transform.position,
                radius,
                contactFilter,
                results
            );

            Vector2 position = transform.position;
            Vector2 forward = facingDirection;

            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector2.up;

            float minDot =
                Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            int targetCount = 0;

            for (int i = 0; i < count; i++)
            {
                Collider2D collider = results[i];

                if (collider == null)
                    continue;

                if (!collider.TryGetComponent(
                        out Threat threat))
                {
                    continue;
                }

                Vector2 offset =
                    (Vector2)collider.transform.position - position;

                if (offset.sqrMagnitude <= 0.0001f)
                    continue;

                Vector2 direction = offset.normalized;

                float dot = Vector2.Dot(
                    forward,
                    direction
                );

                if (dot < minDot)
                    continue;

                targets[targetCount] = threat;
                targetCount++;

                if (targetCount >= maxTargets)
                    break;
            }

            return targetCount;
        }
    }
}