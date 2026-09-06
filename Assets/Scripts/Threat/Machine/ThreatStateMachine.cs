using Game.Patterns;

namespace Game
{
    

    public enum ThreatState
    {
        Idle,
        Attack,
        Patrol
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

            ChangeState(startType);
        }
    }
}