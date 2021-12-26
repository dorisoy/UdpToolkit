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
            uint address,
            ushort port)
        {
            Address = address;
            Port = port;
        }

        /// <summary>
        /// Gets IP address integer representation.
        /// </summary>
        public uint Address { get; }

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
            return $"{Address}:{Port}";
        }

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="other">Other instance.</param>
        /// <returns>True if instances are equal.</returns>
        public bool Equals(IpV4Address other)
        {
            return Address == other.Address && Port == other.Port;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is IpV4Address other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Address.GetHashCode() * 397) ^ Port.GetHashCode();
            }
        }
    }
}