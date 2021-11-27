namespace UdpToolkit.Network.Contracts.Packets
{
    using System;
    using UdpToolkit.Network.Contracts.Channels;
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Abstraction for represent pending network packet.
    /// </summary>
    public readonly struct PendingPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PendingPacket"/> struct.
        /// </summary>
        /// <param name="ipV4Address">Destination ip address.</param>
        /// <param name="buffer">Pooled buffer with payload.</param>
        /// <param name="payloadLength">Buffer length.</param>
        /// <param name="createdAt">Created at.</param>
        /// <param name="channel">Source channel.</param>
        /// <param name="id">Packet identifier.</param>
        public PendingPacket(
            IpV4Address ipV4Address,
            byte[] buffer,
            int payloadLength,
            DateTimeOffset createdAt,
            IChannel channel,
            ushort id)
        {
            IpV4Address = ipV4Address;
            Buffer = buffer;
            PayloadLength = payloadLength;
            CreatedAt = createdAt;
            Channel = channel;
            Id = id;
        }

        /// <summary>
        /// Gets packet identifier.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets destination ip address.
        /// </summary>
        public IpV4Address IpV4Address { get; }

        /// <summary>
        /// Gets pooled buffer.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// Gets buffer length.
        /// </summary>
        public int PayloadLength { get; }

        /// <summary>
        /// Gets date of creation.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets source channel.
        /// </summary>
        public IChannel Channel { get; }
    }
}