namespace UdpToolkit.Core
{
    public enum UdpMode : byte
    {
        Udp = 1,
        Sequenced = 2,
        ReliableUdp = 3,
        ReliableOrderedUdp = 4,
    }
}