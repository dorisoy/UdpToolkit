namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received ping acknowledge packet.
    /// </summary>
    public readonly struct PingAckReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PingAckReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">remote ip address.</param>
        public PingAckReceived(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}