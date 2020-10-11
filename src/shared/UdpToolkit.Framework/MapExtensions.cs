namespace UdpToolkit.Framework
{
    using System;
    using UdpToolkit.Core;
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

        public static BroadcastType Map(
            this BroadcastMode broadcastMode)
        {
            switch (broadcastMode)
            {
                case BroadcastMode.Caller:
                    return BroadcastType.Caller;
                case BroadcastMode.Room:
                    return BroadcastType.Room;
                case BroadcastMode.ExceptCaller:
                    return BroadcastType.ExceptCaller;
                default:
                    throw new ArgumentOutOfRangeException(nameof(broadcastMode), broadcastMode, null);
            }
        }

        public static NetworkPacketType Map(
            this PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.Ack:
                    return NetworkPacketType.Ack;
                case PacketType.Protocol:
                    return NetworkPacketType.Protocol;
                case PacketType.UserDefined:
                    return NetworkPacketType.UserDefined;
                default:
                    throw new ArgumentOutOfRangeException(nameof(packetType), packetType, null);
            }
        }
    }
}