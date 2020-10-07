namespace UdpToolkit.Core
{
    public interface ISubscriptionManager
    {
        void Subscribe(byte hookId, Subscription subscription);

        Subscription GetSubscription(byte hookId);
    }
}