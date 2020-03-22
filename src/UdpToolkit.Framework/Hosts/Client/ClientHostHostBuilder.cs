namespace UdpToolkit.Framework.Hosts.Client
{
    using System;
    using System.Linq;
    using System.Net;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Events;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Peers;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;
    using UdpToolkit.Network.Rudp;

    public sealed class ClientHostHostBuilder : IClientHostBuilder
    {
        private readonly ILogger _logger = Log.ForContext<ClientHostHostBuilder>();

        private readonly ClientSettings _settings;

        public ClientHostHostBuilder(
            ClientSettings settings)
        {
            _settings = settings;
        }

        public IClientHostBuilder Configure(Action<ClientSettings> configurator)
        {
            configurator(_settings);

            return this;
        }

        public IClientHost Build()
        {
            var serializer = _settings.Serializer;

            var udpProtocol = new UdpProtocol(
                frameworkProtocol: new DefaultFrameworkProtocol(),
                reliableUdpProtocol: new ReliableUdpProtocol());

            var udpClientFactory = new UdpClientFactory();

            var localIp = new IPEndPoint(
                address: IPAddress.Any,
                port: 0);

            var client = udpClientFactory.Create(endPoint: localIp);

            var senders = _settings.ServerInputPorts
                .Select(ip => new UdpSender(
                    sender: client,
                    udpProtocol: udpProtocol))
                .ToList();

            var receivers = _settings.ServerOutputPorts
                .Select(port => new UdpReceiver(
                    receiver: client,
                    udpProtocol: udpProtocol))
                .ToList();

            var serverPeers = _settings.ServerInputPorts
                .Select(
                    port => new IPEndPoint(
                        IPAddress.Parse("0.0.0.0"), port))
                .Select(endPoint => new Peer(
                    id: endPoint.ToString(),
                    remotePeer: endPoint,
                    reliableUdpChannel: new ReliableUdpChannel()))
                .ToArray();

            var serverSelector = new RandomServerSelector(
                servers: serverPeers);

            var producedEvents = new BlockingAsyncQueue<ProducedEvent>(
                boundedCapacity: int.MaxValue);

            var inputDispatcher = new InputDispatcher();

            _logger.Information(
                "ClientHost created with settings: {@settings}, serverSelector: {@serverSelector}, servers: {@servers}, receivers count: {@receivers}, senders count: {@senders}",
                _settings,
                nameof(RandomServerSelector),
                serverPeers,
                receivers.Count,
                senders.Count);

            return new ClientHost(
                serverSelector: serverSelector,
                serializer: serializer,
                producedEvents: producedEvents,
                senders: senders,
                receivers: receivers,
                inputDispatcher: inputDispatcher);
        }
    }
}
