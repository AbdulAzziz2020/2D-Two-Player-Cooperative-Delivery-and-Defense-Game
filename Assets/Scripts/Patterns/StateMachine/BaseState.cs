using System;
using UnityEngine;

namespace Game.Patterns
{
    [Serializable]
    public abstract class BaseState<TEntity, TEnum> where TEnum : Enum
    {
        [SerializeField] protected TEntity entity;
        [SerializeField] protected StateMachine<TEntity, TEnum> stateMachine;
        
        public TEntity Entity => entity;
        public abstract TEnum StateType { get; }
        public StateMachine<TEntity, TEnum> StateMachine => stateMachine;
        
        protected BaseState(TEntity entity, StateMachine<TEntity, TEnum> stateMachine)
        {
            this.entity = entity;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter()
        {
            Debug.Log($"Enter {nameof(BaseState<TEntity, TEnum>)} {GetType().Name}");
        }
        
        public virtual void Update(float deltaTime) { }
        
        public virtual void FixedUpdate() { }
        
        public virtual void Exit(){ }
    }
}