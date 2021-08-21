namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Serialization;
    using UdpToolkit.Network.Utils;

    internal sealed unsafe class UdpClient : IUdpClient
    {
        private static readonly int NetworkHeaderSize = sizeof(NetworkHeader);
        private readonly UdpClientSettings _settings;

        private readonly IConnectionIdFactory _connectionIdFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ISocket _client;
        private readonly IUdpToolkitLogger _logger;
        private readonly IConnectionPool _connectionPool;
        private readonly IResendQueue _resendQueue;

        private bool _disposed = false;

        internal UdpClient(
            IConnectionPool connectionPool,
            IUdpToolkitLogger logger,
            IDateTimeProvider dateTimeProvider,
            ISocket client,
            IResendQueue resendQueue,
            UdpClientSettings settings,
            IConnectionIdFactory connectionIdFactory)
        {
            _client = client;
            _connectionPool = connectionPool;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _resendQueue = resendQueue;
            _settings = settings;
            _connectionIdFactory = connectionIdFactory;
        }

        ~UdpClient()
        {
            Dispose(false);
        }

        public event Action<IpV4Address, Guid, byte[], byte> OnPacketReceived;

        public event Action<IpV4Address, Guid, byte[], byte> OnPacketExpired;

        public event Action<IpV4Address, Guid> OnConnected;

        public event Action<IpV4Address, Guid> OnDisconnected;

        public event Action<Guid, TimeSpan> OnHeartbeat;

        private Guid? ConnectionId { get; set; }

        public bool IsConnected(out Guid connectionId)
        {
            connectionId = ConnectionId ?? default;
            return ConnectionId.HasValue;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public unsafe void Connect(
            IpV4Address ipV4Address)
        {
            ConnectionId = _connectionIdFactory.Generate();

            var connection = _connectionPool.GetOrAdd(
                connectionId: ConnectionId.Value,
                keepAlive: true,
                lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                ipV4Address: _client.GetLocalIp());

            connection
                .GetOutcomingChannel(ReliableChannel.Id)
                .HandleOutputPacket(out var id, out var acks);

            var networkHeader = new NetworkHeader(
                channelId: ReliableChannel.Id,
                id: id,
                acks: acks,
                connectionId: connection.ConnectionId,
                packetType: PacketType.Connect);

            var buffer = new byte[NetworkHeaderSize];
            UnsafeSerialization.Write(buffer: buffer, value: networkHeader);

#if DEBUG
            _logger.Debug($"[UdpToolkit.Network] Connecting to: {ipV4Address} bytes length: {buffer.Length}, type: {networkHeader.PacketType}");
#endif

            _client.Send(ref ipV4Address, buffer, buffer.Length);

            _resendQueue.Add(
                connection.ConnectionId,
                new PendingPacket(
                    packetType: networkHeader.PacketType,
                    connectionId: connection.ConnectionId,
                    payload: buffer,
                    to: ipV4Address,
                    createdAt: _dateTimeProvider.GetUtcNow(),
                    id: id,
                    channelId: networkHeader.ChannelId));
        }

        public unsafe void Disconnect(
            IpV4Address ipV4Address)
        {
            if (!ConnectionId.HasValue)
            {
                return;
            }

            ConnectionId = null;

            if (_connectionPool.TryGetConnection(ConnectionId.Value, connection: out var connection))
            {
                connection
                    .GetOutcomingChannel(ReliableChannel.Id)
                    .HandleOutputPacket(out var id, out var acks);

                var networkHeader = new NetworkHeader(
                    channelId: ReliableChannel.Id,
                    id: id,
                    acks: acks,
                    connectionId: connection.ConnectionId,
                    packetType: PacketType.Disconnect);

                var buffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer: buffer, value: networkHeader);

#if DEBUG
                _logger.Debug($"[UdpToolkit.Network] Disconnecting to: {ipV4Address} bytes length: {buffer.Length}, type: {networkHeader.PacketType}");
#endif

                _client.Send(ref ipV4Address, buffer, buffer.Length);

                _resendQueue.Add(
                    connection.ConnectionId,
                    new PendingPacket(
                        packetType: networkHeader.PacketType,
                        connectionId: connection.ConnectionId,
                        payload: buffer,
                        to: ipV4Address,
                        createdAt: _dateTimeProvider.GetUtcNow(),
                        id: id,
                        channelId: networkHeader.ChannelId));
            }
        }

        public unsafe void Heartbeat(
            IpV4Address ipV4Address)
        {
            if (!ConnectionId.HasValue)
            {
                return;
            }

            if (_connectionPool.TryGetConnection(ConnectionId.Value, connection: out var connection))
            {
#if DEBUG
                _logger.Debug($"[UdpToolkit.Network] Heartbeat from: {_client.GetLocalIp()} to: {ipV4Address}");
#endif

                connection
                    .GetOutcomingChannel(ReliableChannel.Id)
                    .HandleOutputPacket(out var id, out var acks);

                connection.OnHeartbeat(_dateTimeProvider.GetUtcNow());

                var networkHeader = new NetworkHeader(
                    channelId: ReliableChannel.Id,
                    id: id,
                    acks: acks,
                    connectionId: connection.ConnectionId,
                    packetType: PacketType.Heartbeat);

                var buffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer: buffer, value: networkHeader);

                ResendPackages(connection);
                _client.Send(ref ipV4Address, buffer, buffer.Length);
            }
        }

        public unsafe void Send(
            Guid connectionId,
            byte channelId,
            byte[] bytes,
            IpV4Address destination)
        {
            if (NetworkHeaderSize + bytes.Length > _settings.MtuSizeLimit)
            {
                _logger.Error($"[UdpToolkit.Network] Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            if (_connectionPool.TryGetConnection(connectionId, out var connection))
            {
                var channel = connection
                    .GetOutcomingChannel(channelId: channelId);

                channel
                    .HandleOutputPacket(out var id, out var acks);

                var networkHeader = new NetworkHeader(
                    channelId: channelId,
                    id: id,
                    acks: acks,
                    connectionId: connectionId,
                    packetType: PacketType.UserDefined);

                var nhBuffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer: nhBuffer, value: networkHeader);

                var buffer = new byte[NetworkHeaderSize + bytes.Length];
                Buffer.BlockCopy(nhBuffer, 0, buffer, 0, nhBuffer.Length);
                Buffer.BlockCopy(bytes, 0, buffer, nhBuffer.Length, bytes.Length);

#if DEBUG
                _logger.Debug($"[UdpToolkit.Network] Sending message to: {destination} bytes length: {buffer.Length}, type: {networkHeader.PacketType}");
#endif

                _client.Send(ref destination, buffer, buffer.Length);

                if (channel.IsReliable)
                {
                    _resendQueue.Add(
                        connection.ConnectionId,
                        new PendingPacket(
                            packetType: networkHeader.PacketType,
                            connectionId: connection.ConnectionId,
                            payload: bytes,
                            to: destination,
                            createdAt: _dateTimeProvider.GetUtcNow(),
                            id: id,
                            channelId: channelId));
                }
            }
        }

        public void StartReceive(
            CancellationToken cancellationToken)
        {
#if DEBUG
            _logger.Debug($"[UdpToolkit.Network] Start receive on ip: {_client.GetLocalIp()}");
#endif

            var remoteIp = new IpV4Address(0, 0);
            var buffer = new byte[_settings.UdpClientBufferSize];

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_client.Poll(_settings.PollFrequency) > 0)
                {
                    var receivedBytes = 0;
                    while ((receivedBytes = _client.ReceiveFrom(ref remoteIp, buffer, buffer.Length)) > 0)
                    {
                        if (receivedBytes < NetworkHeaderSize)
                        {
                            _logger.Error($"[UdpToolkit.Network] Invalid network header received from: {remoteIp}!");
                            continue;
                        }

                        ReceiveCallback(ref remoteIp, buffer, receivedBytes);
                    }
                }
            }
        }

        private unsafe void ReceiveCallback(
            ref IpV4Address remoteIp,
            byte[] buffer,
            int receivedBytes)
        {
            var headerSpan = new ArraySegment<byte>(buffer, 0, NetworkHeaderSize)
                .AsSpan();

            var payloadSpan = new ArraySegment<byte>(buffer, NetworkHeaderSize,  receivedBytes - NetworkHeaderSize)
                .AsSpan();

            var networkHeader = UnsafeSerialization.Read<NetworkHeader>(headerSpan);
#if DEBUG
            _logger.Debug($"[UdpToolkit.Network] Message received from: {remoteIp} bytes length: {receivedBytes}, type: {networkHeader.PacketType}");
#endif
            var connectionId = networkHeader.ConnectionId;

            if (!TryGetConnection(networkHeader, connectionId, remoteIp, out var connection))
            {
                return;
            }

            switch (networkHeader.PacketType)
            {
                case PacketType.Heartbeat when connection != null:
                    HandleIncomingPacket(connection, networkHeader, PacketType.Heartbeat, remoteIp);
                    ResendPackages(connection);

                    break;

                case PacketType.Heartbeat | PacketType.Ack when connection != null:
                    if (TryHandleAck(connection, networkHeader))
                    {
                        connection.OnHeartbeatAck(_dateTimeProvider.GetUtcNow());
                        OnHeartbeat?.Invoke(connectionId, connection.GetRtt());
                    }

                    break;

                case PacketType.Disconnect when connection != null:
                    HandleIncomingPacket(connection, networkHeader, PacketType.Disconnect, remoteIp);
                    _connectionPool.Remove(connection);

                    break;

                case PacketType.Disconnect | PacketType.Ack when connection != null:
                    if (TryHandleAck(connection, networkHeader))
                    {
                        OnDisconnected?.Invoke(remoteIp, connectionId);
                    }

                    break;

                case PacketType.Connect when connection != null:
                    HandleIncomingPacket(connection, networkHeader, PacketType.Connect, remoteIp);

                    break;

                case PacketType.Connect | PacketType.Ack when connection != null:
                    if (TryHandleAck(connection, networkHeader))
                    {
                        OnConnected?.Invoke(remoteIp, connectionId);
                    }

                    break;

                case PacketType.UserDefined when connection != null:
                    HandleIncomingPacket(connection, networkHeader, PacketType.UserDefined, remoteIp);
                    OnPacketReceived?.Invoke(remoteIp, connectionId, payloadSpan.ToArray(), networkHeader.ChannelId);

                    break;

                case PacketType.UserDefined | PacketType.Ack when connection != null:
                    TryHandleAck(connection, networkHeader);

                    break;

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {networkHeader.PacketType} - not supported!");
            }
        }

        private bool TryGetConnection(
            NetworkHeader networkHeader,
            Guid connectionId,
            IpV4Address remoteIp,
            out IConnection connection)
        {
            if (!_settings.AllowIncomingConnections && networkHeader.PacketType == PacketType.Connect)
            {
#if DEBUG
                _logger.Warning($"[UdpToolkit.Network] Attempt connect from: {remoteIp}, incoming connections not allowed");
#endif
                connection = null;
                return false;
            }

            if (networkHeader.PacketType == PacketType.Connect)
            {
                connection = _connectionPool.GetOrAdd(
                    connectionId: connectionId,
                    lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                    keepAlive: false,
                    ipV4Address: remoteIp);

                return true;
            }

            return _connectionPool.TryGetConnection(connectionId, out connection);
        }

        private bool TryHandleAck(
            IConnection connection,
            NetworkHeader networkHeader)
        {
            return connection
                .GetOutcomingChannel(networkHeader.ChannelId)
                .HandleAck(networkHeader.Id, networkHeader.Acks);
        }

        private unsafe void HandleIncomingPacket(
            IConnection connection,
            NetworkHeader networkHeader,
            PacketType packetType,
            IpV4Address remoteIp)
        {
            var incomingChannel = connection
                .GetIncomingChannel(channelId: networkHeader.ChannelId);

            var packetHandled = incomingChannel
                .HandleInputPacket(networkHeader.Id, networkHeader.Acks);

            if (!packetHandled)
            {
                if (incomingChannel.IsReliable)
                {
                    var ackPacket = new NetworkHeader(
                        channelId: networkHeader.ChannelId,
                        id: networkHeader.Id,
                        acks: networkHeader.Acks,
                        connectionId: networkHeader.ConnectionId,
                        packetType: packetType | PacketType.Ack);

                    var buffer = new byte[NetworkHeaderSize];
                    UnsafeSerialization.Write(buffer, ackPacket);

#if DEBUG
                    if (networkHeader.PacketType != PacketType.Heartbeat)
                    {
                        _logger.Debug(
                            $"[UdpToolkit.Network] Resend ack from: - {_client.GetLocalIp()} to: {connection.IpV4Address} packetId: {networkHeader.Id}, packetType {PacketType.Ack}, threadId - {Thread.CurrentThread.ManagedThreadId}");
                    }
#endif

                    var to = connection.IpV4Address;

                    _client.Send(ref to, buffer, buffer.Length);
                }

                return;
            }

            if (incomingChannel.IsReliable)
            {
                var ackPacket = new NetworkHeader(
                    channelId: networkHeader.ChannelId,
                    id: networkHeader.Id,
                    acks: networkHeader.Acks,
                    connectionId: networkHeader.ConnectionId,
                    packetType: packetType | PacketType.Ack);

                var buffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer, ackPacket);

#if DEBUG
                if (networkHeader.PacketType != PacketType.Heartbeat)
                {
                    _logger.Debug(
                        $"[UdpToolkit.Network] " +
                        $"Sended from: - {_client.GetLocalIp()} " +
                        $"to: {connection.IpV4Address} " +
                        $"packetId: {networkHeader.Id} " +
                        $"packetType: {ackPacket.PacketType} " +
                        $"threadId: - {Thread.CurrentThread.ManagedThreadId}");
                }
#endif

                _client.Send(ref remoteIp, buffer, buffer.Length);
            }
        }

        private void ResendPackages(
            IConnection connection)
        {
            var resendQueue = _resendQueue.Get(connection.ConnectionId);
            for (var i = 0; i < resendQueue.Count; i++)
            {
                var pendingPacket = resendQueue[i];

                var isDelivered = connection
                    .GetOutcomingChannel(pendingPacket.ChannelId)
                    .IsDelivered(pendingPacket.Id);

                var isExpired = _dateTimeProvider.GetUtcNow() - pendingPacket.CreatedAt > _settings.ResendTimeout;
                if (isDelivered || isExpired)
                {
#if DEBUG
                    if (isDelivered)
                    {
                        _logger.Debug($"[UdpToolkit.Network] Packet delivered {pendingPacket.Id}");
                    }

                    if (isExpired)
                    {
                        _logger.Debug($"[UdpToolkit.Network] Packet expired {pendingPacket.Id}");
                    }
#endif
                    resendQueue.RemoveAt(i);

                    if (pendingPacket.PacketType == PacketType.UserDefined)
                    {
                        OnPacketExpired?.Invoke(pendingPacket.To, pendingPacket.ConnectionId, pendingPacket.Payload, pendingPacket.ChannelId);
                    }

                    return;
                }

                var to = pendingPacket.To;

#if DEBUG
                _logger.Debug($"[UdpToolkit.Network] Resend packet {pendingPacket.Id}|{pendingPacket.PacketType} to {to} ");
#endif

                _client.Send(ref to, pendingPacket.Payload, pendingPacket.Payload.Length);
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
                // https://github.com/dotnet/runtime/issues/47342
                _client.Dispose();
                _connectionPool.Dispose();
            }

            _disposed = true;
        }
    }
}