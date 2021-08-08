namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Framework.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Connections;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Network.Queues;

    public sealed class HostBuilder : IHostBuilder
    {
        private bool _clientConfigured = false;

        public HostBuilder(
            HostSettings hostSettings,
            HostClientSettings hostClientSettings,
            NetworkSettings networkSettings)
        {
            HostSettings = hostSettings;
            HostClientSettings = hostClientSettings;
            NetworkSettings = networkSettings;
        }

        private HostSettings HostSettings { get; }

        private HostClientSettings HostClientSettings { get; }

        private NetworkSettings NetworkSettings { get; }

        public IHostBuilder ConfigureNetwork(
            Action<NetworkSettings> configurator)
        {
            configurator(NetworkSettings);

            return this;
        }

        public IHostBuilder ConfigureHost(Action<HostSettings> configurator)
        {
            configurator(HostSettings);

            return this;
        }

        public IHostBuilder ConfigureHostClient(Action<HostClientSettings> configurator)
        {
            configurator(HostClientSettings);
            _clientConfigured = true;

            return this;
        }

        public IHost Build()
        {
            ValidateSettings(HostSettings);
            ValidateSettings(NetworkSettings);

            var logger = HostSettings.LoggerFactory.Create<HostBuilder>();

            var dateTimeProvider = new DateTimeProvider();
            var networkDateTimeProvider = new Network.Utils.DateTimeProvider();
            var loggerFactory = HostSettings.LoggerFactory;

            var connectionPool = new ConnectionPool(
                logger: loggerFactory.Create<ConnectionPool>(),
                networkSettings: NetworkSettings,
                dateTimeProvider: networkDateTimeProvider,
                connectionFactory: new ConnectionFactory(NetworkSettings.ChannelsFactory));

            var ips = HostSettings.HostPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(HostSettings.Host), port))
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
                .Select(ip => new UdpToolkit.Network.Clients.UdpClient(
                    resendQueue: new ResendQueue(),
                    networkSettings: NetworkSettings,
                    dateTimeProvider: networkDateTimeProvider,
                    connectionPool: connectionPool,
                    logger: loggerFactory.Create<UdpClient>(),
                    client: NetworkSettings.SocketFactory.Create(ip)))
                .ToArray();

            var hostOutQueues = udpClients.Select(sender => new BlockingAsyncQueue<OutPacket>(
                    boundedCapacity: int.MaxValue,
                    action: sender.Send,
                    logger: loggerFactory.Create<BlockingAsyncQueue<OutPacket>>()))
                .ToArray();

            var hostOutQueueDispatcher = new QueueDispatcher<OutPacket>(
                queues: hostOutQueues,
                logger: loggerFactory.Create<QueueDispatcher<OutPacket>>());

            var cancellationTokenSource = new CancellationTokenSource();

            var hostClient = BuildHostClient(
                cancellationTokenSource: cancellationTokenSource,
                dateTimeProvider: dateTimeProvider,
                connectionPool: connectionPool,
                hostOutQueueDispatcher: hostOutQueueDispatcher);

            return BuildHost(
                cancellationTokenSource: cancellationTokenSource,
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
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher,
            CancellationTokenSource cancellationTokenSource)
        {
            if (!_clientConfigured)
            {
                return new DummyHostClient();
            }

            var remoteHostIps = HostClientSettings.ServerPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(HostClientSettings.ServerHost), port))
                .Select(ipEndPoint => ipEndPoint.ToIp())
                .ToArray();

            var clientConnectionId = Guid.NewGuid();
            var randomRemoteHostIp = remoteHostIps[MurMurHash.Hash3_x86_32(clientConnectionId) % remoteHostIps.Length];

            var clientConnection = connectionPool.GetOrAdd(
                connectionId: clientConnectionId,
                keepAlive: true,
                lastHeartbeat: dateTimeProvider.UtcNow(),
                ipAddress: randomRemoteHostIp);

            var hostClient = new HostClient(
                taskFactory: new TaskFactory(),
                logger: HostSettings.LoggerFactory.Create<HostClient>(),
                dateTimeProvider: dateTimeProvider,
                connectionTimeout: HostClientSettings.ConnectionTimeout,
                clientConnection: clientConnection,
                cancellationTokenSource: cancellationTokenSource,
                outQueueDispatcher: hostOutQueueDispatcher,
                heartbeatDelayMs: HostClientSettings.HeartbeatDelayInMs,
                serializer: HostSettings.Serializer);

            WorkerJob.OnConnectionChanged += (isConnected) => hostClient.IsConnected = isConnected;

            return hostClient;
        }

        private IHost BuildHost(
            IUdpClient[] udpClients,
            IDateTimeProvider dateTimeProvider,
            IConnectionPool connectionPool,
            IUdpToolkitLoggerFactory loggerFactory,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher,
            CancellationTokenSource cancellationTokenSource)
        {
            var scheduler = new Scheduler(
                logger: HostSettings.LoggerFactory.Create<Scheduler>());

            var subscriptionManager = new SubscriptionManager();

            var roomManager = new RoomManager(
                connectionPool: connectionPool,
                dateTimeProvider: dateTimeProvider,
                roomTtl: HostSettings.RoomTtl,
                scanFrequency: HostSettings.RoomsCleanupFrequency,
                logger: loggerFactory.Create<RoomManager>());

            var broadcaster = new Broadcaster(
                logger: HostSettings.LoggerFactory.Create<Broadcaster>(),
                hostOutQueueDispatcher: hostOutQueueDispatcher,
                dateTimeProvider: dateTimeProvider,
                roomManager: roomManager,
                connectionPool: connectionPool);

            var workerJob = new WorkerJob(
                logger: loggerFactory.Create<WorkerJob>(),
                subscriptionManager: subscriptionManager,
                roomManager: roomManager,
                broadcaster: broadcaster,
                serializer: HostSettings.Serializer,
                scheduler: scheduler);

            var inQueues = Enumerable.Range(0, HostSettings.Workers)
                .Select(_ =>
                {
                    return new BlockingAsyncQueue<InPacket>(
                        boundedCapacity: int.MaxValue,
                        action: (inPacket) => workerJob.Execute(inPacket),
                        logger: loggerFactory.Create<BlockingAsyncQueue<InPacket>>());
                })
                .ToArray();

            var inQueuesDispatcher = new QueueDispatcher<InPacket>(
                logger: HostSettings.LoggerFactory.Create<QueueDispatcher<InPacket>>(),
                queues: inQueues);

            for (int i = 0; i < udpClients.Length; i++)
            {
                udpClients[i].OnPacketReceived += (inPacket) =>
                {
                    var queue = inQueuesDispatcher.Dispatch(inPacket.ConnectionId);

                    queue.Produce(@event: inPacket);
                };
            }

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
            toDispose.Add(HostSettings.Executor);

            return new Host(
                cancellationTokenSource: cancellationTokenSource,
                serializer: HostSettings.Serializer,
                udpClients: udpClients,
                logger: loggerFactory.Create<Host>(),
                executor: HostSettings.Executor,
                subscriptionManager: subscriptionManager,
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
                throw new ArgumentNullException(nameof(hostSettings.Serializer), "Serializer not provided..");
            }

            if (hostSettings.LoggerFactory == null)
            {
                throw new ArgumentNullException(nameof(hostSettings.LoggerFactory), "LoggerFactory not provided..");
            }

            if (hostSettings.Executor == null)
            {
                throw new ArgumentNullException(nameof(hostSettings.Executor), "Executor not provided..");
            }

            if (_clientConfigured && !HostClientSettings.ServerPorts.Any())
            {
                throw new ArgumentException("Remote host ports not specified for client..");
            }
        }

        private void ValidateSettings(
            NetworkSettings networkSettings)
        {
            if (networkSettings.ChannelsFactory == null)
            {
                throw new ArgumentNullException(nameof(networkSettings.ChannelsFactory), "ChannelsFactory not provided..");
            }

            if (networkSettings.SocketFactory == null)
            {
                throw new ArgumentNullException(nameof(networkSettings.SocketFactory), "SocketFactory not provided..");
            }
        }
    }
}