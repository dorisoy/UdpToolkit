namespace UdpToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Net;
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
            Action<byte[], Guid, IPEndPoint> onInputEvent,
            Action<byte[], Guid> onOutputEvent,
            Action<Guid> onAck,
            Action<Guid> onAckTimeout,
            BroadcastMode broadcastMode)
        {
            _protocolSubscriptions[hookId] = new ProtocolSubscription(
                broadcastMode: broadcastMode,
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
