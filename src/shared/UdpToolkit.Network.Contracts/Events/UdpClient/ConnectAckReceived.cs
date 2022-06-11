namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received acknowledge for connect packet.
    /// </summary>
    public struct ConnectAckReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectAckReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public ConnectAckReceived(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}