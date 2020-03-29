namespace UdpToolkit.Framework.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UdpToolkit.Annotations;
    using UdpToolkit.Core;

    public abstract class EventFactoryBase
    {
        protected static IEnumerable<EventDescriptor> FindEventsWithAttribute<TAttribute>()
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