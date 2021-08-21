namespace P2P.Contracts
{
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public class FetchPeers
    {
        public FetchPeers(
            int roomId,
            string nickname)
        {
            RoomId = roomId;
            Nickname = nickname;
        }

        [Key(0)]
        public int RoomId { get; }

        [Key(1)]
        public string Nickname { get; }
    }
}