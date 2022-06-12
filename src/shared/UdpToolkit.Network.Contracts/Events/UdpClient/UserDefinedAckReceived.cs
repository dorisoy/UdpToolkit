namespace UdpToolkit.Network.Contracts.Events.UdpClient
{
    using UdpToolkit.Network.Contracts.Sockets;

    /// <summary>
    /// Raised when received acknowledge for user defined packet.
    /// </summary>
    public readonly struct UserDefinedAckReceived
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedAckReceived"/> struct.
        /// </summary>
        /// <param name="dataType">Data type identifier.</param>
        /// <param name="remoteIp">Remote ip address.</param>
        public UserDefinedAckReceived(byte dataType, IpV4Address remoteIp)
        {
            RemoteIp = remoteIp;
            DataType = dataType;
        }

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