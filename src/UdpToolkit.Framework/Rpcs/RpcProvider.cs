namespace UdpToolkit.Framework.Rpcs
{
    using System.Collections.Generic;

    public sealed class RpcProvider : IRpcProvider
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
