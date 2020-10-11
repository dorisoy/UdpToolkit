namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Core;

    public sealed class Room : IRoom, IRawRoom
    {
        private readonly ConcurrentDictionary<Guid, Peer> _roomPeers = new ConcurrentDictionary<Guid, Peer>();

        public Room(
            ushort roomId)
        {
            RoomId = roomId;
        }

        public ushort RoomId { get; }

        public void AddPeer(IPeer peer)
        {
            _roomPeers[peer.PeerId] = peer as Peer;
        }

        public void RemovePeer(Guid peerId)
        {
            _roomPeers.Remove(peerId, out _);
        }

        public void Apply(
            Func<Peer, bool> condition,
            Action<Peer> action)
        {
            for (var i = 0; i < _roomPeers.Count; i++)
            {
                var pair = _roomPeers.ElementAt(i);
                var peer = pair.Value;
                if (!condition(peer))
                {
                    continue;
                }

                action(pair.Value);
            }
        }
    }
}