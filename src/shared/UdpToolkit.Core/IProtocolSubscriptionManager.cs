namespace UdpToolkit.Core
{
    using System;

    public interface IProtocolSubscriptionManager
    {
        void SubscribeOnProtocolEvent<TEvent>(
            byte hookId,
            Action<byte[], Guid> onInputEvent,
            Action<byte[], Guid> onOutputEvent,
            Action<Guid> onAck,
            Action<Guid> onAckTimeout,
            BroadcastMode broadcastMode);

        ProtocolSubscription GetProtocolSubscription(byte hookId);
    }
}