using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class PausePresenter : UIPresenter<PauseView>
    {
        private void Start()
        {
            view.Resume += HandleResume;
            view.Quit += HandleQuit;
            
            GamePhase.Singleton.PhaseRequest.OnValueChanged += HandlePhaseChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
        

        private void HandleClientDisconnect(ulong clientId)
        {
            view.Hide();
        }

        private void OnDestroy()
        {
            if (GamePhase.Singleton != null)
            {
                GamePhase.Singleton.PhaseRequest.OnValueChanged -= HandlePhaseChanged;
            }

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
            
            view.Resume -= HandleResume;
            view.Quit -= HandleQuit;
        }
        
        private void HandlePhaseChanged(PauseRequest pauseRequest, PauseRequest request)
        {
            if (request.IsSame(NetworkManager.Singleton.LocalClientId) && request.isPause)
            {
                view.Show();
                return;
            }
            
            view.Hide();
        }
        
        private void HandleQuit()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
                view.Hide();
            }
        }

        private void HandleResume()
        {
            GamePhase.Singleton.RequestResume(PauseRequest.Resume(NetworkManager.Singleton.LocalClientId));
        }
    }
}