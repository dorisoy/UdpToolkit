namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Framework.Contracts;

    /// <inheritdoc />
    public sealed class Broadcaster : IBroadcaster
    {
        private readonly IRoomManager _roomManager;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        /// <param name="roomManager">Instance of room manager.</param>
        /// <param name="outQueueDispatcher">Instance of queue dispatcher.</param>
        public Broadcaster(
            IRoomManager roomManager,
            IQueueDispatcher<OutPacket> outQueueDispatcher)
        {
            _roomManager = roomManager;
            _outQueueDispatcher = outQueueDispatcher;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        ~Broadcaster()
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
        public void Broadcast<TEvent>(
            Guid caller,
            Guid roomId,
            TEvent @event,
            byte channelId,
            BroadcastMode broadcastMode)
        {
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

                        _outQueueDispatcher
                            .Dispatch(caller)
                            .Produce(new OutPacket(
                                connectionId: roomConnection.ConnectionId,
                                channelId: channelId,
                                @event: @event,
                                ipV4Address: roomConnection.IpV4Address));
                    }

                    return;

                case BroadcastMode.RoomExceptCaller:
                    for (var i = 0; i < room.RoomConnections.Count; i++)
                    {
                        var roomConnection = room.RoomConnections[i];
                        if (roomConnection.ConnectionId == caller)
                        {
                            continue;
                        }

                        _outQueueDispatcher
                            .Dispatch(roomConnection.ConnectionId)
                            .Produce(new OutPacket(
                                connectionId: roomConnection.ConnectionId,
                                channelId: channelId,
                                @event: @event,
                                ipV4Address: roomConnection.IpV4Address));
                    }

                    return;
                case BroadcastMode.Room:
                    for (var i = 0; i < room.RoomConnections.Count; i++)
                    {
                        var roomConnection = room.RoomConnections[i];

                        _outQueueDispatcher
                            .Dispatch(roomConnection.ConnectionId)
                            .Produce(new OutPacket(
                                connectionId: roomConnection.ConnectionId,
                                channelId: channelId,
                                @event: @event,
                                ipV4Address: roomConnection.IpV4Address));
                    }

                    return;

                case BroadcastMode.None:
                case BroadcastMode.Server:
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
                _outQueueDispatcher.Dispose();
                _roomManager.Dispose();
            }

            _disposed = true;
        }
    }
}