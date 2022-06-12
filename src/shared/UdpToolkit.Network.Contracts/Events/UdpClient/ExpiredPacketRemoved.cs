namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised whe expired packet removed from resend queue.
    /// </summary>
    public readonly struct ExpiredPacketRemoved
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredPacketRemoved"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public ExpiredPacketRemoved(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}