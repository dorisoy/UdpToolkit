namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Net;
    using UdpToolkit.Network.Packets;

    public interface IUdpProtocol
    {
        bool TryGetInputPacket(ArraySegment<byte> bytes, IPEndPoint ipEndPoint, out NetworkPacket networkPacket);

        byte[] GetBytes(NetworkPacket networkPacket);
    }
}
