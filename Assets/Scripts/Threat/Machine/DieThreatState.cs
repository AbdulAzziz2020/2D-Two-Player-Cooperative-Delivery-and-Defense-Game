using Game.Patterns;

namespace Game
{
    public class DieThreatState : BaseState<Threat, ThreatState>
    {
        public DieThreatState(Threat entity, StateMachine<Threat, ThreatState> stateMachine) : base(entity, stateMachine)
        {
        }
        
        public override ThreatState StateType => ThreatState.Die;

        public override void Enter()
        {
            if (!Entity.IsServer)
                return;

            if (!Entity.IsSpawned)
                return;

            Entity.NetworkObject.Despawn(false);
        }
    }
}