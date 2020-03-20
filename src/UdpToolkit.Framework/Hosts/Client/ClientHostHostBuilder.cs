using System;
using System.Linq;
using System.Net;
using UdpToolkit.Core;
using UdpToolkit.Framework.Events;
using UdpToolkit.Network.Clients;
using UdpToolkit.Network.Peers;
using UdpToolkit.Network.Protocol;
using UdpToolkit.Network.Queues;
using UdpToolkit.Network.Rudp;

namespace UdpToolkit.Framework.Hosts.Client
{
    public sealed class ClientHostHostBuilder : IClientHostBuilder
    {
        private readonly ClientSettings _clientSettings;

        public ClientHostHostBuilder(
            ClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }

        public IClientHostBuilder Configure(Action<ClientSettings> configurator)
        {
            configurator(_clientSettings);

            return this;
        }

        public IClientHost Build()
        {
            var serializer = _clientSettings.Serializer;
            
            var udpProtocol = new UdpProtocol(
                frameworkProtocol: new DefaultFrameworkProtocol(), 
                reliableUdpProtocol: new ReliableUdpProtocol()); 
            
            var udpClientFactory = new UdpClientFactory();

            var localIp =new IPEndPoint(
                address: IPAddress.Any,
                port: 0);

            var client = udpClientFactory.Create(endPoint: localIp);
            
            var senders = _clientSettings.ServerInputPorts
                .Select(ip => new UdpSender( 
                    sender: client, 
                    udpProtocol: udpProtocol))
                .ToList();

            var receivers = _clientSettings.ServerOutputPorts
                .Select(port => new UdpReceiver(
                    receiver:  client,
                    udpProtocol: udpProtocol))
                .ToList();

            var serverPeers = _clientSettings.ServerInputPorts
                .Select(
                    port => new IPEndPoint(
                        IPAddress.Parse("0.0.0.0"), port))
                .Select(endPoint => new Peer(
                    id: endPoint.ToString(),
                    remotePeer: endPoint,
                    reliableChannel: new ReliableChannel()));
            
            var serverSelector = new RandomServerSelector(
                servers: serverPeers);

            var producedEvents = new BlockingAsyncQueue<ProducedEvent>(
                boundedCapacity: int.MaxValue);

            var inputDispatcher = new InputDispatcher();
            
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
