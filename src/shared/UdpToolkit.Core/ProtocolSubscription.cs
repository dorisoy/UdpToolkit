namespace UdpToolkit.Core
{
    using System;
    using System.Net;

    public class ProtocolSubscription
    {
        public ProtocolSubscription(
            Action<byte[], Guid> onOutputEvent,
            Action<byte[], Guid, IPEndPoint> onInputEvent,
            Action<Guid> onAck,
            Action<Guid> onTimeout,
            BroadcastMode broadcastMode)
        {
            OnOutputEvent = onOutputEvent;
            OnInputEvent = onInputEvent;
            OnAck = onAck;
            OnTimeout = onTimeout;
            BroadcastMode = broadcastMode;
        }

        public Action<byte[], Guid> OnOutputEvent { get; }

        public Action<byte[], Guid, IPEndPoint> OnInputEvent { get; }

        public Action<Guid> OnAck { get; }

        public Action<Guid> OnTimeout { get; }

        public BroadcastMode BroadcastMode { get; }
    }
}