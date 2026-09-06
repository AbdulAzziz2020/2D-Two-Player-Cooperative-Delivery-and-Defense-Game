using Unity.Netcode;

namespace Game.UI
{
    public class OnScreenPresenter : UIPresenter<OnScreenView>
    {
        private void Start()
        {
            view.Pause += HandlePause;

            NetworkManager.Singleton.OnClientConnectedCallback += HandleConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
        }

        private void OnDestroy()
        {
            view.Pause -= HandlePause;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnect;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleConnect;
            }
        }

        private void HandleConnect(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
                return;
            
            view.Show();
        } 

        private void HandleDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.DisconnectEvent == NetworkTransport.DisconnectEvents.TransportShutdown
                || clientId == NetworkManager.Singleton.LocalClientId)
            {
                view.Hide();
            }
        }

        private void HandlePause()
        {
            GamePhase.Singleton.RequestPause(PauseRequest.Pause(NetworkManager.Singleton.LocalClientId));
        }
    }
}