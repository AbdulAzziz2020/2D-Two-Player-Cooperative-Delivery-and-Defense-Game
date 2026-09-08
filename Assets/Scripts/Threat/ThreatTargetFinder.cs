using System;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public sealed class ThreatTargetFinder : NetworkBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float supplyDetectionRange = 5f;
        [SerializeField] private float warehouseDetectionRange = 10f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private bool showGizmos = true;

        [Header("Selection")]
        [SerializeField, Range(0f, 1f)]
        private float supplyTargetChance = 0.7f;

        private const int MAX_RESULTS = 16;

        private readonly Collider2D[] results =
            new Collider2D[MAX_RESULTS];

        private NetworkVariable<NetworkObjectReference> targetReference = new();

        private ContactFilter2D filter;

        public bool HasTarget
        {
            get
            {
                return TargetObject != null;
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

        private void Awake()
        {
            filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = targetLayer,
                useTriggers = true
            };
        }

        public bool TryFindTarget()
        {
            if (!IsServer)
                return false;

            Clear();

            bool preferSupply = UnityEngine.Random.value < supplyTargetChance;

            if (preferSupply)
            {
                if (TryFindSupply())
                    return true;

                return TryFindWarehouse();
            }

            if (TryFindWarehouse())
                return true;

            return TryFindSupply();
        }

        private bool TryFindSupply()
        {
            int size = Physics2D.OverlapCircle(
                transform.position,
                supplyDetectionRange,
                filter,
                results
            );

            for (int i = 0; i < size; i++)
            {
                Collider2D collider = results[i];

                if (collider == null)
                    continue;

                Supply supply = collider.GetComponent<Supply>();

                if (supply == null)
                    continue;

                NetworkObject networkObject =
                    supply.NetworkObject;

                if (networkObject == null ||
                    !networkObject.IsSpawned)
                {
                    continue;
                }

                targetReference.Value =
                    new NetworkObjectReference(networkObject);

                return true;
            }

            return false;
        }

        private bool TryFindWarehouse()
        {
            int size = Physics2D.OverlapCircle(
                transform.position,
                warehouseDetectionRange,
                filter,
                results
            );

            for (int i = 0; i < size; i++)
            {
                Collider2D collider = results[i];

                if (collider == null)
                    continue;

                Warehouse warehouse = collider.GetComponent<Warehouse>();

                if (warehouse == null)
                    continue;

                NetworkObject networkObject =
                    warehouse.NetworkObject;

                if (networkObject == null ||
                    !networkObject.IsSpawned)
                {
                    continue;
                }

                if (warehouse.IsDestroyed)
                    continue;

                targetReference.Value =
                    new NetworkObjectReference(networkObject);

                return true;
            }

            return false;
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
                supplyDetectionRange
            );

            Gizmos.DrawWireSphere(
                transform.position,
                warehouseDetectionRange
            );
        }
    }
}