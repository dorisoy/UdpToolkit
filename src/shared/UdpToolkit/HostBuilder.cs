namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Core;
    using UdpToolkit.Core.Executors;
    using UdpToolkit.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Sockets;

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
            var logger = _hostSettings.LoggerFactory.Create<HostBuilder>();

            var dateTimeProvider = new DateTimeProvider();
            var networkDateTimeProvider = new Network.Utils.DateTimeProvider();
            var loggerFactory = _hostSettings.LoggerFactory;

            var connectionPool = new ConnectionPool(
                logger: loggerFactory.Create<ConnectionPool>(),
                inactivityTimeout: _hostSettings.ConnectionTtl,
                dateTimeProvider: networkDateTimeProvider,
                scanFrequency: _hostSettings.ConnectionsCleanupFrequency);

            var ips = _hostSettings.HostPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToArray();

            for (var i = 0; i < ips.Length; i++)
            {
                connectionPool.GetOrAdd(
                    connectionId: Guid.NewGuid(),
                    keepAlive: true,
                    lastHeartbeat: dateTimeProvider.UtcNow(),
                    ipAddress: ips[i].ToIp());

                logger.Debug($"Host socket created - {ips[i]}");
            }

            var udpClients = ips
                .Select(ip => new UdpClient(
                    resendQueue: new ResendQueue(),
                    resendTimeout: _hostSettings.ResendPacketsTimeout,
                    dateTimeProvider: networkDateTimeProvider,
                    connectionPool: connectionPool,
                    logger: loggerFactory.Create<UdpClient>(),
                    client: SocketFactory.Create(ip, loggerFactory)))
                .ToArray();

            var hostOutQueues = udpClients.Select(sender => new BlockingAsyncQueue<OutPacket>(
                    boundedCapacity: int.MaxValue,
                    action: sender.Send,
                    logger: loggerFactory.Create<BlockingAsyncQueue<OutPacket>>()))
                .ToArray();

            var hostOutQueueDispatcher = new QueueDispatcher<OutPacket>(
                queues: hostOutQueues,
                logger: loggerFactory.Create<QueueDispatcher<OutPacket>>());

            var hostClient = BuildHostClient(
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                hostOutQueueDispatcher: hostOutQueueDispatcher);

            return BuildHost(
                udpClients: udpClients,
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                loggerFactory: loggerFactory,
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
                .Select(ipEndPoint => ipEndPoint.ToIp())
                .ToArray();

            var clientConnectionId = Guid.NewGuid();
            var randomRemoteHostIp = remoteHostIps[MurMurHash.Hash3_x86_32(clientConnectionId) % remoteHostIps.Length];

            var clientConnection = connectionPool.GetOrAdd(
                connectionId: clientConnectionId,
                keepAlive: true,
                lastHeartbeat: dateTimeProvider.UtcNow(),
                ipAddress: randomRemoteHostIp);

            var clientBroadcaster = new ClientBroadcaster(
                outQueueDispatcher: hostOutQueueDispatcher,
                clientConnection: clientConnection,
                dateTimeProvider: dateTimeProvider);

            var hostClient = new HostClient(
                logger: _hostSettings.LoggerFactory.Create<HostClient>(),
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
            IUdpClient[] udpClients,
            IDateTimeProvider dateTimeProvider,
            IConnectionPool connectionPool,
            IUdpToolkitLoggerFactory loggerFactory,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher)
        {
            var scheduler = new Scheduler(
                logger: _hostSettings.LoggerFactory.Create<Scheduler>());

            var subscriptionManager = new SubscriptionManager();
            var executorType = _hostSettings.ExecutorType;

            var executor = ExecutorFactory.Create(
                executorType: executorType,
                logger: loggerFactory.Create<IExecutor>());

            var roomManager = new RoomManager(
                dateTimeProvider: dateTimeProvider,
                roomTtl: _hostSettings.RoomTtl,
                scanFrequency: _hostSettings.RoomsCleanupFrequency,
                logger: loggerFactory.Create<RoomManager>());

            var broadcaster = new Broadcaster(
                logger: _hostSettings.LoggerFactory.Create<Broadcaster>(),
                hostOutQueueDispatcher: hostOutQueueDispatcher,
                dateTimeProvider: dateTimeProvider,
                roomManager: roomManager,
                connectionPool: connectionPool);

            var workerJob = new WorkerJob(
                logger: loggerFactory.Create<WorkerJob>(),
                subscriptionManager: subscriptionManager,
                roomManager: roomManager,
                broadcaster: broadcaster,
                serializer: _hostSettings.Serializer);

            var inQueues = Enumerable.Range(0, _hostSettings.Workers)
                .Select(_ =>
                {
                    return new BlockingAsyncQueue<InPacket>(
                        boundedCapacity: int.MaxValue,
                        action: (inPacket) => workerJob.Execute(inPacket),
                        logger: loggerFactory.Create<BlockingAsyncQueue<InPacket>>());
                })
                .ToArray();

            var inQueuesDispatcher = new QueueDispatcher<InPacket>(
                logger: _hostSettings.LoggerFactory.Create<QueueDispatcher<InPacket>>(),
                queues: inQueues);

            for (int i = 0; i < udpClients.Length; i++)
            {
                udpClients[i].OnPacketReceived += (inPacket) =>
                {
                    var queue = inQueuesDispatcher.Dispatch(inPacket.ConnectionId);

                    queue.Produce(@event: inPacket);
                };
            }
#pragma warning disable
            var toDispose = new List<IDisposable>
            {
                scheduler,
                broadcaster,
                hostClient,
                roomManager,
                connectionPool,
                inQueuesDispatcher,
                hostOutQueueDispatcher,
                workerJob,
            };

            toDispose.AddRange(inQueues);
            toDispose.AddRange(udpClients);
            toDispose.Add(executor);
            

            return new Host(
                serializer: _hostSettings.Serializer,
                udpClients: udpClients,
                logger: loggerFactory.Create<Host>(),
                executor: executor,
                subscriptionManager: subscriptionManager,
                scheduler: scheduler,
                broadcaster: broadcaster,
                hostClient: hostClient,
                inQueueDispatcher: inQueuesDispatcher,
                hostOutQueueDispatcher: hostOutQueueDispatcher,
                toDispose: toDispose);
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