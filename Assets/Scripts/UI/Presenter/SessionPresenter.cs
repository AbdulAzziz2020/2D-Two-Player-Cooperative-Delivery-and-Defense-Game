using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class SessionPresenter : UIPresenter<SessionView>
    {
        private string playerName;

        private void Start()
        {
            view.Host += HandleHost;
            view.Client += HandleClient;
            view.NameChanged += HandleNameChanged;

            GameSession.Singleton.Players.OnListChanged += HandlePlayersChanged;
            CautionPresenter.OnDisconnected += view.Show;
            PausePresenter.Disconnected += view.Show;

            view.Show();
        }

        private void OnDestroy()
        {
            view.Host -= HandleHost;
            view.Client -= HandleClient;
            view.NameChanged -= HandleNameChanged;
            CautionPresenter.OnDisconnected -= view.Show;
            PausePresenter.Disconnected -= view.Show;

            if (GameSession.Singleton != null)
            {
                GameSession.Singleton.Players.OnListChanged -= HandlePlayersChanged;
            }
        }

        private void HandlePlayersChanged(NetworkListEvent<PlayerSession> changeEvent)
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
        }

        private void HandlePlayerDisconnected(PlayerSession playerSession)
        {
            if (playerSession.clientId != NetworkManager.Singleton.LocalClientId)
                return;
            
            playerName = String.Empty;
            
            view.SetButtons(true);
            view.Show();
            
        }

        private void HandlePlayerConnected(PlayerSession playerSession)
        {
            if (playerSession.clientId != NetworkManager.Singleton.LocalClientId)
                return;
            
            view.Hide();
        }

        private void HandleNameChanged(string value)
        {
            playerName = value;
        }

        private void HandleHost()
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return;

            NetworkManager networkManager = NetworkManager.Singleton;
            networkManager.NetworkConfig.ConnectionData = CreateConnectionData();

            bool success = networkManager.StartHost();

            view.SetButtons(false);

            Debug.Log($"Host started: {success}");
        }

        private void HandleClient()
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return;

            NetworkManager networkManager = NetworkManager.Singleton;
            networkManager.NetworkConfig.ConnectionData = CreateConnectionData();

            view.SetButtons(false);

            bool success = networkManager.StartClient();

            if (!success)
                view.SetButtons(true);

            Debug.Log($"Client started: {success}");

            if (!success &&
                !string.IsNullOrEmpty(networkManager.DisconnectReason))
            {
                Debug.LogWarning(
                    $"Connection failed: {networkManager.DisconnectReason}"
                );
            }
        }

        private byte[] CreateConnectionData()
        {
            string playerId = SystemInfo.deviceUniqueIdentifier;

            PlayerInfo info = new(playerId, playerName);
            string json = JsonUtility.ToJson(info);

            return Encoding.UTF8.GetBytes(json);
        }
    }
}