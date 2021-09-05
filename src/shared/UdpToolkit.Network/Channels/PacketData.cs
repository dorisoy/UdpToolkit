namespace UdpToolkit.Network.Channels
{
    /// <summary>
    /// Internal struct for represent data in network window.
    /// </summary>
    internal readonly struct PacketData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketData"/> struct.
        /// </summary>
        /// <param name="id">Packet identifier.</param>
        /// <param name="acks">32 bit history before this packet.</param>
        /// <param name="acked">Flag for represent state of packet.</param>
        internal PacketData(
            ushort id,
            uint acks,
            bool acked)
        {
            Acked = acked;
            Id = id;
            Acks = acks;
        }

        /// <summary>
        /// Gets packet identifier.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets 32 bit history of packet.
        /// </summary>
        public uint Acks { get; }

        /// <summary>
        /// Gets a value indicating whether ack is needed.
        /// </summary>
        public bool Acked { get; }
    }
}