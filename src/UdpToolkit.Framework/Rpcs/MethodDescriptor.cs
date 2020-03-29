namespace UdpToolkit.Framework.Rpcs
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UdpToolkit.Core;

    public readonly struct MethodDescriptor
    {
        public MethodDescriptor(
            IEnumerable<Type> arguments,
            Type returnType,
            Type hubType,
            MethodInfo methodInfo,
            RpcDescriptorId rpcDescriptorId)
        {
            Arguments = arguments;
            ReturnType = returnType;
            HubType = hubType;
            MethodInfo = methodInfo;
            RpcDescriptorId = rpcDescriptorId;
        }

        public RpcDescriptorId RpcDescriptorId { get; }

        public IEnumerable<Type> Arguments { get; }

        public Type ReturnType { get; }

        public Type HubType { get; }

        public MethodInfo MethodInfo { get; }
    }
}