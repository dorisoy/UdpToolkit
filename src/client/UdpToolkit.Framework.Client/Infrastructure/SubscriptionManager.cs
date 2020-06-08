namespace UdpToolkit.Framework.Client.Infrastructure
{
    using System.Collections.Concurrent;
    using UdpToolkit.Core;
    using UdpToolkit.Framework.Client.Core;

    public sealed class SubscriptionManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<RpcDescriptorId, Subscription> _subscriptions = new ConcurrentDictionary<RpcDescriptorId, Subscription>();

        public void Subscribe(RpcDescriptorId rpcDescriptorId, Subscription subscription)
        {
            _subscriptions.TryAdd(rpcDescriptorId, subscription);
        }

        public Subscription GetSubscription(RpcDescriptorId rpcDescriptorId)
        {
            return _subscriptions[rpcDescriptorId];
        }
    }
}
