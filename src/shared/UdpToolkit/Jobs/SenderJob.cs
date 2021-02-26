namespace UdpToolkit.Jobs
{
    using System;
    using System.Threading.Tasks;
    using UdpToolkit.Core;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;
    using UdpToolkit.Network.Queues;

    public sealed class SenderJob
    {
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly ResendQueue _resendQueue;
        private readonly IConnection _serverConnection;
        private readonly IAsyncQueue<PooledObject<CallContext>> _outputQueue;
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;
        private readonly IConnectionPool _connectionPool;
        private readonly Scheduler _scheduler;

        public SenderJob(
            IConnection serverConnection,
            IAsyncQueue<PooledObject<CallContext>> outputQueue,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IConnectionPool connectionPool,
            Scheduler scheduler,
            ResendQueue resendQueue,
            IUdpToolkitLogger udpToolkitLogger)
        {
            _serverConnection = serverConnection;
            _outputQueue = outputQueue;
            _networkPacketPool = networkPacketPool;
            _connectionPool = connectionPool;
            _scheduler = scheduler;
            _resendQueue = resendQueue;
            _udpToolkitLogger = udpToolkitLogger;
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
                        var serverIp = _serverConnection.GetRandomIp();
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
                            connectionId: networkPacketDto.PeerId,
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
                                connectionId: pooledNetworkPacket.Value.ConnectionId,
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

                        var peer = _connectionPool.TryGetConnection(peerId);
                        if (peer == null)
                        {
                            continue;
                        }

                        _scheduler.Schedule(
                            key: peer.ConnectionId,
                            dueTimeMs: 1000,
                            action: () =>
                            {
                                _udpToolkitLogger.Debug($"Resend: {DateTime.UtcNow} PeerId: {peerId}");
                                var resendQueue = _resendQueue.Get(peer.ConnectionId);
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

        private async Task ExecuteInternal(
            IUdpSender udpSender,
            int? roomId,
            BroadcastMode? broadcastMode,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
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
                    var peerId = broadcastMode == BroadcastMode.AckToServer
                        ? _serverConnection.ConnectionId
                        : pooledNetworkPacket.Value.ConnectionId;

                    var connection = _connectionPool.TryGetConnection(connectionId: peerId);
                    if (connection == null)
                    {
                        return;
                    }

                    await udpSender
                        .SendAsync(
                            pooledNetworkPacket: pooledNetworkPacket,
                            connection: connection,
                            broadcastType: broadcastMode?.Map() ?? throw new ArgumentNullException(nameof(broadcastMode)))
                        .ConfigureAwait(false);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}