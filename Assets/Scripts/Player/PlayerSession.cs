using System;
using Unity.Collections;
using Unity.Netcode;

namespace Game
{
    public struct PlayerSession : INetworkSerializable, IEquatable<PlayerSession>
    {
        public ulong clientId;
        public FixedString64Bytes id;
        public FixedString64Bytes name;

        public PlayerSession(ulong clientId, FixedString64Bytes id, FixedString64Bytes name)
        {
            this.clientId = clientId;
            this.id = id;
            this.name = name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref name);
        }

        public bool Equals(PlayerSession other)
        {
            return id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerSession other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}