namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received acknowledge for user defined packet.
    /// </summary>
    public struct UserDefinedAckReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedAckReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public UserDefinedAckReceived(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}