namespace UdpToolkit.Network.Tests.Framework
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    internal class PacketInfo
    {
        internal PacketInfo(
            Guid connectionId,
            IpV4Address ip,
            byte dataType,
            byte channelId,
            byte[] payload)
        {
            ConnectionId = connectionId;
            Ip = ip;
            DataType = dataType;
            ChannelId = channelId;
            Payload = payload;
        }

        public Guid ConnectionId { get; }

        public IpV4Address Ip { get; }

        public byte DataType { get; }

        public byte ChannelId { get; }

        public byte[] Payload { get; }
    }
}