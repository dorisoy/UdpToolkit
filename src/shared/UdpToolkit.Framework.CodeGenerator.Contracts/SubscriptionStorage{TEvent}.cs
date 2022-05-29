// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework
{
    using UdpToolkit.Framework.Contracts;

    /// <summary>
    /// Subscription storage for user-defined subscriptions.
    /// </summary>
    /// <typeparam name="TEvent">
    /// Type of user-defined event.
    /// </typeparam>
    public static class SubscriptionStorage<TEvent>
    {
        private static Subscription<TEvent> _subscription;

        /// <summary>
        /// Subscribes to the user-defined event.
        /// </summary>
        /// <param name="subscription">Instance of subscription.</param>
        public static void Subscribe(
            Subscription<TEvent> subscription)
        {
            if (_subscription == null)
            {
                _subscription = subscription;
            }
        }

        /// <summary>
        /// Gets subscription for event.
        /// </summary>
        /// <returns>Subscription.</returns>
        public static Subscription<TEvent> GetSubscription()
        {
            return _subscription;
        }
    }
}