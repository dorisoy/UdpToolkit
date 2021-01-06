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

            if (ReceiveInternalAsync(udpReceiveResult, pooledNetworkPacket))
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
            UdpReceiveResult udpReceiveResult,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var networkPacket = pooledNetworkPacket.Value;
            _udpToolkitLogger.Debug($"Packet from - {udpReceiveResult.RemoteEndPoint} to {_receiver.Client.LocalEndPoint} received");
            _udpToolkitLogger.Debug($"Packet received: {networkPacket}, Total bytes length: {udpReceiveResult.Buffer.Length}, Payload bytes length: {networkPacket.Serializer().Length}");

            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                    if (!HandleUserDefinedEvent(pooledNetworkPacket))
                    {
                        _udpToolkitLogger.Debug($"UserDefined {nameof(NetworkPacket)} with id - {networkPacket.Id} dropped! ");
                        return false;
                    }

                    break;
                case NetworkPacketType.Protocol:
                    if (!HandleProtocolEvent(pooledNetworkPacket))
                    {
                        _udpToolkitLogger.Debug($"Protocol {nameof(NetworkPacket)} with id - {networkPacket.Id} dropped! ");
                        return false;
                    }

                    break;
                case NetworkPacketType.Ack:
                    if (networkPacket.IsProtocolEvent)
                    {
                        if (!HandleProtocolAck(pooledNetworkPacket))
                        {
                            _udpToolkitLogger.Debug($"Protocol Ack dropped - {networkPacket}");
                            return false;
                        }
                    }
                    else
                    {
                        if (!HandleUserDefinedAck(pooledNetworkPacket))
                        {
                            _udpToolkitLogger.Debug($"UserDefined Ack dropped! {networkPacket.Id}");
                            return false;
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException($"NetworkPacketType {networkPacket.NetworkPacketType} - not supported!");
            }

            return true;
        }

        private bool HandleUserDefinedEvent(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var networkPacket = pooledNetworkPacket.Value;
            if (!_rawPeerManager.TryGetPeer(networkPacket.PeerId, out var rawPeer))
            {
                return false;
            }

            var channel = rawPeer
                .GetIncomingChannel(channelType: networkPacket.ChannelType);

            return channel
                .HandleInputPacket(pooledNetworkPacket.Value);
        }

        private bool HandleProtocolEvent(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var networkPacket = pooledNetworkPacket.Value;
            var peer = _rawPeerManager.AddOrUpdate(
                inactivityTimeout: _peerInactivityTimeout,
                peerId: networkPacket.PeerId,
                ips: new List<IPEndPoint>());

            return peer
                .GetIncomingChannel(networkPacket.ChannelType)
                .HandleInputPacket(pooledNetworkPacket.Value);
        }

        private bool HandleProtocolAck(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var networkPacket = pooledNetworkPacket.Value;
            var peer = _rawPeerManager.AddOrUpdate(
                inactivityTimeout: _peerInactivityTimeout,
                peerId: networkPacket.PeerId,
                ips: new List<IPEndPoint>());

            return peer
                .GetOutcomingChannel(networkPacket.ChannelType)
                .HandleAck(pooledNetworkPacket.Value);
        }

        private bool HandleUserDefinedAck(
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            var networkPacket = pooledNetworkPacket.Value;
            if (!_rawPeerManager.TryGetPeer(networkPacket.PeerId, out var rawPeer))
            {
                return false;
            }

            var channel = rawPeer
                .GetOutcomingChannel(channelType: networkPacket.ChannelType);

            return channel
                .HandleAck(networkPacket: pooledNetworkPacket.Value);
        }
    }
}
