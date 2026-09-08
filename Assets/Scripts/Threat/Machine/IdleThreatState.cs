using Cysharp.Threading.Tasks;
using Game.Patterns;

namespace Game
{
    public class IdleThreatState : BaseState<Threat, ThreatState>
    {
        private const int IDLE_DELAY = 2000;

        public IdleThreatState(
            Threat entity,
            StateMachine<Threat, ThreatState> stateMachine)
            : base(entity, stateMachine)
        {
        }

        public override ThreatState StateType => ThreatState.Idle;

        public override async void Enter()
        {
            await UniTask.Delay(IDLE_DELAY);

            if (Entity == null)
                return;

            if (!Entity.IsServer)
                return;

            Entity.ChangeState(ThreatState.Search);
        }
    }
}