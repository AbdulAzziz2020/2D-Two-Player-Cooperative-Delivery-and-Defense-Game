using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct SupplyData : INetworkSerializable, IEquatable<SupplyData>
    {
        [SerializeField]
        private FixedString64Bytes supplyId;
        // network object id

        public FixedString64Bytes SupplyId => supplyId;

        public bool IsValid => supplyId.Length > 0;

        public SupplyData(string id)
        {
            supplyId = new FixedString64Bytes(id);
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