namespace UdpToolkit.Framework.Hosts.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Peers;
    using UdpToolkit.Framework.Pipelines;
    using UdpToolkit.Framework.Rpcs;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Utils;

    public sealed class ServerHostBuilder : IServerHostBuilder
    {
        private readonly ILogger _logger = Log.ForContext<ServerHostBuilder>();

        private readonly ServerSettings _settings;
        private readonly IContainerBuilder _containerBuilder;

        private Action<IPipelineBuilder> _pipelineConfigurator;

        public ServerHostBuilder(
            ServerSettings settings,
            IContainerBuilder containerBuilder)
        {
            _settings = settings;
            _containerBuilder = containerBuilder;
        }

        public IServerHostBuilder Configure(Action<ServerSettings> configurator)
        {
            configurator(_settings);

            return this;
        }

        public IServerHostBuilder ConfigureServices(Action<IContainerBuilder> configurator)
        {
            configurator(_containerBuilder);

            return this;
        }

        public IServerHostBuilder Use(Action<IPipelineBuilder> configurator)
        {
            _pipelineConfigurator = configurator;

            return this;
        }

        public IServerHost Build()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Logger.Fatal("Server down...");
                Log.CloseAndFlush();
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Log.Error("UnobservedTaskException: args - {@args} - sender {@sender}", args, sender);
            };

            var container = _containerBuilder
                .RegisterSingleton<IUdpClientFactory, UdpClientFactory>(new UdpClientFactory())
                .RegisterSingleton<IDateTimeProvider, DateTimeProvider>(new DateTimeProvider())
                .RegisterSingleton<IFrameworkProtocol, DefaultFrameworkProtocol>(new DefaultFrameworkProtocol())
                .RegisterSingleton<IReliableUdpProtocol, ReliableUdpProtocol>(new ReliableUdpProtocol())
                .RegisterSingleton<IUdpProtocol, UdpProtocol>((context) => new UdpProtocol(
                    frameworkProtocol: context.GetInstance<IFrameworkProtocol>(),
                    reliableUdpProtocol: context.GetInstance<IReliableUdpProtocol>(),
                    dateTimeProvider: context.GetInstance<IDateTimeProvider>()))
                .RegisterSingleton<RpcTransformer, RpcTransformer>(new RpcTransformer())
                .RegisterSingleton<IRpcProvider, RpcProvider>((context) =>
                {
                    var methods = MethodDescriptorStorage.HubMethods;
                    var rpcTransformer = context.GetInstance<RpcTransformer>();
                    var rpcs = rpcTransformer
                        .Transform(methods)
                        .ToDictionary(rpcDescriptor => rpcDescriptor.RpcDescriptorId);

                    return new RpcProvider(rpcs);
                })
                .RegisterSingleton<IPeerScopeTracker, PeerScopeTracker>((context) => new PeerScopeTracker(
                    dateTimeProvider: context.GetInstance<IDateTimeProvider>(),
                    cacheEntryTtl: _settings.CacheOptions.CacheEntryTtl,
                    scanFrequency: _settings.CacheOptions.ScanForExpirationFrequency))
                .RegisterSingleton<GlobalScopeStage, GlobalScopeStage>((context) => new GlobalScopeStage(
                    peerScopeTracker: context.GetInstance<IPeerScopeTracker>(),
                    dateTimeProvider: context.GetInstance<IDateTimeProvider>()))
                .RegisterSingleton<ProcessStage, ProcessStage>((context) => new ProcessStage(
                    rpcProvider: context.GetInstance<IRpcProvider>(),
                    serializer: _settings.Serializer,
                    peerScopeTracker: context.GetInstance<IPeerScopeTracker>(),
                    outputQueue: context.GetInstance<IAsyncQueue<NetworkPacket>>("outputQueue"),
                    ctorArgumentsResolver: context.GetInstance<ICtorArgumentsResolver>()))
                .RegisterSingleton<IAsyncQueue<NetworkPacket>, BlockingAsyncQueue<NetworkPacket>>(
                    instance: new BlockingAsyncQueue<NetworkPacket>(
                        boundedCapacity: _settings.InputQueueBoundedCapacity),
                    name: "inputQueue")
                .RegisterSingleton<IAsyncQueue<NetworkPacket>, BlockingAsyncQueue<NetworkPacket>>(
                    instance: new BlockingAsyncQueue<NetworkPacket>(
                        boundedCapacity: _settings.OutputQueueBoundedCapacity),
                    name: "outputQueue")
                .RegisterSingleton<IReadOnlyCollection<IUdpReceiver>, IReadOnlyCollection<IUdpReceiver>>((context) => _settings.InputPorts
                    .Select(port => new UdpReceiver(
                        receiver: context.GetInstance<IUdpClientFactory>().Create(
                            endPoint: new IPEndPoint(
                                address: IPAddress.Parse(_settings.ServerHost),
                                port: port)),
                        udpProtocol: context.GetInstance<IUdpProtocol>())).ToList())
                .RegisterSingleton<IReadOnlyCollection<IUdpSender>, IReadOnlyCollection<IUdpSender>>((context) => _settings.OutputPorts
                    .Select(port => new UdpSender(
                        sender: context.GetInstance<IUdpClientFactory>().Create(
                            endPoint: new IPEndPoint(
                                address: IPAddress.Parse(_settings.ServerHost),
                                port: port)),
                        udpProtocol: context.GetInstance<IUdpProtocol>())).ToList())
                .RegisterSingleton<IPipelineBuilder, PipelineBuilder>((context) =>
                {
                    var pipelineBuilder = new PipelineBuilder(
                        registrationContext: context);

                    _pipelineConfigurator(pipelineBuilder);

                    return pipelineBuilder;
                })
                .RegisterSingleton<IServerHost, ServerHost>((context) => new ServerHost(
                    outputQueue: context.GetInstance<IAsyncQueue<NetworkPacket>>("outputQueue"),
                    inputQueue: context.GetInstance<IAsyncQueue<NetworkPacket>>("inputQueue"),
                    processWorkers: _settings.ProcessWorkers,
                    senders: context.GetInstance<IReadOnlyCollection<IUdpSender>>(),
                    receivers: context.GetInstance<IReadOnlyCollection<IUdpReceiver>>(),
                    pipeline: context.GetInstance<IPipelineBuilder>().Build()))
                .Build();

            _logger.Information("ServerHost created with settings: {@settings}", _settings);

            return container.GetInstance<IServerHost>();
        }
    }
}
