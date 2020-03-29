namespace UdpToolkit.Network
{
    using System;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;

    internal static class NetworkExtensions
    {
        internal static UdpMode Map(PacketType type)
        {
            switch (type)
            {
                case PacketType.Udp:
                    return UdpMode.Udp;

                case PacketType.ReliableUdp:
                    return UdpMode.ReliableUdp;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}