using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class MessagePresenter : UIPresenter<MessageView>
    {
        private void Start()
        {
            view.Disconnect += HandleClientDisconnect;
            
            GamePhase.Singleton.PhaseRequest.OnValueChanged += HandlePhaseChanged;
            GameSession.Singleton.Players.OnListChanged += HandleListChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
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
                GameSession.Singleton.Players.OnListChanged -= HandleListChanged;
            }
            
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
        }
        
        private void HandleClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.DisconnectEvent == NetworkTransport.DisconnectEvents.ClosedByRemote)
            {
                view.SetMessage(MessageResponse.SERVER_CLOSED);
                return;
            }
            
            view.Hide();
        }

        private void HandleListChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            PhaseChanged(GamePhase.Singleton.PhaseRequest.Value);
        }
        
        private void HandlePhaseChanged(PauseRequest oldRequest, PauseRequest newRequest)
        {
            PhaseChanged(newRequest);
        }

        private void PhaseChanged(PauseRequest newRequest)
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
            }
            
            view.Hide();
        }
    }
}