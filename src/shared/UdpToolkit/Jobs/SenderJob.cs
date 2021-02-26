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
        private readonly RoomManager _roomManager;
        private readonly Scheduler _scheduler;

        public SenderJob(
            IConnection serverConnection,
            IAsyncQueue<PooledObject<CallContext>> outputQueue,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IConnectionPool connectionPool,
            Scheduler scheduler,
            ResendQueue resendQueue,
            IUdpToolkitLogger udpToolkitLogger,
            RoomManager roomManager)
        {
            _serverConnection = serverConnection;
            _outputQueue = outputQueue;
            _networkPacketPool = networkPacketPool;
            _connectionPool = connectionPool;
            _scheduler = scheduler;
            _resendQueue = resendQueue;
            _udpToolkitLogger = udpToolkitLogger;
            _roomManager = roomManager;
        }

        public async Task Execute(
            IUdpSender udpSender)
        {
            foreach (var pooledCallContext in _outputQueue.Consume())
            {
                using (pooledCallContext)
                {
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

                        // TODO rewrite resend logic
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

                        // TODO raise heartbeat events (ping/pong) for sending packet instead of timers
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
                    await _roomManager
                        .ApplyAsync(
                            roomId: roomId ?? throw new ArgumentNullException(nameof(roomId)),
                            condition: (connection) => true,
                            func: (connection) => Send(connection, udpSender, pooledNetworkPacket))
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.RoomExceptCaller:
                    await _roomManager
                        .ApplyAsync(
                            roomId: roomId ?? throw new ArgumentNullException(nameof(roomId)),
                            condition: (connection) => connection.ConnectionId != pooledNetworkPacket.Value.ConnectionId,
                            func: (connection) => Send(connection, udpSender, pooledNetworkPacket))
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.Caller:
                    await Send(
                            connection: _connectionPool.TryGetConnection(pooledNetworkPacket.Value.ConnectionId),
                            udpSender: udpSender,
                            originalPooledNetworkPacket: pooledNetworkPacket)
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.Server:
                    await udpSender
                        .SendAsync(pooledNetworkPacket)
                        .ConfigureAwait(false);

                    break;
                case BroadcastMode.AllPeers:
                    await _connectionPool
                        .Apply(
                            condition: () => true,
                            func: () => udpSender.SendAsync(pooledNetworkPacket))
                        .ConfigureAwait(false);

                    break;

                case BroadcastMode.AckToServer:
                    var connectionId = broadcastMode == BroadcastMode.AckToServer
                        ? _serverConnection.ConnectionId
                        : pooledNetworkPacket.Value.ConnectionId;

                    var connection = _connectionPool.TryGetConnection(connectionId);
                    pooledNetworkPacket.Value.Set(
                        ipEndPoint: connection.GetIp(),
                        networkPacketType: NetworkPacketType.Ack);

                    await udpSender
                        .SendAsync(pooledNetworkPacket)
                        .ConfigureAwait(false);

                    break;

                default:
                    throw new NotSupportedException(broadcastMode.ToString());
            }
        }

        private Task Send(
            IConnection connection,
            IUdpSender udpSender,
            PooledObject<NetworkPacket> originalPooledNetworkPacket)
        {
            if (connection.ConnectionId == originalPooledNetworkPacket.Value.ConnectionId && originalPooledNetworkPacket.Value.IsReliable)
            {
                // produce ack
                var pooledNetworkPacket = _networkPacketPool.Get();

                originalPooledNetworkPacket.Value.Set(networkPacketType: NetworkPacketType.Ack);

                pooledNetworkPacket.Value.Set(
                    hookId: originalPooledNetworkPacket.Value.HookId,
                    channelType: originalPooledNetworkPacket.Value.ChannelType,
                    networkPacketType: NetworkPacketType.Ack,
                    connectionId: originalPooledNetworkPacket.Value.ConnectionId,
                    id: originalPooledNetworkPacket.Value.Id,
                    acks: originalPooledNetworkPacket.Value.Acks,
                    serializer: originalPooledNetworkPacket.Value.Serializer,
                    createdAt: originalPooledNetworkPacket.Value.CreatedAt,
                    ipEndPoint: connection.GetIp());

                return udpSender.SendAsync(pooledNetworkPacket);
            }
            else
            {
                // produce packet
                var pooledNetworkPacket = _networkPacketPool.Get();

                pooledNetworkPacket.Value.Set(
                    id: originalPooledNetworkPacket.Value.Id,
                    acks: originalPooledNetworkPacket.Value.Acks,
                    hookId: originalPooledNetworkPacket.Value.HookId,
                    ipEndPoint: connection.GetIp(),
                    createdAt: DateTimeOffset.UtcNow,
                    serializer: originalPooledNetworkPacket.Value.Serializer,
                    channelType: originalPooledNetworkPacket.Value.ChannelType,
                    connectionId: connection.ConnectionId,
                    networkPacketType: NetworkPacketType.FromServer);

                return udpSender.SendAsync(pooledNetworkPacket);
            }
        }
    }
}
