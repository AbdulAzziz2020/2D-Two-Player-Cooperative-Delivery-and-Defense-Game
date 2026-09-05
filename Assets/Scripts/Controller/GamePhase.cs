using PlasticPipe.PlasticProtocol.Messages;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public enum GamePhaseType
    {
        None,
        Waiting,
        Countdown,
        Running,
        Victory,
        Defeat,
        Paused,
        Aborted
    }

    public class GamePhase : NetworkBehaviour
    {
        public static GamePhase Singleton { get; private set; }

        [Header("Settings")]
        [SerializeField] private int countdownDuration = 3;

        public NetworkVariable<GamePhaseType> Phase { get; private set; } = new();
        public NetworkVariable<int> Countdown { get; private set; } = new();
        public NetworkVariable<PauseRequest> PhaseRequest { get; private set; } = new();
        
        private float countdownTimer;

        private void Awake()
        {
            Singleton = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            if (GameSession.Singleton != null)
            {
                GameSession.Singleton.Players.OnListChanged += HandlePlayersChanged;
            }

            UpdatePhaseFromPlayerCount();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && GameSession.Singleton != null)
            {
                GameSession.Singleton.Players.OnListChanged -= HandlePlayersChanged;
            }

            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (Phase.Value != GamePhaseType.Countdown)
                return;

            UpdateCountdown();
        }

        private void HandlePlayersChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            if (!IsServer)
                return;

            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerSession>.EventType.RemoveAt:
                    ForceResume();
                    break;
            }
            
            UpdatePhaseFromPlayerCount();
        }

        private void ForceResume()
        {
            if (!IsServer)
                return;

            PhaseRequest.Value = PauseRequest.Empty;
        }

        private void UpdatePhaseFromPlayerCount()
        {
            if (!GameSession.Singleton.IsFull)
            {
                SetPhase(GamePhaseType.Waiting);
                return;
            }

            if (Phase.Value == GamePhaseType.Waiting)
            {
                StartCountdown();
            }
        }

        private void StartCountdown()
        {
            Phase.Value = GamePhaseType.Countdown;
            Countdown.Value = countdownDuration;
            countdownTimer = 0f;
        }

        private void UpdateCountdown()
        {
            countdownTimer += Time.deltaTime;

            if (countdownTimer < 1f)
                return;

            countdownTimer -= 1f;
            Countdown.Value--;

            if (Countdown.Value <= 0)
            {
                Countdown.Value = 0;
                Phase.Value = GamePhaseType.Running;
            }
        }

        // =========================================================
        // PAUSE
        // =========================================================

        public void RequestPause(PauseRequest pauseRequest)
        {
            RequestPauseRpc(pauseRequest);
        }

        [Rpc(SendTo.Server)]
        private void RequestPauseRpc(PauseRequest pauseRequest)
        {
            if (Phase.Value != GamePhaseType.Running)
                return;

            Debug.Log($"Pause requested by {pauseRequest.senderId}, {pauseRequest.isPause}");
            PhaseRequest.Value = pauseRequest;
            Phase.Value = pauseRequest.isPause ? GamePhaseType.Paused : GamePhaseType.Running;
        }

        // =========================================================
        // RESUME
        // =========================================================

        public void RequestResume(PauseRequest pauseRequest)
        {
            RequestResumeRpc(pauseRequest);
        }

        [Rpc(SendTo.Server)]
        private void RequestResumeRpc(PauseRequest pauseRequest)
        {
            if (Phase.Value != GamePhaseType.Paused)
                return;

            // Hanya player yang melakukan pause
            // yang boleh melakukan resume.
            if (!pauseRequest.IsSame(PhaseRequest.Value.senderId))
            {
                return;
            }

            PhaseRequest.Value = PauseRequest.Empty;
            Phase.Value = pauseRequest.isPause ? GamePhaseType.Paused : GamePhaseType.Running;
        }
        
        public void SetVictory()
        {
            if (!IsServer)
                return;

            if (Phase.Value != GamePhaseType.Running && 
                Phase.Value != GamePhaseType.Paused)
                return;

            Phase.Value = GamePhaseType.Victory;
        }

        public void SetDefeat()
        {
            if (!IsServer)
                return;

            if (Phase.Value != GamePhaseType.Running && 
                Phase.Value != GamePhaseType.Paused)
                return;

            Phase.Value = GamePhaseType.Defeat;
        }

        public void AbortGame()
        {
            if (!IsServer)
                return;

            Phase.Value = GamePhaseType.Aborted;
        }

        private void SetPhase(GamePhaseType phase)
        {
            if (!IsServer)
                return;

            if (Phase.Value == phase)
                return;

            Phase.Value = phase;
        }

        public override void OnDestroy()
        {
            if (Singleton == this)
                Singleton = null;
        }
    }
}