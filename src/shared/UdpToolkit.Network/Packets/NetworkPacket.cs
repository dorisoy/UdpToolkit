namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public class NetworkPacket : ICloneable
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
            NetworkPacketType networkPacketType)
        {
            Serializer = serializer;
            IpEndPoint = ipEndPoint;
            HookId = hookId;
            ChannelType = channelType;
            PeerId = peerId;
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            NetworkPacketType = networkPacketType;
            ChannelHeader = channelHeader;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid PeerId { get; private set; }

        public ChannelHeader ChannelHeader { get; private set; }

        public NetworkPacketType NetworkPacketType { get; }

        public Func<byte[]> Serializer { get; }

        public IPEndPoint IpEndPoint { get; private set; }

        public TimeSpan ResendTimeout { get; }

        public DateTimeOffset CreatedAt { get; }

        public ProtocolHookId ProtocolHookId => (ProtocolHookId)HookId;

        public bool IsProtocolEvent => HookId >= (byte)ProtocolHookId.P2P;

        public bool IsExpired() => DateTimeOffset.UtcNow - CreatedAt > ResendTimeout;

        public NetworkPacket SetChannelHeader(ChannelHeader channelHeader)
        {
            ChannelHeader = channelHeader;
            return this;
        }

        public NetworkPacket SetIpEndPoint(
            IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;
            return this;
        }

        public NetworkPacket Clone()
        {
            return new NetworkPacket(
                channelHeader: default,
                serializer: this.Serializer,
                ipEndPoint: this.IpEndPoint,
                hookId: this.HookId,
                channelType: this.ChannelType,
                peerId: this.PeerId,
                resendTimeout: this.ResendTimeout,
                createdAt: this.CreatedAt,
                networkPacketType: this.NetworkPacketType);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public NetworkPacket SetPeerId(
            Guid peerId)
        {
            PeerId = peerId;
            return this;
        }
    }
}
