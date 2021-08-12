namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;
    using UdpToolkit.Serialization;

    public class Subscription
    {
        public Subscription(
            Action<byte[], Guid, ISerializer> onProtocolEvent,
            Func<byte[], Guid, IpV4Address, ISerializer, IRoomManager, IScheduler, int> onEvent,
            Action<Guid> onAck,
            Action<Guid> onTimeout,
            BroadcastMode broadcastMode)
        {
            OnProtocolEvent = onProtocolEvent;
            OnEvent = onEvent;
            OnAck = onAck;
            OnTimeout = onTimeout;
            BroadcastMode = broadcastMode;
        }

        public Action<byte[], Guid, ISerializer> OnProtocolEvent { get; }

        public Func<byte[], Guid, IpV4Address, ISerializer, IRoomManager, IScheduler, int> OnEvent { get; }

        public Action<Guid> OnAck { get; }

        public Action<Guid> OnTimeout { get; } // TODO implement this

        public BroadcastMode BroadcastMode { get; }
    }
}