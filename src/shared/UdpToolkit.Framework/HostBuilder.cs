namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
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

            var outputQueue = new BlockingAsyncQueue<CallContext>(
                boundedCapacity: int.MaxValue);

            var inputQueue = new BlockingAsyncQueue<CallContext>(
                boundedCapacity: int.MaxValue);

            var resendQueue = new BlockingAsyncQueue<CallContext>(
                boundedCapacity: int.MaxValue);

            var peerManager = new PeerManager(
                dateTimeProvider: dateTimeProvider);

            if (!_serverHostClientSettings.ServerInputPorts.Any())
            {
                _serverHostClientSettings.ServerInputPorts = new int[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            var inputIps = _serverHostClientSettings.ServerInputPorts
                .Select(port =>
                    new IPEndPoint(
                        address: IPAddress.Parse(ipString: _serverHostClientSettings.ServerHost),
                        port: port))
                .ToArray();

            var randomServerSelector = new RandomServerSelector(
                inputIps: inputIps);

            if (!_hostSettings.OutputPorts.Any())
            {
                _hostSettings.OutputPorts = new[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            var outputPorts = _hostSettings.OutputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var senders = outputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpSender(sender: udpClient))
                .ToList();

            var resenders = outputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpSender(sender: udpClient))
                .ToList();

            if (!_hostSettings.InputPorts.Any())
            {
                _hostSettings.InputPorts = new[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            var inputPorts = _hostSettings.InputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var receivers = inputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpReceiver(receiver: udpClient))
                .ToList();

            var subscriptionManager = new SubscriptionManager();

            var roomManager = new RoomManager(
                peerManager: peerManager);

            var protocolSubscriptionManager = new ProtocolSubscriptionManager();

            var timersPool = new TimersPool(
                dateTimeProvider: dateTimeProvider,
                resendTimeout: _hostSettings.ResendPacketsTimeout,
                peerManager: peerManager,
                roomManager: roomManager,
                protocolSubscriptionManager: protocolSubscriptionManager,
                subscriptionManager: subscriptionManager,
                resendQueue: resendQueue);

            protocolSubscriptionManager.BootstrapSubscriptions(
                timersPool: timersPool,
                serializer: _hostSettings.Serializer,
                peerManager: peerManager,
                dateTimeProvider: dateTimeProvider,
                inactivityTimeout: _hostSettings.PeerInactivityTimeout);

            var broadcastManager = new BroadcastManager(
                rawPeerManager: peerManager,
                rawRoomManager: roomManager,
                serverSelector: randomServerSelector);

            var peerId = Guid.NewGuid();

            var clientIps = inputPorts
                .Select(ip => new ClientIp(host: ip.Address.ToString(), port: ip.Port))
                .ToList();

            var serverHostClient = new ServerHostClient(
                clientIps: clientIps,
                peerId: peerId,
                cancellationTokenSource: new CancellationTokenSource(),
                outputQueue: outputQueue,
                pingDelayMs: _serverHostClientSettings.PingDelayInMs,
                resendPacketsTimeout: _serverHostClientSettings.ResendPacketsTimeout,
                connectionTimeout: _serverHostClientSettings.ConnectionTimeout,
                dateTimeProvider: dateTimeProvider,
                serializer: _hostSettings.Serializer);

            _ = peerManager.AddOrUpdate(
                inactivityTimeout: _hostSettings.PeerInactivityTimeout,
                peerId: peerId,
                ips: inputPorts);

            return new Host(
                broadcastManager: broadcastManager,
                timersPool: timersPool,
                scheduler: new Scheduler(),
                dateTimeProvider: dateTimeProvider,
                rawPeerManager: peerManager,
                serverHostClient: serverHostClient,
                protocolSubscriptionManager: protocolSubscriptionManager,
                roomManager: roomManager,
                hostSettings: _hostSettings,
                subscriptionManager: subscriptionManager,
                outputQueue: outputQueue,
                inputQueue: inputQueue,
                resendQueue: resendQueue,
                senders: senders,
                receivers: receivers,
                resenders: resenders);
        }
    }
}