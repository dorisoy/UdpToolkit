namespace UdpToolkit.Framework.Client.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Framework.Client.Events;
    using UdpToolkit.Framework.Client.Host;
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
            var localIp = new IPEndPoint(
                address: IPAddress.Any,
                port: 0);

            var udpClientFactory = new UdpClientFactory();

            var udpClient = udpClientFactory
                .Create(endPoint: localIp);

            var dateTimeProvider = new DateTimeProvider();

            var now = dateTimeProvider.UtcNow();

            var randomServerSelector = new RandomServerSelector(
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

            var inputDispatcher = new InputDispatcher();

            var defaultFrameworkProtocol = new DefaultFrameworkProtocol();

            var reliableUdpProtocol = new ReliableUdpProtocol();

            var udpProtocol = new UdpProtocol(
                frameworkProtocol: defaultFrameworkProtocol,
                reliableUdpProtocol: reliableUdpProtocol,
                dateTimeProvider: dateTimeProvider);

            var producedEvents = new BlockingAsyncQueue<ProducedEvent>(
                boundedCapacity: int.MaxValue);

            var receivers = _settings.ServerOutputPorts
                .Select(port => new UdpReceiver(
                    receiver: udpClient,
                    udpProtocol: udpProtocol))
                .ToList();

            var senders = _settings.ServerInputPorts
                .Select(ip => new UdpSender(
                    sender: udpClient,
                    udpProtocol: udpProtocol))
                .ToList();

            var clientHost = new ClientHost(
                serverSelector: randomServerSelector,
                serializer: _settings.Serializer,
                producedEvents: producedEvents,
                senders: senders,
                receivers: receivers,
                inputDispatcher: inputDispatcher);

            _logger.Information("ClientHost created with settings: {@settings}", _settings);

            return clientHost;
        }
    }
}
