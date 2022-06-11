namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when user defined packet received.
    /// </summary>
    public struct UserDefinedReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public UserDefinedReceived(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}