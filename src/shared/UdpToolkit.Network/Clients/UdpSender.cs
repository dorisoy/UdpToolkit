namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;

    public sealed class UdpSender : IUdpSender
    {
        private const int MtuSizeLimit = 1500;

        private readonly UdpClient _sender;

        private readonly IConnectionPool _connectionPool;
        private readonly IRawRoomManager _rawRoomManager;
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;
        private readonly IUdpToolkitLogger _udpToolkitLogger;

        public UdpSender(
            UdpClient sender,
            IRawRoomManager rawRoomManager,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IUdpToolkitLogger udpToolkitLogger,
            IConnectionPool connectionPool)
        {
            _sender = sender;
            _rawRoomManager = rawRoomManager;
            _networkPacketPool = networkPacketPool;
            _udpToolkitLogger = udpToolkitLogger;
            _connectionPool = connectionPool;
            _udpToolkitLogger.Debug($"{nameof(UdpSender)} - {sender.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            return SendInternalAsync(pooledNetworkPacket);
        }

        public Task SendAsync(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket,
            BroadcastType broadcastType)
        {
            switch (broadcastType)
            {
                case BroadcastType.Room:
                    return HandleRoom(roomId, pooledNetworkPacket);
                case BroadcastType.RoomExceptCaller:
                    return HandleRoomExceptCaller(roomId, pooledNetworkPacket);
                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastType), broadcastType, null);
            }
        }

        public Task SendAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IConnection connection,
            BroadcastType broadcastType)
        {
            switch (broadcastType)
            {
                case BroadcastType.Caller:
                    return HandleCaller(pooledNetworkPacket, connection);
                case BroadcastType.Server:
                    return HandleServer(pooledNetworkPacket, connection);
                case BroadcastType.AckToServer:
                    return HandleAckToServer(pooledNetworkPacket, connection);
                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastType), broadcastType, null);
            }
        }

        private async Task SendInternalAsync(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var bytes = NetworkPacket.Serialize(pooledNetworkPacket.Value);

            if (bytes.Length > MtuSizeLimit)
            {
                _udpToolkitLogger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            _udpToolkitLogger.Debug($"Packet from - {_sender.Client.LocalEndPoint} to {pooledNetworkPacket.Value.IpEndPoint} sended");
            _udpToolkitLogger.Debug($"Packet sends: {pooledNetworkPacket.Value}, Total bytes length: {bytes.Length}, Payload bytes length: {pooledNetworkPacket.Value.Serializer().Length}");

            await _sender
                .SendAsync(bytes, bytes.Length, pooledNetworkPacket.Value.IpEndPoint)
                .ConfigureAwait(false);
        }

        private Task HandleCaller(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IConnection connection)
        {
            return Send(connection, pooledNetworkPacket);
        }

        private Task HandleRoom(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            return _rawRoomManager
                .Apply(
                    caller: pooledNetworkPacket.Value.ConnectionId,
                    roomId: roomId,
                    condition: (connectionId) => true,
                    func: (connectionId) =>
                    {
                        var connection = _connectionPool.TryGetConnection(connectionId);
                        if (connection != null)
                        {
                            Send(connection, pooledNetworkPacket);
                        }

                        return Task.CompletedTask;
                    });
        }

        private Task HandleServer(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IConnection connection)
        {
            connection
                .GetOutcomingChannel(pooledNetworkPacket.Value.ChannelType)
                .HandleOutputPacket(pooledNetworkPacket.Value);

            return SendInternalAsync(pooledNetworkPacket);
        }

        private Task HandleRoomExceptCaller(
            int roomId,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            return _rawRoomManager
                .Apply(
                    caller: pooledNetworkPacket.Value.ConnectionId,
                    roomId: roomId,
                    condition: (connectionId) => connectionId != pooledNetworkPacket.Value.ConnectionId,
                    func: (connectionId) =>
                    {
                        var connection = _connectionPool.TryGetConnection(connectionId);
                        if (connection != null)
                        {
                            Send(connection, pooledNetworkPacket);
                        }

                        return Task.CompletedTask;
                    });
        }

        private Task HandleAckToServer(
            PooledObject<NetworkPacket> pooledNetworkPacket,
            IConnection connection)
        {
            connection
                .GetOutcomingChannel(pooledNetworkPacket.Value.ChannelType)
                .GetAck(pooledNetworkPacket.Value);

            pooledNetworkPacket.Value.Set(ipEndPoint: connection.GetRandomIp());

            return SendInternalAsync(pooledNetworkPacket);
        }

        private Task Send(
            IConnection connection,
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
                    ipEndPoint: connection.GetRandomIp());

                connection
                    .GetOutcomingChannel(pooledNetworkPacket.Value.ChannelType)
                    .GetAck(pooledNetworkPacket.Value);

                return SendInternalAsync(pooledNetworkPacket);
            }
            else
            {
                // produce packet
                var pooledNetworkPacket = _networkPacketPool.Get();

                pooledNetworkPacket.Value.Set(
                    id: originalPooledNetworkPacket.Value.Id,
                    acks: originalPooledNetworkPacket.Value.Acks,
                    hookId: originalPooledNetworkPacket.Value.HookId,
                    ipEndPoint: connection.GetRandomIp(),
                    createdAt: DateTimeOffset.UtcNow,
                    serializer: originalPooledNetworkPacket.Value.Serializer,
                    channelType: originalPooledNetworkPacket.Value.ChannelType,
                    connectionId: connection.ConnectionId,
                    networkPacketType: NetworkPacketType.FromServer);

                connection
                    .GetOutcomingChannel(channelType: pooledNetworkPacket.Value.ChannelType)
                    .HandleOutputPacket(pooledNetworkPacket.Value);

                return SendInternalAsync(pooledNetworkPacket);
            }
        }
    }
}
