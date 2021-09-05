namespace UdpToolkit.Network.Contracts.Channels
{
    /// <summary>
    /// Abstraction for implementing custom strategies for working with UDP packets.
    /// </summary>
    /// <remarks>
    /// All channels accept packets with UdpToolkit network header.
    /// </remarks>
    public interface IChannel
    {
        /// <summary>
        /// Gets a value indicating whether a reliability requirement.
        /// </summary>
        /// <remarks>
        /// true - for checking pending packets on heartbeat events and resend them.
        /// false - for ignore checking pending packets.
        /// </remarks>
        bool IsReliable { get; }

        /// <summary>
        /// Gets channel Id.
        /// </summary>
        byte ChannelId { get; }

        /// <summary>
        /// Handle input packets.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <param name="acks">32 bits of history about previous packets (not implemented but reserved).</param>
        /// <returns>
        /// true - input packet accepted
        /// false - input packet dropped.
        /// </returns>
        bool HandleInputPacket(
            ushort id,
            uint acks);

        /// <summary>
        /// Handle output packets.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <param name="acks">32 bits of history about previous packets (not implemented but reserved).</param>
        void HandleOutputPacket(
            out ushort id,
            out uint acks);

        /// <summary>
        /// Handle incoming acknowledge packets.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <param name="acks">32 bits of history about previous packets (not implemented but reserved).</param>
        /// <returns>
        /// true - acknowledge packet accepted
        /// false - acknowledge packet dropped.
        /// </returns>
        bool HandleAck(
            ushort id,
            uint acks);

        /// <summary>
        /// Checking packet delivered state.
        /// </summary>
        /// <param name="id">Packet id.</param>
        /// <returns>
        /// true - packet delivered
        /// false - packet not delivered.
        /// </returns>
        bool IsDelivered(
            ushort id);
    }
}