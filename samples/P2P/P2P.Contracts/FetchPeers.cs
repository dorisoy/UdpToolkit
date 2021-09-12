namespace P2P.Contracts
{
    using System;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class FetchPeers
    {
        public FetchPeers(
            Guid roomId,
            string nickname)
        {
            RoomId = roomId;
            Nickname = nickname;
        }

        [Key(0)]
        public Guid RoomId { get; }

        [Key(1)]
        public string Nickname { get; }
    }
}