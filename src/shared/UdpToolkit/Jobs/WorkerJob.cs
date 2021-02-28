namespace UdpToolkit.Jobs
{
    using System;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class WorkerJob
    {
        private readonly IUdpToolkitLogger _udpToolkitLogger;

        private readonly HostSettings _hostSettings;
        private readonly HostClient _hostClient;

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IRoomManager _roomManager;

        private readonly IAsyncQueue<InContext> _inputQueue;
        private readonly IBroadcaster _broadcaster;

        public WorkerJob(
            IAsyncQueue<InContext> inputQueue,
            ISubscriptionManager subscriptionManager,
            IRoomManager roomManager,
            HostSettings hostSettings,
            HostClient hostClient,
            IUdpToolkitLogger udpToolkitLogger,
            IBroadcaster broadcaster)
        {
            _inputQueue = inputQueue;
            _subscriptionManager = subscriptionManager;
            _hostSettings = hostSettings;
            _hostClient = hostClient;
            _udpToolkitLogger = udpToolkitLogger;
            _broadcaster = broadcaster;
            _roomManager = roomManager;
        }

        public IHostClient HostClient => _hostClient;

        public void Execute()
        {
            foreach (var callContext in _inputQueue.Consume())
            {
                try
                {
                    ExecuteInternal(callContext);
                }
                catch (Exception ex)
                {
                    _udpToolkitLogger.Warning($"WorkerError - {ex}");
                }
            }
        }

        private void ExecuteInternal(
            InContext inContext)
        {
            switch (inContext.InPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                    HandleUserDefinedEvent(ref inContext);
                    break;
                case NetworkPacketType.Protocol:
                    HandleProtocolEvent(ref inContext);
                    break;
                case NetworkPacketType.Ack:
                    if (inContext.InPacket.IsProtocolEvent)
                    {
                        HandleProtocolAck(ref inContext);
                    }
                    else
                    {
                        HandleUserDefinedAck(ref inContext);
                    }

                    break;
            }
        }

        private void HandleProtocolEvent(
            ref InContext inContext)
        {
            var networkPacket = inContext.InPacket;
            var protocolHookId = (ProtocolHookId)networkPacket.HookId;

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Heartbeat:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:

                    userDefinedSubscription?.OnProtocolEvent?.Invoke(
                        networkPacket.Serializer(),
                        networkPacket.ConnectionId,
                        _hostSettings.Serializer);

                    break;
            }
        }

        private void HandleUserDefinedEvent(
            ref InContext inContext)
        {
            var networkPacket = inContext.InPacket;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            if (userDefinedSubscription == null)
            {
                _udpToolkitLogger.Error($"Subscription with id {networkPacket.HookId} not found! {nameof(HandleUserDefinedEvent)}");

                return;
            }

            var roomId = userDefinedSubscription.OnEvent(
                    networkPacket.Serializer(),
                    networkPacket.ConnectionId,
                    _hostSettings.Serializer,
                    _roomManager);

            if (networkPacket.NetworkPacketType == NetworkPacketType.FromClient)
            {
                _broadcaster.Broadcast(
                    networkPacketType: NetworkPacketType.FromServer,
                    serializer: networkPacket.Serializer,
                    caller: networkPacket.ConnectionId,
                    roomId: roomId,
                    hookId: networkPacket.HookId,
                    channelType: networkPacket.ChannelType,
                    broadcastMode: userDefinedSubscription.BroadcastMode);
            }
        }

        private void HandleUserDefinedAck(
            ref InContext inContext)
        {
            var networkPacket = inContext.InPacket;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            userDefinedSubscription?.OnAck?.Invoke(networkPacket.ConnectionId);
        }

        private void HandleProtocolAck(
            ref InContext inContext)
        {
            var networkPacket = inContext.InPacket;
            var protocolHookId = (ProtocolHookId)networkPacket.HookId;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Heartbeat:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                    switch (protocolHookId)
                    {
                        case ProtocolHookId.Connect:
                        case ProtocolHookId.Disconnect:
                            _hostClient.IsConnected = protocolHookId == ProtocolHookId.Connect;
                            break;
                    }

                    userDefinedSubscription?.OnAck?.Invoke(networkPacket.ConnectionId);
                    break;
            }
        }
    }
}