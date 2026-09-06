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
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class Player : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private PlayerPickup pickup;

        private PlayerInputReader input;

        public PlayerMovement Movement => movement;
        public PlayerInteractor Interactor => interactor;
        public PlayerPickup Pickup => pickup;

        private void Awake()
        {
            movement ??= GetComponent<PlayerMovement>();
            interactor ??= GetComponent<PlayerInteractor>();
            pickup ??= GetComponent<PlayerPickup>();

            input = new PlayerInputReader();

            interactor.Initialize(this);
        }

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            interactor = GetComponent<PlayerInteractor>();
            pickup = GetComponent<PlayerPickup>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            interactor.Initialize(this);

            if (!IsOwner)
                return;

            input.Move += OnMove;
            input.Interact += OnInteract;

            GamePhase.Singleton.PhaseRequest.OnValueChanged +=
                HandlePhaseRequest;

            input.Enable();
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                input.Move -= OnMove;
                input.Interact -= OnInteract;

                GamePhase.Singleton.PhaseRequest.OnValueChanged -=
                    HandlePhaseRequest;

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
            SendMoveInputToServerRpc(input);
        }

        [Rpc(SendTo.Server)]
        private void SendMoveInputToServerRpc(Vector2 input)
        {
            movement.SetInput(input);
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
            {
                return;
            }

            if (!target.CanInteract(this))
            {
                return;
            }

            NetworkObject networkObject = component.GetComponent<NetworkObject>();

            if (networkObject == null || !networkObject.IsSpawned)
            {
                return;
            }

            SendInteractToServerRpc(networkObject);
        }

        private void TryDrop()
        {
            if (!pickup.IsCarrying)
            {
                return;
            }

            pickup.TryDrop();
        }


        [Rpc(SendTo.Server)]
        private void SendInteractToServerRpc(NetworkObjectReference objectReference)
        {
            if (!objectReference.TryGet(out NetworkObject networkObject))
                return;
            
            IInteractable interactable = networkObject.GetComponent<IInteractable>();

            if (interactable == null)
            {
                return;
            }
            
            bool canInteract = interactable.CanInteract(this);
            
            if (!canInteract)
            {
                return;
            }

            interactable.Interact(this);
        }

        private void FixedUpdate()
        {
            if (!IsServer)
                return;

            movement.Process();
        }
    }

    public interface IInteractable
    {
        void SetHighlight(bool isHighlighted);
        bool CanInteract(Player player);
        void Interact(Player player);
    }
}