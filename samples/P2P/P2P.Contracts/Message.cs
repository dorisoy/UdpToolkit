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
            Guid groupId)
        {
            Text = text;
            GroupId = groupId;
        }

        [Key(0)]
        public string Text { get; }

        [Key(1)]
        public Guid GroupId { get; }
    }
}