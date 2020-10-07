namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public sealed class UdpProtocol : IUdpProtocol
    {
        private const int HookIdIndex = 0;
        private const int ChannelTypeIndex = 1;
        private const int PacketTypeIndex = 2;
        private const int PayloadOffset = 25;
        private const int PeerIdLength = 16;

        public NetworkPacket Deserialize(
            ArraySegment<byte> bytes,
            IPEndPoint ipEndPoint,
            TimeSpan resendPacketTimeout)
        {
            var hookId = bytes.Array[HookIdIndex];
            var channelType = (ChannelType)bytes.Array[ChannelTypeIndex];
            var packetType = (NetworkPacketType)bytes.Array[PacketTypeIndex];

            var idBuffer = new byte[PeerIdLength];
            Buffer.BlockCopy(src: bytes.Array, srcOffset: 3, dst: idBuffer, dstOffset: 0, count: PeerIdLength);
            var peerId = new Guid(idBuffer);

            var id = BitConverter.ToUInt16(
                value: new[]
                {
                    bytes.Array[19],
                    bytes.Array[20],
                },
                startIndex: 0);

            var acks = BitConverter.ToUInt32(
                value: new[]
                {
                    bytes.Array[21],
                    bytes.Array[22],
                    bytes.Array[23],
                    bytes.Array[24],
                },
                startIndex: 0);

            var payloadLength = bytes.Array.Length - PayloadOffset;
            var payloadBuffer = new byte[payloadLength];
            Buffer.BlockCopy(src: bytes.Array, srcOffset: PayloadOffset, dst: payloadBuffer, dstOffset: 0, count: payloadLength);

            return new NetworkPacket(
                networkPacketType: packetType,
                createdAt: DateTimeOffset.UtcNow,
                resendTimeout: resendPacketTimeout,
                channelType: channelType,
                peerId: peerId,
                ipEndPoint: ipEndPoint,
                channelHeader: new ChannelHeader(id: id, acks: acks),
                serializer: () => payloadBuffer,
                hookId: hookId);
        }

        public byte[] Serialize(
            NetworkPacket networkPacket)
        {
            var protocolHeader = new byte[3];

            protocolHeader[HookIdIndex] = networkPacket.HookId;
            protocolHeader[ChannelTypeIndex] = (byte)networkPacket.ChannelType;
            protocolHeader[PacketTypeIndex] = (byte)networkPacket.NetworkPacketType;

            var peerId = networkPacket.PeerId.ToByteArray();

            var idBytes = BitConverter.GetBytes(networkPacket.ChannelHeader.Id);
            var acksBytes = BitConverter.GetBytes(networkPacket.ChannelHeader.Acks);

            var data = networkPacket.Serializer();

            var total = protocolHeader
                .Concat(peerId)
                .Concat(idBytes)
                .Concat(acksBytes)
                .Concat(data)
                .ToArray();

            return total;
        }
    }
}
