namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Settings;
    using UdpToolkit.Framework.Jobs;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Packets;
    using UdpToolkit.Network.Contracts.Sockets;

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

            var dateTimeProvider = new DateTimeProvider();
            var loggerFactory = HostSettings.LoggerFactory;

            var udpClientFactory = new UdpClientFactory(
                networkSettings: NetworkSettings,
                loggerFactory: HostSettings.LoggerFactory);

            var hostIps = HostSettings.HostPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(HostSettings.Host), port))
                .Select(x => x.ToIp())
                .ToArray();

            var udpClients = hostIps
                .Select(hostIp => udpClientFactory.Create(hostIp))
                .ToArray();

            var hostOutQueues = udpClients
                .Select(sender => new BlockingAsyncQueue<OutPacket>(
                    boundedCapacity: int.MaxValue,
                    action: sender.Send,
                    logger: loggerFactory.Create<BlockingAsyncQueue<OutPacket>>()))
                .ToArray();

            var hostOutQueueDispatcher = new QueueDispatcher<OutPacket>(
                queues: hostOutQueues,
                logger: loggerFactory.Create<QueueDispatcher<OutPacket>>());

            var cancellationTokenSource = new CancellationTokenSource();

            var hostClient = BuildHostClient(
                hostIps: hostIps,
                udpClientFactory: udpClientFactory,
                cancellationTokenSource: cancellationTokenSource,
                dateTimeProvider: dateTimeProvider,
                hostOutQueueDispatcher: hostOutQueueDispatcher);

            return BuildHost(
                cancellationTokenSource: cancellationTokenSource,
                udpClients: udpClients,
                dateTimeProvider: dateTimeProvider,
                loggerFactory: loggerFactory,
                hostClient: hostClient,
                hostOutQueueDispatcher: hostOutQueueDispatcher);
        }

        private IHostClient BuildHostClient(
            IpV4Address[] hostIps,
            UdpClientFactory udpClientFactory,
            IDateTimeProvider dateTimeProvider,
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

            var connectionId = HostSettings.ConnectionIdFactory.Generate();

            var randomRemoteHostIp = remoteHostIps[MurMurHash.Hash3_x86_32(connectionId) % remoteHostIps.Length];
            var randomHostIp = hostIps[MurMurHash.Hash3_x86_32(connectionId) % hostIps.Length];

            udpClientFactory.BootstrapConnection(connectionId, randomHostIp);

            var hostClientSettingsInternal = new HostClientSettingsInternal(
                heartbeatDelayMs: HostClientSettings.HeartbeatDelayInMs,
                connectionTimeout: HostClientSettings.ConnectionTimeout,
                connectionId: connectionId,
                serverIpV4: randomRemoteHostIp);

            var hostClient = new HostClient(
                udpClientFactory: udpClientFactory,
                taskFactory: new TaskFactory(),
                logger: HostSettings.LoggerFactory.Create<HostClient>(),
                dateTimeProvider: dateTimeProvider,
                hostClientSettingsInternal: hostClientSettingsInternal,
                cancellationTokenSource: cancellationTokenSource,
                outQueueDispatcher: hostOutQueueDispatcher,
                serializer: HostSettings.Serializer);

            WorkerJob.OnConnectionChanged += (isConnected) => hostClient.IsConnected = isConnected;

            return hostClient;
        }

        private IHost BuildHost(
            IUdpClient[] udpClients,
            IDateTimeProvider dateTimeProvider,
            IUdpToolkitLoggerFactory loggerFactory,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> hostOutQueueDispatcher,
            CancellationTokenSource cancellationTokenSource)
        {
            var scheduler = new Scheduler(
                logger: HostSettings.LoggerFactory.Create<Scheduler>());

            var subscriptionManager = new SubscriptionManager();

            var roomManager = new RoomManager(
                dateTimeProvider: dateTimeProvider,
                roomTtl: HostSettings.RoomTtl,
                scanFrequency: HostSettings.RoomsCleanupFrequency,
                logger: loggerFactory.Create<RoomManager>());

            var broadcaster = new Broadcaster(
                logger: HostSettings.LoggerFactory.Create<Broadcaster>(),
                hostOutQueueDispatcher: hostOutQueueDispatcher,
                dateTimeProvider: dateTimeProvider,
                roomManager: roomManager);

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