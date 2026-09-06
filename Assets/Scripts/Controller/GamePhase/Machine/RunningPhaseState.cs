using Game.Patterns;
using Unity.Netcode;

namespace Game
{
    public class RunningPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public RunningPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }
        
        public override GamePhaseType StateType => GamePhaseType.Running;

        public override void Enter()
        {
            if (!Entity.IsServer)
                return;

            GameSession.Singleton.Players.OnListChanged += HandleValueChanged;
        }

        public override void Exit()
        {
            if (!Entity.IsServer)
                return;
            
            GameSession.Singleton.Players.OnListChanged -= HandleValueChanged;
        }

        private void HandleValueChanged(NetworkListEvent<PlayerSession> changeEvent)
        {
            if (!Entity.IsServer)
                return;

            if (!GameSession.Singleton.IsFull)
            {
                Entity.ChangeState(GamePhaseType.Waiting);
            }
        }
    }
}