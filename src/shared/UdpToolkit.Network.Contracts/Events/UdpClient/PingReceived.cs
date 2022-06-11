namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received heartbeat packet.
    /// </summary>
    public readonly struct PingReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PingReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote ip address.</param>
        /// <param name="rtt">Round trip time for connection.</param>
        public PingReceived(
            IpV4Address remoteIp,
            double rtt)
        {
            RemoteIp = remoteIp;
            Rtt = rtt;
        }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }

        /// <summary>
        /// Gets round trip time for connection.
        /// </summary>
        public double Rtt { get; }
    }
}