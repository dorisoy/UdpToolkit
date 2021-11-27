namespace P2P.Contracts
{
    using System;
    using System.Collections.Generic;
    using MessagePack;
    using UdpToolkit.Annotations;

    [UdpEvent]
    [MessagePackObject]
    public sealed class GroupPeers : IDisposable
    {
        [Obsolete("Serialization only")]
        public GroupPeers()
        {
        }

        public GroupPeers(
            Guid groupId,
            List<Peer> peers)
        {
            GroupId = groupId;
            Peers = peers;
        }

        [Key(0)]
        public Guid GroupId { get; }

        [Key(1)]
        public List<Peer> Peers { get; }

        public void Dispose()
        {
            // nothing to do
        }
    }
}