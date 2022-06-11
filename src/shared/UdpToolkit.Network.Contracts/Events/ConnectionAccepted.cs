namespace UdpToolkit.Network.Contracts.Events
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when new connection accepted.
    /// </summary>
    public readonly struct ConnectionAccepted
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionAccepted"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote IP address instance.</param>
        public ConnectionAccepted(
            IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote IP address value.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}