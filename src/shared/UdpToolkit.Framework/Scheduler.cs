namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;

    /// <summary>
    /// Scheduler, implementation for sending delayed packets.
    /// </summary>
    public sealed class Scheduler : IScheduler
    {
        private readonly IRoomManager _roomManager;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<TimerKey, Lazy<Timer>> _timers = new ConcurrentDictionary<TimerKey, Lazy<Timer>>();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="outQueueDispatcher">Queue dispatcher.</param>
        /// <param name="roomManager">Room manager.</param>
        public Scheduler(
            ILogger logger,
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            IRoomManager roomManager)
        {
            _logger = logger;
            _outQueueDispatcher = outQueueDispatcher;
            _roomManager = roomManager;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Scheduler"/> class.
        /// </summary>
        ~Scheduler()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Schedule<TEvent>(
            TEvent @event,
            Guid caller,
            byte channelId,
            int roomId,
            string eventName,
            TimeSpan dueTime,
            BroadcastMode broadcastMode)
        {
            if (dueTime == TimeSpan.Zero)
            {
                Broadcast(@event, caller, roomId, channelId, broadcastMode);
                return;
            }

            var lazyTimer = _timers.GetOrAdd(
                key: new TimerKey(
                    roomId: roomId,
                    timerId: eventName),
                valueFactory: (key) => new Lazy<Timer>(() => new Timer(
                    callback: (state) =>
                    {
                        Broadcast(@event, caller, roomId, channelId, broadcastMode);
                    },
                    state: null,
                    dueTime: dueTime,
                    period: TimeSpan.FromMilliseconds(Timeout.Infinite))));

            _ = lazyTimer.Value;
        }

        private void Broadcast<TEvent>(
            TEvent @event,
            Guid caller,
            int roomId,
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
                for (var i = 0; i < _timers.Count; i++)
                {
                    var timer = _timers.ElementAt(i).Value;
                    timer.Value.Dispose();
                }

                _roomManager.Dispose();
                _outQueueDispatcher.Dispose();
            }

            _disposed = true;
        }
    }
}
