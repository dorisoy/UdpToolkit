namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Contracts.Packets;

    public sealed class Broadcaster : IBroadcaster
    {
        private readonly IQueueDispatcher<OutPacket> _hostOutQueueDispatcher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRoomManager _roomManager;
        private readonly IUdpToolkitLogger _logger;

        private bool _disposed = false;

        public Broadcaster(
            IDateTimeProvider dateTimeProvider,
            IRoomManager roomManager,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher,
            IUdpToolkitLogger logger)
        {
            _dateTimeProvider = dateTimeProvider;
            _roomManager = roomManager;
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
            byte channelId,
            BroadcastMode broadcastMode)
        {
            var utcNow = _dateTimeProvider.UtcNow();
            var room = _roomManager.GetRoom(roomId);

            switch (broadcastMode)
            {
                case BroadcastMode.Caller:

                    for (int i = 0; i < room.RoomConnections.Count; i++)
                    {
                        var roomConnection = room.RoomConnections[i];
                        if (roomConnection.ConnectionId != caller)
                        {
                            continue;
                        }

                        _hostOutQueueDispatcher
                            .Dispatch(caller)
                            .Produce(new OutPacket(
                                hookId: hookId,
                                channelId: channelId,
                                packetType: packetType,
                                connectionId: caller,
                                serializer: serializer,
                                createdAt: utcNow,
                                destination: roomConnection.IpV4Address));
                    }

                    return;
                case BroadcastMode.Server:
                    return;

                case BroadcastMode.RoomExceptCaller:
                    for (var i = 0; i < room.RoomConnections.Count; i++)
                    {
                        var roomConnection = room.RoomConnections[i];
                        if (roomConnection.ConnectionId == caller)
                        {
                            continue;
                        }

                        _hostOutQueueDispatcher
                            .Dispatch(roomConnection.ConnectionId)
                            .Produce(new OutPacket(
                                hookId: hookId,
                                channelId: channelId,
                                packetType: packetType,
                                connectionId: roomConnection.ConnectionId,
                                serializer: serializer,
                                createdAt: utcNow,
                                destination: roomConnection.IpV4Address));
                    }

                    return;
                case BroadcastMode.Room:
                    for (var i = 0; i < room.RoomConnections.Count; i++)
                    {
                        var roomConnection = room.RoomConnections[i];

                        _hostOutQueueDispatcher
                            .Dispatch(roomConnection.ConnectionId)
                            .Produce(new OutPacket(
                                hookId: hookId,
                                channelId: channelId,
                                packetType: packetType,
                                connectionId: roomConnection.ConnectionId,
                                serializer: serializer,
                                createdAt: utcNow,
                                destination: roomConnection.IpV4Address));
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
                _hostOutQueueDispatcher.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}