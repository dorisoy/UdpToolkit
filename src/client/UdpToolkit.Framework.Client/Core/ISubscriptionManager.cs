namespace UdpToolkit.Framework.Client.Core
{
    using UdpToolkit.Core;

    public interface ISubscriptionManager
    {
        void Subscribe(RpcDescriptorId rpcDescriptorId, Subscription subscription);

        Subscription GetSubscription(RpcDescriptorId rpcDescriptorId);
    }
}