namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when user defined packet received.
    /// </summary>
    public readonly struct UserDefinedReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedReceived"/> struct.
        /// </summary>
        /// <param name="id">UdpClient identifier.</param>
        /// <param name="dataType">Data type identifier.</param>
        /// <param name="remoteIp">Remote ip address.</param>
        public UserDefinedReceived(
            string id,
            byte dataType,
            IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
            Id = id;
            DataType = dataType;
        }

        /// <summary>
        /// Gets UdpClient identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets data type identifier.
        /// </summary>
        public byte DataType { get; }

        /// <summary>
        /// Gets remote ip address.
        /// </summary>
        public IpV4Address RemoteIp { get; }
    }
}