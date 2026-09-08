using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class GamePhase : NetworkBehaviour
    {
        public static GamePhase Singleton { get; private set; }

        private GamePhaseStateMachine machine;
        
        public NetworkVariable<GamePhaseType> Phase { get; private set; } = new();
        public NetworkVariable<PauseRequest> PhaseRequest { get; private set; } = new();
        
        private float countdownTimer;

        private void Awake()
        {
            Singleton = this;

            machine = new();
            machine.Initialize(this, GamePhaseType.Idle);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            GameSession.Singleton.Players.OnListChanged += HandleListChanged;
            PhaseRequest.Value = PauseRequest.Empty;
            ChangeState(GamePhaseType.Waiting);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                GameSession.Singleton.Players.OnListChanged -= HandleListChanged;
            }
            
            ChangeState(GamePhaseType.Idle);
            
            base.OnNetworkDespawn();
        }
        
        private void HandleListChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<PlayerSession>.EventType.RemoveAt)
            {
                if(Phase.Value == GamePhaseType.Victory || Phase.Value == GamePhaseType.Defeat)
                {
                    ChangeState(GamePhaseType.Restart);
                }
                
                if (PhaseRequest.Value.isPause)
                {
                    PhaseRequest.Value = PauseRequest.Empty;
                    ChangeState(GamePhaseType.Waiting);
                }
            }
        }
        
        public void ChangeState(GamePhaseType state)
        {
            if (!IsServer)
                return;

            if (Phase.Value == state)
                return;
            
            Phase.Value = state;
            machine.ChangeState(state);
        }

        private void Update()
        {
            if (!IsServer)
                return;
            
            machine?.Update(Time.deltaTime);
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
            Debug.Log($"Pause requested by {pauseRequest.senderId}, {pauseRequest.isPause}");
            ChangeState(GamePhaseType.Paused);
            PhaseRequest.Value = pauseRequest;
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
            // Hanya player yang melakukan pause
            // yang boleh melakukan resume.
            if (!pauseRequest.IsSame(PhaseRequest.Value.senderId))
            {
                return;
            }

            PhaseRequest.Value = PauseRequest.Empty;
            if (GameSession.Singleton.IsFull)
            {
                ChangeState(GamePhaseType.Running);
                return;
            }
            
            ChangeState(GamePhaseType.Waiting);
        }

        public void Restart()
        {
            if (!IsServer)
                return;
            
            ChangeState(GamePhaseType.Restart);
        }
    }
}