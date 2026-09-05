using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class PlayerPickup : MonoBehaviour
    {
        [SerializeField] private Transform carryPoint;

        private Player player;
        private Supply currentSupply;

        public Supply CurrentSupply => currentSupply;
        public bool HasSupply => currentSupply != null;

        public void Initialize(Player player)
        {
            this.player = player;
        }

        public bool TryPickup(Supply supply)
        {
            if (player == null)
                return false;

            if (supply == null)
                return false;

            if (currentSupply != null)
                return false;

            if (!supply.IsSpawned)
                return false;

            if (!player.IsServer)
                return false;

            currentSupply = supply;

            NetworkObject networkObject = supply.NetworkObject;

            if (!networkObject.TrySetParent(carryPoint, false))
            {
                currentSupply = null;
                return false;
            }

            networkObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return true;
        }

        public bool TryDrop()
        {
            if (currentSupply == null)
                return false;

            if (!player.IsServer)
                return false;

            Supply supply = currentSupply;
            currentSupply = null;

            NetworkObject networkObject = supply.NetworkObject;

            if (networkObject.IsSpawned)
            {
                networkObject.TryRemoveParent();
            }

            return true;
        }

        public bool TryRelease()
        {
            if (currentSupply == null)
                return false;

            if (!player.IsServer)
                return false;

            Supply supply = currentSupply;
            currentSupply = null;

            if (supply.IsSpawned)
            {
                supply.NetworkObject.TryRemoveParent();
            }

            supply.Release();

            return true;
        }
    }
}