using System;
using System.Collections.Generic;
using UdpToolkit.Core;

namespace UdpToolkit.Core
{
    public class RpcDescriptor
    {
        public RpcDescriptor(
            byte rpcId,
            byte hubId,
            HubRpc hubRpc,
            IReadOnlyCollection<Type> parametersTypes,
            IReadOnlyCollection<Type> ctorArguments)
        {
            RpcId = rpcId;
            HubId = hubId;
            HubRpc = hubRpc;
            ParametersTypes = parametersTypes;
            CtorArguments = ctorArguments;
        }

        public IReadOnlyCollection<Type> CtorArguments { get; }
        
        public IReadOnlyCollection<Type> ParametersTypes { get; }

        public HubRpc HubRpc { get; }

        public byte HubId { get; }

        public byte RpcId { get; }
    }
}