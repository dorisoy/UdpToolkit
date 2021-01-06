namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Jobs;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Pooling;
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
            var scheduler = new Scheduler();

            var callContextPool = new ObjectsPool<CallContext>(CallContext.Create, 1000);
            var networkPacketPool = new ObjectsPool<NetworkPacket>(NetworkPacket.Create, 1000);

            var outputQueue = new BlockingAsyncQueue<PooledObject<CallContext>>(
                boundedCapacity: int.MaxValue);

            var inputQueue = new BlockingAsyncQueue<PooledObject<CallContext>>(
                boundedCapacity: int.MaxValue);

            var resendQueue = new BlockingAsyncQueue<PooledObject<CallContext>>(
                boundedCapacity: int.MaxValue);

            var peerManager = new PeerManager(
                dateTimeProvider: dateTimeProvider);

            var roomManager = new RoomManager(
                peerManager: peerManager);

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
                peerManager: peerManager,
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
                .Select(udpClient => new UdpSender(
                    sender: udpClient,
                    rawRoomManager: roomManager,
                    networkPacketPool: networkPacketPool))
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
                .Select(udpClient => new UdpReceiver(
                    peerInactivityTimeout: _hostSettings.PeerInactivityTimeout,
                    rawPeerManager: peerManager,
                    receiver: udpClient,
                    networkPacketPool: networkPacketPool))
                .ToList();

            var subscriptionManager = new SubscriptionManager();

            var protocolSubscriptionManager = new ProtocolSubscriptionManager();

            protocolSubscriptionManager.BootstrapSubscriptions(
                peerManager: peerManager,
                dateTimeProvider: dateTimeProvider,
                inactivityTimeout: _hostSettings.PeerInactivityTimeout);

            var peerId = Guid.NewGuid();

            var clientIps = inputPorts
                .Select(ip => new ClientIp(host: ip.Address.ToString(), port: ip.Port))
                .ToList();

            var serverHostClient = new ServerHostClient(
                callContextPool: callContextPool,
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
                callContextPool: callContextPool,
                dateTimeProvider: dateTimeProvider,
                hostSettings: _hostSettings,
                subscriptionManager: subscriptionManager,
                outputQueue: outputQueue,
                inputQueue: inputQueue,
                senders: senders,
                receivers: receivers,
                sendingJob: new SenderJob(
                    scheduler: scheduler,
                    resendQueue: new ResendQueue(),
                    rawPeerManager: peerManager,
                    serverSelector: randomServerSelector,
                    networkPacketPool: networkPacketPool,
                    protocolSubscriptionManager: protocolSubscriptionManager,
                    outputQueue: outputQueue),
                receivingJob: new ReceiverJob(
                    hostSettings: _hostSettings,
                    dateTimeProvider: dateTimeProvider,
                    callContextPool: callContextPool,
                    inputQueue: inputQueue),
                workerJob: new WorkerJob(
                    inputQueue: inputQueue,
                    outputQueue: outputQueue,
                    callContextPool: callContextPool,
                    subscriptionManager: subscriptionManager,
                    protocolSubscriptionManager: protocolSubscriptionManager,
                    roomManager: roomManager,
                    dateTimeProvider: dateTimeProvider,
                    hostSettings: _hostSettings,
                    scheduler: scheduler,
                    serverHostClient: serverHostClient));
        }
    }
}