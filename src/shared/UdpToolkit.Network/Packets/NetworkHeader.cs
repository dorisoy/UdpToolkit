namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    internal readonly struct NetworkHeader
    {
        internal NetworkHeader(
            byte channelId,
            ushort id,
            uint acks,
            Guid connectionId,
            PacketType packetType)
        {
            Id = id;
            Acks = acks;
            ConnectionId = connectionId;
            PacketType = packetType;
            ChannelId = channelId;
        }

        internal byte ChannelId { get; } // 1 byte

        internal ushort Id { get; } // 2 bytes

        internal uint Acks { get; } // 4 bytes

        internal Guid ConnectionId { get; } // 16 bytes

        internal PacketType PacketType { get; } // 1 bytes
    }
}