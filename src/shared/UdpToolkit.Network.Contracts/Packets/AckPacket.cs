namespace UdpToolkit.Network.Contracts.Packets
{
    using System;
    using System.IO;
    using UdpToolkit.Network.Contracts.Sockets;

    public readonly struct AckPacket
    {
        public AckPacket(
            byte hookId,
            byte channelId,
            Guid connectionId,
            PacketType packetType,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IpV4Address ipAddress)
        {
            HookId = hookId;
            ChannelId = channelId;
            ConnectionId = connectionId;
            PacketType = packetType;
            CreatedAt = createdAt;
            IpAddress = ipAddress;
        }

        public byte HookId { get; }

        public byte ChannelId { get; }

        public Guid ConnectionId { get; }

        public PacketType PacketType { get; }

        public DateTimeOffset CreatedAt { get; }

        public IpV4Address IpAddress { get; }

        public static byte[] Serialize(
            ushort id,
            uint acks,
            ref InPacket inPacket)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write(inPacket.HookId);
                bw.Write(inPacket.ChannelId);
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