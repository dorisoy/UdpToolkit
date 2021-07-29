namespace UdpToolkit.Jobs
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Serialization;

    public sealed class WorkerJob : IDisposable
    {
        private readonly IUdpToolkitLogger _logger;
        private readonly ISerializer _serializer;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IRoomManager _roomManager;
        private readonly IBroadcaster _broadcaster;
        private readonly IScheduler _scheduler;

        private bool _disposed = false;

        public WorkerJob(
            ISubscriptionManager subscriptionManager,
            IRoomManager roomManager,
            ISerializer serializer,
            IUdpToolkitLogger logger,
            IBroadcaster broadcaster,
            IScheduler scheduler)
        {
            _subscriptionManager = subscriptionManager;
            _serializer = serializer;
            _logger = logger;
            _broadcaster = broadcaster;
            _scheduler = scheduler;
            _roomManager = roomManager;
        }

        ~WorkerJob()
        {
            Dispose(false);
        }

        public static event Action<bool> OnConnectionChanged;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Execute(
            InPacket inPacket)
        {
            switch (inPacket.PacketType)
            {
                case PacketType.FromServer:
                case PacketType.FromClient:
                    HandleUserDefinedEvent(ref inPacket);
                    break;
                case PacketType.Protocol:
                    HandleProtocolEvent(ref inPacket);
                    break;
                case PacketType.Ack:
                    if (inPacket.IsProtocolEvent)
                    {
                        HandleProtocolAck(ref inPacket);
                    }
                    else
                    {
                        HandleUserDefinedAck(ref inPacket);
                    }

                    break;
            }
        }

        private void HandleProtocolEvent(
            ref InPacket inPacket)
        {
            var protocolHookId = (ProtocolHookId)inPacket.HookId;

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Heartbeat:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                case ProtocolHookId.Connect2Peer:

                    userDefinedSubscription?.OnProtocolEvent?.Invoke(
                        inPacket.Serializer(),
                        inPacket.ConnectionId,
                        _serializer);

                    break;
            }
        }

        private void HandleUserDefinedEvent(
            ref InPacket inPacket)
        {
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(inPacket.HookId);

            if (userDefinedSubscription == null)
            {
                _logger.Error($"Subscription with id {inPacket.HookId} not found! {nameof(HandleUserDefinedEvent)}");

                return;
            }

            var roomId = userDefinedSubscription.OnEvent(
                    inPacket.Serializer(),
                    inPacket.ConnectionId,
                    _serializer,
                    _roomManager,
                    _scheduler);

            if (userDefinedSubscription.BroadcastMode == BroadcastMode.None || userDefinedSubscription.BroadcastMode == BroadcastMode.Caller)
            {
                return;
            }

            if (inPacket.PacketType == PacketType.FromClient)
            {
                _broadcaster.Broadcast(
                    packetType: PacketType.FromServer,
                    serializer: inPacket.Serializer,
                    caller: inPacket.ConnectionId,
                    roomId: roomId,
                    hookId: inPacket.HookId,
                    channelType: inPacket.ChannelType,
                    broadcastMode: userDefinedSubscription.BroadcastMode);
            }
        }

        private void HandleUserDefinedAck(
            ref InPacket inPacket)
        {
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(inPacket.HookId);

            userDefinedSubscription?.OnAck?.Invoke(inPacket.ConnectionId);
        }

        private void HandleProtocolAck(
            ref InPacket inPacket)
        {
            var protocolHookId = (ProtocolHookId)inPacket.HookId;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Heartbeat:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                case ProtocolHookId.Connect2Peer:
                    switch (protocolHookId)
                    {
                        case ProtocolHookId.Connect:
                        case ProtocolHookId.Disconnect:
                            OnConnectionChanged?.Invoke(protocolHookId == ProtocolHookId.Connect);
                            break;
                    }

                    userDefinedSubscription?.OnAck?.Invoke(inPacket.ConnectionId);
                    break;
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
                _broadcaster.Dispose();
                _scheduler.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}