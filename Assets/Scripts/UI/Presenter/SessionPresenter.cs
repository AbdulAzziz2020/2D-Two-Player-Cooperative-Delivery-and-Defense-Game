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

            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleConnect;
            
            view.Show();
        }

      
        private void OnDestroy()
        {
            view.Host -= HandleHost;
            view.Client -= HandleClient;
            view.NameChanged -= HandleNameChanged;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnect;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleConnect;
            }
        }
        
        private void HandleConnect(ulong obj)
        {
            if(obj != NetworkManager.Singleton.LocalClientId)
                return;
            
            view.Hide();
        }
        
        private void HandleDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.DisconnectEvent == NetworkTransport.DisconnectEvents.TransportShutdown ||
                clientId == NetworkManager.Singleton.LocalClientId)
            {
                view.Show();
                view.SetButtons(true);
            }
        }
        
        private void HandleNameChanged(string value)
        {
            playerName = value;
        }

        private void HandleHost()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    return;

                NetworkManager networkManager = NetworkManager.Singleton;
                networkManager.NetworkConfig.ConnectionData = CreateConnectionData();

                bool success = networkManager.StartHost();

                view.SetButtons(false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                view.SetButtons(true);
            }
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