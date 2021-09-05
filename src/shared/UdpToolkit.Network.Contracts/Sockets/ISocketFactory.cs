namespace UdpToolkit.Network.Contracts.Sockets
{
    /// <summary>
    /// Abstraction for socket creation.
    /// </summary>
    public interface ISocketFactory
    {
        /// <summary>
        /// Create socket.
        /// </summary>
        /// <param name="ipV4Address">Ip address for socket.</param>
        /// <returns>Created socket.</returns>
        ISocket Create(
            IpV4Address ipV4Address);
    }
}