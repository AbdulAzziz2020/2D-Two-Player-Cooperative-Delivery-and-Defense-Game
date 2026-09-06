using Game.Patterns;

namespace Game
{
    public class DefeatPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public DefeatPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType => GamePhaseType.Defeat;
    }
}