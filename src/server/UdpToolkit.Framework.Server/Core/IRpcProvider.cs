namespace UdpToolkit.Framework.Server.Core
{
    using UdpToolkit.Core;

    public interface IRpcProvider
    {
        bool TryProvide(RpcDescriptorId rpcDescriptorId, out RpcDescriptor rpcDescriptor);
    }
}
