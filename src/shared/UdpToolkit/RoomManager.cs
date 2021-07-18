namespace UdpToolkit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;

    public sealed class RoomManager : IRoomManager
    {
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
            IUdpToolkitLogger logger)
        {
            _dateTimeProvider = dateTimeProvider;
            _roomTtl = roomTtl;
            _logger = logger;
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
            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new Room(
                    id: id,
                    connections: new List<Guid>
                    {
                        connectionId,
                    },
                    createdAt: _dateTimeProvider.UtcNow()),
                updateValueFactory: (id, room) =>
                {
                    if (!room.Connections.Contains(connectionId))
                    {
                        room.Connections.Add(connectionId);
                    }

                    return room;
                });
        }

        public List<Guid> GetRoom(int roomId)
        {
            return _rooms[roomId].Connections;
        }

        public void Leave(
            int roomId,
            Guid connectionId)
        {
            _rooms[roomId].Connections.Remove(connectionId);
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

        private readonly struct Room
        {
            public Room(
                int id,
                List<Guid> connections,
                DateTimeOffset createdAt)
            {
                Connections = connections;
                CreatedAt = createdAt;
                Id = id;
            }

            public int Id { get; }

            public List<Guid> Connections { get; }

            public DateTimeOffset CreatedAt { get; }
        }
    }
}