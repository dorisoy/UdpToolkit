namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;
    using UdpToolkit.Core;

    public sealed class SubscriptionManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<byte, Subscription> _subscriptions = new ConcurrentDictionary<byte, Subscription>();

        public void Subscribe<TEvent>(byte hookId, Subscription subscription)
        {
            _subscriptions[hookId] = subscription;
        }

        public Subscription GetSubscription(byte hookId)
        {
            return _subscriptions[hookId];
        }
    }
}