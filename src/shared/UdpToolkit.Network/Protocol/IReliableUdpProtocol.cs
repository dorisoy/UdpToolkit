namespace UdpToolkit.Network.Protocol
{
    using UdpToolkit.Network.Rudp;

    public interface IReliableUdpProtocol
    {
        bool TryDeserialize(byte[] bytes, out ReliableUdpHeader header);

        byte[] Serialize(ReliableUdpHeader header);
    }
}