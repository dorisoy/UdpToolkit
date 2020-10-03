namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public class NetworkPacket
    {
        public NetworkPacket(
            ChannelHeader channelHeader,
            Func<byte[]> serializer,
            IPEndPoint ipEndPoint,
            byte hookId,
            ChannelType channelType,
            Guid peerId,
            TimeSpan resendTimeout,
            DateTimeOffset createdAt,
            Action noAckCallback)
        {
            Serializer = serializer;
            IpEndPoint = ipEndPoint;
            HookId = hookId;
            ChannelType = channelType;
            PeerId = peerId;
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            NoAckCallback = noAckCallback;
            ChannelHeader = channelHeader;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid PeerId { get; }

        public ChannelHeader ChannelHeader { get; }

        public TimeSpan ResendTimeout { get; }

        public DateTimeOffset CreatedAt { get; }

        public Action NoAckCallback { get; }

        public Func<byte[]> Serializer { get; }

        public IPEndPoint IpEndPoint { get; }

        public ProtocolHookId ProtocolHookId => (ProtocolHookId)HookId;

        public bool IsExpired() => DateTimeOffset.UtcNow - CreatedAt > ResendTimeout;
    }
}
