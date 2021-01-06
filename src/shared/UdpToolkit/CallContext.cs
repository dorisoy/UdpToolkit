namespace UdpToolkit
{
    using System;
    using System.Net;
    using UdpToolkit.Core;
    using UdpToolkit.Network;
    using UdpToolkit.Network.Channels;
    using UdpToolkit.Network.Pooling;

    public sealed class CallContext : IResetteble
    {
        [Obsolete("For object pool only")]
        public CallContext()
        {
        }

        private CallContext(
            TimeSpan resendTimeout,
            DateTimeOffset createdAt,
            int? roomId,
            Core.BroadcastMode? broadcastMode)
        {
            ResendTimeout = resendTimeout;
            CreatedAt = createdAt;
            RoomId = roomId;
            BroadcastMode = broadcastMode;
        }

        public int? RoomId { get; private set; }

        public BroadcastMode? BroadcastMode { get; private set; }

        public TimeSpan ResendTimeout { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

#pragma warning disable
        public CallContext.NetworkPacket NetworkPacketDto { get; private set; } = new NetworkPacket();
#pragma warning restore

        public void Reset()
        {
            NetworkPacketDto.Reset();
            RoomId = default;
            BroadcastMode = default;
            ResendTimeout = default;
            CreatedAt = default;
        }

        public void Set(
            TimeSpan? resendTimeout,
            DateTimeOffset? createdAt,
            int? roomId,
            Core.BroadcastMode? broadcastMode)
        {
            ResendTimeout = resendTimeout ?? ResendTimeout;
            CreatedAt = createdAt ?? CreatedAt;
            RoomId = roomId ?? RoomId;
            BroadcastMode = broadcastMode ?? BroadcastMode;
        }

        [Obsolete("For object pool only")]
        public sealed class NetworkPacket : IResetteble
        {
            public NetworkPacket()
            {
            }

            public NetworkPacket(
                ushort id,
                uint acks,
                byte hookId,
                ChannelType channelType,
                Guid peerId,
                NetworkPacketType networkPacketType,
                Func<byte[]> serializer,
                DateTimeOffset createdAt,
                IPEndPoint ipEndPoint)
            {
                Id = id;
                Acks = acks;
                HookId = hookId;
                ChannelType = channelType;
                PeerId = peerId;
                NetworkPacketType = networkPacketType;
                Serializer = serializer;
                CreatedAt = createdAt;
                IpEndPoint = ipEndPoint;
            }

            public ushort Id { get; private set; }

            public uint Acks { get; private set; }

            public byte HookId { get; private set; }

            public ChannelType ChannelType { get; private set; }

            public Guid PeerId { get; private set; }

            public NetworkPacketType NetworkPacketType { get; private set; }

            public Func<byte[]> Serializer { get; private set; }

            public DateTimeOffset CreatedAt { get; private set; }

            public IPEndPoint IpEndPoint { get; private set; }

            public bool IsProtocolEvent => HookId >= (byte)ProtocolHookId.P2P;

            public ProtocolHookId ProtocolHookId => (ProtocolHookId)HookId;

            public bool IsReliable => ChannelType == ChannelType.ReliableUdp || ChannelType == ChannelType.ReliableOrderedUdp;

            public void Set(
                ushort? id = null,
                uint? acks = null,
                byte? hookId = null,
                ChannelType? channelType = null,
                Guid? peerId = null,
                NetworkPacketType? networkPacketType = null,
                Func<byte[]> serializer = null,
                DateTimeOffset? createdAt = null,
                IPEndPoint ipEndPoint = null)
            {
                Id = id ?? Id;
                Acks = acks ?? Acks;
                HookId = hookId ?? HookId;
                ChannelType = channelType ?? ChannelType;
                PeerId = peerId ?? PeerId;
                NetworkPacketType = networkPacketType ?? NetworkPacketType;
                Serializer = serializer ?? Serializer;
                CreatedAt = createdAt ?? CreatedAt;
                IpEndPoint = ipEndPoint ?? IpEndPoint;
            }

            public void Reset()
            {
                Id = default;
                Acks = default;
                HookId = default;
                ChannelType = default;
                PeerId = default;
                NetworkPacketType = default;
                Serializer = default;
                CreatedAt = default;
                IpEndPoint = default;
            }
        }

#pragma warning disable
        public static CallContext Create() => new CallContext();
#pragma warning restore
    }
}