namespace UdpToolkit.Framework.Jobs
{
    using System;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;
    using UdpToolkit.Network.Queues;

    public sealed class WorkerJob
    {
        private readonly ILogger _logger = Log.Logger;

        private readonly Scheduler _scheduler;
        private readonly HostSettings _hostSettings;
        private readonly ServerHostClient _serverHostClient;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;
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
            IProtocolSubscriptionManager protocolSubscriptionManager,
            IRoomManager roomManager,
            IDateTimeProvider dateTimeProvider,
            HostSettings hostSettings,
            Scheduler scheduler,
            ServerHostClient serverHostClient)
        {
            _inputQueue = inputQueue;
            _callContextPool = callContextPool;
            _dateTimeProvider = dateTimeProvider;
            _subscriptionManager = subscriptionManager;
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _hostSettings = hostSettings;
            _scheduler = scheduler;
            _serverHostClient = serverHostClient;
            _roomManager = roomManager;
            _outputQueue = outputQueue;
        }

        public IServerHostClient ServerHostClient => _serverHostClient;

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
                    _logger.Warning("WorkerError - {@Exception}", ex);
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
            var protocolSubscription = _protocolSubscriptionManager
                .GetProtocolSubscription((byte)protocolHookId);

            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping:
                case ProtocolHookId.Pong:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                    protocolSubscription?.OnInputEvent(
                            arg1: networkPacket.Serializer(),
                            arg2: networkPacket.PeerId);

                    userDefinedSubscription?.OnProtocolEvent(
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
                _logger.Error($"Subscription with id {networkPacket.HookId} not found! {nameof(HandleUserDefinedEvent)}");

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

            userDefinedSubscription?
                .OnAck(networkPacket.PeerId);
        }

        private void HandleProtocolAck(
            PooledObject<CallContext> pooledCallContext)
        {
            var networkPacket = pooledCallContext.Value.NetworkPacketDto;
            var protocolHookId = networkPacket.ProtocolHookId;
            var userDefinedSubscription = _subscriptionManager
                .GetSubscription((byte)protocolHookId);

            var protocolSubscription = _protocolSubscriptionManager
                .GetProtocolSubscription((byte)protocolHookId);

            switch (protocolHookId)
            {
                case ProtocolHookId.Ping:
                case ProtocolHookId.Pong:
                case ProtocolHookId.P2P:
                case ProtocolHookId.Disconnect:
                case ProtocolHookId.Connect:
                    switch (protocolHookId)
                    {
                        case ProtocolHookId.Connect:
                        case ProtocolHookId.Disconnect:
                            _serverHostClient.IsConnected = protocolHookId == ProtocolHookId.Connect;
                            break;
                    }

                    protocolSubscription?
                        .OnAck(networkPacket.PeerId);

                    userDefinedSubscription?
                        .OnAck(networkPacket.PeerId);
                    break;
            }
        }
    }
}