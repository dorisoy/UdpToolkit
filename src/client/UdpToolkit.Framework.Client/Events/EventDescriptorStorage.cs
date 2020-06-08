namespace UdpToolkit.Framework.Client.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UdpToolkit.Annotations;
    using UdpToolkit.Core;

    internal static class EventDescriptorStorage
    {
        private static readonly IReadOnlyDictionary<Type, EventDescriptor> Descriptors;

        static EventDescriptorStorage()
        {
            Descriptors = FindEventsWithAttribute<ProducedEventAttribute>()
                .ToDictionary(x => x.EventType, x => x);
        }

        public static EventDescriptor Find(Type type)
        {
            if (Descriptors.TryGetValue(key: type, value: out var eventDescriptor))
            {
                return eventDescriptor;
            }

            throw new EventDescriptorNotFoundException($"Descriptor for type {type} not found!");
        }

        private static IEnumerable<EventDescriptor> FindEventsWithAttribute<TAttribute>()
            where TAttribute : EventBaseAttribute
        {
            return FindTypesWithAttribute<TAttribute>()
                .Select(type =>
                {
                    var attribute = type.GetCustomAttribute<TAttribute>();

                    return new EventDescriptor(
                        rpcDescriptorId: new RpcDescriptorId(hubId: attribute.HubId, rpcId: attribute.RpcId),
                        udpMode: attribute.UdpChannel.Map(),
                        eventType: type);
                });
        }

        private static IEnumerable<Type> FindTypesWithAttribute<TAttribute>()
            where TAttribute : EventBaseAttribute
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttributes<TAttribute>().Any());
        }
    }
}