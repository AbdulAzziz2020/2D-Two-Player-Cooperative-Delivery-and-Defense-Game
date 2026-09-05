using System;
using Unity.Netcode;

namespace Game
{
    public enum SupplyState
    {
        None,
        Available,
        Picked,
    }
    
    public class Supply : NetworkBehaviour, IInteractable
    {
        public event Action<Supply> OnReleased;

        public NetworkVariable<SupplyState> State { get; private set; } =
            new(
                SupplyState.None,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
                return;

            State.Value = SupplyState.Available;
        }

        public bool CanInteract(Player owner)
        {
            if (owner == null)
                return false;

            if (!IsSpawned)
                return false;

            if (State.Value != SupplyState.Available)
                return false;

            if (owner.Pickup == null)
                return false;

            if (owner.Pickup.HasSupply)
                return false;

            return true;
        }

        public void Interact(Player player)
        {
            if (!IsServer)
                return;

            if (!CanInteract(player))
                return;

            if (!player.Pickup.TryPickup(this))
                return;

            State.Value = SupplyState.Picked;
        }

        public void SetAvailable()
        {
            if (!IsServer)
                return;

            State.Value = SupplyState.Available;
        }

        public void SetPicked()
        {
            if (!IsServer)
                return;

            State.Value = SupplyState.Picked;
        }

        public void Release()
        {
            if (!IsServer)
                return;

            if (!IsSpawned)
                return;

            OnReleased?.Invoke(this);
        }

        public void SetHighlight(bool isHighlighted)
        {
            // Visual only.
            //
            // Contoh:
            // highlight.SetActive(isHighlighted);
        }
    }
}