namespace UdpToolkit.Network.Contracts.Packets
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using UdpToolkit.Network.Contracts.Sockets;

    public readonly struct InPacket
    {
        public InPacket(
            byte hookId,
            byte channelId,
            PacketType packetType,
            Guid connectionId,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IpV4Address ipAddress)
        {
            Serializer = serializer;
            CreatedAt = createdAt;
            HookId = hookId;
            ChannelId = channelId;
            ConnectionId = connectionId;
            PacketType = packetType;
            IpAddress = ipAddress;
        }

        public byte HookId { get; }

        public byte ChannelId { get; }

        public Guid ConnectionId { get; }

        public PacketType PacketType { get; }

        public Func<byte[]> Serializer { get; }

        public DateTimeOffset CreatedAt { get; }

        public IpV4Address IpAddress { get; }

        public static InPacket Deserialize(
            byte[] bytes,
            IPEndPoint ipEndPoint,
            int bytesReceived,
            out ushort id,
            out uint acks)
        {
            var arr = bytes.Take(bytesReceived).ToArray();
            using (var reader = new BinaryReader(new MemoryStream(arr)))
            {
                var hookId = reader.ReadByte();
                var channelId = reader.ReadByte();
                var packetType = (PacketType)reader.ReadByte();
                var connectionId = new Guid(reader.ReadBytes(16));
                id = reader.ReadUInt16();
                acks = reader.ReadUInt32();
                var payload = reader.ReadBytes(bytes.Length - 25);

                return new InPacket(
                    hookId: hookId,
                    channelId: channelId,
                    packetType: packetType,
                    connectionId: connectionId,
                    serializer: () => payload,
                    createdAt: DateTimeOffset.UtcNow,
                    ipAddress: ipEndPoint.ToIp());
            }
        }
    }
}