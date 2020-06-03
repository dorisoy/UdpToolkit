namespace UdpToolkit.Framework.Server.Peers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Utils;

    public class PeerManager : IPeerManager
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ConcurrentDictionary<IPEndPoint, Guid> _ips = new ConcurrentDictionary<IPEndPoint, Guid>();
        private readonly ConcurrentDictionary<Guid, Peer> _peers = new ConcurrentDictionary<Guid, Peer>();

        public PeerManager(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public void Add(Peer peer)
        {
            _peers[peer.PeerId] = peer;
            _ips[peer.IpEndPoint] = peer.PeerId;
        }

        public void Remove(Peer peer)
        {
            _peers.Remove(peer.PeerId, out _);
            _ips.Remove(peer.IpEndPoint, out _);
        }

        public Peer Get(Guid peerId)
        {
            return _peers[peerId];
        }

        public Peer GetOrAdd(IPEndPoint ipEndPoint)
        {
            if (_ips.ContainsKey(ipEndPoint))
            {
                var peerId = _ips[ipEndPoint];
                return _peers[peerId];
            }

            var now = _dateTimeProvider.UtcNow();

            var peer = new Peer(
                peerId: Guid.NewGuid(),
                ipEndPoint: ipEndPoint,
                reliableUdpChannel: new ReliableUdpChannel(),
                lastActivityAt: now,
                createdAt: now);

            _ips[ipEndPoint] = peer.PeerId;
            _peers[peer.PeerId] = peer;

            return peer;
        }

        public IEnumerable<Peer> GetAll()
        {
            return _peers.Select(x => x.Value);
        }
    }
}