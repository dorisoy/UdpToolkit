namespace UdpToolkit.Network.Packets
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Header of network protocol.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    internal readonly struct NetworkHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkHeader"/> struct.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="id">Packet identifier.</param>
        /// <param name="acks">32 bit history about previous packets for this channel.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="packetType">Packet type.</param>
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

        /// <summary>
        /// Gets channel id.
        /// </summary>
        internal byte ChannelId { get; } // 1 byte

        /// <summary>
        /// Gets packet id.
        /// </summary>
        internal ushort Id { get; } // 2 bytes

        /// <summary>
        /// Gets 32-bit history about previous packets relative current.
        /// </summary>
        internal uint Acks { get; } // 4 bytes

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        internal Guid ConnectionId { get; } // 16 bytes

        /// <summary>
        /// Gets packet type.
        /// </summary>
        internal PacketType PacketType { get; } // 1 bytes
    }
}