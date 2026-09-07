using System;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class ThreatTargetFinder : NetworkBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private LayerMask supplyLayer;
        [SerializeField] private bool showGizmos = true;

        [Header("Target Chance")]
        [SerializeField, Range(0f, 1f)]
        private float supplyTargetChance = 0.7f;

        private const int MAX_RESULTS = 16;

        private readonly Collider2D[] results = new Collider2D[MAX_RESULTS];

        private NetworkVariable<NetworkObjectReference> targetReference = new();

        private ContactFilter2D filter;

        private void Awake()
        {
            filter = new ContactFilter2D
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
                return targetReference.Value.TryGet(
                           out NetworkObject networkObject
                       ) &&
                       networkObject != null &&
                       networkObject.IsSpawned;
            }
        }

        public NetworkObject TargetObject
        {
            get
            {
                if (!targetReference.Value.TryGet(
                        out NetworkObject networkObject))
                {
                    return null;
                }

                if (networkObject == null ||
                    !networkObject.IsSpawned)
                {
                    return null;
                }

                return networkObject;
            }
        }

        public bool TryFindTarget()
        {
            if (!IsServer)
                return false;

            Clear();

            bool targetSupply = UnityEngine.Random.value < supplyTargetChance;

            if (targetSupply)
                return TryFindSupply();

            return TryFindWarehouse();
        }

        private bool TryFindSupply()
        {
            int size = Physics2D.OverlapCircle(
                transform.position,
                radius,
                filter,
                results
            );

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

            return false;
        }

        private bool TryFindWarehouse()
        {
            Warehouse warehouse = FindFirstObjectByType<Warehouse>();

            if (warehouse == null)
                return false;

            if (!warehouse.NetworkObject.IsSpawned)
                return false;

            if (warehouse.IsDestroyed)
                return false;

            targetReference.Value =
                new NetworkObjectReference(
                    warehouse.NetworkObject
                );

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

            Gizmos.DrawWireSphere(
                transform.position,
                radius
            );
        }
    }
}