namespace UdpToolkit.Network
{
    public static class Consts
    {
        public const int PacketTypeIndex = 2;
        public const int FrameworkHeaderLength = 2;
        public const int PacketTypeHeaderLength = 1;
        public const int FrameworkHeaderOffset = FrameworkHeaderLength + PacketTypeHeaderLength;
        public const int ReliableUdpProtocolHeaderLength = 12;
        public const int ReliableUdpProtocolHeaderOffset = FrameworkHeaderLength + PacketTypeHeaderLength + ReliableUdpProtocolHeaderLength;
    }
}
