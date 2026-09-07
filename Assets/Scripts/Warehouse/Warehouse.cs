using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class Warehouse : NetworkBehaviour, IInteractable
    {
        [Header("Supply")]
        [SerializeField] private int maxSupply = 100;

        [Header("Health")]
        [SerializeField] private int maxHealth = 100;

        public NetworkVariable<int> CurrentSupply { get; private set; } = new();
        public NetworkVariable<int> CurrentHealth { get; private set; } = new();

        [Header("UI")]
        [SerializeField] private TMP_Text progressText;

        public bool IsFull => CurrentSupply.Value >= maxSupply;
        public bool IsDestroyed => CurrentHealth.Value <= 0;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CurrentSupply.OnValueChanged += HandleSupplyChanged;
            CurrentHealth.OnValueChanged += HandleHealthChanged;

            if (IsServer)
            {
                CurrentSupply.Value = 0;
                CurrentHealth.Value = maxHealth;
            }

            RefreshVisual();
        }

        public override void OnNetworkDespawn()
        {
            CurrentSupply.OnValueChanged -= HandleSupplyChanged;
            CurrentHealth.OnValueChanged -= HandleHealthChanged;

            base.OnNetworkDespawn();
        }

        private void HandleSupplyChanged(
            int previousValue,
            int newValue)
        {
            RefreshVisual();
        }

        private void HandleHealthChanged(
            int previousValue,
            int newValue)
        {
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            progressText.text =
                $"Supply: {CurrentSupply.Value}/{maxSupply}\n" +
                $"Health: {CurrentHealth.Value}/{maxHealth}";
        }

        public void SetHighlight(bool isHighlighted)
        {
            //
        }

        public bool CanInteract(Player player)
        {
            return !IsFull &&
                   player.Pickup.IsCarrying;
        }

        public void Interact(Player player)
        {
            player.Pickup.TryDropToWarehouse(this);
        }

        public void AddSupply(int amount)
        {
            if (!IsServer)
                return;

            CurrentSupply.Value = Mathf.Min(
                CurrentSupply.Value + amount,
                maxSupply
            );
        }

        public void TakeDamage()
        {
            if (!IsServer)
                return;

            if (IsDestroyed)
                return;

            CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - 10, 0);
        }
        

        public void Reset()
        {
            if (!IsServer)
                return;

            CurrentSupply.Value = 0;
            CurrentHealth.Value = maxHealth;
        }
    }
}