using Game.Patterns;

namespace Game
{
    public sealed class SearchThreatState : BaseState<Threat, ThreatState>
    {
        public SearchThreatState(
            Threat entity,
            StateMachine<Threat, ThreatState> stateMachine)
            : base(entity, stateMachine)
        {
        }

        public override ThreatState StateType => ThreatState.Search;

        public override void Enter()
        {
            if (!Entity.IsServer)
                return;

            if (Entity.TryFindTarget())
            {
                Entity.ChangeState(ThreatState.Attack);
                return;
            }

            Entity.ChangeState(ThreatState.Patrol);
        }
    }
}