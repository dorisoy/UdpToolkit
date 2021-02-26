namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Pooling;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly UdpClient _receiver;
        private readonly IUdpToolkitLogger _udpToolkitLogger;
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;
        private readonly IRawPeerManager _rawPeerManager;
        private readonly TimeSpan _peerInactivityTimeout;

        public UdpReceiver(
            UdpClient receiver,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IRawPeerManager rawPeerManager,
            TimeSpan peerInactivityTimeout,
            IUdpToolkitLogger udpToolkitLogger)
        {
            _receiver = receiver;
            _networkPacketPool = networkPacketPool;
            _rawPeerManager = rawPeerManager;
            _peerInactivityTimeout = peerInactivityTimeout;
            _udpToolkitLogger = udpToolkitLogger;
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

            var peerId = pooledNetworkPacket.Value.PeerId;
            var rawPeer = pooledNetworkPacket.Value.IsProtocolEvent
                ? _rawPeerManager.AddOrUpdate(
                    inactivityTimeout: _peerInactivityTimeout,
                    peerId: peerId,
                    ips: new List<IPEndPoint>())
                : _rawPeerManager.TryGetPeer(peerId);

            if (ReceiveInternalAsync(rawPeer, udpReceiveResult, pooledNetworkPacket))
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
            IRawPeer rawPeer,
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
                    return rawPeer
                        .GetIncomingChannel(channelType: networkPacket.ChannelType)
                        .HandleInputPacket(pooledNetworkPacket.Value);

                case NetworkPacketType.Ack:
                    return rawPeer
                        .GetOutcomingChannel(networkPacket.ChannelType)
                        .HandleAck(pooledNetworkPacket.Value);

                default:
                    throw new NotSupportedException(
                        $"NetworkPacketType {networkPacket.NetworkPacketType} - not supported!");
            }
        }
    }
}
