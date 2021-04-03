namespace UdpToolkit.Contexts
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;

    public readonly struct ClientOutContext
    {
        public ClientOutContext(
            BroadcastMode broadcastMode,
            DateTimeOffset createdAt,
            OutPacket outPacket)
        {
            BroadcastMode = broadcastMode;
            CreatedAt = createdAt;
            OutPacket = outPacket;
        }

        public BroadcastMode BroadcastMode { get; }

        public DateTimeOffset CreatedAt { get; }

        public OutPacket OutPacket { get; }
    }
}