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
            NetworkManager networkManager = NetworkManager.Singleton;

            view.Host += HandleHost;
            view.Client += HandleClient;
            view.NameChanged += HandleNameChanged;

            networkManager.OnClientDisconnectCallback += HandleDisconnect;
            networkManager.OnClientConnectedCallback += HandleConnect;
            networkManager.OnTransportFailure += HandleTransportFailure;

            view.Show();

            ResetUI();
        }

        private void OnDestroy()
        {
            view.Host -= HandleHost;
            view.Client -= HandleClient;
            view.NameChanged -= HandleNameChanged;

            if (NetworkManager.Singleton == null)
                return;

            NetworkManager networkManager = NetworkManager.Singleton;

            networkManager.OnClientDisconnectCallback -= HandleDisconnect;
            networkManager.OnClientConnectedCallback -= HandleConnect;
            networkManager.OnTransportFailure -= HandleTransportFailure;
        }

        private void HandleHost()
        {
            if (!HasValidName())
                return;

            NetworkManager networkManager = NetworkManager.Singleton;

            view.HideMessage();
            view.SetButtons(false);
            view.SetHostButtonText(MessageResponse.HOSTING_BUTTON.message.ToString());

            networkManager.NetworkConfig.ConnectionData = CreateConnectionData();

            bool success = networkManager.StartHost();

            if (success)
                return;

            Debug.LogError("Failed to start host.");

            ShowConnectionError(MessageResponse.HOST_START_FAILED.message.ToString());
        }

        private void HandleClient()
        {
            if (!HasValidName())
                return;

            NetworkManager networkManager = NetworkManager.Singleton;

            view.HideMessage();
            view.SetButtons(false);
            view.SetClientButtonText(MessageResponse.JOINING_BUTTON.message.ToString());

            networkManager.NetworkConfig.ConnectionData = CreateConnectionData();

            bool success = networkManager.StartClient();

            if (success)
                return;

            Debug.LogError("Failed to start client.");

            ShowConnectionError(MessageResponse.CLIENT_START_FAILED.message.ToString());
        }

        private void HandleConnect(ulong clientId)
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (clientId != networkManager.LocalClientId)
                return;

            Debug.Log("Connected successfully.");

            view.HideMessage();
            view.Hide();
        }

        private void HandleDisconnect(ulong clientId)
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (clientId != networkManager.LocalClientId)
                return;
            
            ShowConnectionError(MessageResponse.CONNECTION_FAILED.message.ToString());
        }

        private void HandleTransportFailure()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            Debug.LogError(
                $"Transport failure. " +
                $"Reason: {networkManager.DisconnectReason}"
            );

            ShowConnectionError(MessageResponse.TRANSPORT_FAILED.message.ToString());
        }

        private void HandleNameChanged(string value)
        {
            playerName = value;
        }

        private bool HasValidName()
        {
            if (!string.IsNullOrWhiteSpace(playerName))
                return true;

            view.SetMessage(MessageResponse.EMPTY_NAME.message.ToString());

            return false;
        }

        private void ShowConnectionError(string message)
        {
            view.Show();
            view.SetButtons(true);
            view.SetMessage(message);

            ResetButtonText();
        }

        private void ResetUI()
        {
            view.SetButtons(true);
            view.HideMessage();

            ResetButtonText();
        }

        private void ResetButtonText()
        {
            view.SetHostButtonText(MessageResponse.HOST_BUTTON.message.ToString());
            view.SetClientButtonText(MessageResponse.CLIENT_BUTTON.message.ToString());
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