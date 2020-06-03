namespace UdpToolkit.Framework.Client.Events.EventProducers
{
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Network.Queues;

    public sealed class EventProducer<TEvent> : IEventProducer<TEvent>
    {
        private readonly byte _roomId;
        private readonly EventDescriptor _eventDescriptor;
        private readonly IAsyncQueue<ProducedEvent> _outputQueue;

        public EventProducer(
            byte roomId,
            EventDescriptor eventDescriptor,
            IAsyncQueue<ProducedEvent> outputQueue)
        {
            _roomId = roomId;
            _eventDescriptor = eventDescriptor;
            _outputQueue = outputQueue;
        }

        public void Produce(TEvent @event)
        {
            _outputQueue.Produce(new ProducedEvent(
                roomId: _roomId,
                eventDescriptor: _eventDescriptor,
                serialize: serializer => serializer.Serialize(@event)));
        }
    }
}
