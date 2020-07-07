namespace P2PChat.Contracts
{
    using System;
    using System.Collections.Generic;
    using MessagePack;

    [MessagePackObject]
    public class FetchedPeers
    {
        [Obsolete(message: "For deserialization only", error: true)]
        public FetchedPeers()
        {
        }

        public FetchedPeers(IEnumerable<ClientPeer> peers)
        {
            Peers = peers;
        }

        [Key(x: 0)]
        public IEnumerable<ClientPeer> Peers { get; set; }
    }
}