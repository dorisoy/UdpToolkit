namespace UdpToolkit.Network
{
    public static class Consts
    {
        public const int Mtu = 3000;
        public const int PacketTypeIndex = 0;
        public const int ChannelTypeIndex = 1;
        public const int ProtocolLength = 2;
        public const int ReliableUdpProtocolHeaderLength = 8;
        public const int ReliableUdpProtocolHeaderOffset = ReliableUdpProtocolHeaderLength;
    }
}
