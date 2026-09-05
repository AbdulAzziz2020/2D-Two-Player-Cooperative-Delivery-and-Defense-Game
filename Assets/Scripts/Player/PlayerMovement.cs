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

        [SerializeField] private NetworkVariable<Vector2> moveInput = new();
        [SerializeField] private NetworkVariable<Vector2> facingDirection = new(Vector2.down);

        public Vector2 MoveInput => moveInput.Value;
        public Vector2 FacingDirection => facingDirection.Value;

        private void Awake()
        {
            rb ??= GetComponent<NetworkRigidbody2D>();
        }

        private void Reset()
        {
            rb = GetComponent<NetworkRigidbody2D>();
        }

        public void SetInput(Vector2 input)
        {
            moveInput.Value = input;

            if (input.sqrMagnitude > 0.0001f)
            {
                facingDirection.Value = input.normalized;
            }
        }

        public void Process()
        {
            rb.Rigidbody2D.linearVelocity = moveInput.Value * moveSpeed;
        }

        public void Stop()
        {
            moveInput.Value = Vector2.zero;
            rb.Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
}