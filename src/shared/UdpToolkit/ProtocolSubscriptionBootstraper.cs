namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Channels;

    public sealed class ProtocolSubscriptionBootstraper
    {
        private readonly IProtocolSubscriptionManager _protocolSubscriptionManager;
        private readonly TimeSpan _inactivityTimeout;
        private readonly IPeerManager _peerManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUdpToolkitLogger _udpToolkitLogger;

        public ProtocolSubscriptionBootstraper(
            IProtocolSubscriptionManager protocolSubscriptionManager,
            TimeSpan inactivityTimeout,
            IPeerManager peerManager,
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLogger udpToolkitLogger)
        {
            _protocolSubscriptionManager = protocolSubscriptionManager;
            _inactivityTimeout = inactivityTimeout;
            _peerManager = peerManager;
            _dateTimeProvider = dateTimeProvider;
            _udpToolkitLogger = udpToolkitLogger;
        }

        public void BootstrapSubscriptions()
        {
            _protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Connect>(
                    hookId: (byte)ProtocolHookId.Connect,
                    onInputEvent: (bytes, peerId) =>
                    {
                        var @event = ProtocolEvent<Connect>.Deserialize(bytes);

                        var ips = @event.ClientIps
                            .Select(server => new IPEndPoint(IPAddress.Parse(server.Host), server.Port))
                            .ToList();

                        _peerManager.AddOrUpdate(
                            inactivityTimeout: _inactivityTimeout,
                            peerId: peerId,
                            ips: ips);

                        _udpToolkitLogger.Debug($"Input - {nameof(Connect)}");
                    },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        _peerManager.TryGetPeer(peerId, out var peer);
                        _udpToolkitLogger.Debug($"Output - {nameof(Connect)}");
                    },
                    onAck: (peerId) =>
                    {
                        _udpToolkitLogger.Debug($"Peer - {peerId}");
                    },
                    onAckTimeout: (peerId) =>
                    {
                    },
                    broadcastMode: BroadcastMode.Caller);

            _protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Disconnect>(
                    hookId: (byte)ProtocolHookId.Disconnect,
                    onInputEvent: (bytes, peerId) =>
                    {
                        var disconnect = ProtocolEvent<Disconnect>.Deserialize(bytes);

                        _peerManager.TryGetPeer(disconnect.PeerId, out var peer);
                    },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        _peerManager.TryGetPeer(peerId, out var peer);

                        peer.OnPing(_dateTimeProvider.UtcNow());
                    },
                    onAck: (peerId) => { },
                    onAckTimeout: (peerId) => { },
                    broadcastMode: BroadcastMode.Caller);

            _protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Ping>(
                    hookId: (byte)ProtocolHookId.Ping,
                    onInputEvent: (bytes, peerId) => { },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        _peerManager.TryGetPeer(peerId, out var peer);

                        peer.OnPing(_dateTimeProvider.UtcNow());
                    },
                    onAck: (peerId) => { },
                    onAckTimeout: (peerId) => { },
                    broadcastMode: BroadcastMode.Caller);

            _protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Pong>(
                    hookId: (byte)ProtocolHookId.Pong,
                    onInputEvent: (bytes, peerId) =>
                    {
                        _peerManager.TryGetPeer(peerId, out var peer);

                        _udpToolkitLogger.Debug($"{ProtocolHookId.Pong}");

                        peer.OnPong(_dateTimeProvider.UtcNow());
                        _udpToolkitLogger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
                    },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        _peerManager.TryGetPeer(peerId, out var peer);
                        _udpToolkitLogger.Debug($"{ProtocolHookId.Pong}");

                        peer.OnPong(_dateTimeProvider.UtcNow());
                        _udpToolkitLogger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
                    },
                    onAck: (peerId) => { },
                    onAckTimeout: (peerId) => { },
                    broadcastMode: BroadcastMode.Caller);
        }
    }
}