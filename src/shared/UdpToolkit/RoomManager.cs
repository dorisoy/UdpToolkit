namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Sockets;
    using IpV4Address = UdpToolkit.Core.IpV4Address;

    public sealed class RoomManager : IRoomManager
    {
        private readonly IConnectionPool _connectionPool;
        private readonly ConcurrentDictionary<int, Room> _rooms = new ConcurrentDictionary<int, Room>();
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _roomTtl;
        private readonly Timer _houseKeeper;
        private readonly IUdpToolkitLogger _logger;

        private bool _disposed = false;

        public RoomManager(
            IDateTimeProvider dateTimeProvider,
            TimeSpan roomTtl,
            TimeSpan scanFrequency,
            IUdpToolkitLogger logger,
            IConnectionPool connectionPool)
        {
            _dateTimeProvider = dateTimeProvider;
            _roomTtl = roomTtl;
            _logger = logger;
            _connectionPool = connectionPool;
            _houseKeeper = new Timer(
                callback: ScanForCleaningInactiveConnections,
                state: null,
                dueTime: TimeSpan.FromSeconds(10),
                period: scanFrequency);
        }

        ~RoomManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void JoinOrCreate(
            int roomId,
            Guid connectionId)
        {
            if (!_connectionPool.TryGetConnection(connectionId, out var connection))
            {
                return;
            }

            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new Room(
                    id: id,
                    roomConnections: new List<RoomConnection>
                    {
                        new RoomConnection(
                            connectionId: connectionId,
                            ipV4Address: new IpV4Address(
                                host: connection.IpAddress.Address.ToHost(),
                                port: connection.IpAddress.Port)),
                    },
                    createdAt: _dateTimeProvider.UtcNow()),
                updateValueFactory: (id, room) =>
                {
                    if (room.RoomConnections.All(x => x.ConnectionId != connectionId))
                    {
                        room.RoomConnections.Add(
                            item: new RoomConnection(
                                connectionId: connectionId,
                                ipV4Address: new IpV4Address(
                                    host: connection.IpAddress.Address.ToHost(),
                                    port: connection.IpAddress.Port)));
                    }

                    return room;
                });
        }

        public Room GetRoom(int roomId)
        {
            return _rooms[roomId];
        }

        public void Leave(
            int roomId,
            Guid connectionId)
        {
            var room = _rooms[roomId];
            var index = room.RoomConnections.FindIndex(x => x.ConnectionId == connectionId);
            if (index >= 0)
            {
                room.RoomConnections.RemoveAt(index);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _houseKeeper?.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }

        private void ScanForCleaningInactiveConnections(object state)
        {
            _logger.Debug($"Cleanup inactive rooms");
            var now = _dateTimeProvider.UtcNow();
            for (var i = 0; i < _rooms.Count; i++)
            {
                var room = _rooms.ElementAt(i);

                var ttlDiff = now - room.Value.CreatedAt;
                if (ttlDiff > _roomTtl)
                {
                    _rooms.TryRemove(room.Key, out _);
                }
            }
        }
    }
}