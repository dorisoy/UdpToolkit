namespace UdpToolkit.Framework.Server.Peers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Network.Peers;

    public sealed class Room : IRoom
    {
        private readonly ConcurrentDictionary<Guid, Peer> _roomPeers = new ConcurrentDictionary<Guid, Peer>();

        public Room(ushort roomId)
        {
            RoomId = roomId;
        }

        public ushort RoomId { get; }

        public int Size => _roomPeers.Count;

        public void AddPeer(Peer peer)
        {
            _roomPeers[peer.PeerId] = peer;
        }

        public void RemovePeer(Guid peerId)
        {
            _roomPeers.Remove(peerId, out _);
        }

        public Peer GetPeer(Guid peerId)
        {
            return _roomPeers[peerId];
        }

        public IEnumerable<Peer> GetPeers()
        {
            return _roomPeers.Select(x => x.Value);
        }
    }
}
