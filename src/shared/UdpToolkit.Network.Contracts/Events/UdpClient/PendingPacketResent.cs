namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised whe expired packet removed from resend queue.
    /// </summary>
    public readonly struct PendingPacketResent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PendingPacketResent"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public PendingPacketResent(IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}