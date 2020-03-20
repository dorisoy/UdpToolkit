using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UdpToolkit.Core;
using UdpToolkit.Framework.Events;
using UdpToolkit.Framework.Events.EventConsumers;
using UdpToolkit.Framework.Events.EventProducers;
using UdpToolkit.Network.Clients;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Protocol;
using UdpToolkit.Network.Queues;

namespace UdpToolkit.Framework.Hosts.Client
{
    public sealed class ClientHost : IClientHost
    {
        private readonly IServerSelector _serverSelector;
        private readonly IAsyncQueue<ProducedEvent> _producedEvents;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly InputDispatcher _inputDispatcher;
        
        public ClientHost(
            IServerSelector serverSelector,
            ISerializer serializer,
            IAsyncQueue<ProducedEvent> producedEvents,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers, 
            InputDispatcher inputDispatcher)
        {
            _serverSelector = serverSelector;
            Serializer = serializer;
            _producedEvents = producedEvents;
            _senders = senders;
            _receivers = receivers;
            _inputDispatcher = inputDispatcher;

            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += packet =>
                {
                    Console.WriteLine($"Packet from server received! {packet.RemotePeer}");
                                
                    ProcessInputUdpPacket(packet);
                };
            }
        }
        
        public Task RunAsync()
        {
            var receiving = _receivers
                .Select(receiver => Task.Run(() => ReceivePackets(receiver)))
                .ToList();

            var sending = _senders
                .Select(sender => Task.Run(() => SendPackets(sender)))
                .ToList();

            var tasks = receiving.Concat(sending);

            return Task.WhenAll(tasks);
        }

        public IEventProducerFactory GetEventProducerFactory()
        {
            return new EventProducerFactory(producedEvents: _producedEvents);
        }

        public IEventConsumerFactory GetEventConsumerFactory()
        {
            return new EventConsumerFactory(
                serializer: Serializer,
                inputDispatcher: _inputDispatcher);
        }

        public ISerializer Serializer { get; }

        private async Task ReceivePackets(IUdpReceiver udpReceiver)
        {
            try
            {
                await udpReceiver.StartReceiveAsync();
            }
            catch (Exception e)
            {
                //TODO logging
                Console.WriteLine(e);
            }
        }

        private async Task SendPackets(IUdpSender udpSender)
        {
            try
            {
                foreach (var producedEvent in _producedEvents.Consume())
                {
                    var bytes = producedEvent.Serialize(Serializer);
                    
                    var outputUdpPacket = new OutputUdpPacket(
                        payload: bytes,
                        peers: new[] { _serverSelector.GetServer() },
                        mode: producedEvent.EventDescriptor.UdpMode,
                        frameworkHeader: new FrameworkHeader(
                            hubId: producedEvent.EventDescriptor.RpcDescriptorId.HubId,
                            rpcId: producedEvent.EventDescriptor.RpcDescriptorId.RpcId,
                            scopeId: producedEvent.ScopeId));
                    
                    await udpSender.Send(outputUdpPacket);
                }
            }
            catch (Exception e)
            {
                //TODO logging
                Console.WriteLine(e);
            }
        }
        
        private void ProcessInputUdpPacket(InputUdpPacket packet)
        {
            _inputDispatcher.Dispatch(packet);            
        }
    }
}