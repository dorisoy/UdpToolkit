namespace UdpToolkit
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Core.Executors;
    using UdpToolkit.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
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

            var executor = ExecutorFactory.Create(
                executorType: executorType,
                logger: loggerFactory.Create<IExecutor>());

            var connectionPool = new ConnectionPool(
                logger: loggerFactory.Create<ConnectionPool>(),
                inactivityTimeout: _hostSettings.ConnectionTtl,
                dateTimeProvider: networkDateTimeProvider,
                scanFrequency: _hostSettings.ConnectionsCleanupFrequency);

            var hostSockets = _hostSettings.HostPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .Select(SocketFactory.Create)
                .ToArray();

            for (var i = 0; i < hostSockets.Length; i++)
            {
                var ip = (IPEndPoint)hostSockets[i].LocalEndPoint;

                connectionPool.GetOrAdd(
                    connectionId: Guid.NewGuid(),
                    keepAlive: true,
                    lastHeartbeat: dateTimeProvider.UtcNow(),
                    ip: ip);

                Console.WriteLine($"Host socket created - {ip}");
            }

            var hostOutQueues = hostSockets
                .Select(socket => new UdpSender(
                    resendQueue: new ResendQueue(),
                    resendTimeout: _hostSettings.ResendPacketsTimeout,
                    dateTimeProvider: networkDateTimeProvider,
                    connectionPool: connectionPool,
                    udpToolkitLogger: loggerFactory.Create<UdpSender>(),
                    sender: socket))
                .Select(sender => new BlockingAsyncQueue<OutPacket>(
                    boundedCapacity: int.MaxValue,
                    action: sender.Send,
                    logger: loggerFactory.Create<BlockingAsyncQueue<OutPacket>>()))
                .ToArray();

            var hostOutQueueDispatcher = new QueueDispatcher<OutPacket>(
                queues: hostOutQueues,
                executor: executor);

            var hostClient = BuildHostClient(
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                hostOutQueueDispatcher: hostOutQueueDispatcher);

            return BuildHost(
                hostSockets: hostSockets,
                executor: executor,
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                loggerFactory: loggerFactory,
                networkDateTimeProvider: networkDateTimeProvider,
                hostClient: hostClient,
                hostOutQueueDispatcher: hostOutQueueDispatcher);
        }

        private IHostClient BuildHostClient(
            IDateTimeProvider dateTimeProvider,
            IConnectionPool connectionPool,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher)
        {
            if (!_clientConfigured)
            {
                return new DummyHostClient();
            }

            var remoteHostIps = _hostClientSettings.ServerPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostClientSettings.ServerHost), port))
                .ToArray();

            var clientConnectionId = Guid.NewGuid();
            var randomRemoteHostIp = remoteHostIps[MurMurHash.Hash3_x86_32(clientConnectionId) % remoteHostIps.Length];

            var clientConnection = connectionPool.GetOrAdd(
                connectionId: clientConnectionId,
                keepAlive: true,
                lastHeartbeat: dateTimeProvider.UtcNow(),
                ip: randomRemoteHostIp);

            var clientBroadcaster = new ClientBroadcaster(
                outQueueDispatcher: hostOutQueueDispatcher,
                clientConnection: clientConnection,
                dateTimeProvider: dateTimeProvider);

            var hostClient = new HostClient(
                dateTimeProvider: dateTimeProvider,
                connectionTimeout: _hostClientSettings.ConnectionTimeout,
                clientConnection: clientConnection,
                cancellationTokenSource: new CancellationTokenSource(),
                clientBroadcaster: clientBroadcaster,
                heartbeatDelayMs: _hostClientSettings.HeartbeatDelayInMs,
                serializer: _hostSettings.Serializer);

            WorkerJob.OnConnectionChanged += (isConnected) => hostClient.IsConnected = isConnected;

            return hostClient;
        }

        private IHost BuildHost(
            Socket[] hostSockets,
            IExecutor executor,
            IDateTimeProvider dateTimeProvider,
            IConnectionPool connectionPool,
            IUdpToolkitLoggerFactory loggerFactory,
            Network.Utils.DateTimeProvider networkDateTimeProvider,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher)
        {
            var scheduler = new Scheduler();
            var subscriptionManager = new SubscriptionManager();

            var roomManager = new RoomManager(
                dateTimeProvider: dateTimeProvider,
                roomTtl: _hostSettings.RoomTtl,
                scanFrequency: _hostSettings.RoomsCleanupFrequency,
                logger: loggerFactory.Create<RoomManager>());

            var broadcaster = new Broadcaster(
                hostOutQueueDispatcher: hostOutQueueDispatcher,
                dateTimeProvider: dateTimeProvider,
                roomManager: roomManager,
                connectionPool: connectionPool);

            var inQueues = Enumerable.Range(0, _hostSettings.Workers)
                .Select(_ =>
                {
                    var workerJob = new WorkerJob(
                        udpToolkitLogger: loggerFactory.Create<WorkerJob>(),
                        subscriptionManager: subscriptionManager,
                        roomManager: roomManager,
                        broadcaster: broadcaster,
                        serializer: _hostSettings.Serializer);

                    return new BlockingAsyncQueue<InPacket>(
                        boundedCapacity: int.MaxValue,
                        action: (inPacket) => workerJob.Execute(inPacket),
                        logger: loggerFactory.Create<BlockingAsyncQueue<InPacket>>());
                })
                .ToArray();

            var inQueuesDispatcher = new QueueDispatcher<InPacket>(
                queues: inQueues,
                executor: executor);

            var receivers = hostSockets
                .Select(socket => new UdpReceiver(
                    action: (inPacket) =>
                    {
                        var queue = inQueuesDispatcher.Dispatch(inPacket.ConnectionId);

                        queue.Produce(@event: inPacket);
                    },
                    dateTimeProvider: networkDateTimeProvider,
                    udpToolkitLogger: loggerFactory.Create<UdpReceiver>(),
                    connectionPool: connectionPool,
                    receiver: socket))
                .ToArray();

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

            if (_clientConfigured && !_hostClientSettings.ServerPorts.Any())
            {
                throw new ArgumentException("Remote host ports not specified for client..");
            }
        }
    }
}