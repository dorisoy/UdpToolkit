namespace UdpToolkit.Core
{
    public interface ISubscriptionManager
    {
        void Subscribe<TEvent>(byte hookId, Subscription subscription);

        Subscription GetSubscription(byte hookId);
    }
}