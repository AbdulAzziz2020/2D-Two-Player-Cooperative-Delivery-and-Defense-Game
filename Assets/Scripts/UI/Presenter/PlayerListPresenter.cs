using System;
using Unity.Netcode;

namespace Game.UI
{
    public class PlayerListPresenter : UIPresenter<PlayerListView>
    {
        private void Start()
        {
            GameSession.Singleton.Players.OnListChanged += HandleListChanged;
            GamePhase.Singleton.Phase.OnValueChanged += HandlePhaseChanged;
            
            CautionPresenter.OnDisconnected += view.Hide;
            PausePresenter.Disconnected += view.Hide;
        }

        private void OnDestroy()
        {
            if (GameSession.Singleton != null)
            {
                GameSession.Singleton.Players.OnListChanged -= HandleListChanged;
            }

            if (GamePhase.Singleton != null)
            {
                GamePhase.Singleton.Phase.OnValueChanged -= HandlePhaseChanged;
            }
            
            CautionPresenter.OnDisconnected -= view.Hide;
            PausePresenter.Disconnected -= view.Hide;
        }

        private void HandlePhaseChanged(GamePhaseType previousValue, GamePhaseType newValue)
        {
            string phase = newValue switch
            {
                GamePhaseType.Waiting => "Waiting for other player",
                GamePhaseType.Running => "Game is Running",
                _ => String.Empty
            };
            
            view.SetStateText(phase);
        }

        private void HandlePlayerConnected(PlayerSession playerSession)
        {
            if (playerSession.clientId != NetworkManager.Singleton.LocalClientId)
                return;
            
            view.Show();
        }
        
        private void HandlePlayerDisconnected(PlayerSession playerSession)
        {
            if (playerSession.clientId != NetworkManager.Singleton.LocalClientId)
                return;
            
            view.Hide();
        }

        private void HandleListChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerSession>.EventType.Add:
                    HandlePlayerConnected(changeEvent.Value);
                    break;

                case NetworkListEvent<PlayerSession>.EventType.RemoveAt:
                    HandlePlayerDisconnected(changeEvent.Value);
                    break;
            }
            
            RefreshPlayerList();
        }

        private void RefreshPlayerList()
        {
            var playerList = GameSession.Singleton.Players;
            PlayerSession[] players = new PlayerSession[playerList.Count];

            for (int i = 0; i < playerList.Count; i++)
            {
                players[i] = playerList[i];
            }

            view.SetPlayerList(players);
        }
    }
}