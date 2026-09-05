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
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float spawnCheckRadius = 0.5f;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private int maxSpawnAttempts = 20;

        [Header("Pool")]
        [SerializeField] private int poolDefaultCapacity = 10;
        [SerializeField] private int poolMaxSize = 50;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private ObjectPool<Supply> pool;

        private float spawnTimer;

        private void Awake()
        {
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

            spawnTimer = spawnInterval;
        }

        public override void OnNetworkDespawn()
        {
            if (pool != null)
            {
                pool.Clear();
            }

            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (!IsSpawned)
                return;

            spawnTimer += Time.deltaTime;

            if (spawnTimer < spawnInterval)
                return;

            spawnTimer -= spawnInterval;

            TrySpawn();
        }

        [Button]
        public bool TrySpawn()
        {
            if (!IsServer)
                return false;

            if (prefab == null)
            {
                Debug.LogWarning(
                    "SupplySpawner has no prefab.",
                    this
                );

                return false;
            }

            if (!TryGetSpawnPosition(out Vector2 spawnPosition))
            {
                Debug.LogWarning(
                    $"Failed to find valid spawn position after " +
                    $"{maxSpawnAttempts} attempts.",
                    this
                );

                return false;
            }

            Supply supply = pool.Get();

            supply.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.identity
            );

            supply.NetworkObject.Spawn();

            return true;
        }

        private Supply CreateSupply()
        {
            Supply supply = Instantiate(
                prefab,
                transform
            );

            supply.gameObject.SetActive(false);

            supply.OnReleased += HandleSupplyReleased;

            return supply;
        }

        private void OnGetSupply(Supply supply)
        {
            supply.gameObject.SetActive(true);
        }

        private void OnReleaseSupply(Supply supply)
        {
            supply.gameObject.SetActive(false);

            supply.transform.SetParent(
                transform,
                false
            );

            supply.transform.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity
            );
        }

        private void OnDestroySupply(Supply supply)
        {
            supply.OnReleased -= HandleSupplyReleased;

            Destroy(supply.gameObject);
        }

        private void HandleSupplyReleased(Supply supply)
        {
            if (!IsServer)
                return;

            if (supply.IsSpawned)
            {
                supply.NetworkObject.Despawn(false);
            }

            pool.Release(supply);
        }

        private bool TryGetSpawnPosition(
            out Vector2 position)
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
            Vector2 randomOffset =
                Random.insideUnitCircle * spawnRadius;

            return (Vector2)transform.position +
                   randomOffset;
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

            Gizmos.DrawWireSphere(
                transform.position,
                spawnRadius
            );

            Gizmos.DrawWireSphere(
                transform.position,
                spawnCheckRadius
            );
        }
    }
}