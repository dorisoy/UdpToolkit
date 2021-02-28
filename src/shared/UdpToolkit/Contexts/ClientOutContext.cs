namespace UdpToolkit.Contexts
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;

    public readonly struct ClientOutContext
    {
        public ClientOutContext(
            BroadcastMode broadcastMode,
            TimeSpan resendTimeout,
            DateTimeOffset createdAt,
            OutPacket outPacket)
        {
            BroadcastMode = broadcastMode;
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            OutPacket = outPacket;
        }

        public BroadcastMode BroadcastMode { get; }

        public TimeSpan ResendTimeout { get; }

        public DateTimeOffset CreatedAt { get; }

        public OutPacket OutPacket { get; }
    }
}