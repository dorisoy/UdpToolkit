namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;

    public sealed class CallContext
    {
        public CallContext(
            NetworkPacket networkPacket,
            TimeSpan resendTimeout,
            DateTimeOffset createdAt,
            int? roomId,
            BroadcastMode? broadcastMode)
        {
            NetworkPacket = networkPacket;
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            RoomId = roomId;
            BroadcastMode = broadcastMode;
        }

        public int? RoomId { get; }

        public BroadcastMode? BroadcastMode { get; }

        public NetworkPacket NetworkPacket { get; }

        public TimeSpan ResendTimeout { get; }

        public DateTimeOffset CreatedAt { get; }
    }
}