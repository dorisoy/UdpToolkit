namespace UdpToolkit.Core
{
    using System;
    using UdpToolkit.Serialization;

    public class Subscription
    {
        public Subscription(
            Action<byte[], Guid, ISerializer, IRoomManager> onEvent,
            Action<Guid> onAck,
            Action<Guid> onTimeout)
        {
            OnEvent = onEvent;
            OnAck = onAck;
            OnTimeout = onTimeout;
        }

        public Action<byte[], Guid, ISerializer, IRoomManager> OnEvent { get; }

        public Action<Guid> OnAck { get; }

        public Action<Guid> OnTimeout { get; }
    }
}