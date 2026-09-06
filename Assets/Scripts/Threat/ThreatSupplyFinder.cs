using System;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class ThreatSupplyFinder : NetworkBehaviour
    {
        [SerializeField] private float radius = 5f;
        [SerializeField] private LayerMask supplyLayer;
        [SerializeField] private bool showGizmos = true;

        private const int MAX_RESULTS = 16;
        private readonly Collider2D[] results = new Collider2D[MAX_RESULTS];

        private NetworkVariable<NetworkObjectReference> targetReference = new();

        private ContactFilter2D filter;

        private void Awake()
        {
            filter = new ContactFilter2D()
            {
                useLayerMask = true,
                layerMask = supplyLayer,
                useTriggers = true
            };
        }

        public bool HasTarget
        {
            get
            {
                return targetReference.Value.TryGet(out NetworkObject networkObject) &&
                       networkObject != null &&
                       networkObject.IsSpawned;
            }
        }

        public Supply Target
        {
            get
            {
                if (!targetReference.Value.TryGet(out NetworkObject networkObject))
                {
                    return null;
                }

                return networkObject.GetComponent<Supply>();
            }
        }

        public bool TryFindSupply()
        {
            if (!IsServer)
                return false;

            int size = Physics2D.OverlapCircle(transform.position, radius, filter, results);

            for (int i = 0; i < size; i++)
            {
                Supply supply = results[i].GetComponent<Supply>();

                if (supply == null)
                    continue;

                if (!supply.NetworkObject.IsSpawned)
                    continue;

                targetReference.Value = new NetworkObjectReference(supply.NetworkObject);

                return true;
            }

            Clear();

            return false;
        }

        public bool TryGetTarget(out Supply supply)
        {
            supply = Target;

            if (supply == null)
            {
                Clear();
                return false;
            }

            return true;
        }

        public void Clear()
        {
            if (!IsServer)
                return;

            targetReference.Value = default;
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos)
                return;

            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}