namespace UdpToolkit.Network.Contracts.Protocol
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Header of network protocol.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]

    public readonly struct NetworkHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkHeader"/> struct.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="id">Packet identifier.</param>
        /// <param name="acks">32 bit history about previous packets for this channel.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="packetType">Packet type.</param>
        /// <param name="dataType">Type of data.</param>
        public NetworkHeader(
            byte channelId,
            ushort id,
            uint acks,
            Guid connectionId,
            PacketType packetType,
            byte dataType)
        {
            Id = id;
            Acks = acks;
            ConnectionId = connectionId;
            PacketType = packetType;
            DataType = dataType;
            ChannelId = channelId;
        }

        /// <summary>
        /// Gets channel id.
        /// </summary>
        public byte ChannelId { get; } // 1 byte

        /// <summary>
        /// Gets packet id.
        /// </summary>
        public ushort Id { get; } // 2 bytes

        /// <summary>
        /// Gets 32-bit history about previous packets relative current.
        /// </summary>
        public uint Acks { get; } // 4 bytes

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; } // 16 bytes

        /// <summary>
        /// Gets packet type.
        /// </summary>
        public PacketType PacketType { get; } // 1 bytes

        /// <summary>
        /// Gets type of data.
        /// </summary>
        public byte DataType { get; } // 1 byte
    }
}