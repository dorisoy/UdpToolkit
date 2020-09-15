namespace UdpToolkit.Framework
{
    using System;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Serialization;

    public sealed class ProtocolSubscriptionManager : IProtocolSubscriptionManager
    {
        private readonly ILogger _logger = Log.ForContext<Host>();
        private readonly IPeerManager _peerManager;
        private readonly ISerializer _serializer;
        private readonly IDatagramBuilder _datagramBuilder;

        public ProtocolSubscriptionManager(
            IPeerManager peerManager,
            ISerializer serializer,
            IDatagramBuilder datagramBuilder)
        {
            _peerManager = peerManager;
            _serializer = serializer;
            _datagramBuilder = datagramBuilder;
        }

        public void OnConnect(Guid peerId, byte[] bytes, IHost host)
        {
            _logger.Debug($"{PacketType.Connect}");

            var exists = _peerManager.Exist(peerId: peerId);
            if (exists)
            {
                return;
            }

            var connect = _serializer.DeserializeContractLess<Connect>(bytes);
            _peerManager.Create(peerId, connect.GetPeerIps());

            var datagram = _datagramBuilder.Caller(
                @event: new Connected(peerId),
                peerId: peerId,
                hookId: (byte)PacketType.Connected);

            host.PublishInternal(
                datagram: datagram,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);
        }

        public void OnConnected(Guid peerId, byte[] bytes)
        {
            _logger.Debug($"{PacketType.Connected}");
        }

        public void OnDisconnect(Guid peerId, byte[] bytes, IHost host)
        {
            _logger.Debug($"{PacketType.Disconnect}");

            var disconnect = _serializer.DeserializeContractLess<Disconnect>(bytes);

            var datagram = _datagramBuilder.Caller(
                @event: new Disconnected(peerId),
                peerId: peerId,
                hookId: (byte)PacketType.Disconnected);

            host.PublishInternal(
                datagram: datagram,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);

            _peerManager.Remove(peerId);
        }

        public void OnDisconnected(Guid peerId, byte[] bytes)
        {
            _logger.Debug($"{PacketType.Disconnected}");

            var disconnected = _serializer.DeserializeContractLess<Disconnected>(bytes);
            _peerManager.Remove(disconnected.PeerId);
        }
    }
}