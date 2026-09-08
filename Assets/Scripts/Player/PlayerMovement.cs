using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private NetworkRigidbody2D rb;
        [SerializeField] private float moveSpeed = 5f;

        private Vector2 moveInput;
        public NetworkVariable<Vector2> FacingDirection { get; private set; } = new (Vector2.down, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public Vector2 MoveInput => moveInput;

        private void Awake()
        {
            rb ??= GetComponent<NetworkRigidbody2D>();
        }

        private void Reset()
        {
            rb = GetComponent<NetworkRigidbody2D>(); 
            FacingDirection.Value = Vector2.down;
        }

        public void SetInput(Vector2 input)
        {
            moveInput = input;

            if (!IsOwner)
                return;
            
            if (input.sqrMagnitude > 0.0001f)
            {
                FacingDirection.Value = input.normalized;
            }
        }

        public void Process()
        {
            rb.Rigidbody2D.linearVelocity = moveInput * moveSpeed;
        }

        public void Stop()
        {
            moveInput = Vector2.zero;
            rb.Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
}