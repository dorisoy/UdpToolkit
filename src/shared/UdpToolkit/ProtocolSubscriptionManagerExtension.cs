namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Serialization;

    public static class ProtocolSubscriptionManagerExtension
    {
        private static readonly ILogger Logger = Log.ForContext<Host>();

        public static void BootstrapSubscriptions(
            this IProtocolSubscriptionManager protocolSubscriptionManager,
            TimeSpan inactivityTimeout,
            IPeerManager peerManager,
            IDateTimeProvider dateTimeProvider)
        {
            protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Connect>(
                    hookId: (byte)ProtocolHookId.Connect,
                    onInputEvent: (bytes, peerId) =>
                    {
                        var @event = ProtocolEvent<Connect>.Deserialize(bytes);

                        var ips = @event.ClientIps
                            .Select(server => new IPEndPoint(IPAddress.Parse(server.Host), server.Port))
                            .ToList();

                        peerManager.AddOrUpdate(
                            inactivityTimeout: inactivityTimeout,
                            peerId: peerId,
                            ips: ips);

                        Logger.Debug($"Input - {nameof(Connect)}");
                    },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        peerManager.TryGetPeer(peerId, out var peer);
                        Logger.Debug($"Output - {nameof(Connect)}");
                    },
                    onAck: (peerId) =>
                    {
                        Logger.Debug($"Peer - {peerId}");
                    },
                    onAckTimeout: (peerId) =>
                    {
                    },
                    broadcastMode: BroadcastMode.Caller);

            protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Disconnect>(
                    hookId: (byte)ProtocolHookId.Disconnect,
                    onInputEvent: (bytes, peerId) =>
                    {
                        var disconnect = ProtocolEvent<Disconnect>.Deserialize(bytes);

                        peerManager.TryGetPeer(disconnect.PeerId, out var peer);
                    },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        peerManager.TryGetPeer(peerId, out var peer);

                        peer.OnPing(dateTimeProvider.UtcNow());
                    },
                    onAck: (peerId) => { },
                    onAckTimeout: (peerId) => { },
                    broadcastMode: BroadcastMode.Caller);

            protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Ping>(
                    hookId: (byte)ProtocolHookId.Ping,
                    onInputEvent: (bytes, peerId) => { },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        peerManager.TryGetPeer(peerId, out var peer);

                        peer.OnPing(dateTimeProvider.UtcNow());
                    },
                    onAck: (peerId) => { },
                    onAckTimeout: (peerId) => { },
                    broadcastMode: BroadcastMode.Caller);

            protocolSubscriptionManager
                .SubscribeOnProtocolEvent<Pong>(
                    hookId: (byte)ProtocolHookId.Pong,
                    onInputEvent: (bytes, peerId) =>
                    {
                        peerManager.TryGetPeer(peerId, out var peer);

                        Logger.Debug($"{ProtocolHookId.Pong}");

                        peer.OnPong(dateTimeProvider.UtcNow());
                        Logger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
                    },
                    onOutputEvent: (bytes, peerId) =>
                    {
                        peerManager.TryGetPeer(peerId, out var peer);
                        Logger.Debug($"{ProtocolHookId.Pong}");

                        peer.OnPong(dateTimeProvider.UtcNow());
                        Logger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
                    },
                    onAck: (peerId) => { },
                    onAckTimeout: (peerId) => { },
                    broadcastMode: BroadcastMode.Caller);
        }
    }
}