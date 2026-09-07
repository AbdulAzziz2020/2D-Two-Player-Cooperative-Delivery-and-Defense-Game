using Unity.Collections;

namespace Game
{
    public readonly struct Message
    {
        public readonly FixedString64Bytes code;
        public readonly FixedString64Bytes message;

        public static implicit operator string(Message message) => message.code.ToString();
        
        public Message(string code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}