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
            NetworkPacketType networkPacketType,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IPEndPoint ipEndPoint)
        {
            HookId = hookId;
            ChannelType = channelType;
            ConnectionId = connectionId;
            NetworkPacketType = networkPacketType;
            CreatedAt = createdAt;
            IpEndPoint = ipEndPoint;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid ConnectionId { get; }

        public NetworkPacketType NetworkPacketType { get; }

        public DateTimeOffset CreatedAt { get; }

        public IPEndPoint IpEndPoint { get; }

        public static byte[] Serialize(
            ushort id,
            uint acks,
            ref NetworkPacket networkPacket)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write(networkPacket.HookId);
                bw.Write((byte)networkPacket.ChannelType);
                bw.Write((byte)NetworkPacketType.Ack);
                bw.Write(buffer: networkPacket.ConnectionId.ToByteArray());
                bw.Write(id);
                bw.Write(acks);
                bw.Write(buffer: Array.Empty<byte>());

                bw.Flush();
                return ms.ToArray();
            }
        }
    }
}