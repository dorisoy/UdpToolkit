namespace UdpToolkit.Framework.Rpcs
{
    public interface IRpcProvider
    {
        bool TryProvide(RpcDescriptorId rpcDescriptorId, out RpcDescriptor rpcDescriptor);
    }
}
