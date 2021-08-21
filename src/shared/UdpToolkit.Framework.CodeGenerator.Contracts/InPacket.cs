// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public readonly struct InPacket
    {
        public InPacket(
            byte[] payload,
            Guid connectionId,
            IpV4Address ipV4Address,
            byte channelId,
            bool expired)
        {
            Payload = payload;
            ConnectionId = connectionId;
            IpV4Address = ipV4Address;
            ChannelId = channelId;
            Expired = expired;
        }

        public byte ChannelId { get; }

        public byte[] Payload { get; }

        public Guid ConnectionId { get; }

        public IpV4Address IpV4Address { get; }

        public bool Expired { get; }
    }
}