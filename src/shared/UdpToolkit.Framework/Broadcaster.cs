namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Pooling;

    /// <inheritdoc />
    public sealed class Broadcaster : IBroadcaster
    {
        private readonly IScheduler _scheduler;
        private readonly IHostWorker _hostWorker;
        private readonly IConnectionPool _connectionPool;
        private readonly IGroupManager _groupManager;
        private readonly IQueueDispatcher<OutNetworkPacket> _outQueueDispatcher;
        private readonly ConcurrentPool<OutNetworkPacket> _pool;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        /// <param name="groupManager">Instance of group manager.</param>
        /// <param name="outQueueDispatcher">Instance of queue dispatcher.</param>
        /// <param name="pool">Instance pool.</param>
        /// <param name="connectionPool">Instance of connection pool.</param>
        /// <param name="hostWorker">Instance of host worker.</param>
        /// <param name="scheduler">Instance of scheduler.</param>
        public Broadcaster(
            IGroupManager groupManager,
            IQueueDispatcher<OutNetworkPacket> outQueueDispatcher,
            ConcurrentPool<OutNetworkPacket> pool,
            IConnectionPool connectionPool,
            IHostWorker hostWorker,
            IScheduler scheduler)
        {
            _groupManager = groupManager;
            _outQueueDispatcher = outQueueDispatcher;
            _pool = pool;
            _connectionPool = connectionPool;
            _hostWorker = hostWorker;
            _scheduler = scheduler;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
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
            Guid groupId,
            TEvent @event,
            byte channelId,
            BroadcastMode broadcastMode)
        where TEvent : class, IDisposable
        {
            if (!_hostWorker.TryGetSubscriptionId(typeof(TEvent), out var subscriptionId))
            {
                return;
            }

            var group = _groupManager.GetGroup(groupId);
            var queue = _outQueueDispatcher.Dispatch(groupId);
            var outPacket = _pool.GetOrCreate();

            outPacket.Setup(
                @event: @event,
                channelId: channelId,
                dataType: subscriptionId,
                connectionId: default,
                ipV4Address: default);

            switch (broadcastMode)
            {
                case BroadcastMode.Caller:
                {
                    for (int i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];
                        if (groupConnection.ConnectionId != caller)
                        {
                            continue;
                        }

                        outPacket.Connections.Add(groupConnection);
                    }

                    queue.Produce(outPacket);

                    break;
                }

                case BroadcastMode.GroupExceptCaller:
                {
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];
                        if (groupConnection.ConnectionId == caller)
                        {
                            continue;
                        }

                        outPacket.Connections.Add(groupConnection);
                    }

                    queue.Produce(outPacket);

                    break;
                }

                case BroadcastMode.Group:
                {
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];

                        outPacket.Connections.Add(groupConnection);
                    }

                    queue.Produce(outPacket);

                    break;
                }

                case BroadcastMode.Server:
                {
                    var connections = _connectionPool.GetAll();
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var connection = connections[i];

                        outPacket.Connections.Add(connection);
                    }

                    queue.Produce(outPacket);

                    break;
                }

                case BroadcastMode.None:
                {
                    @event.Dispose();
                    outPacket.Dispose();

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastMode), broadcastMode, null);
            }
        }

        /// <inheritdoc />
        public void ScheduleBroadcast<TEvent>(
            Guid caller,
            Guid groupId,
            TimerKey timerKey,
            TEvent @event,
            byte channelId,
            TimeSpan delay,
            BroadcastMode broadcastMode,
            TimeSpan frequency)
        where TEvent : class, IDisposable
        {
            _scheduler.Schedule(
                timerKey: timerKey,
                delay: delay,
                frequency: frequency,
                ttl: this._groupManager.GroupTtl,
                action: () =>
                {
                    this.Broadcast(
                        caller: caller,
                        groupId: groupId,
                        @event: @event,
                        channelId: channelId,
                        broadcastMode: broadcastMode);
                });
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
                _groupManager.Dispose();
            }

            _disposed = true;
        }
    }
}