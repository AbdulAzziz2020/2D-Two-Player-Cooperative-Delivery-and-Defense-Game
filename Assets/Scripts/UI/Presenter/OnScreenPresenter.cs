using Unity.Netcode;

namespace Game.UI
{
    public class OnScreenPresenter : UIPresenter<OnScreenView>
    {
        private void Start()
        {
            view.Pause += HandlePause;

            GameSession.Singleton.Players.OnListChanged += HandlePlayersChanged;
            CautionPresenter.OnDisconnected += view.Hide;
            PausePresenter.Disconnected += view.Hide;
        }

        private void OnDestroy()
        {
            view.Pause -= HandlePause;
            if(GameSession.Singleton != null)
                GameSession.Singleton.Players.OnListChanged -= HandlePlayersChanged;
            
            CautionPresenter.OnDisconnected -= view.Hide;
            PausePresenter.Disconnected -= view.Hide;
        }

        private void HandlePlayersChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerSession>.EventType.Add:
                    if(changeEvent.Value.clientId == NetworkManager.Singleton.LocalClientId)
                        view.Show();
                    break;
                case NetworkListEvent<PlayerSession>.EventType.RemoveAt:
                    if(changeEvent.Value.clientId == NetworkManager.Singleton.LocalClientId)
                        view.Hide();
                    break;
            }
        }

        private void HandlePause()
        {
            GamePhase.Singleton.RequestPause(PauseRequest.Pause(NetworkManager.Singleton.LocalClientId));
        }
    }
}