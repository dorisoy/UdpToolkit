namespace UdpToolkit.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using UdpToolkit.Framework.Contracts;

    /// <inheritdoc />
    public sealed class Broadcaster : IBroadcaster
    {
        private readonly IGroupManager _groupManager;
        private readonly IQueueDispatcher<OutPacket> _outQueueDispatcher;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Broadcaster"/> class.
        /// </summary>
        /// <param name="groupManager">Instance of group manager.</param>
        /// <param name="outQueueDispatcher">Instance of queue dispatcher.</param>
        public Broadcaster(
            IGroupManager groupManager,
            IQueueDispatcher<OutPacket> outQueueDispatcher)
        {
            _groupManager = groupManager;
            _outQueueDispatcher = outQueueDispatcher;
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
        {
            var group = _groupManager.GetGroup(groupId);
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

                        _outQueueDispatcher
                            .Dispatch(caller)
                            .Produce(new OutPacket(
                                connectionId: groupConnection.ConnectionId,
                                channelId: channelId,
                                @event: @event,
                                ipV4Address: groupConnection.IpV4Address));
                    }

                    return;

                case BroadcastMode.GroupExceptCaller:
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];
                        if (groupConnection.ConnectionId == caller)
                        {
                            continue;
                        }

                        _outQueueDispatcher
                            .Dispatch(groupConnection.ConnectionId)
                            .Produce(new OutPacket(
                                connectionId: groupConnection.ConnectionId,
                                channelId: channelId,
                                @event: @event,
                                ipV4Address: groupConnection.IpV4Address));
                    }

                    return;
                case BroadcastMode.Group:
                    for (var i = 0; i < group.GroupConnections.Count; i++)
                    {
                        var groupConnection = group.GroupConnections[i];

                        _outQueueDispatcher
                            .Dispatch(groupConnection.ConnectionId)
                            .Produce(new OutPacket(
                                connectionId: groupConnection.ConnectionId,
                                channelId: channelId,
                                @event: @event,
                                ipV4Address: groupConnection.IpV4Address));
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
                _groupManager.Dispose();
            }

            _disposed = true;
        }
    }
}