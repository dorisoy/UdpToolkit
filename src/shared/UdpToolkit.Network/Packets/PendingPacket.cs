namespace UdpToolkit.Network.Packets
{
    using System;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Structure for representing not ack packet.
    /// </summary>
    internal readonly struct PendingPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PendingPacket"/> struct.
        /// </summary>
        /// <param name="payload">Payload.</param>
        /// <param name="to">Destination ip address.</param>
        /// <param name="createdAt">Date of adding a packet to queue.</param>
        /// <param name="id">Packet identifier.</param>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="packetType">Packet type.</param>
        internal PendingPacket(
            byte[] payload,
            IpV4Address to,
            DateTimeOffset createdAt,
            ushort id,
            byte channelId,
            Guid connectionId,
            PacketType packetType)
        {
            Payload = payload;
            To = to;
            CreatedAt = createdAt;
            Id = id;
            ChannelId = channelId;
            ConnectionId = connectionId;
            PacketType = packetType;
        }

        /// <summary>
        /// Gets payload.
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        /// Gets destination ip address.
        /// </summary>
        public IpV4Address To { get; }

        /// <summary>
        /// Gets packet identifier.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets channel identifier.
        /// </summary>
        public byte ChannelId { get; }

        /// <summary>
        /// Gets date of creation pending packet.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets connection identifier.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets packet type.
        /// </summary>
        public PacketType PacketType { get; }
    }
}
