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
            _udpToolkitLogger.Debug($"{nameof(UdpSender)}| - {sender.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _sender.Dispose();
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
                        throw new ArgumentOutOfRangeException();
                }
            }

            connection
                .GetOutcomingChannel(channelType: outPacket.ChannelType)
                .HandleOutputPacket(out var id, out var acks);

            var bytes = OutPacket.Serialize(id, acks, ref outPacket);

            if (bytes.Length > MtuSizeLimit)
            {
                _udpToolkitLogger.Error($"Udp packet oversize mtu limit - {bytes.Length}");

                return;
            }

            if (outPacket.HookId != 253)
            {
                _udpToolkitLogger.Debug(
                    $"Sended from: - {_sender.Client.LocalEndPoint} to: {outPacket.IpEndPoint} packetId: {id} channel: {outPacket.ChannelType} hookId: {outPacket.HookId} packetType {outPacket.PacketType}");
            }

            if (outPacket.IsReliable && outPacket.HookId != 253)
            {
                if (packetType == PacketType.Ack)
                {
                    throw new Exception();
                }

                _resendQueue.Add(connection.ConnectionId, new ResendPacket(
                    hookId: outPacket.HookId,
                    payload: bytes,
                    to: outPacket.IpEndPoint,
                    createdAt: outPacket.CreatedAt,
                    id: id,
                    channelType: outPacket.ChannelType));
            }

            _sender
                .Send(bytes, bytes.Length, outPacket.IpEndPoint);
        }

        private void ResendPackages(
            IConnection connection)
        {
            _udpToolkitLogger.Debug("Heartbeat");
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
                        _udpToolkitLogger.Debug(
                            $"Resend from: - {_sender.Client.LocalEndPoint} to: {resendPacket.To} packetId: {resendPacket.Id} channel: {resendPacket.ChannelType}");
                    }

                    _sender.SendAsync(resendPacket.Payload, resendPacket.Payload.Length, resendPacket.To);
                }
                else
                {
                    _udpToolkitLogger.Debug($"Packet expired {resendPacket.Id}");
                    resendQueue.RemoveAt(i);
                }
            }
        }
    }
}
