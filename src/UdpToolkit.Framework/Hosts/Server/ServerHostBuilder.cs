using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using UdpToolkit.Core;
using UdpToolkit.Framework.Rpcs;
using UdpToolkit.Network.Clients;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Peers;
using UdpToolkit.Network.Protocol;
using UdpToolkit.Network.Queues;

namespace UdpToolkit.Framework.Hosts.Server
{
    public sealed class ServerHostBuilder : IServerHostBuilder
    {
        private readonly ILogger _logger = Log.ForContext<ServerHostBuilder>();
        
        private readonly ServerSettings _settings;
        private readonly IContainerBuilder _containerBuilder;
        
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

        public IServerHost Build()
        {
            var container = _containerBuilder.Build();
            
                        var udpClientFactory = new UdpClientFactory();
            var rpcTransformer = new RpcTransformer();
            var methods = MethodDescriptorStorage.HubMethods;
            var rpcs = rpcTransformer
                    .Transform(methods: methods)
                    .ToDictionary(rpcDescriptor => 
                        new RpcDescriptorId(
                            hubId: rpcDescriptor.HubId,
                            rpcId: rpcDescriptor.RpcId));

            var peerTracker = new ServerPeerTracker();

            var rpcProvider = new RpcProvider(rpcs);

            var inputQueue = new BlockingAsyncQueue<InputUdpPacket>(
                boundedCapacity: _settings.InputQueueBoundedCapacity);
            
            var outputQueue = new BlockingAsyncQueue<OutputUdpPacket>(
                boundedCapacity: _settings.OutputQueueBoundedCapacity);
            
            var frameworkProtocol = new DefaultFrameworkProtocol();
            var reliableUdpProtocol = new ReliableUdpProtocol();
            
            var udpProtocol = new UdpProtocol(
                frameworkProtocol: frameworkProtocol,
                reliableUdpProtocol: reliableUdpProtocol);
            
            var receivers = _settings.InputPorts
                .Select(port => new UdpReceiver(
                    receiver: udpClientFactory.Create(
                        endPoint: new IPEndPoint(
                            address: IPAddress.Parse(_settings.ServerHost), 
                            port: port)),
                    udpProtocol: udpProtocol))
                .ToList();

            var senders = _settings.OutputPorts
                .Select(port => new UdpSender(
                    sender: udpClientFactory.Create(endPoint: new IPEndPoint(
                        address: IPAddress.Parse(_settings.ServerHost), 
                        port: port)), 
                    udpProtocol: udpProtocol))
                .ToList();

            _logger.Information(
                messageTemplate: "ServerHost created with settings: {@settings}, receivers count: {@receivers}, senders count: {@senders}, methods: {@methods}", 
                _settings, 
                receivers.Count, 
                senders.Count,
                methods);

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.Logger.Fatal("Server down...");
                Log.CloseAndFlush();
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Console.WriteLine("dead task");
            };
            
            return new ServerHost(
                    outputQueue: outputQueue,
                    inputQueue: inputQueue,
                    processWorkers: _settings.ProcessWorkers,
                    senders: senders,
                    receivers: receivers,
                    peerTracker: peerTracker,
                    container: container,
                    rpcProvider: rpcProvider,
                    serializer: _settings.Serializer);
        }
    }
}
