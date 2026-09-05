using System;
using Unity.Netcode;

namespace Game
{
    public struct PauseRequest : INetworkSerializable, IEquatable<PauseRequest>
    {
        public ulong senderId;
        public bool isPause;

        public PauseRequest(ulong senderId, bool isPause)
        {
            this.senderId = senderId;
            this.isPause = isPause;
        }

        public bool IsEmpty()
        {
            return senderId == ulong.MaxValue;
        }

        public bool IsSame(ulong clientId)
        {
            return !IsEmpty() && senderId == clientId;
        }

        public bool IsHost()
        {
            return !IsEmpty() &&
                   senderId == NetworkManager.ServerClientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref senderId);
            serializer.SerializeValue(ref isPause);
        }

        public bool Equals(PauseRequest other)
        {
            return senderId == other.senderId && isPause == other.isPause;
        }

        public override bool Equals(object obj)
        {
            return obj is PauseRequest other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(senderId, isPause);
        }

        public static PauseRequest Empty => new(ulong.MaxValue, false);
        
        public static PauseRequest Pause(ulong senderId) => new(senderId, true);
        
        public static PauseRequest Resume(ulong senderId) => new(senderId, false);
    }
}