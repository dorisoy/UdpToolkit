namespace UdpToolkit.Framework.Events.EventProducers
{
    using UdpToolkit.Core;
    using UdpToolkit.Network.Queues;

    public sealed class EventProducer<TEvent> : IEventProducer<TEvent>
    {
        private readonly byte _scopeId;
        private readonly EventDescriptor _eventDescriptor;
        private readonly IAsyncQueue<ProducedEvent> _outputQueue;

        public EventProducer(
            byte scopeId,
            EventDescriptor eventDescriptor,
            IAsyncQueue<ProducedEvent> outputQueue)
        {
            _scopeId = scopeId;
            _eventDescriptor = eventDescriptor;
            _outputQueue = outputQueue;
        }

        public void Produce(TEvent @event)
        {
            _outputQueue.Produce(new ProducedEvent(
                scopeId: _scopeId,
                eventDescriptor: _eventDescriptor,
                serialize: serializer => serializer.Serialize(@event)));
        }
    }
}
