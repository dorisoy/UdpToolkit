namespace P2P.Contracts
{
    using System;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class Message
    {
        public Message(
            string text,
            Guid roomId)
        {
            Text = text;
            RoomId = roomId;
        }

        [Key(0)]
        public string Text { get; }

        [Key(1)]
        public Guid RoomId { get; }
    }
}