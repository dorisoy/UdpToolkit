using System;
using System.Linq;
using System.Net;
using UdpToolkit.Core;
using UdpToolkit.Network;
using UdpToolkit.Serialization;

namespace UdpToolkit.Framework
{
    public class ServerBuilder : IServerBuilder
    {
        private readonly ServerSettings _serverSettings;
        private readonly IContainerBuilder _containerBuilder;
        
        public ServerBuilder(
            ServerSettings serverSettings, 
            IContainerBuilder containerBuilder)
        {
            _serverSettings = serverSettings;
            _containerBuilder = containerBuilder;
        }
        
        public IServerBuilder Configure(Action<ServerSettings> configurator)
        {
            configurator(_serverSettings);

            return this;
        }

        public IServerBuilder ConfigureServices(Action<IContainerBuilder> configurator)
        {
            configurator(_containerBuilder);
            
            return this;
        }

        public IServer Build()
        {
            var serializer = new Serializer();
            var udpClientFactory = new UdpClientFactory();
            var rpcTransformer = new RpcTransformer();
            var methods = FrameworkExtensions.FindAllHubMethods();
            var rpcs = rpcTransformer
                    .Transform(methods: methods)
                    .ToDictionary(rpcDescriptor => 
                        new RpcDescriptorId(
                            hubId: rpcDescriptor.HubId,
                            rpcId: rpcDescriptor.RpcId));

            var peerTracker = new PeerTracker();

            var rpcProvider = new RpcProvider(rpcs);
            var container = _containerBuilder.Build();

            var inputQueue = new AsyncQueue<InputUdpPacket>(
                boundedCapacity: _serverSettings.InputQueueBoundedCapacity);
            
            var outputQueue = new AsyncQueue<OutputUdpPacket>(
                boundedCapacity: _serverSettings.OutputQueueBoundedCapacity);
            
            var udpReceivers = _serverSettings.InputPorts
                .Select(port => new UdpReceiver(
                    peerTracker: peerTracker,
                    inputQueue: inputQueue, 
                    receiver: udpClientFactory.Create(
                        endPoint: new IPEndPoint(
                            address: IPAddress.Parse(_serverSettings.Host), 
                            port: port))))
                .ToList();

            var udpSenders = _serverSettings.OutputPorts
                .Select(port => new UdpSender(
                    outputQueue: outputQueue, 
                    sender: udpClientFactory.Create(
                        endPoint: new IPEndPoint(
                            address: IPAddress.Parse(_serverSettings.Host), 
                            port: port))))
                .ToList();

            var udpSenderProxy = new UdpSenderProxy(
                outputQueue: outputQueue);
            
            var udpPacketsProcessor =
                new UdpPacketsProcessor(
                    inputQueue: inputQueue,
                    serverSettings: _serverSettings,
                    senders: udpSenders,
                    receivers: udpReceivers,
                    udpSenderProxy: udpSenderProxy,
                    peerTracker: peerTracker,
                    container: container,
                    rpcProvider: rpcProvider,
                    serializer: serializer);
            
            return new Server(
                udpPacketsProcessor: udpPacketsProcessor);
        }
    }
}
