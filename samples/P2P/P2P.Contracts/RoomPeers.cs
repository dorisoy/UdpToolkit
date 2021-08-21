namespace P2P.Contracts
{
    using System.Collections.Generic;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class RoomPeers
    {
        public RoomPeers(
            int roomId,
            List<Peer> peers)
        {
            RoomId = roomId;
            Peers = peers;
        }

        [Key(0)]
        public int RoomId { get; }

        [Key(1)]
        public List<Peer> Peers { get; }
    }
}