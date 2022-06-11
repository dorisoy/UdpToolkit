namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when invalid network header received.
    /// </summary>
    public readonly struct InvalidHeaderReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidHeaderReceived"/> struct.
        /// </summary>
        /// <param name="remoteIp">Remote IP address.</param>
        /// <param name="invalidHeader">Invalid header bytes.</param>
        public InvalidHeaderReceived(
            IpV4Address remoteIp,
            byte[] invalidHeader)
        {
            InvalidHeader = invalidHeader;
            RemoteIp = remoteIp;
        }

        /// <summary>
        /// Gets remote IP Address.
        /// </summary>
        public IpV4Address RemoteIp { get; }

        /// <summary>
        /// Gets invalid header bytes.
        /// </summary>
        public byte[] InvalidHeader { get; }
    }
}