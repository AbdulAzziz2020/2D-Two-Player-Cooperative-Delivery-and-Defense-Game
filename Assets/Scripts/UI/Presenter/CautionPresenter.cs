using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class CautionPresenter : UIPresenter<CautionView>
    {
        public static event Action OnDisconnected;
        
        private void Start()
        {
            view.Disconnect += HandleClientDisconnect;
            
            GamePhase.Singleton.PhaseRequest.OnValueChanged += HandlePhaseChanged;
            GameSession.Singleton.Players.OnListChanged += HandlePlayersChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
        
        private void HandleClientDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.ServerClientId)
            {
                Debug.LogWarning("Host disconnected!"); 
                return;
            }

            Debug.Log($"Client {clientId} disconnected.");
        }

        private void OnDestroy()
        {
            view.Disconnect -= HandleClientDisconnect;
            
            if (GamePhase.Singleton != null)
            {
                GamePhase.Singleton.PhaseRequest.OnValueChanged -= HandlePhaseChanged;
            }
            
            if (GameSession.Singleton != null)
            {
                GameSession.Singleton.Players.OnListChanged -= HandlePlayersChanged;
            }
        }
        
        private void HandlePlayersChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            Debug.Log("Event: " + changeEvent.Type);
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerSession>.EventType.RemoveAt:
                    HandlePlayerDisconnected(changeEvent.Value);
                    break;
            }
        }

        private void HandlePlayerDisconnected(PlayerSession playerSession)
        {
            Debug.Log("[MessagePresenter-HandlePlayerDisconnected] Game is not paused.");
            view.Hide();
        }

        private void HandlePhaseChanged(PauseRequest oldRequest, PauseRequest newRequest)
        {
            if (!newRequest.isPause)
            {
                Debug.Log("[MessagePresenter-HandlePhaseChanged] Game is not paused.");
                view.Hide();
                return;
            }

            if (newRequest.IsSame(NetworkManager.Singleton.LocalClientId))
            {
                view.Hide();
                return;
            }

            view.SetMessage(!newRequest.IsHost() ? "Paused by other player." : "Paused by host.");

            view.Show();
        }

        private void HandleClientDisconnect()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
                view.Hide();
                OnDisconnected?.Invoke();
            }
        }
    }
}