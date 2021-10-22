namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Serialization;
    using UdpToolkit.Network.Utils;

    /// <inheritdoc />
    internal sealed unsafe class UdpClient : IUdpClient
    {
        private static readonly byte DummyDataType = 0;
        private static readonly int NetworkHeaderSize = sizeof(NetworkHeader);
        private readonly UdpClientSettings _settings;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ISocket _client;
        private readonly ILogger _logger;
        private readonly IConnectionPool _connectionPool;
        private readonly IResendQueue _resendQueue;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="connectionPool">Instance of connection pool.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        /// <param name="client">Instance of socket.</param>
        /// <param name="resendQueue">Instance of resend queue.</param>
        /// <param name="settings">Instance of settings.</param>
        internal UdpClient(
            IConnectionPool connectionPool,
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            ISocket client,
            IResendQueue resendQueue,
            UdpClientSettings settings)
        {
            _client = client;
            _connectionPool = connectionPool;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _resendQueue = resendQueue;
            _settings = settings;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UdpClient"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~UdpClient()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public event Action<IpV4Address, Guid, byte[], byte, byte> OnPacketReceived;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid, byte[], byte, byte> OnPacketDropped;

        /// <inheritdoc />
        public event Action<IpV4Address, byte[]> OnInvalidPacketReceived;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid, byte[], byte, byte> OnPacketExpired;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnConnected;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnDisconnected;

        /// <inheritdoc />
        public event Action<Guid, TimeSpan> OnHeartbeat;

        private Guid? ConnectionId { get; set; }

        /// <inheritdoc />
        public bool IsConnected(out Guid connectionId)
        {
            connectionId = ConnectionId ?? default;
            return ConnectionId.HasValue;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public unsafe void Connect(
            IpV4Address ipV4Address,
            Guid? connectionId)
        {
            ConnectionId = connectionId ?? Guid.NewGuid();

            var connection = _connectionPool.GetOrAdd(
                connectionId: ConnectionId.Value,
                keepAlive: true,
                lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                ipV4Address: _client.GetLocalIp());

            var networkHeader = connection
                .GetOutgoingChannel(ReliableChannel.Id)
                .HandleOutputPacket(DummyDataType, connection.ConnectionId, PacketType.Connect);

            var buffer = new byte[NetworkHeaderSize];
            UnsafeSerialization.Write(buffer: buffer, value: networkHeader);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Network] Connecting to: {ipV4Address} bytes length: {buffer.Length}, type: {networkHeader.PacketType}");
            }

            _client.Send(ref ipV4Address, buffer, buffer.Length);

            _resendQueue.Add(
                connectionId: networkHeader.ConnectionId,
                pendingPacket: new PendingPacket(
                    networkHeader: networkHeader,
                    payload: buffer,
                    to: ipV4Address,
                    createdAt: _dateTimeProvider.GetUtcNow()));
        }

        /// <inheritdoc />
        public unsafe void Disconnect(
            IpV4Address ipV4Address)
        {
            if (!ConnectionId.HasValue)
            {
                return;
            }

            if (_connectionPool.TryGetConnection(ConnectionId.Value, connection: out var connection))
            {
                var networkHeader = connection
                    .GetOutgoingChannel(ReliableChannel.Id)
                    .HandleOutputPacket(DummyDataType, connection.ConnectionId, PacketType.Disconnect);

                var buffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer: buffer, value: networkHeader);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[UdpToolkit.Network] Disconnecting to: {ipV4Address} bytes length: {buffer.Length}, type: {networkHeader.PacketType}");
                }

                _client.Send(ref ipV4Address, buffer, buffer.Length);

                _resendQueue.Add(
                    connectionId: connection.ConnectionId,
                    pendingPacket: new PendingPacket(
                        networkHeader: networkHeader,
                        payload: buffer,
                        to: ipV4Address,
                        createdAt: _dateTimeProvider.GetUtcNow()));
            }
        }

        /// <inheritdoc />
        public unsafe void Heartbeat(
            IpV4Address ipV4Address)
        {
            if (!ConnectionId.HasValue)
            {
                return;
            }

            if (_connectionPool.TryGetConnection(ConnectionId.Value, connection: out var connection))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[UdpToolkit.Network] Heartbeat from: {_client.GetLocalIp()} to: {ipV4Address}");
                }

                var networkHeader = connection
                    .GetOutgoingChannel(ReliableChannel.Id)
                    .HandleOutputPacket(DummyDataType, connection.ConnectionId, PacketType.Heartbeat);

                connection.OnHeartbeat(_dateTimeProvider.GetUtcNow());

                var buffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer: buffer, value: networkHeader);

                ResendPackages(connection);
                _client.Send(ref ipV4Address, buffer, buffer.Length);
            }
        }

        /// <inheritdoc />
        public unsafe void Send(
            Guid connectionId,
            byte channelId,
            byte dataType,
            byte[] bytes,
            IpV4Address ipV4Address)
        {
            if (NetworkHeaderSize + bytes.Length > _settings.MtuSizeLimit)
            {
                _logger.Error($"[UdpToolkit.Network] Udp packet oversize mtu limit - {bytes.Length}");

                OnPacketDropped?.Invoke(ipV4Address, connectionId, bytes, channelId, dataType);

                return;
            }

            if (_connectionPool.TryGetConnection(connectionId, out var connection))
            {
                var channel = connection
                    .GetOutgoingChannel(channelId: channelId);

                var networkHeader = channel
                    .HandleOutputPacket(dataType, connection.ConnectionId, PacketType.UserDefined);

                var nhBuffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer: nhBuffer, value: networkHeader);

                var buffer = new byte[NetworkHeaderSize + bytes.Length];
                Buffer.BlockCopy(nhBuffer, 0, buffer, 0, nhBuffer.Length);
                Buffer.BlockCopy(bytes, 0, buffer, nhBuffer.Length, bytes.Length);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[UdpToolkit.Network] Sending message to: {ipV4Address} bytes length: {buffer.Length}, type: {networkHeader.PacketType}");
                }

                _client.Send(ref ipV4Address, buffer, buffer.Length);

                if (channel.ResendOnHeartbeat)
                {
                    _resendQueue.Add(
                        connectionId: connection.ConnectionId,
                        pendingPacket: new PendingPacket(
                            networkHeader: networkHeader,
                            payload: bytes,
                            to: ipV4Address,
                            createdAt: _dateTimeProvider.GetUtcNow()));
                }
            }
        }

        /// <inheritdoc />
        public void StartReceive(
            CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Network] Start receive on ip: {_client.GetLocalIp()}");
            }

            var remoteIp = new IpV4Address(0, 0);
            var buffer = new byte[_settings.UdpClientBufferSize];

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _client.Poll(_settings.PollFrequency);
                if (result > 0)
                {
                    var receivedBytes = 0;
                    while ((receivedBytes = _client.ReceiveFrom(ref remoteIp, buffer, buffer.Length)) > 0)
                    {
                        if (receivedBytes < NetworkHeaderSize)
                        {
                            _logger.Error($"[UdpToolkit.Network] Invalid network header received from: {remoteIp}!");

                            OnInvalidPacketReceived?.Invoke(remoteIp, buffer);

                            continue;
                        }

                        ReceiveCallback(ref remoteIp, buffer, receivedBytes);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IpV4Address GetLocalIp()
        {
            return _client.GetLocalIp();
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
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Network] Message received from: {remoteIp} bytes length: {receivedBytes}, type: {networkHeader.PacketType}");
            }

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
                    connection.OnHeartbeat(_dateTimeProvider.GetUtcNow());

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
                        ConnectionId = null;
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
                    OnPacketReceived?.Invoke(remoteIp, connectionId, payloadSpan.ToArray(), networkHeader.ChannelId, networkHeader.DataType);

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
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Warning($"[UdpToolkit.Network] Attempt connect from: {remoteIp}, incoming connections not allowed");
                }

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
            in NetworkHeader networkHeader)
        {
            return connection
                .GetOutgoingChannel(networkHeader.ChannelId)
                .HandleAck(networkHeader);
        }

        private unsafe void HandleIncomingPacket(
            IConnection connection,
            in NetworkHeader networkHeader,
            PacketType packetType,
            IpV4Address remoteIp)
        {
            var incomingChannel = connection
                .GetIncomingChannel(channelId: networkHeader.ChannelId);

            var packetHandled = incomingChannel
                .HandleInputPacket(networkHeader);

            if (!packetHandled)
            {
                if (incomingChannel.ResendOnHeartbeat)
                {
                    var ackPacket = new NetworkHeader(
                        channelId: networkHeader.ChannelId,
                        id: networkHeader.Id,
                        acks: networkHeader.Acks,
                        connectionId: networkHeader.ConnectionId,
                        packetType: packetType | PacketType.Ack,
                        dataType: networkHeader.DataType);

                    var buffer = new byte[NetworkHeaderSize];
                    UnsafeSerialization.Write(buffer, ackPacket);

                    if (_logger.IsEnabled(LogLevel.Debug) && networkHeader.PacketType != PacketType.Heartbeat)
                    {
                        _logger.Debug(
                            $"[UdpToolkit.Network] Resend ack from: - {_client.GetLocalIp()} to: {connection.IpV4Address} packetId: {networkHeader.Id}, packetType {PacketType.Ack}, threadId - {Thread.CurrentThread.ManagedThreadId}");
                    }

                    var to = connection.IpV4Address;

                    _client.Send(ref to, buffer, buffer.Length);
                }

                return;
            }

            if (incomingChannel.ResendOnHeartbeat)
            {
                var ackPacket = new NetworkHeader(
                    channelId: networkHeader.ChannelId,
                    id: networkHeader.Id,
                    acks: networkHeader.Acks,
                    connectionId: networkHeader.ConnectionId,
                    packetType: packetType | PacketType.Ack,
                    dataType: networkHeader.DataType);

                var buffer = new byte[NetworkHeaderSize];
                UnsafeSerialization.Write(buffer, ackPacket);

                if (_logger.IsEnabled(LogLevel.Debug) && networkHeader.PacketType != PacketType.Heartbeat)
                {
                    _logger.Debug(
                        $"[UdpToolkit.Network] " +
                        $"Sended from: - {_client.GetLocalIp()} " +
                        $"to: {connection.IpV4Address} " +
                        $"packetId: {networkHeader.Id} " +
                        $"packetType: {ackPacket.PacketType} " +
                        $"threadId: - {Thread.CurrentThread.ManagedThreadId}");
                }

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
                    .GetOutgoingChannel(pendingPacket.NetworkHeader.ChannelId)
                    .IsDelivered(pendingPacket.NetworkHeader);

                var isExpired = _dateTimeProvider.GetUtcNow() - pendingPacket.CreatedAt > _settings.ResendTimeout;
                if (isDelivered || isExpired)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        if (isDelivered)
                        {
                            _logger.Debug($"[UdpToolkit.Network] Packet delivered {pendingPacket.NetworkHeader.Id}");
                        }

                        if (isExpired)
                        {
                            _logger.Debug($"[UdpToolkit.Network] Packet expired {pendingPacket.NetworkHeader.Id}");
                        }
                    }

                    resendQueue.RemoveAt(i);

                    if (pendingPacket.NetworkHeader.PacketType == PacketType.UserDefined)
                    {
                        OnPacketExpired?.Invoke(pendingPacket.To, pendingPacket.NetworkHeader.ConnectionId, pendingPacket.Payload, pendingPacket.NetworkHeader.ChannelId, pendingPacket.NetworkHeader.DataType);
                    }

                    return;
                }

                var to = pendingPacket.To;

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[UdpToolkit.Network] Resend packet {pendingPacket.NetworkHeader.Id}|{pendingPacket.NetworkHeader.PacketType} to {to} ");
                }

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