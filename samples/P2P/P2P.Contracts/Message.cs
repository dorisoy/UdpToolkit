namespace P2P.Contracts
{
    using MessagePack;

    [MessagePackObject]
    public class Message
    {
        public Message(
            string text,
            int roomId)
        {
            Text = text;
            RoomId = roomId;
        }

        [Key(0)]
        public string Text { get; }

        [Key(1)]
        public int RoomId { get; }
    }
}