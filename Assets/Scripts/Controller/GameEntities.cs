using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace Game
{
    public class GameEntities : NetworkBehaviour
    {
        [Header("Warehouse")]
        [SerializeField] private Warehouse warehouse;
        
        [Header("Threats")]
        [SerializeField] private Threat prefab;
        [SerializeField] private int maxThreats;
        [SerializeField] private float spawnInterval = 60f;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float spawnCheckRadius = 0.5f;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private int maxSpawnAttempts = 20;

        [Header("Pool")]
        [SerializeField] private int poolDefaultCapacity = 10;
        [SerializeField] private int poolMaxSize = 50;
        [SerializeField] private List<Threat> activeThreats;
        
        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        
        private ObjectPool<Threat> pool;
        
        private float spawnTimer;
        public static GameEntities Singleton { get; private set; }
        
        private void Awake()
        {
            Singleton = this;
            
            pool = new ObjectPool<Threat>(
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
            if (IsServer)
            {
                Reset();
            }
            
            base.OnNetworkDespawn();
        }
        
        private void Update()
        {
            if (GamePhase.Singleton.Phase.Value != GamePhaseType.Running)
                return;
            
            if (!IsServer)
                return;

            if (!IsSpawned)
                return;

            if (activeThreats.Count >= maxThreats)
                return;

            spawnTimer += Time.deltaTime;

            if (spawnTimer < spawnInterval)
                return;

            spawnTimer -= spawnInterval;

            TrySpawn();
        }
        
        public bool TrySpawn()
        {
            if (!IsServer)
                return false;

            if (!TryGetSpawnPosition(out Vector2 spawnPosition))
                return false;

            Threat threat = pool.Get();
            threat.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

            threat.NetworkObject.Spawn();
            activeThreats.Add(threat);

            return true;
        }
        
        private Threat CreateSupply()
        {
            Threat supply = Instantiate(prefab, transform);
            supply.gameObject.SetActive(false);

            supply.Release += HandleSupplyReleased;

            return supply;
        }

        private void OnGetSupply(Threat supply)
        {
            supply.gameObject.SetActive(true);
        }

        private void OnReleaseSupply(Threat supply)
        {
            supply.gameObject.SetActive(false);

            supply.transform.SetParent(transform, false);
            supply.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void OnDestroySupply(Threat supply)
        {
            supply.Release -= HandleSupplyReleased;

            Destroy(supply.gameObject);
        }

        private void HandleSupplyReleased(Threat supply)
        {
            if (!IsServer)
                return;

            activeThreats.Remove(supply);

            if (supply.IsSpawned)
            {
                supply.NetworkObject.Despawn(false);
            }

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
        
        public void Reset()
        {
            if (!IsServer)
                return;

            spawnTimer = spawnInterval;

            for (int i = activeThreats.Count - 1; i >= 0; i--)
            {
                Threat threat = activeThreats[i];

                if (threat == null)
                    continue;

                if (threat.IsSpawned)
                {
                    threat.NetworkObject.Despawn(false);
                }

                pool.Release(threat);
            }

            activeThreats.Clear();
        }

        private Vector2 GetRandomPosition()
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return (Vector2)transform.position + randomOffset;
        }

        private bool CanSpawnAt(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, spawnCheckRadius, collisionMask);
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