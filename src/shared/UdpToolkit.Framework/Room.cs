namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;

    public class Room : IRoom
    {
        private readonly ConcurrentDictionary<Guid, Peer> _roomPeers = new ConcurrentDictionary<Guid, Peer>();
        private readonly IPeerManager _peerManager;

        public Room(
            ushort roomId,
            IPeerManager peerManager)
        {
            RoomId = roomId;
            _peerManager = peerManager;
        }

        public ushort RoomId { get; }

        public int Size => _roomPeers.Count;

        public void AddPeer(Guid peerId)
        {
            var peer = _peerManager.Get(peerId: peerId);
            _roomPeers[peerId] = peer;
        }

        public void RemovePeer(Guid peerId)
        {
            _roomPeers.Remove(peerId, out _);
        }

        public IPeer GetPeer(Guid peerId)
        {
            return _roomPeers[peerId];
        }

        public IEnumerable<IPeer> GetPeers()
        {
            return _roomPeers.Select(x => x.Value);
        }
    }
}