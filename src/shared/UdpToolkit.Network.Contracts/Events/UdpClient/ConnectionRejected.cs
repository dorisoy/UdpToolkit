namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when connect packet received with disallow incoming connection setting.
    /// </summary>
    public readonly struct ConnectionRejected
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRejected"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote IP address instance.</param>
        public ConnectionRejected(
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