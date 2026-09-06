using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(ThreatSupplyFinder))]
    [RequireComponent(typeof(ThreatPatrol))]
    public class Threat : NetworkBehaviour
    {
        private ThreatStateMachine machine;

        private ThreatSupplyFinder supplyFinder;
        private ThreatPatrol patrol;

        private NetworkVariable<ThreatState> state = new();

        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Transform visual;
        [SerializeField] private float moveSpeed = 2f;

        public event Action<Threat> Release;

        public float MoveSpeed => moveSpeed;
        public ThreatState State => state.Value;

        private void Awake()
        {
            machine = new ThreatStateMachine();

            supplyFinder = GetComponent<ThreatSupplyFinder>();
            patrol = GetComponent<ThreatPatrol>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            state.OnValueChanged += HandleStateChanged;

            if (!IsServer)
                return;

            machine.Initialize(this, ThreatState.Idle);
            state.Value = ThreatState.Idle;

            RefreshVisual();
        }

        public override void OnNetworkDespawn()
        {
            state.OnValueChanged -= HandleStateChanged;

            Release?.Invoke(this);

            base.OnNetworkDespawn();
        }

        private void HandleStateChanged(ThreatState previousValue, ThreatState newValue)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            stateText.text = state.Value.ToString();
        }

        public bool TryFindSupply()
        {
            return supplyFinder.TryFindSupply();
        }

        public bool TryGetSupply(out Supply supply)
        {
            return supplyFinder.TryGetTarget(out supply);
        }

        public void StartPatrol()
        {
            if (!IsServer)
                return;

            patrol.StartPatrol();
        }

        public bool TryGetPatrolTarget(out Vector2 target)
        {
            target = patrol.Target;

            return !patrol.IsComplete;
        }

        public void CompletePatrol()
        {
            if (!IsServer)
                return;

            patrol.Complete();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            if (GamePhase.Singleton.Phase.Value != GamePhaseType.Running)
                return;

            machine?.Update(Time.deltaTime);
        }

        public void ChangeState(ThreatState newState)
        {
            if (!IsServer)
                return;

            if (state.Value == newState)
                return;

            machine.ChangeState(newState);
            state.Value = newState;
        }
    }
}