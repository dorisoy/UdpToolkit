namespace UdpToolkit.Network.Protocol
{
    using System;
    using System.Linq;
    using System.Net;
    using Serilog;
    using UdpToolkit.Network.Clients;
    using UdpToolkit.Network.Packets;
    using UdpToolkit.Network.Rudp;
    using UdpToolkit.Utils;

    public sealed class UdpProtocol : IUdpProtocol
    {
        private readonly IFrameworkProtocol _frameworkProtocol;
        private readonly IReliableUdpProtocol _reliableUdpProtocol;
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly ILogger _logger = Log.ForContext<UdpProtocol>();

        public UdpProtocol(
            IFrameworkProtocol frameworkProtocol,
            IReliableUdpProtocol reliableUdpProtocol,
            IDateTimeProvider dateTimeProvider)
        {
            _frameworkProtocol = frameworkProtocol;
            _reliableUdpProtocol = reliableUdpProtocol;
            _dateTimeProvider = dateTimeProvider;
        }

        public bool TryGetInputPacket(
            byte[] bytes,
            IPEndPoint ipEndPoint,
            out NetworkPacket networkPacket)
        {
            networkPacket = default;

            var paketResult = TryGetPacketType(bytes, out var packetType);
            if (!paketResult)
            {
                _logger.Warning("Can't parse packet type!");

                return false;
            }

            var payloadResult = TryGetPayload(packet: bytes, packetType: packetType, out var payload);
            if (!payloadResult)
            {
                _logger.Warning("Can't retrieve payload!");

                return false;
            }

            var frameworkHeaderResult = TryGetFrameworkHeader(packet: bytes, out var frameworkHeader);
            if (!frameworkHeaderResult)
            {
                _logger.Warning("Can't parse framework header!");

                return false;
            }

            if (packetType == PacketType.ReliableUdp)
            {
                var rudpResult = TryGetReliableUdpHeader(bytes, out var reliableUdpHeader); // TODO use it!
                if (!rudpResult)
                {
                    _logger.Warning("Can't parse rudp header!");

                    return false;
                }
            }

            networkPacket = new NetworkPacket(
                frameworkHeader: frameworkHeader,
                udpMode: NetworkExtensions.Map(packetType),
                payload: payload.ToArray(), // TODO remove ToArray()!
                ipEndPoint: ipEndPoint);

            return true;
        }

        public byte[] GetBytes(
            NetworkPacket networkPacket,
            ReliableUdpHeader reliableUdpHeader)
        {
            byte[] bytes = null;
            switch (networkPacket.UdpMode)
            {
                case UdpMode.Udp:
                    bytes = GetUdpPacketBytes(
                        frameworkHeader: networkPacket.FrameworkHeader,
                        payload: networkPacket.Payload);

                    break;
                case UdpMode.ReliableUdp:
                    bytes = GetReliableUdpPacketBytes(
                        frameworkHeader: networkPacket.FrameworkHeader,
                        reliableUdpHeader: reliableUdpHeader,
                        payload: networkPacket.Payload);

                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupoorted udp mode - {networkPacket.UdpMode}!");
            }

            return bytes;
        }

        private bool TryGetPacketType(byte[] bytes, out PacketType packetType)
        {
            packetType = PacketType.None;
            if (bytes.Length - 1 < Consts.PacketTypeIndex)
            {
                return false;
            }

            packetType = (PacketType)bytes[Consts.PacketTypeIndex];

            return true;
        }

        private byte[] GetReliableUdpPacketBytes(
            FrameworkHeader frameworkHeader,
            ReliableUdpHeader reliableUdpHeader,
            byte[] payload)
        {
            var frameworkHeaderBytes = _frameworkProtocol.Serialize(header: frameworkHeader);
            var reliableHeaderBytes = _reliableUdpProtocol.Serialize(header: reliableUdpHeader);

            return frameworkHeaderBytes
                .Concat(new[] { (byte)PacketType.ReliableUdp })
                .Concat(reliableHeaderBytes)
                .Concat(payload)
                .ToArray();
        }

        private byte[] GetUdpPacketBytes(
            FrameworkHeader frameworkHeader,
            byte[] payload)
        {
            var frameworkHeaderBytes = _frameworkProtocol.Serialize(header: frameworkHeader);

            return frameworkHeaderBytes
                .Concat(new[] { (byte)PacketType.Udp })
                .Concat(payload)
                .ToArray();
        }

        private bool TryGetReliableUdpHeader(byte[] packet, out ReliableUdpHeader reliableUdpHeader)
        {
            var result = _reliableUdpProtocol.TryDeserialize(packet, out reliableUdpHeader);
            if (!result)
            {
                return false;
            }

            return true;
        }

        private bool TryGetFrameworkHeader(byte[] packet, out FrameworkHeader frameworkHeader)
        {
            frameworkHeader = default;

            var segment = new ArraySegment<byte>(
                array: packet,
                offset: 0,
                count: Consts.FrameworkHeaderLength);

            var result = _frameworkProtocol.TryDeserialize(bytes: segment, out var fh);
            if (!result)
            {
                return false;
            }

            frameworkHeader = fh;

            return true;
        }

        private bool TryGetPayload(byte[] packet, PacketType packetType, out ArraySegment<byte> payload)
        {
            switch (packetType)
            {
                case PacketType.Udp:
                    payload = new ArraySegment<byte>(
                        array: packet,
                        offset: Consts.FrameworkHeaderOffset,
                        count: packet.Length - Consts.FrameworkHeaderOffset);

                    return true;

                case PacketType.ReliableUdp:
                    payload = new ArraySegment<byte>(
                        array: packet,
                        offset: Consts.ReliableUdpProtocolHeaderOffset,
                        count: packet.Length - Consts.ReliableUdpProtocolHeaderOffset);

                    return true;

                default:
                    payload = default;

                    return true;
            }
        }
    }
}
