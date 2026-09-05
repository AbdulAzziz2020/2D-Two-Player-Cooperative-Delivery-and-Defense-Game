using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class Supply : NetworkBehaviour, IInteractable
    {
        [SerializeField] private SpriteRenderer icon;
        
        public NetworkVariable<SupplyData> Data { get; private set; } = new();

        private SupplyData pendingSupply;

        public SupplySO Definition
        {
            get
            {
                Debug.Log("Data Supply: " + Data.Value);
                
                if (!Data.Value.IsValid)
                    return null;

                return SupplyCollection.Singleton.GetSupply(Data.Value.ToString());
            }
        }

        public event Action<Supply> Release;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Data.OnValueChanged += OnDataChanged;
            
            if (IsServer)
            {
                Debug.Log("Process Data Pending Supply: " + pendingSupply);
                Data.Value = pendingSupply;
            }
            
            RefreshVisual();
        }

        public void PrepareSpawn(SupplyData data)
        {
            if (!IsServer)
                return;
            
            pendingSupply = data;
            
            Debug.Log("Prepare Spawn Supply: " + data);
        }

        public override void OnNetworkDespawn()
        {
            Data.OnValueChanged -= OnDataChanged;

            Release?.Invoke(this);

            base.OnNetworkDespawn();
        }

        private void OnDataChanged(SupplyData previous, SupplyData current)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (icon == null)
                return;

            SupplySO definition = Definition;
            
            if (definition == null)
            {
                icon.enabled = false;
                icon.sprite = null;
                return;
            }

            icon.sprite = definition.Icon;
            icon.enabled = true;
        }

        public void SetHighlight(bool isHighlighted)
        {
            // TODO
        }

        public bool CanInteract(Player player)
        {
            if (player == null)
            {
                return false;
            }

            if (!IsSpawned)
            {
                return false;
            }

            if (!Data.Value.IsValid)
            {
                return false;
            }

            if (player.Pickup == null)
            {
                return false;
            }

            if (player.Pickup.IsCarrying)
            {
                return false;
            }
            
            return true;
        }

        public void Interact(Player player)
        {
            player.Pickup.TryPickup(this);
        }
    }
}