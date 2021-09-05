namespace UdpToolkit.Network.Contracts.Sockets
{
    /// <summary>
    /// IpV4 representation.
    /// </summary>
    public readonly struct IpV4Address
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IpV4Address"/> struct.
        /// </summary>
        /// <param name="address">ip address as integer.</param>
        /// <param name="port">ip address port.</param>
        public IpV4Address(
            int address,
            ushort port)
        {
            Address = address;
            Port = port;
        }

        /// <summary>
        /// Gets IP address integer representation.
        /// </summary>
        public int Address { get; }

        /// <summary>
        /// Gets IP address port.
        /// </summary>
        public ushort Port { get; }

        /// <summary>
        /// ToString().
        /// </summary>
        /// <returns>Human readable ip address as string.</returns>
        public override string ToString()
        {
            return this.ToIpEndPoint().ToString();
        }
    }
}