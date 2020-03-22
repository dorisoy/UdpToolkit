namespace UdpToolkit.Network.Protocol
{
    using System;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Rudp;

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
