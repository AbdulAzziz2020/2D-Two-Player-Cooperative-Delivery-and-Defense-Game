using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Game
{
    public class SupplySpawner : NetworkBehaviour
    {
        [Header("Spawn")]
        [SerializeField] private Supply prefab;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float spawnCheckRadius = 0.5f;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private int maxSpawnAttempts = 20;

        [Header("Pool")]
        [SerializeField] private int poolDefaultCapacity = 10;
        [SerializeField] private int poolMaxSize = 50;
        [SerializeField] private List<Supply> activeSupplies = new();

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private ObjectPool<Supply> pool;

        private float spawnTimer;

        public static SupplySpawner Singleton { get; private set; }

        private void Awake()
        {
            Singleton = this;

            pool = new ObjectPool<Supply>(
                CreateSupply,
                OnGetSupply,
                OnReleaseSupply,
                OnDestroySupply,
                collectionCheck: true,
                defaultCapacity: poolDefaultCapacity,
                maxSize: poolMaxSize
            );
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            spawnTimer = 0f;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                Reset();
            }

            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            pool?.Clear();

            if (Singleton == this)
                Singleton = null;

            base.OnDestroy();
        }

        public void Reset()
        {
            if (!IsServer)
                return;

            spawnTimer = 0f;

            for (int i = activeSupplies.Count - 1; i >= 0; i--)
            {
                Supply supply = activeSupplies[i];

                if (supply.NetworkObject.IsSpawned)
                {
                    supply.NetworkObject.Despawn(false);
                }
            }

            activeSupplies.Clear();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (!IsSpawned)
                return;

            if (GamePhase.Singleton.Phase.Value != GamePhaseType.Running)
                return;

            spawnTimer += Time.deltaTime;

            if (spawnTimer < spawnInterval)
                return;

            spawnTimer -= spawnInterval;

            TrySpawn();
        }

        [ContextMenu("Spawn")]
        public bool TrySpawn()
        {
            if (!IsServer)
                return false;

            if (!TryGetSpawnPosition(out Vector2 spawnPosition))
                return false;

            Supply supply = pool.Get();

            SupplySO supplySo =
                SupplyCollection.Singleton.GetRandomSupply();

            SupplyData supplyData = supplySo.Create();

            supply.PrepareSpawn(supplyData);

            supply.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.identity
            );

            supply.NetworkObject.Spawn();

            activeSupplies.Add(supply);

            return true;
        }

        public void SpecificSpawn(
            Vector3 spawnPosition,
            SupplyData data)
        {
            if (!IsServer)
                return;

            Supply supply = pool.Get();

            supply.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.identity
            );

            supply.PrepareSpawn(data);

            supply.NetworkObject.Spawn();

            activeSupplies.Add(supply);
        }

        private Supply CreateSupply()
        {
            Supply supply = Instantiate(prefab);

            supply.gameObject.SetActive(false);
            supply.Release += HandleSupplyReleased;

            return supply;
        }

        private void OnGetSupply(Supply supply)
        {
            supply.gameObject.SetActive(true);
        }

        private void OnReleaseSupply(Supply supply)
        {
            supply.gameObject.SetActive(false);

            supply.transform.SetPositionAndRotation(
                Vector3.zero,
                Quaternion.identity
            );
        }

        private void OnDestroySupply(Supply supply)
        {
            supply.Release -= HandleSupplyReleased;
            if (supply.gameObject != null)
            {
                Destroy(supply.gameObject);
            }
        }

        private void HandleSupplyReleased(Supply supply)
        {
            if (!IsServer)
                return;

            activeSupplies.Remove(supply);
            pool.Release(supply);
        }

        private bool TryGetSpawnPosition(out Vector2 position)
        {
            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                position = GetRandomPosition();

                if (!CanSpawnAt(position))
                    continue;

                return true;
            }

            position = default;
            return false;
        }

        private Vector2 GetRandomPosition()
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return (Vector2)transform.position + randomOffset;
        }

        private bool CanSpawnAt(Vector2 position)
        {
            Collider2D collider =
                Physics2D.OverlapCircle(
                    position,
                    spawnCheckRadius,
                    collisionMask
                );

            return collider == null;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.DrawWireSphere(transform.position, spawnCheckRadius);
        }
    }
}