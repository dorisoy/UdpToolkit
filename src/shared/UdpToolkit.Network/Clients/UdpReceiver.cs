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
    using UdpToolkit.Network.Pooling;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Utils;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly UdpClient _receiver;
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;
        private readonly IConnectionPool _connectionPool;
        private readonly TimeSpan _connectionInactivityTimeout;

        public UdpReceiver(
            UdpClient receiver,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IConnectionPool connectionPool,
            TimeSpan connectionInactivityTimeout,
            IUdpToolkitLogger udpToolkitLogger,
            IDateTimeProvider dateTimeProvider)
        {
            _receiver = receiver;
            _networkPacketPool = networkPacketPool;
            _connectionPool = connectionPool;
            _connectionInactivityTimeout = connectionInactivityTimeout;
            _udpToolkitLogger = udpToolkitLogger;
            _dateTimeProvider = dateTimeProvider;
            _udpToolkitLogger.Debug($"{nameof(UdpReceiver)} - {receiver.Client.LocalEndPoint} created");
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }

        public async Task<PooledObject<NetworkPacket>> ReceiveAsync()
        {
            var udpReceiveResult = await _receiver
                .ReceiveAsync()
                .ConfigureAwait(false);

            var pooledNetworkPacket = _networkPacketPool.Get();

            NetworkPacket.Deserialize(
                bytes: udpReceiveResult.Buffer,
                ipEndPoint: udpReceiveResult.RemoteEndPoint,
                pooledNetworkPacket: pooledNetworkPacket);

            var connectionId = pooledNetworkPacket.Value.ConnectionId;
            var connection = _connectionPool.TryGetConnection(connectionId);
            var networkPacketType = pooledNetworkPacket.Value.NetworkPacketType;

            if (pooledNetworkPacket.Value.IsProtocolEvent)
            {
                switch ((ProtocolHookId)pooledNetworkPacket.Value.HookId)
                {
                    case ProtocolHookId.P2P:
                        break;
                    case ProtocolHookId.Ping when networkPacketType == NetworkPacketType.Protocol:
                        connection?.OnPing(_dateTimeProvider.GetUtcNow());

                        break;

                    case ProtocolHookId.Ping when networkPacketType == NetworkPacketType.Ack:
                        connection?.OnPingAck(_dateTimeProvider.GetUtcNow());

                        break;
                    case ProtocolHookId.Disconnect when networkPacketType == NetworkPacketType.Protocol:
                        _connectionPool.Remove(connection);
                        break;
                    case ProtocolHookId.Connect when networkPacketType == NetworkPacketType.Protocol:
                        var connect = ProtocolEvent<Connect>.Deserialize(pooledNetworkPacket.Value.Serializer());

                        connection = _connectionPool.AddOrUpdate(
                            connectionTimeout: _connectionInactivityTimeout,
                            connectionId: connectionId,
                            ips: connect.InputPorts
                                .Select(port => new IPEndPoint(pooledNetworkPacket.Value.IpEndPoint.Address, port))
                                .ToList());
                        break;
                }
            }

            if (ReceiveInternalAsync(connection, udpReceiveResult, pooledNetworkPacket))
            {
                return pooledNetworkPacket;
            }
            else
            {
                // TODO resend ack
                pooledNetworkPacket?.Dispose();

                return null;
            }
        }

        private bool ReceiveInternalAsync(
            IConnection connection,
            UdpReceiveResult udpReceiveResult,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var networkPacket = pooledNetworkPacket.Value;
            _udpToolkitLogger.Debug(
                $"Packet received from: - {udpReceiveResult.RemoteEndPoint} to: {_receiver.Client.LocalEndPoint} packet: {networkPacket} total bytes length: {udpReceiveResult.Buffer.Length}, payload bytes length: {networkPacket.Serializer().Length}");

            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                case NetworkPacketType.Protocol:
                    return connection
                        .GetIncomingChannel(channelType: networkPacket.ChannelType)
                        .HandleInputPacket(pooledNetworkPacket.Value);

                case NetworkPacketType.Ack:
                    return connection
                        .GetOutcomingChannel(networkPacket.ChannelType)
                        .HandleAck(pooledNetworkPacket.Value);

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {networkPacket.NetworkPacketType} - not supported!");
            }
        }
    }
}
