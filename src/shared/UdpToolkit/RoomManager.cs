namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Network;

    public sealed class RoomManager : IRoomManager
    {
        private readonly ConcurrentDictionary<int, List<Guid>> _rooms = new ConcurrentDictionary<int, List<Guid>>();
        private readonly IConnectionPool _connectionPool;

        public RoomManager(
            IConnectionPool connectionPool)
        {
            _connectionPool = connectionPool;
        }

        public void JoinOrCreate(
            int roomId,
            Guid peerId)
        {
            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new List<Guid>
                {
                    peerId,
                },
                updateValueFactory: (id, room) =>
                {
                    room.Add(peerId);
                    return room;
                });
        }

        public List<Guid> GetRoomPeers(int roomId)
        {
            return _rooms[roomId];
        }

        public void Leave(
            int roomId,
            Guid peerId)
        {
            _rooms[roomId].Remove(peerId);
        }

        public Task ApplyAsync(
            int roomId,
            Func<IConnection, bool> condition,
            Func<IConnection, Task> func)
        {
            var room = _rooms[roomId];
            for (var i = 0; i < room.Count; i++)
            {
                var connectionId = room[i];
                var connection = _connectionPool.TryGetConnection(connectionId);
                if (condition(connection))
                {
                    Task.Run(() => func(connection));
                }
            }

            return Task.CompletedTask;
        }
    }
}