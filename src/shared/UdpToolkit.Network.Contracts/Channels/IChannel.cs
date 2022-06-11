namespace UdpToolkit.Network.Contracts.Channels
{
    using UdpToolkit.Network.Contracts.Protocol;

    /// <summary>
    /// Abstraction for implementing custom strategies for working with UDP packets.
    /// </summary>
    /// <remarks>
    /// All channels accept packets with UdpToolkit network header.
    /// </remarks>
    public interface IChannel
    {
        /// <summary>
        /// Gets a value indicating whether a resending packets requirement.
        /// </summary>
        bool IsReliable { get; }

        /// <summary>
        /// Gets channel Id.
        /// </summary>
        byte ChannelId { get; }

        /// <summary>
        /// Handle input packets.
        /// </summary>
        /// <param name="networkHeader">Network header.</param>
        /// <returns>
        /// true - input packet accepted
        /// false - input packet dropped.
        /// </returns>
        bool HandleInputPacket(
            in NetworkHeader networkHeader);

        /// <summary>
        /// Handle output packets.
        /// </summary>
        /// <param name="dataType">Data type.</param>
        /// <returns>Network header.</returns>
        ushort HandleOutputPacket(
            byte dataType);

        /// <summary>
        /// Handle incoming acknowledge packets.
        /// </summary>
        /// <param name="networkHeader">Network header.</param>
        /// <returns>
        /// true - acknowledge packet accepted
        /// false - acknowledge packet dropped.
        /// </returns>
        bool HandleAck(
            in NetworkHeader networkHeader);

        /// <summary>
        /// Check packet delivery status.
        /// </summary>
        /// <param name="id">Packet identifier.</param>
        /// <returns>
        /// true - packet delivered
        /// false - packet not delivered.
        /// </returns>
        bool IsDelivered(
            ushort id);
    }
}