namespace UdpToolkit.Framework
{
    using System;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Core.ProtocolEvents;
    using UdpToolkit.Network.Channels;

    public static class ProtocolSubscriptionManagerExtension
    {
        private static readonly ILogger Logger = Log.ForContext<Host>();

        public static void BootstrapSubscriptions(
            this IProtocolSubscriptionManager protocolSubscriptionManager)
        {
            protocolSubscriptionManager
                .SubscribeOnInputEvent<Connect>(
                    hookId: (byte)ProtocolHookId.Connect,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var connect = serializer.DeserializeContractLess<Connect>(bytes);
                        var peer = peerManager.Create(peerId: Guid.NewGuid(), peerIps: connect.GetPeerIps());
                        timersPool.EnableResend(peer);

                        var datagram = datagramBuilder.Caller(
                            @event: new Connected(peerId),
                            peerId: peerId,
                            hookId: (byte)ProtocolHookId.Connected);

                        host.PublishInternal(
                            datagram: datagram,
                            udpMode: UdpMode.ReliableUdp,
                            serializer: serializer.SerializeContractLess);
                    });

            protocolSubscriptionManager
                .SubscribeOnOutputEvent<Connect>(
                    hookId: (byte)ProtocolHookId.Connect,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var connect = serializer.DeserializeContractLess<Connect>(bytes);
                        var peer = peerManager.Create(peerId: Guid.Empty, peerIps: connect.GetPeerIps());
                        timersPool.EnableResend(peer);
                        Logger.Debug($"Output - {nameof(Connect)}");
                    });

            protocolSubscriptionManager
                .SubscribeOnOutputEvent<Connected>(
                    hookId: (byte)ProtocolHookId.Connected,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var connected = serializer.DeserializeContractLess<Connected>(bytes);
                        peerManager.TryGetPeer(Guid.Empty, out var tmpPeer);

                        var peer = peerManager.Create(peerId: connected.PeerId, peerIps: tmpPeer.PeerIps);
                        timersPool.EnableResend(peer);
                    });

            protocolSubscriptionManager
                .SubscribeOnInputEvent<Disconnect>(
                    hookId: (byte)ProtocolHookId.Disconnect,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var disconnect = serializer.DeserializeContractLess<Disconnect>(bytes);
                        var exists = peerManager.TryGetPeer(disconnect.PeerId, out var peer);
                        if (!exists)
                        {
                            return;
                        }

                        timersPool.DisableResend(peer.PeerId);

                        var datagram = datagramBuilder.Caller(
                            @event: new Disconnected(peerId),
                            peerId: peerId,
                            hookId: (byte)ProtocolHookId.Disconnected);

                        host.PublishInternal(
                            datagram: datagram,
                            udpMode: UdpMode.ReliableUdp,
                            serializer: serializer.SerializeContractLess);
                    });

            protocolSubscriptionManager
                .SubscribeOnInputEvent<Ping>(
                    hookId: (byte)ProtocolHookId.Ping,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var exists = peerManager.TryGetPeer(peerId, out var peer);
                        if (!exists)
                        {
                            return;
                        }

                        peer.OnPing(dateTimeProvider.UtcNow());

                        var datagram = datagramBuilder.Caller(
                            @event: new Pong(),
                            peerId: peerId,
                            hookId: (byte)ProtocolHookId.Pong);

                        host.PublishInternal(
                            datagram: datagram,
                            udpMode: UdpMode.ReliableUdp,
                            serializer: serializer.SerializeContractLess);
                    });

            protocolSubscriptionManager
                .SubscribeOnOutputEvent<Ping>(
                    hookId: (byte)ProtocolHookId.Ping,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var exists = peerManager.TryGetPeer(peerId, out var peer);
                        if (!exists)
                        {
                            return;
                        }

                        peer.OnPing(dateTimeProvider.UtcNow());

                        var datagram = datagramBuilder.Caller(
                            @event: new Pong(),
                            peerId: peerId,
                            hookId: (byte)ProtocolHookId.Pong);

                        host.PublishInternal(
                            datagram: datagram,
                            udpMode: UdpMode.ReliableUdp,
                            serializer: serializer.SerializeContractLess);
                    });

            protocolSubscriptionManager
                .SubscribeOnInputEvent<Pong>(
                    hookId: (byte)ProtocolHookId.Pong,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var exists = peerManager.TryGetPeer(peerId, out var peer);
                        if (!exists)
                        {
                            return;
                        }

                        Logger.Debug($"{ProtocolHookId.Pong}");

                        peer.OnPong(dateTimeProvider.UtcNow());
                        Logger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
                    });

            protocolSubscriptionManager
                .SubscribeOnInputEvent<Pong>(
                    hookId: (byte)ProtocolHookId.Pong,
                    protocolSubscription: (bytes, peerId, host, peerManager, serializer, timersPool, datagramBuilder, dateTimeProvider) =>
                    {
                        var exists = peerManager.TryGetPeer(peerId, out var peer);
                        if (!exists)
                        {
                            return;
                        }

                        Logger.Debug($"{ProtocolHookId.Pong}");

                        peer.OnPong(dateTimeProvider.UtcNow());
                        Logger.Information($"Rtt - {peer.GetRtt().TotalMilliseconds}");
                    });
        }
    }
}