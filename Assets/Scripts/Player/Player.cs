using System;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(PlayerPickup))]
    [RequireComponent(typeof(PlayerAttack))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Player : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerPickup pickup;
        [SerializeField] private PlayerAttack attack;

        private PlayerInputReader input;

        public PlayerMovement Movement => movement;
        public PlayerInteractor Interactor => interactor;
        public PlayerPickup Pickup => pickup;
        public PlayerAttack Attack => attack;

        private void Awake()
        {
            movement ??= GetComponent<PlayerMovement>();
            interactor ??= GetComponent<PlayerInteractor>();
            pickup ??= GetComponent<PlayerPickup>();
            attack ??= GetComponent<PlayerAttack>();

            input = new PlayerInputReader();

            interactor.Initialize(this);
        }

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            interactor = GetComponent<PlayerInteractor>();
            pickup = GetComponent<PlayerPickup>();
            attack = GetComponent<PlayerAttack>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            interactor.Initialize(this);

            if (!IsOwner)
            {
                Collider2D collider = GetComponent<Collider2D>();

                if (collider != null)
                    collider.enabled = false;

                return;
            }

            input.Move += OnMove;
            input.Interact += OnInteract;
            input.Attack += OnAttack;

            GamePhase.Singleton.PhaseRequest.OnValueChanged += HandlePhaseRequest;
            CameraFollow.Singleton.Set(transform);

            if (!GamePhase.Singleton.PhaseRequest.Value.isPause)
            {
                input.Enable();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                input.Move -= OnMove;
                input.Interact -= OnInteract;
                input.Attack -= OnAttack;

                GamePhase.Singleton.PhaseRequest.OnValueChanged -= HandlePhaseRequest;

                input.Dispose();
            }

            base.OnNetworkDespawn();
        }

        private void HandlePhaseRequest(
            PauseRequest previousValue,
            PauseRequest newValue)
        {
            if (newValue.isPause)
                input.Disable();
            else
                input.Enable();
        }

        private void OnMove(Vector2 input)
        {
            if (!IsOwner)
                return;

            movement.SetInput(input);
        }

        private void OnAttack()
        {
            if (!IsOwner)
                return;

            if (pickup.IsCarrying)
                return;

            SendAttackToServerRpc();
        }

        private void OnInteract()
        {
            if (pickup == null || interactor == null)
            {
                TryDrop();
                return;
            }

            if (!interactor.TryGetTarget(out IInteractable target))
            {
                TryDrop();
                return;
            }

            if (target is not Component component)
                return;

            if (!target.CanInteract(this))
                return;

            NetworkObject networkObject =
                component.GetComponent<NetworkObject>();

            if (networkObject == null || !networkObject.IsSpawned)
                return;

            SendInteractToServerRpc(networkObject);
        }

        private void TryDrop()
        {
            if (!pickup.IsCarrying)
                return;

            pickup.TryDrop();
        }

        [Rpc(SendTo.Server)]
        private void SendAttackToServerRpc()
        {
            if (pickup.IsCarrying)
                return;

            attack.Attack(movement.FacingDirection);
        }

        [Rpc(SendTo.Server)]
        private void SendInteractToServerRpc(
            NetworkObjectReference objectReference)
        {
            if (!objectReference.TryGet(out NetworkObject networkObject))
                return;

            IInteractable interactable =
                networkObject.GetComponent<IInteractable>();

            if (interactable == null)
                return;

            if (!interactable.CanInteract(this))
                return;

            interactable.Interact(this);
        }

        private void FixedUpdate()
        {
            if (!IsOwner)
                return;

            movement.Process();
        }
    }
}