using Game.Patterns;

namespace Game
{
    public class IdlePhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public IdlePhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType => GamePhaseType.Idle;
    }
}