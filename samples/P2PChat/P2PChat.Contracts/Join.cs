namespace P2PChat.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class Join
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public Join()
        {
        }

        public Join(byte roomId)
        {
            RoomId = roomId;
        }

        [Key(0)]
        public byte RoomId { get; set; }
    }
}