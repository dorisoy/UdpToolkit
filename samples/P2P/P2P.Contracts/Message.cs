namespace P2P.Contracts
{
    using MessagePack;

    [MessagePackObject]
    public class Message
    {
        public Message(
            string text)
        {
            Text = text;
        }

        [Key(0)]
        public string Text { get; }
    }
}