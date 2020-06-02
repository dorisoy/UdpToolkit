namespace UdpToolkit.Framework.Server.Rpcs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Server.Hubs;

    public static class MethodDescriptorStorage
    {
        private static readonly Lazy<IReadOnlyCollection<MethodDescriptor>> Methods = new Lazy<IReadOnlyCollection<MethodDescriptor>>(FindAllHubMethods);

        public static IReadOnlyCollection<MethodDescriptor> HubMethods => Methods.Value;

        private static IReadOnlyCollection<MethodDescriptor> FindAllHubMethods()
        {
            var hubs = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Select(type =>
                    new
                    {
                        HubType = type,
                        HubAttribute = type.GetCustomAttribute<HubAttribute>(),
                        Rpcs = type
                            .GetMethods()
                            .Select(method => new
                            {
                                method,
                                rpcAttribute = method.GetCustomAttribute<RpcAttribute>(),
                            })
                            .Where(item => item.rpcAttribute != null),
                    })
                .Where(item => item.HubAttribute != null)
                .ToDictionary(item => item.HubType, item => item);

            if (!hubs.Any())
            {
                return new List<MethodDescriptor>();
            }

            return (from pair in hubs
                let hubType = pair.Key
                let rpcs = pair.Value.Rpcs
                from rpc in rpcs
                select new MethodDescriptor(
                    hubType: hubType,
                    arguments: rpc.method
                        .GetParameters()
                        .Select(parameter => parameter.ParameterType),
                    rpcDescriptorId: new RpcDescriptorId(
                        hubId: pair.Value.HubAttribute.HubId,
                        rpcId: rpc.rpcAttribute.RpcId),
                    returnType: rpc.method.ReturnType,
                    methodInfo: rpc.method))
                .ToList();
        }
    }
}
