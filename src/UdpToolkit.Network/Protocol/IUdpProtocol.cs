using System;
using UdpToolkit.Network.Packets;
using UdpToolkit.Network.Protocol;
using UdpToolkit.Network.Rudp;

namespace UdpToolkit.Network.Protocol
{
    public interface IUdpProtocol
    {
        bool TryParseProtocol(
            byte[] packet,
            out PacketType packetType,
            out FrameworkHeader frameworkHeader,
            out ReliableUdpHeader reliableUdpHeader,
            out ArraySegment<byte> payload);

        byte[] GetReliableUdpPacketBytes(
            FrameworkHeader frameworkHeader,
            ReliableUdpHeader reliableUdpHeader,
            byte[] payload);
        
        byte[] GetUdpPacketBytes(
            FrameworkHeader frameworkHeader,
            byte[] payload);
    }
}
