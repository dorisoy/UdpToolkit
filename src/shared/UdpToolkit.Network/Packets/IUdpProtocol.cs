namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Net;
    using UdpToolkit.Network.Packets;

    public interface IUdpProtocol
    {
        NetworkPacket GetNetworkPacket(ArraySegment<byte> bytes, IPEndPoint ipEndPoint);

        byte[] GetBytes(NetworkPacket networkPacket);
    }
}
