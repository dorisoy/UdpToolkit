namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Buffers;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Utils;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private static readonly int BufferSize = 1500;
        private static readonly EndPoint AnyIp = new IPEndPoint(IPAddress.Any, 0);
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly UdpClient _receiver;
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly IConnectionPool _connectionPool;
        private readonly IConnection _hostConnection;

        public UdpReceiver(
            Action<InPacket> action,
            UdpClient receiver,
            IConnectionPool connectionPool,
            IUdpToolkitLogger udpToolkitLogger,
            IDateTimeProvider dateTimeProvider,
            IConnection hostConnection)
        {
            _receiver = receiver;
            _connectionPool = connectionPool;
            _udpToolkitLogger = udpToolkitLogger;
            _dateTimeProvider = dateTimeProvider;
            _hostConnection = hostConnection;
            OnPacketReceived += action;
            _udpToolkitLogger.Debug($"{nameof(UdpReceiver)}| - {receiver.Client.LocalEndPoint} created");
        }

        public event Action<InPacket> OnPacketReceived;

        public void Dispose()
        {
            _receiver.Dispose();
        }

        public void Receive()
        {
            while (true)
            {
                var remoteIp = AnyIp;
                var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                var bytesReceived = _receiver.Client.ReceiveFrom(
                    buffer: buffer,
                    offset: 0,
                    size: BufferSize,
                    socketFlags: SocketFlags.None,
                    remoteEP: ref remoteIp);

                ReceiveCallback((IPEndPoint)remoteIp, buffer, bytesReceived);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void ReceiveCallback(
            IPEndPoint remoteIp,
            Memory<byte> memory,
            int bytesReceived)
        {
            var inPacket = InPacket.Deserialize(
                bytes: memory,
                ipEndPoint: remoteIp,
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
                        var connect = ProtocolEvent<Connect>.Deserialize(inPacket.Serializer());

                        connection = _connectionPool.AddOrUpdate(
                            connectionId: connectionId,
                            lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                            keepAlive: false,
                            ips: connect.InputPorts
                                .Select(port => new IPEndPoint(inPacket.IpEndPoint.Address, port))
                                .ToList());
                        break;
                }
            }

            if (connection == null)
            {
                return;
            }

            if (inPacket.HookId != 253)
            {
                _udpToolkitLogger.Debug(
                    $"Received from: - {remoteIp} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {inPacket.PacketType}");
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
                                    $"Sended from: - {remoteIp} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}");
                            }

                            _receiver
                                .Send(bytes, bytes.Length, connection.GetIp());
                        }

                        return;
                    }

                    if (inPacket.IsReliable)
                    {
                        var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                        if (inPacket.HookId != 253)
                        {
                            _udpToolkitLogger.Debug(
                                $"Sended from: - {remoteIp} to: {_receiver.Client.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}");
                        }

                        var ip = packetType == PacketType.FromServer
                            ? _hostConnection.GetIp()
                            : connection.GetIp();

                        _receiver
                            .Send(bytes, bytes.Length, ip);
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
    }
}
