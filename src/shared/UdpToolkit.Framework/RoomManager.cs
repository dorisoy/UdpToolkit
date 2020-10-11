namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using UdpToolkit.Core;

    public class RoomManager : IRoomManager, IRawRoomManager
    {
        private readonly ConcurrentDictionary<ushort, IRawRoom> _rooms = new ConcurrentDictionary<ushort, IRawRoom>();
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
                    var room = new Room(roomId);
                    room.AddPeer(peer);
                    peer.SetRoomId(roomId);

                    return room;
                },
                updateValueFactory: (id, room) =>
                {
                    peer.SetRoomId(roomId);
                    room.AddPeer(peer);
                    return room;
                });
        }

        public IRoom GetRoom(
            ushort roomId)
        {
            return _rooms[roomId];
        }

        public void Apply(
            ushort roomId,
            Func<Peer, bool> condition,
            Action<Peer> action)
        {
            _rooms[roomId]
                .Apply(
                    condition: condition,
                    action: action);
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