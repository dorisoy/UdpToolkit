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

            var networkPacket = InPacket.Deserialize(
                bytes: udpReceiveResult.Buffer,
                ipEndPoint: udpReceiveResult.RemoteEndPoint,
                out var id,
                out var acks);

            var connectionId = networkPacket.ConnectionId;
            var connection = _connectionPool.TryGetConnection(connectionId);

            var networkPacketType = networkPacket.NetworkPacketType;

            if (networkPacket.IsProtocolEvent)
            {
                switch ((ProtocolHookId)networkPacket.HookId)
                {
                    case ProtocolHookId.P2P:
                        break;

                    case ProtocolHookId.Heartbeat when networkPacketType == NetworkPacketType.Protocol:
                        connection?.OnHeartbeat(_dateTimeProvider.GetUtcNow());

                        break;

                    case ProtocolHookId.Heartbeat when networkPacketType == NetworkPacketType.Ack:
                        connection?.OnHeartbeatAck(_dateTimeProvider.GetUtcNow());

                        break;
                    case ProtocolHookId.Disconnect when networkPacketType == NetworkPacketType.Protocol:
                        _connectionPool.Remove(connection);
                        break;
                    case ProtocolHookId.Connect when networkPacketType == NetworkPacketType.Protocol:
                        var connect = ProtocolEvent<Connect>.Deserialize(networkPacket.Serializer());

                        connection = _connectionPool.AddOrUpdate(
                            connectionTimeout: _connectionInactivityTimeout,
                            connectionId: connectionId,
                            ips: connect.InputPorts
                                .Select(port => new IPEndPoint(networkPacket.IpEndPoint.Address, port))
                                .ToList());
                        break;
                }
            }

            if (connection == null)
            {
                return (default, false);
            }

            if (networkPacket.HookId != 253)
            {
                _udpToolkitLogger.Debug(
                    $"Received from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {networkPacket.HookId} packetType {networkPacket.NetworkPacketType}");
            }

            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                case NetworkPacketType.Protocol:
                    var handled1 = connection
                        .GetIncomingChannel(channelType: networkPacket.ChannelType)
                        .HandleInputPacket(id, acks);

                    if (!handled1)
                    {
                        if (networkPacket.IsReliable)
                        {
                            var bytes = AckPacket.Serialize(id, acks, ref networkPacket);

                            if (networkPacket.HookId != 253)
                            {
                                _udpToolkitLogger.Debug(
                                    $"Sended from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {networkPacket.HookId} packetType {NetworkPacketType.Ack}");
                            }

                            await _receiver
                                .SendAsync(bytes, bytes.Length, connection.GetIp())
                                .ConfigureAwait(false);
                        }

                        return (default, false);
                    }

                    if (networkPacket.IsReliable)
                    {
                        var bytes = AckPacket.Serialize(id, acks, ref networkPacket);

                        if (networkPacket.HookId != 253)
                        {
                            _udpToolkitLogger.Debug(
                                $"Sended from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {networkPacket.HookId} packetType {NetworkPacketType.Ack}");
                        }

                        var ip = networkPacketType == NetworkPacketType.FromServer
                            ? _remoteHostConnection.GetIp()
                            : connection.GetIp();

                        await _receiver
                            .SendAsync(bytes, bytes.Length, ip)
                            .ConfigureAwait(false);
                    }

                    return (networkPacket, true);

                case NetworkPacketType.Ack:
                    var handled = connection
                        .GetOutcomingChannel(networkPacket.ChannelType)
                        .HandleAck(id, acks);

                    return handled
                        ? (networkPacket, true)
                        : (default, false);

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {networkPacket.NetworkPacketType} - not supported!");
            }
        }
    }
}
