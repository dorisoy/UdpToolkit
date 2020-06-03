namespace UdpToolkit.Framework.Server.Di.Autofac
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::Autofac;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Host;
    using UdpToolkit.Framework.Server.Peers;
    using UdpToolkit.Framework.Server.Pipelines;
    using UdpToolkit.Framework.Server.Rpcs;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Utils;

    public class AutofacServerHostBuilder : IServerHostBuilder
    {
        private readonly ILogger _logger = Log.ForContext<ServerHostBuilder>();

        private readonly ContainerBuilder _containerBuilder;
        private readonly ServerSettings _serverSettings;
        private Action<IPipelineBuilder> _pipelineConfigurator;

        public AutofacServerHostBuilder(ServerSettings serverSettings, Action<ContainerBuilder> configurator)
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

            _containerBuilder
                .RegisterInstance(_serverSettings.Serializer)
                .As<ISerializer>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<UdpClientFactory>()
                .As<IUdpClientFactory>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<UdpClientFactory>()
                .As<IUdpClientFactory>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<DateTimeProvider>()
                .As<IDateTimeProvider>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<DefaultFrameworkProtocol>()
                .As<IFrameworkProtocol>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<ReliableUdpProtocol>()
                .As<IReliableUdpProtocol>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<AutofacCtorArgumentsResolver>()
                .As<ICtorArgumentsResolver>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<UdpProtocol>()
                .As<IUdpProtocol>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<RpcTransformer>()
                .AsSelf()
                .SingleInstance();

            _containerBuilder
                .Register((context) =>
                {
                    var methods = MethodDescriptorStorage.HubMethods;
                    var rpcTransformer = context.Resolve<RpcTransformer>();
                    var rpcs = rpcTransformer
                        .Transform(methods)
                        .ToDictionary(rpcDescriptor => rpcDescriptor.RpcDescriptorId);

                    return new RpcProvider(rpcs);
                })
                .As<IRpcProvider>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<PeerManager>()
                .As<IPeerManager>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<RoomManager>()
                .As<IRoomManager>()
                .SingleInstance();

            _containerBuilder
                .Register((context) => new HubClients(
                    peerManager: context.Resolve<IPeerManager>(),
                    roomManager: context.Resolve<IRoomManager>(),
                    outputQueue: context.ResolveNamed<IAsyncQueue<NetworkPacket>>("outputQueue"),
                    serializer: context.Resolve<ISerializer>()))
                .As<IHubClients>()
                .SingleInstance();

            _containerBuilder
                .RegisterType<ProcessStage>()
                .AsSelf()
                .SingleInstance();

            _containerBuilder
                .RegisterInstance(new BlockingAsyncQueue<NetworkPacket>(
                        boundedCapacity: _serverSettings.InputQueueBoundedCapacity))
                .Named<IAsyncQueue<NetworkPacket>>("inputQueue")
                .SingleInstance();

            _containerBuilder
                .RegisterInstance(new BlockingAsyncQueue<NetworkPacket>(
                    boundedCapacity: _serverSettings.OutputQueueBoundedCapacity))
                .Named<IAsyncQueue<NetworkPacket>>("outputQueue")
                .SingleInstance();

            _containerBuilder
                .Register((context) => _serverSettings.InputPorts
                    .Select(port => new UdpReceiver(
                        receiver: context.Resolve<IUdpClientFactory>().Create(
                            endPoint: new IPEndPoint(
                                address: IPAddress.Parse(_serverSettings.ServerHost),
                                port: port)),
                        udpProtocol: context.Resolve<IUdpProtocol>())).ToList())
                .As<IReadOnlyCollection<IUdpReceiver>>()
                .SingleInstance();

            _containerBuilder
                .Register((context) => _serverSettings.OutputPorts
                    .Select(port => new UdpSender(
                        sender: context.Resolve<IUdpClientFactory>().Create(
                            endPoint: new IPEndPoint(
                                address: IPAddress.Parse(_serverSettings.ServerHost),
                                port: port)),
                        udpProtocol: context.Resolve<IUdpProtocol>())).ToList())
                .As<IReadOnlyCollection<IUdpSender>>()
                .SingleInstance();

            _containerBuilder
                .Register((context) =>
                {
                    var pipelineBuilder = new AutofacPipelineBuilder(
                        componentContext: context);

                    _pipelineConfigurator(pipelineBuilder);

                    return pipelineBuilder;
                })
                .As<IPipelineBuilder>()
                .SingleInstance();

            _containerBuilder
                .Register((context) => new ServerHost(
                    peerManager: context.Resolve<IPeerManager>(),
                    outputQueue: context.ResolveNamed<IAsyncQueue<NetworkPacket>>("outputQueue"),
                    inputQueue: context.ResolveNamed<IAsyncQueue<NetworkPacket>>("inputQueue"),
                    processWorkers: _serverSettings.ProcessWorkers,
                    senders: context.Resolve<IReadOnlyCollection<IUdpSender>>(),
                    receivers: context.Resolve<IReadOnlyCollection<IUdpReceiver>>(),
                    pipeline: context.Resolve<IPipelineBuilder>().Build()))
                .As<IServerHost>()
                .SingleInstance();

            _logger.Information("ServerHost created with settings: {@settings}", _serverSettings);

            return _containerBuilder.Build().Resolve<IServerHost>();
        }
    }
}