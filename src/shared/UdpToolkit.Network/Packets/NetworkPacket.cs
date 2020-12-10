namespace UdpToolkit.Network.Packets
{
    using System;
    using System.IO;
    using System.Net;
    using UdpToolkit.Network.Channels;

    public sealed class NetworkPacket
    {
        [Obsolete("Deserialization only")]
        public NetworkPacket(DateTimeOffset createdAt)
        {
            CreatedAt = createdAt;
        }

        public NetworkPacket(
            byte hookId,
            ChannelType channelType,
            NetworkPacketType networkPacketType,
            Guid peerId,
            ushort id,
            uint acks,
            Func<byte[]> serializer,
            DateTimeOffset createdAt,
            IPEndPoint ipEndPoint)
        {
            Serializer = serializer;
            CreatedAt = createdAt;
            HookId = hookId;
            ChannelType = channelType;
            PeerId = peerId;
            NetworkPacketType = networkPacketType;
            Id = id;
            Acks = acks;
            IpEndPoint = ipEndPoint;
        }

        public ushort Id { get; private set; }

        public uint Acks { get; private set; }

        public byte HookId { get; }

        public ChannelType ChannelType { get; }

        public Guid PeerId { get; }

        public NetworkPacketType NetworkPacketType { get; }

        public Func<byte[]> Serializer { get; }

        public DateTimeOffset CreatedAt { get; }

        public IPEndPoint IpEndPoint { get; private set; }

        public ProtocolHookId ProtocolHookId => (ProtocolHookId)HookId;

        public bool IsProtocolEvent => HookId >= (byte)ProtocolHookId.P2P;

        public bool IsReliable => ChannelType == ChannelType.ReliableUdp || ChannelType == ChannelType.ReliableOrderedUdp;

        public static byte[] Serialize(NetworkPacket networkPacket)
        {
            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);

                bw.Write(networkPacket.HookId);
                bw.Write((byte)networkPacket.ChannelType);
                bw.Write((byte)networkPacket.NetworkPacketType);
                bw.Write(buffer: networkPacket.PeerId.ToByteArray());
                bw.Write(networkPacket.Id);
                bw.Write(networkPacket.Acks);
                bw.Write(buffer: networkPacket.Serializer());

                bw.Flush();
                return ms.ToArray();
            }
        }

        public static NetworkPacket Deserialize(byte[] bytes, IPEndPoint ipEndPoint)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var hookId = reader.ReadByte();
                var channelType = (ChannelType)reader.ReadByte();
                var networkPacketType = (NetworkPacketType)reader.ReadByte();
                var peerId = new Guid(reader.ReadBytes(16));
                var id = reader.ReadUInt16();
                var acks = reader.ReadUInt32();
                var payload = reader.ReadBytes(bytes.Length - 25);

                return new NetworkPacket(
                    ipEndPoint: ipEndPoint,
                    createdAt: DateTimeOffset.UtcNow,
                    hookId: hookId,
                    channelType: channelType,
                    networkPacketType: networkPacketType,
                    peerId: peerId,
                    id: id,
                    acks: acks,
                    serializer: () => payload);
            }
        }

        public bool IsExpired(TimeSpan resendTimeout) => DateTimeOffset.UtcNow - CreatedAt > resendTimeout;

        public NetworkPacket Clone(
            Guid peerId,
            IPEndPoint ipEndPoint,
            NetworkPacketType networkPacketType)
        {
            return new NetworkPacket(
                id: Id,
                acks: Acks,
                hookId: HookId,
                ipEndPoint: ipEndPoint,
                createdAt: DateTimeOffset.UtcNow,
                serializer: this.Serializer,
                channelType: this.ChannelType,
                peerId: peerId,
                networkPacketType: networkPacketType);
        }

        public void SetIp(IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;
        }

        public void SetHeader(
            ushort id,
            uint acks)
        {
            Id = id;
            Acks = acks;
        }
    }
}
