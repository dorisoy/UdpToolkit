namespace UdpToolkit.Network.Contracts.Packets
{
    using System;
    using System.IO;
    using UdpToolkit.Network.Contracts.Sockets;

    public readonly struct OutPacket
    {
        public OutPacket(
            byte hookId,
            byte channelId,
            PacketType packetType,
            Guid connectionId,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IpV4Address destination)
        {
            Serializer = serializer;
            CreatedAt = createdAt;
            HookId = hookId;
            ChannelId = channelId;
            ConnectionId = connectionId;
            PacketType = packetType;
            Destination = destination;
        }

        public byte HookId { get; }

        public byte ChannelId { get; }

        public Guid ConnectionId { get; }

        public PacketType PacketType { get; }

        public Func<byte[]> Serializer { get; }

        public DateTimeOffset CreatedAt { get; }

        public IpV4Address Destination { get; }

        public static byte[] Serialize(
            ushort id,
            uint acks,
            ref OutPacket outPacket)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write(outPacket.HookId);
                bw.Write(outPacket.ChannelId);
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