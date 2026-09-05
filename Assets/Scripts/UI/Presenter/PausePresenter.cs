using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class PausePresenter : UIPresenter<PauseView>
    {
        public static event Action Disconnected;
        
        private void Start()
        {
            view.Resume += HandleResume;
            view.Quit += HandleQuit;
            
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
            if (GamePhase.Singleton != null)
            {
                GamePhase.Singleton.PhaseRequest.OnValueChanged -= HandlePhaseChanged;
            }

            if (GameSession.Singleton != null)
            {
                GameSession.Singleton.Players.OnListChanged -= HandlePlayersChanged;
            }
            
            view.Resume -= HandleResume;
            view.Quit -= HandleQuit;
        }
        
        private void HandlePlayersChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerSession>.EventType.RemoveAt:
                    HandlePlayerDisconnected(changeEvent.Value);
                    break;
            }
        }

        private void HandlePlayerDisconnected(PlayerSession playerSession)
        {
            if (playerSession.clientId == NetworkManager.Singleton.LocalClientId)
                return;
            
            Debug.Log("[PausePresenter-HandlePlayerDisconnected] Game is not paused.");
            view.Hide();
        }

        private void HandlePhaseChanged(PauseRequest pauseRequest, PauseRequest request)
        {
            if (request.IsSame(NetworkManager.Singleton.LocalClientId) && request.isPause)
            {
                view.Show();
                return;
            }
            
            Debug.Log("[PausePresenter-HandlePhaseChanged] Game is not paused.");
            view.Hide();
        }
        
        private void HandleQuit()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
                view.Hide();
                Disconnected?.Invoke();
            }
        }

        private void HandleResume()
        {
            GamePhase.Singleton.RequestResume(PauseRequest.Resume(NetworkManager.Singleton.LocalClientId));
        }
    }
}