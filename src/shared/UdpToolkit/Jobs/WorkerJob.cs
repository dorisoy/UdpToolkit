namespace UdpToolkit.Jobs
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Pooling;
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

        private readonly IObjectsPool<CallContext> _callContextPool;

        private readonly IAsyncQueue<PooledObject<CallContext>> _inputQueue;
        private readonly IAsyncQueue<PooledObject<CallContext>> _outputQueue;

        public WorkerJob(
            IAsyncQueue<PooledObject<CallContext>> inputQueue,
            IAsyncQueue<PooledObject<CallContext>> outputQueue,
            IObjectsPool<CallContext> callContextPool,
            ISubscriptionManager subscriptionManager,
            IRoomManager roomManager,
            IDateTimeProvider dateTimeProvider,
            HostSettings hostSettings,
            Scheduler scheduler,
            HostClient hostClient,
            IUdpToolkitLogger udpToolkitLogger)
        {
            _inputQueue = inputQueue;
            _callContextPool = callContextPool;
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
            foreach (var pooledCallContext in _inputQueue.Consume())
            {
                try
                {
                    using (pooledCallContext)
                    {
                        ExecuteInternal(pooledCallContext);
                    }
                }
                catch (Exception ex)
                {
                    _udpToolkitLogger.Warning($"WorkerError - {ex}");
                }
            }
        }

        private void ExecuteInternal(
            PooledObject<CallContext> pooledCallContext)
        {
            switch (pooledCallContext.Value.NetworkPacketDto.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                    HandleUserDefinedEvent(pooledCallContext);
                    break;
                case NetworkPacketType.Protocol:
                    HandleProtocolEvent(pooledCallContext);
                    break;
                case NetworkPacketType.Ack:
                    if (pooledCallContext.Value.NetworkPacketDto.IsProtocolEvent)
                    {
                        HandleProtocolAck(pooledCallContext);
                    }
                    else
                    {
                        HandleUserDefinedAck(pooledCallContext);
                    }

                    break;
            }
        }

        private void HandleProtocolEvent(
            PooledObject<CallContext> pooledCallContext)
        {
            var networkPacket = pooledCallContext.Value.NetworkPacketDto;
            var protocolHookId = networkPacket.ProtocolHookId;

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:

                    userDefinedSubscription?.OnProtocolEvent?.Invoke(
                        networkPacket.Serializer(),
                        networkPacket.PeerId,
                        _hostSettings.Serializer);

                    break;
            }

            var protocolAck = _callContextPool.Get();

            protocolAck.Value.Set(
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                createdAt: _dateTimeProvider.UtcNow(),
                roomId: null,
                broadcastMode: Core.BroadcastMode.Caller);

            protocolAck.Value.NetworkPacketDto.Set(
                id: networkPacket.Id,
                acks: networkPacket.Acks,
                hookId: networkPacket.HookId,
                channelType: networkPacket.ChannelType,
                peerId: networkPacket.PeerId,
                networkPacketType: networkPacket.NetworkPacketType,
                serializer: networkPacket.Serializer,
                createdAt: networkPacket.CreatedAt,
                ipEndPoint: networkPacket.IpEndPoint);

            _outputQueue.Produce(protocolAck);
        }

        private void HandleUserDefinedEvent(
            PooledObject<CallContext> pooledCallContext)
        {
            var networkPacket = pooledCallContext.Value.NetworkPacketDto;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            if (userDefinedSubscription == null)
            {
                _udpToolkitLogger.Error($"Subscription with id {networkPacket.HookId} not found! {nameof(HandleUserDefinedEvent)}");

                return;
            }

            var roomId = userDefinedSubscription.OnEvent(
                    networkPacket.Serializer(),
                    networkPacket.PeerId,
                    _hostSettings.Serializer,
                    _roomManager,
                    _scheduler);

            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromClient when networkPacket.IsReliable:
                    // ack to client
                    var pooledClientAck = _callContextPool.Get();

                    pooledClientAck.Value.Set(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow(),
                        roomId: roomId,
                        broadcastMode: BroadcastMode.Caller);

                    pooledClientAck.Value.NetworkPacketDto.Set(
                        id: networkPacket.Id,
                        acks: networkPacket.Acks,
                        hookId: networkPacket.HookId,
                        channelType: networkPacket.ChannelType,
                        peerId: networkPacket.PeerId,
                        networkPacketType: NetworkPacketType.Ack,
                        serializer: networkPacket.Serializer,
                        createdAt: networkPacket.CreatedAt,
                        ipEndPoint: networkPacket.IpEndPoint);

                    _outputQueue.Produce(pooledClientAck);
                    break;
                case NetworkPacketType.FromServer when networkPacket.IsReliable:
                    // ack to server
                    var pooledServerAck = _callContextPool.Get();

                    pooledServerAck.Value.Set(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow(),
                        roomId: roomId,
                        broadcastMode: BroadcastMode.AckToServer);

                    pooledServerAck.Value.NetworkPacketDto.Set(
                        id: networkPacket.Id,
                        acks: networkPacket.Acks,
                        hookId: networkPacket.HookId,
                        channelType: networkPacket.ChannelType,
                        peerId: networkPacket.PeerId,
                        networkPacketType: networkPacket.NetworkPacketType,
                        serializer: networkPacket.Serializer,
                        createdAt: networkPacket.CreatedAt,
                        ipEndPoint: networkPacket.IpEndPoint);

                    _outputQueue.Produce(pooledServerAck);
                    break;

                case NetworkPacketType.FromServer when networkPacket.ChannelType == ChannelType.Sequenced:
                    // release call context?
                    break;
                case NetworkPacketType.FromClient when networkPacket.ChannelType == ChannelType.Sequenced:
                    var pooledFromServer = _callContextPool.Get();

                    pooledFromServer.Value.Set(
                        resendTimeout: _hostSettings.ResendPacketsTimeout,
                        createdAt: _dateTimeProvider.UtcNow(),
                        roomId: roomId,
                        broadcastMode: userDefinedSubscription.BroadcastMode);

                    pooledFromServer.Value.NetworkPacketDto.Set(
                        id: networkPacket.Id,
                        acks: networkPacket.Acks,
                        hookId: networkPacket.HookId,
                        channelType: networkPacket.ChannelType,
                        peerId: networkPacket.PeerId,
                        networkPacketType: networkPacket.NetworkPacketType,
                        serializer: networkPacket.Serializer,
                        createdAt: networkPacket.CreatedAt,
                        ipEndPoint: networkPacket.IpEndPoint);

                    _outputQueue.Produce(pooledFromServer);

                    break;
            }
        }

        private void HandleUserDefinedAck(
            PooledObject<CallContext> pooledCallContext)
        {
            var networkPacket = pooledCallContext.Value.NetworkPacketDto;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription(networkPacket.HookId);

            userDefinedSubscription?.OnAck?.Invoke(networkPacket.PeerId);
        }

        private void HandleProtocolAck(
            PooledObject<CallContext> pooledCallContext)
        {
            var networkPacket = pooledCallContext.Value.NetworkPacketDto;
            var protocolHookId = networkPacket.ProtocolHookId;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping:
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

                    userDefinedSubscription?.OnAck?.Invoke(networkPacket.PeerId);
                    break;
            }
        }
    }
}