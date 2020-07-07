namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network.Channels;

    public static class MapExtensions
    {
        public static ChannelType Map(this UdpMode udpMode)
        {
            switch (udpMode)
            {
                case UdpMode.Udp:
                    return ChannelType.Udp;
                case UdpMode.Sequenced:
                    return ChannelType.Sequenced;
                case UdpMode.ReliableUdp:
                    return ChannelType.ReliableUdp;
                case UdpMode.ReliableOrderedUdp:
                    return ChannelType.ReliableOrderedUdp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(udpMode), udpMode, null);
            }
        }

        public static UdpMode Map(this ChannelType udpMode)
        {
            switch (udpMode)
            {
                case ChannelType.Udp:
                    return UdpMode.Udp;
                case ChannelType.Sequenced:
                    return UdpMode.Sequenced;
                case ChannelType.ReliableUdp:
                    return UdpMode.ReliableUdp;
                case ChannelType.ReliableOrderedUdp:
                    return UdpMode.ReliableOrderedUdp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(udpMode), udpMode, null);
            }
        }
    }
}