namespace UdpToolkit.Framework.Client.Events.EventProducers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Annotations;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;
    using UdpToolkit.Network.Queues;

    public sealed class EventProducerFactory : EventFactoryBase, IEventProducerFactory
    {
        private static readonly Lazy<IReadOnlyDictionary<Type, EventDescriptor>> Consumers =
            new Lazy<IReadOnlyDictionary<Type, EventDescriptor>>(
                FindEventsWithAttribute<ProducedEventAttribute>()
                    .ToDictionary(
                        descriptor => descriptor.EventType,
                        descriptor => descriptor));

        private readonly IAsyncQueue<ProducedEvent> _producedEvents;

        public EventProducerFactory(
            IAsyncQueue<ProducedEvent> producedEvents)
        {
            _producedEvents = producedEvents;
        }

        public IEventProducer<TEvent> Create<TEvent>(byte scopeId)
        {
            var eventDescriptor = GetEventDescriptor(type: typeof(TEvent));

            return new EventProducer<TEvent>(
                scopeId: scopeId,
                eventDescriptor: eventDescriptor,
                outputQueue: _producedEvents);
        }

        private static EventDescriptor GetEventDescriptor(Type type)
        {
            var success = Consumers.Value.TryGetValue(type, out var eventDescriptor);
            if (!success)
            {
                throw new EventDescriptorNotFoundException($"EventDescriptor for type {nameof(type)} not found!");
            }

            return eventDescriptor;
        }
    }
}
