namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Generic;
    using UdpToolkit.Core;

    public sealed class ProtocolSubscriptionManager : IProtocolSubscriptionManager
    {
        private readonly Dictionary<byte, ProtocolSubscription> _protocolSubscriptions;

        public ProtocolSubscriptionManager()
        {
            _protocolSubscriptions = new Dictionary<byte, ProtocolSubscription>();
        }

        public void SubscribeOnProtocolEvent<TEvent>(
            byte hookId,
            Action<byte[], Guid> onInputEvent,
            Action<byte[], Guid> onOutputEvent,
            Action<Guid> onAck,
            Action<Guid> onAckTimeout)
        {
            _protocolSubscriptions[hookId] = new ProtocolSubscription(
                onOutputEvent: onOutputEvent,
                onInputEvent: onInputEvent,
                onAck: onAck,
                onTimeout: onAckTimeout);
        }

        public ProtocolSubscription GetProtocolSubscription(byte hookId)
        {
            return _protocolSubscriptions[hookId];
        }
    }
}
