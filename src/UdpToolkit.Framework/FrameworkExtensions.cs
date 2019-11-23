using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdpToolkit.Core;

namespace UdpToolkit.Framework
{
    public static class FrameworkExtensions
    {
        public static IReadOnlyCollection<MethodDescriptor> FindAllHubMethods()
        {
            var hubs = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttribute<HubAttribute>() != null)
                .ToDictionary(hub => hub, hub => hub.GetCustomAttribute<HubAttribute>());

            if (!hubs.Any())
            {
                return new List<MethodDescriptor>();
            }

            return hubs
                .SelectMany(pair => pair.Key
                    .GetMethods()
                    .Where(method => method.GetCustomAttribute<RpcAttribute>() != null))
                .Select(method => new MethodDescriptor(
                    hubType: method.DeclaringType,
                    hubId: method.DeclaringType.GetCustomAttribute<HubAttribute>().HubId,
                    arguments: method
                        .GetParameters()
                        .Select(parameter => parameter.ParameterType),
                    methodId: method.GetCustomAttribute<RpcAttribute>().RpcId,
                    returnType: method.ReturnType,
                    methodInfo: method))
                .ToList();
        }
    }
}
