namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;

    public class RoomManager : IRoomManager, IRawRoomManager
    {
        private readonly ConcurrentDictionary<int, IRawRoom> _rooms = new ConcurrentDictionary<int, IRawRoom>();
        private readonly IConnectionPool _connectionPool;

        public RoomManager(
            IConnectionPool connectionPool)
        {
            _connectionPool = connectionPool;
        }

        public void JoinOrCreate(int roomId, Guid peerId)
        {
            var connection = _connectionPool.TryGetConnection(peerId);
            if (connection == null)
            {
                return;
            }

            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) =>
                {
                    var room = new Room();
                    room.AddPeer(connection.ConnectionId);

                    return room;
                },
                updateValueFactory: (id, room) =>
                {
                    room.AddPeer(connection.ConnectionId);
                    return room;
                });
        }

        public IEnumerable<Guid> GetRoomPeers(int roomId)
        {
            return _rooms[roomId].GetPeers();
        }

        public void Remove(
            int roomId,
            IConnection connection)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                return;
            }

            room.RemovePeer(connection.ConnectionId);
        }

        public Task Apply(
            int roomId,
            Guid caller,
            Func<Guid, bool> condition,
            Func<Guid, Task> func)
        {
            JoinOrCreate(roomId, caller);

            return _rooms[roomId]
                .Apply(
                    condition: condition,
                    func: func);
        }

        public void Leave(int roomId, Guid peerId)
        {
            var connection = _connectionPool.TryGetConnection(peerId);
            if (connection == null)
            {
                return;
            }

            _rooms[roomId].RemovePeer(peerId: peerId);
        }
    }
}