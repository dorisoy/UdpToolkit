namespace UdpToolkit.Network.Packets
{
    using System;
    using System.IO;
    using System.Net;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Pooling;

    public sealed class NetworkPacket : IResetteble
    {
        [Obsolete("Deserialization only")]
        public NetworkPacket()
        {
        }

        private NetworkPacket(
            byte hookId,
            ChannelType channelType,
            NetworkPacketType networkPacketType,
            Guid connectionId,
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
            ConnectionId = connectionId;
            NetworkPacketType = networkPacketType;
            Id = id;
            Acks = acks;
            IpEndPoint = ipEndPoint;
        }

        public ushort Id { get; private set; }

        public uint Acks { get; private set; }

        public byte HookId { get; private set; }

        public ChannelType ChannelType { get; private set; }

        public Guid ConnectionId { get; private set; }

        public NetworkPacketType NetworkPacketType { get; private set; }

        public Func<byte[]> Serializer { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public IPEndPoint IpEndPoint { get; private set; }

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
                bw.Write(buffer: networkPacket.ConnectionId.ToByteArray());
                bw.Write(networkPacket.Id);
                bw.Write(networkPacket.Acks);
                bw.Write(buffer: networkPacket.Serializer());

                bw.Flush();
                return ms.ToArray();
            }
        }

        public static void Deserialize(
            byte[] bytes,
            IPEndPoint ipEndPoint,
            PooledObject<NetworkPacket> pooledNetworkPacket)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var hookId = reader.ReadByte();
                var channelType = (ChannelType)reader.ReadByte();
                var networkPacketType = (NetworkPacketType)reader.ReadByte();
                var connectionId = new Guid(reader.ReadBytes(16));
                var id = reader.ReadUInt16();
                var acks = reader.ReadUInt32();
                var payload = reader.ReadBytes(bytes.Length - 25);

                pooledNetworkPacket.Value.Set(
                    ipEndPoint: ipEndPoint,
                    createdAt: DateTimeOffset.UtcNow,
                    hookId: hookId,
                    channelType: channelType,
                    networkPacketType: networkPacketType,
                    connectionId: connectionId,
                    id: id,
                    acks: acks,
                    serializer: () => payload);
            }
        }

        public bool IsExpired(TimeSpan resendTimeout) => DateTimeOffset.UtcNow - CreatedAt > resendTimeout;

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Acks)}: {Acks}, {nameof(HookId)}: {HookId}, {nameof(ChannelType)}: {ChannelType}, {nameof(ConnectionId)}: {ConnectionId}, {nameof(NetworkPacketType)}: {NetworkPacketType}, {nameof(CreatedAt)}: {CreatedAt}, {nameof(IpEndPoint)}: {IpEndPoint}, {nameof(IsProtocolEvent)}: {IsProtocolEvent}, {nameof(IsReliable)}: {IsReliable}";
        }

        public void Reset()
        {
            Serializer = default;
            CreatedAt = default;
            HookId = default;
            ChannelType = default;
            ConnectionId = default;
            NetworkPacketType = default;
            Id = default;
            Acks = default;
            IpEndPoint = default;
        }

        public void Set(
            byte? hookId = null,
            ChannelType? channelType = null,
            NetworkPacketType? networkPacketType = null,
            Guid? connectionId = null,
            ushort? id = null,
            uint? acks = null,
            Func<byte[]> serializer = null,
            DateTimeOffset? createdAt = null,
            IPEndPoint ipEndPoint = null)
        {
            Serializer = serializer ?? Serializer;
            CreatedAt = createdAt ?? CreatedAt;
            HookId = hookId ?? HookId;
            ChannelType = channelType ?? ChannelType;
            ConnectionId = connectionId ?? ConnectionId;
            NetworkPacketType = networkPacketType ?? NetworkPacketType;
            Id = id ?? Id;
            Acks = acks ?? Acks;
            IpEndPoint = ipEndPoint ?? IpEndPoint;
        }

#pragma warning disable
        public static NetworkPacket Create() => new NetworkPacket();
#pragma warning restore
    }
}
