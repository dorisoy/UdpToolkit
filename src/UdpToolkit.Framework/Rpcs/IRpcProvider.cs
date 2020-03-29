namespace UdpToolkit.Framework.Rpcs
{
    using UdpToolkit.Core;

    public interface IRpcProvider
    {
        bool TryProvide(RpcDescriptorId rpcDescriptorId, out RpcDescriptor rpcDescriptor);
    }
}
