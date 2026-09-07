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
        [SerializeField] private float spawnInterval = 30f;
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

            activeThreats ??= new List<Threat>();

            pool = new ObjectPool<Threat>(
                CreateThreat,
                OnGetThreat,
                OnReleaseThreat,
                OnDestroyThreat,
                collectionCheck: true,
                defaultCapacity: poolDefaultCapacity,
                maxSize: poolMaxSize
            );
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                spawnTimer = spawnInterval;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                ResetServer();
            }

            ResetLocal();

            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            if (Singleton == this)
            {
                Singleton = null;
            }
            
            pool?.Clear();

            base.OnDestroy();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (!IsSpawned)
                return;

            if (GamePhase.Singleton == null)
                return;

            if (GamePhase.Singleton.Phase.Value != GamePhaseType.Running)
                return;

            spawnTimer += Time.deltaTime;

            if (spawnTimer < spawnInterval)
                return;

            spawnTimer -= spawnInterval;

            TrySpawn();
        }

        // ============================================================
        // SPAWN
        // ============================================================

        public bool TrySpawn()
        {
            if (!IsServer)
                return false;

            if (!TryGetSpawnPosition(out Vector2 spawnPosition))
                return false;

            Threat threat = pool.Get();

            if (threat == null)
                return false;

            threat.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            threat.NetworkObject.Spawn();

            activeThreats.Add(threat);

            return true;
        }
        
        private Threat CreateThreat()
        {
            Threat threat = Instantiate(prefab, transform);

            threat.gameObject.SetActive(false);
            threat.Release += HandleThreatReleased;

            return threat;
        }

        private void OnGetThreat(Threat threat)
        {
            if (threat == null)
                return;

            threat.gameObject.SetActive(true);
        }

        private void OnReleaseThreat(Threat threat)
        {
            if (threat == null)
                return;

            threat.gameObject.SetActive(false);
            threat.transform.SetParent(transform, false);

            threat.transform.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity
            );
        }

        private void OnDestroyThreat(Threat threat)
        {
            if (threat == null)
                return;

            threat.Release -= HandleThreatReleased;

            Destroy(threat.gameObject);
        }

        private void HandleThreatReleased(Threat threat)
        {
            if (!IsServer)
                return;

            if (threat == null)
                return;

            activeThreats.Remove(threat);
            pool.Release(threat);
        }
        
        private void ResetLocal()
        {
            activeThreats.Clear();
            spawnTimer = 0f;
        }

        private void ResetServer()
        {
            if (!IsServer)
                return;

            spawnTimer = spawnInterval;
            warehouse.Reset();
            
            for (int i = activeThreats.Count - 1; i >= 0; i--)
            {
                Threat threat = activeThreats[i];
                
                if (threat.NetworkObject.IsSpawned)
                {
                    threat.NetworkObject.Despawn(false);
                }
            }

            activeThreats.Clear();
        }

        // ============================================================
        // SPAWN POSITION
        // ============================================================

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
            Vector2 randomOffset =
                Random.insideUnitCircle * spawnRadius;

            return (Vector2)transform.position + randomOffset;
        }

        private bool CanSpawnAt(Vector2 position)
        {
            Collider2D collider = Physics2D.OverlapCircle(
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