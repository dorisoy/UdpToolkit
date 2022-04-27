namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Buffers;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Serialization;
    using UdpToolkit.Network.Utils;

    /// <inheritdoc />
    internal sealed class UdpClient : IUdpClient
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ISocket _client;
        private readonly ILogger _logger;
        private readonly IConnectionPool _connectionPool;

        private readonly BlockingCollection<IConnection> _resendRequests = new BlockingCollection<IConnection>(new ConcurrentQueue<IConnection>());
        private readonly ConcurrentPool<NetworkPacket> _packetsPool;
        private readonly ArrayPool<byte> _arrayPool;

        private readonly UdpClientSettings _settings;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClient"/> class.
        /// </summary>
        /// <param name="connectionPool">Instance of connection pool.</param>
        /// <param name="logger">Instance of logger.</param>
        /// <param name="dateTimeProvider">Instance of date time provider.</param>
        /// <param name="client">Instance of socket.</param>
        /// <param name="settings">Instance of settings.</param>
        /// <param name="packetsPool">Pool of network packets.</param>
        /// <param name="arrayPool">Array pool.</param>
        internal UdpClient(
            IConnectionPool connectionPool,
            ILogger logger,
            IDateTimeProvider dateTimeProvider,
            ISocket client,
            UdpClientSettings settings,
            ConcurrentPool<NetworkPacket> packetsPool,
            ArrayPool<byte> arrayPool)
        {
            _client = client;
            _connectionPool = connectionPool;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _settings = settings;
            _packetsPool = packetsPool;
            _arrayPool = arrayPool;

            // TODO move count of resend jobs to config
            for (int i = 0; i < 2; i++)
            {
                Task.Factory.StartNew(
                    action: ResendPendingPackets,
                    cancellationToken: default,
                    creationOptions: TaskCreationOptions.LongRunning,
                    scheduler: TaskScheduler.Current);
            }
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
        public event Action<NetworkPacket> OnPacketReceived;

        /// <inheritdoc />
        public event Action<NetworkPacket> OnPacketDropped;

        /// <inheritdoc />
        public event Action<NetworkPacket> OnInvalidPacketReceived;

        /// <inheritdoc />
        public event Action<NetworkPacket> OnPacketExpired;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnConnected;

        /// <inheritdoc />
        public event Action<IpV4Address, Guid> OnDisconnected;

        /// <inheritdoc />
        public event Action<Guid, TimeSpan> OnHeartbeat;

        private enum PoolingStrategy : byte
        {
            Return = 0,
            FireAndForget = 1,
        }

        private Guid ConnectionId { get; set; }

        /// <inheritdoc />
        public bool IsConnected(out Guid connectionId)
        {
            if (ConnectionId == default)
            {
                return false;
            }

            connectionId = ConnectionId;
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Connect(
            IpV4Address ipV4Address,
            Guid connectionId)
        {
            ConnectionId = connectionId;
            var buffer = _arrayPool.Rent(Consts.NetworkHeaderSize);
            try
            {
                var connection = _connectionPool.GetOrAdd(
                    connectionId: ConnectionId,
                    keepAlive: true,
                    lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                    ipV4Address: _client.GetLocalIp());

                if (connection.GetOutgoingChannel(ReliableChannel.Id, out var channel))
                {
                    SendProtocolPacketInternal(
                        connection,
                        channel,
                        ipV4Address,
                        buffer,
                        PacketType.Connect,
                        byte.MaxValue);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.Error($"[UdpToolkit.Network] Channel with id - {ReliableChannel.Id} does not exists!");
                    }

                    _arrayPool.Return(buffer);
                }
            }
            catch (Exception ex)
            {
                _arrayPool.Return(buffer);
                ConnectionId = default;
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.Error($"[UdpToolkit.Network] An error occurred while trying connecting {ex}!");
                }

                throw;
            }
        }

        /// <inheritdoc />
        public void Disconnect(
            IpV4Address ipV4Address)
        {
            var buffer = _arrayPool.Rent(Consts.NetworkHeaderSize);
            try
            {
                if (ExistsBothOut(ConnectionId, ReliableChannel.Id, out var connection, out var channel))
                {
                    SendProtocolPacketInternal(
                        connection,
                        channel,
                        ipV4Address,
                        buffer,
                        PacketType.Disconnect,
                        byte.MaxValue);
                }
                else
                {
                    _arrayPool.Return(buffer);
                }
            }
            catch (Exception ex)
            {
                _arrayPool.Return(buffer);
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.Error($"[UdpToolkit.Network] An error occurred while trying to disconnecting {ex}!");
                }

                throw;
            }
        }

        /// <inheritdoc />
        public void Heartbeat(
            IpV4Address ipV4Address)
        {
            var buffer = _arrayPool.Rent(Consts.NetworkHeaderSize);
            try
            {
                if (ExistsBothOut(ConnectionId, SequencedChannel.Id, out var connection, out var channel))
                {
                    connection.OnHeartbeat(_dateTimeProvider.GetUtcNow());
                    SendProtocolPacketInternal(
                        connection: connection,
                        channel: channel,
                        ipV4Address: ipV4Address,
                        buffer: buffer,
                        packetType: PacketType.Heartbeat,
                        dataType: byte.MaxValue);
                    _resendRequests.Add(connection);
                }
                else
                {
                    _arrayPool.Return(buffer);
                }
            }
            catch (Exception ex)
            {
                _arrayPool.Return(buffer);
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.Error($"[UdpToolkit.Network] An error occurred while trying sent heartbeat {ex}!");
                }

                throw;
            }
        }

        /// <inheritdoc />
        public void Send(
            Guid connectionId,
            byte channelId,
            byte dataType,
            ReadOnlySpan<byte> payload,
            IpV4Address ipV4Address)
        {
            var packetLength = Consts.NetworkHeaderSize + payload.Length;
            if (packetLength > _settings.MtuSizeLimit)
            {
                _logger.Error($"[UdpToolkit.Network] Udp packet oversize mtu limit - {payload.Length}");

                if (OnPacketDropped == null)
                {
                    return;
                }

                var droppedBytes = _arrayPool.Rent(packetLength);
                payload.CopyTo(droppedBytes);
                var networkPacket = _packetsPool.GetOrCreate();
                networkPacket.Setup(
                    buffer: droppedBytes,
                    ipV4: ipV4Address,
                    connectionId: connectionId,
                    channelId: channelId,
                    dataType: dataType,
                    bytesReceived: default,
                    isExpired: false);
                OnPacketDropped.Invoke(networkPacket);

                return;
            }

            var buffer = _arrayPool.Rent(packetLength);
            try
            {
                if (ExistsBothOut(connectionId, channelId, out var connection, out var channel))
                {
                    var strategy = SendPacketInternal(
                        connection: connection,
                        channel: channel,
                        dataType: dataType,
                        payload: payload,
                        ipV4Address: ipV4Address,
                        packetLength: packetLength,
                        buffer: buffer);

                    if (strategy == PoolingStrategy.Return)
                    {
                        _arrayPool.Return(buffer);
                    }
                }
                else
                {
                    _arrayPool.Return(buffer);
                }
            }
            catch (Exception ex)
            {
                _arrayPool.Return(buffer);
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.Error($"[UdpToolkit.Network] An error occurred while trying sent packet {ex}!");
                }

                throw;
            }
        }

        /// <inheritdoc />
        public void StartReceive(
            CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var ipV4Address = _client.GetLocalIp();
                _logger.Debug($"[UdpToolkit.Network] Start receive on ip: {IpUtils.ToString(ipV4Address.Address)}:{ipV4Address.Port}");
            }

            var buffer = new byte[_settings.UdpClientBufferSize];
            var remoteIp = new IpV4Address(0, 0);
            while (!cancellationToken.IsCancellationRequested)
            {
                Poll(_settings.PollFrequency, ref remoteIp, buffer);
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Poll(
            int timeout,
            ref IpV4Address remoteIp,
            byte[] buffer)
        {
            var result = _client.Poll(timeout);
            if (result > 0)
            {
                var receivedBytes = 0;
                while ((receivedBytes = _client.ReceiveFrom(ref remoteIp, buffer, buffer.Length)) > 0)
                {
                    if (receivedBytes < Consts.NetworkHeaderSize)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.Error($"[UdpToolkit.Network] Invalid network header received from: {remoteIp}!");
                        }

                        if (OnInvalidPacketReceived == null)
                        {
                            continue;
                        }

                        var networkPacket = _packetsPool.GetOrCreate();

                        var invalidPacketBuffer = _arrayPool.Rent(32);
                        var invalidSpan = buffer.AsSpan().Slice(0, receivedBytes);
                        invalidSpan.CopyTo(invalidPacketBuffer); // TODO - avoid copy

                        networkPacket.Setup(
                            buffer: invalidPacketBuffer,
                            ipV4: remoteIp,
                            connectionId: default,
                            channelId: default,
                            dataType: default,
                            bytesReceived: receivedBytes,
                            isExpired: false);

                        OnInvalidPacketReceived.Invoke(networkPacket);

                        continue;
                    }

                    var bufferSpan = buffer.AsSpan();
                    var headerSpan = bufferSpan.Slice(0, Consts.NetworkHeaderSize);
                    var networkHeader = UnsafeSerialization.Deserialize<NetworkHeader>(headerSpan);

                    var payloadBuffer = _arrayPool.Rent(_settings.UdpClientBufferSize);
                    try
                    {
                        bufferSpan.Slice(25, receivedBytes).CopyTo(payloadBuffer); // TODO - avoid copy
                        var strategy = ReceiveCallback(
                            remoteIp: remoteIp,
                            networkHeader: networkHeader,
                            receivedBytes: receivedBytes,
                            buffer: payloadBuffer);

                        if (strategy == PoolingStrategy.Return)
                        {
                            _arrayPool.Return(payloadBuffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        _arrayPool.Return(payloadBuffer);
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.Error($"[UdpToolkit.Network] Error occured while receive data {ex}!");
                        }

                        throw;
                    }
                }
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IpV4Address GetLocalIp()
        {
            return _client.GetLocalIp();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendProtocolPacketInternal(
            IConnection connection,
            IChannel channel,
            IpV4Address ipV4Address,
            byte[] buffer,
            PacketType packetType,
            byte dataType)
        {
            var bufferSpan = buffer
                .AsSpan()
                .Slice(0, Consts.NetworkHeaderSize);

            var networkHeader = new NetworkHeader(
                channelId: channel.ChannelId,
                id: channel.HandleOutputPacket(dataType),
                acks: default,
                connectionId: connection.ConnectionId,
                packetType: packetType,
                dataType: dataType);

            UnsafeSerialization.Serialize(bufferSpan, networkHeader);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Network] {packetType} sent to: {IpUtils.ToString(ipV4Address.Address)}:{ipV4Address.Port} bytes length: {bufferSpan.Length}");
            }

            if (channel.IsReliable)
            {
                connection.PendingPackets.Add(new PendingPacket(
                    ipV4Address: ipV4Address,
                    buffer: buffer,
                    payloadLength: bufferSpan.Length,
                    createdAt: _dateTimeProvider.GetUtcNow(),
                    channel: channel,
                    id: networkHeader.Id));
            }

            _client.Send(ref ipV4Address, buffer, bufferSpan.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendAckPacket(
            IChannel channel,
            IpV4Address ipV4Address,
            PacketType packetType,
            in NetworkHeader inNetworkHeader)
        {
            var buffer = _arrayPool.Rent(_settings.UdpClientBufferSize);
            try
            {
                var bufferSpan = buffer
                    .AsSpan()
                    .Slice(0, Consts.NetworkHeaderSize);

                var ackType = packetType | PacketType.Ack;
                var networkHeader = new NetworkHeader(
                    channelId: channel.ChannelId,
                    id: inNetworkHeader.Id,
                    acks: default,
                    connectionId: inNetworkHeader.ConnectionId,
                    packetType: ackType,
                    dataType: inNetworkHeader.DataType);

                UnsafeSerialization.Serialize(bufferSpan, networkHeader);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"[UdpToolkit.Network] {ackType} sent to: {IpUtils.ToString(ipV4Address.Address)}:{ipV4Address.Port} bytes length: {bufferSpan.Length}");
                }

                _client.Send(ref ipV4Address, buffer, bufferSpan.Length);
            }
            finally
            {
                _arrayPool.Return(buffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PoolingStrategy SendPacketInternal(
            IConnection connection,
            IChannel channel,
            byte dataType,
            ReadOnlySpan<byte> payload,
            IpV4Address ipV4Address,
            int packetLength,
            byte[] buffer)
        {
            var bufferSpan = buffer.AsSpan();
            var networkHeaderSpan = bufferSpan.Slice(0, Consts.NetworkHeaderSize);
            var packetSpan = bufferSpan.Slice(Consts.NetworkHeaderSize, payload.Length);
            var networkPacketSpan = bufferSpan.Slice(0, packetLength);

            payload.CopyTo(packetSpan);

            var networkPacketId = channel
                .HandleOutputPacket(dataType);

            var networkHeader = new NetworkHeader(
                channelId: channel.ChannelId,
                id: networkPacketId,
                acks: default,
                connectionId: connection.ConnectionId,
                packetType: PacketType.UserDefined,
                dataType: dataType);

            UnsafeSerialization.Serialize(buffer: networkHeaderSpan, value: networkHeader);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.Debug($"[UdpToolkit.Network] Sending message to: {IpUtils.ToString(ipV4Address.Address)}:{ipV4Address.Port} bytes length: {packetLength}, type: {networkHeader.PacketType}");
            }

            _client.Send(ref ipV4Address, buffer, networkPacketSpan.Length);

            if (channel.IsReliable)
            {
                connection.PendingPackets.Add(new PendingPacket(
                    ipV4Address: ipV4Address,
                    buffer: buffer,
                    payloadLength: bufferSpan.Length,
                    createdAt: _dateTimeProvider.GetUtcNow(),
                    channel: channel,
                    id: networkHeader.Id));
            }

            return channel.IsReliable
                ? PoolingStrategy.FireAndForget
                : PoolingStrategy.Return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PoolingStrategy ReceiveCallback(
            in IpV4Address remoteIp,
            in NetworkHeader networkHeader,
            int receivedBytes,
            byte[] buffer)
        {
            var connectionId = networkHeader.ConnectionId;
            var channelId = networkHeader.ChannelId;

            switch (networkHeader.PacketType)
            {
                case PacketType.Heartbeat when ExistsBothIn(connectionId, channelId, out var connection, out var channel):
                {
                    if (channel.HandleInputPacket(networkHeader))
                    {
                        connection.OnHeartbeat(_dateTimeProvider.GetUtcNow());
                    }

                    _resendRequests.Add(connection);

                    // TODO Heartbeat not reliable
                    SendAckPacket(
                        channel: channel,
                        ipV4Address: remoteIp,
                        packetType: networkHeader.PacketType,
                        inNetworkHeader: networkHeader);

                    break;
                }

                case PacketType.Heartbeat | PacketType.Ack when ExistsBothOut(connectionId, channelId, out var connection, out var channel):
                {
                    if (channel.HandleAck(networkHeader))
                    {
                        connection.OnHeartbeatAck(_dateTimeProvider.GetUtcNow());
                        OnHeartbeat?.Invoke(connection.ConnectionId, connection.GetRtt());
                    }

                    break;
                }

                case PacketType.Disconnect when ExistsBothIn(connectionId, channelId, out var connection, out var channel):
                {
                    if (channel.HandleInputPacket(networkHeader))
                    {
                        _connectionPool.Remove(connection);
                    }

                    SendAckPacket(
                        channel: channel,
                        ipV4Address: remoteIp,
                        packetType: networkHeader.PacketType,
                        inNetworkHeader: networkHeader);

                    break;
                }

                case PacketType.Disconnect | PacketType.Ack when ExistsBothOut(connectionId, channelId, out var connection, out var channel):
                {
                    if (channel.HandleAck(networkHeader))
                    {
                        ConnectionId = default;
                        OnDisconnected?.Invoke(remoteIp, connection.ConnectionId);
                    }

                    break;
                }

                case PacketType.Connect:
                {
                    if (!_settings.AllowIncomingConnections && networkHeader.PacketType == PacketType.Connect)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.Warning($"[UdpToolkit.Network] Attempt connect from: {remoteIp}, incoming connections not allowed");
                        }

                        break;
                    }

                    var newConnection = _connectionPool.GetOrAdd(
                        connectionId: networkHeader.ConnectionId,
                        lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                        keepAlive: false,
                        ipV4Address: remoteIp);

                    if (!newConnection.GetIncomingChannel(networkHeader.ChannelId, out var inChannel))
                    {
                        break;
                    }

                    if (!newConnection.GetOutgoingChannel(networkHeader.ChannelId, out var outChannel))
                    {
                        break;
                    }

                    inChannel.HandleInputPacket(networkHeader);

                    SendAckPacket(
                        channel: inChannel,
                        ipV4Address: remoteIp,
                        packetType: networkHeader.PacketType,
                        inNetworkHeader: networkHeader);

                    break;
                }

                case PacketType.Connect | PacketType.Ack when ExistsBothOut(connectionId, channelId, out var connection, out var channel):
                {
                    if (channel.HandleAck(networkHeader))
                    {
                        OnConnected?.Invoke(remoteIp, connection.ConnectionId);
                    }

                    break;
                }

                case PacketType.UserDefined when ExistsBothIn(connectionId, channelId, out var connection, out var channel):
                {
                    channel.HandleInputPacket(networkHeader);
                    SendAckPacket(
                        channel: channel,
                        ipV4Address: remoteIp,
                        packetType: networkHeader.PacketType,
                        inNetworkHeader: networkHeader);

                    if (OnPacketReceived == null)
                    {
                        break;
                    }

                    var networkPacket = _packetsPool.GetOrCreate();
                    networkPacket.Setup(
                        buffer: buffer,
                        ipV4: remoteIp,
                        connectionId: connection.ConnectionId,
                        channelId: networkHeader.ChannelId,
                        dataType: networkHeader.DataType,
                        bytesReceived: receivedBytes - Consts.NetworkHeaderSize,
                        isExpired: false);

                    OnPacketReceived.Invoke(networkPacket);

                    return PoolingStrategy.FireAndForget;
                }

                case PacketType.UserDefined | PacketType.Ack when ExistsBothOut(connectionId, channelId, out var connection, out var channel):
                {
                    channel.HandleAck(networkHeader);

                    break;
                }
            }

            return PoolingStrategy.Return;
        }

        private void ResendPendingPackets()
        {
            foreach (var connection in _resendRequests.GetConsumingEnumerable())
            {
                // TODO limit to resend
                for (var i = 0; i < connection.PendingPackets.Count; i++)
                {
                    var pendingPacket = connection.PendingPackets[i];

                    // TODO resend packet timeout
                    var isExpired = _dateTimeProvider.GetUtcNow() - pendingPacket.CreatedAt > TimeSpan.FromSeconds(15);
                    var isDelivered = pendingPacket.Channel.IsDelivered(pendingPacket.Id);
                    if (isExpired || isDelivered)
                    {
                        _arrayPool.Return(pendingPacket.Buffer);
                        connection.PendingPackets.RemoveAt(i);
                        if (isExpired)
                        {
                            OnPacketExpired?.Invoke(default);
                        }
                    }
                    else
                    {
                        var ip = pendingPacket.IpV4Address;
                        _client.Send(ref ip, pendingPacket.Buffer, pendingPacket.PayloadLength);
                    }
                }
            }
        }

        private bool ExistsBothIn(
            Guid connectionId,
            byte channelId,
            out IConnection connection,
            out IChannel channel)
        {
            channel = default;
            return _connectionPool.TryGetConnection(connectionId, out connection) &&
                   connection.GetIncomingChannel(channelId, out channel);
        }

        private bool ExistsBothOut(
            Guid connectionId,
            byte channelId,
            out IConnection connection,
            out IChannel channel)
        {
            channel = default;
            return _connectionPool.TryGetConnection(connectionId, out connection) &&
                   connection.GetOutgoingChannel(channelId, out channel);
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