using Game.Patterns;

namespace Game
{
    public class PausedPhaseState : BaseState<GamePhase, GamePhaseType>
    {
        public PausedPhaseState(GamePhase entity, StateMachine<GamePhase, GamePhaseType> stateMachine) : base(entity, stateMachine)
        {
        }

        public override GamePhaseType StateType =>  GamePhaseType.Paused;
    }
}