namespace UdpToolkit.Network.Channels
{
    public enum ChannelType : byte
    {
        Udp = 1,
        Sequenced = 2,
        ReliableUdp = 3,
        ReliableOrderedUdp = 4,
    }
}