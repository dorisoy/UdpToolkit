namespace UdpToolkit.Contexts
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;

    public readonly struct HostOutContext
    {
        public HostOutContext(
            int roomId,
            BroadcastMode broadcastMode,
            DateTimeOffset createdAt,
            OutPacket outPacket)
        {
            RoomId = roomId;
            BroadcastMode = broadcastMode;
            CreatedAt = createdAt;
            OutPacket = outPacket;
        }

        public int RoomId { get; }

        public BroadcastMode BroadcastMode { get; }

        public DateTimeOffset CreatedAt { get; }

        public OutPacket OutPacket { get; }
    }
}