using Game.Patterns;
using UnityEngine;

namespace Game
{
    public sealed class PatrolThreatState : BaseState<Threat, ThreatState>
    {
        private const float ARRIVAL_DISTANCE = 0.1f;
        private const float ARRIVAL_DISTANCE_SQR =
            ARRIVAL_DISTANCE * ARRIVAL_DISTANCE;

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

            if (Entity.TryFindTarget())
            {
                Entity.ChangeState(ThreatState.Attack);
                return;
            }

            if (!Entity.TryGetPatrolTarget(out Vector2 target))
            {
                Entity.ChangeState(ThreatState.Idle);
                return;
            }

            Vector2 currentPosition = Entity.transform.position;
            Entity.Rotate2D(target);

            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, target, Entity.MoveSpeed * deltaTime);
            Entity.transform.position = nextPosition;

            if ((nextPosition - target).sqrMagnitude > ARRIVAL_DISTANCE_SQR)
            {
                return;
            }

            Entity.CompletePatrol();
            Entity.ChangeState(ThreatState.Idle);
        }
    }
}