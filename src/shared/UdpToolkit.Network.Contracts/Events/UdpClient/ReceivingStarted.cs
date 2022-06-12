namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when UDP client start receive packets.
    /// </summary>
    public readonly struct ReceivingStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingStarted"/> struct.
        /// </summary>
        /// <param name="id">UdpClient identifier.</param>
        /// <param name="clientIpAddress">Client IP address.</param>
        public ReceivingStarted(
            string id,
            IpV4Address clientIpAddress)
        {
            ClientIpAddress = clientIpAddress;
            Id = id;
        }

        /// <summary>
        /// Gets UdpClient identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets client IP address.
        /// </summary>
        public IpV4Address ClientIpAddress { get; }
    }
}