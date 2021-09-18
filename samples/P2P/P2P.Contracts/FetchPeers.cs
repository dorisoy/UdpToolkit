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
            Guid groupId,
            string nickname)
        {
            GroupId = groupId;
            Nickname = nickname;
        }

        [Key(0)]
        public Guid GroupId { get; }

        [Key(1)]
        public string Nickname { get; }
    }
}