using Game.Patterns;

namespace Game
{
    public class VictoryPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public VictoryPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType => GamePhaseType.Victory;
    }
}