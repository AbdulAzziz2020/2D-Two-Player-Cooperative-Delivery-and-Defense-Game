using Game.Patterns;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class AttackThreatState : BaseState<Threat, ThreatState>
    {
        private const float ARRIVAL_DISTANCE = 0.1f;

        public AttackThreatState(
            Threat entity,
            StateMachine<Threat, ThreatState> stateMachine)
            : base(entity, stateMachine)
        {
        }

        public override ThreatState StateType => ThreatState.Attack;

        public override void Update(float deltaTime)
        {
            if (!Entity.IsServer)
                return;

            NetworkObject target = Entity.TargetObject;

            if (target == null)
            {
                Entity.ChangeState(ThreatState.Idle);
                return;
            }

            Vector2 currentPosition = Entity.transform.position;
            Vector2 targetPosition = target.transform.position;

            Vector2 nextPosition = Vector2.MoveTowards(
                currentPosition,
                targetPosition,
                Entity.MoveSpeed * deltaTime
            );

            Entity.transform.position = nextPosition;

            if (Vector2.Distance(nextPosition, targetPosition) > ARRIVAL_DISTANCE)
                return;

            if (target.TryGetComponent(out Supply supply))
            {
                supply.TakeDamage();
            }
            else if (target.TryGetComponent(out Warehouse warehouse))
            {
                warehouse.TakeDamage();
                Entity.ChangeState(ThreatState.Die);
                return;
            }

            Entity.ChangeState(ThreatState.Idle);
        }
    }
}