namespace UdpToolkit.Network.Packets
{
    using System;
    using System.IO;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public readonly struct AckPacket
    {
        public AckPacket(
            byte hookId,
            ChannelType channelType,
            Guid connectionId,
            PacketType packetType,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IPEndPoint ipEndPoint)
        {
            HookId = hookId;
            ChannelType = channelType;
            ConnectionId = connectionId;
            PacketType = packetType;
            CreatedAt = createdAt;
            IpEndPoint = ipEndPoint;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid ConnectionId { get; }

        public PacketType PacketType { get; }

        public DateTimeOffset CreatedAt { get; }

        public IPEndPoint IpEndPoint { get; }

        public static byte[] Serialize(
            ushort id,
            uint acks,
            ref InPacket inPacket)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write(inPacket.HookId);
                bw.Write((byte)inPacket.ChannelType);
                bw.Write((byte)PacketType.Ack);
                bw.Write(buffer: inPacket.ConnectionId.ToByteArray());
                bw.Write(id);
                bw.Write(acks);
                bw.Write(buffer: Array.Empty<byte>());

                bw.Flush();
                return ms.ToArray();
            }
        }
    }
}