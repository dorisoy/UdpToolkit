namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Jobs;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public sealed class HostBuilder : IHostBuilder
    {
        private readonly HostSettings _hostSettings;
        private readonly HostClientSettings _hostClientSettings;

        public HostBuilder(
            HostSettings hostSettings,
            HostClientSettings hostClientSettings)
        {
            _hostSettings = hostSettings;
            _hostClientSettings = hostClientSettings;
        }

        public IHostBuilder ConfigureHost(Action<HostSettings> configurator)
        {
            configurator(_hostSettings);

            return this;
        }

        public IHostBuilder ConfigureHostClient(Action<HostClientSettings> configurator)
        {
            configurator(_hostClientSettings);

            return this;
        }

        public IHost Build()
        {
            ValidateSettings(_hostSettings);

            var dateTimeProvider = new DateTimeProvider();
            var networkDateTimeProvider = new Network.Utils.DateTimeProvider();
            var scheduler = new Scheduler();
            var loggerFactory = _hostSettings.LoggerFactory;

            var hostOutQueue = new BlockingAsyncQueue<HostOutContext>(
                boundedCapacity: int.MaxValue);

            var clientOutQueue = new BlockingAsyncQueue<ClientOutContext>(
                boundedCapacity: int.MaxValue);

            var inputQueue = new BlockingAsyncQueue<InContext>(
                boundedCapacity: int.MaxValue);

            var connectionPool = new ConnectionPool(networkDateTimeProvider);

            var roomManager = new RoomManager();

            if (!_hostClientSettings.ServerInputPorts.Any())
            {
                _hostClientSettings.ServerInputPorts = new int[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

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
                .Select(UdpClientFactory.Create)
                .Select(udpClient => new UdpSender(
                    resendQueue: new ResendQueue(),
                    resendTimeout: _hostClientSettings.ResendPacketsTimeout,
                    dateTimeProvider: networkDateTimeProvider,
                    connectionPool: connectionPool,
                    udpToolkitLogger: loggerFactory.Create<UdpSender>(),
                    sender: udpClient))
                .ToList();

            if (!_hostSettings.InputPorts.Any())
            {
                _hostSettings.InputPorts = new[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            var inputEndPoints = _hostSettings.InputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var hostConnectionId = Guid.NewGuid();
            var remoteHostConnectionId = Guid.NewGuid();

            var hostConnection = connectionPool.AddOrUpdate(
                connectionTimeout: _hostSettings.InactivityTimeout,
                connectionId: hostConnectionId,
                ips: inputEndPoints);

            var remoteHostConnection = connectionPool.AddOrUpdate(
                connectionTimeout: _hostSettings.InactivityTimeout,
                connectionId: remoteHostConnectionId,
                ips: _hostClientSettings.ServerInputPorts
                    .Select(port => new IPEndPoint(IPAddress.Parse(_hostClientSettings.ServerHost), port))
                    .ToList());

            var receivers = inputEndPoints
                .Select(UdpClientFactory.Create)
                .Select(udpClient => new UdpReceiver(
                    remoteHostConnection: remoteHostConnection,
                    dateTimeProvider: networkDateTimeProvider,
                    udpToolkitLogger: loggerFactory.Create<UdpReceiver>(),
                    connectionInactivityTimeout: _hostSettings.InactivityTimeout,
                    connectionPool: connectionPool,
                    receiver: udpClient))
                .ToList();

            var broadcaster = new Broadcaster(
                remoteHostConnection: remoteHostConnection,
                clientOutQueue: clientOutQueue,
                hostOutQueue: hostOutQueue,
                dateTimeProvider: dateTimeProvider,
                roomManager: roomManager,
                connectionPool: connectionPool,
                hostSettings: _hostSettings);

            var hostClient = new HostClient(
                remoteHostConnection: remoteHostConnection,
                inputPorts: _hostSettings.InputPorts.ToArray(),
                hostConnection: hostConnection,
                cancellationTokenSource: new CancellationTokenSource(),
                broadcaster: broadcaster,
                heartbeatDelayMs: _hostClientSettings.HeartbeatDelayInMs,
                resendPacketsTimeout: _hostClientSettings.ResendPacketsTimeout,
                connectionTimeout: _hostClientSettings.ConnectionTimeout,
                serializer: _hostSettings.Serializer);

            var subscriptionManager = new SubscriptionManager();

            return new Host(
                udpToolkitLogger: loggerFactory.Create<Host>(),
                hostSettings: _hostSettings,
                subscriptionManager: subscriptionManager,
                inputQueue: inputQueue,
                senders: senders,
                receivers: receivers,
                scheduler: scheduler,
                broadcaster: broadcaster,
                clientSenderJob: new ClientSenderJob(
                    clientOutQueue: clientOutQueue),
                hostSendingJob: new HostSenderJob(
                    hostOutQueue: hostOutQueue),
                receivingJob: new ReceiverJob(
                    hostSettings: _hostSettings,
                    dateTimeProvider: dateTimeProvider,
                    inputQueue: inputQueue),
                workerJob: new WorkerJob(
                    udpToolkitLogger: loggerFactory.Create<WorkerJob>(),
                    inputQueue: inputQueue,
                    subscriptionManager: subscriptionManager,
                    roomManager: roomManager,
                    broadcaster: broadcaster,
                    hostSettings: _hostSettings,
                    hostClient: hostClient));
        }

        private void ValidateSettings(
            HostSettings hostSettings)
        {
            if (hostSettings.Serializer == null)
            {
                throw new ArgumentNullException(nameof(hostSettings.Serializer), "PLease install serializer NuGet Package, or write your own..");
            }

            if (hostSettings.LoggerFactory == null)
            {
                throw new ArgumentNullException(nameof(hostSettings.Serializer), "PLease install logging NuGet Package, or write your own..");
            }
        }
    }
}