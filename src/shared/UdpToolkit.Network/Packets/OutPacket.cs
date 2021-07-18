namespace UdpToolkit.Network.Packets
{
    using System;
    using System.IO;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Sockets;

    public readonly struct OutPacket
    {
        public OutPacket(
            byte hookId,
            ChannelType channelType,
            PacketType packetType,
            Guid connectionId,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IpV4Address ipAddress)
        {
            Serializer = serializer;
            CreatedAt = createdAt;
            HookId = hookId;
            ChannelType = channelType;
            ConnectionId = connectionId;
            PacketType = packetType;
            IpAddress = ipAddress;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid ConnectionId { get; }

        public PacketType PacketType { get; }

        public Func<byte[]> Serializer { get; }

        public DateTimeOffset CreatedAt { get; }

        public IpV4Address IpAddress { get; }

        public bool IsProtocolEvent => HookId >= (byte)ProtocolHookId.P2P;

        public bool IsReliable => ChannelType == ChannelType.ReliableUdp || ChannelType == ChannelType.ReliableOrderedUdp;

        public static byte[] Serialize(
            ushort id,
            uint acks,
            ref OutPacket outPacket)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write(outPacket.HookId);
                bw.Write((byte)outPacket.ChannelType);
                bw.Write((byte)outPacket.PacketType);
                bw.Write(buffer: outPacket.ConnectionId.ToByteArray());
                bw.Write(id);
                bw.Write(acks);
                bw.Write(buffer: outPacket.Serializer());

                bw.Flush();
                return ms.ToArray();
            }
        }
    }
}