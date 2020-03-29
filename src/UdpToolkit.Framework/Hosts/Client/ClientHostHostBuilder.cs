namespace UdpToolkit.Framework.Hosts.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Events;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Utils;

    public sealed class ClientHostHostBuilder : IClientHostBuilder
    {
        private readonly ILogger _logger = Log.ForContext<ClientHostHostBuilder>();

        private readonly ClientSettings _settings;
        private readonly IContainerBuilder _containerBuilder;

        public ClientHostHostBuilder(
            ClientSettings settings, IContainerBuilder containerBuilder)
        {
            _settings = settings;
            _containerBuilder = containerBuilder;
        }

        public IClientHostBuilder Configure(Action<ClientSettings> configurator)
        {
            configurator(_settings);

            return this;
        }

        public IClientHost Build()
        {
            var localIp = new IPEndPoint(
                address: IPAddress.Any,
                port: 0);

            var container = _containerBuilder
                .RegisterSingleton<UdpClient, UdpClient>((context) => context.GetInstance<IUdpClientFactory>().Create(endPoint: localIp))
                .RegisterSingleton<IServerSelector, RandomServerSelector>((context) =>
                {
                    var now = context.GetInstance<IDateTimeProvider>().UtcNow();

                    return new RandomServerSelector(
                        servers: _settings.ServerInputPorts
                            .Select(
                                port => new IPEndPoint(
                                    IPAddress.Parse("0.0.0.0"), port))
                            .Select(endPoint => new Peer(
                                id: endPoint.ToString(),
                                ipEndPoint: endPoint,
                                reliableUdpChannel: new ReliableUdpChannel(),
                                createdAt: now,
                                lastActivityAt: now))
                            .ToArray());
                })
                .RegisterSingleton<InputDispatcher, InputDispatcher>(new InputDispatcher())
                .RegisterSingleton<IUdpClientFactory, UdpClientFactory>(new UdpClientFactory())
                .RegisterSingleton<IDateTimeProvider, DateTimeProvider>(new DateTimeProvider())
                .RegisterSingleton<IFrameworkProtocol, DefaultFrameworkProtocol>(new DefaultFrameworkProtocol())
                .RegisterSingleton<IReliableUdpProtocol, ReliableUdpProtocol>(new ReliableUdpProtocol())
                .RegisterSingleton<IUdpProtocol, UdpProtocol>((context) => new UdpProtocol(
                    frameworkProtocol: context.GetInstance<IFrameworkProtocol>(),
                    reliableUdpProtocol: context.GetInstance<IReliableUdpProtocol>(),
                    dateTimeProvider: context.GetInstance<IDateTimeProvider>()))
                .RegisterSingleton<IAsyncQueue<ProducedEvent>, BlockingAsyncQueue<ProducedEvent>>(
                    instance: new BlockingAsyncQueue<ProducedEvent>(
                        boundedCapacity: int.MaxValue),
                    name: "producedEvents")
                .RegisterSingleton<IReadOnlyCollection<IUdpReceiver>, IReadOnlyCollection<IUdpReceiver>>((context) => _settings.ServerOutputPorts
                    .Select(port => new UdpReceiver(
                        receiver: context.GetInstance<UdpClient>(),
                        udpProtocol: context.GetInstance<IUdpProtocol>()))
                    .ToList())
                .RegisterSingleton<IReadOnlyCollection<IUdpSender>, IReadOnlyCollection<IUdpSender>>((context) => _settings.ServerInputPorts
                    .Select(ip => new UdpSender(
                        sender: context.GetInstance<UdpClient>(),
                        udpProtocol: context.GetInstance<IUdpProtocol>()))
                    .ToList())
                .RegisterSingleton<IClientHost, ClientHost>((context) => new ClientHost(
                    serverSelector: context.GetInstance<IServerSelector>(),
                    serializer: _settings.Serializer,
                    producedEvents: context.GetInstance<IAsyncQueue<ProducedEvent>>("producedEvents"),
                    senders: context.GetInstance<IReadOnlyCollection<IUdpSender>>(),
                    receivers: context.GetInstance<IReadOnlyCollection<IUdpReceiver>>(),
                    inputDispatcher: context.GetInstance<InputDispatcher>()))
                .Build();

            _logger.Information("ClientHost created with settings: {@settings}", _settings);

            return container.GetInstance<IClientHost>();
        }
    }
}
