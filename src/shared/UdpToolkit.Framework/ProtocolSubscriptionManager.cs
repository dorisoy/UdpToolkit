namespace UdpToolkit.Framework
{
    using System.Collections.Generic;
    using UdpToolkit.Core;

    public sealed class ProtocolSubscriptionManager : IProtocolSubscriptionManager
    {
        private readonly Dictionary<byte, ProtocolSubscription> _inputSubscriptions;
        private readonly Dictionary<byte, ProtocolSubscription> _outputSubscriptions;

        public ProtocolSubscriptionManager()
        {
            _inputSubscriptions = new Dictionary<byte, ProtocolSubscription>();
            _outputSubscriptions = new Dictionary<byte, ProtocolSubscription>();
        }

        public void SubscribeOnInputEvent<TEvent>(byte hookId, ProtocolSubscription protocolSubscription)
        {
            _inputSubscriptions[hookId] = protocolSubscription;
        }

        public void SubscribeOnOutputEvent<TEvent>(byte hookId, ProtocolSubscription protocolSubscription)
        {
            _outputSubscriptions[hookId] = protocolSubscription;
        }

        public ProtocolSubscription GetInputSubscription(byte hookId)
        {
            return _inputSubscriptions[hookId];
        }

        public ProtocolSubscription GetOutputSubscription(byte hookId)
        {
            return _outputSubscriptions[hookId];
        }
    }
}
