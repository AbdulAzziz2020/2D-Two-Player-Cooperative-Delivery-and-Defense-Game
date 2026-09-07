using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class PlayerPickup : NetworkBehaviour
    {
        [Header("Drop")]
        [SerializeField] private float dropRadius = 1.5f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer pickedIcon;

        public NetworkVariable<SupplyData> Data { get; private set; } = new();

        public bool IsCarrying => Data.Value.IsValid;

        public SupplyData CurrentData => Data.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Data.OnValueChanged += OnDataChanged;

            RefreshVisual();
        }

        public override void OnNetworkDespawn()
        {
            Data.OnValueChanged -= OnDataChanged;

            base.OnNetworkDespawn();
        }

        public void TryPickup(Supply supply)
        {
            if (supply == null)
                return;

            if (IsCarrying)
                return;

            if (!supply.IsSpawned)
                return;

            PickupRpc(new NetworkObjectReference(supply.NetworkObject));
        }

        public void TryDrop()
        {
            if (!IsCarrying)
                return;

            DropRpc();
        }

        [Rpc(SendTo.Server)]
        private void PickupRpc(NetworkObjectReference supplyReference)
        {
            if (IsCarrying)
                return;

            if (!supplyReference.TryGet(out NetworkObject networkObject))
            {
                return;
            }

            Supply supply = networkObject.GetComponent<Supply>();

            if (supply == null)
                return;

            if (!ValidatePickup(supply))
                return;

            SupplyData data = supply.Data.Value;

            if (!data.IsValid)
                return;

            // Transfer item data dari world ke player.
            Data.Value = data;

            // Hapus world representation.
            supply.NetworkObject.Despawn(false);
        }

        [Rpc(SendTo.Server)]
        private void DropRpc()
        {
            if (!IsCarrying)
                return;

            SupplyData data = Data.Value;

            if (!data.IsValid)
                return;
            
            var dropPosition = GetRandomDropPosition();
            SupplySpawner.Singleton.SpecificSpawn(dropPosition, data);

            Data.Value = SupplyData.Empty;
        }

        private bool ValidatePickup(Supply supply)
        {
            if (!IsServer)
                return false;

            if (!supply.IsSpawned)
                return false;

            if (!supply.Data.Value.IsValid)
                return false;

            return true;
        }

        private Vector3 GetRandomDropPosition()
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;

            return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        private void OnDataChanged(SupplyData previous, SupplyData current)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (pickedIcon == null)
                return;

            if (!IsCarrying)
            {
                pickedIcon.enabled = false;
                pickedIcon.sprite = null;
                return;
            }

            SupplySO definition = SupplyCollection.Singleton.GetSupply(Data.Value.ToString());

            if (definition == null)
            {
                pickedIcon.enabled = false;
                pickedIcon.sprite = null;
                return;
            }

            pickedIcon.sprite = definition.Icon;
            pickedIcon.enabled = true;
        }

        public void TryDropToWarehouse(Warehouse warehouse)
        {
            if (!IsCarrying)
                return;
            
            RequestStoreToWarehouseRpc(new NetworkObjectReference(warehouse.NetworkObject));
        }

        [Rpc(SendTo.Server)]
        private void RequestStoreToWarehouseRpc(NetworkObjectReference warehouseRef)
        {
            if (!IsCarrying)
                return;

            if (!warehouseRef.TryGet(out NetworkObject networkObject))
                return;
            
            Warehouse warehouse = networkObject.GetComponent<Warehouse>();
            if (warehouse == null)
                return;
            
            if (!Data.Value.IsValid)
                return;
            
            warehouse.AddSupply(Data.Value);
            Data.Value = SupplyData.Empty;
        }
    }
}