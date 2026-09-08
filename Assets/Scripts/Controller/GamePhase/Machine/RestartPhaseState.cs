using Game.Patterns;

namespace Game
{
    public class RestartPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public RestartPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType => GamePhaseType.Restart;

        public override void Enter()
        {
            if (!Entity.IsServer)
                return;

            if (!GameSession.Singleton.IsFull)
            {
                Entity.ChangeState(GamePhaseType.Waiting);
                return;
            }
            
            Entity.ChangeState(GamePhaseType.Running);
        }
    }
}