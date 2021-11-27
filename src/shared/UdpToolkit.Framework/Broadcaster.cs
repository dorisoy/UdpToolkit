namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Network.Contracts.Pooling;

    /// <inheritdoc />
    public sealed class Broadcaster : IBroadcaster
    {
        private readonly IGroupManager _groupManager;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;
        private readonly ConcurrentPool<OutPacket> _pool;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        /// <param name="groupManager">Instance of group manager.</param>
        /// <param name="outQueueDispatcher">Instance of queue dispatcher.</param>
        /// <param name="pool">Instance pool.</param>
        public Broadcaster(
            IGroupManager groupManager,
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            ConcurrentPool<OutPacket> pool)
        {
            _groupManager = groupManager;
            _outQueueDispatcher = outQueueDispatcher;
            _pool = pool;
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
            var group = _groupManager.GetGroup(groupId);
            var queue = _outQueueDispatcher.Dispatch(groupId);
            var outPacket = _pool.GetOrCreate();
            outPacket.Setup(@event: @event, channelId: channelId);
            switch (broadcastMode)
            {
                case BroadcastMode.Caller:

                    for (int i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];
                        if (groupConnection.ConnectionId != caller)
                        {
                            continue;
                        }

                        outPacket.Connections.Add(groupConnection);
                    }

                    break;

                case BroadcastMode.GroupExceptCaller:
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];
                        if (groupConnection.ConnectionId == caller)
                        {
                            continue;
                        }

                        outPacket.Connections.Add(groupConnection);
                    }

                    break;

                case BroadcastMode.Group:
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];

                        outPacket.Connections.Add(groupConnection);
                    }

                    break;

                case BroadcastMode.None:
                case BroadcastMode.Server:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastMode), broadcastMode, null);
            }

            queue.Produce(outPacket);
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