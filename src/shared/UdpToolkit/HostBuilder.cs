namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Framework.Contracts.Events;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Contracts;
    using UdpToolkit.Network.Contracts.Clients;
    using UdpToolkit.Network.Contracts.Pooling;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <inheritdoc />
    public sealed class HostBuilder : IHostBuilder
    {
        private bool _clientConfigured = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilder"/> class.
        /// </summary>
        /// <param name="hostClientSettings">Host client settings.</param>
        /// <param name="networkSettings">Network settings.</param>
        public HostBuilder(
            HostClientSettings hostClientSettings,
            INetworkSettings networkSettings)
        {
            HostClientSettings = hostClientSettings;
            NetworkSettings = networkSettings;
        }

        private HostSettings HostSettings { get; set; }

        private HostClientSettings HostClientSettings { get; }

        private INetworkSettings NetworkSettings { get; }

        private IHostWorker HostWorkerInternal { get; set; }

        /// <inheritdoc/>
        public IHostBuilder ConfigureNetwork(
            Action<INetworkSettings> configurator)
        {
            configurator(NetworkSettings);

            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder ConfigureHost(
            HostSettings settings,
            Action<HostSettings> configurator)
        {
            configurator(settings);
            HostSettings = settings;

            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder ConfigureHostClient(
            Action<HostClientSettings> configurator)
        {
            configurator(HostClientSettings);
            _clientConfigured = true;

            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder BootstrapWorker(
            IHostWorker hostWorker)
        {
            HostWorkerInternal = hostWorker;

            return this;
        }

        /// <inheritdoc/>
        public IHost Build()
        {
            ValidateSettings(HostSettings);

            var dateTimeProvider = new DateTimeProvider();
            var hostEventReporter = HostSettings.HostEventReporter;

            var udpClientFactory = new UdpClientFactory(
                networkSettings: NetworkSettings);

            var hostIps = HostSettings.HostPorts
                .Select(port => new IpV4Address(IpUtils.ToInt(HostSettings.Host), (ushort)port))
                .ToArray();

            var udpClients = hostIps
                .Select((hostIp, id) => udpClientFactory.Create(id: $"UdpClient_{id}", hostIp))
                .ToArray();

            // out packets processing
            var outQueues = udpClients
                .Select((sender, id) => new AsyncQueue<OutNetworkPacket>(
                    id: $"Sender_{id}",
                    action: (outPacket) =>
                    {
                        using (outPacket)
                        {
                            sender.Send(
                                connectionId: outPacket.ConnectionId,
                                channelId: outPacket.ChannelId,
                                dataType: outPacket.DataType,
                                payload: outPacket.BufferWriter.WrittenSpan,
                                ipV4Address: outPacket.IpV4Address);
                        }
                    },
                    hostEventReporter: hostEventReporter))
                .ToArray();

            var outQueueDispatcher = new QueueDispatcher<OutNetworkPacket>(
                queues: outQueues);

            // in packets processing
            var inQueues = Enumerable.Range(0, HostSettings.Workers)
                .Select(id =>
                {
                    return new AsyncQueue<InNetworkPacket>(
                        id: $"Worker_{id}",
                        action: (networkPacket) =>
                        {
                            using (networkPacket)
                            {
                                HostWorkerInternal.Process(networkPacket);
                            }
                        },
                        hostEventReporter: hostEventReporter);
                })
                .ToArray();

            var inQueueDispatcher = new QueueDispatcher<InNetworkPacket>(
                queues: inQueues);

            var cancellationTokenSource = new CancellationTokenSource();

            var outPacketsPool = new ConcurrentPool<OutNetworkPacket>(
                factory: (pool) => new OutNetworkPacket(pool),
                initSize: NetworkSettings.PacketsPoolSize);

            var hostClient = _clientConfigured
                ? (IHostClient)BuildHostClient(
                    udpClients: udpClients,
                    cancellationTokenSource: cancellationTokenSource,
                    outQueueDispatcher: outQueueDispatcher,
                    outPacketsPool: outPacketsPool)
                : new DummyHostClient();

            SubscribeOnNetworkEvents(udpClients, hostClient, inQueueDispatcher);

            return BuildHost(
                outPacketsPool: outPacketsPool,
                inQueues: inQueues,
                inQueueDispatcher: inQueueDispatcher,
                cancellationTokenSource: cancellationTokenSource,
                udpClients: udpClients,
                dateTimeProvider: dateTimeProvider,
                hostEventReporter: hostEventReporter,
                hostClient: hostClient,
                outQueueDispatcher: outQueueDispatcher);
        }

        private void SubscribeOnNetworkEvents(
            IUdpClient[] udpClients,
            IHostClient hostClient,
            IQueueDispatcher<InNetworkPacket> inQueueDispatcher)
        {
            var client = hostClient as HostClient;

            foreach (var udpClient in udpClients)
            {
                udpClient.OnPacketExpired += (pendingPacket) =>
                {
                    HostWorkerInternal.Process(in pendingPacket);
                };

                udpClient.OnPacketReceived += (networkPacket) =>
                {
                    inQueueDispatcher
                        .Dispatch(networkPacket.ConnectionId)
                        .Produce(networkPacket);
                };

                udpClient.OnConnected += (ipV4, connectionId) =>
                {
                    client?.Connected(ipV4, connectionId);
                };

                udpClient.OnDisconnected += (ipV4, connectionId) =>
                {
                    client?.Disconnected(ipV4, connectionId);
                };

                udpClient.OnPing += (connectionId, rtt) =>
                {
                    client?.RttReceived(rtt);
                };
            }
        }

        private HostClient BuildHostClient(
            IUdpClient[] udpClients,
            IQueueDispatcher<OutNetworkPacket> outQueueDispatcher,
            CancellationTokenSource cancellationTokenSource,
            ConcurrentPool<OutNetworkPacket> outPacketsPool)
        {
            var remoteHostIps = HostClientSettings.ServerPorts
                .Select(port => new IpV4Address(IpUtils.ToInt(HostClientSettings.ServerHost), (ushort)port))
                .ToArray();

            var seed = Guid.NewGuid();

            var randomRemoteHostIp = remoteHostIps[MurMurHash.Hash3_x86_32(seed) % remoteHostIps.Length];
            var udpClient = udpClients[MurMurHash.Hash3_x86_32(seed) % udpClients.Length];

            var hostClient = new HostClient(
                serializer: HostSettings.Serializer,
                hostWorker: HostWorkerInternal,
                outPacketsPool: outPacketsPool,
                serverIpAddress: randomRemoteHostIp,
                udpClient: udpClient,
                cancellationTokenSource: cancellationTokenSource,
                outQueueDispatcher: outQueueDispatcher);

            return hostClient;
        }

        private IHost BuildHost(
            IUdpClient[] udpClients,
            IDateTimeProvider dateTimeProvider,
            IHostEventReporter hostEventReporter,
            IHostClient hostClient,
            IQueueDispatcher<OutNetworkPacket> outQueueDispatcher,
            IQueueDispatcher<InNetworkPacket> inQueueDispatcher,
            IAsyncQueue<InNetworkPacket>[] inQueues,
            CancellationTokenSource cancellationTokenSource,
            ConcurrentPool<OutNetworkPacket> outPacketsPool)
        {
            var sharedConnectionsPool = udpClients
                .First()
                .GetConnectionPool();

            var groupManager = new GroupManager(
                dateTimeProvider: dateTimeProvider,
                groupTtl: HostSettings.GroupTtl,
                scanFrequency: HostSettings.GroupsCleanupFrequency,
                hostEventReporter: hostEventReporter,
                connectionPool: sharedConnectionsPool);

            var scheduler = new Scheduler(
                groupTtl: HostSettings.GroupTtl,
                dateTimeProvider: dateTimeProvider,
                cleanupFrequency: HostSettings.TimersCleanupFrequency,
                hostEventReporter: hostEventReporter);

            var broadcaster = new Broadcaster(
                serializer: HostSettings.Serializer,
                scheduler: scheduler,
                hostWorker: HostWorkerInternal,
                connectionPool: sharedConnectionsPool,
                groupManager: groupManager,
                outQueueDispatcher: outQueueDispatcher,
                pool: outPacketsPool);

            HostWorkerInternal.Serializer = HostSettings.Serializer;
            HostWorkerInternal.Broadcaster = broadcaster;

            var toDispose = new List<IDisposable>
            {
                scheduler,
                hostClient,
                groupManager,
                inQueueDispatcher,
                outQueueDispatcher,
                HostWorkerInternal,
                broadcaster,
            };

            toDispose.AddRange(inQueues);
            toDispose.AddRange(udpClients);
            toDispose.Add(HostSettings.Executor);

            return new Host(
                resendInterval: HostSettings.ResendPacketsInterval,
                serviceProvider: new ServiceProvider(
                    groupManager: groupManager,
                    broadcaster: broadcaster),
                cancellationTokenSource: cancellationTokenSource,
                udpClients: udpClients,
                hostEventReporter: hostEventReporter,
                executor: HostSettings.Executor,
                hostClient: hostClient,
                inQueueDispatcher: inQueueDispatcher,
                outQueueDispatcher: outQueueDispatcher,
                toDispose: toDispose);
        }

        private void ValidateSettings(
            HostSettings hostSettings)
        {
            if (hostSettings.Serializer == null)
            {
                throw new ArgumentNullException(nameof(hostSettings.Serializer), "Serializer not provided..");
            }

            if (_clientConfigured && !HostClientSettings.ServerPorts.Any())
            {
                throw new ArgumentException("Remote host ports not specified for client..");
            }

            if (HostWorkerInternal == null)
            {
                throw new ArgumentException("HostWorker not provided..");
            }
        }
    }
}