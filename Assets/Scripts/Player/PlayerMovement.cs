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
        private Vector2 facingDirection = Vector2.down;

        public Vector2 MoveInput => moveInput;
        public Vector2 FacingDirection => facingDirection;

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
            moveInput = input;

            if (input.sqrMagnitude > 0.0001f)
            {
                facingDirection = input.normalized;
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