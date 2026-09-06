using System;
using System.Collections.Generic;
using System.Text;
using Game.UI;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameSession : NetworkBehaviour
    {
        public static GameSession Singleton { get; private set; }

        [Header("Player")]
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Transform[] spawnPoints;

        [Header("Settings")]
        [SerializeField] private int maxPlayers = 2;

        public NetworkList<PlayerSession> Players { get; private set; } = new();

        [ShowInInspector] private int playerCount => Players.Count;
        public bool IsFull => Players.Count >= maxPlayers;
        
        [ShowInInspector] private readonly Dictionary<ulong, PlayerInfo> pendingPlayers = new();

        private void Awake()
        {
            Singleton = this;

            if (spawnPoints.Length < maxPlayers)
            {
                Debug.LogError(
                    $"Spawn points ({spawnPoints.Length}) " +
                    $"must be >= max players ({maxPlayers}).");
            }
        }

        private void Start()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        public override void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            RegisterNetworkCallbacks();
        }
        
        public override void OnNetworkDespawn()
        {
            UnregisterNetworkCallbacks();

            if (IsServer)
            {
                Debug.Log("Game Session Rilis");
                pendingPlayers.Clear();
                Players.Clear();
            }

            base.OnNetworkDespawn();
        }
        
        private void RegisterNetworkCallbacks()
        {
            if (NetworkManager.Singleton == null)
                return;
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        private void UnregisterNetworkCallbacks()
        {
            if (NetworkManager.Singleton == null)
                return;
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                response.Approved = false;
                response.CreatePlayerObject = false;
                response.Pending = false;
                return;
            }

            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= maxPlayers)
            {
                response.Approved = false;
                response.CreatePlayerObject = false;
                response.Pending = false;
                response.Reason = MessageResponse.GAME_FULL;
                return;
            }

            if (!TryParsePlayerInfo(request.Payload, out PlayerInfo playerInfo))
            {
                response.Approved = false;
                response.CreatePlayerObject = false;
                response.Pending = false;
                response.Reason = MessageResponse.CONNECTION_INVALID;
                return;
            }

            ulong clientId = request.ClientNetworkId;

            pendingPlayers[clientId] = playerInfo;

            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Pending = false;
            response.Reason = string.Empty;
        }

        private bool TryParsePlayerInfo(byte[] payload, out PlayerInfo playerInfo)
        {
            playerInfo = default;

            if (payload == null || payload.Length == 0)
                return false;

            try
            {
                string json = Encoding.UTF8.GetString(payload);

                if (string.IsNullOrEmpty(json))
                    return false;

                playerInfo = JsonUtility.FromJson<PlayerInfo>(json);

                return !string.IsNullOrEmpty(playerInfo.playerId);
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // CLIENT CONNECTED
        // ============================================================

        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            AddPlayer(clientId);
            SpawnPlayer(clientId);
        }

        private void AddPlayer(ulong clientId)
        {
            if (!pendingPlayers.TryGetValue(clientId, out PlayerInfo playerInfo))
            {
                return;
            }

            var playerSession = new PlayerSession(clientId, playerInfo.playerId, playerInfo.playerName);

            Players.Add(playerSession);
            pendingPlayers.Remove(clientId);
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            if (IsServer)
            {
                pendingPlayers.Remove(clientId);
                RemovePlayer(clientId);
            }
        }
        
        private void RemovePlayer(ulong clientId)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].clientId != clientId)
                    continue;

                Players.RemoveAt(i);
                return;
            }
        }

        // ============================================================
        // SPAWN
        // ============================================================

        private void SpawnPlayer(ulong clientId)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (playerPrefab == null)
                return;

            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            {
                return;
            }

            if (client.PlayerObject != null)
                return;

            Vector3 position = spawnPoints[Players.Count - 1].position;
            NetworkObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.SpawnAsPlayerObject(clientId, true);
        }
    }
}