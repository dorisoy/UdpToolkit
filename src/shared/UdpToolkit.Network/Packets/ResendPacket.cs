namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public readonly struct ResendPacket
    {
        public ResendPacket(
            byte[] payload,
            IPEndPoint to,
            DateTimeOffset createdAt,
            ushort id,
            ChannelType channelType,
            byte hookId)
        {
            Payload = payload;
            To = to;
            CreatedAt = createdAt;
            Id = id;
            ChannelType = channelType;
            HookId = hookId;
        }

        public byte[] Payload { get; }

        public IPEndPoint To { get; }

        public ushort Id { get; }

        public ChannelType ChannelType { get; }

        public DateTimeOffset CreatedAt { get; }

        public byte HookId { get; }

        public bool IsExpired(TimeSpan resendTimeout) => DateTimeOffset.UtcNow - CreatedAt > resendTimeout;
    }
}