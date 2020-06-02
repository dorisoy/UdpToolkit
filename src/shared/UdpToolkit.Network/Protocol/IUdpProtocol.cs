namespace UdpToolkit.Network.Protocol
{
    using System.Net;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Rudp;

    public interface IUdpProtocol
    {
        bool TryGetInputPacket(byte[] bytes, IPEndPoint ipEndPoint, out NetworkPacket networkPacket);

        byte[] GetBytes(NetworkPacket networkPacket, ReliableUdpHeader reliableUdpHeader);
    }
}
