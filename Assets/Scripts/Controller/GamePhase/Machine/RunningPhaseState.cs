using Game.Patterns;
using Unity.Netcode;

namespace Game
{
    public sealed class RunningPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        private bool isChangingState;

        public RunningPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType => GamePhaseType.Running;

        public override void Enter()
        {
            if (!Entity.IsServer)
                return;

            isChangingState = false;

            GameSession.Singleton.Players.OnListChanged += HandlePlayersChanged;

            var warehouse = GameEntities.Singleton.Warehouse;

            warehouse.CurrentHealth.OnValueChanged += HandleHealthChanged;
            warehouse.CurrentSupply.OnValueChanged += HandleSupplyChanged;

            EvaluateState();
        }

        public override void Exit()
        {
            if (!Entity.IsServer)
                return;

            UnsubscribeEvents();
            isChangingState = false;
        }

        private void HandleHealthChanged(int previousValue, int newValue)
        {
            if (isChangingState)
                return;

            EvaluateState();
        }

        private void HandleSupplyChanged(int previousValue, int newValue)
        {
            if (isChangingState)
                return;

            EvaluateState();
        }

        private void HandlePlayersChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            if (isChangingState)
                return;

            EvaluateState();
        }

        private void EvaluateState()
        {
            if (isChangingState)
                return;

            var session = GameSession.Singleton;
            var warehouse = GameEntities.Singleton.Warehouse;

            /*
             * Determine the final state in one place.
             *
             * Defeat has priority over Victory when both conditions
             * become true at the same time.
             */
            if (warehouse.CurrentHealth.Value <= 0)
            {
                ChangeState(GamePhaseType.Defeat);
                return;
            }

            if (warehouse.IsFull)
            {
                ChangeState(GamePhaseType.Victory);
                return;
            }

            if (!session.IsFull)
            {
                ChangeState(GamePhaseType.Waiting);
            }
        }

        private void ChangeState(GamePhaseType stateType)
        {
            if (isChangingState)
                return;

            isChangingState = true;

            // Stop receiving events before changing state.
            UnsubscribeEvents();

            Entity.ChangeState(stateType);
        }

        private void UnsubscribeEvents()
        {
            GameSession.Singleton.Players.OnListChanged -= HandlePlayersChanged;

            var warehouse = GameEntities.Singleton.Warehouse;

            warehouse.CurrentHealth.OnValueChanged -= HandleHealthChanged;
            warehouse.CurrentSupply.OnValueChanged -= HandleSupplyChanged;
        }
    }
}