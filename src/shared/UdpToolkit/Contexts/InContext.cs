namespace UdpToolkit.Contexts
{
    using System;
    using UdpToolkit.Network.Packets;

    public readonly struct InContext
    {
        public InContext(
            TimeSpan resendTimeout,
            DateTimeOffset createdAt,
            InPacket inPacket)
        {
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            InPacket = inPacket;
        }

        public TimeSpan ResendTimeout { get; }

        public DateTimeOffset CreatedAt { get; }

        public InPacket InPacket { get; }
    }
}