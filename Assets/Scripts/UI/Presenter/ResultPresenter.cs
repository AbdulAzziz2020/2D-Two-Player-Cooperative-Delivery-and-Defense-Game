using Unity.Netcode;

namespace Game.UI
{
    public class ResultPresenter : UIPresenter<ResultView>
    {
        public void Start()
        {
            GamePhase.Singleton.Phase.OnValueChanged += HandlePhaseChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

            view.Retry += HandleRetry;
            view.Disconnect += HandleDisconnect;
        }

        public void OnDestroy()
        {
            if(GamePhase.Singleton != null)
                GamePhase.Singleton.Phase.OnValueChanged -= HandlePhaseChanged;
            
            if(NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;

            view.Retry -= HandleRetry;
            view.Disconnect -= HandleDisconnect;
        }
        
        
        private void HandleClientDisconnect(ulong obj)
        {
            view.Hide();
        }
        
        private void HandleRetry()
        {
            GamePhase.Singleton.Restart();
        }
        
        private void HandleDisconnect()
        {
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
                
            }
        }

        private void HandlePhaseChanged(GamePhaseType previousValue, GamePhaseType newValue)
        {
            if (newValue == GamePhaseType.Victory)
                HandleVictory();
            else if (newValue == GamePhaseType.Defeat)
                HandleDefeat();
            else 
                view.Hide();
        }

        private void HandleVictory()
        {
            bool isHost = NetworkManager.Singleton.IsHost;
            
            view.SetResultText("We win.")
                .SetButtonDisconnect(true)
                .SetButtonRetry(isHost)
                .Show();
        }

        private void HandleDefeat()
        {
            bool isHost = NetworkManager.Singleton.IsHost;
            
            view.SetResultText("We Lose")
                .SetButtonDisconnect(true)
                .SetButtonRetry(isHost)
                .Show();
            
            view.Show();
        }
    }
}