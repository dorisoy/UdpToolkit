namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Linq;
    using System.Net;
    using Serilog;
    using UdpToolkit.Network.Channels;

    public sealed class UdpProtocol : IUdpProtocol
    {
        private const int PayloadOffset = 24;
        private const int PacketTypeIndex = 0;
        private const int ChannelTypeIndex = 1;

        public NetworkPacket GetNetworkPacket(ArraySegment<byte> bytes, IPEndPoint ipEndPoint)
        {
            var hookId = bytes.Array[PacketTypeIndex];
            var channelType = (ChannelType)bytes.Array[ChannelTypeIndex];

            var idBuffer = new byte[16];
            Buffer.BlockCopy(src: bytes.Array, srcOffset: 2, dst: idBuffer, dstOffset: 0, count: 16);
            var peerId = new Guid(idBuffer);

            var channelHeader = new ChannelHeader(
                    id: BitConverter.ToUInt16(
                        value: new[]
                        {
                            bytes.Array[18],
                            bytes.Array[19],
                        },
                        startIndex: 0),
                    acks: BitConverter.ToUInt32(
                        value: new[]
                        {
                            bytes.Array[20],
                            bytes.Array[21],
                            bytes.Array[22],
                            bytes.Array[23],
                        },
                        startIndex: 0));

            var payloadLength = bytes.Array.Length - PayloadOffset;
            var payloadBuffer = new byte[payloadLength];
            Buffer.BlockCopy(src: bytes.Array, srcOffset: PayloadOffset, dst: payloadBuffer, dstOffset: 0, count: payloadLength);

            return new NetworkPacket(
                channelType: channelType,
                peerId: peerId,
                ipEndPoint: ipEndPoint,
                channelHeader: channelHeader,
                serializer: () => payloadBuffer,
                hookId: hookId);
        }

        public byte[] GetBytes(
            NetworkPacket networkPacket)
        {
            var protocolHeader = new byte[2];

            protocolHeader[PacketTypeIndex] = (byte)networkPacket.HookId;
            protocolHeader[ChannelTypeIndex] = (byte)networkPacket.ChannelType;

            var peerId = networkPacket.PeerId.ToByteArray();

            var idBytes = BitConverter.GetBytes(networkPacket.ChannelHeader.Id);
            var acksBytes = BitConverter.GetBytes(networkPacket.ChannelHeader.Acks);

            var data = networkPacket.Serializer();

            return protocolHeader
                .Concat(peerId)
                .Concat(idBytes)
                .Concat(acksBytes)
                .Concat(data)
                .ToArray();
        }
    }
}
