namespace UdpToolkit.Network.Protocol
{
    using System;
    using System.Linq;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Protocol;
    using UdpToolkit.Network.Rudp;

    public sealed class UdpProtocol : IUdpProtocol
    {
        private readonly IFrameworkProtocol _frameworkProtocol;
        private readonly IReliableUdpProtocol _reliableUdpProtocol;

        public UdpProtocol(
            IFrameworkProtocol frameworkProtocol,
            IReliableUdpProtocol reliableUdpProtocol)
        {
            _frameworkProtocol = frameworkProtocol;
            _reliableUdpProtocol = reliableUdpProtocol;
        }

        public bool TryParseProtocol(
            byte[] packet,
            out PacketType packetType,
            out FrameworkHeader frameworkHeader,
            out ReliableUdpHeader reliableUdpHeader,
            out ArraySegment<byte> payload)
        {
            packetType = (PacketType)packet[Consts.PacketTypeIndex];
            payload = GetPayload(packet: packet, packetType: packetType);
            frameworkHeader = GetFrameworkHeader(packet: packet);
            reliableUdpHeader = GetReliableUdpHeader(packet);

            return true;
        }

        public byte[] GetReliableUdpPacketBytes(FrameworkHeader frameworkHeader, ReliableUdpHeader reliableUdpHeader, byte[] payload)
        {
            var frameworkHeaderBytes = _frameworkProtocol.Serialize(header: frameworkHeader);
            var reliableHeaderBytes = _reliableUdpProtocol.Serialize(header: reliableUdpHeader);

            return frameworkHeaderBytes
                .Concat(new[] { (byte)PacketType.ReliableUdp })
                .Concat(reliableHeaderBytes)
                .Concat(payload)
                .ToArray();
        }

        public byte[] GetUdpPacketBytes(FrameworkHeader frameworkHeader, byte[] payload)
        {
            var frameworkHeaderBytes = _frameworkProtocol.Serialize(header: frameworkHeader);

            return frameworkHeaderBytes
                .Concat(new[] { (byte)PacketType.Udp })
                .Concat(payload)
                .ToArray();
        }

        private ReliableUdpHeader GetReliableUdpHeader(byte[] packet)
        {
            _reliableUdpProtocol.TryDeserialize(packet, out var reliableUdpHeader);

            return reliableUdpHeader;
        }

        private FrameworkHeader GetFrameworkHeader(byte[] packet)
        {
            var fh = new ArraySegment<byte>(
                array: packet,
                offset: 0,
                count: Consts.FrameworkHeaderLength);

            _frameworkProtocol.TryDeserialize(bytes: fh, out var frameworkHeader);

            return frameworkHeader;
        }

        private ArraySegment<byte> GetPayload(byte[] packet, PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.Udp:
                    return new ArraySegment<byte>(
                        array: packet,
                        offset: Consts.FrameworkHeaderOffset,
                        count: packet.Length - Consts.FrameworkHeaderOffset);

                case PacketType.ReliableUdp:
                    return new ArraySegment<byte>(
                        array: packet,
                        offset: Consts.ReliableUdpProtocolHeaderOffset,
                        count: packet.Length - Consts.ReliableUdpProtocolHeaderOffset);

                default:
                    return null;
            }
        }
    }
}
