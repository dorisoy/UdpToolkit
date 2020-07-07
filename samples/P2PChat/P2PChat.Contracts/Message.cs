namespace P2PChat.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class Message
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public Message()
        {
        }

        public Message(
            string from,
            string text)
        {
            From = from;
            Text = text;
        }

        [Key(0)]
        public string From { get; set; }

        [Key(1)]
        public string Text { get; set; }
    }
}