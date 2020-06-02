namespace UdpToolkit.Framework.Client.Events.EventConsumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UdpToolkit.Annotations;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;

    public sealed class EventConsumerFactory : EventFactoryBase, IEventConsumerFactory
    {
        private static readonly Lazy<IReadOnlyCollection<EventDescriptor>> Consumers = new Lazy<IReadOnlyCollection<EventDescriptor>>(
            () => FindEventsWithAttribute<ConsumedEventAttribute>().ToList());

        private static readonly Lazy<IReadOnlyDictionary<Type, EventDescriptor>> ConsumersByType =
            new Lazy<IReadOnlyDictionary<Type, EventDescriptor>>(
                Consumers.Value
                    .ToDictionary(
                        descriptor => descriptor.EventType,
                        descriptor => descriptor));

        private readonly ISerializer _serializer;
        private readonly InputDispatcher _inputDispatcher;

        public EventConsumerFactory(
            ISerializer serializer,
            InputDispatcher inputDispatcher)
        {
            _serializer = serializer;
            _inputDispatcher = inputDispatcher;
        }

        public IEventConsumer<TEvent> Create<TEvent>()
        {
            var eventDescriptor = GetEventDescriptor(type: typeof(TEvent));

            var eventConsumer = new EventConsumer<TEvent>(
                serializer: _serializer,
                rpcDescriptorId: eventDescriptor.RpcDescriptorId);

            _inputDispatcher.AddEventConsumer(
                eventConsumer: eventConsumer);

            return eventConsumer;
        }

        private static EventDescriptor GetEventDescriptor(Type type)
        {
            var success = ConsumersByType.Value.TryGetValue(type, out var eventDescriptor);
            if (!success)
            {
                throw new EventDescriptorNotFoundException($"EventDescriptor for type {nameof(type)} not found!");
            }

            return eventDescriptor;
        }
    }
}
