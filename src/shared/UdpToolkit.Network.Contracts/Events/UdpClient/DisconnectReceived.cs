namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received disconnect packet.
    /// </summary>
    public readonly struct DisconnectReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        public DisconnectReceived(
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