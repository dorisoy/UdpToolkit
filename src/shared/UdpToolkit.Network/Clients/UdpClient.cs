namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Protocol;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Utils;

    internal sealed class UdpClient : IUdpClient
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ISocket _client;
        private readonly IUdpToolkitLogger _logger;
        private readonly IConnectionPool _connectionPool;
        private readonly NetworkSettings _networkSettings;

        private readonly IResendQueue _resendQueue;

        private bool _disposed = false;

        internal UdpClient(
            IConnectionPool connectionPool,
            IUdpToolkitLogger logger,
            IDateTimeProvider dateTimeProvider,
            ISocket client,
            IResendQueue resendQueue,
            NetworkSettings networkSettings)
        {
            _client = client;
            _connectionPool = connectionPool;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _resendQueue = resendQueue;
            _networkSettings = networkSettings;
        }

        ~UdpClient()
        {
            Dispose(false);
        }

        public event Action<InPacket> OnPacketReceived;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Send(
            OutPacket outPacket)
        {
            if (!_connectionPool.TryGetConnection(outPacket.ConnectionId, out var connection))
            {
                return;
            }

            var packetType = outPacket.PacketType;

            if (ProtocolUtils.IsProtocolEvent(outPacket.HookId))
            {
                switch ((ProtocolHookId)outPacket.HookId)
                {
                    case ProtocolHookId.P2P:
                        break;
                    case ProtocolHookId.Heartbeat when packetType == PacketType.Protocol:
                        connection.OnHeartbeat(_dateTimeProvider.GetUtcNow());
                        ResendPackages(connection);

                        break;
                    case ProtocolHookId.Heartbeat when packetType == PacketType.Ack:
                        connection.OnHeartbeatAck(_dateTimeProvider.GetUtcNow());

                        break;
                    case ProtocolHookId.Disconnect:
                    case ProtocolHookId.Connect:
                    case ProtocolHookId.Connect2Peer:
                    case ProtocolHookId.Heartbeat:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(OutPacket.HookId), outPacket.HookId.ToString(), null);
                }
            }

            var channel = connection.GetOutcomingChannel(channelId: outPacket.ChannelId);
            channel.HandleOutputPacket(out var id, out var acks);

            var bytes = OutPacket.Serialize(id, acks, ref outPacket);

            if (bytes.Length > _networkSettings.MtuSizeLimit)
            {
                _logger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            if (outPacket.HookId != (byte)ProtocolHookId.Heartbeat)
            {
                _logger.Debug(
                    $"Sended from: - {_client.GetLocalIp()} to: {outPacket.Destination} packetId: {id} channel: {outPacket.ChannelId} hookId: {outPacket.HookId} packetType {outPacket.PacketType} threadId - {Thread.CurrentThread.ManagedThreadId}");
            }

            if (channel.IsReliable && outPacket.HookId != (byte)ProtocolHookId.Heartbeat)
            {
                if (packetType == PacketType.Ack)
                {
                    throw new NotSupportedException();
                }

                _resendQueue.Add(connection.ConnectionId, new PendingPacket(
                    hookId: outPacket.HookId,
                    payload: bytes,
                    to: outPacket.Destination,
                    createdAt: outPacket.CreatedAt,
                    id: id,
                    channelId: outPacket.ChannelId));
            }

            var ipAddress = outPacket.Destination;

            _client.Send(ref ipAddress, bytes, bytes.Length);
        }

        public void Receive(
            CancellationToken cancellationToken)
        {
            _logger.Debug($"Start receiving on ip: {_client.GetLocalIp()}");
            var remoteIp = new IpV4Address
            {
                Port = 0,
                Address = 0,
            };
            var buffer = new byte[_networkSettings.UdpClientBufferSize];

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_client.Poll(15) > 0)
                {
                    var receivedBytes = 0;

                    while ((receivedBytes = _client.ReceiveFrom(ref remoteIp, buffer, buffer.Length)) > 0)
                    {
                        ReceiveCallback(ref remoteIp, buffer, receivedBytes);
                        _logger.Debug($"Message received from - IP: {remoteIp} bytes length: {receivedBytes}");
                    }
                }
            }
        }

        public TimeSpan? GetRtt(Guid connectionId)
        {
            if (_connectionPool.TryGetConnection(connectionId, out var connection))
            {
                return connection.GetRtt();
            }

            return null;
        }

        private void ReceiveCallback(
            ref IpV4Address remoteIp,
            byte[] memory,
            int bytesReceived)
        {
            var inPacket = InPacket.Deserialize(
                bytes: memory,
                ipEndPoint: remoteIp.ToIpEndPoint(),
                bytesReceived: bytesReceived,
                out var id,
                out var acks);

            var connectionId = inPacket.ConnectionId;
            var isConnectionRequest = (ProtocolHookId)inPacket.HookId == ProtocolHookId.Connect || (ProtocolHookId)inPacket.HookId == ProtocolHookId.Connect2Peer;
            if (!_connectionPool.TryGetConnection(connectionId, out var connection) && !isConnectionRequest)
            {
                _logger.Debug($"Connection - {connectionId} dropped!");
                return;
            }

            var packetType = inPacket.PacketType;

            if (ProtocolUtils.IsProtocolEvent(inPacket.HookId))
            {
                switch ((ProtocolHookId)inPacket.HookId)
                {
                    case ProtocolHookId.P2P:
                        break;

                    case ProtocolHookId.Heartbeat when packetType == PacketType.Protocol:
                        connection?.OnHeartbeat(_dateTimeProvider.GetUtcNow());

                        break;

                    case ProtocolHookId.Heartbeat when packetType == PacketType.Ack:
                        connection?.OnHeartbeatAck(_dateTimeProvider.GetUtcNow());

                        break;

                    case ProtocolHookId.Disconnect when packetType == PacketType.Protocol:
                        _connectionPool.Remove(connection);

                        break;
                    case ProtocolHookId.Connect when packetType == PacketType.Protocol:
                    case ProtocolHookId.Connect2Peer when packetType == PacketType.Protocol:
                        connection = _connectionPool.GetOrAdd(
                            connectionId: connectionId,
                            lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                            keepAlive: false,
                            ipV4Address: remoteIp);

                        break;
                }
            }

            if (connection == null)
            {
                _logger.Debug($"Connection - not found!");
                return;
            }

            if (inPacket.HookId != (byte)ProtocolHookId.Heartbeat)
            {
                _logger.Debug(
                    $"Received from: - {remoteIp} to: {_client.GetLocalIp()} packetId: {id} hookId: {inPacket.HookId} packetType {inPacket.PacketType}");
            }

            switch (inPacket.PacketType)
            {
                case PacketType.FromServer:
                case PacketType.FromClient:
                case PacketType.Protocol:
                    var incomingChannel = connection.GetIncomingChannel(channelId: inPacket.ChannelId);
                    var protocolHandled = incomingChannel.HandleInputPacket(id, acks);

                    if (!protocolHandled)
                    {
                        if (incomingChannel.IsReliable)
                        {
                            var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                            if (inPacket.HookId != (byte)ProtocolHookId.Heartbeat)
                            {
                                _logger.Debug(
                                    $"Resend ack from: - {_client.GetLocalIp()} to: {connection.IpAddress} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}, threadId - {Thread.CurrentThread.ManagedThreadId}");
                            }

                            var to = connection.IpAddress;

                            _client.Send(ref to, bytes, bytes.Length);
                        }

                        return;
                    }

                    if (incomingChannel.IsReliable)
                    {
                        var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                        if (inPacket.HookId != (byte)ProtocolHookId.Heartbeat)
                        {
                            _logger.Debug(
                                $"Sended from: - {_client.GetLocalIp()} to: {inPacket.IpAddress} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack} threadId - {Thread.CurrentThread.ManagedThreadId}");
                        }

                        var to = inPacket.IpAddress;

                        _client.Send(ref to, bytes, bytes.Length);
                    }

                    OnPacketReceived?.Invoke(inPacket);
                    break;

                case PacketType.Ack:
                    var ackHandled = connection
                        .GetOutcomingChannel(inPacket.ChannelId)
                        .HandleAck(id, acks);

                    if (ackHandled)
                    {
                        OnPacketReceived?.Invoke(inPacket);
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {inPacket.PacketType} - not supported!");
            }
        }

        private void ResendPackages(
            IConnection connection)
        {
            _logger.Debug("Heartbeat");
            var resendQueue = _resendQueue.Get(connection.ConnectionId);
            for (var i = 0; i < resendQueue.Count; i++)
            {
                var pendingPacket = resendQueue[i];

                var isDelivered = connection
                    .GetOutcomingChannel(pendingPacket.ChannelId)
                    .IsDelivered(pendingPacket.Id);

                var isExpired = pendingPacket.IsExpired(_networkSettings.ResendTimeout);

                if (!isDelivered && !isExpired)
                {
                    if (pendingPacket.HookId != (byte)ProtocolHookId.Heartbeat)
                    {
                        _logger.Debug(
                            $"Resend from: - {_client.GetLocalIp()} to: {pendingPacket.To} packetId: {pendingPacket.Id} channel: {pendingPacket.ChannelId}");
                    }

                    var to = pendingPacket.To;

                    _client.Send(ref to, pendingPacket.Payload, pendingPacket.Payload.Length);
                }
                else
                {
                    _logger.Debug($"Packet expired {pendingPacket.Id}");
                    resendQueue.RemoveAt(i);
                }
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

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}