namespace Game
{
    public readonly struct Message
    {
        public readonly string code;
        public readonly string message;

        public static implicit operator string(Message message) => message.code;
        
        public Message(string code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}