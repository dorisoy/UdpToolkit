namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Utils;

    public sealed class UdpSender : IUdpSender
    {
        private const int MtuSizeLimit = 1500; // TODO detect automatic

        private readonly UdpClient _sender;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConnectionPool _connectionPool;
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly ResendQueue _resendQueue;
        private readonly TimeSpan _resendTimeout;

        public UdpSender(
            UdpClient sender,
            IUdpToolkitLogger udpToolkitLogger,
            IConnectionPool connectionPool,
            IDateTimeProvider dateTimeProvider,
            ResendQueue resendQueue,
            TimeSpan resendTimeout)
        {
            _sender = sender;
            _udpToolkitLogger = udpToolkitLogger;
            _connectionPool = connectionPool;
            _dateTimeProvider = dateTimeProvider;
            _resendQueue = resendQueue;
            _resendTimeout = resendTimeout;
            _udpToolkitLogger.Debug($"{nameof(UdpSender)} - {sender.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public Task SendAsync(
            ref NetworkPacket networkPacket)
        {
            var connection = _connectionPool
                .TryGetConnection(networkPacket.ConnectionId);

            if (connection == null)
            {
                return Task.CompletedTask;
            }

            var networkPacketType = networkPacket.NetworkPacketType;

            if (networkPacket.IsProtocolEvent)
            {
                switch ((ProtocolHookId)networkPacket.HookId)
                {
                    case ProtocolHookId.P2P:
                        break;
                    case ProtocolHookId.Heartbeat when networkPacketType == NetworkPacketType.Protocol:
                        connection?.OnHeartbeat(_dateTimeProvider.GetUtcNow());
                        ResendPackages(connection);

                        break;
                    case ProtocolHookId.Heartbeat when networkPacketType == NetworkPacketType.Ack:
                        connection?.OnHeartbeatAck(_dateTimeProvider.GetUtcNow());

                        break;
                    case ProtocolHookId.Disconnect:
                        break;
                    case ProtocolHookId.Connect:
                        break;
                    case ProtocolHookId.Heartbeat:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SendInternal(connection, ref networkPacket, out var id, out var acks);

            var bytes = NetworkPacket.Serialize(id, acks, ref networkPacket);

            if (bytes.Length > MtuSizeLimit)
            {
                _udpToolkitLogger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return Task.CompletedTask;
            }

            if (networkPacket.HookId != 253)
            {
                _udpToolkitLogger.Debug(
                    $"Sended from: - {_sender.Client.LocalEndPoint} to: {networkPacket.IpEndPoint} packetId: {id} channel: {networkPacket.ChannelType} hookId: {networkPacket.HookId} packetType {networkPacket.NetworkPacketType}");
            }

            if (networkPacket.IsReliable && networkPacket.NetworkPacketType != NetworkPacketType.Ack && networkPacket.HookId != 253)
            {
                _resendQueue.Add(connection.ConnectionId, new ResendPacket(
                    hookId: networkPacket.HookId,
                    payload: bytes,
                    to: networkPacket.IpEndPoint,
                    createdAt: networkPacket.CreatedAt,
                    id: id,
                    channelType: networkPacket.ChannelType));
            }

            return _sender
                .SendAsync(bytes, bytes.Length, networkPacket.IpEndPoint);
        }

        private void ResendPackages(
            IConnection connection)
        {
            // _udpToolkitLogger.Debug("Heartbeat");
            var resendQueue = _resendQueue.Get(connection.ConnectionId);
            for (var i = 0; i < resendQueue.Count; i++)
            {
                var networkPacket = resendQueue[i];

                var isDelivered = connection
                    .GetOutcomingChannel(networkPacket.ChannelType)
                    .IsDelivered(networkPacket.Id);

                var isExpired = networkPacket
                    .IsExpired(_resendTimeout);

                if (!isDelivered && !isExpired)
                {
                    if (networkPacket.HookId != 253)
                    {
                        _udpToolkitLogger.Debug(
                            $"Resend from: - {_sender.Client.LocalEndPoint} to: {networkPacket.To} packetId: {networkPacket.Id} channel: {networkPacket.ChannelType}");
                    }

                    _sender.SendAsync(networkPacket.Payload, networkPacket.Payload.Length, networkPacket.To);
                }
                else
                {
                    resendQueue.RemoveAt(i);
                }
            }
        }

        private bool SendInternal(
            IConnection connection,
            ref NetworkPacket networkPacket,
            out ushort id,
            out uint acks)
        {
            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                case NetworkPacketType.Protocol:
                    connection
                        .GetOutcomingChannel(channelType: networkPacket.ChannelType)
                        .HandleOutputPacket(out id, out acks);

                    return true;

                case NetworkPacketType.Ack:
                    connection
                        .GetOutcomingChannel(networkPacket.ChannelType);

                    id = default;
                    acks = default;

                    return false;

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {networkPacket.NetworkPacketType} - not supported!");
            }
        }
    }
}
