namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Linq;
    using System.Net;
    using Serilog;
    using UdpToolkit.Network.Channels;

    public sealed class UdpProtocol : IUdpProtocol
    {
        private readonly ILogger _logger = Log.ForContext<UdpProtocol>();

        public UdpProtocol()
        {
        }

        public bool TryGetInputPacket(
            ArraySegment<byte> bytes,
            IPEndPoint ipEndPoint,
            out NetworkPacket networkPacket)
        {
            networkPacket = default;

#pragma warning disable
            var hookId = bytes.Array[Consts.PacketTypeIndex];
            var channelType = (ChannelType)bytes.Array[Consts.ChannelTypeIndex];
            var peerId = new Guid(new ArraySegment<byte>(bytes.Array,2,16).ToArray());
#pragma warning restore

            ChannelHeader channelHeader = default;

            var offset = channelType >= ChannelType.ReliableUdp
                ? 24
                : 18;
            if (channelType >= ChannelType.ReliableUdp)
            {
                channelHeader = new ChannelHeader(
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
            }

            var payload = new ArraySegment<byte>(
                    array: bytes.Array,
                    offset: offset,
                    count: bytes.Array.Length - offset)
                .ToArray();

            networkPacket = new NetworkPacket(
                channelType: channelType,
                peerId: peerId,
                ipEndPoint: ipEndPoint,
                channelHeader: channelHeader,
                serializer: () => payload,
                hookId: hookId);

            return true;
        }

        public byte[] GetBytes(
            NetworkPacket networkPacket)
        {
            var protocolHeader = new byte[2];

            protocolHeader[Consts.PacketTypeIndex] = (byte)networkPacket.HookId;
            protocolHeader[Consts.ChannelTypeIndex] = (byte)networkPacket.ChannelType;

            var peerId = networkPacket.PeerId.ToByteArray();

            var channelHeader = Array.Empty<byte>();
            if (networkPacket.ChannelType >= ChannelType.ReliableUdp)
            {
                var idBytes = BitConverter.GetBytes(networkPacket.ChannelHeader.Id);
                var acksBytes = BitConverter.GetBytes(networkPacket.ChannelHeader.Acks);

                channelHeader = idBytes.Concat(acksBytes).ToArray();
            }

            var data = networkPacket.Serializer();

            return protocolHeader
                .Concat(peerId)
                .Concat(channelHeader)
                .Concat(data)
                .ToArray();
        }
    }
}
