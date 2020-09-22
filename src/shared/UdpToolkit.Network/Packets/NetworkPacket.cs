namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public readonly struct NetworkPacket
    {
        public NetworkPacket(
            ChannelHeader channelHeader,
            Func<byte[]> serializer,
            IPEndPoint ipEndPoint,
            byte hookId,
            ChannelType channelType,
            Guid peerId)
        {
            Serializer = serializer;
            IpEndPoint = ipEndPoint;
            HookId = hookId;
            ChannelType = channelType;
            PeerId = peerId;
            ChannelHeader = channelHeader;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid PeerId { get; }

        public ChannelHeader ChannelHeader { get; }

        public Func<byte[]> Serializer { get; }

        public IPEndPoint IpEndPoint { get; }

        public PacketType Type => (PacketType)HookId;
    }
}
