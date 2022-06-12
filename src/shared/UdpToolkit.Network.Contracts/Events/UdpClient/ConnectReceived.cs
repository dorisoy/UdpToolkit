namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when connect packet received.
    /// </summary>
    public readonly struct ConnectReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public ConnectReceived(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}