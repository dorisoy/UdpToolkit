// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework.Contracts
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    public readonly struct OutPacket
    {
        public OutPacket(
            Guid connectionId,
            byte channelId,
            object @event,
            IpV4Address ipV4Address)
        {
            ChannelId = channelId;
            Event = @event;
            IpV4Address = ipV4Address;
            ConnectionId = connectionId;
        }

        public Guid ConnectionId { get; }

        public object Event { get; }

        public byte ChannelId { get; }

        public IpV4Address IpV4Address { get; }
    }
}
