namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Peers;

    public class RoomManager : IRoomManager, IRawRoomManager
    {
        private readonly ConcurrentDictionary<int, IRawRoom> _rooms = new ConcurrentDictionary<int, IRawRoom>();
        private readonly IPeerManager _peerManager;

        public RoomManager(IPeerManager peerManager)
        {
            _peerManager = peerManager;
        }

        public void JoinOrCreate(int roomId, Guid peerId)
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
                    var room = new Room();
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

        public void Remove(
            int roomId,
            IRawPeer peer)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                return;
            }

            room.RemovePeer(peer.PeerId);
        }

        public Task Apply(
            int roomId,
            Guid caller,
            Func<IRawPeer, bool> condition,
            Func<IRawPeer, Task> func)
        {
            JoinOrCreate(roomId, caller);

            return _rooms[roomId]
                .Apply(
                    condition: condition,
                    func: func);
        }

        public void Leave(int roomId, Guid peerId)
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