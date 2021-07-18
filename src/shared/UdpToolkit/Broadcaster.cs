namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;

    public sealed class Broadcaster : IBroadcaster
    {
        private readonly IQueueDispatcher<OutPacket> _hostOutQueueDispatcher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRoomManager _roomManager;
        private readonly IConnectionPool _connectionPool;
        private readonly IUdpToolkitLogger _logger;

        private bool _disposed = false;

        public Broadcaster(
            IDateTimeProvider dateTimeProvider,
            IRoomManager roomManager,
            IConnectionPool connectionPool,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher,
            IUdpToolkitLogger logger)
        {
            _dateTimeProvider = dateTimeProvider;
            _roomManager = roomManager;
            _connectionPool = connectionPool;
            _hostOutQueueDispatcher = hostOutQueueDispatcher;
            _logger = logger;
        }

        ~Broadcaster()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Broadcast(
            Func<byte[]> serializer,
            Guid caller,
            int roomId,
            byte hookId,
            PacketType packetType,
            ChannelType channelType,
            BroadcastMode broadcastMode)
        {
            var utcNow = _dateTimeProvider.UtcNow();

            switch (broadcastMode)
            {
                case BroadcastMode.Caller:

                    if (!_connectionPool.TryGetConnection(caller, out var connection))
                    {
                        return;
                    }

                    _hostOutQueueDispatcher
                        .Dispatch(caller)
                        .Produce(new OutPacket(
                            hookId: hookId,
                            channelType: channelType,
                            packetType: packetType,
                            connectionId: caller,
                            serializer: serializer,
                            createdAt: utcNow,
                            ipAddress: connection.IpAddress));
                    return;
                case BroadcastMode.AllConnections:
                    _connectionPool.Apply(
                        action: (connection) =>
                        {
                            _hostOutQueueDispatcher
                                .Dispatch(connection.ConnectionId)
                                .Produce(new OutPacket(
                                    hookId: hookId,
                                    channelType: channelType,
                                    packetType: packetType,
                                    connectionId: caller,
                                    serializer: serializer,
                                    createdAt: utcNow,
                                    ipAddress: connection.IpAddress));
                        });

                    return;
                case BroadcastMode.Server:
                    return;
            }

            var room = _roomManager.GetRoom(roomId);
            switch (broadcastMode)
            {
                case BroadcastMode.RoomExceptCaller:
                    for (var i = 0; i < room.Count; i++)
                    {
                        if (!_connectionPool.TryGetConnection(room[i], out var c) || c.ConnectionId == caller)
                        {
                            continue;
                        }

                        _hostOutQueueDispatcher
                            .Dispatch(c.ConnectionId)
                            .Produce(new OutPacket(
                                hookId: hookId,
                                channelType: channelType,
                                packetType: packetType,
                                connectionId: room[i],
                                serializer: serializer,
                                createdAt: utcNow,
                                ipAddress: c.IpAddress));
                    }

                    return;
                case BroadcastMode.Room:
                    for (var i = 0; i < room.Count; i++)
                    {
                        if (!_connectionPool.TryGetConnection(room[i], out var c))
                        {
                            continue;
                        }

                        _hostOutQueueDispatcher
                            .Dispatch(c.ConnectionId)
                            .Produce(new OutPacket(
                                hookId: hookId,
                                channelType: channelType,
                                packetType: packetType,
                                connectionId: room[i],
                                serializer: serializer,
                                createdAt: utcNow,
                                ipAddress: c.IpAddress));
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastMode), broadcastMode, null);
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
                _roomManager.Dispose();
                _connectionPool.Dispose();
                _hostOutQueueDispatcher.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}