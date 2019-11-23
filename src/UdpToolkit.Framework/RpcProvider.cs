using System.Collections.Generic;
using UdpToolkit.Core;

namespace UdpToolkit.Framework
{
    public class RpcProvider : IRpcProvider
    {
        private readonly IReadOnlyDictionary<RpcDescriptorId, RpcDescriptor> _hubs;

        public RpcProvider(IReadOnlyDictionary<RpcDescriptorId, RpcDescriptor> hubs)
        {
            _hubs = hubs;
        }

        public bool TryProvide(RpcDescriptorId rpcDescriptorId, out RpcDescriptor rpcDescriptor)
        {
            return _hubs.TryGetValue(rpcDescriptorId, out rpcDescriptor);
        }
    }
}
