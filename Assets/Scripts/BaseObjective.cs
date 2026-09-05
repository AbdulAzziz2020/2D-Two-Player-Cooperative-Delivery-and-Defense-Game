using Unity.Netcode;
using UnityEngine;

namespace BumiDeliveryDefense
{
    public sealed class BaseObjective : NetworkBehaviour
    {
        public static BaseObjective Instance { get; private set; }

        [SerializeField] private Transform deliveryPoint;
        [SerializeField] private float deliveryRange = 1.5f;
        [SerializeField] private float repairRange = 2f;

        private void Awake()
        {
            Instance = this;

            if (deliveryPoint == null)
                deliveryPoint = transform;
        }

        public bool IsInDeliveryRange(Vector2 playerPosition)
        {
            return Vector2.Distance(playerPosition, deliveryPoint.position) <= deliveryRange;
        }

        public bool IsInRepairRange(Vector2 playerPosition)
        {
            return Vector2.Distance(playerPosition, transform.position) <= repairRange;
        }
    }
}
