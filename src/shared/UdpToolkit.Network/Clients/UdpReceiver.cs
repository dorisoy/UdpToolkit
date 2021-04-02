namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Utils;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly UdpClient _receiver;
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly IConnectionPool _connectionPool;
        private readonly TimeSpan _connectionInactivityTimeout;
        private readonly IConnection _remoteHostConnection;

        public UdpReceiver(
            UdpClient receiver,
            IConnectionPool connectionPool,
            TimeSpan connectionInactivityTimeout,
            IUdpToolkitLogger udpToolkitLogger,
            IDateTimeProvider dateTimeProvider,
            IConnection remoteHostConnection)
        {
            _receiver = receiver;
            _connectionPool = connectionPool;
            _connectionInactivityTimeout = connectionInactivityTimeout;
            _udpToolkitLogger = udpToolkitLogger;
            _dateTimeProvider = dateTimeProvider;
            _remoteHostConnection = remoteHostConnection;
            _udpToolkitLogger.Debug($"{nameof(UdpReceiver)} - {receiver.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }

        public async Task<ValueTuple<InPacket, bool>> ReceiveAsync()
        {
            var udpReceiveResult = await _receiver
                .ReceiveAsync()
                .ConfigureAwait(false);

            var inPacket = InPacket.Deserialize(
                bytes: udpReceiveResult.Buffer,
                ipEndPoint: udpReceiveResult.RemoteEndPoint,
                out var id,
                out var acks);

            var connectionId = inPacket.ConnectionId;
            var connection = _connectionPool.TryGetConnection(connectionId);

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
                        var connect = ProtocolEvent<Connect>.Deserialize(inPacket.Serializer());

                        connection = _connectionPool.AddOrUpdate(
                            connectionTimeout: _connectionInactivityTimeout,
                            connectionId: connectionId,
                            ips: connect.InputPorts
                                .Select(port => new IPEndPoint(inPacket.IpEndPoint.Address, port))
                                .ToList());
                        break;
                }
            }

            if (connection == null)
            {
                return (default, false);
            }

            if (inPacket.HookId != 253)
            {
                _udpToolkitLogger.Debug(
                    $"Received from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {inPacket.PacketType}");
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
                                _udpToolkitLogger.Debug(
                                    $"Sended from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}");
                            }

                            await _receiver
                                .SendAsync(bytes, bytes.Length, connection.GetIp())
                                .ConfigureAwait(false);
                        }

                        return (default, false);
                    }

                    if (inPacket.IsReliable)
                    {
                        var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                        if (inPacket.HookId != 253)
                        {
                            _udpToolkitLogger.Debug(
                                $"Sended from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}");
                        }

                        var ip = packetType == PacketType.FromServer
                            ? _remoteHostConnection.GetIp()
                            : connection.GetIp();

                        await _receiver
                            .SendAsync(bytes, bytes.Length, ip)
                            .ConfigureAwait(false);
                    }

                    return (inPacket, true);

                case PacketType.Ack:
                    var handled = connection
                        .GetOutcomingChannel(inPacket.ChannelType)
                        .HandleAck(id, acks);

                    return handled
                        ? (inPacket, true)
                        : (default, false);

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {inPacket.PacketType} - not supported!");
            }
        }
    }
}
