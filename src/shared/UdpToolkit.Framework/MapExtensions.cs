namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;

    public static class MapExtensions
    {
        public static ChannelType Map(
            this UdpMode udpMode)
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
    }
}