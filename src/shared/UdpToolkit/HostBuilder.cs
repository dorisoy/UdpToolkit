namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using UdpToolkit.Framework;
    using UdpToolkit.Framework.Contracts;
    using UdpToolkit.Logging;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Connections;
    using UdpToolkit.Network.Contracts.Clients;
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
            ValidateSettings(NetworkSettings);

            var dateTimeProvider = new DateTimeProvider();
            var loggerFactory = HostSettings.LoggerFactory;

            var udpClientFactory = new UdpClientFactory(
                connectionPoolSettings: new ConnectionPoolSettings(
                    connectionTimeout: NetworkSettings.ConnectionTimeout,
                    connectionsCleanupFrequency: NetworkSettings.ConnectionsCleanupFrequency),
                udpClientSettings: new UdpClientSettings(
                    mtuSizeLimit: NetworkSettings.MtuSizeLimit,
                    udpClientBufferSize: NetworkSettings.UdpClientBufferSize,
                    pollFrequency: NetworkSettings.PollFrequency,
                    allowIncomingConnections: NetworkSettings.AllowIncomingConnections,
                    resendTimeout: NetworkSettings.ResendTimeout,
                    channelsFactory: NetworkSettings.ChannelsFactory,
                    socketFactory: NetworkSettings.SocketFactory),
                loggerFactory: HostSettings.LoggerFactory,
                connectionIdFactory: NetworkSettings.ConnectionIdFactory);

            var hostIps = HostSettings.HostPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(HostSettings.Host), port))
                .Select(x => x.ToIp())
                .ToArray();

            var udpClients = hostIps
                .Select(hostIp => udpClientFactory.Create(hostIp))
                .ToArray();

            // out packets processing
            var outQueues = udpClients
                .Select(sender => new BlockingAsyncQueue<OutPacket>(
                    boundedCapacity: int.MaxValue,
                    action: (outPacket) =>
                    {
                        var bytes = HostWorkerInternal.Process(outPacket);
                        sender.Send(connectionId: outPacket.ConnectionId, channelId: outPacket.ChannelId, bytes: bytes, outPacket.IpV4Address);
                    },
                    logger: loggerFactory.Create<BlockingAsyncQueue<OutPacket>>()))
                .ToArray();

            var outQueueDispatcher = new QueueDispatcher<OutPacket>(
                queues: outQueues);

            // in packets processing
            var inQueues = Enumerable.Range(0, HostSettings.Workers)
                .Select(_ =>
                {
                    return new BlockingAsyncQueue<InPacket>(
                        boundedCapacity: int.MaxValue,
                        action: (inPacket) => HostWorkerInternal.Process(inPacket),
                        logger: loggerFactory.Create<BlockingAsyncQueue<InPacket>>());
                })
                .ToArray();

            var inQueueDispatcher = new QueueDispatcher<InPacket>(
                queues: inQueues);

            var cancellationTokenSource = new CancellationTokenSource();

            IHostClient hostClient = _clientConfigured
                ? BuildHostClient(
                    udpClients: udpClients,
                    cancellationTokenSource: cancellationTokenSource,
                    dateTimeProvider: dateTimeProvider,
                    outQueueDispatcher: outQueueDispatcher)
                : new DummyHostClient();

            SubscribeOnNetworkEvents(udpClients, hostClient, inQueueDispatcher);

            return BuildHost(
                inQueues: inQueues,
                inQueueDispatcher: inQueueDispatcher,
                cancellationTokenSource: cancellationTokenSource,
                udpClients: udpClients,
                dateTimeProvider: dateTimeProvider,
                loggerFactory: loggerFactory,
                hostClient: hostClient,
                outQueueDispatcher: outQueueDispatcher);
        }

        private void SubscribeOnNetworkEvents(
            IUdpClient[] udpClients,
            IHostClient hostClient,
            IQueueDispatcher<InPacket> inQueueDispatcher)
        {
            var client = hostClient as HostClient;

            foreach (var udpClient in udpClients)
            {
                udpClient.OnPacketExpired += (ipV4, connectionId, payload, channelId) =>
                {
                    inQueueDispatcher
                        .Dispatch(connectionId)
                        .Produce(
                            item: new InPacket(
                                channelId: channelId,
                                payload: payload,
                                connectionId: connectionId,
                                ipV4Address: ipV4,
                                expired: true));
                };

                udpClient.OnPacketReceived += (ipV4, connectionId, payload, channelId) =>
                {
                    inQueueDispatcher
                        .Dispatch(connectionId)
                        .Produce(
                            item: new InPacket(
                                channelId: channelId,
                                payload: payload,
                                connectionId: connectionId,
                                ipV4Address: ipV4,
                                expired: false));
                };

                udpClient.OnConnected += (ipV4, connectionId) =>
                {
                    client?.Connected(ipV4, connectionId);
                };

                udpClient.OnDisconnected += (ipV4, connectionId) =>
                {
                    client?.Disconnected(ipV4, connectionId);
                };

                udpClient.OnHeartbeat += (connectionId, rtt) =>
                {
                    client?.HeartbeatReceived(rtt.TotalMilliseconds);
                };
            }
        }

        private HostClient BuildHostClient(
            IUdpClient[] udpClients,
            IDateTimeProvider dateTimeProvider,
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            CancellationTokenSource cancellationTokenSource)
        {
            var remoteHostIps = HostClientSettings.ServerPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(HostClientSettings.ServerHost), port))
                .Select(ipEndPoint => ipEndPoint.ToIp())
                .ToArray();

            var connectionId = NetworkSettings.ConnectionIdFactory.Generate();

            var randomRemoteHostIp = remoteHostIps[MurMurHash.Hash3_x86_32(connectionId) % remoteHostIps.Length];
            var udpClient = udpClients[MurMurHash.Hash3_x86_32(connectionId) % udpClients.Length];

            var hostClientSettingsInternal = new HostClientSettingsInternal(
                heartbeatDelayMs: HostClientSettings.HeartbeatDelayInMs,
                connectionTimeout: HostClientSettings.ConnectionTimeout,
                serverIpV4: randomRemoteHostIp);

            var hostClient = new HostClient(
                logger: HostSettings.LoggerFactory.Create<HostClient>(),
                dateTimeProvider: dateTimeProvider,
                hostClientSettingsInternal: hostClientSettingsInternal,
                udpClient: udpClient,
                cancellationTokenSource: cancellationTokenSource,
                outQueueDispatcher: outQueueDispatcher);

            return hostClient;
        }

        private IHost BuildHost(
            IUdpClient[] udpClients,
            IDateTimeProvider dateTimeProvider,
            ILoggerFactory loggerFactory,
            IHostClient hostClient,
            IQueueDispatcher<OutPacket> outQueueDispatcher,
            IQueueDispatcher<InPacket> inQueueDispatcher,
            IAsyncQueue<InPacket>[] inQueues,
            CancellationTokenSource cancellationTokenSource)
        {
            var groupManager = new GroupManager(
                dateTimeProvider: dateTimeProvider,
                groupTtl: HostSettings.GroupTtl,
                scanFrequency: HostSettings.GroupsCleanupFrequency,
                logger: loggerFactory.Create<GroupManager>());

            var broadcaster = new Broadcaster(
                groupManager: groupManager,
                outQueueDispatcher: outQueueDispatcher);

            var scheduler = new Scheduler(
                groupTtl: HostSettings.GroupTtl,
                dateTimeProvider: dateTimeProvider,
                cleanupFrequency: HostSettings.TimersCleanupFrequency,
                logger: HostSettings.LoggerFactory.Create<Scheduler>());

            HostWorkerInternal.Logger = loggerFactory.Create<IHostWorker>();
            HostWorkerInternal.Serializer = HostSettings.Serializer;

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
                serviceProvider: new ServiceProvider(
                    groupManager: groupManager,
                    scheduler: scheduler,
                    broadcaster: broadcaster),
                cancellationTokenSource: cancellationTokenSource,
                udpClients: udpClients,
                logger: loggerFactory.Create<Host>(),
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

            if (HostWorkerInternal == null)
            {
                throw new ArgumentException("HostWorker not provided..");
            }
        }

        private void ValidateSettings(
            INetworkSettings networkSettings)
        {
            if (networkSettings.ChannelsFactory == null)
            {
                throw new ArgumentNullException(nameof(networkSettings.ChannelsFactory), "ChannelsFactory not provided..");
            }

            if (networkSettings.SocketFactory == null)
            {
                throw new ArgumentNullException(nameof(networkSettings.SocketFactory), "SocketFactory not provided..");
            }

            if (networkSettings.ConnectionIdFactory == null)
            {
                throw new ArgumentNullException(nameof(networkSettings.SocketFactory), "ConnectionIdFactory not provided..");
            }
        }
    }
}