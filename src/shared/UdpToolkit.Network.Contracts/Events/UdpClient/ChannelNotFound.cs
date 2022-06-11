namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    /// <summary>
    /// Raised when channel with specified identifier not found.
    /// </summary>
    public readonly struct ChannelNotFound
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelNotFound"/> struct.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        public ChannelNotFound(
            byte channelId)
        {
            ChannelId = channelId;
        }

        /// <summary>
        /// Gets channel identifier value.
        /// </summary>
        public byte ChannelId { get; }
    }
}