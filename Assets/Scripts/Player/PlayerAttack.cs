using UnityEngine;

namespace Game
{
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float radius = 1.5f;
        [SerializeField] private float angle = 90f;
        [SerializeField] private ContactFilter2D contactFilter;

        [Header("Gizmos")]
        [SerializeField] private bool showGizmos = true;

        private const int MAX_RESULTS = 16;

        private readonly Collider2D[] results =
            new Collider2D[MAX_RESULTS];

        private readonly Threat[] targets =
            new Threat[MAX_RESULTS];

        private Player player;
        
        public void Initialize(Player player)
        {
            this.player = player;
        }
        
        public void Attack()
        {
            int count = FindTargets(targets, targets.Length);

            for (int i = 0; i < count; i++)
            {
                Threat threat = targets[i];

                if (threat == null)
                    continue;

                if (!threat.NetworkObject.IsSpawned)
                    continue;

                threat.TakeDamage();
                targets[i] = null;
            }
        }

        private int FindTargets(Threat[] targets, int maxTargets)
        {
            int count = Physics2D.OverlapCircle(
                transform.position,
                radius,
                contactFilter,
                results
            );

            Vector2 position = transform.position;
            Vector2 forward = player.Movement.FacingDirection.Value;

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

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;

            Vector2 forward = player.Movement.FacingDirection.Value;

            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector2.up;

            forward.Normalize();

            Vector2 position = transform.position;

            float halfAngle = angle * 0.5f;

            Vector2 leftDirection = Rotate(
                forward,
                -halfAngle
            );

            Vector2 rightDirection = Rotate(
                forward,
                halfAngle
            );

            // Detection radius
            Gizmos.DrawWireSphere(
                position,
                radius
            );

            // Center direction
            Gizmos.DrawLine(
                position,
                position + forward * radius
            );

            // Cone boundaries
            Gizmos.DrawLine(
                position,
                position + leftDirection * radius
            );

            Gizmos.DrawLine(
                position,
                position + rightDirection * radius
            );

            // Arc
            DrawArc(
                position,
                radius,
                forward,
                angle
            );
        }

        private static Vector2 Rotate(
            Vector2 direction,
            float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;

            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos
            );
        }

        private static void DrawArc(
            Vector2 center,
            float radius,
            Vector2 forward,
            float totalAngle)
        {
            const int segments = 24;

            float halfAngle = totalAngle * 0.5f;
            float step = totalAngle / segments;

            Vector2 previous =
                center + Rotate(forward, -halfAngle) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float currentAngle =
                    -halfAngle + step * i;

                Vector2 current =
                    center + Rotate(forward, currentAngle) * radius;

                Gizmos.DrawLine(previous, current);

                previous = current;
            }
        }
    }
}