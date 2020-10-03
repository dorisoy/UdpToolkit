namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public class PeerManager : IPeerManager, IRawPeerManager
    {
        private readonly IAsyncQueue<NetworkPacket> _outputQueue;
        private readonly ConcurrentDictionary<Guid, Peer> _peers = new ConcurrentDictionary<Guid, Peer>();

        public PeerManager(
            IAsyncQueue<NetworkPacket> outputQueue)
        {
            _outputQueue = outputQueue;
        }

        public IPeer Create(Guid peerId, List<IPEndPoint> peerIps)
        {
            var peer = Peer.New(
                peerId: peerId,
                peerIps: peerIps,
                outputQueue: _outputQueue);

            _peers[peerId] = peer;

            return peer;
        }

        public void Remove(Guid peerId)
        {
            _peers.TryRemove(peerId, out _);
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

        public bool TryGetPeer(Guid peerId, out Peer peer)
        {
            return _peers.TryGetValue(peerId, out peer);
        }

        Peer IRawPeerManager.Create(Guid peerId, List<IPEndPoint> peerIps)
        {
            var peer = Peer.New(
                peerId: peerId,
                peerIps: peerIps,
                outputQueue: _outputQueue);

            _peers[peerId] = peer;

            return peer;
        }
    }
}