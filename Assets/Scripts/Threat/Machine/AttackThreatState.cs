using Game.Patterns;
using UnityEngine;

namespace Game
{
    public class AttackThreatState : BaseState<Threat, ThreatState>
    {
        private const float ARRIVAL_DISTANCE = 0.1f;
        
        public AttackThreatState(Threat entity, StateMachine<Threat, ThreatState> stateMachine) : base(entity, stateMachine)
        {
        }

        public override ThreatState StateType => ThreatState.Attack;

        public override void Update(float deltaTime)
        {
            if (!Entity.IsServer)
                return;

            if (!Entity.TryGetSupply(out Supply supply))
            {
                Entity.ChangeState(ThreatState.Idle);
                return;
            }
            
            Vector2 currentPosition = Entity.transform.position;
            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, supply.transform.position, Entity.MoveSpeed * deltaTime);
            
            Entity.transform.position = nextPosition;
            
            if (Vector2.Distance(nextPosition, supply.transform.position) > ARRIVAL_DISTANCE)
                return;
            
            supply.TakeDamage();
            
            Entity.ChangeState(ThreatState.Idle);
        }
    }
}