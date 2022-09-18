namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using UdpToolkit.Framework.CodeGenerator.Contracts;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Serialization;

    /// <inheritdoc />
    public sealed class Broadcaster : IBroadcaster
    {
        private readonly ISerializer _serializer;
        private readonly IScheduler _scheduler;
        private readonly IHostWorker _hostWorker;
        private readonly IConnectionPool _connectionPool;
        private readonly IGroupManager _groupManager;
        private readonly IQueueDispatcher<IOutNetworkPacket> _outQueueDispatcher;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        /// <param name="groupManager">Instance of group manager.</param>
        /// <param name="outQueueDispatcher">Instance of queue dispatcher.</param>
        /// <param name="connectionPool">Instance of connection pool.</param>
        /// <param name="hostWorker">Instance of host worker.</param>
        /// <param name="scheduler">Instance of scheduler.</param>
        /// <param name="serializer">Instance of serializer.</param>
        public Broadcaster(
            IGroupManager groupManager,
            IQueueDispatcher<IOutNetworkPacket> outQueueDispatcher,
            IConnectionPool connectionPool,
            IHostWorker hostWorker,
            IScheduler scheduler,
            ISerializer serializer)
        {
            _groupManager = groupManager;
            _outQueueDispatcher = outQueueDispatcher;
            _connectionPool = connectionPool;
            _hostWorker = hostWorker;
            _scheduler = scheduler;
            _serializer = serializer;
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
            using (@event)
            {
                if (!_hostWorker.TryGetSubscriptionId(typeof(TEvent), out var subscriptionId))
                {
                    return;
                }

                var group = _groupManager.GetGroup(groupId);
                var bufferWriter = ObjectsPool<BufferWriter<byte>>.GetOrCreate();
                _serializer.Serialize(bufferWriter, @event);

                BroadcastInternal(
                    broadcastMode: broadcastMode,
                    @group: group,
                    caller: caller,
                    groupId: groupId,
                    subscriptionId: subscriptionId,
                    channelId: channelId,
                    bufferWriter: bufferWriter);
            }
        }

        /// <inheritdoc />
        public void BroadcastUnmanaged<TEvent>(
            Guid caller,
            Guid groupId,
            TEvent @event,
            byte channelId,
            BroadcastMode broadcastMode)
        where TEvent : unmanaged
        {
            if (!_hostWorker.TryGetSubscriptionId(typeof(TEvent), out var subscriptionId))
            {
                return;
            }

            var group = _groupManager.GetGroup(groupId);
            var bufferWriter = ObjectsPool<BufferWriter<byte>>.GetOrCreate();
            _serializer.SerializeUnmanaged(bufferWriter, @event);

            BroadcastInternal(
                broadcastMode: broadcastMode,
                @group: group,
                caller: caller,
                groupId: groupId,
                subscriptionId: subscriptionId,
                channelId: channelId,
                bufferWriter: bufferWriter);
        }

        /// <inheritdoc />
        public void ScheduleBroadcast<TEvent>(
            Guid caller,
            Guid groupId,
            TimerKey timerKey,
            Func<TEvent> factory,
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
                        @event: factory(),
                        channelId: channelId,
                        broadcastMode: broadcastMode);
                });
        }

        /// <inheritdoc />
        public void ScheduleBroadcastUnmanaged<TEvent>(
            Guid caller,
            Guid groupId,
            TimerKey timerKey,
            Func<TEvent> factory,
            byte channelId,
            TimeSpan delay,
            BroadcastMode broadcastMode,
            TimeSpan frequency)
        where TEvent : unmanaged
        {
            _scheduler.Schedule(
                timerKey: timerKey,
                delay: delay,
                frequency: frequency,
                ttl: this._groupManager.GroupTtl,
                action: () =>
                {
                    this.BroadcastUnmanaged(
                        caller: caller,
                        groupId: groupId,
                        @event: factory(),
                        channelId: channelId,
                        broadcastMode: broadcastMode);
                });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BroadcastInternal(
            BroadcastMode broadcastMode,
            Group group,
            Guid caller,
            Guid groupId,
            byte subscriptionId,
            byte channelId,
            BufferWriter<byte> bufferWriter)
        {
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

                        PublishOutPacket(
                            connectionId: groupConnection.ConnectionId,
                            groupId: groupId,
                            ipV4Address: groupConnection.IpV4Address,
                            subscriptionId: subscriptionId,
                            channelId: channelId,
                            bufferWriter: bufferWriter);
                    }

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

                        PublishOutPacket(
                            connectionId: groupConnection.ConnectionId,
                            groupId: groupId,
                            ipV4Address: groupConnection.IpV4Address,
                            subscriptionId: subscriptionId,
                            channelId: channelId,
                            bufferWriter: bufferWriter);
                    }

                    break;
                }

                case BroadcastMode.Group:
                {
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];

                        PublishOutPacket(
                            connectionId: groupConnection.ConnectionId,
                            groupId: groupId,
                            ipV4Address: groupConnection.IpV4Address,
                            subscriptionId: subscriptionId,
                            channelId: channelId,
                            bufferWriter: bufferWriter);
                    }

                    break;
                }

                case BroadcastMode.Server:
                {
                    var connections = _connectionPool.GetAll();
                    for (int i = 0; i < connections.Count; i++)
                    {
                        var connection = connections[i];

                        PublishOutPacket(
                            connectionId: connection.ConnectionId,
                            groupId: groupId,
                            ipV4Address: connection.IpV4Address,
                            subscriptionId: subscriptionId,
                            channelId: channelId,
                            bufferWriter: bufferWriter);
                    }

                    break;
                }

                case BroadcastMode.None:
                {
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastMode), broadcastMode, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PublishOutPacket(
            Guid connectionId,
            Guid groupId,
            IpV4Address ipV4Address,
            byte subscriptionId,
            byte channelId,
            BufferWriter<byte> bufferWriter)
        {
            bufferWriter.AddReference();

            var outPacket = ObjectsPool<HostOutNetworkPacket>.GetOrCreate();

            outPacket.Setup(
                bufferWriter: bufferWriter,
                channelId: channelId,
                dataType: subscriptionId,
                connectionId: connectionId,
                ipV4Address: ipV4Address);

            _outQueueDispatcher.Dispatch(groupId).Produce(outPacket);
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