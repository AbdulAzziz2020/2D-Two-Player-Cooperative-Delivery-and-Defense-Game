using System;
using Game.Patterns;

namespace Game
{
    public enum GamePhaseType
    {
        Idle,
        Waiting,
        Running,
        Victory,
        Defeat,
        Paused,
        Restart
    }
    
    public class GamePhaseStateMachine : StateMachine<GamePhase, GamePhaseType>
    {
        public override void Initialize(GamePhase entity, GamePhaseType startType)
        {
            Register(new IdlePhaseState(entity, this));
            Register(new WaitingPhaseState(entity, this));
            Register(new RunningPhaseState(entity, this));
            Register(new VictoryPhaseState(entity, this));
            Register(new DefeatPhaseState(entity, this));
            Register(new PausedPhaseState(entity, this));
            Register(new RestartPhaseState(entity, this));
            
            ChangeState(startType);
        }
    }
}