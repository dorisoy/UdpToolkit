namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received ping packet.
    /// </summary>
    public readonly struct PingReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PingReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public PingReceived(
            IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}