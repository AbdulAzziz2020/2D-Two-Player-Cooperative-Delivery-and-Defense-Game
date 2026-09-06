using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Patterns
{
    [Serializable]
    public abstract class StateMachine<TEntity, TEnum> where TEnum : Enum
    {
        [SerializeReference] protected BaseState<TEntity, TEnum> current;
        [ShowInInspector] protected Dictionary<TEnum, BaseState<TEntity, TEnum>> states = new();
        
        public BaseState<TEntity, TEnum> Current => current;
        
        public abstract void Initialize(TEntity entity, TEnum startType);

        public virtual void ChangeState(TEnum newState)
        {
            current?.Exit();
            current = states[newState];
            current.Enter();
        }
        
        protected void Register(BaseState<TEntity, TEnum> state)
        {
            Debug.Log($"Registering state {state} : {state.StateType}");
            states[state.StateType] = state;
        }
        
        public virtual void Update(float deltaTime) => current?.Update(deltaTime);
        
        public virtual void FixedUpdate() => current?.FixedUpdate();
    }
}