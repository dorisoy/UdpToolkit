namespace UdpToolkit.Core
{
    using System;

    public class ProtocolSubscription
    {
        public ProtocolSubscription(
            Action<byte[], Guid> onOutputEvent,
            Action<byte[], Guid> onInputEvent,
            Action<Guid> onAck,
            Action<Guid> onTimeout)
        {
            OnOutputEvent = onOutputEvent;
            OnInputEvent = onInputEvent;
            OnAck = onAck;
            OnTimeout = onTimeout;
        }

        public Action<byte[], Guid> OnOutputEvent { get; }

        public Action<byte[], Guid> OnInputEvent { get; }

        public Action<Guid> OnAck { get; }

        public Action<Guid> OnTimeout { get; }
    }
}