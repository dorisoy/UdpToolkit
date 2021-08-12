namespace UdpToolkit.Network.Packets
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    internal readonly struct PendingPacket
    {
        public PendingPacket(
            byte[] payload,
            IpV4Address to,
            DateTimeOffset createdAt,
            ushort id,
            byte channelId,
            byte hookId)
        {
            Payload = payload;
            To = to;
            CreatedAt = createdAt;
            Id = id;
            ChannelId = channelId;
            HookId = hookId;
        }

        public byte[] Payload { get; }

        public IpV4Address To { get; }

        public ushort Id { get; }

        public byte ChannelId { get; }

        public DateTimeOffset CreatedAt { get; }

        public byte HookId { get; }

        public bool IsExpired(TimeSpan resendTimeout) => DateTimeOffset.UtcNow - CreatedAt > resendTimeout;
    }
}