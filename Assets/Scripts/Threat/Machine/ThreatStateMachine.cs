using Game.Patterns;

namespace Game
{
    

    public enum ThreatState
    {
        Idle,
        Attack,
        Patrol,
        Die
    }
    
    public class ThreatStateMachine : StateMachine<Threat, ThreatState>
    {
        public override void Initialize(
            Threat entity,
            ThreatState startType)
        {
            Register(new IdleThreatState(entity, this));
            Register(new PatrolThreatState(entity, this));
            Register(new AttackThreatState(entity, this));
            Register(new DieThreatState(entity, this));

            ChangeState(startType);
        }
    }
}