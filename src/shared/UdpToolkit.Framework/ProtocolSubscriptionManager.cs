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
        private readonly IDateTimeProvider _dateTimeProvider;

        public ProtocolSubscriptionManager(
            IPeerManager peerManager,
            ISerializer serializer,
            IDatagramBuilder datagramBuilder,
            IDateTimeProvider dateTimeProvider)
        {
            _peerManager = peerManager;
            _serializer = serializer;
            _datagramBuilder = datagramBuilder;
            _dateTimeProvider = dateTimeProvider;
        }

        public void OnPong(Guid peerId, byte[] bytes, IHost host)
        {
            _logger.Debug($"{PacketType.Pong}");

            var peer = _peerManager.Get(peerId);
            peer.OnPong(_dateTimeProvider.UtcNow());
            _logger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
        }

        public void OnPing(Guid peerId, byte[] bytes, IHost host)
        {
            _logger.Debug($"{PacketType.Ping}");

            var peer = _peerManager.Get(peerId);
            peer.OnPing(_dateTimeProvider.UtcNow());

            var datagram = _datagramBuilder.Caller(
                @event: new Pong(),
                peerId: peerId,
                hookId: (byte)PacketType.Pong);

            host.PublishInternal(
                datagram: datagram,
                udpMode: UdpMode.ReliableUdp,
                serializer: _serializer.SerializeContractLess);
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

            _peerManager.Remove(peerId);
        }
    }
}