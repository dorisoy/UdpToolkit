namespace UdpToolkit.Network.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Pooling;

    public sealed class UdpReceiver : IUdpReceiver
    {
        private readonly UdpClient _receiver;
        private readonly ILogger _logger = Log.ForContext<UdpReceiver>();
        private readonly IObjectsPool<NetworkPacket> _networkPacketPool;
        private readonly IRawPeerManager _rawPeerManager;
        private readonly TimeSpan _peerInactivityTimeout;

        public UdpReceiver(
            UdpClient receiver,
            IObjectsPool<NetworkPacket> networkPacketPool,
            IRawPeerManager rawPeerManager,
            TimeSpan peerInactivityTimeout)
        {
            _receiver = receiver;
            _networkPacketPool = networkPacketPool;
            _rawPeerManager = rawPeerManager;
            _peerInactivityTimeout = peerInactivityTimeout;
            _logger.Debug($"{nameof(UdpReceiver)} - {receiver.Client.LocalEndPoint} created");
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
            _logger.Debug($"Packet from - {udpReceiveResult.RemoteEndPoint} to {_receiver.Client.LocalEndPoint} received");
            _logger.Debug(
                    messageTemplate: "Packet received: {@packet}, Total bytes length: {@length}, Payload bytes length: {@payload}",
                    propertyValue0: networkPacket,
                    propertyValue1: udpReceiveResult.Buffer.Length,
                    propertyValue2: networkPacket.Serializer().Length);

            switch (networkPacket.NetworkPacketType)
            {
                case NetworkPacketType.FromServer:
                case NetworkPacketType.FromClient:
                    if (!HandleUserDefinedEvent(pooledNetworkPacket))
                    {
                        _logger.Debug($"UserDefined {nameof(NetworkPacket)} with id - {networkPacket.Id} dropped! ");
                        return false;
                    }

                    break;
                case NetworkPacketType.Protocol:
                    if (!HandleProtocolEvent(pooledNetworkPacket))
                    {
                        _logger.Debug($"Protocol {nameof(NetworkPacket)} with id - {networkPacket.Id} dropped! ");
                        return false;
                    }

                    break;
                case NetworkPacketType.Ack:
                    if (networkPacket.IsProtocolEvent)
                    {
                        if (!HandleProtocolAck(pooledNetworkPacket))
                        {
                            _logger.Debug("Protocol Ack dropped - {@packet}", networkPacket);
                            return false;
                        }
                    }
                    else
                    {
                        if (!HandleUserDefinedAck(pooledNetworkPacket))
                        {
                            _logger.Debug($"UserDefined Ack dropped! {networkPacket.Id}");
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
