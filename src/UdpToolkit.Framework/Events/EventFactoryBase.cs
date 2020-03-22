namespace UdpToolkit.Framework.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UdpToolkit.Annotations;
    using UdpToolkit.Framework.Rpcs;

    public abstract class EventFactoryBase
    {
        protected static IEnumerable<EventDescriptor> FindEventsWithAttribute<TAttribute>()
            where TAttribute : Attribute, IEventAttribute
        {
            return FindTypesWithAttribute<TAttribute>()
                .Select(type =>
                {
                    var attribute = type.GetCustomAttribute<TAttribute>();

                    return new EventDescriptor(
                        rpcDescriptorId: new RpcDescriptorId(hubId: attribute.HubId, rpcId: attribute.RpcId),
                        udpMode: FrameworkExtensions.Map(attribute.UdpChannel),
                        eventType: type);
                });
        }

        private static IEnumerable<Type> FindTypesWithAttribute<TAttribute>()
            where TAttribute : Attribute, IEventAttribute
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttributes<TAttribute>().Any());
        }
    }
}