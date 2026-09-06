using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class Warehouse : NetworkBehaviour, IInteractable
    {
        [SerializeField] private int maxCapacity = 100;
        
        public NetworkVariable<int> CurrentCapacity { get; private set; } = new();

        [Header("UI")] 
        [SerializeField] private TMP_Text progressText;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CurrentCapacity.OnValueChanged += HandleCapacityChanged;
            
            RefreshVisual();
        }

        public override void OnNetworkDespawn()
        {
            CurrentCapacity.OnValueChanged -= HandleCapacityChanged;
            
            if (IsServer)
            {
                CurrentCapacity.Value = 0;
            }
            
            base.OnNetworkDespawn();
        }

        private void HandleCapacityChanged(int previousValue, int newValue)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            progressText.text = $"{CurrentCapacity.Value}/{maxCapacity}";
        }

        public void SetHighlight(bool isHighlighted)
        {
            //
        }

        public bool CanInteract(Player player)
        {
            return player.Pickup.IsCarrying;
        }

        public void Interact(Player player)
        {
            player.Pickup.TryDropToWarehouse(this);
        }

        public void UpdateCapacity(int amount)
        {
            if (!IsServer)
                return;
            
            CurrentCapacity.Value = Math.Min(CurrentCapacity.Value + amount, maxCapacity);
        }
    }
}