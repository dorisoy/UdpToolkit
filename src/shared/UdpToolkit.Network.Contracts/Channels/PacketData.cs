namespace UdpToolkit.Network.Contracts.Channels
{
    /// <summary>
    /// Abstraction for representing sent network packet data.
    /// </summary>
    public struct PacketData
    {
#pragma warning disable S1104
        /// <summary>
        /// Is delivered.
        /// </summary>
        public bool IsDelivered;

        /// <summary>
        /// Identifier.
        /// </summary>
        public ushort Id;
#pragma warning restore S1104
    }
}