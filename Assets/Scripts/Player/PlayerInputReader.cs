using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerInputReader : PlayerInput.IGameActions
    {
        public event Action<Vector2> Move;
        public event Action Interact;

        private PlayerInput input = new();
        
        public void Enable()
        {
            input.Game.SetCallbacks(this);
            input.Game.Enable();
        }

        public void Disable()
        {
            input.Game.Disable();
            input.Game.SetCallbacks(null);
        }
        
        public void Dispose()
        {
            Disable();
            input.Dispose();
        }
        
        public void OnMovement(InputAction.CallbackContext context)
        {
            Move?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
                return;
            
            Interact?.Invoke();
        }
    }
}