namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;

    public class PeerManager : IPeerManager, IRawPeerManager
    {
        private readonly ConcurrentDictionary<Guid, Peer> _peers = new ConcurrentDictionary<Guid, Peer>();

        public IPeer Create(Guid peerId, IPEndPoint peerIp)
        {
            var peer = Peer.New(
                peerId: peerId,
                peerIps: new List<IPEndPoint>
                {
                    peerIp,
                });

            _peers[peerId] = peer;

            return peer;
        }

        public IPeer Create(Guid peerId, List<IPEndPoint> peerIps)
        {
            var peer = Peer.New(
                peerId: peerId,
                peerIps: peerIps);

            _peers[peerId] = peer;

            return peer;
        }

        public bool TryRemove(Guid peerId, out IPeer peer)
        {
            peer = null;
            var removed = _peers.TryRemove(peerId, out var rawPeer);
            if (!removed)
            {
                return false;
            }

            peer = rawPeer;
            return true;
        }

        public Peer GetPeer(Guid peerId)
        {
            return _peers[peerId];
        }

        IPeer IPeerManager.AddOrUpdate(Guid peerId, List<IPEndPoint> ips)
        {
            return AddOrUpdate(peerId, ips);
        }

        public bool Exist(Guid peerId)
        {
            return _peers.ContainsKey(peerId);
        }

        public bool TryGetPeer(Guid peerId, out IPeer peer)
        {
            peer = null;
            var exists = _peers.TryGetValue(peerId, out var rawPeer);
            if (!exists)
            {
                return false;
            }

            peer = rawPeer;

            return true;
        }

        public IEnumerable<IPeer> GetAll()
        {
            return _peers.Select(x => x.Value);
        }

        public Peer AddOrUpdate(Guid peerId, List<IPEndPoint> ips)
        {
            return _peers.AddOrUpdate(
                key: peerId,
                addValueFactory: (key) => Peer.New(
                    peerId: peerId,
                    peerIps: ips),
                updateValueFactory: (key, peer) =>
                {
                    foreach (var ip in ips)
                    {
                        if (!peer.PeerIps.Contains(ip))
                        {
                            peer.PeerIps.Add(ip);
                        }
                    }

                    return peer;
                });
        }
    }
}