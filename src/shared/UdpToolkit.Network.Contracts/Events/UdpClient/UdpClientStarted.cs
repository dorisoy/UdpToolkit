namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when UDP client start receive packets.
    /// </summary>
    public readonly struct UdpClientStarted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpClientStarted"/> struct.
        /// </summary>
        /// <param name="clientIpAddress">Client IP address.</param>
        public UdpClientStarted(
            IpV4Address clientIpAddress)
        {
            ClientIpAddress = clientIpAddress;
        }

        /// <summary>
        /// Gets client IP address.
        /// </summary>
        public IpV4Address ClientIpAddress { get; }
    }
}