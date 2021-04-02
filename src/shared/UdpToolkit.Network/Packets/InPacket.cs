namespace UdpToolkit.Network.Packets
{
    using System;
    using System.IO;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public readonly struct InPacket
    {
        public InPacket(
            byte hookId,
            ChannelType channelType,
            PacketType packetType,
            Guid connectionId,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IPEndPoint ipEndPoint)
        {
            Serializer = serializer;
            CreatedAt = createdAt;
            HookId = hookId;
            ChannelType = channelType;
            ConnectionId = connectionId;
            PacketType = packetType;
            IpEndPoint = ipEndPoint;
        }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid ConnectionId { get; }

        public PacketType PacketType { get; }

        public Func<byte[]> Serializer { get; }

        public DateTimeOffset CreatedAt { get; }

        public IPEndPoint IpEndPoint { get; }

        public bool IsProtocolEvent => HookId >= (byte)ProtocolHookId.P2P;

        public bool IsReliable => ChannelType == ChannelType.ReliableUdp || ChannelType == ChannelType.ReliableOrderedUdp;

        public static InPacket Deserialize(
            byte[] bytes,
            IPEndPoint ipEndPoint,
            out ushort id,
            out uint acks)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var hookId = reader.ReadByte();
                var channelType = (ChannelType)reader.ReadByte();
                var packetType = (PacketType)reader.ReadByte();
                var connectionId = new Guid(reader.ReadBytes(16));
                id = reader.ReadUInt16();
                acks = reader.ReadUInt32();
                var payload = reader.ReadBytes(bytes.Length - 25);

                return new InPacket(
                    hookId: hookId,
                    channelType: channelType,
                    packetType: packetType,
                    connectionId: connectionId,
                    serializer: () => payload,
                    createdAt: DateTimeOffset.UtcNow,
                    ipEndPoint: ipEndPoint);
            }
        }
    }
}