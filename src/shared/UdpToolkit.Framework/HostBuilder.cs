namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class HostBuilder : IHostBuilder
    {
        private readonly HostSettings _hostSettings;
        private readonly ServerHostClientSettings _serverHostClientSettings;

        public HostBuilder(
            HostSettings hostSettings,
            ServerHostClientSettings serverHostClientSettings)
        {
            _hostSettings = hostSettings;
            _serverHostClientSettings = serverHostClientSettings;
        }

        public IHostBuilder ConfigureHost(Action<HostSettings> configurator)
        {
            configurator(_hostSettings);

            return this;
        }

        public IHostBuilder ConfigureServerHostClient(Action<ServerHostClientSettings> configurator)
        {
            configurator(_serverHostClientSettings);

            return this;
        }

        public IHost Build()
        {
            var dateTimeProvider = new DateTimeProvider();
            var udpClientFactory = new UdpClientFactory();

            var outputQueue = new BlockingAsyncQueue<NetworkPacket>(
                boundedCapacity: int.MaxValue);

            var inputQueue = new BlockingAsyncQueue<NetworkPacket>(
                boundedCapacity: int.MaxValue);

            var peerManager = new PeerManager(
                dateTimeProvider: dateTimeProvider);

            var serverIps = _serverHostClientSettings.ServerPorts
                .Select(port =>
                    new IPEndPoint(
                        address: IPAddress.Parse(ipString: _serverHostClientSettings.ServerHost),
                        port: port))
                .ToArray();

            var randomServerSelector = new RandomServerSelector(serverIps);

            var udpProtocol = new UdpProtocol();

            var outputPorts = _hostSettings.OutputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var senders = outputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpSender(sender: udpClient, udpProtocol: udpProtocol))
                .ToList();

            var inputPorts = _hostSettings.InputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var receivers = inputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpReceiver(
                    receiver: udpClient,
                    udpProtocol: udpProtocol,
                    resendPacketTimeout: _hostSettings.ResendPacketsTimeout))
                .ToList();

            var subscriptionManager = new SubscriptionManager();

            var roomManager = new RoomManager(
                peerManager: peerManager);

            var protocolSubscriptionManager = new ProtocolSubscriptionManager();

            var timersPool = new TimersPool(
                peerManager: peerManager,
                roomManager: roomManager,
                protocolSubscriptionManager: protocolSubscriptionManager,
                subscriptionManager: subscriptionManager,
                outputQueue: outputQueue);

            protocolSubscriptionManager.BootstrapSubscriptions(
                timersPool: timersPool,
                serializer: _hostSettings.Serializer,
                peerManager: peerManager,
                dateTimeProvider: dateTimeProvider,
                inactivityTimeout: _hostSettings.PeerInactivityTimeout);

            var broadcastStrategyResolver = new BroadcastStrategyResolver(
                broadcastStrategies: new IBroadcastStrategy[]
                {
                    new BroadcastCallerStrategy(
                        type: BroadcastType.Caller,
                        rawPeerManager: peerManager,
                        outputQueue: outputQueue),

                    new BroadcastRoomStrategy(
                        type: BroadcastType.Room,
                        rawRoomManager: roomManager,
                        outputQueue: outputQueue),

                    new BroadcastExceptCallerStrategy(
                        type: BroadcastType.ExceptCaller,
                        rawRoomManager: roomManager,
                        outputQueue: outputQueue),

                    new BroadcastServerStrategy(
                        type: BroadcastType.Server,
                        rawPeerManager: peerManager,
                        outputQueue: outputQueue),
                });

            var serverHostClient = new ServerHostClient(
                peerInactivityTimeout: _hostSettings.PeerInactivityTimeout,
                pingDelayMs: _serverHostClientSettings.PingDelayInMs,
                broadcastStrategyResolver: broadcastStrategyResolver,
                peerManager: peerManager,
                inputPorts: _hostSettings.InputPorts.ToList(),
                clientHost: _serverHostClientSettings.ClientHost,
                resendPacketsTimeout: _serverHostClientSettings.ResendPacketsTimeout,
                connectionTimeout: _serverHostClientSettings.ConnectionTimeout,
                dateTimeProvider: dateTimeProvider,
                serverSelector: randomServerSelector,
                serializer: _hostSettings.Serializer);

            return new Host(
                broadcastStrategyResolver: broadcastStrategyResolver,
                rawPeerManager: peerManager,
                serverHostClient: serverHostClient,
                protocolSubscriptionManager: protocolSubscriptionManager,
                roomManager: roomManager,
                hostSettings: _hostSettings,
                subscriptionManager: subscriptionManager,
                outputQueue: outputQueue,
                inputQueue: inputQueue,
                senders: senders,
                receivers: receivers);
        }
    }
}