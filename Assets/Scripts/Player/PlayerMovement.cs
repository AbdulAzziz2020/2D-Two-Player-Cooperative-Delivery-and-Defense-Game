using System;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private NetworkRigidbody2D rb;
        [SerializeField] private float moveSpeed = 5f;

        [SerializeField] private Vector2 moveInput;
        [SerializeField] private Vector2 lastDirection = Vector2.down;

        public Vector2 MoveInput => moveInput;
        public Vector2 LastDirection => lastDirection;

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

            if (input.sqrMagnitude > 0.001f)
                lastDirection = input.normalized;
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