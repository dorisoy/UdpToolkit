namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Sockets;
    using UdpToolkit.Network.Utils;

    public sealed class UdpClient : IUdpClient
    {
        private const int MtuSizeLimit = 1500; // TODO detect automatic
        private const int BufferSize = 2048;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ISocket _client;
        private readonly IUdpToolkitLogger _logger;
        private readonly IConnectionPool _connectionPool;

        private readonly ResendQueue _resendQueue;
        private readonly TimeSpan _resendTimeout;

        private bool _disposed = false;
        private bool _cancelled = false;

        public UdpClient(
            IConnectionPool connectionPool,
            IUdpToolkitLogger logger,
            IDateTimeProvider dateTimeProvider,
            ISocket client,
            ResendQueue resendQueue,
            TimeSpan resendTimeout)
        {
            _client = client;
            _connectionPool = connectionPool;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _resendQueue = resendQueue;
            _resendTimeout = resendTimeout;
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

            if (outPacket.IsProtocolEvent)
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
                        break;
                    case ProtocolHookId.Connect:
                        break;
                    case ProtocolHookId.Heartbeat:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(OutPacket.HookId), outPacket.HookId.ToString(), null);
                }
            }

            connection
                .GetOutcomingChannel(channelType: outPacket.ChannelType)
                .HandleOutputPacket(out var id, out var acks);

            var bytes = OutPacket.Serialize(id, acks, ref outPacket);

            if (bytes.Length > MtuSizeLimit)
            {
                _logger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            if (outPacket.HookId != 253)
            {
                _logger.Debug(
                    $"Sended from: - {_client.GetLocalIp()} to: {outPacket.IpAddress} packetId: {id} channel: {outPacket.ChannelType} hookId: {outPacket.HookId} packetType {outPacket.PacketType} threadId - {Thread.CurrentThread.ManagedThreadId}");
            }

            if (outPacket.IsReliable && outPacket.HookId != 253)
            {
                if (packetType == PacketType.Ack)
                {
                    throw new NotSupportedException();
                }

                _resendQueue.Add(connection.ConnectionId, new ResendPacket(
                    hookId: outPacket.HookId,
                    payload: bytes,
                    to: outPacket.IpAddress,
                    createdAt: outPacket.CreatedAt,
                    id: id,
                    channelType: outPacket.ChannelType));
            }

            var ipAddress = outPacket.IpAddress;

            _client.Send(ref ipAddress, bytes, bytes.Length);
        }

        public void Receive()
        {
            _logger.Debug($"Start receiving on ip: {_client.GetLocalIp()}");
            var remoteIp = new IpV4Address
            {
                Port = 0,
                Address = 0,
            };
            var buffer = new byte[BufferSize];

            while (!_cancelled)
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

            _client.Dispose();
            _logger.Debug($"Socket disposed!");
        }

        private void ReceiveCallback(
            ref IpV4Address remoteIp,
            Memory<byte> memory,
            int bytesReceived)
        {
            var inPacket = InPacket.Deserialize(
                bytes: memory,
                ipEndPoint: remoteIp.ToIpEndPoint(),
                bytesReceived: bytesReceived,
                out var id,
                out var acks);

            var connectionId = inPacket.ConnectionId;
            if (!_connectionPool.TryGetConnection(connectionId, out var connection) && (ProtocolHookId)inPacket.HookId != ProtocolHookId.Connect)
            {
                return;
            }

            var packetType = inPacket.PacketType;

            if (inPacket.IsProtocolEvent)
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
                        connection = _connectionPool.GetOrAdd(
                            connectionId: connectionId,
                            lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                            keepAlive: false,
                            ipAddress: remoteIp);
                        break;
                }
            }

            if (connection == null)
            {
                return;
            }

            if (inPacket.HookId != 253)
            {
                _logger.Debug(
                    $"Received from: - {remoteIp} to: {_client.GetLocalIp()} packetId: {id} hookId: {inPacket.HookId} packetType {inPacket.PacketType}");
            }

            switch (inPacket.PacketType)
            {
                case PacketType.FromServer:
                case PacketType.FromClient:
                case PacketType.Protocol:
                    var handled1 = connection
                        .GetIncomingChannel(channelType: inPacket.ChannelType)
                        .HandleInputPacket(id, acks);

                    if (!handled1)
                    {
                        if (inPacket.IsReliable)
                        {
                            var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                            if (inPacket.HookId != 253)
                            {
                                _logger.Debug(
                                    $"Resend ack from: - {_client.GetLocalIp()} to: {connection.IpAddress} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}, threadId - {Thread.CurrentThread.ManagedThreadId}");
                            }

                            var to = connection.IpAddress;

                            _client.Send(ref to, bytes, bytes.Length);
                        }

                        return;
                    }

                    if (inPacket.IsReliable)
                    {
                        var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                        if (inPacket.HookId != 253)
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
                    var handled = connection
                        .GetOutcomingChannel(inPacket.ChannelType)
                        .HandleAck(id, acks);

                    if (handled)
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
                var resendPacket = resendQueue[i];

                var isDelivered = connection
                    .GetOutcomingChannel(resendPacket.ChannelType)
                    .IsDelivered(resendPacket.Id);

                var isExpired = resendPacket.IsExpired(_resendTimeout);

                if (!isDelivered && !isExpired)
                {
                    if (resendPacket.HookId != 253)
                    {
                        _logger.Debug(
                            $"Resend from: - {_client.GetLocalIp()} to: {resendPacket.To} packetId: {resendPacket.Id} channel: {resendPacket.ChannelType}");
                    }

                    var to = resendPacket.To;

                    _client.Send(ref to, resendPacket.Payload, resendPacket.Payload.Length);
                }
                else
                {
                    _logger.Debug($"Packet expired {resendPacket.Id}");
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
                _cancelled = true;
                _connectionPool.Dispose();
            }

            _logger.Debug($"{this.GetType().Name} - disposed!");
            _disposed = true;
        }
    }
}