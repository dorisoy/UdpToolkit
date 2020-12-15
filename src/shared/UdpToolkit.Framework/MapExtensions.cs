namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Packets;

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

        public static BroadcastType Map(
            this BroadcastMode udpMode)
        {
            switch (udpMode)
            {
                case BroadcastMode.Caller:
                    return BroadcastType.Caller;
                case BroadcastMode.Room:
                    return BroadcastType.Room;
                case BroadcastMode.Server:
                    return BroadcastType.Server;
                case BroadcastMode.RoomExceptCaller:
                    return BroadcastType.RoomExceptCaller;
                case BroadcastMode.AckToServer:
                    return BroadcastType.AckToServer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(udpMode), udpMode, null);
            }
        }
    }
}