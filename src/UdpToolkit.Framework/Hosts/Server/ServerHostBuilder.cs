using System;
using System.Linq;
using System.Net;
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
        private readonly ServerSettings _serverSettings;
        private readonly IContainerBuilder _containerBuilder;
        
        public ServerHostBuilder(
            ServerSettings serverSettings, 
            IContainerBuilder containerBuilder)
        {
            _serverSettings = serverSettings;
            _containerBuilder = containerBuilder;
        }
        
        public IServerHostBuilder Configure(Action<ServerSettings> configurator)
        {
            configurator(_serverSettings);

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
                boundedCapacity: _serverSettings.InputQueueBoundedCapacity);
            
            var outputQueue = new BlockingAsyncQueue<OutputUdpPacket>(
                boundedCapacity: _serverSettings.OutputQueueBoundedCapacity);
            
            var frameworkProtocol = new DefaultFrameworkProtocol();
            var reliableUdpProtocol = new ReliableUdpProtocol();
            
            var udpProtocol = new UdpProtocol(
                frameworkProtocol: frameworkProtocol,
                reliableUdpProtocol: reliableUdpProtocol);
            
            var udpReceivers = _serverSettings.InputPorts
                .Select(port => new UdpReceiver(
                    receiver: udpClientFactory.Create(
                        endPoint: new IPEndPoint(
                            address: IPAddress.Parse(_serverSettings.ServerHost), 
                            port: port)),
                    udpProtocol: udpProtocol))
                .ToList();

            var udpSenders = _serverSettings.OutputPorts
                .Select(port => new UdpSender(
                    sender: udpClientFactory.Create(endPoint: new IPEndPoint(
                        address: IPAddress.Parse(_serverSettings.ServerHost), 
                        port: port)), 
                    udpProtocol: udpProtocol))
                .ToList();

            return new ServerHost(
                    outputQueue: outputQueue,
                    inputQueue: inputQueue,
                    processWorkers: _serverSettings.ProcessWorkers,
                    senders: udpSenders,
                    receivers: udpReceivers,
                    peerTracker: peerTracker,
                    container: container,
                    rpcProvider: rpcProvider,
                    serializer: _serverSettings.Serializer);
        }
    }
}
