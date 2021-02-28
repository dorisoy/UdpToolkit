namespace UdpToolkit.Jobs
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class WorkerJob
    {
        private readonly IUdpToolkitLogger _udpToolkitLogger;

        private readonly Scheduler _scheduler;
        private readonly HostSettings _hostSettings;
        private readonly HostClient _hostClient;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IRoomManager _roomManager;

        private readonly IAsyncQueue<CallContext> _inputQueue;
        private readonly IAsyncQueue<CallContext> _outputQueue;

        public WorkerJob(
            IAsyncQueue<CallContext> inputQueue,
            IAsyncQueue<CallContext> outputQueue,
            ISubscriptionManager subscriptionManager,
            IRoomManager roomManager,
            IDateTimeProvider dateTimeProvider,
            HostSettings hostSettings,
            Scheduler scheduler,
            HostClient hostClient,
            IUdpToolkitLogger udpToolkitLogger)
        {
            _inputQueue = inputQueue;
            _dateTimeProvider = dateTimeProvider;
            _subscriptionManager = subscriptionManager;
            _hostSettings = hostSettings;
            _scheduler = scheduler;
            _hostClient = hostClient;
            _udpToolkitLogger = udpToolkitLogger;
            _roomManager = roomManager;
            _outputQueue = outputQueue;
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
            CallContext callContext)
        {
            switch (callContext.NetworkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                    HandleUserDefinedEvent(ref callContext);
                    break;
                case NetworkPacketType.Protocol:
                    HandleProtocolEvent(ref callContext);
                    break;
                case NetworkPacketType.Ack:
                    if (callContext.NetworkPacket.IsProtocolEvent)
                    {
                        HandleProtocolAck(ref callContext);
                    }
                    else
                    {
                        HandleUserDefinedAck(ref callContext);
                    }

                    break;
            }
        }

        private void HandleProtocolEvent(
            ref CallContext callContext)
        {
            var networkPacket = callContext.NetworkPacket;
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
            ref CallContext callContext)
        {
            var networkPacket = callContext.NetworkPacket;
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
                    _roomManager,
                    _scheduler);

            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer when networkPacket.ChannelType == ChannelType.Sequenced:
                    // release call context?
                    break;
                case NetworkPacketType.FromClient when networkPacket.ChannelType == ChannelType.Sequenced:

                    _outputQueue.Produce(new CallContext(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow(),
                        roomId: roomId,
                        broadcastMode: userDefinedSubscription.BroadcastMode,
                        networkPacket: new NetworkPacket(
                            hookId: networkPacket.HookId,
                            channelType: networkPacket.ChannelType,
                            networkPacketType: NetworkPacketType.FromServer,
                            connectionId: networkPacket.ConnectionId,
                            createdAt: networkPacket.CreatedAt,
                            serializer: networkPacket.Serializer,
                            ipEndPoint: networkPacket.IpEndPoint)));

                    break;
            }
        }

        private void HandleUserDefinedAck(
            ref CallContext callContext)
        {
            var networkPacket = callContext.NetworkPacket;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            userDefinedSubscription?.OnAck?.Invoke(networkPacket.ConnectionId);
        }

        private void HandleProtocolAck(
            ref CallContext callContext)
        {
            var networkPacket = callContext.NetworkPacket;
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