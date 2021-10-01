namespace UdpToolkit.Network.Contracts.Protocol
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
        /// <param name="networkHeader">Network header.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="to">Destination ip address.</param>
        /// <param name="createdAt">Date of adding a packet to queue.</param>
        internal PendingPacket(
            NetworkHeader networkHeader,
            byte[] payload,
            IpV4Address to,
            DateTimeOffset createdAt)
        {
            NetworkHeader = networkHeader;
            Payload = payload;
            To = to;
            CreatedAt = createdAt;
        }

        /// <summary>
        /// Gets network header.
        /// </summary>
        public NetworkHeader NetworkHeader { get; }

        /// <summary>
        /// Gets payload.
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        /// Gets destination ip address.
        /// </summary>
        public IpV4Address To { get; }

        /// <summary>
        /// Gets date of creation pending packet.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }
    }
}
