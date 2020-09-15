namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
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
            var me = Guid.NewGuid();
            var peerManager = new PeerManager();
            var udpClientFactory = new UdpClientFactory();

            var serverIps = _serverHostClientSettings.ServerPorts
                .Select(port =>
                    new IPEndPoint(
                        address: IPAddress.Parse(ipString: _serverHostClientSettings.ServerHost),
                        port: port))
                .ToArray();

            var randomServerSelector = new RandomServerSelector(serverIps);

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

            var subscriptionManager = new SubscriptionManager();

            peerManager.Create(me, inputPorts);

            var serverHostClient = new ServerHostClient(
                me: me,
                ips: _hostSettings.InputPorts.ToList(),
                outputQueue: outputQueue,
                serverSelector: randomServerSelector,
                serializer: _hostSettings.Serializer);

            var roomManager = new RoomManager(
                peerManager: peerManager);

            var dataGramBuilder = new DatagramBuilder(
                me: me,
                serverSelector: randomServerSelector,
                peerManager: peerManager,
                roomManager: roomManager);

            var protocolSubscriptionManager = new ProtocolSubscriptionManager(
                datagramBuilder: dataGramBuilder,
                peerManager: peerManager,
                serializer: _hostSettings.Serializer);

            return new Host(
                protocolSubscriptionManager: protocolSubscriptionManager,
                roomManager: roomManager,
                serverHostClient: serverHostClient,
                datagramBuilder: dataGramBuilder,
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