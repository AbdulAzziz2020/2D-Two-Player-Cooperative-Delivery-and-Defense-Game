using System;
using Cysharp.Threading.Tasks;
using Game.Patterns;
using PlasticPipe.PlasticProtocol.Messages;
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
        
        public void ChangeState(GamePhaseType state)
        {
            if (!IsServer)
                return;

            if (Phase.Value == state)
                return;
            
            machine.ChangeState(state);
            Phase.Value = state;
        }
        
        private void HandleListChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<PlayerSession>.EventType.RemoveAt && PhaseRequest.Value.isPause)
            {
                PhaseRequest.Value = PauseRequest.Empty;
                ChangeState(GamePhaseType.Waiting);
            }
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
            PhaseRequest.Value = pauseRequest;
            
            ChangeState(GamePhaseType.Paused);
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
    }
}