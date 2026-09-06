using Cysharp.Threading.Tasks;
using Game.Patterns;

namespace Game
{
    public class IdleThreatState : BaseState<Threat, ThreatState>
    {
        public IdleThreatState(
            Threat entity,
            StateMachine<Threat, ThreatState> stateMachine)
            : base(entity, stateMachine)
        {
        }

        public override ThreatState StateType => ThreatState.Idle;

        public override async void Enter()
        {
            await UniTask.Delay(2000);

            if (Entity == null)
                return;

            if (!Entity.IsServer)
                return;

            if (Entity.TryFindSupply())
            {
                Entity.ChangeState(ThreatState.Attack);
                return;
            }

            Entity.ChangeState(ThreatState.Patrol);
        }
    }
}