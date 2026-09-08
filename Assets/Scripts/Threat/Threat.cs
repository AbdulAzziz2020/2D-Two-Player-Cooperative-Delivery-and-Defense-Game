using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(ThreatTargetFinder))]
    [RequireComponent(typeof(ThreatPatrol))]
    public class Threat : NetworkBehaviour
    {
        private ThreatStateMachine machine;

        private ThreatTargetFinder _targetFinder;
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

            _targetFinder = GetComponent<ThreatTargetFinder>();
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
            Debug.Log("New State: " + newValue);
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            stateText.text = state.Value.ToString();
        }

        public NetworkObject TargetObject => _targetFinder.TargetObject;

        public bool TryFindTarget()
        {
            return _targetFinder.TryFindTarget();
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

            state.Value = newState;
            machine.ChangeState(newState);
        }
        
        public void Rotate2D(Vector3 target)
        {
            Vector3 direction = target - transform.position;

            if (direction.sqrMagnitude <= 0f)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            visual.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        public void TakeDamage()
        {
            ChangeState(ThreatState.Die);
        }
    }
}