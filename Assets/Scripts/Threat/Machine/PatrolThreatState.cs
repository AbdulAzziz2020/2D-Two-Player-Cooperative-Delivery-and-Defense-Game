using Game.Patterns;
using UnityEngine;

namespace Game
{
    public class PatrolThreatState : BaseState<Threat, ThreatState>
    {
        private const float ARRIVAL_DISTANCE = 0.1f;

        public PatrolThreatState(
            Threat entity,
            StateMachine<Threat, ThreatState> stateMachine)
            : base(entity, stateMachine)
        {
        }

        public override ThreatState StateType => ThreatState.Patrol;

        public override void Enter()
        {
            if (!Entity.IsServer)
                return;

            Entity.StartPatrol();
        }

        public override void Update(float deltaTime)
        {
            if (!Entity.IsServer)
                return;

            // Prioritas pertama: cari target.
            if (Entity.TryFindTarget())
            {
                Entity.ChangeState(ThreatState.Attack);
                return;
            }

            if (!Entity.TryGetPatrolTarget(out Vector2 target))
                return;

            Vector2 currentPosition = Entity.transform.position;

            Vector2 nextPosition = Vector2.MoveTowards(
                currentPosition,
                target,
                Entity.MoveSpeed * deltaTime
            );

            Entity.transform.position = nextPosition;

            if (Vector2.Distance(nextPosition, target) > ARRIVAL_DISTANCE)
                return;

            Entity.CompletePatrol();

            // Setelah sampai, coba cari target lagi.
            if (Entity.TryFindTarget())
            {
                Entity.ChangeState(ThreatState.Attack);
                return;
            }

            Entity.ChangeState(ThreatState.Idle);
        }
    }
}