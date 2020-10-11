namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Serialization;

    public class Subscription
    {
        public Subscription(
            Action<byte[], Guid, ISerializer, IRoomManager> onEvent,
            Action<Guid> onAck,
            Action<Guid> onTimeout,
            BroadcastMode broadcastMode)
        {
            OnEvent = onEvent;
            OnAck = onAck;
            OnTimeout = onTimeout;
            BroadcastMode = broadcastMode;
        }

        public Action<byte[], Guid, ISerializer, IRoomManager> OnEvent { get; }

        public Action<Guid> OnAck { get; }

        public Action<Guid> OnTimeout { get; }

        public BroadcastMode BroadcastMode { get; }
    }
}