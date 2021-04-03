namespace UdpToolkit.Contexts
{
    using System;
    using UdpToolkit.Network.Packets;

    public readonly struct InContext
    {
        public InContext(
            DateTimeOffset createdAt,
            InPacket inPacket)
        {
            CreatedAt = createdAt;
            InPacket = inPacket;
        }

        public DateTimeOffset CreatedAt { get; }

        public InPacket InPacket { get; }
    }
}