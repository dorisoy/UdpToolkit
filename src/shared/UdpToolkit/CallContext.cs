namespace UdpToolkit
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Packets;

    public readonly struct CallContext
    {
        public CallContext(
            TimeSpan resendTimeout,
            DateTimeOffset createdAt,
            int? roomId,
            Core.BroadcastMode? broadcastMode,
            NetworkPacket networkPacket)
        {
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            RoomId = roomId;
            BroadcastMode = broadcastMode;
            NetworkPacket = networkPacket;
        }

        public int? RoomId { get; }

        public BroadcastMode? BroadcastMode { get; }

        public TimeSpan ResendTimeout { get; }

        public DateTimeOffset CreatedAt { get; }

        public NetworkPacket NetworkPacket { get; }
    }
}