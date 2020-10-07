namespace UdpToolkit.Network
{
    public enum NetworkPacketType : byte
    {
        UserDefined = 0,
        Protocol = 1,
        Ack = 2,
    }
}