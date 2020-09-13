namespace UdpToolkit.Framework
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Queues;

    public sealed class HostBuilder : IHostBuilder
    {
        private readonly HostSettings _hostSettings;
        private readonly ServerHostClientSettings _serverHostClientSettings;

        public HostBuilder(
            HostSettings hostSettings,
            ServerHostClientSettings serverHostClientSettings)
        {
            _hostSettings = hostSettings;
            _serverHostClientSettings = serverHostClientSettings;
        }

        public IHostBuilder ConfigureHost(Action<HostSettings> configurator)
        {
            configurator(_hostSettings);

            return this;
        }

        public IHostBuilder ConfigureServerHostClient(Action<ServerHostClientSettings> configurator)
        {
            configurator(_serverHostClientSettings);

            return this;
        }

        public IHost Build()
        {
            var udpClientFactory = new UdpClientFactory();

            var servers = _serverHostClientSettings.ServerPorts
                .Select(port =>
                    new IPEndPoint(
                        address: IPAddress.Parse(ipString: _serverHostClientSettings.ServerHost),
                        port: port))
                .ToArray();

            var randomServerSelector = new RandomServerSelector(servers: servers);

            var udpProtocol = new UdpProtocol();

            var outputQueue = new BlockingAsyncQueue<NetworkPacket>(
                boundedCapacity: int.MaxValue);

            var inputQueue = new BlockingAsyncQueue<NetworkPacket>(
                boundedCapacity: int.MaxValue);

            var outputPorts = _hostSettings.OutputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var senders = outputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpSender(sender: udpClient, udpProtocol: udpProtocol))
                .ToList();

            var inputPorts = _hostSettings.InputPorts
                .Select(port => new IPEndPoint(IPAddress.Parse(_hostSettings.Host), port))
                .ToList();

            var receivers = inputPorts
                .Select(udpClientFactory.Create)
                .Select(udpClient => new UdpReceiver(receiver: udpClient, udpProtocol: udpProtocol))
                .ToList();

            var peerManager = new PeerManager();
            var subscriptionManager = new SubscriptionManager();

            var hostClient = new ServerHostClient(
                subscriptionManager: subscriptionManager,
                peerManager: peerManager,
                peerIps: inputPorts,
                outputQueue: outputQueue,
                serverSelector: randomServerSelector,
                serializer: _hostSettings.Serializer);

            var roomManager = new RoomManager(
                peerManager: peerManager);

            var dataGramBuilder = new DataGramBuilder(
                peerManager: peerManager,
                roomManager: roomManager);

            var protocolSubscriptionManager = new ProtocolSubscriptionManager(
                peerManager: peerManager,
                serializer: _hostSettings.Serializer);

            return new Host(
                protocolSubscriptionManager: protocolSubscriptionManager,
                roomManager: roomManager,
                serverHostClient: hostClient,
                dataGramBuilder: dataGramBuilder,
                workers: _hostSettings.Workers,
                peerManager: peerManager,
                subscriptionManager: subscriptionManager,
                serializer: _hostSettings.Serializer,
                outputQueue: outputQueue,
                inputQueue: inputQueue,
                senders: senders,
                receivers: receivers);
        }
    }
}