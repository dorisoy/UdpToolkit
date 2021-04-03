namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Contexts;
    using UdpToolkit.Core;
    using UdpToolkit.Core.Executors;
    using UdpToolkit.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Queues;

    public sealed class HostBuilder : IHostBuilder
    {
        private readonly HostSettings _hostSettings;
        private readonly HostClientSettings _hostClientSettings;
        private bool _clientConfigured = false;

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
            _clientConfigured = true;

            return this;
        }

        public IHost Build()
        {
            ValidateSettings(_hostSettings);

            var dateTimeProvider = new DateTimeProvider();
            var networkDateTimeProvider = new Network.Utils.DateTimeProvider();
            var loggerFactory = _hostSettings.LoggerFactory;
            var executorType = _hostSettings.ExecutorType;
            var executor = ExecutorFactory.Create(executorType, loggerFactory.Create<IExecutor>());
            var connectionPool = new ConnectionPool(
                logger: loggerFactory.Create<ConnectionPool>(),
                inactivityTimeout: _hostSettings.ConnectionTtl,
                dateTimeProvider: networkDateTimeProvider,
                scanFrequency: _hostSettings.ConnectionsCleanupFrequency);

            var outHostIps = _hostSettings.OutputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToArray();

            if (!_hostClientSettings.ServerInputPorts.Any())
            {
                _hostClientSettings.ServerInputPorts = new int[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            var remoteHostConnectionId = Guid.NewGuid();
            var remoteHostConnection = connectionPool.AddOrUpdate(
                connectionId: remoteHostConnectionId,
                keepAlive: true,
                lastHeartbeat: dateTimeProvider.UtcNow(),
                ips: _hostClientSettings.ServerInputPorts
                    .Select(port => new IPEndPoint(IPAddress.Parse(_hostClientSettings.ServerHost), port))
                    .ToList());

            var hostClient = BuildHostClient(
                executor: executor,
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                loggerFactory: loggerFactory,
                remoteHostConnection: remoteHostConnection,
                networkDateTimeProvider: networkDateTimeProvider,
                outHostIps: outHostIps);

            return BuildHost(
                executor: executor,
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                loggerFactory: loggerFactory,
                remoteHostConnection: remoteHostConnection,
                networkDateTimeProvider: networkDateTimeProvider,
                hostClient: hostClient,
                outHostIps: outHostIps);
        }

        private IHostClient BuildHostClient(
            IExecutor executor,
            IDateTimeProvider dateTimeProvider,
            IConnectionPool connectionPool,
            IUdpToolkitLoggerFactory loggerFactory,
            IConnection remoteHostConnection,
            Network.Utils.DateTimeProvider networkDateTimeProvider,
            IPEndPoint[] outHostIps)
        {
            if (!_clientConfigured)
            {
                return new DummyHostClient();
            }

            var hostConnectionId = Guid.NewGuid();
            var hostConnection = connectionPool.AddOrUpdate(
                connectionId: hostConnectionId,
                keepAlive: true,
                lastHeartbeat: dateTimeProvider.UtcNow(),
                ips: _hostSettings.InputPorts
                    .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                    .ToList());

            var clientOutQueues = outHostIps
                .Select(ip => UdpClientFactory.Create(ip, false))
                .Select(udpClient => new UdpSender(
                    resendQueue: new ResendQueue(),
                    resendTimeout: _hostSettings.ResendPacketsTimeout,
                    dateTimeProvider: networkDateTimeProvider,
                    connectionPool: connectionPool,
                    udpToolkitLogger: loggerFactory.Create<UdpSender>(),
                    sender: udpClient))
                .Select(sender => new BlockingAsyncQueue<ClientOutContext>(
                    boundedCapacity: int.MaxValue,
                    action: (clientOutContext) => sender.Send(clientOutContext.OutPacket),
                    logger: loggerFactory.Create<BlockingAsyncQueue<ClientOutContext>>()))
                .ToArray();

            var clientOutQueueDispatcher = new QueueDispatcher<ClientOutContext>(
                queues: clientOutQueues,
                executor: executor);

            var clientBroadcaster = new ClientBroadcaster(
                clientOutQueueDispatcher: clientOutQueueDispatcher,
                hostConnection: remoteHostConnection,
                dateTimeProvider: dateTimeProvider);

            clientOutQueueDispatcher.RunAll();

            var hostClient = new HostClient(
                dateTimeProvider: dateTimeProvider,
                connectionTimeout: _hostClientSettings.ConnectionTimeout,
                inputPorts: _hostSettings.InputPorts.ToArray(),
                hostConnection: hostConnection,
                cancellationTokenSource: new CancellationTokenSource(),
                clientBroadcaster: clientBroadcaster,
                heartbeatDelayMs: _hostClientSettings.HeartbeatDelayInMs,
                serializer: _hostSettings.Serializer);

            WorkerJob.OnConnectionChanged += (isConnected) => hostClient.IsConnected = isConnected;

            return hostClient;
        }

        private IHost BuildHost(
            IExecutor executor,
            IDateTimeProvider dateTimeProvider,
            IConnectionPool connectionPool,
            IUdpToolkitLoggerFactory loggerFactory,
            Network.Utils.DateTimeProvider networkDateTimeProvider,
            IHostClient hostClient,
            IConnection remoteHostConnection,
            IPEndPoint[] outHostIps)
        {
            var scheduler = new Scheduler();
            var subscriptionManager = new SubscriptionManager();

            var roomManager = new RoomManager(
                dateTimeProvider: dateTimeProvider,
                roomTtl: _hostSettings.RoomTtl,
                scanFrequency: _hostSettings.RoomsCleanupFrequency,
                logger: loggerFactory.Create<RoomManager>());

            if (!_hostSettings.OutputPorts.Any())
            {
                _hostSettings.OutputPorts = new[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            if (!_hostSettings.InputPorts.Any())
            {
                _hostSettings.InputPorts = new[]
                {
                    NetworkUtils.GetAvailablePort(),
                    NetworkUtils.GetAvailablePort(),
                };
            }

            var inHostIps = _hostSettings.InputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var hostOutQueues = outHostIps
                .Select(ip => UdpClientFactory.Create(ip, false))
                .Select(udpClient => new UdpSender(
                    resendQueue: new ResendQueue(),
                    resendTimeout: _hostSettings.ResendPacketsTimeout,
                    dateTimeProvider: networkDateTimeProvider,
                    connectionPool: connectionPool,
                    udpToolkitLogger: loggerFactory.Create<UdpSender>(),
                    sender: udpClient))
                .Select(sender => new BlockingAsyncQueue<HostOutContext>(
                    boundedCapacity: int.MaxValue,
                    action: (hostOutContext) => sender.Send(hostOutContext.OutPacket),
                    logger: loggerFactory.Create<BlockingAsyncQueue<HostOutContext>>()))
                .ToArray();

            var hostOutQueueDispatcher = new QueueDispatcher<HostOutContext>(hostOutQueues, executor: executor);

            var broadcaster = new Broadcaster(
                hostOutQueueDispatcher: hostOutQueueDispatcher,
                dateTimeProvider: dateTimeProvider,
                roomManager: roomManager,
                connectionPool: connectionPool);

            var workerJob = new WorkerJob(
                udpToolkitLogger: loggerFactory.Create<WorkerJob>(),
                subscriptionManager: subscriptionManager,
                roomManager: roomManager,
                broadcaster: broadcaster,
                serializer: _hostSettings.Serializer);

            var inQueues = inHostIps
                .Select(_ => new BlockingAsyncQueue<InContext>(
                    boundedCapacity: int.MaxValue,
                    action: (inContext) => workerJob.Execute(inContext),
                    logger: loggerFactory.Create<BlockingAsyncQueue<InContext>>()))
                .ToArray();

            var inQueuesDispatcher = new QueueDispatcher<InContext>(
                queues: inQueues,
                executor: executor);

            var receivers = inHostIps
                .Select(ip => UdpClientFactory.Create(ip, true))
                .Select(udpClient => new UdpReceiver(
                    action: (inPacket) => inQueuesDispatcher
                        .Dispatch(inPacket.ConnectionId)
                        .Produce(
                            @event: new InContext(
                                createdAt: dateTimeProvider.UtcNow(),
                                inPacket: inPacket)),
                    hostConnection: remoteHostConnection,
                    dateTimeProvider: networkDateTimeProvider,
                    udpToolkitLogger: loggerFactory.Create<UdpReceiver>(),
                    connectionPool: connectionPool,
                    receiver: udpClient));

            return new Host(
                serializer: _hostSettings.Serializer,
                receivers: receivers,
                udpToolkitLogger: loggerFactory.Create<Host>(),
                executor: executor,
                subscriptionManager: subscriptionManager,
                scheduler: scheduler,
                broadcaster: broadcaster,
                hostClient: hostClient,
                inQueueDispatcher: inQueuesDispatcher,
                hostOutQueueDispatcher: hostOutQueueDispatcher);
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