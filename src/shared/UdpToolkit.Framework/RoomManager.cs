namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using UdpToolkit.Core;

    public class RoomManager : IRoomManager
    {
        private readonly ConcurrentDictionary<ushort, IRoom> _rooms = new ConcurrentDictionary<ushort, IRoom>();
        private readonly IPeerManager _peerManager;

        public RoomManager(IPeerManager peerManager)
        {
            _peerManager = peerManager;
        }

        public void JoinOrCreate(ushort roomId, Guid peerId)
        {
            var exists = _peerManager.TryGetPeer(peerId, out var peer);
            if (!exists)
            {
                return;
            }

            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) =>
                {
                    var room = new Room(roomId, _peerManager);
                    room.AddPeer(peer.PeerId);

                    return room;
                },
                updateValueFactory: (id, room) =>
                {
                    room.AddPeer(peer.PeerId);
                    return room;
                });
        }

        public void JoinOrCreate(ushort roomId, Guid peerId, int limit)
        {
            var exists = _peerManager.TryGetPeer(peerId, out var peer);
            if (!exists)
            {
                return;
            }

            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new Room(roomId, _peerManager),
                updateValueFactory: (id, room) =>
                {
                    if (room.Size < limit)
                    {
                        room.AddPeer(peer.PeerId);
                        return room;
                    }

                    return room;
                });
        }

        public IRoom GetRoom(ushort roomId)
        {
            return _rooms[roomId];
        }

        public void Leave(ushort roomId, Guid peerId)
        {
            var exists = _peerManager.TryGetPeer(peerId, out var peer);
            if (!exists)
            {
                return;
            }

            _rooms[roomId].RemovePeer(peerId: peer.PeerId);
        }
    }
}