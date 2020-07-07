namespace P2PChat.Contracts
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class FetchPeers
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public FetchPeers()
        {
        }

        public FetchPeers(byte roomId)
        {
            RoomId = roomId;
        }

        [Key(x: 0)]
        public byte RoomId { get; set; }
    }
}