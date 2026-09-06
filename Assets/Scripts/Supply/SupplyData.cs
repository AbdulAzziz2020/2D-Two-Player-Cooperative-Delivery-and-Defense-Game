using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct SupplyData : INetworkSerializable, IEquatable<SupplyData>
    {
        [SerializeField] private FixedString64Bytes supplyId;
        [SerializeField] private int quantity;
        
        public FixedString64Bytes SupplyId => supplyId;
        public  int Quantity => quantity;

        public static implicit operator string(SupplyData data) => data.SupplyId.ToString();
        public static implicit operator int(SupplyData data) => data.Quantity;
        public static implicit operator FixedString64Bytes(SupplyData data) => data.SupplyId;
        
        public bool IsValid => supplyId.Length > 0;

        public SupplyData(string id, int amount)
        {
            supplyId = new FixedString64Bytes(id);
            quantity = amount;
        }

        public void NetworkSerialize<T>( BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref supplyId);
        }

        public bool Equals(SupplyData other)
        {
            return supplyId.Equals(other.supplyId);
        }

        public override bool Equals(object obj)
        {
            return obj is SupplyData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return supplyId.GetHashCode();
        }

        public override string ToString()
        {
            return supplyId.ToString();
        }

        public static SupplyData Empty => default;
    }
}