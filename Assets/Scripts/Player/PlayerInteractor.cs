using System;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float radius = 1.5f;
        [SerializeField, UnityEngine.Range(0f, 180f)] private float angle = 90f;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private int maxResults = 16;

        [Header("Selection")]
        [SerializeField] private float directionWeight = 10f;
        [SerializeField] private float distanceWeight = 1f;

        private ContactFilter2D contactFilter;
        private Collider2D[] results;

        private IInteractable currentTarget;

        private Player owner;
        
        public event Action<IInteractable> TargetChanged;

        public void Initialize(Player player)
        {
            owner = player;

            results = new Collider2D[maxResults];

            contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = interactionLayer,
                useTriggers = true
            };
        }

        private void Update()
        {
            if (owner == null)
                return;

            if (!owner.IsOwner || !owner.IsSpawned)
                return;

            UpdateTarget();
        }

        private void UpdateTarget()
        {
            IInteractable target = FindTarget();

            if (currentTarget == target)
                return;

            // Remove highlight from previous target.
            if (currentTarget != null)
                currentTarget.SetHighlight(false);

            currentTarget = target;

            // Add highlight to new target.
            if (currentTarget != null)
                currentTarget.SetHighlight(true);

            TargetChanged?.Invoke(currentTarget);
        }

        private IInteractable FindTarget()
        {
            if (owner == null)
                return null;

            int count = Physics2D.OverlapCircle(transform.position, radius, contactFilter, results);

            IInteractable bestTarget = null;
            float bestScore = float.MinValue;

            Vector2 position = transform.position;
            Vector2 forward = owner.Movement.LastDirection;

            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector2.up;

            float minDot = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            for (int i = 0; i < count; i++)
            {
                Collider2D collider = results[i];

                if (collider == null)
                    continue;

                IInteractable interactable = collider.GetComponent<IInteractable>();

                if (interactable == null)
                    continue;

                if (!interactable.CanInteract(owner))
                    continue;

                Vector2 offset = (Vector2)collider.transform.position - position;

                float sqrDistance = offset.sqrMagnitude;
                if (sqrDistance <= 0.0001f)
                    continue;

                Vector2 direction = offset.normalized;

                float dot = Vector2.Dot(forward, direction);
                if (dot < minDot)
                    continue;

                float score = dot * directionWeight - sqrDistance * distanceWeight;

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestTarget = interactable;
            }

            return bestTarget;
        }

        public bool TryGetTarget(out IInteractable target)
        {
            target = currentTarget;
            return target != null;
        }
        
        public bool TryGetTarget<T>(out T target) where T : class
        {
            if (currentTarget is T interactable)
            {
                target = interactable;
                return true;
            }
            
            target = null;
            return false;
        }
        
        private void OnDestroy()
        {
            if (currentTarget != null)
                currentTarget.SetHighlight(false);
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;
            Vector3 forward = owner?.Movement.LastDirection ?? Vector2.up;

            DrawDetectionGizmos(position, forward);
            DrawTargetGizmos(position);
            DrawCandidateGizmos(position, forward);
        }

        private void DrawDetectionGizmos(Vector3 position, Vector3 forward)
        {
            Gizmos.color = Color.white;

            Gizmos.DrawWireSphere(position, radius);
            Gizmos.DrawLine(position, position + forward * radius);

            float halfAngle = angle * 0.5f;

            Vector3 left = Quaternion.Euler(0f, 0f, halfAngle) * forward;
            Vector3 right = Quaternion.Euler(0f, 0f, -halfAngle) * forward;

            Gizmos.DrawLine(position, position + left * radius);
            Gizmos.DrawLine(position, position + right * radius);

            DrawArc(position, radius, forward, halfAngle);
        }

        private void DrawCandidateGizmos(Vector3 position, Vector3 forward)
        {
            if (results == null)
                return;

            int count = Physics2D.OverlapCircle(position, radius, contactFilter, results);
            float minDot = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            for (int i = 0; i < count; i++)
            {
                Collider2D collider = results[i];

                if (collider == null)
                    continue;

                IInteractable interactable = collider.GetComponent<IInteractable>();

                if (interactable == null)
                    continue;

                Vector3 targetPosition = collider.transform.position;
                Vector2 offset = targetPosition - position;
                float sqrDistance = offset.sqrMagnitude;

                if (sqrDistance <= 0.0001f)
                    continue;

                Vector2 direction = offset.normalized;
                float dot = Vector2.Dot(forward, direction);

                bool insideCone = dot >= minDot;
                bool canInteract = owner != null && interactable.CanInteract(owner);

                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(targetPosition, 0.1f);

                if (!insideCone)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(position, targetPosition);

                    continue;
                }

                if (!canInteract)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(position, targetPosition);
                    continue;
                }

                float score = dot * directionWeight - sqrDistance * distanceWeight;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(targetPosition, 0.12f);
                Gizmos.DrawLine(position, targetPosition);

#if UNITY_EDITOR
                Handles.Label(targetPosition + Vector3.up * 0.15f, $"Score: {score:F2}\nDot: {dot:F2}");
#endif
            }
        }

        private void DrawTargetGizmos(Vector3 position)
        {
            if (currentTarget == null)
                return;

            Component component = currentTarget as Component;

            if (component == null)
                return;

            Vector3 targetPosition = component.transform.position;

            Gizmos.color = Color.green;
            
            Gizmos.DrawLine(position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.18f);
            Gizmos.DrawSphere(targetPosition, 0.06f);

#if UNITY_EDITOR
            Handles.Label(targetPosition + Vector3.up * 0.35f, "CURRENT TARGET");
#endif
        }

        private static void DrawArc(Vector3 center, float radius, Vector3 forward, float halfAngle)
        {
            const int segments = 24;
            Vector3 previous = center + Quaternion.Euler(0f, 0f, halfAngle) * forward * radius;

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float currentAngle = Mathf.Lerp(halfAngle, -halfAngle, t);
                Vector3 current = center + Quaternion.Euler(0f, 0f, currentAngle) * forward * radius;
                
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

#endif
    }
}