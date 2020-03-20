namespace UdpToolkit.Network
{
    public static class Consts
    {
        public const int PacketTypeIndex = 4;
        public const int FrameworkHeaderLength = 4;
        public const int PacketTypeHeaderLength = 1;
        public const int FrameworkHeaderOffset = FrameworkHeaderLength + PacketTypeHeaderLength;
        public const int ReliableUdpProtocolHeaderLength = 12;
        public const int ReliableUdpProtocolHeaderOffset = FrameworkHeaderLength + PacketTypeHeaderLength + ReliableUdpProtocolHeaderLength;
    }
}
