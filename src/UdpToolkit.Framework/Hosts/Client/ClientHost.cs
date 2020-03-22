using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
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
        private readonly ILogger _logger = Log.ForContext<ClientHost>(); 
        
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
                    _logger.Debug("Packet received: {@packet}", packet);
                                
                    ProcessInputUdpPacket(packet);
                };
            }
        }
        
        public Task RunAsync()
        {
            var receivers = _receivers
                .Select(
                    receiver => Task.Run(
                        () => StartReceiver(receiver)
                            .RestartJobOnFail(
                                job: () => StartReceiver(receiver),
                                logger: (exception) =>
                                {
                                    _logger.Error("Exception on receive task: {@Exception}", exception);
                                    _logger.Warning("Restart receiver...");
                                })))
                .ToList();

            var senders = _senders
                .Select(
                    sender => Task.Run(
                        () => StartSender(sender)
                            .RestartJobOnFail(
                                job: () => StartSender(sender),
                                logger: (exception) =>
                                {
                                    _logger.Error("Exception on send task: {@Exception}", exception);
                                    _logger.Warning("Restart sender...");
                                })))
                .ToList();

            var tasks = receivers.Concat(senders);

            _logger.Information($"{nameof(ClientHost)} running...");
            
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

        private async Task StartReceiver(IUdpReceiver udpReceiver)
        {
            await udpReceiver.StartReceiveAsync();
        }

        private async Task ProcessProducedEvent(IUdpSender udpSender, ProducedEvent producedEvent)
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
                    
            await udpSender.SendAsync(outputUdpPacket);
        }

        private async Task StartSender(IUdpSender udpSender)
        {
            foreach (var producedEvent in _producedEvents.Consume())
            {
                try
                {
                    await ProcessProducedEvent(udpSender, producedEvent);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Unhandled exception on processing produced event, {@Exception}", ex);
                }
            }
        }
        
        private void ProcessInputUdpPacket(InputUdpPacket packet)
        {
            _inputDispatcher.Dispatch(packet);            
        }
    }
}