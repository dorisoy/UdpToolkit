namespace UdpToolkit.Framework.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Pooling;
    using UdpToolkit.Network.Queues;

    public sealed class SenderJob
    {
        private readonly ILogger _logger = Log.Logger;
        private readonly ResendQueue _resendQueue;
        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;
        private readonly IAsyncQueue<PooledObject<CallContext>> _outputQueue;
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;
        private readonly IRawPeerManager _rawPeerManager;
        private readonly IServerSelector _serverSelector;
        private readonly Scheduler _scheduler;

        public SenderJob(
            IProtocolSubscriptionManager protocolSubscriptionManager,
            IAsyncQueue<PooledObject<CallContext>> outputQueue,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IRawPeerManager rawPeerManager,
            IServerSelector serverSelector,
            Scheduler scheduler,
            ResendQueue resendQueue)
        {
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _outputQueue = outputQueue;
            _networkPacketPool = networkPacketPool;
            _rawPeerManager = rawPeerManager;
            _serverSelector = serverSelector;
            _scheduler = scheduler;
            _resendQueue = resendQueue;
        }

        public async Task Execute(
            IUdpSender udpSender)
        {
            foreach (var pooledCallContext in _outputQueue.Consume())
            {
                using (pooledCallContext)
                {
                    // for avoiding effect on UI thread set IP at here
                    if (pooledCallContext.Value.NetworkPacketDto.IpEndPoint == null)
                    {
                        var serverIp = _serverSelector.GetServer().GetRandomIp();
                        pooledCallContext.Value.NetworkPacketDto.Set(ipEndPoint: serverIp);
                    }

                    var networkPacketDto = pooledCallContext.Value.NetworkPacketDto;
                    var resendTimeout = pooledCallContext.Value.ResendTimeout;
                    var peerId = pooledCallContext.Value.NetworkPacketDto.PeerId;
                    using (var pooledNetworkPacket = _networkPacketPool.Get())
                    {
                        pooledNetworkPacket.Value.Set(
                            hookId: networkPacketDto.HookId,
                            channelType: networkPacketDto.ChannelType,
                            networkPacketType: networkPacketDto.NetworkPacketType,
                            peerId: networkPacketDto.PeerId,
                            id: networkPacketDto.Id,
                            acks: networkPacketDto.Acks,
                            serializer: networkPacketDto.Serializer,
                            createdAt: networkPacketDto.CreatedAt,
                            ipEndPoint: networkPacketDto.IpEndPoint);

                        await ExecuteInternal(
                                udpSender: udpSender,
                                roomId: pooledCallContext.Value.RoomId,
                                broadcastMode: pooledCallContext.Value.BroadcastMode,
                                pooledNetworkPacket: pooledNetworkPacket)
                            .ConfigureAwait(false);

                        var pt = pooledNetworkPacket.Value.NetworkPacketType;

                        if (networkPacketDto.IsReliable)
                        {
                            var newPooledNetworkPacket = _networkPacketPool.Get();

                            newPooledNetworkPacket.Value.Set(
                                hookId: pooledNetworkPacket.Value.HookId,
                                channelType: pooledNetworkPacket.Value.ChannelType,
                                networkPacketType: pt,
                                peerId: pooledNetworkPacket.Value.PeerId,
                                id: pooledNetworkPacket.Value.Id,
                                acks: pooledNetworkPacket.Value.Acks,
                                serializer: pooledNetworkPacket.Value.Serializer,
                                createdAt: pooledNetworkPacket.Value.CreatedAt,
                                ipEndPoint: pooledNetworkPacket.Value.IpEndPoint);

                            if (newPooledNetworkPacket.Value.NetworkPacketType != NetworkPacketType.Ack)
                            {
                                _resendQueue.Add(newPooledNetworkPacket);
                            }
                        }

                        if (_rawPeerManager.TryGetPeer(peerId, out var peer))
                        {
                            _scheduler.Schedule(
                                key: peer.PeerId,
                                dueTimeMs: 1000,
                                action: () =>
                                {
                                    _logger.Debug($"Resend: {DateTime.UtcNow} PeerId: {peerId}");
                                    var resendQueue = _resendQueue.Get(peer.PeerId);
                                    for (var i = 0; i < resendQueue.Count; i++)
                                    {
                                        var pendingNetworkPacket = resendQueue[i];
                                        var isDelivered = peer
                                            .GetOutcomingChannel(pendingNetworkPacket.Value.ChannelType)
                                            .IsDelivered(pendingNetworkPacket.Value.Id);

                                        var isExpired = pendingNetworkPacket.Value
                                            .IsExpired(resendTimeout);

                                        if (!isDelivered && !isExpired)
                                        {
                                            udpSender.SendAsync(pendingNetworkPacket)
                                                .GetAwaiter()
                                                .GetResult();
                                        }
                                        else
                                        {
                                            resendQueue.RemoveAt(i);
                                            pendingNetworkPacket?.Dispose();
                                        }
                                    }
                                });
                        }
                    }
                }
            }
        }

        private async Task ExecuteInternal(
            IUdpSender udpSender,
            int? roomId,
            BroadcastMode? broadcastMode,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            if (pooledNetworkPacket.Value.IsProtocolEvent)
            {
                var protocolSubscription = _protocolSubscriptionManager
                    .GetProtocolSubscription(pooledNetworkPacket.Value.HookId);

                protocolSubscription?.OnOutputEvent(
                    arg1: pooledNetworkPacket.Value.Serializer(),
                    arg2: pooledNetworkPacket.Value.PeerId);
            }

            switch (broadcastMode)
            {
                case BroadcastMode.Room:
                case BroadcastMode.RoomExceptCaller:
                    await udpSender
                        .SendAsync(
                            roomId: roomId ?? throw new ArgumentNullException(nameof(roomId)),
                            pooledNetworkPacket: pooledNetworkPacket,
                            broadcastType: broadcastMode?.Map() ?? throw new ArgumentNullException(nameof(broadcastMode)))
                        .ConfigureAwait(false);

                    break;

                case BroadcastMode.Caller:
                case BroadcastMode.Server:
                case BroadcastMode.AllPeers:
                case BroadcastMode.AckToServer:
                    var id = broadcastMode == BroadcastMode.AckToServer
                        ? _serverSelector.GetServer().PeerId
                        : pooledNetworkPacket.Value.PeerId;

                    _rawPeerManager.TryGetPeer(peerId: id, out var rawPeer);

                    await udpSender
                        .SendAsync(
                            pooledNetworkPacket: pooledNetworkPacket,
                            rawPeer: rawPeer,
                            broadcastType: broadcastMode?.Map() ?? throw new ArgumentNullException(nameof(broadcastMode)))
                        .ConfigureAwait(false);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}