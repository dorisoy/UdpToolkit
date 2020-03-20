using UdpToolkit.Network.Rudp;

namespace UdpToolkit.Network.Protocol
{
    public interface IReliableUdpProtocol
    {
        bool TryDeserialize(byte[] bytes, out ReliableUdpHeader header);
        byte[] Serialize(ReliableUdpHeader header);
    }
}