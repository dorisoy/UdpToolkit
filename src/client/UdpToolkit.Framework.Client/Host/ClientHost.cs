namespace UdpToolkit.Framework.Client.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Framework.Client.Events;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Queues;

    public sealed class ClientHost : IClientHost
    {
        private readonly ILogger _logger = Log.ForContext<ClientHost>();

        private readonly IServerSelector _serverSelector;
        private readonly IAsyncQueue<ProducedEvent> _outputQueue;

        private readonly IEnumerable<IUdpSender> _senders;
        private readonly IEnumerable<IUdpReceiver> _receivers;

        private readonly ISubscriptionManager _subscriptionManager;

        public ClientHost(
            IServerSelector serverSelector,
            ISerializer serializer,
            IAsyncQueue<ProducedEvent> outputQueue,
            IEnumerable<IUdpSender> senders,
            IEnumerable<IUdpReceiver> receivers,
            ISubscriptionManager subscriptionManager)
        {
            _serverSelector = serverSelector;
            Serializer = serializer;
            _outputQueue = outputQueue;
            _senders = senders;
            _receivers = receivers;
            _subscriptionManager = subscriptionManager;

            foreach (var receiver in _receivers)
            {
                receiver.UdpPacketReceived += packet =>
                {
                    _logger.Debug("Packet received: {@packet}", packet);

                    ProcessNetworkPacket(packet);
                };
            }
        }

        public ISerializer Serializer { get; }

        public Task RunAsync()
        {
            var receivers = _receivers
                .Select(
                    receiver => Task.Run(
                        () => StartReceiverAsync(receiver)
                            .RestartJobOnFailAsync(
                                job: () => StartReceiverAsync(receiver),
                                logger: (exception) =>
                                {
                                    _logger.Error("Exception on receive task: {@Exception}", exception);
                                    _logger.Warning("Restart receiver...");
                                })))
                .ToList();

            var senders = _senders
                .Select(
                    sender => Task.Run(
                        () => StartSenderAsync(sender)
                            .RestartJobOnFailAsync(
                                job: () => StartSenderAsync(sender),
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

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        public void On<TEvent>(Action<TEvent> handler)
        {
            var eventDescriptor = EventDescriptorStorage.Find(typeof(TEvent));

            _subscriptionManager.Subscribe(
                rpcDescriptorId: eventDescriptor.RpcDescriptorId,
                subscription: (bytes) =>
                {
                    var @event = Serializer.Deserialize<TEvent>(new ArraySegment<byte>(bytes));
                    handler(@event);
                });
        }

        public void Publish<TEvent>(TEvent @event)
        {
            _outputQueue.Produce(new ProducedEvent(
                eventDescriptor: EventDescriptorStorage.Find(typeof(TEvent)),
                serialize: (serializer) => serializer.Serialize(@event)));
        }

        public void Dispose()
        {
            foreach (var sender in _senders)
            {
                sender.Dispose();
            }

            foreach (var receiver in _receivers)
            {
                receiver.Dispose();
            }

            _outputQueue.Dispose();
        }

        private async Task StartReceiverAsync(IUdpReceiver udpReceiver)
        {
            await udpReceiver
                .StartReceiveAsync()
                .ConfigureAwait(false);
        }

        private async Task ProcessProducedEventAsync(IUdpSender udpSender, ProducedEvent producedEvent)
        {
            var bytes = producedEvent.Serialize(Serializer);

            var networkPacket = new NetworkPacket(
                payload: bytes,
                ipEndPoint: _serverSelector.GetServer().IpEndPoint,
                udpMode: producedEvent.EventDescriptor.UdpMode,
                frameworkHeader: new FrameworkHeader(
                    hubId: producedEvent.EventDescriptor.RpcDescriptorId.HubId,
                    rpcId: producedEvent.EventDescriptor.RpcDescriptorId.RpcId));

            await udpSender.SendAsync(networkPacket).ConfigureAwait(false);
        }

        private async Task StartSenderAsync(IUdpSender udpSender)
        {
            foreach (var producedEvent in _outputQueue.Consume())
            {
                try
                {
                    await ProcessProducedEventAsync(udpSender, producedEvent)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Unhandled exception on processing produced event, {@Exception}", ex);
                }
            }
        }

        private void ProcessNetworkPacket(NetworkPacket networkPacket)
        {
            var rpcDescriptorId = new RpcDescriptorId(
                hubId: networkPacket.FrameworkHeader.HubId,
                rpcId: networkPacket.FrameworkHeader.RpcId);

            _subscriptionManager.GetSubscription(rpcDescriptorId)(networkPacket.Payload);
        }
    }
}