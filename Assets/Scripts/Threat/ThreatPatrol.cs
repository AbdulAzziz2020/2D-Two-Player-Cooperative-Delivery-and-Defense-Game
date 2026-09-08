using System;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class ThreatPatrol : NetworkBehaviour
    {
        [SerializeField] private float radius = 3f;
        [SerializeField] private bool showGizmos = true;
        
        private Vector2 target;
        private bool isComplete;

        public bool IsComplete => isComplete;
        public Vector2 Target => target;

        public void StartPatrol()
        {
            if (!IsServer)
                return;

            target = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * radius;

            isComplete = false;
        }

        public void Complete()
        {
            if (!IsServer)
                return;

            isComplete = true;
        }

        private void OnDrawGizmos()
        {
            if(!showGizmos)
                return;
            
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(transform.position, target);
        }
    }
}