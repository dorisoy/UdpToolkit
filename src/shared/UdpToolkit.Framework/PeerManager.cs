namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class PeerManager : IPeerManager
    {
        private readonly ConcurrentDictionary<Guid, Peer> _peers = new ConcurrentDictionary<Guid, Peer>();

        public void Create(Guid peerId, List<IPEndPoint> peerIps)
        {
            _peers[peerId] = Peer.New(peerId: peerId, peerIps: peerIps);
        }

        public void Remove(Guid peerId)
        {
            _peers.TryRemove(peerId, out _);
        }

        public bool Exist(Guid peerId)
        {
            return _peers.ContainsKey(peerId);
        }

        public Peer Get(Guid peerId)
        {
            return _peers[peerId];
        }

        public IEnumerable<Peer> GetAll()
        {
            return _peers.Select(x => x.Value);
        }
    }
}