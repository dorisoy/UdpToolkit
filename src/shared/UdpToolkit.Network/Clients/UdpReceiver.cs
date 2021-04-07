namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Buffers;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Utils;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private static readonly int BufferSize = 2048;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly Socket _receiver;
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly IConnectionPool _connectionPool;

        public UdpReceiver(
            Action<InPacket> action,
            Socket receiver,
            IConnectionPool connectionPool,
            IUdpToolkitLogger udpToolkitLogger,
            IDateTimeProvider dateTimeProvider)
        {
            _receiver = receiver;
            _connectionPool = connectionPool;
            _udpToolkitLogger = udpToolkitLogger;
            _dateTimeProvider = dateTimeProvider;
            OnPacketReceived += action;
        }

        public event Action<InPacket> OnPacketReceived;

        public void Dispose()
        {
            _receiver.Dispose();
        }

        public void Receive()
        {
            _udpToolkitLogger.Debug($"Start receiving on ip: {_receiver.LocalEndPoint}");
            var remoteIp = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                var bytesReceived = _receiver.ReceiveFrom(
                    buffer: buffer,
                    size: BufferSize,
                    socketFlags: SocketFlags.None,
                    ref remoteIp);
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

                        connection = _connectionPool.GetOrAdd(
                            connectionId: connectionId,
                            lastHeartbeat: _dateTimeProvider.GetUtcNow(),
                            keepAlive: false,
                            ip: new IPEndPoint(remoteIp.Address, remoteIp.Port));
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
                    $"Received from: - {remoteIp} to: {_receiver.LocalEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {inPacket.PacketType}");
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
                                    $"Resend ack from: - {_receiver.LocalEndPoint} to: {connection.Ip} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack}, threadId - {Thread.CurrentThread.ManagedThreadId}");
                            }

                            _receiver.SendTo(bytes, SocketFlags.None, connection.Ip);
                        }

                        return;
                    }

                    if (inPacket.IsReliable)
                    {
                        var bytes = AckPacket.Serialize(id, acks, ref inPacket);

                        if (inPacket.HookId != 253)
                        {
                            _udpToolkitLogger.Debug(
                                $"Sended from: - {_receiver.LocalEndPoint} to: {inPacket.IpEndPoint} packetId: {id} hookId: {inPacket.HookId} packetType {PacketType.Ack} threadId - {Thread.CurrentThread.ManagedThreadId}");
                        }

                        _receiver.SendTo(bytes, SocketFlags.None, inPacket.IpEndPoint);
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
