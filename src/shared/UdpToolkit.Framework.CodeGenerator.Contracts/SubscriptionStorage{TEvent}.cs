// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework
{
    using UdpToolkit.Framework.Contracts;

    public static class SubscriptionStorage<TEvent>
    {
        private static Subscription<TEvent> _subscription;

        public static void Subscribe(
            Subscription<TEvent> subscription)
        {
            if (_subscription == null)
            {
                _subscription = subscription;
            }
        }

        public static Subscription<TEvent> GetSubscription() => _subscription;
    }
}