using Game.Patterns;

namespace Game
{
    public class AbortedPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public AbortedPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType => GamePhaseType.Aborted;
    }
}