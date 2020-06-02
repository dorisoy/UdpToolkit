namespace UdpToolkit.Framework.Server.Host
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Di;
    using UdpToolkit.Framework.Server.Peers;
    using UdpToolkit.Framework.Server.Pipelines;
    using UdpToolkit.Framework.Server.Rpcs;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Utils;

    public sealed class ServerHostBuilder : IServerHostBuilder
    {
        private readonly ILogger _logger = Log.ForContext<ServerHostBuilder>();

        private readonly ServerSettings _serverSettings;
        private readonly IContainerBuilder _containerBuilder;

        private Action<IPipelineBuilder> _pipelineConfigurator;

        public ServerHostBuilder(
            ServerSettings serverSettings,
            Action<IContainerBuilder> configurator)
        {
            _serverSettings = serverSettings;
            var containerBuilder = new ContainerBuilder();
            configurator(containerBuilder);
            _containerBuilder = containerBuilder;
        }

        public IServerHostBuilder Configure(Action<ServerSettings> configurator)
        {
            configurator(_serverSettings);

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

            var udpClientFactory = new UdpClientFactory();

            var dateTimeProvider = new DateTimeProvider();

            var frameworkProtocol = new DefaultFrameworkProtocol();

            var reliableUdpProtocol = new ReliableUdpProtocol();

            var udpProtocol = new UdpProtocol(
                frameworkProtocol: frameworkProtocol,
                reliableUdpProtocol: reliableUdpProtocol,
                dateTimeProvider: dateTimeProvider);

            var rpcTransformer = new RpcTransformer();

            var methods = MethodDescriptorStorage.HubMethods;
            var rpcs = rpcTransformer
                .Transform(methods)
                .ToDictionary(rpcDescriptor => rpcDescriptor.RpcDescriptorId);

            var rpcProvider = new RpcProvider(rpcs);

            var peerScopeTracker = new PeerScopeTracker(
                dateTimeProvider: dateTimeProvider,
                cacheEntryTtl: _serverSettings.CacheOptions.CacheEntryTtl,
                scanFrequency: _serverSettings.CacheOptions.ScanForExpirationFrequency);

            var globalScopeStage = new GlobalScopeStage(
                peerScopeTracker: peerScopeTracker,
                dateTimeProvider: dateTimeProvider);

            var outputQueue = new BlockingAsyncQueue<NetworkPacket>(
                boundedCapacity: _serverSettings.OutputQueueBoundedCapacity);

            var container = _containerBuilder.Build();

            var processStage = new ProcessStage(
                rpcProvider: rpcProvider,
                serializer: _serverSettings.Serializer,
                peerScopeTracker: peerScopeTracker,
                outputQueue: outputQueue,
                ctorArgumentsResolver: new CtorArgumentsResolver(container));

            var inputQueue = new BlockingAsyncQueue<NetworkPacket>(
                boundedCapacity: _serverSettings.InputQueueBoundedCapacity);

            var receivers = _serverSettings.InputPorts
                .Select(port => new UdpReceiver(
                    receiver: udpClientFactory.Create(
                        endPoint: new IPEndPoint(
                            address: IPAddress.Parse(_serverSettings.ServerHost),
                            port: port)),
                    udpProtocol: udpProtocol))
                .ToList();

            var senders = _serverSettings.OutputPorts
                .Select(port => new UdpSender(
                    sender: udpClientFactory.Create(
                        endPoint: new IPEndPoint(
                            address: IPAddress.Parse(_serverSettings.ServerHost),
                            port: port)),
                    udpProtocol: udpProtocol))
                .ToList();

            var pipelineBuilder = new PipelineBuilder(container);

            _pipelineConfigurator(pipelineBuilder);

            var serverHost = new ServerHost(
                outputQueue: outputQueue,
                inputQueue: inputQueue,
                processWorkers: _serverSettings.ProcessWorkers,
                senders: senders,
                receivers: receivers,
                pipeline: pipelineBuilder.Build());

            _logger.Information("ServerHost created with settings: {@settings}", _serverSettings);

            return serverHost;
        }
    }
}
