namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <inheritdoc />
    public sealed class RoomManager : IRoomManager
    {
        private readonly ConcurrentDictionary<Guid, Room> _rooms = new ConcurrentDictionary<Guid, Room>();
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _roomTtl;
        private readonly Timer _houseKeeper;
        private readonly ILogger _logger;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomManager"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        /// <param name="roomTtl">Room ttl.</param>
        /// <param name="scanFrequency">Scan frequency for cleanup inactive rooms.</param>
        /// <param name="logger">Instance of logger.</param>
        public RoomManager(
            IDateTimeProvider dateTimeProvider,
            TimeSpan roomTtl,
            TimeSpan scanFrequency,
            ILogger logger)
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

        /// <summary>
        /// Finalizes an instance of the <see cref="RoomManager"/> class.
        /// </summary>
        ~RoomManager()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void JoinOrCreate(
            Guid roomId,
            Guid connectionId,
            IpV4Address ipV4Address)
        {
            _rooms.AddOrUpdate(
                key: roomId,
                addValueFactory: (id) => new Room(
                    id: id,
                    roomConnections: new List<RoomConnection>
                    {
                        new RoomConnection(
                            connectionId: connectionId,
                            ipV4Address: ipV4Address),
                    },
                    createdAt: _dateTimeProvider.GetUtcNow()),
                updateValueFactory: (id, room) =>
                {
                    if (room.RoomConnections.All(x => x.ConnectionId != connectionId))
                    {
                        room.RoomConnections.Add(
                            item: new RoomConnection(
                                connectionId: connectionId,
                                ipV4Address: ipV4Address));
                    }

                    return room;
                });
        }

        /// <inheritdoc />
        public Room GetRoom(Guid roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
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

            _disposed = true;
        }

        private void ScanForCleaningInactiveConnections(object state)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Framework] Cleanup inactive rooms");
            }

            var now = _dateTimeProvider.GetUtcNow();
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