namespace UdpToolkit.Core
{
    using System;

    public interface IProtocolSubscriptionManager
    {
        void SubscribeOnInputEvent<TEvent>(byte hookId, ProtocolSubscription protocolSubscription);

        void SubscribeOnOutputEvent<TEvent>(byte hookId, ProtocolSubscription protocolSubscription);

        ProtocolSubscription GetInputSubscription(byte hookId);

        ProtocolSubscription GetOutputSubscription(byte hookId);
    }
}